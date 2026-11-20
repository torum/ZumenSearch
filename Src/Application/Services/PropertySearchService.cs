using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;

namespace ZumenSearch.Application.Services;

public sealed class PropertySearchService(IPropertySearchRepository repository)
{
    public PropertySearchResultWrapper GetRecent() => repository.SelectRecentProperties();
    public PropertySearchResultWrapper Search(string keyword, PropertySearchTarget searchTarget = PropertySearchTarget.PropertyName) => repository.SelectPropertiesByKeyword(keyword, searchTarget);
}
