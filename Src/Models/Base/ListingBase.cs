namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class ListingBase : Entity
{
    // 物件（建物のIDを保持）
    public string PropertyId { get; private set; }

    public EnumEntityStatus PropertyStatus { get; set; } = EnumEntityStatus.New;

    public EnumPropertyKind PropertyKind { get; init; } = EnumPropertyKind.Unknown;

    protected ListingBase(string id, EnumEntityStatus status, string propertyId, EnumEntityStatus propertyStatus, EnumPropertyKind propertyKind) : base(id, status)
    {
        PropertyId = propertyId;
        PropertyStatus = propertyStatus;
        PropertyKind = propertyKind;
    }
};
