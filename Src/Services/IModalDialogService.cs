using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models;

namespace ZumenSearch.Services;

public interface IModalDialogService
{
    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog(Window win);

    Task<RailLine?> ShowRailLineSelectDialog(Window win);

    Task<RailStation?> ShowRailStationSelectDialog(Window win, string railLineCode);
}
