using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

// Rent Commercial listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Rent.Commercials.Listing.Listing? Unit;
}

