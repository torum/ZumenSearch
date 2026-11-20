using ZumenSearch.Domain.Entities.Base;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Domain.Enums;
namespace ZumenSearch.Application.SearchResults;

// 部屋検索結果一覧表示用（
public sealed partial class ListingSearchResultItem : ListingBase
{
    public ListingSearchResultItem(string id, string propertyId, PropertyContextType contextType, string fudousanId) : base(id, EntityStatus.Saved, propertyId, EntityStatus.Saved, contextType, fudousanId)
    {
        //PropertyId = propertyId;
    }

    //public string PropertyId { get; init; }

    public string PropertyName
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            IsModified = true;//?
        }
    }

}
