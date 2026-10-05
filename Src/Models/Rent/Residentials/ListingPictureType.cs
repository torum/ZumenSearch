using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class ListingPictureType(EnumListingPictureType key)
{
    private Dictionary<EnumListingPictureType, string> RoomPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumListingPictureType, string>()
    {
                {EnumListingPictureType.Unspecified, "未指定"},
                {EnumListingPictureType.Madori, "間取り図"},
                //{EnumUnitPictureType.Gaikan, "外観"},
                {EnumListingPictureType.Situnai, "室内"},
                {EnumListingPictureType.LivingDining, "リビング・ダイニング"},
                {EnumListingPictureType.Bedroom, "寝室"},
                {EnumListingPictureType.Kitchen, "キッチン"},
                {EnumListingPictureType.Bathroom, "浴室"},
                {EnumListingPictureType.Restroom, "トイレ"},
                {EnumListingPictureType.Washroom, "洗面"},
                {EnumListingPictureType.StorageSpace, "収納"},
                {EnumListingPictureType.Appliance, "設備"},
                {EnumListingPictureType.FrontDoor, "玄関"},
                {EnumListingPictureType.Balcony, "バルコニー"},
                //{EnumUnitPictureType.Entrance, "エントランス"},
                //{EnumUnitPictureType.Neighborhood, "周辺"},
                {EnumListingPictureType.Other, "その他"},
            };

    public string Label => RoomPictureTypeDictionary[Key];

    public EnumListingPictureType Key => key;
};
