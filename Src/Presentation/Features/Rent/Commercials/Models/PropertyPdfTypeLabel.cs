using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class PropertyPdfTypeLabel(PropertyPdfType key)
{
    private Dictionary<PropertyPdfType, string> BuildingPdfTypeDictionary
    {
        get;
    } = new Dictionary<PropertyPdfType, string>()
    {
                {PropertyPdfType.Unspecified, "未指定"},
                {PropertyPdfType.Listing, "募集図面"},
                {PropertyPdfType.Architectural, "建築図面"},
                {PropertyPdfType.Registry, "登記簿謄本"},
                {PropertyPdfType.Map, "公図・地図"},
                {PropertyPdfType.Other, "その他"},
    };

    public string Label => BuildingPdfTypeDictionary[Key];

    public PropertyPdfType Key => key;
};
