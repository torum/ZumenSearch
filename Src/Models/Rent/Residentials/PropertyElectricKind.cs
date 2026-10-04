using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed class PropertyElectricKind
{
    public string Label { get; set; }

    public Property.EnumElectricKind Key { get; set; }

    public PropertyElectricKind(Property.EnumElectricKind key, string label)
    {
        Key = key;
        Label = label;
    }
};
