using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.SearchResult;

// Common Listings SearchResult
public sealed class ListingSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<ListingSearchResultItem> ListingSearchResult { get; set; } = [];
}
