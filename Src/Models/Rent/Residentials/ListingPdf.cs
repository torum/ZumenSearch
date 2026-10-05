using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Rent.Residentials;

// TODO: This is same as ZumenSearch.Models.Rent.Residentials.Pdf.
// Consider removable.
// No... We need ParentViewModel(ViewModels.Rent.Residentials.ListingViewModel)
// Then, consider creating a wrapper just like Lessor wrapper?
// ParentViewModel and type Dictionary are the main difference.

public sealed partial class ListingPdf : PdfBase
{
    public ViewModels.Rent.Residentials.ListingViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<ListingPdfType> RoomPdfTypes =
        [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new ListingPdfType(Models.Rent.Residentials.EnumListingPdfType.Maisoku),
        new ListingPdfType(Models.Rent.Residentials.EnumListingPdfType.Architectural),
        new ListingPdfType(Models.Rent.Residentials.EnumListingPdfType.Toukibo),
        new ListingPdfType(Models.Rent.Residentials.EnumListingPdfType.Kouzu),
        new ListingPdfType(Models.Rent.Residentials.EnumListingPdfType.Other)
        ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ListingPdfType PdfType
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
    } = new(EnumListingPdfType.Unspecified);

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

    public ListingPdf(string id, string pdfLocation, string thumbnailLocation, EnumEntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;

        IsModified = false;
    }

    public EnumListingPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<EnumListingPdfType>(Str, out var result))
        {
            PdfType = RoomPdfTypes.FirstOrDefault<ListingPdfType>(x => x.Key == result) ?? new(EnumListingPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(EnumListingPdfType.Unspecified);

            return EnumListingPdfType.Unspecified;
        }
    }
};
