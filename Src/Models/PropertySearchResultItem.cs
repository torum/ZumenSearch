using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models;


// 総合検索画面の物件一覧用。賃貸売買兼用。(最近更新された物件一覧)
public sealed partial class PropertySearchResultItem : PropertyBase
{
    // TODO: more

    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public PropertySearchResultItem(string id, PropertyContextType contextType) : base(id, EntityStatus.Saved, contextType)
    {
        //
    }
}
