#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Presentation.Search.ViewModels;

public sealed partial class PropertySearchResultItemViewModel : ObservableObject
{
    private readonly PropertySearchResultItem _model;

    public PropertySearchResultItemViewModel(PropertySearchResultItem model)
    {
        _model = model;
    }

    public string Id => _model.Id;
    public PropertyContextType PropertyContextType => _model.PropertyContextType;
    public string Name => _model.Name;
    public string UpdatedAt => _model.UpdatedAtFormatted;
    public string CreatedAt => _model.CreatedAtFormatted;
    public string? ThumbImagePath => _model.ThumbImagePath;

    public string ThumbnailFilename
    {
        get => _model.ThumbnailFilename;
        set
        {
            _model.ThumbnailFilename = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ThumbImagePath));
        }
    }

    public void SetName(string name)
    {
        string previousName = _model.Name;
        _model.SetName(name);
        if (previousName != _model.Name)
        {
            OnPropertyChanged(nameof(Name));
        }
    }
}
