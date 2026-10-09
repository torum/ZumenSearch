using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Sale.Residentials;

public sealed class PropertyContextType(PropertyKindType key)
{
    private static readonly Dictionary<PropertyKindType, string> Labels =
        new Dictionary<PropertyKindType, string>
        {
            [PropertyKindType.Unspecified] = "未指定",
            [PropertyKindType.Apartment] = "アパート",
            [PropertyKindType.Mansion] = "マンション",
            [PropertyKindType.House] = "一戸建て",
            [PropertyKindType.TerraceHouse] = "テラスハウス",
            [PropertyKindType.TownHouse] = "タウンハウス",
            [PropertyKindType.ShareHouse] = "シェアハウス",
            [PropertyKindType.Dormitory] = "寮・下宿"
        };

    public PropertyKindType Key => key;

    public string Label => Labels[Key];
}
