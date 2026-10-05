using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyPdfType(EnumPropertyPdfType key)
{
    private static readonly Dictionary<EnumPropertyPdfType, string> pdfTypes = new()
    {
        [EnumPropertyPdfType.Unspecified] = "未指定",
        [EnumPropertyPdfType.Listing] = "募集図面",
        [EnumPropertyPdfType.Architectural] = "建築図面",
        [EnumPropertyPdfType.Registry] = "登記簿謄本",
        [EnumPropertyPdfType.Map] = "公図・地図",
        [EnumPropertyPdfType.Other] = "その他"
    };
    private static readonly Dictionary<EnumPropertyPdfType, string> Labels = pdfTypes;

    public EnumPropertyPdfType Key => key;

    public string Label => Labels[key];
}
