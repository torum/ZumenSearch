using System.Collections.ObjectModel;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Presentation.Features.Rent.Residentials.Models;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Rent.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public sealed partial class PropertyPicture : PictureBase
{
    public PropertyPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;

        IsModified = false;
    }

    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel? ParentViewModel { get; set; }

    public ObservableCollection<PropertyPictureTypeLabel> ResidentialPictureTypes { get; } =
    [
        new PropertyPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyPictureType.Unspecified),
        new PropertyPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyPictureType.Gaikan),
        new PropertyPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyPictureType.Entrance),
        new PropertyPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyPictureType.Neighborhood),
        new PropertyPictureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyPictureType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public PropertyPictureTypeLabel PictureType
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
    } = new(PropertyPictureType.Unspecified);

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

    public PropertyPictureType SetTypeFromString(string Str)
    {
        if (!Enum.TryParse<PropertyPictureType>(Str, out var result) || !Enum.IsDefined(result))
        {
            //throw new FormatException($"Invalid residential property Picture type: {Str}");
            Debug.WriteLine($"Invalid residential property Picture type: {Str}. Defaulting to Unspecified.");
            PictureType = ResidentialPictureTypes.First(item => item.Key == PropertyPictureType.Unspecified);
            return PropertyPictureType.Unspecified;
        }

        PictureType = ResidentialPictureTypes.First(item => item.Key == result);
        return result;
    }

    public PropertyPicture Clone()
    {
        var clone = new PropertyPicture(Id, ImageFilename, Status)
        {
            BasePath = this.BasePath,
            ThumbImagePath = this.ThumbImagePath,
            Description = this.Description,
            PictureType = ResidentialPictureTypes.FirstOrDefault(item => item.Key == this.PictureType.Key) ?? new PropertyPictureTypeLabel(this.PictureType.Key),
            IsMain = this.IsMain
        };

        //clone.PictureType = ResidentialPictureTypes.FirstOrDefault(item => item.Key == this.PictureType.Key) ?? new PropertyPictureTypeLabel(this.PictureType.Key);
        clone.SetIsModified(IsModified);
        return clone;
    }
};
