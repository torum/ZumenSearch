using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyKind(EnumCommercialKinds key)
{
    private static readonly IReadOnlyDictionary<
        EnumCommercialKinds,
        string> Labels = new Dictionary<
            EnumCommercialKinds,
            string>
        {
            [EnumCommercialKinds.Unspecified] = "未指定",
            [EnumCommercialKinds.Office] = "事務所",
            [EnumCommercialKinds.Retail] = "店舗",
            [EnumCommercialKinds.Warehouse] = "倉庫",
            [EnumCommercialKinds.Factory] = "工場",
            [EnumCommercialKinds.Clinic] = "診療所",
            [EnumCommercialKinds.Restaurant] = "飲食店",
            [EnumCommercialKinds.Hotel] = "ホテル・旅館",
            [EnumCommercialKinds.Land] = "事業用土地",
            [EnumCommercialKinds.Other] = "その他"
        };

    public EnumCommercialKinds Key => key;

    public string Label => Labels[Key];
}
