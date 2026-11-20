using System.Collections.ObjectModel;
using ZumenSearch.Domain.Entities.Base;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Application.SearchResults;
namespace ZumenSearch.Application.Results;

// TODO: This uses ObservableCollection, a UI-oriented collection type. Longer term,
// application contracts would be cleaner with ordinary or read-only collections, leaving observable state to Presentation

// Person SearchResult
public sealed class PersonsSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<PersonSearchResultItem> PersonSearchResult { get; set; } = [];
}
