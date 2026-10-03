using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumResidentialPictureType
{
    //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
    Unspecified, Gaikan, Entrance, Neighborhood, Other
}

public sealed class ResidentialPictureType(EnumResidentialPictureType key)
{
    private Dictionary<EnumResidentialPictureType, string> ResidentialPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumResidentialPictureType, string>()
    {
                {EnumResidentialPictureType.Unspecified, "未指定"},
                //{EnumResidentialPictureType.Madori, "間取り図"},
                {EnumResidentialPictureType.Gaikan, "外観"},
                //{EnumResidentialPictureType.Situnai, "室内"},
                //{EnumResidentialPictureType.LivingDining, "リビング・ダイニング"},
                //{EnumResidentialPictureType.Bedroom, "寝室"},
                //{EnumResidentialPictureType.Kitchen, "キッチン"},
                //{EnumResidentialPictureType.Bathroom, "浴室"},
                //{EnumResidentialPictureType.Restroom, "トイレ"},
                //{EnumResidentialPictureType.Washroom, "洗面"},
                //{EnumResidentialPictureType.StorageSpace, "収納"},
                //{EnumResidentialPictureType.Appliance, "設備"},
                //{EnumResidentialPictureType.Balcony, "バルコニー"},
                //{EnumResidentialPictureType.FrontDoor, "玄関"},
                {EnumResidentialPictureType.Entrance, "エントランス"},
                {EnumResidentialPictureType.Neighborhood, "周辺"},
                {EnumResidentialPictureType.Other, "その他"},
            };

    public string Label => ResidentialPictureTypeDictionary[Key];

    public EnumResidentialPictureType Key => key;
};

public sealed partial class Picture : PictureBase
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<ResidentialPictureType> ResidentialPictureTypes =
    [
        //new ResidentialPictureType(EnumResidentialPictureType.Unspecified, "未指定"),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Madori),
        new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Gaikan),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Situnai),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.LivingDining),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Bedroom),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Kitchen),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Bathroom),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Restroom),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Washroom),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.StorageSpace),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Appliance),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.FrontDoor),
        //new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Balcony),
        new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Entrance),
        new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Neighborhood),
        new ResidentialPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ResidentialPictureType PictureType
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
    } = new(EnumResidentialPictureType.Unspecified);

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

    public Picture(string id, string imageLocation, EnumEntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public EnumResidentialPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumResidentialPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from ResidentialPictureTypes.
            PictureType = ResidentialPictureTypes.FirstOrDefault<ResidentialPictureType>(x => x.Key == result) ?? new(EnumResidentialPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumResidentialPictureType.Unspecified);

            return EnumResidentialPictureType.Unspecified;
        }
    }
};
