using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using TranslateGame.ViewModel;

namespace TranslateGame
{
    /// <summary>
    /// Description for EditFile.
    /// </summary>
    public partial class EditFile : Window
    {
        /// <summary>
        /// Initializes a new instance of the EditFile class.
        /// </summary>
        public EditFile()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "onCloseEdit", onCloseEdit);            
        }
        void onCloseEdit(string noti)
        {
            this.Close();
        }

        private void txtFrom_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as EditFileViewModel).FromIndex = txtContent.SelectionStart;
        }

        private void txtTo_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as EditFileViewModel).ToIndex = txtContent.SelectionStart;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (this.DataContext as EditFileViewModel).RefreshData();
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.chkHV.IsChecked = true;
            this.chkST.IsChecked = true;
            this.checkBox.IsChecked = false;
            this.checkBox_Copy1.IsChecked = false;
            this.checkBox_Copy2.IsChecked = false;
            this.checkBox_Copy4.IsChecked = false;
            
        }
    }
}