using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

public abstract class ListingBase : ObservableObject
{
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

    public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected ListingBase(string id)
    {
        Id = id;
    }
};
