using System.Collections.ObjectModel;
using System.Data;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed partial class ListingPicture : PictureBase
{
    public ViewModels.Rent.Residentials.ListingViewModel? ParentViewModel { get; set; }

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public ListingPictureTypeLabel PictureType
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
    } = new(ListingPictureType.Unspecified);

    public ObservableCollection<ListingPictureTypeLabel> ListingPictureTypes { get;} =
    [
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Unspecified),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Madori),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Gaikan),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Situnai),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.LivingDining),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Bedroom),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Kitchen),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Bathroom),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Restroom),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Washroom),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.StorageSpace),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Appliance),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.FrontDoor),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Balcony),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Entrance),
        //new UnitPictureType(Models.Rent.Residentials.EnumUnitPictureType.Neighborhood),
        new ListingPictureTypeLabel(Models.Rent.Residentials.ListingPictureType.Other)
    ];

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

    // Do not use SetProperty. PropertyChanged is being subscribed.
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

    public ListingPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public ListingPictureType SetTypeFromString(string Str)
    {
        if (!Enum.TryParse<ListingPictureType>(Str, out var result) || !Enum.IsDefined(result))
        {
            //throw new FormatException($"Invalid residential listing PDF type: {Str}");
            Debug.WriteLine($"Invalid residential listing PDF type: {Str}. Defaulting to Unspecified.");
            PictureType = ListingPictureTypes.First(item => item.Key == ListingPictureType.Unspecified);
            return ListingPictureType.Unspecified;
        }

        PictureType = ListingPictureTypes.First(item => item.Key == result);
        return result;
    }

    public ListingPicture Clone()
    {
        var clone = new ListingPicture(Id, ImageFilename, Status)
        {
            BasePath = BasePath,
            ThumbImagePath = ThumbImagePath,
            Description = Description,
            IsMain = IsMain
        };

        clone.PictureType = clone.ListingPictureTypes.FirstOrDefault(item => item.Key == PictureType.Key)
            ?? new ListingPictureTypeLabel(PictureType.Key);
        clone.SetIsModified(IsModified);
        return clone;
    }

};
