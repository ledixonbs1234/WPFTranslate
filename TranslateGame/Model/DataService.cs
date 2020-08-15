using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TranslateGame.Model
{
    public class DataService : IDataService
    {
        private const string URLHANVIET = "http://vietphrase.info/Vietphrase/TranslateHanViet";
        private const string URLVIETPHARE = "http://vietphrase.info/Vietphrase/TranslateVietPhraseS";

        private string[] _files;

        public void GetData(Action<List<TextModel>, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(null, null);
        }

        public void GetFile(Action<string[]> callback)
        {
            OpenFileDialog of = new OpenFileDialog();

            of.Multiselect = true;

            if (of.ShowDialog() == true)
            {
                this._files = of.FileNames;

                callback(of.FileNames);
            }
        }

        public async void PublishToWeb(ObservableCollection<TextModel> data)
        {
            HttpResponseMessage respon;
            string json = JsonConvert.SerializeObject(data);
            using (var client = new HttpClient())
            {
                Uri url = new Uri("https://api.jsonbin.io/b/5f1ba72b9180616628486d65");
                string key = "$2b$10$I6nC5O8Afebi/cTFNddD9u4kh3YLZZ5zyVaYfsNd65llvfVuUFzii";
                //var payload = "{\"data\" : \"" + json + "\"}";
                var payload = json;
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = url,
                    Content = content,
                };
                client.DefaultRequestHeaders.Add("secret-key", key);

                respon = await client.SendAsync(request);
                if (respon.IsSuccessStatusCode)
                {
                }
            }
        }

        private string readTextFromPath(string path)
        {
            string text = "";
            using (StreamReader sr = new StreamReader(path))
            {
                text = sr.ReadToEnd();
            }
            return text;
        }

        public void FileTextChina(string path, Action<JsonModel> callback)
        {
            Regex regex = new Regex("[\u4E00-\u9FA5]");
            List<TextModel> listText; int countList = 1;
            JsonModel json;
            string textContent = readTextFromPath(path);
            MatchCollection listWordChina = regex.Matches(textContent);
            listText = new List<TextModel>();

            TextModel text;
            int count = 0;
            int countSTT = 1;
            bool isAddLastChina = false;
            for (int i = 1; i < listWordChina.Count; i++)
            {
                if (listWordChina[i].Index - listWordChina[i - 1].Index != 1 || listWordChina.Count - 1 == i)
                {
                    countList++;
                    text = new TextModel(path);
                    text.StartIndex = listWordChina[count].Index;

                    if (listWordChina.Count - 1 == i && listWordChina[i].Index - listWordChina[i - 1].Index == 1)
                    {
                        text.ChinaText = textContent.Substring(text.StartIndex, (listWordChina[i].Index - text.StartIndex) + 1);
                    }
                    else if (listWordChina.Count - 1 == i && listWordChina[i].Index - listWordChina[i - 1].Index != 1)
                    {
                        text.ChinaText = textContent.Substring(text.StartIndex, (listWordChina[i - 1].Index - text.StartIndex) + 1);
                        //add last china
                        isAddLastChina = true;
                    }
                    else
                    {
                        text.ChinaText = textContent.Substring(text.StartIndex, (listWordChina[i - 1].Index - text.StartIndex) + 1);
                    }
                    text.RealLineText = GetLineTextInIndex(textContent, text.StartIndex, text.ChinaText);

                    count = i;
                    text.STT = countSTT;
                    listText.Add(text);
                    if (isAddLastChina)
                    {
                        text = new TextModel(path);
                        text.StartIndex = listWordChina[listWordChina.Count - 1].Index;
                        text.ChinaText = textContent.Substring(text.StartIndex, 1);
                        text.RealLineText = GetLineTextInIndex(textContent, text.StartIndex, text.ChinaText);
                        text.STT = countSTT;

                        listText.Add(text);
                        isAddLastChina = false;
                    }
                    countSTT++;
                }
            }
            json = new JsonModel();
            json.FullPath = path;
            json.IsChangeJson = false;
            json.IsChangeTxt = false;
            json.Name = Path.GetFileNameWithoutExtension(path);
            json.ListText = listText;
            callback(json);
        }

        //Lay line cua hang
        public string GetLineTextInIndex(string content, int index, string chinaText)
        {
            string startContent = content.Substring(0, index);
            int startIndex = startContent.LastIndexOf('\n');
            string endContent = content.Substring(startIndex + 1);
            int endIndex = endContent.IndexOf('\n');
            string betweenContent;
            if (endIndex != -1)
            {
                betweenContent = content.Substring(startIndex + 1, endIndex);
            }
            else
            {
                betweenContent = content.Substring(startIndex + 1);
            }
            int resultIndex;
            //thuc hien lay index cua file hien tai
            if (startIndex != -1)
            {
                resultIndex = index - startIndex - 2;
            }
            else
            {
                resultIndex = index - startIndex - 1;
            }
            string result = betweenContent.Remove(resultIndex + 1, chinaText.Length).Insert(resultIndex + 1, "<--" + chinaText + "-->");
            return result;
        }

        private async Task<HttpResponseMessage> GetTextFromWeb(string urlWebPost, string textChina)
        {
            HttpResponseMessage respon;
            using (var client = new HttpClient())
            {
                Uri url = new Uri(urlWebPost);
                var payload = "{\"chineseContent\" : \"" + textChina + "\"}";
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = url,
                    Content = content
                };

                respon = await client.SendAsync(request);
                if (respon.IsSuccessStatusCode)
                {
                    return respon;
                }
            }
            return respon;
        }

        public void SaveTextChinaToFile(string text)
        {
            //get forder path current working
            string pathFile = CreateFile("ChinaText.html", text);
            Process.Start(pathFile);
        }

        public async void TranslateVietPharse(string mergeText, Action<List<string>, List<string>> callback)
        {
            HttpResponseMessage responseVietPhare = await GetTextFromWeb(URLVIETPHARE, mergeText);
            string tempVietPhare = responseVietPhare.Content.ReadAsStringAsync().Result;
            string[] splitTextVietPhare = tempVietPhare.Split('~');
            List<string> listVietPhare = new List<string>();
            listVietPhare.AddRange(splitTextVietPhare);
            HttpResponseMessage responseHanViet = await GetTextFromWeb(URLHANVIET, mergeText);
            string tempHanViet = responseHanViet.Content.ReadAsStringAsync().Result;
            string[] splitTextHanViet = tempHanViet.Split('~');
            List<string> listHanViet = new List<string>();
            listHanViet.AddRange(splitTextHanViet);
            callback(listVietPhare, listHanViet);
        }

        public void SaveJsonToFile(List<TextModel> data)
        {
            string json = JsonConvert.SerializeObject(data);
            //get name file
            if (data == null)
            {
                return;
            }
            string path = data[0].FullPath;
            string name = Path.GetFileNameWithoutExtension(path);
            CreateFile(name + ".json", json);
        }

        private string CreateFile(string filename, string content)
        {
            string pathForlder = Environment.CurrentDirectory;
            string pathFile = pathForlder + "\\" + filename;
            File.WriteAllText(pathFile, content);
            return pathFile;
        }

        public void LoadObject(Action<List<JsonModel>> callback)
        {
            string pathForlder = Environment.CurrentDirectory;
            //get File

            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = pathForlder;
            of.Multiselect = true;
            of.DefaultExt = ".json";
            of.Filter = "Json File (.json)|*.json";

            if (of.ShowDialog() == true)
            {
                List<JsonModel> listJson = new List<JsonModel>();
                foreach (string pathFile in of.FileNames)
                {
                    string json = File.ReadAllText(pathFile);

                    List<TextModel> data = JsonConvert.DeserializeObject<List<TextModel>>(json);
                    JsonModel jsonModel = new JsonModel();
                    jsonModel.FullPath = pathFile;
                    jsonModel.Name = Path.GetFileNameWithoutExtension(pathFile);
                    jsonModel.IsChangeJson = false;
                    jsonModel.IsChangeTxt = false;
                    jsonModel.ListText = data;
                    listJson.Add(jsonModel);
                }
                callback(listJson);
            }
            else
                callback(null);
        }

        public void ConvertListJsonToFiles(List<JsonModel> jsonNeedSaved, Action<string> callback)
        {
            string contentFile = "";
            int count = 0;
            string pathFile = "";
            foreach (JsonModel json in jsonNeedSaved)
            {
                callback(json.Name);
                pathFile = json.ListText[0].FullPath;
                contentFile = File.ReadAllText(pathFile);
                count = 0;

                foreach (TextModel text in json.ListText)
                {
                    if (!string.IsNullOrEmpty(text.VietText))
                    {
                        contentFile = contentFile.Remove(text.StartIndex + count, text.ChinaText.Length)
                                                    .Insert(text.StartIndex + count, text.VietText);
                        count += (text.VietText.Length - text.ChinaText.Length);
                    }
                }
                FileInfo file = new FileInfo(pathFile);
                var dir = file.Directory.CreateSubdirectory("Finish");
                string exportPath = dir.FullName + "\\" + file.Name;
                File.WriteAllText(exportPath, contentFile);
            }
        }
    }
}