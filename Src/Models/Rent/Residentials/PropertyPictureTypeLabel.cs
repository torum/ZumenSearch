using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum PropertyPictureType
{
    //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
    Unspecified, Gaikan, Entrance, Neighborhood, Other
}

public sealed class PropertyPictureTypeLabel(PropertyPictureType key)
{
    private Dictionary<PropertyPictureType, string> PropertyPictureTypeDictionary
    {
        get;
    } = new Dictionary<PropertyPictureType, string>()
    {
                {PropertyPictureType.Unspecified, "未指定"},
                //{EnumResidentialPictureType.Madori, "間取り図"},
                {PropertyPictureType.Gaikan, "外観"},
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
                {PropertyPictureType.Entrance, "エントランス"},
                {PropertyPictureType.Neighborhood, "周辺"},
                {PropertyPictureType.Other, "その他"},
            };

    public string Label => PropertyPictureTypeDictionary[Key];

    public PropertyPictureType Key => key;
};
