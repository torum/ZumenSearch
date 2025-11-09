using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.Services;

public interface IModalDialogService
{
    void ShowUnitDialog(ResidentialsViewModel editVM, EditorWindow editWin);

    Task<RailLine?> ShowRailLineSelectDialog(Window win);

    Task<RailStation?> ShowRailStationSelectDialog(Window win, string railLineCode);
}
