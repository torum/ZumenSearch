using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class PdfBase : ObservableObject
{
    public string PdfFilename
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public string ThumbnailFilename
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = string.Empty;

    public ImageSource? ThumbImage
    {
        get => field ?? CreateThumb();
        set;
    }

    private BitmapImage? CreateThumb()
    {
        if (string.IsNullOrEmpty(ThumbnailFilename))
        {
            Debug.WriteLine("ThumbnailLocation is empty. (PdfBase)");
            return null;
        }

        if (string.IsNullOrEmpty(BasePath))
        {
            Debug.WriteLine("BasePath is empty. (PdfBase)");
            return null;
        }

        var pdfFilePath = System.IO.Path.Combine(BasePath, ThumbnailFilename);

        if (!Path.Exists(pdfFilePath))
        {
            Debug.WriteLine($"File ThumbnailLocation does not exists. (PdfBase) {pdfFilePath}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        Uri uri = new(pdfFilePath);
        bitmapImage.UriSource = uri;
        return bitmapImage;
    }

    public string Id { get; } = string.Empty;

    public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected PdfBase(string id)
    {
        Id = id;
    }
};
