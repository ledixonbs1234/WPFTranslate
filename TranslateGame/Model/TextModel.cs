using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateGame.Model
{
    public class TextModel
    {
        public string ChinaText { get; set; }
        public int STT { get; set; }
        public string VietText { get; set; }
        public string VietPharseText { get; set; }
        public string HanVietText { get; set; }
        public string GoogleText { get; set; }
        public int StartIndex { get; set; }
        public string FullPath { get; set; }
        public string RealLineText { get; set; }


        public TextModel(string path)
        {
            this.FullPath = path;
        }
    }
}
