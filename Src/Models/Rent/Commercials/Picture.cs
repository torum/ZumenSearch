using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumCommercialPictureType
{
    Unspecified,
    Exterior,
    Entrance,
    Neighborhood,
    Interior,
    Other
}

public sealed class CommercialPictureType(EnumCommercialPictureType key)
{
    private static readonly IReadOnlyDictionary<EnumCommercialPictureType, string> Labels =
        new Dictionary<EnumCommercialPictureType, string>
        {
            [EnumCommercialPictureType.Unspecified] = "未指定",
            [EnumCommercialPictureType.Exterior] = "外観",
            [EnumCommercialPictureType.Entrance] = "エントランス",
            [EnumCommercialPictureType.Neighborhood] = "周辺",
            [EnumCommercialPictureType.Interior] = "室内",
            [EnumCommercialPictureType.Other] = "その他"
        };

    public EnumCommercialPictureType Key => key;

    public string Label => Labels[key];
}



public sealed partial class Picture : PictureBase
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<CommercialPictureType> CommercialPictureTypes =
    [
        new(EnumCommercialPictureType.Exterior),
        new(EnumCommercialPictureType.Entrance),
        new(EnumCommercialPictureType.Neighborhood),
        new(EnumCommercialPictureType.Interior),
        new(EnumCommercialPictureType.Other)
    ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public CommercialPictureType PictureType
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
    } = new(EnumCommercialPictureType.Unspecified);

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

    public EnumCommercialPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumCommercialPictureType>(titleStr, out var result))
        {
            //PictureType = new(result); // Not good for assigning to combobox. So select from CommercialPictureTypes.
            PictureType = CommercialPictureTypes.FirstOrDefault<CommercialPictureType>(x => x.Key == result) ?? new(EnumCommercialPictureType.Unspecified);

            //Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Label}");
            return result;
        }
        else
        {
            PictureType = new(EnumCommercialPictureType.Unspecified);

            return EnumCommercialPictureType.Unspecified;
        }
    }
};
