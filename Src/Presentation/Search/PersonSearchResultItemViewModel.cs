using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Presentation.Search;

public sealed class PersonSearchResultItemViewModel : ObservableObject
{
    internal PersonSearchResultItem Model { get; }

    public PersonSearchResultItemViewModel(PersonSearchResultItem model)
    {
        Model = model;
    }

    public string Id => Model.Id;
    public PersonKindType PersonKind => Model.PersonKind;
    public string Name => Model.Name;

    public void SetName(string name)
    {
        var previousName = Model.Name;
        Model.SetName(name);
        if (previousName != Model.Name)
        {
            OnPropertyChanged(nameof(Name));
        }
    }
}
