using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

public enum EnumPropertyStatus
{
    Saved,
    New,
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

public abstract partial class PropertyBase : ObservableObject
{
    public EnumPropertyStatus PropertyStatus { get; set; } = EnumPropertyStatus.New;

    public EnumPropertyKind PropertyKind { get; init; } = EnumPropertyKind.Unknown;

    public bool IsModified { get; set; } = false;

    protected private string _id;
    public string Id => _id;

    public string Name
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

    public ImageSource? ThumbImage 
    {
        get => field ?? CreateThumb();
        set; 
    }

    private BitmapImage? CreateThumb()
    {
        if (string.IsNullOrEmpty(ThumbnailImageFilePath))
        {
            //Debug.WriteLine("ThumbnailImageFilePath is empty. (PropertyBase)");
            return null;
        }

        if (!Path.Exists(ThumbnailImageFilePath))
        {
            Debug.WriteLine($"File ThumbnailImageFilePath does not exists. (PropertyBase) {ThumbnailImageFilePath}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        Uri uri = new(ThumbnailImageFilePath);
        bitmapImage.UriSource = uri;
        return bitmapImage;
    }

    public string ThumbnailImageFilePath
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

    #endregion

    protected PropertyBase(string id, EnumPropertyKind kind)
    {
        _id = id;
        PropertyKind = kind;
    }

    #region == Public Methods ==

    public void ClearId()
    {
        _id = string.Empty;
    }

    public void SetId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        }

        _id = id;
    }

    #endregion
}
