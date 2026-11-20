
using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Sale.Residentials;
namespace ZumenSearch.Presentation.Features.Sale.Residentials.Models;


// Sales Residential
public sealed class PropertyResultWrapper : ResultWrapperBase
{
    public ZumenSearch.Domain.Entities.Sale.Residentials.Property? Building { get; set; }
}
