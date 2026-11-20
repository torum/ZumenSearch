using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Commercials;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

// Rent Commercial
public sealed class PropertyResultWrapper : ResultWrapperBase
{
    public ZumenSearch.Domain.Entities.Rent.Commercials.Property? Property { get; set; }
    public List<ListingSummary> ListingSummaries { get; } = [];
}
