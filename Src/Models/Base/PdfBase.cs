using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Base;

// <summary>
// Base class for all PDF entities. Aggregate Root entities are property and listing.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class PdfBase : EntityBase
{
    protected PdfBase(string id, EntityStatus staus) : base(id, staus)
    {
        //
    }

    public string PdfFilename
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public string ThumbnailFilename
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public ImageSource? ThumbImage
    {
        get => field ?? CreateThumb();
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
};
