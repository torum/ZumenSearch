using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums.Rent.Residentials;
using ZumenSearch.Presentation.Features.Rent.Residentials.Models;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;

public sealed partial class ListingPictureViewModel : ObservableObject
{
    private readonly ListingPicture _model;
    private ListingPictureTypeLabel _pictureType;

    public ListingPictureViewModel(ListingPicture model, ListingViewModel parentViewModel)
    {
        _model = model;
        ParentViewModel = parentViewModel;
        _pictureType = ListingPictureTypes.First(item => item.Key == model.PictureType);
    }

    public ListingPicture Model => _model;
    public ListingViewModel ParentViewModel { get; }
    public string Id => _model.Id;
    public string ImageFilename => _model.ImageFilename;
    public bool IsModified => _model.IsModified;

    public string? ThumbImagePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.BasePath) || string.IsNullOrWhiteSpace(ImageFilename)) return null;
            var path = Path.Combine(_model.BasePath, ImageFilename);
            return Path.Exists(path) ? path : null;
        }
    }

    public ObservableCollection<ListingPictureTypeLabel> ListingPictureTypes { get; } =
    [
        new(ListingPictureType.Unspecified),
        new(ListingPictureType.Madori),
        new(ListingPictureType.Situnai),
        new(ListingPictureType.LivingDining),
        new(ListingPictureType.Bedroom),
        new(ListingPictureType.Kitchen),
        new(ListingPictureType.Bathroom),
        new(ListingPictureType.Restroom),
        new(ListingPictureType.Washroom),
        new(ListingPictureType.StorageSpace),
        new(ListingPictureType.Appliance),
        new(ListingPictureType.FrontDoor),
        new(ListingPictureType.Balcony),
        new(ListingPictureType.Other)
    ];

    public ListingPictureTypeLabel PictureType
    {
        get => _pictureType;
        set
        {
            if (value is null || _pictureType == value) return;
            _model.PictureType = value.Key;
            SetProperty(ref _pictureType, value);
        }
    }

    public string Description
    {
        get => _model.Description;
        set => SetProperty(_model.Description, value ?? string.Empty, _model, static (picture, description) => picture.Description = description);
    }

    public bool IsMain
    {
        get => _model.IsMain;
        set => SetProperty(_model.IsMain, value, _model, static (picture, isMain) => picture.IsMain = isMain);
    }
}
