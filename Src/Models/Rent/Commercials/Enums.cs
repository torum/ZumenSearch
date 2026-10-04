using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public enum EnumCommercialKinds
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

public enum EnumCommercialPdfType
{
    Unspecified,
    Listing,
    Architectural,
    Registry,
    Map,
    Other
}

public enum EnumCommercialPictureType
{
    Unspecified,
    Exterior,
    Entrance,
    Neighborhood,
    Interior,
    Other
}
