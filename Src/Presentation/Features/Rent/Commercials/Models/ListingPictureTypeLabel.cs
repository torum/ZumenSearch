using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingPictureTypeLabel(ListingPictureType key)
{
    private Dictionary<ListingPictureType, string> ListingPictureTypeDictionary
    {
        get;
    } = new Dictionary<ListingPictureType, string>()
    {
                {ListingPictureType.Unspecified, "未指定"},
                {ListingPictureType.Madori, "間取り図"},
                {ListingPictureType.Interior, "室内"},
                {ListingPictureType.Entrance, "入口"},
                {ListingPictureType.Equipment, "設備"},
                {ListingPictureType.Other, "その他"},
            };

    public string Label => ListingPictureTypeDictionary[Key];

    public ListingPictureType Key => key;
};
