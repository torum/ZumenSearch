using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class ListingBase : EntryBase
{
    public EnumEntryStatus PropertyStatus { get; set; } = EnumEntryStatus.New;

    protected ListingBase(string id, EnumEntryStatus status) : base(id, status)
    {
        //
    }
};
