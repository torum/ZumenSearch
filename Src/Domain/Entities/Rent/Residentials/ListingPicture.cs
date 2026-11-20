using System.Collections.ObjectModel;
using System.Data;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Presentation.Features.Rent.Residentials.Models;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Rent.Residentials;

public sealed partial class ListingPicture : PictureBase
{
    public ListingPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel? ParentViewModel { get; set; }

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
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Unspecified),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Madori),
        //new UnitPictureType(ZumenSearch.Presentation.Features.Rent.Residentials.Models.EnumUnitPictureType.Gaikan),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Situnai),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.LivingDining),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Bedroom),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Kitchen),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Bathroom),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Restroom),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Washroom),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.StorageSpace),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Appliance),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.FrontDoor),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Balcony),
        //new UnitPictureType(ZumenSearch.Presentation.Features.Rent.Residentials.Models.EnumUnitPictureType.Entrance),
        //new UnitPictureType(ZumenSearch.Presentation.Features.Rent.Residentials.Models.EnumUnitPictureType.Neighborhood),
        new ListingPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingPictureType.Other)
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
