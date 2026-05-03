using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models;

public abstract class UnitBase : ObservableObject
{
    public string Id { get; set; } = string.Empty;

    public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected UnitBase(string id)
    {
        Id = id;
    }
};
