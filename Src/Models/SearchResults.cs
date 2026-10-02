using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 総合検索画面の物件一覧用。賃貸売買兼用。(最近更新された物件一覧)
public sealed partial class PropertySearchResultItem : PropertyBase
{
    // TODO: more

    public string CreatedAt = string.Empty;
    public string UpdatedAt = string.Empty;

    public PropertySearchResultItem(string id, EnumPropertyKind kind) : base(id, EnumEntityStatus.Saved, kind)
    {
        //
    }
}

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

    public ListingSearchResultItem(string id, string propertyId, EnumPropertyKind propertyKind) : base(id, EnumEntityStatus.Saved, propertyId, EnumEntityStatus.Saved, propertyKind)
    {
        //PropertyId = propertyId;
    }
}

// 貸主検索結果一覧表示用 
public sealed partial class PersonSearchResultItem : PersonBase
{
    // TODO: more

    public string CreatedAt = string.Empty;
    public string UpdatedAt = string.Empty;

    public PersonSearchResultItem(string id, EnumPersonKind personKind) : base(id, EnumEntityStatus.Saved, personKind)
    {
        //
    }
}




