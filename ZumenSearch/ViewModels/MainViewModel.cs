using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

/// ■ TODO:
///
/// 不動産IDの入力欄
/// DataAccessのAsync化
/// エラー処理、及びログ保存
/// 

/// ● 履歴：
/// v0.0.0.23 VS2022に更新。ダークテーマ化し、レイアウト含め諸々変更。
/// v0.0.0.22 部屋画面のデザイン途中。区分所有の表示切り替え。
/// v0.0.0.21 PDF追加画面と建物画面での表示（とりあえず）。
/// v0.0.0.20 建物編集画面をリデザイン。
/// v0.0.0.19 Round corner 化。PDF編集画面途中。
/// v0.0.0.18 Renamingの置換もろもろ。画像編集画面でのIsDirty修正（IsEdit追加）
/// v0.0.0.17 クリーンアップ。
/// v0.0.0.16 Windowの表示位置保存を修正。
/// v0.0.0.16 画像編集Windowを追加し、画像の追加、一覧、編集、削除のサイクルは一旦完了。PDF画面途中。部屋編集画面と保存の流れを変更。他。
/// v0.0.0.15 MAXIMIZE時のタスクバー被りのFix
/// v0.0.0.14 ステータスバー追加等、もろもろ
/// v0.0.0.13 もろもろ
/// v0.0.0.11 サイズ・レイアウト
/// 

/// ◆ 後で・検討中： 
/// 〒・住所・Geo関連のデータを入力補完
/// XMLでインポート・エクスポート
/// RESTful API, Server, P2P and beyond...
/// Webサーバーを作り、会社内共有
/// （サーバー機能を追加して、同一ネットワーク内のローカルIPアドレスを指定してお互いに物件をフェッチし合えるようにする・・重複対策は不動産ID?・・・）
/// 技術的に対応できるならば、P2Pで会社間共有
/// 

namespace ZumenSearch.ViewModels
{

    public class MainViewModel : ViewModelBase
    {

        #region == 基本 ==

        // Application version.
        private const string _appVer = "0.0.0.22";

        // Application name.
        private const string _appName = "ZumenSearch";

        // Application config file folder name aka "publisher".
        private const string _appDeveloper = "torum";

        // Application Window Title.
        public string AppTitle
        {
            get
            {
                return _appName;
            }
        }

        public string AppTitleVer
        {
            get
            {
                return _appName + " v" + _appVer;
            }
        }

        #endregion

        #region == SQLite データベース ==

        // SQLite DB へのデータアクセスの共通モジュール
        private readonly DataAccess dataAccessModule = new DataAccess();

        public SqliteConnectionStringBuilder connectionStringBuilder;

        #endregion

        #region == 物件関連オブジェクト ==

        public ObservableCollection<RentLivingSummary> RentSearchResult { get; } = new ObservableCollection<RentLivingSummary>();

        #region == 賃貸住居用 ==

        private RentLivingSummary _rentLivingEditSelectedItem;
        public RentLivingSummary RentLivingEditSelectedItem
        {
            get
            {
                return _rentLivingEditSelectedItem;
            }
            set
            {
                if (_rentLivingEditSelectedItem == value) return;

                _rentLivingEditSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingEditSelectedItem");
            }
        }

        private RentLivingRoom _rentLivingNewSectionSelectedItem;
        public RentLivingRoom RentLivingNewSectionSelectedItem
        {
            get
            {
                return _rentLivingNewSectionSelectedItem;
            }
            set
            {
                if (_rentLivingNewSectionSelectedItem == value) return;

                _rentLivingNewSectionSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingNewSectionSelectedItem");
            }
        }

        private RentLiving _rentLivingNew = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLiving RentLivingNew
        {
            get
            {
                return _rentLivingNew;
            }
            set
            {
                if (_rentLivingNew == value) return;

                _rentLivingNew = value;
                this.NotifyPropertyChanged("RentLivingNew");
            }
        }

        private RentLiving _rentLivingEdit = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLiving RentLivingEdit
        {
            get
            {
                return _rentLivingEdit;
            }
            set
            {
                if (_rentLivingEdit == value) return;

                _rentLivingEdit = value;
                this.NotifyPropertyChanged("RentLivingEdit");
            }
        }

