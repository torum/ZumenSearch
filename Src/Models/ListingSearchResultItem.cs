using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models;

// 部屋検索結果一覧表示用（
public sealed partial class ListingSearchResultItem : ListingBase
{
    //public string PropertyId { get; init; }

    public string PropertyName
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    public ListingSearchResultItem(string id, string propertyId, PropertyKind propertyKind) : base(id, EntityStatus.Saved, propertyId, EntityStatus.Saved, propertyKind)
    {
        //PropertyId = propertyId;
    }
}
