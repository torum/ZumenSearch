using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

// Aggregate Root entity. Property Listing Context.

// <summary>
// Base class for all Aggregate Root entities : properties. Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract partial class PropertyBase : EntityAggregateBase
{
    protected PropertyBase(string id, EnumEntityStatus status, EnumPropertyKind kind) : base(id, status)
    {
        PropertyKind = kind;
    }

    public EnumPropertyKind PropertyKind { get; init; }

    // TODO: Use Address class from Models.Location.Address.cs instead of using Prefecture, CountyAndCity, WardAndOaza, Choume classes directly.
    public AddressClass Address
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new AddressClass();

    #region == 所在地 ==

    public string LocPrefId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocPrefecture
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocMachiazaId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCounty
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCity
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocWard
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocOazaCho
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocChoume
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocEdaban
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocLocationFull
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocationLatitude
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = string.Empty;

    public string LocationLongitude
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = string.Empty;
    #endregion

    // TODO: Use Train class from Models.Transportation.Train.cs instead of using classes directly.
    public TrainClass Train
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new TrainClass();


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

}


public enum EnumPropertyKind
{
    RentResidential,
    RentCommercial,
    RentParking,
    SaleResidential,
    SaleCommercial,
    SaleLand,
    Unknown
}