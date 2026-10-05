using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class ListingPdfType(EnumListingPdfType key)
{
    private Dictionary<EnumListingPdfType, string> RoomPdfTypeDictionary
    {
        get;
    } = new Dictionary<EnumListingPdfType, string>()
    {
                {EnumListingPdfType.Unspecified, "未指定"},
                {EnumListingPdfType.Maisoku, "募集図面"},
                {EnumListingPdfType.Architectural, "建築図面"},
                {EnumListingPdfType.Toukibo, "登記簿謄本"},
                {EnumListingPdfType.Kouzu, "公図・地図"},
                {EnumListingPdfType.Other, "その他"},
    };

    public string Label => RoomPdfTypeDictionary[Key];

    public EnumListingPdfType Key => key;
};
