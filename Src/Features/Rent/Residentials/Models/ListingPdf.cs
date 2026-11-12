using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

// TODO: This is same as ZumenSearch.Models.Rent.Residentials.Pdf.
// Consider removable.
// No... We need ParentViewModel(ViewModels.Rent.Residentials.ListingViewModel)
// Then, consider creating a wrapper just like Lessor wrapper?
// ParentViewModel and type Dictionary are the main difference.

public sealed partial class ListingPdf : PdfBase
{
    public ViewModels.Rent.Residentials.ListingViewModel? ParentViewModel { get; set; }

    public ObservableCollection<ListingPdfTypeLabel> RoomPdfTypes { get; set; } =
        [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Maisoku),
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Architectural),
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Toukibo),
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Kouzu),
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Other)
        ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ListingPdfTypeLabel PdfType
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
    } = new(ListingPdfType.Unspecified);

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

    public ListingPdf(string id, string pdfLocation, string thumbnailLocation, EntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;

        IsModified = false;
    }

    public ListingPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<ListingPdfType>(Str, out var result))
        {
            PdfType = RoomPdfTypes.FirstOrDefault<ListingPdfTypeLabel>(x => x.Key == result) ?? new(ListingPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(ListingPdfType.Unspecified);

            return ListingPdfType.Unspecified;
        }
    }
};
