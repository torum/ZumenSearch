using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class BasicPage : Page
{
    public ViewModels.Rent.Residentials.Bldg.PropertyViewModel? ViewModel { get; private set; }

    private bool _initialized;

    public BasicPage()
    {
        //Debug.WriteLine("Views.Rent.Residentials.Editor.BasicPage init!");

        InitializeComponent();
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBox_Name.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Bldg.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Bldg.PropertyViewModel;

            if (!_initialized)
            {
                //Init();
            }
        }

        base.OnNavigatedTo(e);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Init();
    }

    private void TextBoxDigitOnly_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        /*
        var text = ((TextBox)sender).Text;

        var regex = new Regex("^[0-9]*$");

        string pattern = @"^\d{10,}$"; // 10 or more digits
        if (Regex.IsMatch(text, pattern))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            return;
        }

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
        }
        */
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
        /*
        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            string pattern = @"^(?:\d{5,})?$";//@"^\d{5,}$"; // 5 or more digits
            if (Regex.IsMatch(text, pattern))
            {
                Debug.WriteLine("5 or more digits @TextBoxHalfWidth4DigitOrLess_TextChanging");
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            regex = new Regex(@"^(?:\d{0,4})?$");//new Regex(@"^\d{0,4}$");
            if (!regex.IsMatch(text))
            {
                Debug.WriteLine("^(?:\\d{0,4})?$ @TextBoxHalfWidth4DigitOrLess_TextChanging");
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
        */
    }

    private void TextBox4DigitOnly_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = ((TextBox)sender).Text;

        if (string.IsNullOrEmpty(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            return;
        }

        string pattern = @"^\d{5,}$"; // 5 or more digits
        if (Regex.IsMatch(text, pattern))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            return;
        }

        var regex = new Regex(@"^(?:\d{4})?$");//new Regex(@"^\d{4}$");
        if (regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
        }
        else
        {
            regex = new Regex(@"^(?:\d{0,4})?$");//new Regex(@"^\d{13}$");
            if (regex.IsMatch(text))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
        /*
        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {


        }
        */
    }

    private void TextBox5DigitOrLess_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = ((TextBox)sender).Text;

        if (string.IsNullOrEmpty(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            return;
        }

        var regex = new Regex(@"^(?:\d{6,})?$"); // 6 digit or more.
        if (regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            regex = new Regex(@"^(?:\d{0,5})?$");//new Regex(@"^\d{0,5}$"); // 5 digit or less
            if (regex.IsMatch(text))
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
            else
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
        /*
        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            string pattern = @"^\d{6,}$"; // 6 or more digits
            if (Regex.IsMatch(text, pattern))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            regex = new Regex(@"^(?:\d{0,5})?$");//new Regex(@"^\d{4}$");
            if (!regex.IsMatch(text))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
        */
    }

    private void TextBox13DigitOnly_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        var text = ((TextBox)sender).Text;

        if (string.IsNullOrEmpty(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            return;
        }

        string pattern = @"^\d{14,}$"; // 14 or more digits
        if (Regex.IsMatch(text, pattern))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            return;
        }

        var regex = new Regex(@"^(?:\d{13})?$");//new Regex(@"^\d{13}$");
        if (regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
        }
        else
        {
            regex = new Regex(@"^(?:\d{0,13})?$");//new Regex(@"^\d{13}$");
            if (regex.IsMatch(text))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
        /*
        var regex = new Regex("^[0-9]*$");

        if (!regex.IsMatch(text))
        {
            ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
        }
        else
        {
            string pattern = @"^\d{14,}$"; // 14 or more digits
            if (Regex.IsMatch(text, pattern))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            regex = new Regex(@"^(?:\d{13})?$");//new Regex(@"^\d{13}$");
            if (!regex.IsMatch(text))
            {
                regex = new Regex(@"^(?:\d{0,13})?$");//new Regex(@"^\d{13}$");
                if (!regex.IsMatch(text))
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
        */
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

    private async void TextBoxCleanNonDigitExact4_Paste(object sender, TextControlPasteEventArgs e)
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

            string pattern = @"^\d{5,}$"; // 10 or more digits
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            var regex = new Regex(@"^(?:\d{4})?$");
            if (!regex.IsMatch(textBox.Text))
            {
                regex = new Regex(@"^(?:\d{0,4})?$");
                if (!regex.IsMatch(textBox.Text))
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
    }

    private async void TextBoxCleanNonDigitMax5_Paste(object sender, TextControlPasteEventArgs e)
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

            string pattern = @"^\d{6,}$"; // 6 or more digits
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            var regex = new Regex(@"^(?:\d{0,5})?$");
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

    private async void TextBoxCleanNonDigitExact13_Paste(object sender, TextControlPasteEventArgs e)
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

            var regex = new Regex(@"^(?:\d{13})?$");
            if (!regex.IsMatch(textBox.Text))
            {
                regex = new Regex(@"^(?:\d{0,13})?$");
                if (!regex.IsMatch(textBox.Text))
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    ((TextBox)sender).BorderBrush = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                ((TextBox)sender).BorderBrush = (App.Current.Resources["TextControlBorderBrush"] as Brush)!;
            }
        }
    }

    private async void TextBox_MaxLength_Paste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;

        if (sender is TextBox textBox)
        {
            // 2. Grab the text payload from the clipboard
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string pasteText = await dataPackageView.GetTextAsync();

                // 3. Determine how many characters are allowed to be added
                // (Takes into account any text the user currently has highlighted/selected for replacement)
                int currentLengthWithoutSelection = textBox.Text.Length - textBox.SelectionLength;
                int allowedRemainingChars = textBox.MaxLength - currentLengthWithoutSelection;

                if (allowedRemainingChars > 0)
                {
                    // Truncate the text if it exceeds the remaining allowed space
                    if (pasteText.Length > allowedRemainingChars)
                    {
                        pasteText = pasteText.Substring(0, allowedRemainingChars);
                    }

                    // 4. Insert the allowed text at the current cursor position
                    int selectionStart = textBox.SelectionStart;

                    // Replace selected text or insert at cursor
                    string newText = textBox.Text.Remove(selectionStart, textBox.SelectionLength);
                    textBox.Text = newText.Insert(selectionStart, pasteText);

                    // 5. Restore the cursor position to the end of the newly pasted text
                    textBox.SelectionStart = selectionStart + pasteText.Length;
                }
            }
        }
    }
}
