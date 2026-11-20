using System.Collections.ObjectModel;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.SearchResults;
namespace ZumenSearch.Application.Results;

// TODO: This uses ObservableCollection, a UI-oriented collection type. Longer term,
// application contracts would be cleaner with ordinary or read-only collections, leaving observable state to Presentation

// Common Properties SearchResult
public sealed class PropertySearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<PropertySearchResultItem> PropertySearchResult { get; set; } = [];
}
