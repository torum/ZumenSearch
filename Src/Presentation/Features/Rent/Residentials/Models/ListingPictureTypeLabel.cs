using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class ListingPictureTypeLabel(ListingPictureType key)
{
    private Dictionary<ListingPictureType, string> ListingPictureTypeDictionary
    {
        get;
    } = new Dictionary<ListingPictureType, string>()
    {
                {ListingPictureType.Unspecified, "未指定"},
                {ListingPictureType.Madori, "間取り図"},
                //{EnumUnitPictureType.Gaikan, "外観"},
                {ListingPictureType.Situnai, "室内"},
                {ListingPictureType.LivingDining, "リビング・ダイニング"},
                {ListingPictureType.Bedroom, "寝室"},
                {ListingPictureType.Kitchen, "キッチン"},
                {ListingPictureType.Bathroom, "浴室"},
                {ListingPictureType.Restroom, "トイレ"},
                {ListingPictureType.Washroom, "洗面"},
                {ListingPictureType.StorageSpace, "収納"},
                {ListingPictureType.Appliance, "設備"},
                {ListingPictureType.FrontDoor, "玄関"},
                {ListingPictureType.Balcony, "バルコニー"},
                //{EnumUnitPictureType.Entrance, "エントランス"},
                //{EnumUnitPictureType.Neighborhood, "周辺"},
                {ListingPictureType.Other, "その他"},
            };

    public string Label => ListingPictureTypeDictionary[Key];

    public ListingPictureType Key => key;
};
