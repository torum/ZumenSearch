using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyTypeLabel(PropertyType key)
{
    private static readonly IReadOnlyDictionary<
        PropertyType,
        string> Labels = new Dictionary<
            PropertyType,
            string>
        {
            [PropertyType.Unspecified] = "未指定",
            [PropertyType.Office] = "事務所",
            [PropertyType.Retail] = "店舗",
            [PropertyType.Warehouse] = "倉庫",
            [PropertyType.Factory] = "工場",
            [PropertyType.Clinic] = "診療所",
            [PropertyType.Restaurant] = "飲食店",
            [PropertyType.Hotel] = "ホテル・旅館",
            [PropertyType.Land] = "事業用土地",
            [PropertyType.Other] = "その他"
        };

    public PropertyType Key => key;

    public string Label => Labels[Key];
}
