using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed partial class ListingPdf : PdfBase
{
    public ListingPdf(string id, string pdfLocation, string thumbnailLocation, EntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;
        //PdfType = ListingPdfTypes[0];
        PdfType = ListingPdfTypes.First(item => item.Key == ListingPdfType.Unspecified);

        IsModified = false;
    }

    public ViewModels.Rent.Residentials.ListingViewModel? ParentViewModel { get; set; }

    public ObservableCollection<ListingPdfTypeLabel> ListingPdfTypes { get; set; } =
        [
        new ListingPdfTypeLabel(Models.Rent.Residentials.ListingPdfType.Unspecified),
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

    public ListingPdfType SetTypeFromString(string Str)
    {
        if (!Enum.TryParse<ListingPdfType>(Str, out var result) || !Enum.IsDefined(result))
        {
            //throw new FormatException($"Invalid residential listing PDF type: {Str}");
            Debug.WriteLine($"Invalid residential listing PDF type: {Str}. Defaulting to Unspecified.");
            PdfType = ListingPdfTypes.First(item => item.Key == ListingPdfType.Unspecified);
            return ListingPdfType.Unspecified;
        }

        PdfType = ListingPdfTypes.First(item => item.Key == result);
        return result;
    }

    public ListingPdf Clone()
    {
        var clone = new ListingPdf(Id, PdfFilename, ThumbnailFilename, Status)
        {
            BasePath = BasePath,
            Description = Description,
            IsMain = IsMain
        };

        clone.PdfType = clone.ListingPdfTypes.FirstOrDefault(item => item.Key == PdfType.Key) ?? new ListingPdfTypeLabel(PdfType.Key);
        clone.SetIsModified(IsModified);
        return clone;
    }

};
