using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyPdfTypeLabel(PropertyPdfType key)
{
    private static readonly Dictionary<PropertyPdfType, string> pdfTypes = new()
    {
        [PropertyPdfType.Unspecified] = "未指定",
        [PropertyPdfType.Listing] = "募集図面",
        [PropertyPdfType.Architectural] = "建築図面",
        [PropertyPdfType.Registry] = "登記簿謄本",
        [PropertyPdfType.Map] = "公図・地図",
        [PropertyPdfType.Other] = "その他"
    };
    private static readonly Dictionary<PropertyPdfType, string> Labels = pdfTypes;

    public PropertyPdfType Key => key;

    public string Label => Labels[key];
}
