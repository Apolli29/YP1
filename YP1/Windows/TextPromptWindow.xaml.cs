using System.Windows;

namespace YP1.Windows
{
    public partial class TextPromptWindow : Window
    {
        public TextPromptWindow(string title, string description, string initialText)
        {
            InitializeComponent();
            Title = title;
            HeaderTextBlock.Text = title;
            DescriptionTextBlock.Text = description;
            InputTextBox.Text = initialText;
            InputTextBox.Focus();
            InputTextBox.CaretIndex = InputTextBox.Text.Length;
        }

        public string ResultText
        {
            get { return InputTextBox.Text.Trim(); }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Поле не должно быть пустым.", "Пустой текст", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
