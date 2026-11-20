using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class PropertyPdfTypeLabel(PropertyPdfType key)
{
    private Dictionary<PropertyPdfType, string> BuildingPdfTypeDictionary
    {
        get;
    } = new Dictionary<PropertyPdfType, string>()
    {
                {PropertyPdfType.Unspecified, "未指定"},
                {PropertyPdfType.Maisoku, "募集図面"},
                {PropertyPdfType.Architectural, "建築図面"},
                {PropertyPdfType.Toukibo, "登記簿謄本"},
                {PropertyPdfType.Kouzu, "公図・地図"},
                {PropertyPdfType.Other, "その他"},
    };

    public string Label => BuildingPdfTypeDictionary[Key];

    public PropertyPdfType Key => key;
};
