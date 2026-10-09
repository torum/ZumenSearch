using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.SearchResult;

// Common Properties SearchResult
public sealed class PropertySearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<PropertySearchResultItem> PropertySearchResult { get; set; } = [];
}
