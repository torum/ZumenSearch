using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using ZumenSearch.Presentation.Utilities;

namespace ZumenSearch.Presentation.Helpers;

internal static class AdditionalCodeInputHelper
{
    public static bool TryNormalize(string value, out string normalized)
    {
        normalized = Common.NormalizeFullWidthCharacters(value).Trim().ToUpperInvariant();
        return normalized.Length <= 4 && normalized.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9');
    }

    public static async Task<bool> PasteAsync(TextBox textBox)
    {
        var clipboard = Clipboard.GetContent();
        if (!clipboard.Contains(StandardDataFormats.Text)) return true;

        var pastedText = Common.NormalizeFullWidthCharacters(await clipboard.GetTextAsync()).Trim().ToUpperInvariant();
        var selectionStart = textBox.SelectionStart;
        var candidate = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, pastedText);
        if (!TryNormalize(candidate, out var normalized)) return false;

        textBox.Text = normalized;
        textBox.SelectionStart = Math.Min(selectionStart + pastedText.Length, textBox.Text.Length);
        return true;
    }
}
