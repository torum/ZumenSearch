using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

// TODO: Consider creating a wrapper just like Lessor wrapper?
// Almost dupe with Models.Rent.Residentials.ListingPdf

public sealed partial class PropertyPdf : PdfBase
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ParentViewModel { get; set; }

    public ObservableCollection<PropertyPdfTypeLabel> BuildingPdfTypes { get; } =
    [
        new(PropertyPdfType.Listing),
        new(PropertyPdfType.Architectural),
        new(PropertyPdfType.Registry),
        new(PropertyPdfType.Map),
        new(PropertyPdfType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public PropertyPdfTypeLabel PdfType
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
    } = new(PropertyPdfType.Unspecified);

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

    public PropertyPdf(string id, string pdfLocation, string thumbnailLocation, EntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;

        IsModified = false;
    }

    public PropertyPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<PropertyPdfType>(Str, out var result))
        {
            PdfType = BuildingPdfTypes.FirstOrDefault<PropertyPdfTypeLabel>(x => x.Key == result) ?? new(PropertyPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(PropertyPdfType.Unspecified);

            return PropertyPdfType.Unspecified;
        }
    }
};
