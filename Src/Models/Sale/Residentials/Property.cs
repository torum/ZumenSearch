using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Sale.Residentials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Required for clarity. And I don't care about Visual Basic Keywords.")]
public sealed partial class Property : PropertyBase
{
    public PropertyKind BuildingKind
    {
        get => field ?? new(PropertyType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public PropertyStructure BuildingStructure
    {
        get => field ?? new(StructureType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int FloorCountAboveGround
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int FloorCountBasement
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int TotalUnitCount
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public DateTimeOffset BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public string Remarks
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public Property(string id, EntityStatus status)
        : base(id, status, Enums.PropertyKind.SaleResidential)
    {
    }

    public void SetKindTypeFromString(string value)
    {
        BuildingKind = Enum.TryParse(value, out PropertyType result)
            ? new PropertyKind(result)
            : new PropertyKind(PropertyType.Unspecified);
    }

    public void SetStructureTypeFromString(string value)
    {
        BuildingStructure = Enum.TryParse(value, out StructureType result)
            ? new PropertyStructure(result)
            : new PropertyStructure(StructureType.Unspecified);
    }

    public void SetBuildYearMonthFromString(string value)
    {
        BuiltYearAndMonth = string.IsNullOrWhiteSpace(value)
            ? new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero)
            : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
    }
}
