using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TranslateGame.Model;

namespace TranslateGame.Design
{
    public class DesignDataService :IDataService
    {
        public void ConvertListJsonToFiles(List<JsonModel> jsonNeedSaved, Action<string> callback)
        {
            throw new NotImplementedException();
        }

        public void FileTextChina(string path, Action<JsonModel> callback)
        {
            throw new NotImplementedException();
        }

        public void GetData(Action<List<TextModel>, Exception> callback)
        {
            // Use this to create design time data

            List<TextModel> texts = new List<TextModel>();
            TextModel t = new TextModel("dsfsd");
            t.ChinaText = "查看当前传承点";
            t.VietText = "chao cac ban";
            t.STT = 123;
            for (int i = 0; i < 15; i++)
            {
                texts.Add(t);
            }
            
            callback(texts, null);
        }

        public void GetFile(Action<string[]> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadObject(Action<List<JsonModel>> callback)
        {
            throw new NotImplementedException();
        }

        public void PublishToWeb(ObservableCollection<TextModel> data)
        {
            throw new NotImplementedException();
        }

        public void SaveJsonToFile(List<TextModel> data)
        {
            throw new NotImplementedException();
        }

        public void SaveTextChinaToFile(string text)
        {
            throw new NotImplementedException();
        }

        public void Translate3Section(string mergeTextChina, Action<List<string>> callback)
        {
            throw new NotImplementedException();
        }

        public void TranslateGoogle(List<string> merges, Action<List<string>> callback)
        {
            throw new NotImplementedException();
        }

        public void TranslateVietPharse(string mergeText, Action<List<string>, List<string>> callback)
        {
            throw new NotImplementedException();
        }
    }
}