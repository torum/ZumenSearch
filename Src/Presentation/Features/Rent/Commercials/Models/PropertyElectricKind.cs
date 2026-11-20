using System;
using System.Collections.Generic;
using System.Text;

using ZumenSearch.Domain.Entities.Rent.Commercials;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class PropertyElectricKind
{
    public string Label { get; set; }

    public Property.EnumElectricType Key { get; set; }

    public PropertyElectricKind(Property.EnumElectricType key, string label)
    {
        Key = key;
        Label = label;
    }
};
