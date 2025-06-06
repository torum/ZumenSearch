using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZumenSearch.Models;
using ZumenSearch.Services;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class EditorViewModel : ObservableObject, INotifyPropertyChanged
{
    public Views.Rent.Residentials.Editor.EditorWindow? EditorWin
    {
        get; set;
    }

    private Models.Rent.RentResidential _entry;
    public Models.Rent.RentResidential Entry
    {
        get => _entry; 
        set
        {
            if (value != null)
            {
                _entry = value;

                if (EditorWin != null)
                {
                    // If the EditorWin is not null, set the Id property of the window to the Entry.Id.
                    EditorWin.Id = _entry.Id;
                }
                else
                {
                    // TODO: If EditorWin is null, log an error or handle it accordingly.
                    Debug.WriteLine("EditorWin is null. Cannot set Id.");
                }
            
                Name = _entry.Name;
                //TODO: Set other properties

                IsEntryDirty = false;
            }
        }
    }

    private bool _isEntryDirty;
    public bool IsEntryDirty
    {
        get => _isEntryDirty;
        set
        {
            if (SetProperty(ref _isEntryDirty, value))
            {

            }
        }
    }

    private string? _name;
    public string Name
    {
        get => _name ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref _name, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    //private readonly MainViewModel _viewModel = App.GetService<MainViewModel>();

    //
    public event EventHandler<string>? EventAddNew;
    //
    public event EventHandler<string>? EventGoBack;
    public event EventHandler<string>? EventBackToSummary;
    //
    public event EventHandler<string>? EventEditStructure;
    public event EventHandler<string>? EventEditLocation;
    public event EventHandler<string>? EventEditTransportation;
    public event EventHandler<string>? EventEditAppliance;


    private readonly IDataAccessService _dataAccessService;

    public EditorViewModel(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;

        //
        _entry = new Models.Rent.RentResidential();

    }

    private RelayCommand? saveCommand;

    public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(Save);

    private async void Save()
    {
        if (IsEntryDirty)
        {
            Entry.Name = Name;
            //TODO: Set other properties

            if (string.IsNullOrEmpty(Entry.Id))
            {
                // If the Entry.Id is empty, generate a new ID.
                Entry.SetId = Guid.NewGuid().ToString();
                await SaveAsNewAsync();
            }
            else
            {
                await SaveAsUpdate();
            }
        }
    }

    private Task SaveAsNewAsync()
    {
        var resInsert = _dataAccessService.InsertRentResidential(Entry.Id, Entry.Name, "some comment");
        if (resInsert.IsError)
        {
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            IsEntryDirty = false;
            Entry.IsDirty = false;
            Debug.WriteLine("No errors on insert.");
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    private Task SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(Entry.Id, Entry.Name, "some update comment");
        if (resInsert.IsError)
        {
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            IsEntryDirty = false;
            Entry.IsDirty = false;
            Debug.WriteLine("No errors on update.");

            /*
            //UpdateSource()
            if (_viewModel.RentResidentialSearchResult.Contains(Entry))
            {
                // If the Entry is already in the search result, update it in the list.
                var index = _viewModel.RentResidentialSearchResult.IndexOf(Entry);
                if (index >= 0)
                {
                    _viewModel.RentResidentialSearchResult[index] = Entry;
                }
            }
            */
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    // Add New Modal window command
    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
        EventAddNew?.Invoke(this, "asdf");
    }

    // Go Back command (don't use this?)
    private RelayCommand? goBackCommand;

    public IRelayCommand BackCommand => goBackCommand ??= new RelayCommand(GoBack);

    private void GoBack()
    {
        EventGoBack?.Invoke(this, "asdf");
    }

    private RelayCommand? backToSummaryCommand;

    public IRelayCommand BackToSummaryCommand => backToSummaryCommand ??= new RelayCommand(GoBackToSummary);

    private void GoBackToSummary()
    {
        EventBackToSummary?.Invoke(this, "asdf");
    }

    private RelayCommand? editStructureCommand;
    public IRelayCommand EditStructureCommand => editStructureCommand ??= new RelayCommand(EditStructure);

    private void EditStructure()
    {
        EventEditStructure?.Invoke(this, "asdf");
    }

    private RelayCommand? editLocationCommand;
    public IRelayCommand EditLocationCommand => editLocationCommand ??= new RelayCommand(EditLocation);

    private void EditLocation()
    {
        EventEditLocation?.Invoke(this, "asdf");
    }

    private RelayCommand? editTransportationCommand;
    public IRelayCommand EditTransportationCommand => editTransportationCommand ??= new RelayCommand(EditTransportation);

    private void EditTransportation()
    {
        EventEditTransportation?.Invoke(this, "asdf");
    }

    private RelayCommand? editApplianceCommand;
    public IRelayCommand EditApplianceCommand => editApplianceCommand ??= new RelayCommand(EditAppliance);

    private void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, "asdf");
    }
}
