using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public class PropertyKanriShutai(Property.EnumKanriShutai key, string label)
{
    public string Label => label;

    public Property.EnumKanriShutai Key => key;
};
