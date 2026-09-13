using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 総合検索画面の最近更新された物件一覧用。賃貸売買兼用。
public sealed partial class PropertySearchResultItem : PropertyBase
{
    // TODO: more

    public string CreatedAt = string.Empty;
    public string UpdatedAt = string.Empty;

    public PropertySearchResultItem(string id) : base(id)
    {
        //
    }
}

