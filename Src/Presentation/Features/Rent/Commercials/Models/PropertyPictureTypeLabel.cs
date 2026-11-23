using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class PropertyPictureTypeLabel(PropertyPictureType key)
{
    private Dictionary<PropertyPictureType, string> PropertyPictureTypeDictionary
    {
        get;
    } = new Dictionary<PropertyPictureType, string>()
    {
                {PropertyPictureType.Unspecified, "未指定"},
                {PropertyPictureType.Exterior, "外観"},
                {PropertyPictureType.Interior, "室内"},
                {PropertyPictureType.Entrance, "エントランス"},
                {PropertyPictureType.Neighborhood, "周辺"},
                {PropertyPictureType.Other, "その他"},
            };

    public string Label => PropertyPictureTypeDictionary[Key];

    public PropertyPictureType Key => key;
};
