using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyPictureType(EnumCommercialPictureType key)
{
    private static readonly Dictionary<EnumCommercialPictureType, string> PictureTypes = new()
    {
        [EnumCommercialPictureType.Unspecified] = "未指定",
        [EnumCommercialPictureType.Exterior] = "外観",
        [EnumCommercialPictureType.Entrance] = "エントランス",
        [EnumCommercialPictureType.Neighborhood] = "周辺",
        [EnumCommercialPictureType.Interior] = "室内",
        [EnumCommercialPictureType.Other] = "その他"
    };
    private static readonly Dictionary<EnumCommercialPictureType, string> Labels = PictureTypes;

    public EnumCommercialPictureType Key => key;

    public string Label => Labels[key];
}
