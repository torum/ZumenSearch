using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class TransportationPage : Page
{
    public ViewModels.Rent.Residentials.Bldg.PropertyViewModel? ViewModel { get; private set; }

    public TransportationPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Bldg.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Bldg.PropertyViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private void TextBoxRailLine1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxRailLine1.Text))
        {
            TextBoxRailLine1.IsEnabled = false;
        }
        else
        {
            TextBoxRailLine1.IsEnabled = true;
        }
    }

    private void TextBoxRailStation1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxRailStation1.Text))
        {
            TextBoxRailStation1.IsEnabled = false;
        }
        else
        {
            TextBoxRailStation1.IsEnabled = true;
        }
    }

    private void TextBoxHalfWidthDigitOnly_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = ((TextBox)sender).Text;

        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
        }

        /*
        var currentPosition = sender.SelectionStart - 1;
        var text = ((TextBox)sender).Text;

        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            var foundChar = Regex.Match(sender.Text, @"[^0-9]");
            if (foundChar.Success)
            {
                sender.Text = sender.Text.Remove(foundChar.Index, 1);
            }

            sender.Select(currentPosition, 0);
        }
        */
    }

    private async void TextBoxCleanNonDigit_Paste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;

        var textBox = sender as TextBox;
        if (textBox == null) return;

        // 2. Safely grab the clipboard contents
        var dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.Text))
        {
            // Fetch text asynchronously
            string originalText = await dataPackageView.GetTextAsync();

            // 3. Perform your custom modifications (e.g., removing all line breaks)
            string cleanedText = Helpers.Common.ReplaceZenkakuNumbers(originalText);

            // 4. Manually insert the manipulated text at the current cursor position
            int selectionStart = textBox.SelectionStart;

            // Remove any currently highlighted text before inserting
            if (textBox.SelectionLength > 0)
            {
                textBox.Text = textBox.Text.Remove(selectionStart, textBox.SelectionLength);
            }

            // Inject the text
            textBox.Text = textBox.Text.Insert(selectionStart, cleanedText);

            // Move the caret forward to the end of your newly pasted text
            textBox.SelectionStart = selectionStart + cleanedText.Length;
        }
    }
}
