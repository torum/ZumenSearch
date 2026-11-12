using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;


// Kind：物件種目（アパート・マンション・一戸建て・他）
public enum PropertyKindType
{
    Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
}

public sealed class PropertyKindTypeLabel(PropertyKindType key)
{
    private Dictionary<PropertyKindType, string> PropertyKindTypeDictionary { get;} = new()
        {
            {PropertyKindType.Unspecified, "未指定"},
            {PropertyKindType.Apartment, "アパート"},
            {PropertyKindType.Mansion, "マンション"},
            {PropertyKindType.House, "一戸建て"},
            {PropertyKindType.TerraceHouse, "テラスハウス"},
            {PropertyKindType.TownHouse, "タウンハウス"},
            {PropertyKindType.ShareHouse, "シェアハウス"},
            {PropertyKindType.Dormitory, "寮・下宿"}
        };

    public string Label => PropertyKindTypeDictionary[Key];

    public PropertyKindType Key => key;

    public static IReadOnlyList<PropertyKindTypeLabel> GetAll() => Enum.GetValues<PropertyKindType>()
        .Select(value => new PropertyKindTypeLabel(value))
        .ToArray();
};