        private RentLivingRoom _rentLivingNewSectionNew = new RentLivingRoom(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingRoom RentLivingNewSectionNew
        {
            get
            {
                return _rentLivingNewSectionNew;
            }
            set
            {
                if (_rentLivingNewSectionNew == value) return;

                _rentLivingNewSectionNew = value;
                this.NotifyPropertyChanged("RentLivingNewSectionNew");
            }
        }

        private RentLivingRoom _rentLivingNewSectionEdit = new RentLivingRoom(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingRoom RentLivingNewSectionEdit
        {
            get
            {
                return _rentLivingNewSectionEdit;
            }
            set
            {
                if (_rentLivingNewSectionEdit == value) return;

                _rentLivingNewSectionEdit = value;
                this.NotifyPropertyChanged("RentLivingNewSectionEdit");
            }
        }

        #endregion

        #region == 元付け業者 ==

        public ObservableCollection<Agency> Agencies { get; } = new ObservableCollection<Agency>();

        // 元付け業者　一覧の選択されたオブジェクトを保持
        private Agency _agenciesSelectedItem;
        public Agency AgenciesSelectedItem
        {
            get
            {
                return _agenciesSelectedItem;
            }
            set
            {
                if (_agenciesSelectedItem == value) return;

                _agenciesSelectedItem = value;
                this.NotifyPropertyChanged("AgenciesSelectedItem");
            }
        }

        private Agency _agencyEdit = new Agency(Guid.NewGuid().ToString());
        public Agency AgencyEdit
        {
            get
            {
                return _agencyEdit;
            }
            set
            {
                if (_agencyEdit == value) return;

                _agencyEdit = value;
                this.NotifyPropertyChanged("AgencyEdit");
            }
        }

        #endregion

        #region == 管理会社 ==

        public ObservableCollection<MaintenanceCompany> MaintenanceCompanies { get; } = new ObservableCollection<MaintenanceCompany>();

        private MaintenanceCompany _maintenanceCompaniesSelectedItem;
        public MaintenanceCompany MaintenanceCompaniesSelectedItem
        {
            get
            {
                return _maintenanceCompaniesSelectedItem;
            }
            set
            {
                if (_maintenanceCompaniesSelectedItem == value) return;

                _maintenanceCompaniesSelectedItem = value;
                this.NotifyPropertyChanged("MaintenanceCompaniesSelectedItem");
            }
        }

        // 管理会社
        private MaintenanceCompany _maintenanceCompanyEdit = new MaintenanceCompany(Guid.NewGuid().ToString());
        public MaintenanceCompany MaintenanceCompanyEdit
        {
            get
            {
                return _maintenanceCompanyEdit;
            }
            set
            {
                if (_maintenanceCompanyEdit == value) return;

                _maintenanceCompanyEdit = value;
                this.NotifyPropertyChanged("MaintenanceCompanyEdit");
            }
        }

        #endregion

        #region == オーナー ==

        public ObservableCollection<Owner> Owners { get; } = new ObservableCollection<Owner>();

        private Owner _ownersSelectedItem;
        public Owner OwnersSelectedItem
        {
            get
            {
                return _ownersSelectedItem;
            }
            set
            {
                if (_ownersSelectedItem == value) return;

                _ownersSelectedItem = value;
                this.NotifyPropertyChanged("OwnersSelectedItem");
            }
        }

        private Owner _ownderEdit = new Owner(Guid.NewGuid().ToString());
        public Owner OwnerEdit
        {
            get
            {
                return _ownderEdit;
            }
            set
            {
                if (_ownderEdit == value) return;

                _ownderEdit = value;
                this.NotifyPropertyChanged("OwnerEdit");
            }
        }

        #endregion

        #endregion

        #region == 表示切替のフラグ ==

        // 賃貸住居用　検索結果のTabControlインデックス切り替え
        private int _rentLivingSearchTabSelectedIndex = 0;
        public int RentLivingSearchTabSelectedIndex
        {
            get
            {
                return _rentLivingSearchTabSelectedIndex;
            }
            set
            {
                if (_rentLivingSearchTabSelectedIndex == value) return;

                _rentLivingSearchTabSelectedIndex = value;
                this.NotifyPropertyChanged("RentLivingSearchTabSelectedIndex");
            }
        }

        // 削除予定 RL検索一覧・追加タブインデックス
        private int _showRentLivingTabActiveIndex = 0;
        public int ShowRentLivingTabActiveIndex
        {
            get
            {
                return _showRentLivingTabActiveIndex;
            }
            set
            {
                if (_showRentLivingTabActiveIndex == value) return;

                _showRentLivingTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingTabActiveIndex");

            }
        }

        // 削除予定 RL検索一覧画面の表示フラグ 
        private bool _showRentLivingSearchList = true;
        public bool ShowRentLivingSearchList
        {
            get
            {
                return _showRentLivingSearchList;
            }
            set
            {
                if (_showRentLivingSearchList == value) return;

                _showRentLivingSearchList = value;
                this.NotifyPropertyChanged("ShowRentLivingSearchList");

                if (_showRentLivingSearchList)
                {
                    ShowRentLivingTabActiveIndex = 0;

                    ShowRentLivingNew = false;
                    ShowRentLivingEdit = false;
                }

            }
        }

        // 削除予定 RL新規追加画面の表示フラグ
        private bool _showRentLivingNew = false;
        public bool ShowRentLivingNew
        {
            get
            {
                return _showRentLivingNew;
            }
            set
            {
                if (_showRentLivingNew == value) return;

                _showRentLivingNew = value;
                this.NotifyPropertyChanged("ShowRentLivingNew");

                ShowRentLivingSearchList = !_showRentLivingNew;

                if (_showRentLivingNew)
                {
                    ShowRentLivingTabActiveIndex = 1;

                    ShowRentLivingSearchList = false;
                    ShowRentLivingEdit = false;
                }

            }
        }

        // 削除予定 RL編集画面の表示フラグ
        private bool _showRentLivingEdit = false;
        public bool ShowRentLivingEdit
        {
            get
            {
                return _showRentLivingEdit;
            }
            set
            {
                if (_showRentLivingEdit == value) return;

                _showRentLivingEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingEdit");

                ShowRentLivingSearchList = !_showRentLivingEdit;

                if (_showRentLivingEdit)
                {
                    ShowRentLivingTabActiveIndex = 2;

                    ShowRentLivingNew = false;
                }

            }
        }


        // 削除予定 RL新規物件の新規部屋タブインデックス
        private int _showRentLivingNewSectionTabActiveIndex = 0;
        public int ShowRentLivingNewSectionTabActiveIndex
        {
            get
            {
                return _showRentLivingNewSectionTabActiveIndex;
            }
            set
            {
                if (_showRentLivingNewSectionTabActiveIndex == value) return;

                _showRentLivingNewSectionTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionTabActiveIndex");

            }
        }

        // 削除予定 RL新規物件の部屋一覧画面の表示フラグ
        private bool _showRentLivingNewSectionList = true;
        public bool ShowRentLivingNewSectionList
        {
            get
            {
                return _showRentLivingNewSectionList;
            }
            set
            {
                if (_showRentLivingNewSectionList == value) return;

                _showRentLivingNewSectionList = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionList");

                if (_showRentLivingNewSectionList)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionNew = false;
                    ShowRentLivingNewSectionEdit = false;
                }

            }
        }

        // 削除予定 RL新規物件の部屋新規追加画面の表示フラグ
        private bool _showRentLivingNewSectionNew = false;
        public bool ShowRentLivingNewSectionNew
        {
            get
            {
                return _showRentLivingNewSectionNew;
            }
            set
            {
                if (_showRentLivingNewSectionNew == value) return;

                _showRentLivingNewSectionNew = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionNew");

                if (_showRentLivingNewSectionNew)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 1;
                    ShowRentLivingNewSectionList = false;
                    ShowRentLivingNewSectionEdit = false;
                }
                else
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionList = true;
                    ShowRentLivingNewSectionEdit = false;
                }

            }
        }

