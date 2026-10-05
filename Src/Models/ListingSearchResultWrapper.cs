using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

// Common Listings SearchResult
public sealed class ListingSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.ListingSearchResultItem> ListingSearchResult { get; set; } = [];
}
