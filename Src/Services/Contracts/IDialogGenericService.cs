using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.Services.Contracts;

public interface IDialogGenericService
{
    void Initialize(XamlRoot xamlRoot, Window window);

    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog();

    Task<Models.PersonSearchResultItem?> ShowLessorSelectDialog(ViewModels.Dialogs.LessorSelectViewModel viewModel);

    Task<Models.PersonSearchResultItem?> ShowBrokerSelectDialog(ViewModels.Dialogs.BrokerSelectViewModel viewModel);

    Task<RailLine?> ShowRailLineSelectDialog();

    Task<RailStation?> ShowRailStationSelectDialog(string railLineCode);
}
