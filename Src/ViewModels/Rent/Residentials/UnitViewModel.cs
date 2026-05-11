using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class UnitViewModel : ObservableObject
{

    private ViewModels.Rent.Residentials.MainViewModel _mainViewModel;
    private Models.Rent.Residentials.Room? _unit;

    #region == Public Properties ==

    // This flag indicates if the room is dirty (i.e., has unsaved changes).
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty {  get; private set; }

    public string RoomName
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
                //_editRoom?.IsModified = true;

                //OnPropertyChanged(nameof(WindowTitle));//TODO

                //_mainViewModel.EventTitleChanged?.Invoke(this, EventArgs.Empty);//TODO
            }
        }
    }

    #endregion

    private readonly IDataAccessService _dataAccessService;

    public UnitViewModel(ViewModels.Rent.Residentials.MainViewModel vm, IDataAccessService dataAccessService)
    {
        _mainViewModel = vm;
        _dataAccessService = dataAccessService;

        //_unit = new Room(Guid.CreateVersion7().ToString("N"));
    }

    private void SetValuesToUnit()
    {
        if (!IsDirty)
        {
            return;
        }

        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't set room.");
            return;
        }

        _unit.RoomName = RoomName;

        // TODO: More.


        //_unit.IsModified = true;
    }

    private void UpdateBldg()
    {
        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't save room.");
            return;
        }

        var existingRoom = _mainViewModel.Bldg.Rooms.FirstOrDefault(r => r.Id == _unit.Id);
        if (existingRoom is not null)
        {
            // Update existing room
            var index = _mainViewModel.Bldg.Rooms.IndexOf(existingRoom);
            _mainViewModel.Bldg.Rooms[index] = _unit;

        }
        else
        {
            // Add new room
            _mainViewModel.Bldg.Rooms.Add(_unit);
        }
    }

    #region == Public Methods ==

    public void SetEditUnit(Models.Rent.Residentials.Room room)
    {
        _unit = room;

        RoomName = _unit.RoomName; // Set the value to trigger the setter logic if needed.

        // TODO: Set other properties for editing..


        _unit.IsModified = false;
        IsDirty = false;
    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save() 
    {
        if (!IsDirty)
        {
            return;
        }

        if (_unit is null)
        {
            Debug.WriteLine("_unit is null. Can't save room.");
            return;
        }

        SetValuesToUnit();

        if (_mainViewModel.Bldg.EntryStatus == Models.EnumEntryStatus.New)
        {
            // update Bldg and done.
            UpdateBldg();

            IsDirty = false;
        }
        else
        {
            // save room directry to db.
            // 
            var resInsert = _dataAccessService.UpsertRentResidentialRoom(_mainViewModel.Id, _unit);
            if (resInsert.IsError)
            {
                Debug.WriteLine("Error on UpsertRentResidentialRoom. @Save() in Residentials.MainViewModel");
                Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

                // TODO: return error object.
                return;
            }
            else
            {
                UpdateBldg();

                IsDirty = false;
            }
        }
    }

    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
    }

    #endregion
}
