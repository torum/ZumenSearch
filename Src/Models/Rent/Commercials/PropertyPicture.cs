using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Rent.Commercials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public sealed partial class PropertyPicture : PictureBase
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<PropertyPictureType> CommercialPictureTypes =
    [
        new(EnumPropertyPictureType.Exterior),
        new(EnumPropertyPictureType.Entrance),
        new(EnumPropertyPictureType.Neighborhood),
        new(EnumPropertyPictureType.Interior),
        new(EnumPropertyPictureType.Other)
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
    } = new(EnumPropertyPictureType.Unspecified);

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

    public EnumPropertyPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumPropertyPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from CommercialPictureTypes.
            PictureType = CommercialPictureTypes.FirstOrDefault<PropertyPictureType>(x => x.Key == result) ?? new(EnumPropertyPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumPropertyPictureType.Unspecified);

            return EnumPropertyPictureType.Unspecified;
        }
    }
};
