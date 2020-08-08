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