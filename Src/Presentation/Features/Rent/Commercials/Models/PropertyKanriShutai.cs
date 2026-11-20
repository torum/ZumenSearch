using System;
using System.Collections.Generic;
using System.Text;

using ZumenSearch.Domain.Entities.Rent.Commercials;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public class PropertyKanriShutai(Property.EnumKanriShutai key, string label)
{
    public string Label => label;

    public Property.EnumKanriShutai Key => key;
};
