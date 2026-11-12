using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.SearchResult;

// 部屋検索結果一覧表示用（
public sealed partial class ListingSearchResultItem : ListingBase
{
    public ListingSearchResultItem(string id, string propertyId, PropertyContextType contextType) : base(id, EntityStatus.Saved, propertyId, EntityStatus.Saved, contextType)
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
