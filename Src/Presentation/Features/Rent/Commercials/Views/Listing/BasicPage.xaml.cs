using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class BasicPage : Page
{
    private bool _initialized;

    public BasicPage()
    {
        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Init();
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void AdditionalCode_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        if (ViewModel is not { } vm) return;

        var selectionStart = sender.SelectionStart;
        vm.FudousanIdAdditionalCode = sender.Text;
        if (sender.Text == vm.FudousanIdAdditionalCode) return;

        sender.Text = vm.FudousanIdAdditionalCode;
        sender.SelectionStart = Math.Min(selectionStart, sender.Text.Length);
    }

    private async void AdditionalCode_Paste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;
        if (sender is not TextBox textBox || ViewModel is not { } vm) return;

        try
        {
            if (!await ZumenSearch.Presentation.Helpers.AdditionalCodeInputHelper.PasteAsync(textBox))
            {
                vm.ShowSimpleErrorInfoBar("入力項目に誤りがあります", "特定コードは4文字以内の半角英数字で入力してください。英字は大文字に変換されます。");
            }
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to paste listing additional code: {ex}");
            vm.ShowSimpleErrorInfoBar("クリップボードのエラー", $"貼り付けできませんでした: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }
        base.OnNavigatedTo(e);
    }
}
