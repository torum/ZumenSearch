using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Domain.Entities.Base;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// TODO: See if reuse this with broker

// TODO:
/*
public sealed partial class Person : PersonBase
{
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

    public Person(string id, EnumEntityStatus status) : base(id, status, EnumPersonKind.Undetermined)
    {
        //IsModified = false;
    }

};
*/

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PropertyPersonViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel ParentViewModel { get;  }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("PropertyPersonViewModel's Person updated");
            }
        }
    }

    public PropertyPersonViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel parentPropertyViewModel)
    {
        Person = person;
        ParentViewModel = parentPropertyViewModel;
    }
}

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class ListingPersonViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel ParentViewModel { get; }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("ListingPersonViewModel's Person updated");
            }
        }
    }

    public ListingPersonViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel parentListingViewModel)
    {
        Person = person;
        ParentViewModel = parentListingViewModel;
    }
}