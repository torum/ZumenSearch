using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

// Rent Residential listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string BuildingName { get; set; } = string.Empty;
    public Listing? Room { get; set; }
}
