using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyKind(EnumKinds key)
{
    private static readonly IReadOnlyDictionary<
        EnumKinds,
        string> Labels = new Dictionary<
            EnumKinds,
            string>
        {
            [EnumKinds.Unspecified] = "未指定",
            [EnumKinds.Office] = "事務所",
            [EnumKinds.Retail] = "店舗",
            [EnumKinds.Warehouse] = "倉庫",
            [EnumKinds.Factory] = "工場",
            [EnumKinds.Clinic] = "診療所",
            [EnumKinds.Restaurant] = "飲食店",
            [EnumKinds.Hotel] = "ホテル・旅館",
            [EnumKinds.Land] = "事業用土地",
            [EnumKinds.Other] = "その他"
        };

    public EnumKinds Key => key;

    public string Label => Labels[Key];
}
