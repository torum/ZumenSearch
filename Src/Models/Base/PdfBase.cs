using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class PdfBase : ObservableObject
{
    public string PdfLocation
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

    public string ThumbnailLocation
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
        if (string.IsNullOrEmpty(ThumbnailLocation))
        {
            Debug.WriteLine("ThumbnailLocation is empty. (PdfBase)");
            return null;
        }

        if (!Path.Exists(ThumbnailLocation))
        {
            Debug.WriteLine($"File ThumbnailLocation does not exists. (PdfBase) {ThumbnailLocation}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        Uri uri = new(ThumbnailLocation);
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
