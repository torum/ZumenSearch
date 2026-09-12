using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumListingStatus
{
    Saved,
    New,
}

public abstract class ListingBase : ObservableObject
{
    // TODO: Clarify the difference between IsNew and ListingStatus.
    // IsNew indicates whether the listing is newly created, while ListingStatus indicates whether the listing has been saved to the database or not.
    // ステータス（保存済みか新規か）
    public EnumPropertyStatus PropertyStatus { get; set; } = EnumPropertyStatus.New;
    public EnumListingStatus ListingStatus { get; set; } = EnumListingStatus.New;

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

    //public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected ListingBase(string id)
    {
        Id = id;
    }
};
