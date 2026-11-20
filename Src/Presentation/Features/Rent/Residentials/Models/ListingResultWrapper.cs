using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Residentials;
namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

// Rent Residential listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string PropertyName { get; set; } = string.Empty;
    public Listing? Listing { get; set; }
}
