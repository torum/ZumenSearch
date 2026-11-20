using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums.Rent.Residentials;
using ZumenSearch.Presentation.Features.Rent.Residentials.Models;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;

public sealed partial class PropertyPictureViewModel : ObservableObject
{
    private readonly PropertyPicture _model;
    private PropertyPictureTypeLabel _pictureType;

    public PropertyPictureViewModel(PropertyPicture model, PropertyViewModel parentViewModel)
    {
        _model = model;
        ParentViewModel = parentViewModel;
        _pictureType = ResidentialPictureTypes.First(item => item.Key == model.PictureType);
    }

    public PropertyPicture Model => _model;
    public PropertyViewModel ParentViewModel { get; }
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

    public ObservableCollection<PropertyPictureTypeLabel> ResidentialPictureTypes { get; } =
    [
        new(PropertyPictureType.Unspecified),
        new(PropertyPictureType.Gaikan),
        new(PropertyPictureType.Entrance),
        new(PropertyPictureType.Neighborhood),
        new(PropertyPictureType.Other)
    ];

    public PropertyPictureTypeLabel PictureType
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
