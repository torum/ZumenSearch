using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public abstract class PersonBase : EntryBase
{
    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                Name = $"{NameLast} {NameFirst}";
                IsModified = true;
            }
        }
    }

    public string NameLast
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                Name = $"{NameLast} {NameFirst}";
                IsModified = true;
            }
        }
    }

    protected PersonBase(string id, EnumEntryStatus status) : base(id, status)
    {
        //
    }
};
