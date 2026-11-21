namespace ZumenSearch.Presentation.Helpers;

internal static class ImageFileHelper
{
    public static bool IsSupportedImageExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
}
