using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class ListingBase : EntryBase
{
    // 物件（建物のIDを保持）
    public string PropertyId { get; private set; }

    public EnumEntryStatus PropertyStatus { get; set; } = EnumEntryStatus.New;

    public EnumPropertyKind PropertyKind { get; init; } = EnumPropertyKind.Unknown;

    protected ListingBase(string id, EnumEntryStatus status, string propertyId, EnumEntryStatus propertyStatus, EnumPropertyKind propertyKind) : base(id, status)
    {
        PropertyId = propertyId;
        PropertyStatus = propertyStatus;
        PropertyKind = propertyKind;
    }
};
