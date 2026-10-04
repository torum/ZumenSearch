using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

// TODO: Consider creating a wrapper just like Lessor wrapper?
// Almost dupe of Models.Rent.Residentials.ListingPdf
// ParentViewModel and type Dictionary are the main difference. Consider if we can reuse this.


public sealed class ResidentialPdfType(EnumResidentialPdfType key)
{
    private Dictionary<EnumResidentialPdfType, string> BuildingPdfTypeDictionary
    {
        get;
    } = new Dictionary<EnumResidentialPdfType, string>()
    {
                {EnumResidentialPdfType.Unspecified, "未指定"},
                {EnumResidentialPdfType.Maisoku, "募集図面"},
                {EnumResidentialPdfType.Architectural, "建築図面"},
                {EnumResidentialPdfType.Toukibo, "登記簿謄本"},
                {EnumResidentialPdfType.Kouzu, "公図・地図"},
                {EnumResidentialPdfType.Other, "その他"},
    };

    public string Label => BuildingPdfTypeDictionary[Key];

    public EnumResidentialPdfType Key => key;
};

public sealed partial class PropertyPdf : PdfBase
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<ResidentialPdfType> BuildingPdfTypes =
        [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new ResidentialPdfType(Models.Rent.Residentials.EnumResidentialPdfType.Maisoku),
        new ResidentialPdfType(Models.Rent.Residentials.EnumResidentialPdfType.Architectural),
        new ResidentialPdfType(Models.Rent.Residentials.EnumResidentialPdfType.Toukibo),
        new ResidentialPdfType(Models.Rent.Residentials.EnumResidentialPdfType.Kouzu),
        new ResidentialPdfType(Models.Rent.Residentials.EnumResidentialPdfType.Other)
        ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ResidentialPdfType PdfType
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
    } = new(EnumResidentialPdfType.Unspecified);

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

    public EnumResidentialPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<EnumResidentialPdfType>(Str, out var result))
        {
            PdfType = BuildingPdfTypes.FirstOrDefault<ResidentialPdfType>(x => x.Key == result) ?? new(EnumResidentialPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(EnumResidentialPdfType.Unspecified);

            return EnumResidentialPdfType.Unspecified;
        }
    }
};
