using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyKind(EnumKinds key)
{
    private Dictionary<EnumKinds, string> BuildingKindTypeDictionary
    {
        get;
    } = new Dictionary<EnumKinds, string>()
        {
            {EnumKinds.Unspecified, "未指定"},
            {EnumKinds.Apartment, "アパート"},
            {EnumKinds.Mansion, "マンション"},
            {EnumKinds.House, "一戸建て"},
            {EnumKinds.TerraceHouse, "テラスハウス"},
            {EnumKinds.TownHouse, "タウンハウス"},
            {EnumKinds.ShareHouse, "シェアハウス"},
            {EnumKinds.Dormitory, "寮・下宿"}
        };

    public string Label => BuildingKindTypeDictionary[Key];

    public EnumKinds Key => key;
};
