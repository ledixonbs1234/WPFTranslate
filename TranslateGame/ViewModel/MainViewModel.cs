using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TranslateGame.Model;

namespace TranslateGame.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private static string UppercaseWords(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        private void CovertGoogleToVietAndRefresh()
        {
            SelectText.VietText = this.SelectText.GoogleText;
            OnRefreshUI();
        }

        private void CovertHanVietToVietAndRefresh()
        {
            SelectText.VietText = this.SelectText.HanVietText;
            OnRefreshUI();
        }

        private void findAndReplaceSameText()
        {
            int OldIndex = Texts.IndexOf(SelectText) - 1;
            TextModel oldText = Texts[OldIndex];
            string chinaOldText = oldText.ChinaText;
            string vietOldText = oldText.VietText;
            if (IsWorkForAll)
            {
                for (int i = 0; i < Jsons.Count; i++)
                {
                    for (int j = 0; j < Jsons[i].ListText.Count; j++)
                    {
                        if (Jsons[i].ListText[j].ChinaText == chinaOldText)
                        {
                            Jsons[i].ListText[j].VietText = vietOldText;
                            if (!Jsons[i].IsChangeJson || !Jsons[i].IsChangeTxt)
                            {
                                Jsons[i].IsChangeJson = true;
                                Jsons[i].IsChangeTxt = true;
                            }
                        }
                    }
                }
            }
            else
            {
                JsonSelected.IsChangeJson = true;
                JsonSelected.IsChangeTxt = true;
                for (int i = 0; i < Texts.Count; i++)
                {
                    if (Texts[OldIndex].ChinaText == Texts[i].ChinaText)
                    {
                        Texts[i].VietText = Texts[OldIndex].VietText;
                    }
                }
            }

            RaisePropertyChanged(TextsPropertyName);
            OnRefreshUI();
        }

        private void findText()
        {
            if (IsWorkForAll)
            {
                bool isFindJson = false;
                foreach (JsonModel json in Jsons)
                {
                    foreach (TextModel textM in json.ListText)
                    {
                        bool isFinded = textM.VietText.Contains(TextForFind);

                        if (!isFinded)
                            continue;
                        else
                        {
                            JsonSelected = json;

                            SelectText = textM;
                            MessengerInstance.Send<TextModel>(textM, "doScrollToView");
                            isFindJson = true;
                            break;
                        }
                    }
                    if (isFindJson)
                    {
                        break;
                    }
                }
            }
            else
            {
                foreach (TextModel textM in Texts)
                {
                    bool isFinded = textM.VietText.Contains(TextForFind);

                    if (!isFinded)
                        continue;
                    else
                    {
                        SelectText = textM;
                        MessengerInstance.Send<TextModel>(textM, "doScrollToView");
                        break;
                    }
                }
            }
        }

        private void loadDataGrid(JsonModel json)
        {
            Texts = new ObservableCollection<TextModel>();
            json.ListText.ForEach(m =>
            {
                Texts.Add(m);
            });
            FromSelect = Texts[0];
            ToSelect = Texts[Texts.Count - 1];
        }

        private string MergeTextChina(JsonModel jsonModel)
        {
            //thuc hien cac cong viec
            //gop text chia thanh 1 text
            if (jsonModel == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            List<string> merges = new List<string>();
            foreach (TextModel text in jsonModel.ListText)
            {
                sb.Append(text.ChinaText + "~");
            }
            return sb.ToString();
        }

        private void NextItemAndSave()
        {
            CountToSave++;
            JsonSelected.IsChangeJson = true;
            int indexSelect = Texts.IndexOf(SelectText);
            //thuc hien replace trong nay

            //nho ho count -1
            if (Texts.Count - 1 > indexSelect)
            {
                SelectText = Texts[indexSelect + 1];
            }
            // auto save
            if (CountToSave >= 5)
            {
                SaveDataCommand.Execute(null);
                CountToSave = 0;
                JsonSelected.IsChangeJson = false;
            }
        }

        private void onJsonSelected()
        {
            loadDataGrid(_jsonSelected);
        }

        private void OnRefreshUI()
        {
            MessengerInstance.Send<string>("focus", "Dofocus");
            CollectionViewSource.GetDefaultView(Texts).Refresh();

            RaisePropertyChanged(SelectTextPropertyName);
        }

        private string UpperFirstText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string lower = text.ToLower();
                return char.ToUpper(lower[0]) + lower.Substring(1);
            }
            else
            {
                return "";
            }
        }

        private void VietPhare()
        {
            SelectText.VietText = this.SelectText.VietPharseText;
            OnRefreshUI();
        }
        /// <summary>
        /// The <see cref="SelectNhap" /> property's name.
        /// </summary>
        public const string SelectNhapPropertyName = "SelectNhap";

        private string _selectNhap = "";

        /// <summary>
        /// Sets and gets the SelectNhap property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectNhap
        {
            get
            {
                return _selectNhap;
            }

            set
            {
                if (_selectNhap == value)
                {
                    return;
                }

                _selectNhap = value;
                RaisePropertyChanged(SelectNhapPropertyName);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<JsonModel> jsons = new List<JsonModel>();
            string[] files = e.Argument as string[];
            int count = 1;
            foreach (string path in files)
            {
                _dataService.FileTextChina(path, json =>
                 {
                     jsons.Add(json);
                     (sender as BackgroundWorker).ReportProgress(count);
                     count++;
                 });
            }
            e.Result = jsons;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Message = "File đã chuyển là : " + e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Jsons = new ObservableCollection<JsonModel>();
            List<JsonModel> listJson = e.Result as List<JsonModel>;
            listJson.ForEach(m =>
            {
                Jsons.Add(m);
            });
            JsonSelected = Jsons[0];
        }

        public RelayCommand AllToHanVietCommand
        {
            get
            {
                return _allToHanVietCommand
                    ?? (_allToHanVietCommand = new RelayCommand(
                    () =>
                    {
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            Texts[i].VietText = UppercaseWords(Texts[i].HanVietText);
                        }
                        RaisePropertyChanged(TextsPropertyName);
                        OnRefreshUI();
                    }));
            }
        }

        public RelayCommand AllToVietPhareCommand
        {
            get
            {
                return _allToVietPhareCommand
                    ?? (_allToVietPhareCommand = new RelayCommand(
                    () =>
                    {
                    }));
            }
        }

        public CheckBoxGroupModel CheckBoxGroup
        {
            get
            {
                return _checkBoxGroup;
            }

            set
            {
                if (_checkBoxGroup == value)
                {
                    return;
                }

                _checkBoxGroup = value;
                RaisePropertyChanged(CheckBoxGroupPropertyName);
            }
        }

        public RelayCommand ConvertChinaToFileCommand
        {
            get
            {
                return _convertChinaToFileCommand
                    ?? (_convertChinaToFileCommand = new RelayCommand(
                    () =>
                    {
                        Regex regex = new Regex("[\u4E00-\u9FA5]");
                        foreach (TextModel text in Texts)
                        {
                            //Tim Text china

                            string content = File.ReadAllText(text.FullPath);
                            MatchCollection list = regex.Matches(content);
                            string c = content.Remove(text.StartIndex, text.VietText.Length)
                   .Insert(text.StartIndex, text.ChinaText);
                            FileInfo file = new FileInfo(text.FullPath);
                            var dir = file.Directory.CreateSubdirectory("Finish");
                            string exportPath = dir.FullName + file.Name;
                            File.WriteAllText(text.FullPath, c);
                        }
                    }));
            }
        }

        public RelayCommand ConvertIndexToFileCommand
        {
            get
            {
                return _convertIndexToFileCommand
                    ?? (_convertIndexToFileCommand = new RelayCommand(
                    () =>
                    {
                        Regex regex = new Regex("[\u4E00-\u9FA5]");
                        int countText = 0;
                        string lastPath = "";
                        foreach (TextModel text in Texts)
                        {
                            //Tim Text china
                            string content = File.ReadAllText(text.FullPath);
                            string changedContent;
                            if (text.FullPath == lastPath)
                            {
                                string textConverted = "*" + text.STT.ToString() + "*";
                                changedContent = content.Remove(text.StartIndex + countText, text.ChinaText.Length)
                                                   .Insert(text.StartIndex + countText, textConverted);
                                countText += textConverted.Length - text.ChinaText.Length;
                            }
                            else
                            {
                                string textConverted = "*" + text.STT.ToString() + "*";
                                changedContent = content.Remove(text.StartIndex, text.ChinaText.Length)
                                                   .Insert(text.StartIndex, textConverted);
                                countText = textConverted.Length - text.ChinaText.Length;
                            }

                            File.WriteAllText(text.FullPath, changedContent);

                            lastPath = text.FullPath;
                        }
                    }));
            }
        }

        public RelayCommand ConvertOptionCommand
        {
            get
            {

                return _convertOptionCommand
                    ?? (_convertOptionCommand = new RelayCommand(
                    () =>
                    {
                        if (IsWorkForAll)
                        {
                            foreach (JsonModel json in Jsons)
                            {
                                json.IsChangeTxt = true;
                                json.IsChangeJson = true;
                                int IndexFrom = 0;
                                int IndexTo = json.ListText.Count -1;
                                string text = "";
                                for (int i = IndexFrom; i <= IndexTo; i++)
                                {
                                    if (CheckBoxGroup.IsGoogle)
                                        text = json.ListText[i].GoogleText;
                                    if (CheckBoxGroup.IsHanViet)
                                        text = json.ListText[i].HanVietText;
                                    if (CheckBoxGroup.IsVietPhare)
                                        text = json.ListText[i].VietPharseText;
                                    if (string.IsNullOrEmpty(text))
                                        text = json.ListText[i].ChinaText;
                                    if (CheckBoxGroup.IsSampletext)
                                        text = UpperFirstText(text);
                                    if (CheckBoxGroup.IsSampleText)
                                        text = UppercaseWords(text);
                                    if (CheckBoxGroup.IsSAMPLETEXT)
                                        text = text.ToUpper();

                                    json.ListText[i].VietText = text;
                                }
                            }
                        }
                        else
                        {
                            JsonSelected.IsChangeJson = true;
                            JsonSelected.IsChangeTxt = true;
                            //lay index from and to
                            int IndexFrom = Texts.IndexOf(FromSelect);
                            int IndexTo = Texts.IndexOf(ToSelect);
                            string text = "";
                            for (int i = IndexFrom; i <= IndexTo; i++)
                            {
                                if (CheckBoxGroup.IsGoogle)
                                    text = Texts[i].GoogleText;
                                if (CheckBoxGroup.IsHanViet)
                                    text = Texts[i].HanVietText;
                                if (CheckBoxGroup.IsVietPhare)
                                    text = Texts[i].VietPharseText;
                                if (string.IsNullOrEmpty(text))
                                    text = Texts[i].ChinaText;
                                if (CheckBoxGroup.IsSampletext)
                                    text = UpperFirstText(text);
                                if (CheckBoxGroup.IsSampleText)
                                    text = UppercaseWords(text);
                                if (CheckBoxGroup.IsSAMPLETEXT)
                                    text = text.ToUpper();

                                Texts[i].VietText = text;
                            }
                        }
                        RaisePropertyChanged(TextsPropertyName);
                        OnRefreshUI();
                    }));
            }
        }

        public RelayCommand ConvertToFileCommand
        {
            get
            {
                return _convertToFileCommand
                    ?? (_convertToFileCommand = new RelayCommand(
                    () =>
                    {
                        Regex regex = new Regex("[\u4E00-\u9FA5]");
                        string oldPath = "";
                        int count = 0;
                        string content = "";
                        List<JsonModel> jsonNeedSaved = new List<JsonModel>();
                        foreach (JsonModel json in Jsons)
                        {
                            if (json.IsChangeTxt)
                            {
                                jsonNeedSaved.Add(json);
                                json.IsChangeTxt = false;
                            }
                        }
                        if (jsonNeedSaved.Count == 0)
                        {
                            Message = "Chưa có file nào được chỉnh sửa nên không lưu được";
                            return;
                        }
                        _dataService.ConvertListJsonToFiles(jsonNeedSaved);

                        SaveDataCommand.Execute(null);
                    }));
            }
        }

        public int CountFile
        {
            get
            {
                return _countFile;
            }

            set
            {
                if (_countFile == value)
                {
                    return;
                }

                _countFile = value;
                RaisePropertyChanged(CountFilePropertyName);
            }
        }

        public RelayCommand FindTextChinaCommand
        {
            get
            {
                return _findTextChinaCommand
                    ?? (_findTextChinaCommand = new RelayCommand(
                    () =>
                    {
                        worker.RunWorkerAsync(_files);
                    }));
            }
        }

        public TextModel FromSelect
        {
            get
            {
                return _fromSelect;
            }

            set
            {
                if (_fromSelect == value)
                {
                    return;
                }

                _fromSelect = value;
                RaisePropertyChanged(FromSelectPropertyName);
            }
        }

        public ObservableCollection<JsonModel> Jsons
        {
            get
            {
                return _jsons;
            }

            set
            {
                if (_jsons == value)
                {
                    return;
                }

                _jsons = value;
                RaisePropertyChanged(JsonsPropertyName);
            }
        }

        public JsonModel JsonSelected
        {
            get
            {
                return _jsonSelected;
            }

            set
            {
                if (_jsonSelected == value)
                {
                    return;
                }

                _jsonSelected = value;
                if (value != null)
                {
                    onJsonSelected();
                }
                RaisePropertyChanged(JsonSelectedPropertyName);
            }
        }

        public RelayCommand LoadDataCommand
        {
            get
            {
                return _loadDataCommand
                    ?? (_loadDataCommand = new RelayCommand(
                    () =>
                    {
                        _dataService.LoadObject((items) =>
                        {
                            Jsons = new ObservableCollection<JsonModel>();
                            items.ForEach(m => Jsons.Add(m));
                            JsonSelected = Jsons[0];
                        });
                    }));
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                if (_message == value)
                {
                    return;
                }

                _message = value;
                RaisePropertyChanged(MessagePropertyName);
            }
        }

        public RelayCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand
                    ?? (_openFileCommand = new RelayCommand(
                    () =>
                    {
                        _dataService.GetFile((items) =>
                        {
                            _files = items;
                            CountFile = _files.Length;
                        });
                    }));
            }
        }

        public RelayCommand PublishCommand
        {
            get
            {
                return _publishCommand
                    ?? (_publishCommand = new RelayCommand(
                    () =>
                    {
                        //Thuc hien update to web
                        _dataService.PublishToWeb(Texts);
                    }));
            }
        }

        public RelayCommand SampletextCommand
        {
            get
            {
                return _sampletextCommand
                    ?? (_sampletextCommand = new RelayCommand(
                    () =>
                    {
                        this.SelectText.VietText = UpperFirstText(this.SelectText.VietText);
                        OnRefreshUI();
                    }));
            }
        }

        public RelayCommand SampleTextCommand
        {
            get
            {
                return _sampleTextCommand
                    ?? (_sampleTextCommand = new RelayCommand(
                    () =>
                    {
                        this.SelectText.VietText = UppercaseWords(this.SelectText.VietText);
                        OnRefreshUI();
                    }));
            }
        }

        public RelayCommand SAMPLETEXTCommand
        {
            get
            {
                return _sAMPLETEXTCommand
                    ?? (_sAMPLETEXTCommand = new RelayCommand(
                    () =>
                    {
                        this.SelectText.VietText = SelectText.VietText.ToUpper();
                        OnRefreshUI();
                    }));
            }
        }

        public RelayCommand SaveDataCommand
        {
            get
            {
                return _saveDataCommand
                    ?? (_saveDataCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var json in Jsons)
                        {
                            if (json.IsChangeJson)
                            {
                                _dataService.SaveJsonToFile(json.ListText);
                                json.IsChangeJson = false;
                            }
                        }
                    }));
            }
        }

        public TextModel SelectText
        {
            get
            {
                return _selectText;
            }

            set
            {
                if (_selectText == value)
                {
                    return;
                }
                _selectText = value;
                RaisePropertyChanged(SelectTextPropertyName);
            }
        }

        public RelayCommand SettingGoogleCommand
        {
            get
            {
                return _settingGoogleCommand
                    ?? (_settingGoogleCommand = new RelayCommand(
                    () =>
                    {
                        IsCopyedGoogle = true;
                        StringBuilder bd = new StringBuilder();
                        foreach (TextModel text in Texts)
                        {
                            bd.Append("<p>" + text.ChinaText + "</p><p>~</p>");
                        }
                        _dataService.SaveTextChinaToFile(bd.ToString());
                    }));
            }
        }

        public RelayCommand TestCommand
        {
            get
            {
                return _testCommand
                    ?? (_testCommand = new RelayCommand(
                    () =>
                    {
                        Texts.RemoveAt(1);
                        int count = JsonSelected.ListText.Count;
                    }));
            }
        }

        public string TextForFind
        {
            get
            {
                return _textForFind;
            }

            set
            {
                if (_textForFind == value)
                {
                    return;
                }

                _textForFind = value;
                RaisePropertyChanged(TextForFindPropertyName);
            }
        }

        public ObservableCollection<TextModel> Texts
        {
            get
            {
                return _texts;
            }

            set
            {
                if (_texts == value)
                {
                    return;
                }

                _texts = value;
                RaisePropertyChanged(TextsPropertyName);
            }
        }

        public TextModel ToSelect
        {
            get
            {
                return _toSelect;
            }

            set
            {
                if (_toSelect == value)
                {
                    return;
                }

                _toSelect = value;
                RaisePropertyChanged(ToSelectPropertyName);
            }
        }

        public RelayCommand TranslateCommand
        {
            get
            {

                return _translateCommand
                    ?? (_translateCommand = new RelayCommand(
                    () =>
                    {
                        List<string> tranedVietPhare = new List<string>();
                        List<string> tranedHanViet = new List<string>();
                        string textChinas;
                        if (IsWorkForAll)
                        {
                            foreach (JsonModel json in Jsons)
                            {
                                textChinas = MergeTextChina(json);

                                _dataService.TranslateVietPharse(textChinas, (vietphare, hanviet) =>
                                {
                                    tranedVietPhare = vietphare;
                                    tranedHanViet = hanviet;
                                    for (int i = 0; i < json.ListText.Count; i++)
                                    {
                                        json.ListText[i].VietPharseText = UpperFirstText(tranedVietPhare[i].Trim());
                                        json.ListText[i].HanVietText = UpperFirstText(tranedHanViet[i].Trim());
                                    }
                                    RaisePropertyChanged(TextsPropertyName);
                                });
                            }
                        }
                        else
                        {
                            textChinas = MergeTextChina(JsonSelected);
                            List<string> tranedGoogle = new List<string>();
                            if (IsCopyedGoogle)
                            {
                                string textGoogle = Clipboard.GetText();
                                //thuc hien dich google truoc
                                string[] textSplitGoogle = textGoogle.Split('~');
                                foreach (string item in textSplitGoogle)
                                {
                                    tranedGoogle.Add(item.Trim());
                                }
                            }
                             _dataService.TranslateVietPharse(textChinas, (vietphare, hanviet) =>
                             {
                                 tranedVietPhare = vietphare;
                                 tranedHanViet = hanviet;
                                 for (int i = 0; i < JsonSelected.ListText.Count; i++)
                                 {
                                     if (IsCopyedGoogle)
                                     {
                                         JsonSelected.ListText[i].GoogleText = UpperFirstText(tranedGoogle[i].Trim());
                                     }

                                     JsonSelected.ListText[i].VietPharseText = UpperFirstText(tranedVietPhare[i].Trim());
                                     JsonSelected.ListText[i].HanVietText = UpperFirstText(tranedHanViet[i].Trim());
                                 }
                                 Message = "File " + JsonSelected.Name + " dịch xong";
                             });
                        }

                        RaisePropertyChanged(TextsPropertyName);
                    }));
            }
        }

        public RelayCommand<KeyEventArgs> WindowKeyDownCommand
        {
            get
            {
                return _windowKeyDownCommand
                    ?? (_windowKeyDownCommand = new RelayCommand<KeyEventArgs>(
                    e =>
                    {
                        if (e.Key == System.Windows.Input.Key.F4 || e.SystemKey == System.Windows.Input.Key.F4)
                        {
                            SampletextCommand.Execute(null);
                        }
                        else if (e.Key == System.Windows.Input.Key.F5 || e.SystemKey == System.Windows.Input.Key.F5)
                        {
                            SampleTextCommand.Execute(null);
                        }
                        else if (e.Key == System.Windows.Input.Key.F6 || e.SystemKey == System.Windows.Input.Key.F6)
                        {
                            SAMPLETEXTCommand.Execute(null);
                        }
                        else if (e.Key == System.Windows.Input.Key.F8 || e.SystemKey == System.Windows.Input.Key.F8)
                        {
                            //Texts[Texts.IndexOf(SelectText)].VietText = this.SelectText.VietPharseText;
                            VietPhare();
                        }
                        else if (e.Key == System.Windows.Input.Key.F9 || e.SystemKey == System.Windows.Input.Key.F9)
                        {
                            CovertHanVietToVietAndRefresh();
                        }
                        else if (e.Key == System.Windows.Input.Key.F10 || e.SystemKey == System.Windows.Input.Key.F10)
                        {
                            CovertGoogleToVietAndRefresh();
                        }
                        else if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                        {
                            if (CheckBoxGroup.IsVietPhare)
                            {
                                VietPhare();
                            }
                            if (CheckBoxGroup.IsHanViet)
                            {
                                CovertHanVietToVietAndRefresh();
                            }
                            if (CheckBoxGroup.IsGoogle)
                            {
                                CovertGoogleToVietAndRefresh();
                            }
                            if (CheckBoxGroup.IsSampletext)
                            {
                                SampletextCommand.Execute(null);
                            }
                            if (CheckBoxGroup.IsSampleText)
                            {
                                SampleTextCommand.Execute(null);
                            }
                            if (CheckBoxGroup.IsSAMPLETEXT)
                            {
                                SAMPLETEXTCommand.Execute(null);
                            }
                            NextItemAndSave();
                            findAndReplaceSameText();
                        }
                        else if (e.Key == System.Windows.Input.Key.Enter || e.SystemKey == System.Windows.Input.Key.Enter)
                        {
                            MessengerInstance.Send<string>("run", "DoIsFocusFind");
                            if (IsFindFocus)
                                findText();
                            else
                            {
                                NextItemAndSave();
                                findAndReplaceSameText();
                            }
                        }
                    }));
            }
        }

        public const string CheckBoxGroupPropertyName = "CheckBoxGroup";
        public const string CountFilePropertyName = "CountFile";
        public const string FromSelectPropertyName = "FromSelect";

        public const string IsTranslatePropertyName = "IsTranslate";
        public const string JsonSelectedPropertyName = "JsonSelected";

        public const string JsonsPropertyName = "Jsons";

        public const string MessagePropertyName = "Message";
        public const string SelectTextPropertyName = "SelectText";
        public const string TextForFindPropertyName = "TextForFind";

        public const string TextsPropertyName = "Texts";
        public const string ToSelectPropertyName = "ToSelect";
        public bool IsFindFocus = false;
        private readonly IDataService _dataService;
        private RelayCommand _allToHanVietCommand;
        private RelayCommand _allToVietPhareCommand;
        private CheckBoxGroupModel _checkBoxGroup = null;
        private RelayCommand _convertChinaToFileCommand;
        private RelayCommand _convertIndexToFileCommand;
        private RelayCommand _convertOptionCommand;
        private RelayCommand _convertToFileCommand;
        private int _countFile = 0;
        private string[] _files;
        private RelayCommand _findTextChinaCommand;
        private TextModel _fromSelect = null;

        private ObservableCollection<JsonModel> _jsons = null;
        private JsonModel _jsonSelected = null;
        private RelayCommand _loadDataCommand;
        private string _message = "";
        private RelayCommand _openFileCommand;
        private RelayCommand _publishCommand;
        private RelayCommand _sampletextCommand;
        private RelayCommand _sampleTextCommand;
        private RelayCommand _sAMPLETEXTCommand;
        private RelayCommand _saveDataCommand;
        private TextModel _selectText = null;
        private RelayCommand _settingGoogleCommand;
        private RelayCommand _testCommand;
        private string _textChinas = "";
        private string _textForFind = "";
        private ObservableCollection<TextModel> _texts = null;
        private TextModel _toSelect = null;
        private RelayCommand _translateCommand;
        private RelayCommand<KeyEventArgs> _windowKeyDownCommand;
        private int CountToSave = 0;
        private bool IsCopyedGoogle = false;
        private BackgroundWorker worker;
        private BackgroundWorker workerTranslate;

        /// <summary>
        /// The <see cref="IsWorkForAll" /> property's name.
        /// </summary>
        public const string WorkForAllPropertyName = "IsWorkForAll";

        private bool _isWorkForAll = false;

        /// <summary>
        /// Sets and gets the WorkForAll property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool IsWorkForAll
        {
            get
            {
                return _isWorkForAll;
            }

            set
            {
                if (_isWorkForAll == value)
                {
                    return;
                }
                Messenger.Default.Send<bool>(value, "onChangedWordForAll");

                _isWorkForAll = value;
                RaisePropertyChanged(WorkForAllPropertyName);
            }
        }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            _dataService.GetData(
                (items, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }
                    if (items != null)
                    {
                        Texts = new ObservableCollection<TextModel>();
                        foreach (var item in items)
                        {
                            Texts.Add(item);
                        }
                    }
                });
            CheckBoxGroup = new CheckBoxGroupModel();
            CheckBoxGroup.IsSampletext = true;
            CheckBoxGroup.IsVietPhare = true;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        /// <summary>
        /// The <see cref="IsSaveAll" /> property's name.
        /// </summary>
        public const string IsSaveAllPropertyName = "IsSaveAll";

        private bool _isSaveAll = false;

        /// <summary>
        /// The <see cref="IsConvertAll" /> property's name.
        /// </summary>
        public const string IsConvertAllPropertyName = "IsConvertAll";

        private bool _isConvertAll = false;

        /// <summary>
        /// Sets and gets the IsConvertAll property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool IsConvertAll
        {
            get
            {
                return _isConvertAll;
            }

            set
            {
                if (_isConvertAll == value)
                {
                    return;
                }
                foreach (JsonModel json in Jsons)
                {
                    json.IsChangeTxt = value;
                }

                _isConvertAll = value;
                RaisePropertyChanged(IsConvertAllPropertyName);
            }
        }

        /// <summary>
        /// Sets and gets the IsSaveAll property.
        /// Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public bool IsSaveAll
        {
            get
            {
                return _isSaveAll;
            }

            set
            {
                if (_isSaveAll == value)
                {
                    return;
                }
                foreach (JsonModel json in Jsons)
                {
                    json.IsChangeJson = value;
                }

                _isSaveAll = value;
                RaisePropertyChanged(IsSaveAllPropertyName);
            }
        }

        private void WorkerTranslate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Message = "Đã dịch file thứ : " + e.ProgressPercentage;
        }
    }
}