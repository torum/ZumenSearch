using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using ZumenSearch.Models;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor.Modal;

public partial class ModalViewModel : ObservableRecipient
{
    public Views.Rent.Residentials.Editor.Modal.ModalWindow? ModalWin
    {
        get; set;
    }


    private object? _selected;
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public event EventHandler<string>? EventBackToSummary;

    public ModalViewModel()
    {

    }

    private RelayCommand? saveCommand;

    public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(SaveAsync);

    public void SaveAsync()
    {
        // TODO: (make async)
        Debug.WriteLine("SaveAsync() called in ModalViewModel");
    }

    private RelayCommand? backToSummaryCommand;

    public IRelayCommand BackToSummaryCommand => backToSummaryCommand ??= new RelayCommand(GoBackToSummary);

    public void GoBackToSummary()
    {
        EventBackToSummary?.Invoke(this, "asdf");
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        /*
        //IsBackEnabled = NavigationService.CanGoBack;
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
        */
    }

    public bool Closing()
    {
        // TODO: check if dirty.

        return true;
    }
}
