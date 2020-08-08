using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslateGame.Model
{
    public class JsonModel: ViewModelBase
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public List<TextModel> ListText { get;
            set; }

        
        
        
        public const string IsChangeJsonPropertyName = "IsChangeJson";

        private bool _isChangeJson = false;

        

        
        
        
        
        public bool IsChangeJson
        {
            get
            {
                return _isChangeJson;
            }

            set
            {
                if (_isChangeJson == value)
                {
                    return;
                }

                _isChangeJson = value;
                RaisePropertyChanged(IsChangeJsonPropertyName);
            }
        }
        
        
        
        public const string IsChangeTxtPropertyName = "IsChangeTxt";

        private bool _isChangeTxt = false;

        
        
        
        
        public bool IsChangeTxt
        {
            get
            {
                return _isChangeTxt;
            }

            set
            {
                if (_isChangeTxt == value)
                {
                    return;
                }

                _isChangeTxt = value;
                RaisePropertyChanged(IsChangeTxtPropertyName);
            }
        }
    }
}
