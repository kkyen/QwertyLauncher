using System.Windows;

namespace QwertyLauncher.Views
{
    /// <summary>
    /// InputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class InputBox : Window
    {
        internal InputBox(string _title)
        {
            InitializeComponent();
            this.Title = _title;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            value.Text = null;
            Close();
        }
    }
}
