using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using TranslateGame.Model;

namespace TranslateGame.ViewModel
{






    public class EditFileViewModel : ViewModelBase
    {



        public void OnLoadViewModel()
        {
            this.Content = File.ReadAllText(jsonSelected.ListText[0].FullPath);
            CheckBoxGroup = new CheckBoxGroupModel();
            RaisePropertyChanged(CheckBoxGroupPropertyName);
        }

        private static string UppercaseWords(string value)
        {
            char[] array = value.ToCharArray();

            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }


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

        /// <summary>
        /// The <see cref="RegexText" /> property's name.
        /// </summary>
        public const string RegexTextPropertyName = "RegexText";

        private string _regexText = @"「((\w|\W)+?)」";

        /// <summary>
        /// Sets and gets the RegexText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RegexText
        {
            get
            {
                return _regexText;
            }

            set
            {
                if (_regexText == value)
                {
                    return;
                }

                _regexText = value;
                RaisePropertyChanged(RegexTextPropertyName);
            }
        }
        private RelayCommand _findRegexCommand;

        /// <summary>
        /// Gets the FindRegexCommand.
        /// </summary>
        public RelayCommand FindRegexCommand
        {
            get
            {
                return _findRegexCommand
                    ?? (_findRegexCommand = new RelayCommand(
                    () =>
                    {
                        var regex = new Regex(RegexText);
                        var matches = regex.Matches(Content);
                        Texts.Clear();
                        List<string> listmatch = new List<string>();

                        for (int i = 0; i < matches.Count; i++)
                        {
                            string value = matches[i].Groups[CurrentGroup].Value;
                            if (string.IsNullOrEmpty(value))
                            {
                                continue;
                            }
                            listmatch.Add(value);
                        }
                        if (listmatch.Count == 0)
                        {
                            return;
                        }
                        foreach (TextModel text in jsonSelected.ListText)
                        {
                            for (int i = 0; i < listmatch.Count; i++)
                            {
                                if (text.ChinaText == listmatch[i])
                                {
                                    Texts.Add(text);
                                    break;
                                }
                            }
                        }
                        FromSelect = 0;
                        ToSelect = Texts.Count - 1;
                    }));
            }
        }

        /// <summary>
        /// The <see cref="CurrentGroup" /> property's name.
        /// </summary>
        public const string CurrentGroupPropertyName = "CurrentGroup";

        private int _currentGroup = 0;

        /// <summary>
        /// Sets and gets the CurrentGroup property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int CurrentGroup
        {
            get
            {
                return _currentGroup;
            }

            set
            {
                if (_currentGroup == value)
                {
                    return;
                }

                _currentGroup = value;
                RaisePropertyChanged(CurrentGroupPropertyName);
            }
        }

        private int GetIndexFromStart(string source, int line, int start)
        {
            int currentLine = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (currentLine == line)
                    return i + start;

                if (source[i] == '\n')
                    currentLine++;
            }
            return -1;
        }

        private Tuple<int, int> GetStartAndLineFromIndex(string source, int index)
        {
            int currentLine = 0;
            int count = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (i == index)
                    return Tuple.Create(count, currentLine);
                count++;

                if (source[i] == '\n')
                {
                    currentLine++;
                    count = 0;
                }
            }
            return null;
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

        public string Content
        {
            get
            {
                return _content;
            }

            set
            {
                if (_content == value)
                {
                    return;
                }

                _content = value;
                RaisePropertyChanged(ContentPropertyName);
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

                        int IndexFrom = FromSelect;
                        int IndexTo = ToSelect;
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
                        RaisePropertyChanged(TextsPropertyName);
                        CollectionViewSource.GetDefaultView(Texts).Refresh();
                    }));
            }
        }

        public RelayCommand FindTextCommand
        {
            get
            {
                return _findTextCommand
                    ?? (_findTextCommand = new RelayCommand(
                    () =>
                    {
                        Tuple<int, int> fromInfo = GetStartAndLineFromIndex(Content, FromIndex);
                        Tuple<int, int> toInfo = GetStartAndLineFromIndex(Content, ToIndex);
                        int space = toInfo.Item1 - fromInfo.Item1;
                        if (Texts == null)
                        {
                            Texts = new ObservableCollection<TextModel>();
                        }
                        else
                            Texts.Clear();

                        string[] lines = Content.Split('\n');
                        for (int i = fromInfo.Item2; i <= toInfo.Item2; i++)
                        {
                            for (int j = fromInfo.Item1; j < toInfo.Item1; j++)
                            {
                                int indexReal = GetIndexFromStart(Content, i, j);
                                foreach (TextModel text in jsonSelected.ListText)
                                {
                                    if (text.StartIndex == indexReal)
                                    {
                                        Texts.Add(text);
                                        break;
                                    }
                                    if (text.StartIndex > indexReal)
                                    {
                                        break;
                                    }
                                }


                            }
                        }
                        ToSelect = Texts.Count - 1; }));
            }
        }

        private RelayCommand _completeCommand;

        /// <summary>
        /// Gets the CompleteCommand.
        /// </summary>
        public RelayCommand CompleteCommand
        {
            get
            {
                return _completeCommand
                    ?? (_completeCommand = new RelayCommand(
                    () =>
                    {
                        //thuc hien viec chuyen tu dang sua dua ve text

                        MessengerInstance.Send<ObservableCollection<TextModel>>(Texts, "onUpdateTexts");
                        MessengerInstance.Send<string>("onCloseEdit", "onCloseEdit");
                        RefreshData();

                    }));
            }
        }

        public void RefreshData()
        {
            Texts.Clear();
            ToIndex = 0;
            FromIndex = 0;
            FromSelect = 0;
            ToSelect = 0;
            CheckBoxGroup = new CheckBoxGroupModel();

        }
    

        public int FromIndex
        {
            get
            {
                return _fromIndex;
            }

            set
            {
                if (_fromIndex == value)
                {
                    return;
                }

                _fromIndex = value;
                RaisePropertyChanged(FromIndexPropertyName);
            }
        }

        public int FromSelect
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

        public RelayCommand SelectFromCommand
        {
            get
            {
                return _selectFromCommand
                    ?? (_selectFromCommand = new RelayCommand(
                    () =>
                    {
                    }));
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

        public int ToIndex
        {
            get
            {
                return _toIndex;
            }

            set
            {
                if (_toIndex == value)
                {
                    return;
                }

                _toIndex = value;
                RaisePropertyChanged(ToIndexPropertyName);
            }
        }

        public int ToSelect
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

        public const string CheckBoxGroupPropertyName = "CheckBoxGroup";

        public const string ContentPropertyName = "Content";

        public const string FromIndexPropertyName = "FromIndex";

        public const string FromSelectPropertyName = "FromSelect";

        public const string TextsPropertyName = "Texts";

        public const string ToIndexPropertyName = "ToIndex";

        public const string ToSelectPropertyName = "ToSelect";

        public JsonModel jsonSelected;

        public int SelectIndex;

        private CheckBoxGroupModel _checkBoxGroup = null;

        private string _content = "Chao";

        private RelayCommand _convertOptionCommand;

        private RelayCommand _findTextCommand;

        private int _fromIndex = 0;

        private int _fromSelect = 0;

        private RelayCommand _selectFromCommand;

        private ObservableCollection<TextModel> _texts = null;

        private int _toIndex = 0;

        private int _toSelect = 0;

        public EditFileViewModel()
        {
            CheckBoxGroup = new CheckBoxGroupModel();
            Texts = new ObservableCollection<TextModel>();
        }
    }
}