using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Bldg;

public enum EnumBuildingPdfType
{
    Unspecified,
    Maisoku,
    Architectural,
    Other
}
public sealed class BuildingPdfType(EnumBuildingPdfType key)
{
    private Dictionary<EnumBuildingPdfType, string> BuildingPdfTypeDictionary
    {
        get;
    } = new Dictionary<EnumBuildingPdfType, string>()
    {
                {EnumBuildingPdfType.Unspecified, "未指定"},
                {EnumBuildingPdfType.Maisoku, "募集図面"},
                {EnumBuildingPdfType.Architectural, "建築図面"},
                {EnumBuildingPdfType.Other, "その他"},
    };

    public string Label => BuildingPdfTypeDictionary[Key];

    public EnumBuildingPdfType Key => key;
};

public sealed partial class Pdf : PdfBase
{
    public ViewModels.Rent.Residentials.Bldg.MainViewModel? ParentViewModel { get; set; }

    public readonly ObservableCollection<BuildingPdfType> BuildingPdfTypes =
        [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new BuildingPdfType(Models.Rent.Residentials.Bldg.EnumBuildingPdfType.Maisoku),
        new BuildingPdfType(Models.Rent.Residentials.Bldg.EnumBuildingPdfType.Architectural),
        new BuildingPdfType(Models.Rent.Residentials.Bldg.EnumBuildingPdfType.Other)
        ];

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public BuildingPdfType PdfType
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
                //field = new(EnumBuildingPdfType.Unspecified);
            }

            OnPropertyChanged();
        }
    } = new(EnumBuildingPdfType.Unspecified);

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

    public Pdf(string id, string pdfLocation, string thumbnailLocation) : base(id)
    {
        PdfLocation = pdfLocation;
        ThumbnailLocation = thumbnailLocation;

        IsModified = false;
    }

    public EnumBuildingPdfType? SetTypeFromString(string Str)
    {
        if (Enum.TryParse<EnumBuildingPdfType>(Str, out var result))
        {
            PdfType = BuildingPdfTypes.FirstOrDefault<BuildingPdfType>(x => x.Key == result) ?? new(EnumBuildingPdfType.Unspecified);

            return result;
        }
        else
        {
            PdfType = new(EnumBuildingPdfType.Unspecified);

            return EnumBuildingPdfType.Unspecified;
        }
    }
};
