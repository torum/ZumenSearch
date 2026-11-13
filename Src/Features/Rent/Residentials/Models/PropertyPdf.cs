using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed partial class PropertyPdf : PdfBase
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ParentViewModel { get; set; }

    public ObservableCollection<PropertyPdfTypeLabel> BuildingPdfTypes { get;} =
        [
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Unspecified),
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Maisoku),
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Architectural),
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Toukibo),
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Kouzu),
        new PropertyPdfTypeLabel(Models.Rent.Residentials.PropertyPdfType.Other)
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
        PdfType = BuildingPdfTypes[0];

        IsModified = false;
    }

    public PropertyPdfType? SetTypeFromString(string Str)
    {
        if (!Enum.TryParse<PropertyPdfType>(Str, out var result) || !Enum.IsDefined(result))
        {
            throw new FormatException($"Invalid residential property PDF type: {Str}");
        }

        PdfType = BuildingPdfTypes.First(item => item.Key == result);
        return result;
    }
};
