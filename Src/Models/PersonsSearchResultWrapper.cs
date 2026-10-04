using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models;

// Person SearchResult
public sealed class PersonsSearchResultWrapper : ResultWrapperBase
{
    public ObservableCollection<PersonSearchResultItem> PersonSearchResult = [];
}
