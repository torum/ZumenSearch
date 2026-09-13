using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 検索結果一覧表示用（建物）
public sealed partial class PropertySearchResultItem : PropertyBase
{
    // TODO: more

    public PropertySearchResultItem(string id, EnumPropertyKind kind) : base(id, kind)
    {
        //
    }
}

// 検索結果一覧表示用（部屋）
public sealed partial class ListingSearchResultItem : ListingBase
{
    public string PropertyId { get; init; }

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

    public ListingSearchResultItem(string id, string propertyId) : base(id)
    {
        PropertyId = propertyId;
    }
}
