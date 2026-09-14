using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

public interface IDialogGenericService
{
    void Initialize(XamlRoot xamlRoot);

    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog();

    Task<ContentDialogResult> ShowLeaveUnitDirtyConfirmationDialog();

    Task<RailLine?> ShowRailLineSelectDialog();

    Task<RailStation?> ShowRailStationSelectDialog(string railLineCode);
}
