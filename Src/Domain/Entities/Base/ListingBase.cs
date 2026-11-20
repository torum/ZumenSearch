#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Domain.Enums;


namespace ZumenSearch.Domain.Entities.Base;

// <summary>
// Base class for all listings entities. Aggregate Root entity is property.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class ListingBase : EntityAggregateBase
{
    protected ListingBase(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, PropertyContextType propertyKind, string fudousanId) : base(id, status)
    {
        PropertyId = propertyId;
        PropertyStatus = propertyStatus;
        PropertyContextType = propertyKind;
        FudousanId = fudousanId;
    }

    // Aggregate Root entity's ID.
    public string PropertyId { get; private set; }

    // Aggregate Root entity's status.
    // ANEMIC property, but we simply want to keep track of whether the entity has been modified since it was loaded from the database.
    public EntityStatus PropertyStatus { get; private set; } = EntityStatus.New;

    // Aggregate Root entity's property kind.
    public PropertyContextType PropertyContextType { get; init; } = PropertyContextType.Unknown;

    public string FudousanId { get; private set; }

    public void SetPropertyStatus(EntityStatus status)
    {
        PropertyStatus = status;
    }

    public void SetFudousanId(string fudousanId)
    {
        FudousanId = fudousanId;
    }

};
