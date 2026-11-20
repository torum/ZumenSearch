using System;
using System.Collections.Generic;
using System.Text;

using ZumenSearch.Domain.Entities.Rent.Residentials;
namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

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
