using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace ZumenSearch.Helpers;

internal static class NumericPasteHelper
{
    public static async Task<bool> PasteAsync(TextBox textBox)
    {
        var clipboard = Clipboard.GetContent();
        if (!clipboard.Contains(StandardDataFormats.Text)) return true;

        var pastedText = Common.NormalizeFullWidthCharacters(await clipboard.GetTextAsync());
        var selectionStart = textBox.SelectionStart;
        var candidate = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, pastedText);
        var normalized = Common.NormalizeFullWidthCharacters(candidate);
        if (normalized.Length > textBox.MaxLength || normalized.Any(character => character < '0' || character > '9'))
        {
            return false;
        }

        textBox.Text = candidate;
        textBox.SelectionStart = selectionStart + pastedText.Length;
        return true;
    }
}
