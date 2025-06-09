using System.Windows;

namespace WpfIronPythonApp.Views
{
    /// <summary>
    /// 輸入對話框
    /// </summary>
    public partial class InputDialog : Window
    {
        public string InputText { get; set; } = "";

        public InputDialog(string prompt, string title = "輸入")
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            DataContext = this;
            
            // 設定焦點到輸入框
            Loaded += (s, e) => InputTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 