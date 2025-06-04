using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class SummaryViewModel : ObservableRecipient
{
    public SummaryViewModel()
    {
        Debug.WriteLine("ViewModels.Rent.Residentials.Editor.SummaryViewModel init!");
    }

    public event EventHandler<string>? EventEditStructure;
    public event EventHandler<string>? EventEditLocation;
    public event EventHandler<string>? EventEditTransportation;
    public event EventHandler<string>? EventEditAppliance;

    private RelayCommand? editStructureCommand;
    public IRelayCommand EditStructureCommand => editStructureCommand ??= new RelayCommand(EditStructure);

    private void EditStructure()
    {
        Debug.WriteLine("EditStructure command executed!");
        // Navigate to the SummaryPage
        EventEditStructure?.Invoke(this, "asdf");
    }

    private RelayCommand? editLocationCommand;
    public IRelayCommand EditLocationCommand => editLocationCommand ??= new RelayCommand(EditLocation);

    private void EditLocation()
    {
        Debug.WriteLine("EditLocation command executed!");
        // Navigate to the SummaryPage
        EventEditLocation?.Invoke(this, "asdf");
    }

    private RelayCommand? editTransportationCommand;
    public IRelayCommand EditTransportationCommand => editTransportationCommand ??= new RelayCommand(EditTransportation);

    private void EditTransportation()
    {
        Debug.WriteLine("EditTransportation command executed!");
        // Navigate to the SummaryPage
        EventEditTransportation?.Invoke(this, "asdf");
    }

    private RelayCommand? editApplianceCommand;
    public IRelayCommand EditApplianceCommand => editApplianceCommand ??= new RelayCommand(EditAppliance);

    private void EditAppliance()
    {
        Debug.WriteLine("EditAppliance command executed!");
        // Navigate to the SummaryPage
        EventEditAppliance?.Invoke(this, "asdf");
    }
}
