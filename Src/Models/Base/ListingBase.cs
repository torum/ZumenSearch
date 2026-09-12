using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumListingStatus
{
    Saved,
    New,
}

public abstract class ListingBase : ObservableObject // TODO: Remove ObservableObject?
{
    public EnumPropertyStatus PropertyStatus { get; set; } = EnumPropertyStatus.New;
    public EnumListingStatus ListingStatus { get; set; } = EnumListingStatus.New;

    public bool IsModified { get; set; } = false;

    public string Id { get; }

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

    protected ListingBase(string id)
    {
        Id = id;
    }
};
