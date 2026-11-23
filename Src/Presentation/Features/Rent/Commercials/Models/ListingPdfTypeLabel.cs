using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingPdfTypeLabel(ListingPdfType key)
{
    private Dictionary<ListingPdfType, string> ListingPdfTypeDictionary
    {
        get;
    } = new Dictionary<ListingPdfType, string>()
    {
                {ListingPdfType.Unspecified, "未指定"},
                {ListingPdfType.Listing, "募集図面"},
                {ListingPdfType.Architectural, "建築図面"},
                {ListingPdfType.Registry, "登記簿謄本"},
                {ListingPdfType.Map, "公図・地図"},
                {ListingPdfType.Other, "その他"},
    };

    public string Label => ListingPdfTypeDictionary[Key];

    public ListingPdfType Key => key;
};
