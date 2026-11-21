#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using System.Globalization;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
namespace ZumenSearch.Application.SearchResults;

// 総合検索画面の「物件検索結果一覧」と「最近更新された物件一覧」。賃貸売買兼用。

public sealed partial class PropertySearchResultItem : PropertyBase
{
    public PropertySearchResultItem(string id, PropertyContextType contextType) : base(id, EntityStatus.Saved, contextType)
    {
        //
    }

}
