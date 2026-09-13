using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class PictureBase : ObservableObject
{
    public string ImageLocation
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
        if (string.IsNullOrEmpty(ImageLocation))
        {
            Debug.WriteLine("ImageLocation is empty. (PictureBase)");
            return null;
        }

        if (!Path.Exists(ImageLocation))
        {
            Debug.WriteLine($"File ImageLocation does not exists. (PictureBase) {ImageLocation}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        Uri uri = new(ImageLocation);
        bitmapImage.UriSource = uri;
        return bitmapImage;
    }

    public string Id { get; } = string.Empty;

    public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected PictureBase(string id)
    {
        Id = id;
    }
};
