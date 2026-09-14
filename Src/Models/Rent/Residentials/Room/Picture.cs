using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Room;

public enum EnumRoomPictureType
{
    Unspecified, Madori, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Other
    //Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
}

public sealed class RoomPictureType(EnumRoomPictureType key)
{
    private Dictionary<EnumRoomPictureType, string> RoomPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumRoomPictureType, string>()
    {
                {EnumRoomPictureType.Unspecified, "未指定"},
                {EnumRoomPictureType.Madori, "間取り図"},
                //{EnumUnitPictureType.Gaikan, "外観"},
                {EnumRoomPictureType.Situnai, "室内"},
                {EnumRoomPictureType.LivingDining, "リビング・ダイニング"},
                {EnumRoomPictureType.Bedroom, "寝室"},
                {EnumRoomPictureType.Kitchen, "キッチン"},
                {EnumRoomPictureType.Bathroom, "浴室"},
                {EnumRoomPictureType.Restroom, "トイレ"},
                {EnumRoomPictureType.Washroom, "洗面"},
                {EnumRoomPictureType.StorageSpace, "収納"},
                {EnumRoomPictureType.Appliance, "設備"},
                {EnumRoomPictureType.FrontDoor, "玄関"},
                {EnumRoomPictureType.Balcony, "バルコニー"},
                //{EnumUnitPictureType.Entrance, "エントランス"},
                //{EnumUnitPictureType.Neighborhood, "周辺"},
                {EnumRoomPictureType.Other, "その他"},
            };

    public string Label => RoomPictureTypeDictionary[Key];

    public EnumRoomPictureType Key => key;
};

public sealed partial class Picture : PictureBase
{
    public ViewModels.Rent.Residentials.Room.ListingViewModel? ParentViewModel { get; set; }

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public RoomPictureType PictureType
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
                //field = new(EnumUnitPictureType.Unspecified);
            }

            OnPropertyChanged();
        }
    } = new(EnumRoomPictureType.Unspecified);

    public readonly ObservableCollection<RoomPictureType> RoomPictureTypes =
    [
        new RoomPictureType(EnumRoomPictureType.Unspecified),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Madori),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Gaikan),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Situnai),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.LivingDining),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Bedroom),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Kitchen),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Bathroom),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Restroom),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Washroom),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.StorageSpace),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Appliance),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.FrontDoor),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Balcony),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Entrance),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Neighborhood),
        new RoomPictureType(Models.Rent.Residentials.Room.EnumRoomPictureType.Other)
    ];

    // Do not use SetProperty.
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

    public Picture(string id, string imageLocation) : base(id)
    {
        ImageLocation = imageLocation;

        IsModified = false;
    }

    public EnumRoomPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumRoomPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from BuildingPictureTypes.
            PictureType = RoomPictureTypes.FirstOrDefault<RoomPictureType>(x => x.Key == result) ?? new(EnumRoomPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumRoomPictureType.Unspecified);

            return EnumRoomPictureType.Unspecified;
        }
    }
};
