using System;
using System.Collections.Generic;
using System.Text;

// TODO: separate files?

namespace ZumenSearch.Models.Rent.Commercials;

#region == Property ==

public enum PropertyKindType
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

public enum StructureType
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

public enum PropertyPdfType
{
    Unspecified,
    Listing,
    Architectural,
    Registry,
    Map,
    Other
}

public enum PropertyPictureType
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
