using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyPictureType(EnumPropertyPictureType key)
{
    private Dictionary<EnumPropertyPictureType, string> PropertyPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumPropertyPictureType, string>()
    {
                {EnumPropertyPictureType.Unspecified, "未指定"},
                //{EnumResidentialPictureType.Madori, "間取り図"},
                {EnumPropertyPictureType.Gaikan, "外観"},
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
                {EnumPropertyPictureType.Entrance, "エントランス"},
                {EnumPropertyPictureType.Neighborhood, "周辺"},
                {EnumPropertyPictureType.Other, "その他"},
            };

    public string Label => PropertyPictureTypeDictionary[Key];

    public EnumPropertyPictureType Key => key;
};
