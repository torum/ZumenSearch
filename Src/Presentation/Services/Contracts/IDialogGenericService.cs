using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Domain.ValueObjects.Transportation;
namespace ZumenSearch.Presentation.Services.Contracts;

public interface IDialogGenericService
{
    void Initialize(XamlRoot xamlRoot, Window window);

    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog();

    Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message);

    Task<ZumenSearch.Application.SearchResults.PersonSearchResultItem?> ShowLessorSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel viewModel);

    Task<ZumenSearch.Application.SearchResults.PersonSearchResultItem?> ShowBrokerSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel viewModel);

    Task<RailLine?> ShowRailLineSelectDialog();

    Task<RailStation?> ShowRailStationSelectDialog(string railLineCode);
}
