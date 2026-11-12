using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyPictureTypeLabel(PropertyPictureType key)
{
    private static readonly Dictionary<PropertyPictureType, string> PictureTypes = new()
    {
        [PropertyPictureType.Unspecified] = "未指定",
        [PropertyPictureType.Exterior] = "外観",
        [PropertyPictureType.Entrance] = "エントランス",
        [PropertyPictureType.Neighborhood] = "周辺",
        [PropertyPictureType.Interior] = "室内",
        [PropertyPictureType.Other] = "その他"
    };
    private static readonly Dictionary<PropertyPictureType, string> Labels = PictureTypes;

    public PropertyPictureType Key => key;

    public string Label => Labels[key];
}
