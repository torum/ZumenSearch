using System;
using System.Collections.Generic;
using System.Text;

// TODO: separate files?

namespace ZumenSearch.Models.Rent.Commercials;

#region == Property ==

public enum EnumKinds
{
    Unspecified,
    Office,
    Retail,
    Warehouse,
    Factory,
    Clinic,
    Restaurant,
    Hotel,
    Land,
    Other
}

public enum EnumStructures
{
    Unspecified,
    Wood,
    Block,
    LightSteel,
    Steel,
    RC,
    SRC,
    ALC,
    PC,
    HPC,
    Other
}

public enum EnumPropertyPdfType
{
    Unspecified,
    Listing,
    Architectural,
    Registry,
    Map,
    Other
}

public enum EnumPropertyPictureType
{
    Unspecified,
    Exterior,
    Entrance,
    Neighborhood,
    Interior,
    Other
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
