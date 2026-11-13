using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingPictureType
{
    Unspecified, Interior, Entrance, Equipment, Other
}

public sealed class ListingPictureTypeLabel(ListingPictureType key)
{
    public ListingPictureType Key => key;
    public string Label => Key switch
    {
        ListingPictureType.Unspecified => "未指定",
        ListingPictureType.Interior => "室内",
        ListingPictureType.Entrance => "入口",
        ListingPictureType.Equipment => "設備",
        ListingPictureType.Other => "その他",
        _ => throw new ArgumentOutOfRangeException(nameof(Key))
    };
}

public sealed class ListingPicture : PictureBase
{
    public ListingPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;
        PictureType = ListingPictureTypes[0];
        SetIsModified(false);
    }

    public ViewModels.Rent.Commercials.ListingViewModel? ParentViewModel { get; set; }
    public ObservableCollection<ListingPictureTypeLabel> ListingPictureTypes { get; } =
    [
        new(ListingPictureType.Unspecified),
        new(ListingPictureType.Interior),
        new(ListingPictureType.Entrance),
        new(ListingPictureType.Equipment),
        new(ListingPictureType.Other)
    ];

    public ListingPictureTypeLabel PictureType
    {
        get;
        set
        {
            if (value is not null && field != value)
            {
                field = value;
                IsModified = true;
                OnPropertyChanged();
            }
        }
    } = new(ListingPictureType.Unspecified);

    public string Description
    {
        get;
        set
        {
            var description = value ?? string.Empty;
            if (field != description)
            {
                field = description;
                IsModified = true;
                OnPropertyChanged();
            }
        }
    } = string.Empty;

    public bool IsMain
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                IsModified = true;
                OnPropertyChanged();
            }
        }
    }

    public ListingPictureType? SetLabelFromString(string value)
    {
        if (!Enum.TryParse<ListingPictureType>(value, out var type) || !Enum.IsDefined(type) || int.TryParse(value, out _))
        {
            throw new FormatException($"Invalid commercial listing picture type: {value}");
        }

        PictureType = ListingPictureTypes.First(item => item.Key == type);
        return type;
    }

}
