using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

internal abstract class UnitBase : ObservableObject
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

    protected UnitBase(string id)
    {
        Id = id;
    }
};
