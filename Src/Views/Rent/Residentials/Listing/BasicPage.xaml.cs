using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class BasicPage : Page
{
    public ViewModels.Rent.Residentials.ListingViewModel? ViewModel { get; private set; }

    private bool _initialized;
    private bool _isSynchronizingStatusDate;

    public BasicPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel();
        InitializeComponent();

    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        this.TextBoxName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Residentials.ListingViewModel vm)
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = vm;
            Bindings.Update();
            if (!_initialized)
            {
                //Init();
            }
        }

        base.OnNavigatedTo(e);
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel?.CurrentStatusCheckedAt is DateTimeOffset checkedAt)
        {
            SetStatusDatePickerDate(checkedAt);
        }

        Init();
    }

    private void CurrentStatusDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        if (!_isSynchronizingStatusDate && ViewModel is not null)
        {
            ViewModel.CurrentStatusCheckedAt = e.NewDate;
        }
    }

    private void SetCurrentStatusDateToToday(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var clickedAt = DateTimeOffset.Now;
        SetStatusDatePickerDate(clickedAt);

        if (ViewModel is not null)
        {
            ViewModel.CurrentStatusCheckedAt = clickedAt;
        }
    }

    private void SetStatusDatePickerDate(DateTimeOffset value)
    {
        _isSynchronizingStatusDate = true;
        try
        {
            CurrentStatusDatePicker.Date = value;
        }
        finally
        {
            _isSynchronizingStatusDate = false;
        }
    }
}
