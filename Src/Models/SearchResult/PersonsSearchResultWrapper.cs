using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.SearchResult;

// Person SearchResult
public sealed class PersonsSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<PersonSearchResultItem> PersonSearchResult { get; set; } = [];
}
