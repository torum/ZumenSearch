using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ZumenSearch.Models.Base;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Models.Rent.Residentials;

public partial class UnitResidentialSearchResult : UnitBase
{
    public string EntryId { get; init; }

    public string EntryName
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    public UnitResidentialSearchResult(string id, string entryId) : base(id)
    {
        EntryId = entryId;
    }
}

public partial class UnitResidential : UnitBase
{
    // 物件写真（部屋）リスト
    public ObservableCollection<PictureUnit> UnitPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;//?
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（部屋）のIDリスト
    public ObservableCollection<PictureUnit> UnitPicturesToBeDeleted = [];

    public int Chinryou
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value > -1) 
            {
                field = value;
                OnPropertyChanged();
                IsModified = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
                throw new ArgumentOutOfRangeException("Chinryou", "Must be at least 0.");
            }
        }
    }

    public UnitResidential(string id) : base(id)
    {
        //
    }
}
