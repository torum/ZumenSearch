using System;
using System.Collections.Generic;
using System.Text;

// TODO: separate files?

namespace ZumenSearch.Models.Rent.Residentials
{
    #region == Property ==

    // Kind：物件種目（アパート・マンション・一戸建て・他）
    public enum PropertyType
    {
        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    // Structure: 建物構造
    public enum StructureType
    {
        Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
    }

    public enum PropertyPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum PropertyPictureType
    {
        //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
        Unspecified, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion

    #region == listing ==

    public enum ListingPdfType
    {
        Unspecified,
        Maisoku,
        Architectural,
        Toukibo,
        Kouzu,
        Other
    }

    public enum ListingPictureType
    {
        Unspecified, Madori, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Other
        //Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
    }

    #endregion
}
