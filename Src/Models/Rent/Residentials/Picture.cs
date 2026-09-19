using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumBuildingPictureType
{
    //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
    Unspecified, Gaikan, Entrance, Neighborhood, Other
}

public sealed class BuildingPictureType(EnumBuildingPictureType key)
{
    private Dictionary<EnumBuildingPictureType, string> BuildingPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumBuildingPictureType, string>()
    {
                {EnumBuildingPictureType.Unspecified, "未指定"},
                //{EnumBuildingPictureType.Madori, "間取り図"},
                {EnumBuildingPictureType.Gaikan, "外観"},
                //{EnumBuildingPictureType.Situnai, "室内"},
                //{EnumBuildingPictureType.LivingDining, "リビング・ダイニング"},
                //{EnumBuildingPictureType.Bedroom, "寝室"},
                //{EnumBuildingPictureType.Kitchen, "キッチン"},
                //{EnumBuildingPictureType.Bathroom, "浴室"},
                //{EnumBuildingPictureType.Restroom, "トイレ"},
                //{EnumBuildingPictureType.Washroom, "洗面"},
                //{EnumBuildingPictureType.StorageSpace, "収納"},
                //{EnumBuildingPictureType.Appliance, "設備"},
                //{EnumBuildingPictureType.Balcony, "バルコニー"},
                //{EnumBuildingPictureType.FrontDoor, "玄関"},
                {EnumBuildingPictureType.Entrance, "エントランス"},
                {EnumBuildingPictureType.Neighborhood, "周辺"},
                {EnumBuildingPictureType.Other, "その他"},
            };

    public string Label => BuildingPictureTypeDictionary[Key];

    public EnumBuildingPictureType Key => key;
};

public sealed partial class Picture : PictureBase
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<BuildingPictureType> BuildingPictureTypes =
    [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Madori),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Gaikan),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Situnai),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.LivingDining),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Bedroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Kitchen),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Bathroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Restroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Washroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.StorageSpace),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Appliance),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.FrontDoor),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Balcony),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Entrance),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Neighborhood),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public BuildingPictureType PictureType
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
                //field = new(EnumBuildingPictureType.Unspecified);
            }

            OnPropertyChanged();
        }
    } = new(EnumBuildingPictureType.Unspecified);

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
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public EnumBuildingPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumBuildingPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from BuildingPictureTypes.
            PictureType = BuildingPictureTypes.FirstOrDefault<BuildingPictureType>(x => x.Key == result) ?? new(EnumBuildingPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumBuildingPictureType.Unspecified);

            return EnumBuildingPictureType.Unspecified;
        }
    }
};
