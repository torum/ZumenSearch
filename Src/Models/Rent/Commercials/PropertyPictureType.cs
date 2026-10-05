using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyPictureType(EnumPropertyPictureType key)
{
    private static readonly Dictionary<EnumPropertyPictureType, string> PictureTypes = new()
    {
        [EnumPropertyPictureType.Unspecified] = "未指定",
        [EnumPropertyPictureType.Exterior] = "外観",
        [EnumPropertyPictureType.Entrance] = "エントランス",
        [EnumPropertyPictureType.Neighborhood] = "周辺",
        [EnumPropertyPictureType.Interior] = "室内",
        [EnumPropertyPictureType.Other] = "その他"
    };
    private static readonly Dictionary<EnumPropertyPictureType, string> Labels = PictureTypes;

    public EnumPropertyPictureType Key => key;

    public string Label => Labels[key];
}
