using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TranslateGame.Model;
using TranslateGame.ViewModel;

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
            Messenger.Default.Register<string>(this, "Dofocus", doFocus);
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

        private void checkFocusFind(string run)
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

        private List<string> listText;
        private List<string> listSelectedText;

        private void txtNhap_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftCtrl || e.SystemKey == System.Windows.Input.Key.LeftCtrl)
            {
                               string text = txtNhap.Text;

                string selectedText = txtNhap.SelectedText;
                if (string.IsNullOrEmpty(selectedText))
                {
                    return;
                }
                string unselectedText = text.Replace(selectedText, " ");
                listText = splitText(unselectedText);
                listSelectedText = splitText(selectedText);
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;
                foreach (string item in listText)
                {
                    menuItem = new MenuItem();
                    menuItem.Header = item;
                    menuItem.Click += MenuItem_Click;
                    menu.Items.Add(menuItem);
                }
                txtNhap.ContextMenu = menu;
                txtNhap.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string header = ((MenuItem)sender).Header.ToString();
            int index = listText.IndexOf(header) +1;
            listText.InsertRange(index, listSelectedText);
            string text = "";
            for (int i = 0; i < listText.Count; i++)
            {
                if (i == listText.Count - 1)
                    text += listText[i];
                else
                    text += listText[i] + " ";
            }
            txtNhap.Text = text;
            ((MainViewModel)DataContext).SampletextCommand.Execute(null);
        }

        private List<string> splitText(string text)
        {
            string[] textSplited = text.Trim().Split(' ');
            List<string> listText = new List<string>();
            listText.AddRange(textSplited);
            List<string> listclearNull = new List<string>();
            foreach (string item in listText)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    listclearNull.Add(item);
            }
            return listclearNull;
        }
    }
}