using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Views.Brokers;

public sealed partial class BasicPage : Page
{
    public ViewModels.Brokers.BrokerViewModel? ViewModel { get; private set; }

    private bool _initialized;

    public BasicPage()
    {
        //Debug.WriteLine("Views.Rent.Residentials.Editor.BasicPage init!");

        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBox_NameCompany.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Brokers.BrokerViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Brokers.BrokerViewModel;

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
