using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

/*
public enum EnumListingStatus
{
    Saved,
    New,
}
*/

public abstract class ListingBase : EntryBase
{
    public EnumEntryStatus PropertyStatus { get; set; } = EnumEntryStatus.New;
    //public EnumEntryStatus ListingStatus { get; set; } = EnumEntryStatus.New;

    //public bool IsModified { get; set; } = false;

    //public string Id { get; }

    /*
    public string Name
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
    */

    protected ListingBase(string id, EnumEntryStatus status) : base(id)
    {
        //Id = id;
        Status = status;
    }
};
