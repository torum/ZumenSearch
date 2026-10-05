using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Sale.Residentials;

#pragma warning disable IDE0290 // Use primary constructor

// Remove Enux suffix it is considered bad practice.

public enum PropertyType
{
    Unspecified,
    Apartment,
    Mansion,
    House,
    TerraceHouse,
    TownHouse,
    ShareHouse,
    Dormitory
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
    RB,
    CFT,
    Other
}

