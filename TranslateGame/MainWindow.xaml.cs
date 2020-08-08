using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using TranslateGame.Model;
using TranslateGame.ViewModel;
using System;

namespace TranslateGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<string>(this,"Dofocus", doFocus);
            Messenger.Default.Register<string>(this, "DoIsFocusFind", checkFocusFind);
            Messenger.Default.Register<TextModel>(this, "doScrollToView", doScrollToView);
            Messenger.Default.Register<bool>(this, "onChangedWordForAll", onChangedWordForAll);

        }
        private void onChangedWordForAll(bool isChanged)
        {
            if (isChanged)
            {
                txtTranslate.Foreground = Brushes.Red;
                btnNhap.Background = Brushes.Red;
                btnConvert.Background = Brushes.Red;
            }
            else
            {
                txtTranslate.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF535353");
                btnNhap.Background = (Brush)new BrushConverter().ConvertFrom("#FF4E96FF");
                btnConvert.Background = (Brush)new BrushConverter().ConvertFrom("#FF4E96FF");
            }
        }

        private void doScrollToView(TextModel obj)
        {
            dataGrid.ScrollIntoView(obj);
        }

        void checkFocusFind(string run)
        {
            if (run == "run")
            {
                ((MainViewModel)this.DataContext).IsFindFocus = txtFind.IsFocused;
                
            }

        }

        public void doFocus(string msg)
        {
            if (msg == "focus")
            {
                this.txtNhap.Focus();
                this.dataGrid.CancelEdit();
            }
        }
    }
}