using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyTypeLabel(PropertyType key)
{
    private Dictionary<PropertyType, string> BuildingKindTypeDictionary { get;} = new Dictionary<PropertyType, string>()
        {
            {PropertyType.Unspecified, "未指定"},
            {PropertyType.Apartment, "アパート"},
            {PropertyType.Mansion, "マンション"},
            {PropertyType.House, "一戸建て"},
            {PropertyType.TerraceHouse, "テラスハウス"},
            {PropertyType.TownHouse, "タウンハウス"},
            {PropertyType.ShareHouse, "シェアハウス"},
            {PropertyType.Dormitory, "寮・下宿"}
        };

    public string Label => BuildingKindTypeDictionary[Key];

    public PropertyType Key => key;
};