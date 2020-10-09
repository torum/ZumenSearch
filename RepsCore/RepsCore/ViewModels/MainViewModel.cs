﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using RepsCore.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows.Threading;
//using System.Data.SQLite;
using Microsoft.Data.Sqlite;

namespace RepsCore.ViewModels
{

    #region == Rentのクラス ==

    /// <summary>
    /// 物件のクラス
    /// </summary>
    public class Rent : ViewModelBase
    {
        // GUID and Primary Key
        private string _id;
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id == value) return;

                _id = value;
                this.NotifyPropertyChanged("ID");
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value) return;

                _name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        public enum RentTypes
        {
            RentLiving, RentBussiness, RentParking
        }

        public Dictionary<RentTypes, string> RentTypeToLabel { get; } = new Dictionary<RentTypes, string>()
        {
            {RentTypes.RentLiving, "賃貸住居用"},
            {RentTypes.RentBussiness, "賃貸事業用"},
            {RentTypes.RentParking, "賃貸駐車場"},
        };

        public Dictionary<string, RentTypes> StringToRentType { get; } = new Dictionary<string, RentTypes>()
        {
            {"RentLiving", RentTypes.RentLiving},
            {"RentBussiness", RentTypes.RentBussiness},
            {"RentParking", RentTypes.RentParking},
        };

        private RentTypes _Type;
        public RentTypes Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (_Type == value) return;

                _Type = value;
                this.NotifyPropertyChanged("Type");
            }
        }

        public string TypeLabel
        {
            get
            {
                return this.RentTypeToLabel[this.Type];
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get
            {
                return _postalCode;
            }
            set
            {
                if (_postalCode == value) return;

                _postalCode = value;
                this.NotifyPropertyChanged("PostalCode");
            }
        }

        private string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location == value) return;

                _location = value;
                this.NotifyPropertyChanged("Location");
            }
        }

        private string _trainStation1;
        public string TrainStation1
        {
            get
            {
                return _trainStation1;
            }
            set
            {
                if (_trainStation1 == value) return;

                _trainStation1 = value;
                this.NotifyPropertyChanged("TrainStation1");
            }
        }

        private string _trainStation2;
        public string TrainStation2
        {
            get
            {
                return _trainStation2;
            }
            set
            {
                if (_trainStation2 == value) return;

                _trainStation2 = value;
                this.NotifyPropertyChanged("TrainStation2");
            }
        }

        public Rent()
        {

        }
    }

    #endregion

    public class MainViewModel : ViewModelBase
    {
        #region == 基本 ==

        // Application version
        private string _appVer = "0.0.0.1";

        // Application name
        private string _appName = "RepsCore";

        // Application config file folder
        private string _appDeveloper = "torum";

        // Application Window Title
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }

        #endregion

        #region == データベース ==

        // Sqlite DB file path
        private string _dataBaseFilePath;
        public string DataBaseFilePath
        {
            get { return _dataBaseFilePath; }
        }

        // SqliteConnectionStringBuilder
        public SqliteConnectionStringBuilder connectionStringBuilder;// = new SqliteConnectionStringBuilder();

        #endregion

        #region == 表示フラグ ==

        // RL新規追加画面の表示フラグ
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
            }
        }

        // RL新規追加画面の表示フラグ
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
            }
        }

        #endregion

        #region == 物件関連のクラス ==

        // 賃貸物件　新規追加用のクラス
        private Rent _rentNew = new Rent();
        public Rent RentNew
        {
            get
            {
                return _rentNew;
            }
            set
            {
                if (_rentNew == value) return;

                _rentNew = value;
                this.NotifyPropertyChanged("RentNew");
            }
        }

        // 賃貸物件　編集更新用のクラス
        private Rent _rentEdit = new Rent();
        public Rent RentEdit
        {
            get
            {
                return _rentEdit;
            }
            set
            {
                if (_rentEdit == value) return;

                _rentEdit = value;
                this.NotifyPropertyChanged("RentEdit");
            }
        }

        // 賃貸物件　編集一覧用のコレクション
        private ObservableCollection<Rent> _editRents = new ObservableCollection<Rent>();
        public ObservableCollection<Rent> EditRents
        {
            get { return this._editRents; }
        }

        #endregion

        #region == その他のプロパティ ==

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

        private Rent _rentLivingEditSelectedItem;
        public Rent RentLivingEditSelectedItem
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

        #endregion

        #region == コマンド ==

        public ICommand RentLivingNewCommand { get; }
        public ICommand RentLivingNewAddCommand { get; }
        public ICommand RentLivingNewCancelCommand { get; }
        public ICommand RentLivingEditListCommand { get; }
        public ICommand RentLivingEditSearchCommand { get; }
        public ICommand RentLivingEditSelectedEditCommand { get; }
        public ICommand RentLivingEditSelectedEditUpdateCommand { get; }
        public ICommand RentLivingEditSelectedEditCancelCommand { get; }
        public ICommand RentLivingEditSelectedViewCommand { get; }
        public ICommand RentLivingEditSelectedDeleteCommand { get; }

        #endregion

        public MainViewModel()
        {
            #region == DB のイニシャライズ ==

            // DB file path のセット
            _dataBaseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "SqliteDB.db";


            // Create a table if not exists.
            connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = DataBaseFilePath
            };

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();

                    using (var tableCmd = connection.CreateCommand())
                    {
                        tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Rent (" +
                                "ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Type TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Location TEXT NOT NULL," +
                                "TrainStation1 TEXT NOT NULL," +
                                "TrainStation2 TEXT NOT NULL)";

                        tableCmd.ExecuteNonQuery();
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        string err = e.InnerException.Message;
                        System.Diagnostics.Debug.WriteLine(err);
                    }
                }

            }


            #endregion

            #region == コマンドのイニシャライズ ==

            RentLivingNewCommand = new RelayCommand(RentLivingNewCommand_Execute, RentLivingNewCommand_CanExecute);
            RentLivingNewAddCommand = new RelayCommand(RentLivingNewAddCommand_Execute, RentLivingNewAddCommand_CanExecute);
            RentLivingNewCancelCommand = new RelayCommand(RentLivingNewCancelCommand_Execute, RentLivingNewCancelCommand_CanExecute);
            RentLivingEditListCommand = new RelayCommand(RentLivingEditListCommand_Execute, RentLivingEditListCommand_CanExecute);
            RentLivingEditSearchCommand = new RelayCommand(RentLivingEditSearchCommand_Execute, RentLivingEditSearchCommand_CanExecute);
            RentLivingEditSelectedEditCommand = new RelayCommand(RentLivingEditSelectedEditCommand_Execute, RentLivingEditSelectedEditCommand_CanExecute);
            RentLivingEditSelectedEditUpdateCommand = new RelayCommand(RentLivingEditSelectedEditUpdateCommand_Execute, RentLivingEditSelectedEditUpdateCommand_CanExecute);
            RentLivingEditSelectedEditCancelCommand = new RelayCommand(RentLivingEditSelectedEditCancelCommand_Execute, RentLivingEditSelectedEditCancelCommand_CanExecute);
            RentLivingEditSelectedViewCommand = new RelayCommand(RentLivingEditSelectedViewCommand_Execute, RentLivingEditSelectedViewCommand_CanExecute);
            RentLivingEditSelectedDeleteCommand = new RelayCommand(RentLivingEditSelectedDeleteCommand_Execute, RentLivingEditSelectedDeleteCommand_CanExecute);
            
            #endregion

        }

        #region == イベント系 ==

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

        }

        #endregion

        #region == メソッド ==

        #endregion

        #region == コマンド ==

        // 賃貸住居用物件、追加画面表示
        public bool RentLivingNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCommand_Execute()
        {
            if (!ShowRentLivingNew) ShowRentLivingNew = true;
        }

        // 賃貸住居用物件、追加 (INSERT)
        public bool RentLivingNewAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewAddCommand_Execute()
        {
            try
            {
                RentNew.ID = Guid.NewGuid().ToString();
                RentNew.Type = Rent.RentTypes.RentLiving;

                string sql = String.Format("INSERT INTO Rent (ID, Name, Type, PostalCode, Location, TrainStation1, TrainStation2) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", RentNew.ID, RentNew.Name, RentNew.Type.ToString(), RentNew.PostalCode, RentNew.Location, RentNew.TrainStation1, RentNew.TrainStation2);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sql;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                // TODO
                            }
                            cmd.Transaction.Commit();

                            /*
                            Rent r = new Rent();
                            r.ID = RentNew.ID;
                            r.Name = RentNew.Name;
                            r.Type = r.StringToRentType[Convert.ToString(reader["Type"])];
                            r.PostalCode = RentNew.PostalCode;
                            r.Location = RentNew.Location;
                            r.TrainStation1 = RentNew.TrainStation1;
                            r.TrainStation2 = RentNew.TrainStation2;

                            _editRents.Add(r);
                            */

                            // 編集を非表示に（閉じる）
                            if (ShowRentLivingNew) ShowRentLivingNew = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message);
                            //TODO 
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        // 賃貸住居用物件、追加キャンセル
        public bool RentLivingNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            if (ShowRentLivingNew) ShowRentLivingNew = false;

            // TODO RentNew をクリアする。
        }

        // 賃貸住居用物件、一覧
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // Firest, clear it.
            _editRents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText = "SELECT * FROM Rent";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Rent r = new Rent();
                            r.ID = Convert.ToString(reader["ID"]);
                            r.Name = Convert.ToString(reader["Name"]);
                            r.Type = r.StringToRentType[Convert.ToString(reader["Type"])];
                            r.PostalCode = Convert.ToString(reader["PostalCode"]);
                            r.Location = Convert.ToString(reader["Location"]);
                            r.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            r.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            _editRents.Add(r);

                        }
                    }
                }
            }
        }

        // 賃貸住居用物件、検索
        public bool RentLivingEditSearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(RentLivingEditSearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSearchCommand_Execute()
        {
            // Firest, clear it.
            _editRents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText = "SELECT * FROM Rent WHERE Name Like '%" + RentLivingEditSearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Rent r = new Rent();
                            r.ID = Convert.ToString(reader["ID"]);
                            r.Name = Convert.ToString(reader["Name"]);
                            r.Type = r.StringToRentType[Convert.ToString(reader["Type"])];
                            r.PostalCode = Convert.ToString(reader["PostalCode"]);
                            r.Location = Convert.ToString(reader["Location"]);
                            r.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            r.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            _editRents.Add(r);

                        }
                    }
                }
            }
        }

        // 賃貸住居用物件、選択アイテム編集表示
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
            if (RentLivingEditSelectedItem != null)
            {
                // 選択アイテムのデータを一旦編集オブジェクトに格納
                RentEdit.ID = RentLivingEditSelectedItem.ID;
                RentEdit.Name = RentLivingEditSelectedItem.Name;
                RentEdit.PostalCode = RentLivingEditSelectedItem.PostalCode;
                RentEdit.Location = RentLivingEditSelectedItem.Location;
                RentEdit.TrainStation1 = RentLivingEditSelectedItem.TrainStation1;
                RentEdit.TrainStation2 = RentLivingEditSelectedItem.TrainStation2;

                // TODO Open DB and SELECT !!




                // 編集画面の表示
                if (!ShowRentLivingEdit) ShowRentLivingEdit = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RentLivingEditSelectedItem == null at RentLivingEditSelectedEditCommand_Execute");
            }
        }

        // 賃貸住居用物件、選択アイテム編集更新（UPDATE）
        public bool RentLivingEditSelectedEditUpdateCommand_CanExecute()
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
        public void RentLivingEditSelectedEditUpdateCommand_Execute()
        {
            if (RentLivingEditSelectedItem != null)
            {
                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sql = String.Format("UPDATE Rent SET Name = '{1}', Type = '{2}', PostalCode = '{3}', Location = '{4}', TrainStation1 = '{5}', TrainStation2 = '{6}' WHERE ID = '{0}'", RentEdit.ID, RentEdit.Name, RentEdit.Type.ToString(), RentEdit.PostalCode, RentEdit.Location, RentEdit.TrainStation1, RentEdit.TrainStation2);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sql;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                // TODO
                            }
                            cmd.Transaction.Commit();

                            // 編集オブジェクトに格納された情報を、選択アイテムに更新（Listviewの情報が更新されるー＞DBから読み込みし直さなくて良くなる）
                            RentLivingEditSelectedItem.Name = RentEdit.Name;
                            RentLivingEditSelectedItem.PostalCode = RentEdit.PostalCode;
                            RentLivingEditSelectedItem.Location = RentEdit.Location;
                            RentLivingEditSelectedItem.Location = RentEdit.TrainStation1;
                            RentLivingEditSelectedItem.Location = RentEdit.TrainStation2;

                            // 編集画面を非表示に
                            if (ShowRentLivingEdit) ShowRentLivingEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message);
                            //TODO 
                        }
                    }
                }
            }
        }

        // 賃貸住居用物件、選択アイテム編集更新キャンセル
        public bool RentLivingEditSelectedEditCancelCommand_CanExecute()
        {
            if (ShowRentLivingEdit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void RentLivingEditSelectedEditCancelCommand_Execute()
        {
            if (ShowRentLivingEdit) ShowRentLivingEdit = false;
        }


        // 賃貸住居用物件、選択アイテム表示(PDFとか)
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

        // 賃貸住居用物件、選択アイテム削除（DELETE）
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
                // 選択アイテムのデータを削除

                string sql = String.Format("DELETE FROM Rent WHERE ID = '{0}'", RentLivingEditSelectedItem.ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sql;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                // TODO
                            }
                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (_editRents.Remove(RentLivingEditSelectedItem))
                            {
                                RentLivingEditSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message);
                            //TODO 
                        }
                    }
                }
            }
        }

    }



    #endregion

}
