using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace ZumenSearch.Presentation.Features.Rent.Lessors.Views;

public sealed partial class BasicPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel? ViewModel { get; private set; }

    private bool _initialized;

    public BasicPage()
    {
        //Debug.WriteLine("ZumenSearch.Presentation.Features.Rent.Residentials.Views.Editor.BasicPage init!");

        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBox_NameLast.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

        // Segmented.SelectedIndex binds two-way to PersonKindIndex, and that setter marks the VM dirty whenever the index changes.
        // The initial control-binding update is being treated as a user edit, so we need to reset IsDirty here.
        ViewModel?.ResetIsDirtyState();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
            /*
            if (!_initialized)
            {
                Init();
            }
            */
        }

        base.OnNavigatedTo(e);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Init();
    }

    private void ComboBoxPref_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxCountyAndCity.IsEnabled = ComboBoxPref.SelectedIndex > -1;
    }

    private void ComboBoxCountyAndCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxWardAndOaza.IsEnabled = ComboBoxCountyAndCity.SelectedIndex > -1;
    }

    private void ComboBoxWardAndOaza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxChoume.IsEnabled = ComboBoxWardAndOaza.SelectedIndex > -1;
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
