using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

public enum EnumPropertyStatus
{
    Saved,
    New,
}

public abstract partial class PropertyBase : ObservableObject
{
    // TODO: I don't think we need this anymore....
    public bool IsDirty
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    protected private string _id;
    public string Id => _id;

    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    #region == 所在地 ==

    public string LocPrefId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocPrefecture
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocMachiazaId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCounty
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCity
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocWard
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocOazaCho
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocChoume
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocEdaban
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocLocationFull
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    #endregion

    protected PropertyBase(string id)
    {
        _id = id;
    }

    #region == Public Methods ==

    public void ClearId()
    {
        _id = string.Empty;
    }

    public void SetId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        }

        _id = id;
    }

    #endregion
}
