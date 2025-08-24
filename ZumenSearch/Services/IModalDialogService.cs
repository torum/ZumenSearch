using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.Services;

public interface IModalDialogService
{
    void ShowUnitDialog(EditorViewModel editVM, EditorWindow editWin);
}
