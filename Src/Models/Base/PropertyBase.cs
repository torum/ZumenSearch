using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ZumenSearch.Models.Enums;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Transportation;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

namespace ZumenSearch.Models.Base;

// Aggregate Root entity. Property Listing Context.

// <summary>
// Base class for all Aggregate Root entities : properties. Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract partial class PropertyBase : EntityAggregateBase
{
    protected PropertyBase(string id, EntityStatus status, PropertyKind kind) : base(id, status)
    {
        PropertyKind = kind;
    }

    public PropertyKind PropertyKind { get; init; }

    public Address Address
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Address();

    public Train Train1
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public Train Train2
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public Train Train3
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public Train Train4
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();



    // TODO: Create a nested class for ThumbImage so that BasePath is always set along with ThumbnailFilename as parameters.
    #region == Thumb Image ==

    public string BasePath { get; set; } = string.Empty;

    public string ThumbnailFilename
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    // Automatically generate a thumbnail image when the property is accessed, if it hasn't been set yet.
    public ImageSource? ThumbImage 
    {
        get => field ?? CreateThumb();
        set; 
    }

    private BitmapImage? CreateThumb()
    {
        if (string.IsNullOrEmpty(ThumbnailFilename))
        {
            //Debug.WriteLine("ThumbnailImageFilePath is empty. (PropertyBase)");
            return null;
        }

        if (string.IsNullOrEmpty(BasePath))
        {
            Debug.WriteLine("BasePath is empty. (PropertyBase)");
            return null;
        }

        var thumbFilemame = System.IO.Path.Combine(BasePath, ThumbnailFilename);

        if (!Path.Exists(thumbFilemame))
        {
            Debug.WriteLine($"File ThumbnailImageFilePath does not exists. (PropertyBase) {thumbFilemame}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        
        Uri uri = new(thumbFilemame);
        bitmapImage.UriSource = uri;

        return bitmapImage;
    }

    #endregion

    #region == Public Methods ==

    // TODO: Implement similar setter methods for Train1, Train2, Train3, Train4 .

    #endregion

}


