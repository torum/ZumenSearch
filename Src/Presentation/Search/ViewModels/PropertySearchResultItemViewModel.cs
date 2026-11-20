using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Presentation.Search.ViewModels;

public sealed class PropertySearchResultItemViewModel : ObservableObject
{
    private readonly PropertySearchResultItem _model;

    public PropertySearchResultItemViewModel(PropertySearchResultItem model)
    {
        _model = model;
    }

    public string Id => _model.Id;
    public PropertyContextType PropertyContextType => _model.PropertyContextType;
    public string Name => _model.Name;
    public string UpdatedAt => _model.UpdatedAt;
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
        var previousName = _model.Name;
        _model.SetName(name);
        if (previousName != _model.Name)
        {
            OnPropertyChanged(nameof(Name));
        }
    }
}
