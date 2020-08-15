using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TranslateGame.Model
{
    public interface IDataService
    {
        void GetData(Action<List<TextModel>, Exception> callback);
        void GetFile(Action<string[]> callback);
        void FileTextChina(string path, Action<JsonModel> callback);
        void SaveTextChinaToFile(string text);
        void TranslateVietPharse(string mergeText, Action<List<string>, List<string>> callback);
        void LoadObject(Action<List<JsonModel>> callback);
        void SaveJsonToFile(List<TextModel> data);
        void PublishToWeb(ObservableCollection<TextModel> data);
        void ConvertListJsonToFiles(List<JsonModel> jsonNeedSaved, Action<string> callback);
    }
}
