using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.Views.Rent;

public sealed partial class ResidentialSearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public ListingSearchCriteria SearchCriteria { get; } = new();

    public IReadOnlyList<PropertyKindTypeLabel> PropertyKinds { get; } = PropertyKindTypeLabel.GetAll();
    public IReadOnlyList<PropertyStructureTypeLabel> PropertyStructures { get; } = PropertyStructureTypeLabel.GetAll();
    public IReadOnlyList<ListingFloorPlanTypeLabel> FloorPlans { get; } = ListingFloorPlanTypeLabel.GetAll();
    public IReadOnlyList<ListingHandlingTypeLabel> HandlingTypes { get; } = ListingHandlingTypeLabel.GetAll();
    public IReadOnlyList<ListingTransactionTypeLabel> TransactionTypes { get; } = ListingTransactionTypeLabel.GetAll();
    public IReadOnlyList<ListingCommissionUnitTypeLabel> CommissionUnits { get; } = ListingCommissionUnitTypeLabel.GetAll();

    private readonly INavigationService _navigationService;

    public ResidentialSearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void SearchOption_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
        {
            return;
        }

        switch (checkBox.Tag)
        {
            case PropertyKindType propertyKind:
                SearchCriteria.PropertyKinds.Add(propertyKind);
                break;
            case StructureType propertyStructure:
                SearchCriteria.PropertyStructures.Add(propertyStructure);
                break;
            case FloorPlanType floorPlan:
                SearchCriteria.FloorPlans.Add(floorPlan);
                break;
        }
    }

    private void SearchOption_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
        {
            return;
        }

        switch (checkBox.Tag)
        {
            case PropertyKindType propertyKind:
                SearchCriteria.PropertyKinds.Remove(propertyKind);
                break;
            case StructureType propertyStructure:
                SearchCriteria.PropertyStructures.Remove(propertyStructure);
                break;
            case FloorPlanType floorPlan:
                SearchCriteria.FloorPlans.Remove(floorPlan);
                break;
        }
    }

    private void BasicFieldsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        bool useTwoColumns = e.NewSize.Width >= 640;
        BasicSecondColumn.Width = useTwoColumns ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        BasicFieldsGrid.ColumnSpacing = useTwoColumns ? 16 : 0;

        Grid.SetRow(AddressTextBox, useTwoColumns ? 0 : 1);
        Grid.SetColumn(AddressTextBox, useTwoColumns ? 1 : 0);
        Grid.SetRow(StationTextBox, useTwoColumns ? 1 : 2);
        Grid.SetRow(LineTextBox, useTwoColumns ? 1 : 3);
        Grid.SetColumn(LineTextBox, useTwoColumns ? 1 : 0);
        Grid.SetRow(LessorTextBox, useTwoColumns ? 2 : 4);
        Grid.SetRow(RealEstateIdTextBox, useTwoColumns ? 2 : 5);
        Grid.SetColumn(RealEstateIdTextBox, useTwoColumns ? 1 : 0);
    }
}
