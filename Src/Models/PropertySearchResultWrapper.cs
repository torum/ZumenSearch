using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

// Common Properties SearchResult
public sealed class PropertySearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.PropertySearchResultItem> PropertySearchResult { get; set; } = [];
}
