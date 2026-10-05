using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

// Rent Residential
public sealed class PropertyResultWrapper : ResultWrapperBase
{
    public Property? Building { get; set; }
}
