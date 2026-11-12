
using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Sale.Residentials;


// Sales Residential
public sealed class PropertyResultWrapper : ResultWrapperBase
{
    public Models.Sale.Residentials.Property? Building { get; set; }
}
