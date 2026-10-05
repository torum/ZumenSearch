using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed partial class ListingPicture : PictureBase
{
    public ViewModels.Rent.Residentials.ListingViewModel? ParentViewModel { get; set; }

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ListingPictureType PictureType
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
    } = new(EnumListingPictureType.Unspecified);

    public readonly ObservableCollection<ListingPictureType> RoomPictureTypes =
    [
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Unspecified),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Madori),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Gaikan),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Situnai),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.LivingDining),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Bedroom),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Kitchen),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Bathroom),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Restroom),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Washroom),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.StorageSpace),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Appliance),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.FrontDoor),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Balcony),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Entrance),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Neighborhood),
        new ListingPictureType(Models.Rent.Residentials.EnumListingPictureType.Other)
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

    public ListingPicture(string id, string imageLocation, EnumEntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public EnumListingPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumListingPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from BuildingPictureTypes.
            PictureType = RoomPictureTypes.FirstOrDefault<ListingPictureType>(x => x.Key == result) ?? new(EnumListingPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumListingPictureType.Unspecified);

            return EnumListingPictureType.Unspecified;
        }
    }
};
