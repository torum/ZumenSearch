using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Sale.Residentials;
namespace ZumenSearch.Presentation.Features.Sale.Residentials.Models;

// Sales Residential listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string BuildingName { get; set; } = string.Empty;

    public ZumenSearch.Domain.Entities.Sale.Residentials.Listing? Room { get; set; }
}

