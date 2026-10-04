using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyKind(EnumResidentialKinds key)
{
    private Dictionary<EnumResidentialKinds, string> BuildingKindTypeDictionary
    {
        get;
    } = new Dictionary<EnumResidentialKinds, string>()
        {
            {EnumResidentialKinds.Unspecified, "未指定"},
            {EnumResidentialKinds.Apartment, "アパート"},
            {EnumResidentialKinds.Mansion, "マンション"},
            {EnumResidentialKinds.House, "一戸建て"},
            {EnumResidentialKinds.TerraceHouse, "テラスハウス"},
            {EnumResidentialKinds.TownHouse, "タウンハウス"},
            {EnumResidentialKinds.ShareHouse, "シェアハウス"},
            {EnumResidentialKinds.Dormitory, "寮・下宿"}
        };

    public string Label => BuildingKindTypeDictionary[Key];

    public EnumResidentialKinds Key => key;
};
