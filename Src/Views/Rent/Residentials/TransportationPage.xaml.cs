using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class TransportationPage : Page
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }

    public TransportationPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.PropertyViewModel;
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

    private void TextBox4DigitOrLess_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = ((TextBox)sender).Text;

        if (string.IsNullOrEmpty(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            return;
        }

        var regex = new Regex(@"^(?:\d{5,})?$"); // 5 digit or more.
        if (regex.IsMatch(text))
        {
            Debug.WriteLine("5 or more digits @TextBoxHalfWidth4DigitOrLess_TextChanging");
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            regex = new Regex(@"^(?:\d{0,4})?$");//new Regex(@"^\d{0,4}$"); // 4 digit or less
            if (regex.IsMatch(text))
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
            else
            {
                Debug.WriteLine("Not 4 digit or less @TextBoxHalfWidth4DigitOrLess_TextChanging");
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }

    private async void TextBoxCleanNonDigitMax4_Paste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;

        var textBox = sender as TextBox;
        if (textBox == null) return;

        var dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.Text))
        {
            // Fetch text asynchronously
            string originalText = await dataPackageView.GetTextAsync();

            string cleanedText = Helpers.Common.ReplaceZenkakuNumbers(originalText);

            // Manually insert the manipulated text at the current cursor position
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

            string pattern = @"^\d{5,}$"; // 5 or more digits
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (!regex.IsMatch(textBox.Text))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
    }
}
