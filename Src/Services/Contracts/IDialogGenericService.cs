using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

public interface IDialogGenericService
{
    void Initialize(XamlRoot xamlRoot, Window window);

    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog();

    Task<Models.Common.PersonSearchResultItem?> ShowLessorSelectDialog(ViewModels.Dialogs.LessorSelectViewModel viewModel);

    Task<RailLine?> ShowRailLineSelectDialog();

    Task<RailStation?> ShowRailStationSelectDialog(string railLineCode);
}
