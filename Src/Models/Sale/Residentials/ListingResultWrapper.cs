using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Sale.Residentials;

// Sales Residential listing
public sealed class ListingResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Sale.Residentials.Listing? Room;
}

