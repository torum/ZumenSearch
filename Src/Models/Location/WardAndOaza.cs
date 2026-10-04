using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Location;

public sealed class WardAndOaza(string machiazaId, string ward, string oaza)
{
    // db:location_machiaza_id
    public string MachiazaId { get; init; } = machiazaId;

    // db:location_ward
    public string Ward { get; init; } = ward;

    // db:location_oaza_cho
    public string Oaza { get; init; } = oaza;

    public string Combined
    {
        get
        {
            if (string.IsNullOrEmpty(Ward) && string.IsNullOrEmpty(Oaza))
            {
                return "";//該当なし
            }
            else
            {
                return Ward + Oaza;
            }
        }
    }
}
