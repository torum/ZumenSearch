using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;

namespace ZumenSearch.Application.Contracts;

public interface IPropertySearchRepository
{
    PropertySearchResultWrapper SelectRecentProperties();
    PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword, PropertySearchTarget searchTarget = PropertySearchTarget.PropertyName);
}
