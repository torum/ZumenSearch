using System;
using System.Diagnostics;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Models;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels.Rent.Residentials.Modal;

public partial class ModalViewModel : ObservableRecipient
{
    public Views.Rent.Residentials.Editor.Modal.ModalWindow? ModalWin
    {
        get; set;
    }

    private readonly string _windowTitleBase = "賃貸住居用";

    public string WindowTitle
    {
        get
        {
            if (string.IsNullOrEmpty(_name))
            {
                return $"{_windowTitleBase} - 部屋";
            }
            else
            {
                return $"{_windowTitleBase} - {_name} - 部屋";
            }
        }
    }

    private string? _name;
    public string Name
    {
        get => _name ?? string.Empty; 
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
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

    public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(Save);

    public void Save()
    {
        // TODO: (make async)
        Debug.WriteLine("Save() called in ModalViewModel");
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
