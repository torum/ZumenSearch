using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

// TODO: Consider creating a wrapper just like Lessor wrapper?
// Almost dupe with Models.Rent.Residentials.ListingPdf


public sealed class CommercialBuildingPdfType(EnumCommercialPdfType key)
{
    private static readonly Dictionary<EnumCommercialPdfType, string> pdfTypes = new()
    {
        [EnumCommercialPdfType.Unspecified] = "未指定",
        [EnumCommercialPdfType.Listing] = "募集図面",
        [EnumCommercialPdfType.Architectural] = "建築図面",
        [EnumCommercialPdfType.Registry] = "登記簿謄本",
        [EnumCommercialPdfType.Map] = "公図・地図",
        [EnumCommercialPdfType.Other] = "その他"
    };
    private static readonly Dictionary<EnumCommercialPdfType, string> Labels = pdfTypes;

    public EnumCommercialPdfType Key => key;

    public string Label => Labels[key];
}

public sealed partial class PropertyPdf : PdfBase
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ParentViewModel { get; set; }

    public ObservableCollection<CommercialBuildingPdfType> BuildingPdfTypes { get; } =
    [
        new(EnumCommercialPdfType.Listing),
        new(EnumCommercialPdfType.Architectural),
        new(EnumCommercialPdfType.Registry),
        new(EnumCommercialPdfType.Map),
        new(EnumCommercialPdfType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public CommercialBuildingPdfType PdfType
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                // DO NOT DO THIS. This raize unneccesary property changed events that triggers IsDirty.
                //field = new(EnumBuildingPdfType.Unspecified);
            }

            OnPropertyChanged();
        }
    } = new(EnumCommercialPdfType.Unspecified);

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public string Description
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                field = string.Empty;
            }

            OnPropertyChanged();
        }
    } = string.Empty;

    // Do not use SetProperty.
    public bool IsMain
    {
        get;
        set
        {
            if (field == value) return;

            // Set this before raize PropertyChanged.
            IsModified = true;

            // Raize PropertyChanged event here.
            field = value;

            OnPropertyChanged();
        }
    }

    public PropertyPdf(string id, string pdfLocation, string thumbnailLocation, EnumEntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;

        IsModified = false;
    }

    public EnumCommercialPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<EnumCommercialPdfType>(Str, out var result))
        {
            PdfType = BuildingPdfTypes.FirstOrDefault<CommercialBuildingPdfType>(x => x.Key == result) ?? new(EnumCommercialPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(EnumCommercialPdfType.Unspecified);

            return EnumCommercialPdfType.Unspecified;
        }
    }
};
