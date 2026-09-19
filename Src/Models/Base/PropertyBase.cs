using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

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

public abstract partial class PropertyBase : EntryBase
{
    public EnumPropertyKind PropertyKind { get; init; } = EnumPropertyKind.Unknown;

    public string BasePath { get; set; } = string.Empty;

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

    protected PropertyBase(string id, EnumEntryStatus status, EnumPropertyKind kind): base(id, status)
    {
        PropertyKind = kind;
    }

}
