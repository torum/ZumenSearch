using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public sealed partial class PropertyPicture : PictureBase
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ParentViewModel { get; set; }

    public ObservableCollection<PropertyPictureTypeLabel> CommercialPictureTypes { get;} =
    [
        new(PropertyPictureType.Unspecified),
        new(PropertyPictureType.Exterior),
        new(PropertyPictureType.Entrance),
        new(PropertyPictureType.Neighborhood),
        new(PropertyPictureType.Interior),
        new(PropertyPictureType.Other)
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

    public PropertyPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;
        PictureType = CommercialPictureTypes[0];

        IsModified = false;
    }

    public PropertyPictureType? SetLabelFromString(string titleStr)
    {
        if (!Enum.TryParse<PropertyPictureType>(titleStr, out var result) || !Enum.IsDefined(result))
        {
            throw new FormatException($"Invalid commercial property picture type: {titleStr}");
        }

        PictureType = CommercialPictureTypes.First(item => item.Key == result);
        return result;
    }
};
