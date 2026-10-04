using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public sealed partial class PropertyPicture : PictureBase
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<PropertyPictureType> ResidentialPictureTypes =
    [
        //new PropertyPictureType(EnumResidentialPictureType.Unspecified, "未指定"),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Madori),
        new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Gaikan),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Situnai),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.LivingDining),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Bedroom),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Kitchen),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Bathroom),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Restroom),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Washroom),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.StorageSpace),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Appliance),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.FrontDoor),
        //new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Balcony),
        new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Entrance),
        new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Neighborhood),
        new PropertyPictureType(Models.Rent.Residentials.EnumResidentialPictureType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public PropertyPictureType PictureType
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

    public PropertyPicture(string id, string imageLocation, EnumEntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public EnumResidentialPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumResidentialPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from ResidentialPictureTypes.
            PictureType = ResidentialPictureTypes.FirstOrDefault<PropertyPictureType>(x => x.Key == result) ?? new(EnumResidentialPictureType.Unspecified);

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
