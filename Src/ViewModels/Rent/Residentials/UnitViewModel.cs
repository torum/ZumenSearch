using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class UnitViewModel : ObservableObject
{
    #region == Private variables ==
    
    private ViewModels.Rent.Residentials.MainViewModel _mainViewModel;
    private Models.Rent.Residentials.UnitResidential? _unit;
    
    #endregion

    #region == Public Properties ==

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty {  get; private set; }

    /*
    public bool IsDirty
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                _mainViewModel.IsDirty = true;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
    */

    [ObservableProperty]
    public partial bool HasErrors { get; private set; }

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

    public string Chinryou
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            if (value.Equals("0"))
            {
                field = "";
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
            }

            OnPropertyChanged();
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

        if (string.IsNullOrEmpty(RoomName))
        {
            // TODO: Show InfoBar?
            HasErrors = true;
            return;
        }

        _unit.Name = RoomName;

        if (int.TryParse(Chinryou, out var result))
        {
            if (result > -1)
            {
                _unit.Chinryou = result;
            }
            else
            {
                Debug.WriteLine("整数変換に失敗。（マイナス）");
            }
        }



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

    private void PopulateUnitValues()
    {
        if (_unit is null)
        {
            return;
        }

        RoomName = _unit.Name; // Set the value to trigger the setter logic if needed.

        var test = _unit.Chinryou.ToString();
        Chinryou = _unit.Chinryou.ToString();


        // TODO: Set other properties for editing..


        _unit.IsModified = false;
        IsDirty = false;
    }

    #region == Public Methods ==

    public void SetEditUnit(Models.Rent.Residentials.UnitResidential room)//SetEditUnit //PopulateUnitValues
    {
        _unit = room;

        PopulateUnitValues();
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

        if (_mainViewModel.Bldg.EntryStatus == EnumEntryStatus.New)
        {
            // update Bldg and done.
            UpdateBldg();

            IsDirty = false;
        }
        else
        {
            // save room directry to db.
            // 
            var resInsert = _dataAccessService.UpsertRentResidentialUnit(_mainViewModel.Id, _unit);
            if (resInsert.IsError)
            {
                Debug.WriteLine("Error on UpsertRentResidentialUnit. @Save() in Residentials.MainViewModel");
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
