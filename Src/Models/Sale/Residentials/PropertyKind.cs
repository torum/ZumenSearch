using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Sale.Residentials;

public sealed class PropertyKind(PropertyType key)
{
    private static readonly Dictionary<PropertyType, string> Labels =
        new Dictionary<PropertyType, string>
        {
            [PropertyType.Unspecified] = "未指定",
            [PropertyType.Apartment] = "アパート",
            [PropertyType.Mansion] = "マンション",
            [PropertyType.House] = "一戸建て",
            [PropertyType.TerraceHouse] = "テラスハウス",
            [PropertyType.TownHouse] = "タウンハウス",
            [PropertyType.ShareHouse] = "シェアハウス",
            [PropertyType.Dormitory] = "寮・下宿"
        };

    public PropertyType Key => key;

    public string Label => Labels[Key];
}
