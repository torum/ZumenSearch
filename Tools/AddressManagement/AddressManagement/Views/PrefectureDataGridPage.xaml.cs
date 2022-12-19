using System.Collections.ObjectModel;
using AddressManagement.Core.Models;
using AddressManagement.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;

namespace AddressManagement.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class PrefectureDataGridPage : Page
{
    public PrefectureCodeDataGridViewModel ViewModel
    {
        get;
    }

    public PrefectureDataGridPage()
    {
        ViewModel = App.GetService<PrefectureCodeDataGridViewModel>();
        InitializeComponent();
    }


}
