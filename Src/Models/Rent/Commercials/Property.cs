using System.Globalization;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed partial class Property : PropertyBase
{
    public Kind CommercialKind
    {
        get => field ?? new(EnumCommercialKinds.Unspecified);
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

    public Structure BuildingStructure
    {
        get => field ?? new(EnumStructures.Unspecified);
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

    public decimal TotalFloorArea
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

    public Property(string id, EnumEntryStatus status)
        : base(id, status, EnumPropertyKind.RentCommercial)
    {
    }

    public void SetCommercialKindFromString(string value)
    {
        CommercialKind = Enum.TryParse(
            value,
            out EnumCommercialKinds result)
            ? new Kind(result)
            : new Kind(EnumCommercialKinds.Unspecified);
    }

    public void SetStructureTypeFromString(string value)
    {
        BuildingStructure = Enum.TryParse(
            value,
            out EnumStructures result)
            ? new Structure(result)
            : new Structure(EnumStructures.Unspecified);
    }

    public void SetBuildYearMonthFromString(string value)
    {
        BuiltYearAndMonth = string.IsNullOrWhiteSpace(value)
            ? new DateTimeOffset(
                1900,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero)
            : DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture);
    }
}