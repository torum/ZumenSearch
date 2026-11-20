using System.Collections.ObjectModel;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.SearchResults;
namespace ZumenSearch.Application.Results;

// TODO: This uses ObservableCollection, a UI-oriented collection type. Longer term,
// application contracts would be cleaner with ordinary or read-only collections, leaving observable state to Presentation

// Common Listings SearchResult
public sealed class ListingSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<ListingSearchResultItem> ListingSearchResult { get; set; } = [];
}
