using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials
{
    #region == Property ==

    // Kind：物件種目（アパート・マンション・一戸建て・他）
    public enum EnumResidentialKinds
    {
        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    // Structure: 建物構造
    public enum EnumStructures
    {
        Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
    }

    public enum EnumResidentialPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum EnumResidentialPictureType
    {
        //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
        Unspecified, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion

    #region == listing ==

    public enum EnumRoomPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum EnumRoomPictureType
    {
        Unspecified, Madori, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Other
        //Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion
}
