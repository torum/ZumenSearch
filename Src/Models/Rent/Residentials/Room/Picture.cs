using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Room;

public enum EnumUnitPictureType
{
    Unspecified, Madori, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Other
    //Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
}

public sealed class UnitPictureType(EnumUnitPictureType key)
{
    private Dictionary<EnumUnitPictureType, string> UnitPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumUnitPictureType, string>()
    {
                {EnumUnitPictureType.Unspecified, "未指定"},
                {EnumUnitPictureType.Madori, "間取り図"},
                //{EnumUnitPictureType.Gaikan, "外観"},
                {EnumUnitPictureType.Situnai, "室内"},
                {EnumUnitPictureType.LivingDining, "リビング・ダイニング"},
                {EnumUnitPictureType.Bedroom, "寝室"},
                {EnumUnitPictureType.Kitchen, "キッチン"},
                {EnumUnitPictureType.Bathroom, "浴室"},
                {EnumUnitPictureType.Restroom, "トイレ"},
                {EnumUnitPictureType.Washroom, "洗面"},
                {EnumUnitPictureType.StorageSpace, "収納"},
                {EnumUnitPictureType.Appliance, "設備"},
                {EnumUnitPictureType.FrontDoor, "玄関"},
                {EnumUnitPictureType.Balcony, "バルコニー"},
                //{EnumUnitPictureType.Entrance, "エントランス"},
                //{EnumUnitPictureType.Neighborhood, "周辺"},
                {EnumUnitPictureType.Other, "その他"},
            };

    public string Label => UnitPictureTypeDictionary[Key];

    public EnumUnitPictureType Key => key;
};

public sealed partial class Picture : PictureBase
{
    public ViewModels.Rent.Residentials.Room.ListingViewModel? ParentViewModel { get; set; }

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public UnitPictureType PictureType
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
    } = new(EnumUnitPictureType.Unspecified);

    public readonly ObservableCollection<UnitPictureType> UnitPictureTypes =
    [
        new UnitPictureType(EnumUnitPictureType.Unspecified),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Madori),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Gaikan),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Situnai),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.LivingDining),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Bedroom),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Kitchen),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Bathroom),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Restroom),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Washroom),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.StorageSpace),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Appliance),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.FrontDoor),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Balcony),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Entrance),
        //new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Neighborhood),
        new UnitPictureType(Models.Rent.Residentials.Room.EnumUnitPictureType.Other)
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

    public EnumUnitPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumUnitPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from BuildingPictureTypes.
            PictureType = UnitPictureTypes.FirstOrDefault<UnitPictureType>(x => x.Key == result) ?? new(EnumUnitPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumUnitPictureType.Unspecified);

            return EnumUnitPictureType.Unspecified;
        }
    }
};
