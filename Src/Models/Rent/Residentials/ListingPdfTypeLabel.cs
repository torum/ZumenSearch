using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class ListingPdfTypeLabel(ListingPdfType key)
{
    private Dictionary<ListingPdfType, string> RoomPdfTypeDictionary
    {
        get;
    } = new Dictionary<ListingPdfType, string>()
    {
                {ListingPdfType.Unspecified, "未指定"},
                {ListingPdfType.Maisoku, "募集図面"},
                {ListingPdfType.Architectural, "建築図面"},
                {ListingPdfType.Toukibo, "登記簿謄本"},
                {ListingPdfType.Kouzu, "公図・地図"},
                {ListingPdfType.Other, "その他"},
    };

    public string Label => RoomPdfTypeDictionary[Key];

    public ListingPdfType Key => key;
};
