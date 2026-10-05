using System;
using System.Collections.Generic;
using System.Text;

// TODO: separate files?

namespace ZumenSearch.Models.Rent.Residentials
{
    #region == Property ==

    // Kind：物件種目（アパート・マンション・一戸建て・他）
    public enum EnumKinds
    {
        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    // Structure: 建物構造
    public enum EnumStructures
    {
        Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
    }

    public enum EnumPropertyPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum EnumPropertyPictureType
    {
        //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
        Unspecified, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion

    #region == listing ==

    public enum EnumListingPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum EnumListingPictureType
    {
        Unspecified, Madori, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Other
        //Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion
}
