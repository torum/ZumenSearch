using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyPdfType(EnumPropertyPdfType key)
{
    private Dictionary<EnumPropertyPdfType, string> BuildingPdfTypeDictionary
    {
        get;
    } = new Dictionary<EnumPropertyPdfType, string>()
    {
                {EnumPropertyPdfType.Unspecified, "未指定"},
                {EnumPropertyPdfType.Maisoku, "募集図面"},
                {EnumPropertyPdfType.Architectural, "建築図面"},
                {EnumPropertyPdfType.Toukibo, "登記簿謄本"},
                {EnumPropertyPdfType.Kouzu, "公図・地図"},
                {EnumPropertyPdfType.Other, "その他"},
    };

    public string Label => BuildingPdfTypeDictionary[Key];

    public EnumPropertyPdfType Key => key;
};
