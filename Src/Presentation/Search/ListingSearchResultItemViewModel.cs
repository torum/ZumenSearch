using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Presentation.Search;

public sealed class ListingSearchResultItemViewModel : ObservableObject
{
    private readonly ListingSearchResultItem _model;

    public ListingSearchResultItemViewModel(ListingSearchResultItem model)
    {
        _model = model;
    }

    public string Id => _model.Id;
    public string PropertyId => _model.PropertyId;
    public PropertyContextType PropertyContextType => _model.PropertyContextType;
    public string FudousanId => _model.FudousanId;
    public string Name => _model.Name;
    public string PropertyName => _model.PropertyName;

    public void SetName(string name)
    {
        var previousName = _model.Name;
        _model.SetName(name);
        _model.SetIsModified(false);
        if (previousName != _model.Name)
        {
            OnPropertyChanged(nameof(Name));
        }
    }
}
