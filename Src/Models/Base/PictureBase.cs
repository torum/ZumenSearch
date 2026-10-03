using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class PictureBase : EntityBase
{
    protected PictureBase(string id, EnumEntityStatus staus) : base(id, staus)
    {
        //
    }

    //public string Id { get; } = string.Empty;

    //public bool IsNew { get; set; } = true;

    //public bool IsModified { get; set; } = false;

    public string ImageFilename
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

    public ImageSource? ThumbImage
    {
        get => field ?? CreateThumb();
        set;
    }

    private BitmapImage? CreateThumb()
    {
        if (string.IsNullOrEmpty(ImageFilename))
        {
            Debug.WriteLine("ImageFilename is empty. (PictureBase)");
            return null;
        }

        if (string.IsNullOrEmpty(BasePath))
        {
            Debug.WriteLine("BasePath is empty. (PictureBase)");
            return null;
        }

        var imageFilePath = System.IO.Path.Combine(BasePath, ImageFilename);

        if (!Path.Exists(imageFilePath))
        {
            Debug.WriteLine($"File ImageFilename does not exists. (PictureBase) {imageFilePath}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        Uri uri = new(imageFilePath);
        bitmapImage.UriSource = uri;
        return bitmapImage;
    }

};
