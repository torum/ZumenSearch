using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

// 検索結果一覧表示用（建物）
internal sealed partial class EntryResidentialSearchResult : EntryBase
{
    // TODO:

    public EntryResidentialSearchResult(string id) : base(id)
    {
        //
    }
}

// 編集用（建物）
internal sealed partial class EntryResidential : EntryBase
{
    // ステータス（保存済みか新規か）
    public EnumEntryStatus EntryStatus { get; set; }

    // 物件に属する部屋のリスト
    public ObservableCollection<UnitResidential> Rooms
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき部屋のIDリスト
    public ObservableCollection<UnitResidential> RoomsToBeDeleted = [];

    // 物件写真（建物）リスト
    public ObservableCollection<PictureBldg> BuildingPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true; //?
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（建物）のIDリスト
    public ObservableCollection<PictureBldg> BuildingPicturesToBeDeleted = [];

    // 図面（建物）リスト
    public ObservableCollection<PdfBldg> BuildingPdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<PdfBldg> BuildingPdfsToBeDeleted = [];

    // 物件種別
    public Models.Rent.Residentials.Kind BuildingKind
    {
        get => field ?? new(Models.Rent.Residentials.EnumKinds.Unspecified, "未指定");
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 区分所有か一括所有か
    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 建物構造
    public Models.Rent.Residentials.Structure BuildingStructure
    {
        get => field ?? new(Models.Rent.Residentials.EnumStructure.Unspecified, "未指定");
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 地上階
    public string AboveGroundFloorCount
    {
        get => field ?? string.Empty;
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                IsDirty = true;
            }

            OnPropertyChanged();
        }
    }

    // 地下階
    public string BasementFloorCount
    {
        get => field ?? string.Empty;
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                IsDirty = true;
            }

            OnPropertyChanged();
        }
    }

    // 総戸数
    public string TotalUnitCount
    {
        get => field ?? string.Empty;
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                IsDirty = true;
            }

            OnPropertyChanged();
        }
    }

    // 築年月
    public DateTimeOffset? BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged();
            }
        }
    }

    // 不動産ID (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            // TODO: check 13桁.

            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        set
        {
            // TODO: check ４桁.

            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    } = "0000";

    // 備考
    public string Remarks
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    // TODO: More.


    public EntryResidential(string id, EnumEntryStatus status) : base(id)
    {
        EntryStatus = status;
    }
}
