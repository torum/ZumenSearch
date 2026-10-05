using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Enums;

// <summary>
// Represents the status of an entity in the application, indicating whether it is saved in the database or new.
// It is used to track the state of entities such as properties, listings, persons, pictures, and PDFs.
// </summary>
// <remarks>
// </remarks>
public enum EnumEntityStatus
{
    Saved,
    New,
}