        // 削除予定 RL新規物件の部屋編集画面の表示フラグ
        private bool _showRentLivingNewSectionEdit = false;
        public bool ShowRentLivingNewSectionEdit
        {
            get
            {
                return _showRentLivingNewSectionEdit;
            }
            set
            {
                if (_showRentLivingNewSectionEdit == value) return;

                _showRentLivingNewSectionEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionEdit");

                if (_showRentLivingNewSectionEdit)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 2;
                    ShowRentLivingNewSectionList = false;
                    ShowRentLivingNewSectionNew = false;
                }
                else
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionList = true;
                    ShowRentLivingNewSectionNew = false;
                }

            }
        }


        // 削除予定 RL編集物件の新規部屋タブインデックス
        private int _showRentLivingEditSectionTabActiveIndex = 0;
        public int ShowRentLivingEditSectionTabActiveIndex
        {
            get
            {
                return _showRentLivingEditSectionTabActiveIndex;
            }
            set
            {
                if (_showRentLivingEditSectionTabActiveIndex == value) return;

                _showRentLivingEditSectionTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionTabActiveIndex");

            }
        }

        // 削除予定 RL編集物件の部屋一覧画面の表示フラグ
        private bool _showRentLivingEditSectionList = true;
        public bool ShowRentLivingEditSectionList
        {
            get
            {
                return _showRentLivingEditSectionList;
            }
            set
            {
                if (_showRentLivingEditSectionList == value) return;

                _showRentLivingEditSectionList = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionList");

                if (_showRentLivingEditSectionList)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionNew = false;
                    ShowRentLivingEditSectionEdit = false;
                }

            }
        }

        // 削除予定 RL編集物件の部屋新規追加画面の表示フラグ
        private bool _showRentLivingEditSectionNew = false;
        public bool ShowRentLivingEditSectionNew
        {
            get
            {
                return _showRentLivingEditSectionNew;
            }
            set
            {
                if (_showRentLivingEditSectionNew == value) return;

                _showRentLivingEditSectionNew = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionNew");

                if (_showRentLivingEditSectionNew)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 1;
                    ShowRentLivingEditSectionList = false;
                    ShowRentLivingEditSectionEdit = false;
                }
                else
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionList = true;
                    ShowRentLivingEditSectionEdit = false;
                }
            }
        }

        // 削除予定 RL編集物件の部屋編集画面の表示フラグ
        private bool _showRentLivingEditSectionEdit = false;
        public bool ShowRentLivingEditSectionEdit
        {
            get
            {
                return _showRentLivingEditSectionEdit;
            }
            set
            {
                if (_showRentLivingEditSectionEdit == value) return;

                _showRentLivingEditSectionEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionEdit");

                if (_showRentLivingEditSectionEdit)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 2;
                    ShowRentLivingEditSectionList = false;
                    ShowRentLivingEditSectionNew = false;
                }
                else
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionList = true;
                    ShowRentLivingEditSectionNew = false;
                }

            }
        }


        // 削除予定 元付け業者Agency 検索一覧・追加・編集タブインデックス
        private int _showAgencyTabActiveIndex = 0;
        public int ShowAgencyTabActiveIndex
        {
            get
            {
                return _showAgencyTabActiveIndex;
            }
            set
            {
                if (_showAgencyTabActiveIndex == value) return;

                _showAgencyTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowAgencyTabActiveIndex");
            }
        }

        // 削除予定 元付け業者Agency検索一覧画面の表示フラグ
        private bool _showAgencySearchList = true;
        public bool ShowAgencySearchList
        {
            get
            {
                return _showAgencySearchList;
            }
            set
            {
                if (_showAgencySearchList == value) return;

                _showAgencySearchList = value;
                this.NotifyPropertyChanged("ShowAgencySearchList");

                if (_showAgencySearchList)
                {
                    ShowAgencyTabActiveIndex = 0;

                    ShowAgencyEdit = false;
                }

            }

        }

        // 削除予定 元付け業者Agency新規追加画面の表示フラグ
        private bool _showAgencyEdit = false;
        public bool ShowAgencyEdit
        {
            get
            {
                return _showAgencyEdit;
            }
            set
            {
                if (_showAgencyEdit == value) return;

                _showAgencyEdit = value;
                this.NotifyPropertyChanged("ShowAgencyEdit");

                ShowAgencySearchList = !_showAgencyEdit;

                if (_showAgencyEdit)
                {
                    ShowAgencyTabActiveIndex = 1;
                }

            }
        }


        // 削除予定 管理会社 検索一覧・追加・編集タブインデックス
        private int _showMaintenanceCompanyTabActiveIndex = 0;
        public int ShowMaintenanceCompanyTabActiveIndex
        {
            get
            {
                return _showMaintenanceCompanyTabActiveIndex;
            }
            set
            {
                if (_showMaintenanceCompanyTabActiveIndex == value) return;

                _showMaintenanceCompanyTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanyTabActiveIndex");
            }
        }

        // 削除予定 管理会社 検索一覧画面の表示フラグ
        private bool _showMaintenanceCompanySearchList = true;
        public bool ShowMaintenanceCompanySearchList
        {
            get
            {
                return _showMaintenanceCompanySearchList;
            }
            set
            {
                if (_showMaintenanceCompanySearchList == value) return;

                _showMaintenanceCompanySearchList = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanySearchList");

                if (_showMaintenanceCompanySearchList)
                {
                    ShowMaintenanceCompanyTabActiveIndex = 0;

                    ShowMaintenanceCompanyEdit = false;
                }

            }

        }

        // 削除予定 管理会社 新規追加画面の表示フラグ
        private bool _showMaintenanceCompanyEdit = false;
        public bool ShowMaintenanceCompanyEdit
        {
            get
            {
                return _showMaintenanceCompanyEdit;
            }
            set
            {
                if (_showMaintenanceCompanyEdit == value) return;

                _showMaintenanceCompanyEdit = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanyEdit");

                ShowMaintenanceCompanySearchList = !_showMaintenanceCompanyEdit;

                if (_showMaintenanceCompanyEdit)
                {
                    ShowMaintenanceCompanyTabActiveIndex = 1;
                }

            }
        }


        // 削除予定 オーナー 検索一覧・追加・編集タブインデックス
        private int _showOwnerTabActiveIndex = 0;
        public int ShowOwnerTabActiveIndex
        {
            get
            {
                return _showOwnerTabActiveIndex;
            }
            set
            {
                if (_showOwnerTabActiveIndex == value) return;

                _showOwnerTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowOwnerTabActiveIndex");
            }
        }

        // 削除予定 オーナー 検索一覧画面の表示フラグ
        private bool _showOwnerSearchList = true;
        public bool ShowOwnerSearchList
        {
            get
            {
                return _showOwnerSearchList;
            }
            set
            {
                if (_showOwnerSearchList == value) return;

                _showOwnerSearchList = value;
                this.NotifyPropertyChanged("ShowOwnerSearchList");

                if (_showOwnerSearchList)
                {
                    ShowOwnerTabActiveIndex = 0;

                    ShowOwnerEdit = false;
                }

            }

        }

        // 削除予定 オーナー 新規追加画面の表示フラグ
        private bool _showOwnerEdit = false;
        public bool ShowOwnerEdit
        {
            get
            {
                return _showOwnerEdit;
            }
            set
            {
                if (_showOwnerEdit == value) return;

                _showOwnerEdit = value;
                this.NotifyPropertyChanged("ShowOwnerEdit");

                ShowOwnerSearchList = !_showOwnerEdit;

                if (_showOwnerEdit)
                {
                    ShowOwnerTabActiveIndex = 1;
                }

            }
        }


        // エラー通知表示フラグ
        private bool _showErrorDialog = false;
        public bool ShowErrorDialog
        {
            get
            {
                return _showErrorDialog;
            }
            set
            {
                if (_showErrorDialog == value) return;

                _showErrorDialog = value;
                this.NotifyPropertyChanged("ShowErrorDialog");
            }
        }

        #endregion

        #region == その他のプロパティ ==

        // 賃貸住居用編集画面の検索文字列
        private string _rentLivingEditSearchText;
        public string RentLivingEditSearchText
        {
            get
            {
                return _rentLivingEditSearchText;
            }
            set
            {
                if (_rentLivingEditSearchText == value) return;

                _rentLivingEditSearchText = value;
                this.NotifyPropertyChanged("RentLivingEditSearchText");
            }
        }

        // 業者検索 検索文字列
        private string _agencySearchText;
        public string AgencySearchText
        {
            get
            {
                return _agencySearchText;
            }
            set
            {
                if (_agencySearchText == value) return;

                _agencySearchText = value;
                this.NotifyPropertyChanged("AgencySearchText");
            }
        }

        // 管理会社 検索文字列
        private string _maintenanceCompanySearchText;
        public string MaintenanceCompanySearchText
        {
            get
            {
                return _maintenanceCompanySearchText;
            }
            set
            {
                if (_maintenanceCompanySearchText == value) return;

                _maintenanceCompanySearchText = value;
                this.NotifyPropertyChanged("MaintenanceCompanySearchText");
            }
        }

        // オーナー 検索文字列
        private string _ownerSearchText;
        public string OwnerSearchText
        {
            get
            {
                return _ownerSearchText;
            }
            set
            {
                if (_ownerSearchText == value) return;

                _ownerSearchText = value;
                this.NotifyPropertyChanged("OwnerSearchText");
            }
        }

        #endregion

        #region == エラー関連のプロパティ ==

        private StringBuilder _errorText = new StringBuilder();
        public string ErrorText
        {
            get
            {
                return _errorText.ToString();
            }
            set
            {
                _errorText.Insert(0, value + Environment.NewLine);
                this.NotifyPropertyChanged("ErrorText");
            }
        }

        // エラーのリスト TODO: ログ保存
        private ObservableCollection<ErrorObject> _errors = new ObservableCollection<ErrorObject>();
        public ObservableCollection<ErrorObject> Errors
        {
            get { return this._errors; }
        }

        #endregion

        #region == ダイアログ ==

        // サービスのインジェクションは・・・とりあえずしない。
        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();

        #endregion

        #region == ホーム画面 ==

        private RentSearchTags _selectedRentSearchTags = RentSearchTags.RentName;
        public RentSearchTags SelectedRentSearchTags
        {
            get
            {
                return _selectedRentSearchTags;
            }
            set
            {
                if (_selectedRentSearchTags == value)
                    return;

                _selectedRentSearchTags = value;
                NotifyPropertyChanged("SelectedRentSearchTags");
            }
        }

        private string _homeSearchText;
        public string HomeSearchText
        {
            get
            {
                return _homeSearchText;
            }
            set
            {
                if (_homeSearchText == value) return;

                _homeSearchText = value;
                this.NotifyPropertyChanged("HomeSearchText");
            }
        }

        #endregion

        #region == イベント ==

        // エラー通知イベント
        public delegate void MyErrorEvent(ErrorObject err);
        public event MyErrorEvent ErrorOccured;

        // 物件編集画面表示イベント
        public event EventHandler<OpenRentLivingBuildingWindowEventArgs> OpenRentLivingBuildingWindow;

        #endregion

        public MainViewModel()
        {
            #region == イベントへのサブスクライブ ==

            ErrorOccured += new MyErrorEvent(OnError);

            #endregion

            #region == コマンドのイニシャライズ ==

            // ホーム検索
            HomeSearchExecCommand = new RelayCommand(HomeSearchExecCommand_Execute, HomeSearchExecCommand_CanExecute);

            // RL検索
            RentLivingEditSearchCommand = new RelayCommand(RentLivingEditSearchCommand_Execute, RentLivingEditSearchCommand_CanExecute);
            // RL一覧
            RentLivingEditListCommand = new RelayCommand(RentLivingEditListCommand_Execute, RentLivingEditListCommand_CanExecute);
            // RL 検索条件画面に戻る
            RentLivingSearchBackCommand = new RelayCommand(RentLivingSearchBackCommand_Execute, RentLivingSearchBackCommand_CanExecute);

            // RL新規
            RentLivingEditNewCommand = new RelayCommand(RentLivingEditNewCommand_Execute, RentLivingEditNewCommand_CanExecute);
            // RL編集
            RentLivingEditSelectedEditCommand = new RelayCommand(RentLivingEditSelectedEditCommand_Execute, RentLivingEditSelectedEditCommand_CanExecute);
            // RL表示
            RentLivingEditSelectedViewCommand = new RelayCommand(RentLivingEditSelectedViewCommand_Execute, RentLivingEditSelectedViewCommand_CanExecute);
            // RL削除
            RentLivingEditSelectedDeleteCommand = new RelayCommand(RentLivingEditSelectedDeleteCommand_Execute, RentLivingEditSelectedDeleteCommand_CanExecute);


            // 元付け業者
            AgencySearchCommand = new RelayCommand(AgencySearchCommand_Execute, AgencySearchCommand_CanExecute);
            AgencyListCommand = new RelayCommand(AgencyListCommand_Execute, AgencyListCommand_CanExecute);
            AgencyNewCommand = new RelayCommand(AgencyNewCommand_Execute, AgencyNewCommand_CanExecute);
            AgencySelectedEditCommand = new RelayCommand(AgencySelectedEditCommand_Execute, AgencySelectedEditCommand_CanExecute);
            AgencySelectedDeleteCommand = new RelayCommand(AgencySelectedDeleteCommand_Execute, AgencySelectedDeleteCommand_CanExecute);
            AgencyNewOrEditCancelCommand = new RelayCommand(AgencyNewOrEditCancelCommand_Execute, AgencyNewOrEditCancelCommand_CanExecute);
            AgencyInsertOrUpdateCommand = new RelayCommand(AgencyInsertOrUpdateCommand_Execute, AgencyInsertOrUpdateCommand_CanExecute);

            // 管理会社
            MaintenanceCompanySearchCommand = new RelayCommand(MaintenanceCompanySearchCommand_Execute, MaintenanceCompanySearchCommand_CanExecute);
            MaintenanceCompanyListCommand = new RelayCommand(MaintenanceCompanyListCommand_Execute, MaintenanceCompanyListCommand_CanExecute);
            MaintenanceCompanyNewCommand = new RelayCommand(MaintenanceCompanyNewCommand_Execute, MaintenanceCompanyNewCommand_CanExecute);
            MaintenanceCompanySelectedEditCommand = new RelayCommand(MaintenanceCompanySelectedEditCommand_Execute, MaintenanceCompanySelectedEditCommand_CanExecute);
            MaintenanceCompanySelectedDeleteCommand = new RelayCommand(MaintenanceCompanySelectedDeleteCommand_Execute, MaintenanceCompanySelectedDeleteCommand_CanExecute);
            MaintenanceCompanyNewOrEditCancelCommand = new RelayCommand(MaintenanceCompanyNewOrEditCancelCommand_Execute, MaintenanceCompanyNewOrEditCancelCommand_CanExecute);
            MaintenanceCompanyInsertOrUpdateCommand = new RelayCommand(MaintenanceCompanyInsertOrUpdateCommand_Execute, MaintenanceCompanyInsertOrUpdateCommand_CanExecute);

            // オーナー
            OwnerSearchCommand = new RelayCommand(OwnerSearchCommand_Execute, OwnerSearchCommand_CanExecute);
            OwnerListCommand = new RelayCommand(OwnerListCommand_Execute, OwnerListCommand_CanExecute);
            OwnerNewCommand = new RelayCommand(OwnerNewCommand_Execute, OwnerNewCommand_CanExecute);
            OwnerSelectedEditCommand = new RelayCommand(OwnerSelectedEditCommand_Execute, OwnerSelectedEditCommand_CanExecute);
            OwnerSelectedDeleteCommand = new RelayCommand(OwnerSelectedDeleteCommand_Execute, OwnerSelectedDeleteCommand_CanExecute);
            OwnerNewOrEditCancelCommand = new RelayCommand(OwnerNewOrEditCancelCommand_Execute, OwnerNewOrEditCancelCommand_CanExecute);
            OwnerInsertOrUpdateCommand = new RelayCommand(OwnerInsertOrUpdateCommand_Execute, OwnerInsertOrUpdateCommand_CanExecute);

            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);

            #endregion

            #region == SQLite DB のイニシャライズ ==
            try
            {
                // DBのファイルパスを設定（TODO）
                ///var dataBaseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + ".db";
                var databaseFileFolerPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + _appName;
                System.IO.Directory.CreateDirectory(databaseFileFolerPath);
                var dataBaseFilePath = databaseFileFolerPath + System.IO.Path.DirectorySeparatorChar + _appName + ".db";

                // SQLite DB のイニシャライズ
                ResultWrapper res = dataAccessModule.InitializeDatabase(dataBaseFilePath);
                if (res.IsError)
                {
                    // エラーの通知
                    ErrorOccured?.Invoke(res.Error);

                    return;
                }
            }
            catch
            {
                //TODO:
            }

            #endregion

            //RentLivingEditListCommand_Execute();

        }

        #region == イベントの実装 ==

        // 起動時の処理
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定のロード  ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // アプリ設定情報の読み込み
            if (File.Exists(AppConfigFilePath))
            {
                XDocument xdoc = XDocument.Load(AppConfigFilePath);

                #region == ウィンドウ関連 ==
                /*
                if (sender is Window)
                {
                    // Main Window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow != null)
                    {
                        var hoge = mainWindow.Attribute("top");
                        if (hoge != null)
                        {
                            (sender as Window).Top = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("left");
                        if (hoge != null)
                        {
                            (sender as Window).Left = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("height");
                        if (hoge != null)
                        {
                            (sender as Window).Height = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge != null)
                        {
                            (sender as Window).Width = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("state");
                        if (hoge != null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                (sender as Window).WindowState = WindowState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                        }
                    }
                }
                */
                #endregion

            }
            else
            {
                // デフォ
            }

            #endregion
        }

        // 終了時の処理
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定の保存 ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == ウィンドウ関連 ==
            /*
            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);



                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }
            */
            #endregion

            try
            {
                // 設定ファイルの保存
                doc.Save(AppConfigFilePath);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + AppConfigFilePath);
            }

            #endregion

            #region == 編集ウィンドウを閉じる ==

            // TODO: When MainWindow try to close itself, confirm to close all the child windows. 

            App app = App.Current as App;

            // Now, use "app.Windows" this time.
            foreach (var w in app.Windows)
            {
                if (w is RentLivingRoomWindow)
                {
                    if ((w as RentLivingRoomWindow).DataContext == null)
                        continue;

                    if (!((w as RentLivingRoomWindow).DataContext is RentLivingRoomViewModel))
                        continue;

                    (w as RentLivingRoomWindow).Close();
                }
            }

            foreach (var w in app.Windows)
            {
                if (w is RentLivingBuildingWindow)
                {
                    if ((w as RentLivingBuildingWindow).DataContext == null)
                        continue;

                    if (!((w as RentLivingBuildingWindow).DataContext is RentLivingBuildingViewModel))
                        continue;

                    (w as RentLivingBuildingWindow).Close();
                }

            }

            #endregion

            #region == ウィンドウ関連 ==
            /*
            if (sender is Window)
            {
                if ((sender as Window).WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.MainWindow_Left = (sender as Window).Left;
                    Properties.Settings.Default.MainWindow_Top = (sender as Window).Top;
                    Properties.Settings.Default.MainWindow_Height = (sender as Window).Height;
                    Properties.Settings.Default.MainWindow_Width = (sender as Window).Width;
                }

                Properties.Settings.Default.Save();
            }
            */

            #endregion

        }

        // エラーイベントの実装
        private void OnError(ErrorObject err)
        {
            if (err == null) { return; }

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                // リストに追加。TODO：あとあとログ保存等
                _errors.Insert(0, err);

                ErrorText = String.Format("エラー：{3}、エラー内容 {4}、 タイプ {1}、発生箇所 {2}、発生時刻 {0}", err.ErrDatetime.ToString(), err.ErrType.ToString(), err.ErrPlace, err.ErrText, err.ErrDescription);

            });

            // エラーの表示
            ShowErrorDialog = true;
        }

        #endregion

        #region == コマンド ==

        #region == ホーム画面 ==

        public ICommand HomeSearchExecCommand { get; }
        public bool HomeSearchExecCommand_CanExecute()
        {
            return true;
        }
        public void HomeSearchExecCommand_Execute()
        {
            //
        }

        #endregion

        #region == 賃貸住居用 物件検索・編集 ==

        // 検索
        public ICommand RentLivingEditSearchCommand { get; }
        public bool RentLivingEditSearchCommand_CanExecute()
        {
            // TODO
            if (String.IsNullOrEmpty(RentLivingEditSearchText))
            {
                //return false;
            }
            else
            {
                //return true;
            }
            return true;
        }
        public void RentLivingEditSearchCommand_Execute()
        {
            // 一覧をクリア
            RentSearchResult.Clear();

            // 既に一覧を表示していたら、検索画面に戻る。
            if (RentLivingSearchTabSelectedIndex == 1)
            {
                RentLivingSearchBackCommand_Execute();
                return;
            }

            RentLivingSearchTabSelectedIndex = 1;
            // 検索結果一覧タブに移動

            // 検索結果一覧を取得
            ResultWrapper res = dataAccessModule.RentLivingSearch(RentSearchResult, RentLivingEditSearchText);

            if (res.IsError)
            {
                // エラーの通知
                ErrorOccured?.Invoke(res.Error);

                return;
            }

        }

        // 一覧
        public ICommand RentLivingEditListCommand { get; }
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // 一覧をクリア
            RentSearchResult.Clear();

            // 検索結果一覧タブに移動
            RentLivingSearchTabSelectedIndex = 1;

            // 一覧を取得
            ResultWrapper res = dataAccessModule.RentLivingSearch(RentSearchResult, "*");

            if (res.IsError)
            {
                // エラーの通知
                ErrorOccured?.Invoke(res.Error);

                return;
            }
        }

        // 検索へ戻る
        public ICommand RentLivingSearchBackCommand { get; }
        public bool RentLivingSearchBackCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingSearchBackCommand_Execute()
        {
            // 検索結果一覧アイテムの選択保持変数をクリア・解除
            RentLivingEditSelectedItem = null;

            // 検索タブに移動
            RentLivingSearchTabSelectedIndex = 0;
        }

        // 新規追加（編集画面表示）
        public ICommand RentLivingEditNewCommand { get; }
        public bool RentLivingEditNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditNewCommand_Execute()
        {
            // 編集用の賃貸住居用物件オブジェクトを用意
            RentLiving rl = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                IsNew = true
            };

            // 編集画面に渡すEventArgsを用意
            OpenRentLivingBuildingWindowEventArgs ag = new OpenRentLivingBuildingWindowEventArgs
            {
                Id = rl.RentLivingId,
                RentLivingObject = rl,
                DataAccessModule = this.dataAccessModule
            };

            // 画面表示イベント発火
            OpenRentLivingBuildingWindow?.Invoke(this, ag);
        }

        // 編集（編集画面表示）
        public ICommand RentLivingEditSelectedEditCommand { get; }
        public bool RentLivingEditSelectedEditCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedEditCommand_Execute()
        {
            if (RentLivingEditSelectedItem == null)
                return;

            // 編集用の賃貸住居用物件オブジェクトを取得
            ResultWrapper res = dataAccessModule.RentLivingSelectById(RentLivingEditSelectedItem.RentId, RentLivingEditSelectedItem.RentLivingId);
            if (res.IsError)
            {
                // エラーの通知
                ErrorOccured?.Invoke(res.Error);

                return;
            }

            if (res.Data == null)
                return;
            if (res.Data is not RentLiving)
                return;

            RentLiving rl = res.Data as RentLiving;

            // 編集画面に渡すEventArgsを用意
            OpenRentLivingBuildingWindowEventArgs ag = new OpenRentLivingBuildingWindowEventArgs
            {
                Id = rl.RentLivingId,
                RentLivingObject = rl,
                DataAccessModule = dataAccessModule
            };

            // 画面表示イベント発火
            OpenRentLivingBuildingWindow?.Invoke(this, ag);
        }

        // 表示 (PDFとか)
        public ICommand RentLivingEditSelectedViewCommand { get; }
        public bool RentLivingEditSelectedViewCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedViewCommand_Execute()
        {
            //RentLivingEditSelectedItem
            System.Diagnostics.Debug.WriteLine("RentLivingEditSelectedViewCommand_Execute");

            // TODO view
        }

        // 削除（DELETE）
        public ICommand RentLivingEditSelectedDeleteCommand { get; }
        public bool RentLivingEditSelectedDeleteCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedDeleteCommand_Execute()
        {
            if (RentLivingEditSelectedItem != null)
            {
                // DBから削除
                ResultWrapper res = dataAccessModule.RentLivingDelete(RentLivingEditSelectedItem.RentId);

                if (res.IsError)
                {
                    // エラー通知
                    ErrorOccured?.Invoke(res.Error);

                    return;
                }

                // 一覧からも削除
                if (RentSearchResult.Remove(RentLivingEditSelectedItem))
                {
                    RentLivingEditSelectedItem = null;
                }
            }
        }

        #endregion

        #region == 元付け業者 ==

        // 業者　管理、検索
        public ICommand AgencySearchCommand { get; }
        public bool AgencySearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(AgencySearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void AgencySearchCommand_Execute()
        {
            // Firest, clear it.
            Agencies.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Agency WHERE Name Like '%" + AgencySearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Agency ag = new Agency(Convert.ToString(reader["Agency_ID"]));
                            ag.Name = Convert.ToString(reader["Name"]);
                            ag.Branch = Convert.ToString(reader["Branch"]);
                            ag.PostalCode = Convert.ToString(reader["PostalCode"]);
                            ag.Address = Convert.ToString(reader["Address"]);
                            ag.TelNumber = Convert.ToString(reader["TelNumber"]);
                            ag.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            ag.Memo = Convert.ToString(reader["Memo"]);

                            ag.IsNew = false;
                            ag.IsDirty = false;

                            Agencies.Add(ag);

                        }
                    }
                }
            }
        }

        // 業者　管理、一覧
        public ICommand AgencyListCommand { get; }
        public bool AgencyListCommand_CanExecute()
        {
            return true;
        }
        public void AgencyListCommand_Execute()
        {
            // Firest, clear it.
            Agencies.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Agency";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                Agency ag = new Agency(Convert.ToString(reader["Agency_ID"]));
                                ag.Name = Convert.ToString(reader["Name"]);
                                ag.Branch = Convert.ToString(reader["Branch"]);
                                ag.PostalCode = Convert.ToString(reader["PostalCode"]);
                                ag.Address = Convert.ToString(reader["Address"]);
                                ag.TelNumber = Convert.ToString(reader["TelNumber"]);
                                ag.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                ag.Memo = Convert.ToString(reader["Memo"]);

                                ag.IsNew = false;
                                ag.IsDirty = false;

                                Agencies.Add(ag);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::AgencyListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::AgencyListCommand_Execute()");
                }
                /*
                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "業者を一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::AgencyListCommand_Execute()";
                ErrorOccured?.Invoke(er);
                */
            }


        }

        // 業者　管理、一覧選択アイテム削除（DELETE）
        public ICommand AgencySelectedDeleteCommand { get; }
        public bool AgencySelectedDeleteCommand_CanExecute()
        {
            if (AgenciesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void AgencySelectedDeleteCommand_Execute()
        {
            if (AgenciesSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM Agency WHERE Agency_ID = '{0}'", AgenciesSelectedItem.Agency_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (Agencies.Remove(AgenciesSelectedItem))
                            {
                                AgenciesSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @AgencySelectedDeleteCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::AgencySelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }
        }

        // 業者　追加（画面表示）
        public ICommand AgencyNewCommand { get; }
        public bool AgencyNewCommand_CanExecute()
        {
            return true;
        }
        public void AgencyNewCommand_Execute()
        {
            // Agencyオブジェクトを用意
            AgencyEdit = new Agency(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowAgencyEdit = true;
        }

        // 業者　編集（画面表示）
        public ICommand AgencySelectedEditCommand { get; }
        public bool AgencySelectedEditCommand_CanExecute()
        {
            if (AgenciesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void AgencySelectedEditCommand_Execute()
        {
            if (AgenciesSelectedItem == null) return;

            // Agencyオブジェクトを設定
            AgencyEdit = AgenciesSelectedItem;

            ShowAgencyEdit = true;
        }

        // 業者　追加・更新のキャンセル
        public ICommand AgencyNewOrEditCancelCommand { get; }
        public bool AgencyNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void AgencyNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            if (ShowAgencyEdit) ShowAgencyEdit = false;
        }

        // 業者　追加または更新処理(InsertOrUpdate)
        public ICommand AgencyInsertOrUpdateCommand { get; }
        public bool AgencyInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void AgencyInsertOrUpdateCommand_Execute()
        {
            if (AgencyEdit == null) return;

            // TODO: 入力チェック

            if (AgencyEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertIntoAgency = String.Format("INSERT INTO Agency (Agency_ID, Name, Branch, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        AgencyEdit.Agency_ID, AgencyEdit.Name, AgencyEdit.Branch, AgencyEdit.PostalCode, AgencyEdit.Address, AgencyEdit.TelNumber, AgencyEdit.FaxNumber, AgencyEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // Agencyテーブルへ追加
                                cmd.CommandText = sqlInsertIntoAgency;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    AgencyEdit.IsNew = false;
                                    AgencyEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowAgencyEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();

                                Debug.WriteLine("Opps. " + "「" + e.Message + "」");
                                /*
                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::AgencyInsertOrUpdateCommand()";
                                ErrorOccured?.Invoke(er);
                                */
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::AgencyInsertOrUpdateCommand()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::AgencyInsertOrUpdateCommand()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::AgencyInsertOrUpdateCommand()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::AgencyInsertOrUpdateCommand()");
                    }
                }
            }
            else
            {
                // 更新

                if (AgenciesSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdateAgency = String.Format("UPDATE Agency SET Name = '{1}', Branch = '{2}', PostalCode = '{3}', Address = '{4}', TelNumber = '{5}', FaxNumber = '{6}', Memo = '{7}' WHERE Agency_ID = '{0}'",
                    AgencyEdit.Agency_ID, AgencyEdit.Name, AgencyEdit.Branch, AgencyEdit.PostalCode, AgencyEdit.Address, AgencyEdit.TelNumber, AgencyEdit.FaxNumber, AgencyEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdateAgency;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //AgencyEdit.IsNew = false;
                                AgencyEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowAgencyEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::AgencyInsertOrUpdateCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "元付け業者の編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::AgencyInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }

        }

        #endregion

        #region == 管理会社 ==

        // 管理会社　検索
        public ICommand MaintenanceCompanySearchCommand { get; }
        public bool MaintenanceCompanySearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(MaintenanceCompanySearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void MaintenanceCompanySearchCommand_Execute()
        {
            // Firest, clear it.
            MaintenanceCompanies.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM MaintenanceCompany WHERE Name Like '%" + MaintenanceCompanySearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            MaintenanceCompany mc = new MaintenanceCompany(Convert.ToString(reader["MaintenanceCompany_ID"]));
                            mc.Name = Convert.ToString(reader["Name"]);
                            mc.Branch = Convert.ToString(reader["Branch"]);
                            mc.PostalCode = Convert.ToString(reader["PostalCode"]);
                            mc.Address = Convert.ToString(reader["Address"]);
                            mc.TelNumber = Convert.ToString(reader["TelNumber"]);
                            mc.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            mc.Memo = Convert.ToString(reader["Memo"]);

                            mc.IsNew = false;
                            mc.IsDirty = false;

                            MaintenanceCompanies.Add(mc);

                        }
                    }
                }
            }
        }

        // 管理会社　一覧
        public ICommand MaintenanceCompanyListCommand { get; }
        public bool MaintenanceCompanyListCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyListCommand_Execute()
        {
            // Firest, clear it.
            MaintenanceCompanies.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MaintenanceCompany";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                MaintenanceCompany mc = new MaintenanceCompany(Convert.ToString(reader["MaintenanceCompany_ID"]));
                                mc.Name = Convert.ToString(reader["Name"]);
                                mc.Branch = Convert.ToString(reader["Branch"]);
                                mc.PostalCode = Convert.ToString(reader["PostalCode"]);
                                mc.Address = Convert.ToString(reader["Address"]);
                                mc.TelNumber = Convert.ToString(reader["TelNumber"]);
                                mc.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                mc.Memo = Convert.ToString(reader["Memo"]);

                                mc.IsNew = false;
                                mc.IsDirty = false;

                                MaintenanceCompanies.Add(mc);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::MaintenanceCompanyListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::MaintenanceCompanyListCommand_Execute()");
                }
                /*
                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "管理会社を一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::MaintenanceCompanyListCommand_Execute()";
                ErrorOccured?.Invoke(er);
                */
            }


        }

        // 管理会社　一覧選択アイテム削除（DELETE）
        public ICommand MaintenanceCompanySelectedDeleteCommand { get; }
        public bool MaintenanceCompanySelectedDeleteCommand_CanExecute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void MaintenanceCompanySelectedDeleteCommand_Execute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM MaintenanceCompany WHERE MaintenanceCompany_ID = '{0}'",
                    MaintenanceCompaniesSelectedItem.MaintenanceCompany_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (MaintenanceCompanies.Remove(MaintenanceCompaniesSelectedItem))
                            {
                                MaintenanceCompaniesSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MaintenanceCompanySelectedDeleteCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::MaintenanceCompanySelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }
        }

        // 管理会社　追加（画面表示）
        public ICommand MaintenanceCompanyNewCommand { get; }
        public bool MaintenanceCompanyNewCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyNewCommand_Execute()
        {
            // MaintenanceCompany
            MaintenanceCompanyEdit = new MaintenanceCompany(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowMaintenanceCompanyEdit = true;
        }

        // 管理会社　編集（画面表示）
        public ICommand MaintenanceCompanySelectedEditCommand { get; }
        public bool MaintenanceCompanySelectedEditCommand_CanExecute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void MaintenanceCompanySelectedEditCommand_Execute()
        {
            if (MaintenanceCompaniesSelectedItem == null) return;

            // MaintenanceCompanyオブジェクトを設定
            MaintenanceCompanyEdit = MaintenanceCompaniesSelectedItem;

            ShowMaintenanceCompanyEdit = true;
        }

        // 管理会社　追加・更新のキャンセル
        public ICommand MaintenanceCompanyNewOrEditCancelCommand { get; }
        public bool MaintenanceCompanyNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            ShowMaintenanceCompanyEdit = false;
        }

        // 管理会社　追加または更新処理(InsertOrUpdate)
        public ICommand MaintenanceCompanyInsertOrUpdateCommand { get; }
        public bool MaintenanceCompanyInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyInsertOrUpdateCommand_Execute()
        {
            if (MaintenanceCompanyEdit == null) return;

            // TODO: 入力チェック

            if (MaintenanceCompanyEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertInto = String.Format("INSERT INTO MaintenanceCompany (MaintenanceCompany_ID, Name, Branch, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        MaintenanceCompanyEdit.MaintenanceCompany_ID, MaintenanceCompanyEdit.Name, MaintenanceCompanyEdit.Branch, MaintenanceCompanyEdit.PostalCode, MaintenanceCompanyEdit.Address, MaintenanceCompanyEdit.TelNumber, MaintenanceCompanyEdit.FaxNumber, MaintenanceCompanyEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // テーブルへ追加
                                cmd.CommandText = sqlInsertInto;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    MaintenanceCompanyEdit.IsNew = false;
                                    MaintenanceCompanyEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowMaintenanceCompanyEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();


                                Debug.WriteLine("Opps. " + "「" + e.Message + "」");
                                /*
                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                                */
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    }
                }
            }
            else
            {
                // 更新

                if (MaintenanceCompaniesSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdate = String.Format("UPDATE MaintenanceCompany SET Name = '{1}', Branch = '{2}', PostalCode = '{3}', Address = '{4}', TelNumber = '{5}', FaxNumber = '{6}', Memo = '{7}' WHERE Agency_ID = '{0}'",
                    MaintenanceCompanyEdit.MaintenanceCompany_ID, MaintenanceCompanyEdit.Name, MaintenanceCompanyEdit.Branch, MaintenanceCompanyEdit.PostalCode, MaintenanceCompanyEdit.Address, MaintenanceCompanyEdit.TelNumber, MaintenanceCompanyEdit.FaxNumber, MaintenanceCompanyEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdate;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //MaintenanceCompanyEdit.IsNew = false;
                                MaintenanceCompanyEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowMaintenanceCompanyEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "管理会社の編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }
        }

        #endregion

        #region == オーナー ==

        // オーナー　検索
        public ICommand OwnerSearchCommand { get; }
        public bool OwnerSearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(OwnerSearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void OwnerSearchCommand_Execute()
        {
            // Firest, clear it.
            Owners.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Owner WHERE Name Like '%" + OwnerSearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Owner o = new Owner(Convert.ToString(reader["Owner_ID"]));
                            o.Name = Convert.ToString(reader["Name"]);
                            o.PostalCode = Convert.ToString(reader["PostalCode"]);
                            o.Address = Convert.ToString(reader["Address"]);
                            o.TelNumber = Convert.ToString(reader["TelNumber"]);
                            o.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            o.Memo = Convert.ToString(reader["Memo"]);

                            o.IsNew = false;
                            o.IsDirty = false;

                            Owners.Add(o);

                        }
                    }
                }
            }
        }

        // オーナー　一覧
        public ICommand OwnerListCommand { get; }
        public bool OwnerListCommand_CanExecute()
        {
            return true;
        }
        public void OwnerListCommand_Execute()
        {
            // Firest, clear it.
            Owners.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Owner";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                Owner o = new Owner(Convert.ToString(reader["Owner_ID"]));
                                o.Name = Convert.ToString(reader["Name"]);
                                o.PostalCode = Convert.ToString(reader["PostalCode"]);
                                o.Address = Convert.ToString(reader["Address"]);
                                o.TelNumber = Convert.ToString(reader["TelNumber"]);
                                o.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                o.Memo = Convert.ToString(reader["Memo"]);

                                o.IsNew = false;
                                o.IsDirty = false;

                                Owners.Add(o);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::OwnerListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::OwnerListCommand_Execute()");
                }
                /*
                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "オーナーを一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::OwnerListCommand_Execute()";
                ErrorOccured?.Invoke(er);
                */
            }


        }

        // オーナー　一覧選択アイテム削除（DELETE）
        public ICommand OwnerSelectedDeleteCommand { get; }
        public bool OwnerSelectedDeleteCommand_CanExecute()
        {
            if (OwnersSelectedItem != null)
                return true;
            else
                return false;
        }
        public void OwnerSelectedDeleteCommand_Execute()
        {
            if (OwnersSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM Owner WHERE Owner_ID = '{0}'",
                    OwnersSelectedItem.Owner_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (Owners.Remove(OwnersSelectedItem))
                            {
                                OwnersSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @OwnerSelectedDeleteCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::OwnerSelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }
        }

        // オーナー　追加（画面表示）
        public ICommand OwnerNewCommand { get; }
        public bool OwnerNewCommand_CanExecute()
        {
            return true;
        }
        public void OwnerNewCommand_Execute()
        {
            // Owner
            OwnerEdit = new Owner(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowOwnerEdit = true;
        }

        // オーナー　編集（画面表示）
        public ICommand OwnerSelectedEditCommand { get; }
        public bool OwnerSelectedEditCommand_CanExecute()
        {
            if (OwnersSelectedItem != null)
                return true;
            else
                return false;
        }
        public void OwnerSelectedEditCommand_Execute()
        {
            if (OwnersSelectedItem == null) return;

            // Owner
            OwnerEdit = OwnersSelectedItem;

            ShowOwnerEdit = true;
        }

        // オーナー　追加・更新のキャンセル
        public ICommand OwnerNewOrEditCancelCommand { get; }
        public bool OwnerNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void OwnerNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            ShowOwnerEdit = false;
        }

        // オーナー　追加または更新処理(InsertOrUpdate)
        public ICommand OwnerInsertOrUpdateCommand { get; }
        public bool OwnerInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void OwnerInsertOrUpdateCommand_Execute()
        {
            if (OwnerEdit == null) return;

            // TODO: 入力チェック

            if (OwnerEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertInto = String.Format("INSERT INTO Owner (Owner_ID, Name, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                        OwnerEdit.Owner_ID, OwnerEdit.Name, OwnerEdit.PostalCode, OwnerEdit.Address, OwnerEdit.TelNumber, OwnerEdit.FaxNumber, OwnerEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // テーブルへ追加
                                cmd.CommandText = sqlInsertInto;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    OwnerEdit.IsNew = false;
                                    OwnerEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowOwnerEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();

                                Debug.WriteLine("Opps. " + "「" + e.Message + "」");
                                /*
                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::OwnerInsertOrUpdateCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                                */
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    }
                }
            }
            else
            {
                // 更新

                if (OwnersSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdate = String.Format("UPDATE Owner SET Name = '{1}', PostalCode = '{2}', Address = '{3}', TelNumber = '{4}', FaxNumber = '{5}', Memo = '{6}' WHERE Owner_ID = '{0}'",
                    OwnerEdit.Owner_ID, OwnerEdit.Name, OwnerEdit.PostalCode, OwnerEdit.Address, OwnerEdit.TelNumber, OwnerEdit.FaxNumber, OwnerEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdate;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //OwnerEdit.IsNew = false;
                                OwnerEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowOwnerEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                            /*
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "オーナーの編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::OwnerInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                            */
                        }
                    }
                }
            }
        }

        #endregion

        #region == エラー通知画面周り ==

        // エラー通知画面のクローズ
        public ICommand CloseErrorCommand { get; }
        public bool CloseErrorCommand_CanExecute()
        {
            return true;
        }
        public void CloseErrorCommand_Execute()
        {
            ShowErrorDialog = false;
        }

        #endregion

    }

    #endregion

}
