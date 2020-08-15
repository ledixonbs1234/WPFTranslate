using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateGame.Model
{
    public class CheckBoxGroupModel
    {
        public bool IsSampletext { get; set; }
        public bool IsSampleText { get; set; }
        public bool IsSAMPLETEXT { get; set; }
        public bool IsVietPhare { get; set; }
        public bool IsHanViet { get; set; }
        public bool IsGoogle { get; set; }
        public CheckBoxGroupModel()
        {
            IsSampleText = true;
            IsVietPhare = true;
        }

    }
}
