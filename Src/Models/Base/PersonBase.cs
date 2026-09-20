using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public enum EnumPersonKind
{
    Natural,
    Legal,
    Undetermined
}

public abstract class PersonBase : EntryBase
{
    public EnumPersonKind PersonKind { get; set; }

    // Do not use SetProperty.
    public string Remarks
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                field = string.Empty;
            }

            OnPropertyChanged();
        }
    } = string.Empty;

    protected PersonBase(string id, EnumEntryStatus status, EnumPersonKind personKind) : base(id, status)
    {
        PersonKind = personKind;
    }
};
