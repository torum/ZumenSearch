using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

// Common Properties SearchResult
public sealed class PropertySearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.PropertySearchResultItem> PropertySearchResult { get; set; } = [];
}
