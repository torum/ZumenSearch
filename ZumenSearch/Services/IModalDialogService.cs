using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials;
using ZumenSearch.Views.Rent.Residentials.Bldg;
using static ZumenSearch.Services.ModalDialogService;

namespace ZumenSearch.Services;

public interface IModalDialogService
{
    Task<ContentDialogResult> ShowEditorCloseConfirmationDialog(Window win);

    Task<RailLine?> ShowRailLineSelectDialog(Window win);

    Task<RailStation?> ShowRailStationSelectDialog(Window win, string railLineCode);
}
