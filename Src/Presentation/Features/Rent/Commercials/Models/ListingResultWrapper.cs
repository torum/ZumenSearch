using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Commercials;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

// Rent Commercial listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string BuildingName { get; set; } = string.Empty;

    public ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing? Unit { get; set; }
}

