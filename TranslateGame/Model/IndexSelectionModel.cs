using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateGame.Model
{
    public class IndexSelectionModel
    {
        public int Start { get; set; }
        public int End { get; set; }
        public IndexSelectionModel(int start,int end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
