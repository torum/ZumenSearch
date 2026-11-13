using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed class ListingPdf : PdfBase
{
    public ListingPdf(string id, string pdfFilename, string thumbnailFilename, EntityStatus status) : base(id, status)
    {
        PdfFilename = pdfFilename;
        ThumbnailFilename = thumbnailFilename;
        SetPdfTypeLabel(ListingPdfTypes[0]);
        SetIsModified(false);
    }

    public ViewModels.Rent.Commercials.ListingViewModel? ParentViewModel { get; set; }
    public ObservableCollection<ListingPdfTypeLabel> ListingPdfTypes { get; } =
    [
        new(ListingPdfType.Unspecified),
        new(ListingPdfType.Listing),
        new(ListingPdfType.FloorPlan),
        new(ListingPdfType.Other)
    ];

    public ListingPdfTypeLabel PdfType
    {
        get;
        set
        {
            if (value is null || field == value) return;
            field = value;
            IsModified = true;
            OnPropertyChanged();
        }
    } = new(ListingPdfType.Unspecified);

    public string Description
    {
        get;
        set
        {
            var description = value ?? string.Empty;
            if (field == description) return;
            field = description;
            IsModified = true;
            OnPropertyChanged();
        }
    } = string.Empty;

    public bool IsMain
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
            OnPropertyChanged();
        }
    }

    public void SetPdfTypeLabel(ListingPdfTypeLabel value)
    {
        PdfType = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void SetPdfTypeLabelFromString(string value)
    {
        if (!Enum.TryParse<ListingPdfType>(value, out var type) || !Enum.IsDefined(type) || int.TryParse(value, out _))
        {
            throw new FormatException($"Invalid commercial listing PDF type: {value}");
        }

        SetPdfTypeLabel(ListingPdfTypes.First(item => item.Key == type));
    }

}
