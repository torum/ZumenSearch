using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Listing;

// TODO: This is same as ZumenSearch.Models.Rent.Residentials.Pdf.
// Consider removable.
// No... We need ParentViewModel(ViewModels.Rent.Residentials.Listing.ListingViewModel)
// Then, consider creating a wrapper just like Lessor wrapper?
// Consider if we can reuse this for Sales 

public enum EnumRoomPdfType
{
    Unspecified,
    Maisoku,
    Architectural,
    Toukibo,
    Kouzu,
    Other
}
public sealed class RoomPdfType(EnumRoomPdfType key)
{
    private Dictionary<EnumRoomPdfType, string> RoomPdfTypeDictionary
    {
        get;
    } = new Dictionary<EnumRoomPdfType, string>()
    {
                {EnumRoomPdfType.Unspecified, "未指定"},
                {EnumRoomPdfType.Maisoku, "募集図面"},
                {EnumRoomPdfType.Architectural, "建築図面"},
                {EnumRoomPdfType.Toukibo, "登記簿謄本"},
                {EnumRoomPdfType.Kouzu, "公図・地図"},
                {EnumRoomPdfType.Other, "その他"},
    };

    public string Label => RoomPdfTypeDictionary[Key];

    public EnumRoomPdfType Key => key;
};

public sealed partial class Pdf : PdfBase
{
    public ViewModels.Rent.Residentials.Listing.ListingViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<RoomPdfType> RoomPdfTypes =
        [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new RoomPdfType(Models.Rent.Residentials.Listing.EnumRoomPdfType.Maisoku),
        new RoomPdfType(Models.Rent.Residentials.Listing.EnumRoomPdfType.Architectural),
        new RoomPdfType(Models.Rent.Residentials.Listing.EnumRoomPdfType.Toukibo),
        new RoomPdfType(Models.Rent.Residentials.Listing.EnumRoomPdfType.Kouzu),
        new RoomPdfType(Models.Rent.Residentials.Listing.EnumRoomPdfType.Other)
        ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public RoomPdfType PdfType
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
    } = new(EnumRoomPdfType.Unspecified);

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

    public Pdf(string id, string pdfLocation, string thumbnailLocation) : base(id)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;

        IsModified = false;
    }

    public EnumRoomPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<EnumRoomPdfType>(Str, out var result))
        {
            PdfType = RoomPdfTypes.FirstOrDefault<RoomPdfType>(x => x.Key == result) ?? new(EnumRoomPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(EnumRoomPdfType.Unspecified);

            return EnumRoomPdfType.Unspecified;
        }
    }
};
