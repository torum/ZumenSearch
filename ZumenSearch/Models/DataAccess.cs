using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ZumenSearch.Models.Classes;
using ZumenSearch.Common;

namespace ZumenSearch
{
    /// <summary>
    /// データアクセスのモジュール
    /// </summary>
    public class DataAccess
    {
        // Sqlite DB ファイルパス
        private string _dataBaseFilePath;
        public string DataBaseFilePath
        {
            get { return _dataBaseFilePath; }
        }

        // Sqlite DB コネクションビルダー
        private SqliteConnectionStringBuilder connectionStringBuilder;

        public DataAccess()
        {

        }

        // Sqlite DB のイニシャライズメソッド
        public void InitializeDatabase(string dataBaseFilePath)
        {
            _dataBaseFilePath = dataBaseFilePath;

            // Create a table if not exists.
            connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dataBaseFilePath
            };

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();

                    using (var tableCmd = connection.CreateCommand())
                    {
                        // トランザクション開始
                        tableCmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            // メインの賃貸物件「インデックス」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Rent (" +
                                "Rent_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Type TEXT NOT NULL," +
                                "PostalCode TEXT," +
                                "Location TEXT," +
                                "LocationHiddenPart TEXT," +
                                "GeoLocationLatitude TEXT," +
                                "GeoLocationLongitude TEXT," +
                                "TrainStation1 TEXT," +
                                "TrainStation2 TEXT)";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件のテーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLiving (" +
                                "RentLiving_ID TEXT NOT NULL PRIMARY KEY," +
                                "Rent_ID TEXT NOT NULL," +
                                "Kind TEXT NOT NULL," +
                                "Floors INTEGER NOT NULL," +
                                "FloorsBasement INTEGER," +
                                "BuiltYear INTEGER NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「写真」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingPicture (" +
                                "RentLivingPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PictureData BLOB NOT NULL," +
                                "PictureThumbW200xData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「図面」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingZumenPdf (" +
                                "RentLivingZumenPdf_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PdfData BLOB NOT NULL," +
                                "DateTimeAdded TEXT NOT NULL," +
                                "DateTimePublished TEXT NOT NULL," +
                                "DateTimeVerified TEXT NOT NULL," +
                                "FileSize REAL NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「部屋」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingSection(" +
                                "RentLivingSection_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "RoomNumber TEXT," +
                                "Price INTEGER NOT NULL," +
                                "Madori TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「部屋の写真」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingSectionPicture (" +
                                "RentLivingSectionPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLivingSection_ID TEXT NOT NULL," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PictureData BLOB NOT NULL," +
                                "PictureThumbW200xData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)," +
                                "FOREIGN KEY (RentLivingSection_ID) REFERENCES RentLivingSection(RentLivingSection_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 元付け業者テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Agency (" +
                                "Agency_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Branch TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 管理会社テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS MaintenanceCompany (" +
                                "MaintenanceCompany_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Branch TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // オーナーテーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Owner (" +
                                "Owner_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // トランザクションコミット
                            tableCmd.Transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            // トランザクションのロールバック
                            tableCmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel()");

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
                        string err = e.InnerException.Message;
                        System.Diagnostics.Debug.WriteLine(err);
                    }
                }

            }

        }

        // 賃貸住居用一覧
        public void GetListOfRentLiving(ObservableCollection<RentLiving> rents)
        {
            rents.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                RentLiving rl = new RentLiving(Convert.ToString(reader["Rent_ID"]), Convert.ToString(reader["RentLiving_ID"]));
                                //rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                                rl.Name = Convert.ToString(reader["Name"]);
                                //rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                                rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                                rl.Location = Convert.ToString(reader["Location"]);
                                rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                                rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                                rents.Add(rl);

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
                    Debug.WriteLine(e.InnerException.Message + " @MainViewModel::RentLivingListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::RentLivingListCommand_Execute()");
                }

                // TODO: 
                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "賃貸住居用　物件管理、一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::RentLivingListCommand_Execute()";
                //ErrorOccured?.Invoke(er);
            }

        }

        // 賃貸住居用検索
        public void GetSearchResultOfRentLiving(ObservableCollection<RentLiving> rents, string searchText)
        {
            rents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID WHERE Name Like '%" + searchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            RentLiving rl = new RentLiving(Convert.ToString(reader["Rent_ID"]), Convert.ToString(reader["RentLiving_ID"]));
                            //rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                            rl.Name = Convert.ToString(reader["Name"]);
                            //rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                            rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                            rl.Location = Convert.ToString(reader["Location"]);
                            rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            rents.Add(rl);

                        }
                    }
                }
            }
        }

        // 賃貸住居用物件オブジェクトをIDから取得
        public RentLiving GetRentLivingById(string rentId, string rentLivingId)
        {
            RentLiving rl = new RentLiving(rentId, rentLivingId);
            rl.IsNew = false;

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();

                    // 物件＋住居用ジョイン
                    cmd.CommandText = String.Format("SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID WHERE Rent.Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Rl.Rent_ID = rentid;
                            rl.Name = Convert.ToString(reader["Name"]);
                            //RentLivingEdit.Type = RentLivingEdit.StringToRentType[Convert.ToString(reader["Type"])];
                            rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                            rl.Location = Convert.ToString(reader["Location"]);
                            rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            //RentLivingEdit.RentLiving_ID = Convert.ToString(reader["RentLiving_ID"]);
                            rl.Kind = rl.StringToRentLivingKind[Convert.ToString(reader["Kind"])];
                            rl.Floors = Convert.ToInt32(reader["Floors"]);
                            rl.FloorsBasement = Convert.ToInt32(reader["FloorsBasement"]);
                            rl.BuiltYear = Convert.ToInt32(reader["BuiltYear"]);

                        }
                    }

                    // 物件写真
                    cmd.CommandText = String.Format("SELECT * FROM RentLivingPicture WHERE Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            RentLivingPicture rlpic = new RentLivingPicture(rentId, rentLivingId, Convert.ToString(reader["RentLivingPicture_ID"]));

                            byte[] imageBytes = (byte[])reader["PictureData"];
                            rlpic.PictureData = imageBytes;

                            byte[] imageThumbBytes = (byte[])reader["PictureThumbW200xData"];
                            rlpic.PictureThumbW200xData = imageThumbBytes;


                            rlpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                            rlpic.Picture = Methods.BitmapImageFromBytes(imageThumbBytes);

                            rl.RentLivingPictures.Add(rlpic);
                        }
                    }

                    // 図面
                    cmd.CommandText = String.Format("SELECT * FROM RentLivingZumenPdf WHERE Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RentLivingZumenPDF rlpdf = new RentLivingZumenPDF(rentId, rentLivingId, Convert.ToString(reader["RentLivingZumenPdf_ID"]));

                            byte[] pdfBytes = (byte[])reader["PdfData"];
                            rlpdf.PDFData = pdfBytes;


                            DateTime dt = new DateTime();

                            dt = DateTime.Parse(Convert.ToString(reader["DateTimeAdded"]));
                            rlpdf.DateTimeAdded = dt.ToLocalTime();
                            dt = DateTime.Parse(Convert.ToString(reader["DateTimePublished"]));
                            rlpdf.DateTimePublished = dt.ToLocalTime();
                            dt = DateTime.Parse(Convert.ToString(reader["DateTimeVerified"]));
                            rlpdf.DateTimeVerified = dt.ToLocalTime();

                            rlpdf.FileSize = Convert.ToInt64(reader["FileSize"]);

                            rlpdf.IsNew = false;

                            rl.RentLivingZumenPDFs.Add(rlpdf);
                        }
                    }

                    // 部屋
                    cmd.CommandText = String.Format("SELECT * FROM RentLivingSection WHERE Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RentLivingSection room = new RentLivingSection(rentId, rentLivingId, Convert.ToString(reader["RentLivingSection_ID"]));
                            room.RentLivingSectionRoomNumber = Convert.ToString(reader["RoomNumber"]);
                            room.RentLivingSectionMadori = Convert.ToString(reader["Madori"]);
                            room.RentLivingSectionPrice = Convert.ToInt32(reader["Price"]);

                            room.IsNew = false;
                            room.IsDirty = false;

                            rl.RentLivingSections.Add(room);
                        }
                    }

                    // 部屋写真
                    foreach (var hoge in rl.RentLivingSections)
                    {
                        cmd.CommandText = String.Format("SELECT * FROM RentLivingSectionPicture WHERE RentLivingSection_ID = '{0}'", hoge.RentLivingSectionId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingSectionPicture rlsecpic = new RentLivingSectionPicture(rentId, rentLivingId, hoge.RentLivingSectionId, Convert.ToString(reader["RentLivingSectionPicture_ID"]));

                                byte[] imageBytes = (byte[])reader["PictureData"];
                                rlsecpic.PictureData = imageBytes;

                                byte[] imageThumbBytes = (byte[])reader["PictureThumbW200xData"];
                                rlsecpic.PictureThumbW200xData = imageThumbBytes;


                                rlsecpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                                rlsecpic.Picture = Methods.BitmapImageFromBytes(imageThumbBytes);

                                rlsecpic.IsNew = false;
                                rlsecpic.IsModified = false;

                                hoge.RentLivingSectionPictures.Add(rlsecpic);

                            }
                        }

                    }

                    cmd.Transaction.Commit();
                }
            }

            // 大事
            rl.IsDirty = false;

            return rl;

        }

        // 賃貸住居用物件を追加（INSERT）
        public bool AddRentLiving(RentLiving rl)
        {
            Debug.WriteLine("DataAccess:AddRentLiving: " + rl.Name);

            bool result = false;

            string sqlInsertIntoRent = String.Format("INSERT INTO Rent (Rent_ID, Name, Type, PostalCode, Location, TrainStation1, TrainStation2) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                rl.RentId, rl.Name, rl.Type.ToString(), rl.PostalCode, rl.Location, rl.TrainStation1, rl.TrainStation2);

            string sqlInsertIntoRentLiving = String.Format("INSERT INTO RentLiving (RentLiving_ID, Rent_ID, Kind, Floors, FloorsBasement, BuiltYear) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                rl.RentLivingId, rl.RentId, rl.Kind.ToString(), rl.Floors, rl.FloorsBasement, rl.BuiltYear);

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            // Rentテーブルへ追加
                            cmd.CommandText = sqlInsertIntoRent;
                            var InsertIntoRentResult = cmd.ExecuteNonQuery();
                            if (InsertIntoRentResult != 1)
                            {
                                // これいる?
                            }

                            // RentLivingテーブルへ追加
                            cmd.CommandText = sqlInsertIntoRentLiving;
                            var InsertIntoRentLivingResult = cmd.ExecuteNonQuery();
                            if (InsertIntoRentLivingResult != 1)
                            {
                                // これいる?
                            }

                            // 写真追加
                            if (rl.RentLivingPictures.Count > 0)
                            {
                                foreach (var pic in rl.RentLivingPictures)
                                {
                                    // 新規なので全てIsNewのはずだけど・・・
                                    if (pic.IsNew)
                                    {
                                        string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}')",
                                            pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

                                        // 物件画像の追加
                                        cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                        // ループなので、前のパラメーターをクリアする。
                                        cmd.Parameters.Clear();

                                        SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                        parameter1.Value = pic.PictureData;
                                        cmd.Parameters.Add(parameter1);

                                        SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                        parameter2.Value = pic.PictureThumbW200xData;
                                        cmd.Parameters.Add(parameter2);

                                        var r = cmd.ExecuteNonQuery();
                                        if (r > 0)
                                        {
                                            pic.IsNew = false;
                                            pic.IsModified = false;
                                        }
                                    }
                                }
                            }

                            // 図面追加
                            if (rl.RentLivingZumenPDFs.Count > 0)
                            {
                                foreach (var pdf in rl.RentLivingZumenPDFs)
                                {
                                    string sqlInsertIntoRentLivingZumenPdf = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentZumenPdfId, rl.RentLivingId, rl.RentId, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumenPdf;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PDFData;
                                    cmd.Parameters.Add(parameter1);

                                    var r = cmd.ExecuteNonQuery();
                                    if (r > 0)
                                    {
                                        pdf.IsNew = false;
                                        //pic.IsModified = false;
                                    }
                                }
                            }

                            // 部屋追加
                            if (rl.RentLivingSections.Count > 0)
                            {
                                foreach (var room in rl.RentLivingSections)
                                {
                                    string sqlInsertIntoRentLivingSection = String.Format("INSERT INTO RentLivingSection (RentLivingSection_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingSectionId, rl.RentLivingId, rl.RentId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingSection;
                                    var InsertIntoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingSectionResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsDirty = false;
                                    }

                                    // 部屋の写真
                                    if (room.RentLivingSectionPictures.Count > 0)
                                    {
                                        foreach (var roompic in room.RentLivingSectionPictures)
                                        {
                                            string sqlInsertIntoRentLivingSectionPic = String.Format("INSERT INTO RentLivingSectionPicture (RentLivingSectionPicture_ID, RentLivingSection_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, rl.RentLivingId, rl.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                    }

                                }
                            }

                            //　コミット
                            cmd.Transaction.Commit();

                            result = true;
                        }
                        catch (Exception e)
                        {
                            result = false;

                            // ロールバック
                            cmd.Transaction.Rollback();

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingAddNewCommand_Execute()";
                            //ErrorOccured?.Invoke(er);

                            Debug.WriteLine(e.Message + " @DataAccess:AddRentLiving:Transaction.Commit");
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("Opps. TargetInvocationException@DataAccess:AddRentLiving");
                result = false;
                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("Opps. InvalidOperationException@DataAccess:AddRentLiving");
                result = false;
                throw ex.InnerException;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess:AddRentLiving");
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess:AddRentLiving");
                }
                result = false;
            }

            return result;
        }

        // 賃貸住居用物件を更新（UPDATE）
        public void UpdateRentLiving(RentLiving rl)
        {
            Debug.WriteLine("DataAccess:UpdateRentLiving: " + rl.Name);

            string sqlUpdateRent = String.Format("UPDATE Rent SET Name = '{1}', Type = '{2}', PostalCode = '{3}', Location = '{4}', TrainStation1 = '{5}', TrainStation2 = '{6}' WHERE Rent_ID = '{0}'",
                rl.RentId, rl.Name, rl.Type.ToString(), rl.PostalCode, rl.Location, rl.TrainStation1, rl.TrainStation2);

            string sqlUpdateRentLiving = String.Format("UPDATE RentLiving SET Kind = '{1}', Floors = '{2}', FloorsBasement = '{3}', BuiltYear = '{4}' WHERE RentLiving_ID = '{0}'",
                rl.RentLivingId, rl.Kind.ToString(), rl.Floors, rl.FloorsBasement, rl.BuiltYear);

            // TODO more to come

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        cmd.CommandText = sqlUpdateRent;
                        var result = cmd.ExecuteNonQuery();
                        if (result != 1)
                        {
                            // 
                        }

                        cmd.CommandText = sqlUpdateRentLiving;
                        result = cmd.ExecuteNonQuery();
                        if (result != 1)
                        {
                            // 
                        }

                        // 物件写真の追加または更新
                        if (rl.RentLivingPictures.Count > 0)
                        {
                            foreach (var pic in rl.RentLivingPictures)
                            {
                                if (pic.IsNew)
                                {
                                    string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}')",
                                        pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

                                    // 物件画像の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbW200xData;
                                    cmd.Parameters.Add(parameter2);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }
                                else if (pic.IsModified)
                                {
                                    string sqlUpdateRentLivingPicture = String.Format("UPDATE RentLivingPicture SET PictureData = @0, PictureThumbW200xData = @1, PictureFileExt = '{5}' WHERE RentLivingPicture_ID = '{0}'",
                                        pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

                                    // 物件画像の更新
                                    cmd.CommandText = sqlUpdateRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbW200xData;
                                    cmd.Parameters.Add(parameter2);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }

                            }
                        }

                        // 物件写真の削除リストを処理
                        if (rl.RentLivingPicturesToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in rl.RentLivingPicturesToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingPicture = String.Format("DELETE FROM RentLivingPicture WHERE RentLivingPicture_ID = '{0}'",
                                        id);

                                cmd.CommandText = sqlDeleteRentLivingPicture;
                                var DelRentLivingPicResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPicResult > 0)
                                {
                                    //
                                }
                            }
                            rl.RentLivingPicturesToBeDeletedIDs.Clear();
                        }

                        // 図面の更新
                        if (rl.RentLivingZumenPDFs.Count > 0)
                        {
                            foreach (var pdf in rl.RentLivingZumenPDFs)
                            {
                                if (pdf.IsNew)
                                {
                                    string sqlInsertIntoRentLivingZumen = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentZumenPdfId, rl.RentLivingId, rl.RentId, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumen;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PDFData;
                                    cmd.Parameters.Add(parameter1);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsDirty = false;
                                    }
                                }
                                else if (pdf.IsDirty)
                                {
                                    string sqlUpdateRentLivingZumen = String.Format("UPDATE RentLivingZumenPdf SET DateTimePublished = '{1}', DateTimeVerified = '{2}' WHERE RentLivingZumenPdf_ID = '{0}'",
                                        pdf.RentZumenPdfId, pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面アトリビュート情報の更新
                                    cmd.CommandText = sqlUpdateRentLivingZumen;

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsDirty = false;
                                    }
                                }
                            }
                        }

                        // 図面の削除リストを処理
                        if (rl.RentLivingZumenPdfToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in rl.RentLivingZumenPdfToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingPDF = String.Format("DELETE FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'",
                                        id);

                                cmd.CommandText = sqlDeleteRentLivingPDF;
                                var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPdfResult > 0)
                                {
                                    //
                                }
                            }
                            rl.RentLivingZumenPdfToBeDeletedIDs.Clear();
                        }

                        // 部屋更新
                        if (rl.RentLivingSections.Count > 0)
                        {
                            foreach (var room in rl.RentLivingSections)
                            {
                                if (room.IsNew)
                                {
                                    // 新規追加
                                    string sqlInsertIntoRentLivingSection = String.Format("INSERT INTO RentLivingSection (RentLivingSection_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingSectionId, rl.RentLivingId, rl.RentId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingSection;
                                    var InsertIntoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingSectionResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsDirty = false;
                                    }
                                }
                                // TODO:
                                //else if (room.IsDirty)
                                else
                                {
                                    // 更新
                                    string sqlUpdateRentLivingSection = String.Format("UPDATE RentLivingSection SET RoomNumber = '{1}', Price = '{2}', Madori = '{3}' WHERE RentLivingSection_ID = '{0}'",
                                            room.RentLivingSectionId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

                                    cmd.CommandText = sqlUpdateRentLivingSection;
                                    var UpdateoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (UpdateoRentLivingSectionResult > 0)
                                    {
                                        //room.IsNew = false;
                                        room.IsDirty = false;
                                    }
                                }

                                // 部屋の写真
                                if (room.RentLivingSectionPictures.Count > 0)
                                {
                                    foreach (var roompic in room.RentLivingSectionPictures)
                                    {
                                        if (roompic.IsNew)
                                        {
                                            string sqlInsertIntoRentLivingSectionPic = String.Format("INSERT INTO RentLivingSectionPicture (RentLivingSectionPicture_ID, RentLivingSection_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                        else if (roompic.IsModified)
                                        {
                                            string sqlUpdateRentLivingSectionPic = String.Format("UPDATE RentLivingSectionPicture SET PictureData = @0, PictureThumbW200xData = @1, PictureFileExt = '{6}' WHERE RentLivingSectionPicture_ID = '{0}'",
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlUpdateRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var UpdateRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (UpdateRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                    }
                                }

                                // 部屋画像の削除リストを処理
                                if (room.RentLivingSectionPicturesToBeDeletedIDs.Count > 0)
                                {
                                    foreach (var id in room.RentLivingSectionPicturesToBeDeletedIDs)
                                    {
                                        // 削除
                                        string sqlDeleteRentLivingSectionPicture = String.Format("DELETE FROM RentLivingSectionPicture WHERE RentLivingSectionPicture_ID = '{0}'",
                                                id);

                                        cmd.CommandText = sqlDeleteRentLivingSectionPicture;
                                        var UpdateoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                        if (UpdateoRentLivingSectionResult > 0)
                                        {
                                            //
                                        }
                                    }
                                }

                            }
                        }

                        // 部屋の削除リストを処理
                        if (rl.RentLivingSectionToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in rl.RentLivingSectionToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingSection = String.Format("DELETE FROM RentLivingSection WHERE RentLivingSection_ID = '{0}'", id);

                                cmd.CommandText = sqlDeleteRentLivingSection;
                                var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPdfResult > 0)
                                {
                                    //
                                }
                            }
                            rl.RentLivingSectionToBeDeletedIDs.Clear();
                        }

                        cmd.Transaction.Commit();

                        // 編集オブジェクトに格納された情報を、選択アイテムに更新（Listviewの情報が更新されるー＞DBからSelectして一覧を読み込みし直さなくて良くなる）
                        //RentLivingEditSelectedItem.Name = RentLivingEdit.Name;
                        //RentLivingEditSelectedItem.PostalCode = RentLivingEdit.PostalCode;
                        //RentLivingEditSelectedItem.Location = RentLivingEdit.Location;
                        //RentLivingEditSelectedItem.TrainStation1 = RentLivingEdit.TrainStation1;
                        //RentLivingEditSelectedItem.TrainStation2 = RentLivingEdit.TrainStation2;
                        // TODO

                        // 編集画面を非表示に
                        //if (ShowRentLivingEdit) ShowRentLivingEdit = false;
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::RentLivingUpdateCommand_Execute()");

                        // エラーイベント発火
                        MyError er = new MyError();
                        er.ErrType = "DB";
                        er.ErrCode = 0;
                        er.ErrText = "「" + e.Message + "」";
                        er.ErrDescription = "賃貸住居用物件の選択アイテム編集更新 (UPDATE)で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                        er.ErrDatetime = DateTime.Now;
                        er.ErrPlace = "MainViewModel::RentLivingUpdateCommand_Execute()";
                        //ErrorOccured?.Invoke(er);
                    }
                }
            }
        }

        // 賃貸住居用物件を削除（DELETE）
        public void DeleteRentLiving(string rentId)
        {
            // 選択アイテムのデータを削除

            string sqlRentTable = String.Format("DELETE FROM Rent WHERE Rent_ID = '{0}'", rentId);

            string sqlRentLivingTable = String.Format("DELETE FROM RentLiving WHERE Rent_ID = '{0}'", rentId);

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        cmd.CommandText = sqlRentLivingTable;
                        var result = cmd.ExecuteNonQuery();


                        cmd.CommandText = sqlRentTable;
                        result = cmd.ExecuteNonQuery();

                        // TODO その他の外部キー依存しているテーブルからも削除


                        cmd.Transaction.Commit();



                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        Debug.WriteLine(e.Message + " @RentLivingDeleteCommand_Execute()");

                        // エラーイベント発火
                        MyError er = new MyError();
                        er.ErrType = "DB";
                        er.ErrCode = 0;
                        er.ErrText = "「" + e.Message + "」";
                        er.ErrDescription = "賃貸住居用物件の選択アイテム削除（DELETE）で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                        er.ErrDatetime = DateTime.Now;
                        er.ErrPlace = "MainViewModel::RentLivingDeleteCommand_Execute()";
                        //ErrorOccured?.Invoke(er);
                    }
                }
            }
        }

    }

}
