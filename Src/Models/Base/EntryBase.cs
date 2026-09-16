using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumEntryStatus
{
    Saved,
    New,
}

public abstract class EntryBase : ObservableObject
{
    public EnumEntryStatus Status { get; set; } = EnumEntryStatus.New;

    public string Id { get; private set; } = string.Empty;

    public bool IsModified { get; set; } = false;

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

    protected EntryBase(string id)
    {
        Id = id;
    }

    #region == Public Methods ==

    public void ClearId()
    {
        Id = string.Empty;
    }

    public void SetId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        }

        Id = id;
    }

    #endregion
};
