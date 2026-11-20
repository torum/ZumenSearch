using System;
using System.Collections.Generic;
using System.Text;

using ZumenSearch.Domain.Entities.Rent.Residentials;
namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public class PropertyKanriShutai(Property.EnumKanriShutai key, string label)
{
    public string Label => label;

    public Property.EnumKanriShutai Key => key;
};
