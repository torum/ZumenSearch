using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Location;

public sealed class Choume(string machiazaId, string choume)
{
    // db:location_machiaza_id
    public string MachiazaId { get; init; } = machiazaId;

    // db:location_choume
    public string Chou { get; init; } = choume;
}
