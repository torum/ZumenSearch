using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyPictureType(EnumResidentialPictureType key)
{
    private Dictionary<EnumResidentialPictureType, string> PropertyPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumResidentialPictureType, string>()
    {
                {EnumResidentialPictureType.Unspecified, "未指定"},
                //{EnumResidentialPictureType.Madori, "間取り図"},
                {EnumResidentialPictureType.Gaikan, "外観"},
                //{EnumResidentialPictureType.Situnai, "室内"},
                //{EnumResidentialPictureType.LivingDining, "リビング・ダイニング"},
                //{EnumResidentialPictureType.Bedroom, "寝室"},
                //{EnumResidentialPictureType.Kitchen, "キッチン"},
                //{EnumResidentialPictureType.Bathroom, "浴室"},
                //{EnumResidentialPictureType.Restroom, "トイレ"},
                //{EnumResidentialPictureType.Washroom, "洗面"},
                //{EnumResidentialPictureType.StorageSpace, "収納"},
                //{EnumResidentialPictureType.Appliance, "設備"},
                //{EnumResidentialPictureType.Balcony, "バルコニー"},
                //{EnumResidentialPictureType.FrontDoor, "玄関"},
                {EnumResidentialPictureType.Entrance, "エントランス"},
                {EnumResidentialPictureType.Neighborhood, "周辺"},
                {EnumResidentialPictureType.Other, "その他"},
            };

    public string Label => PropertyPictureTypeDictionary[Key];

    public EnumResidentialPictureType Key => key;
};
