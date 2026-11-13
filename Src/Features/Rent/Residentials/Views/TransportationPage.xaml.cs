using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
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
        if (e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

#pragma warning disable CA1822 // Mark members as static
    private void TextBox4DigitOrLess_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
#pragma warning restore CA1822 // Mark members as static
    {
        UpdateNumericBorder(sender);
    }

    private void TextBox4DigitOrLess_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox) UpdateNumericBorder(textBox);
    }

    private static void UpdateNumericBorder(TextBox textBox)
    {
        var normalized = Helpers.Common.NormalizeFullWidthCharacters(textBox.Text.Trim());
        textBox.BorderBrush = normalized.Length <= 4 && normalized.All(character => character >= '0' && character <= '9')
            ? (App.Current.Resources["TextControlBorderBrush"] as Brush)!
            : new SolidColorBrush(Colors.Red);
    }

    private async void TextBoxCleanNonDigitMax4_Paste(object sender, TextControlPasteEventArgs e)
    {
        e.Handled = true;
        if (ViewModel is not { } vm || sender is not TextBox textBox) return;
        var selectedTransportationIndex = vm.SelectedTransportationIndex;
        var selectionChanged = false;
        void OnPropertyChanged(object? changedSender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(vm.SelectedTransportationIndex)) selectionChanged = true;
        }
        vm.PropertyChanged += OnPropertyChanged;
        //vm.BeginPicker();
        try
        {
            var clipboard = Clipboard.GetContent();
            if (!clipboard.Contains(StandardDataFormats.Text)) return;

            var pastedText = Helpers.Common.NormalizeFullWidthCharacters(await clipboard.GetTextAsync());
            if (selectionChanged || vm.SelectedTransportationIndex != selectedTransportationIndex)
            {
                vm.InfoBarErrorMessage = "交通の選択が変更されたため、貼り付けを中止しました。";
                vm.IsInfoBarErrorOpen = true;
                return;
            }

            var selectionStart = textBox.SelectionStart;
            var candidate = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, pastedText);
            var normalized = Helpers.Common.NormalizeFullWidthCharacters(candidate);
            if (normalized.Length > textBox.MaxLength || normalized.Any(character => character < '0' || character > '9'))
            {
                vm.InfoBarErrorMessage = $"貼り付ける値は0以上の{textBox.MaxLength}桁以内の整数にしてください。";
                vm.IsInfoBarErrorOpen = true;
                return;
            }

            textBox.Text = candidate;
            textBox.SelectionStart = selectionStart + pastedText.Length;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to paste clipboard text: {ex}");
            vm.InfoBarErrorMessage = $"貼り付けできませんでした: {ex.Message}";
            vm.IsInfoBarErrorOpen = true;
        }
        finally
        {
            vm.PropertyChanged -= OnPropertyChanged;
            //vm.EndPicker();
        }
    }
}
