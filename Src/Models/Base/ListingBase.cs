namespace ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Entity

// <summary>
// Base class for all listings entities. Aggregate Root entity is property.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class ListingBase : EntityAggregateBase
{
    // Aggregate Root entity's ID.
    public string PropertyId { get; private set; }

    // Aggregate Root entity's status.
    // ANEMIC property, but we simply want to keep track of whether the entity has been modified since it was loaded from the database.
    public EnumEntityStatus PropertyStatus { get; set; } = EnumEntityStatus.New;

    // Aggregate Root entity's property kind.
    public EnumPropertyKind PropertyKind { get; init; } = EnumPropertyKind.Unknown;

    protected ListingBase(string id, EnumEntityStatus status, string propertyId, EnumEntityStatus propertyStatus, EnumPropertyKind propertyKind) : base(id, status)
    {
        PropertyId = propertyId;
        PropertyStatus = propertyStatus;
        PropertyKind = propertyKind;
    }
};
