using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ZumenSearch.Models;
using ZumenSearch.Common;
using System.Data;

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
        public ResultWrapper InitializeDatabase(string dataBaseFilePath)
        {
            ResultWrapper res = new ResultWrapper();

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
                                "PictureThumbData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "PictureType TEXT NOT NULL," +
                                "PictureDescription TEXT NOT NULL," +
                                "PictureIsMain INTERGER  NOT NULL," +
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
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingRoom(" +
                                "RentLivingRoom_ID TEXT NOT NULL PRIMARY KEY," +
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
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingRoomPicture (" +
                                "RentLivingRoomPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLivingRoom_ID TEXT NOT NULL," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PictureData BLOB NOT NULL," +
                                "PictureThumbData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)," +
                                "FOREIGN KEY (RentLivingRoom_ID) REFERENCES RentLivingRoom(RentLivingRoom_ID)" +
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

                            // エラー
                            res.IsError = true;
                            res.Error.ErrType = ErrorObject.ErrTypes.DB;
                            res.Error.ErrCode = 0;
                            res.Error.ErrText = "「" + e.Message + "」";
                            res.Error.ErrDescription = "データベースを初期化する処理でエラーが発生し、ロールバックしました。";
                            res.Error.ErrDatetime = DateTime.Now;
                            res.Error.ErrPlace = "@DataAccess::InitializeDatabase::Transaction.Commit";

                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;
                    res.Error.ErrText = "「" + ex.Message + "」";
                    res.Error.ErrDescription = "データベースを初期化する処理でエラーが発生しました。";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "TargetInvocationException@DataAccess::InitializeDatabase::connection.Open";

                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;
                    res.Error.ErrText = "「" + ex.Message + "」";
                    res.Error.ErrDescription = "データベースを初期化する処理でエラーが発生しました。";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "InvalidOperationException@DataAccess::InitializeDatabase::connection.Open";

                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = 0;

                    if (e.InnerException != null)
                    {
                        res.Error.ErrText = "「" + e.InnerException.Message + "」";
                    }
                    else
                    {
                        res.Error.ErrText = "「" + e.Message + "」";
                    }
                    res.Error.ErrDescription = "データベースを初期化する処理でエラーが発生しました。";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "Exception@DataAccess::InitializeDatabase::connection.Open";
                }

            }

            return res;
        }

        // 賃貸住居用一覧
        public ResultWrapper RentLivingList(ObservableCollection<RentLiving> rents)
        {
            ResultWrapper res = new ResultWrapper();

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
                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;

                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess::GetListOfRentLiving");
                    res.Error.ErrText = "「" + e.InnerException.Message + "」";
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess::GetListOfRentLiving");
                    res.Error.ErrText = "「" + e.Message + "」";
                }
                res.Error.ErrDescription = "賃貸住居用物件の一覧取得処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Exception@DataAccess::GetListOfRentLiving";

            }

            return res;
        }

        // 賃貸住居用検索
        public ResultWrapper RentLivingSearch(ObservableCollection<RentLiving> rents, string searchText)
        {
            ResultWrapper res = new ResultWrapper();

            rents.Clear();

            try
            {
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
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("Opps. TargetInvocationException@DataAccess::GetSearchResultOfRentLiving");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "賃貸住居用物件の検索結果取得処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "TargetInvocationException@DataAccess::GetSearchResultOfRentLiving::connection.Open";

                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("Opps. InvalidOperationException@DataAccess::GetSearchResultOfRentLiving");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "賃貸住居用物件の検索結果取得処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "InvalidOperationException@DataAccess::GetSearchResultOfRentLiving";

                throw ex.InnerException;
            }
            catch (Exception e)
            {
                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;

                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess::GetSearchResultOfRentLiving");
                    res.Error.ErrText = "「" + e.InnerException.Message + "」";
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess::GetSearchResultOfRentLiving");
                    res.Error.ErrText = "「" + e.Message + "」";
                }
                res.Error.ErrDescription = "賃貸住居用物件の検索結果取得処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Exception@DataAccess::GetSearchResultOfRentLiving";

            }


            return res;
        }

        // 賃貸住居用物件オブジェクトをIDで取得
        public ResultWrapper RentLivingSelectById(string rentId, string rentLivingId)
        {
            ResultWrapper res = new ResultWrapper();

            RentLiving rl = new RentLiving(rentId, rentLivingId);

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

                            rlpic.Picture = Methods.BitmapImageFromBytes(imageBytes);

                            byte[] imageThumbBytes = (byte[])reader["PictureThumbData"];
                            rlpic.PictureThumbData = imageThumbBytes;

                            rlpic.PictureThumb = Methods.BitmapImageFromBytes(imageThumbBytes);

                            rlpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                            rlpic.PictureType = Convert.ToString(reader["PictureType"]);

                            rlpic.PictureDescription = Convert.ToString(reader["PictureDescription"]);

                            // test
                            if (ColumnExists(reader, "PictureIsMain"))
                            {
                                var bln = Convert.ToInt32(reader["PictureIsMain"]);
                                if (bln > 0)
                                    rlpic.PictureIsMain = true;
                                else
                                    rlpic.PictureIsMain = false;
                            }

                            rlpic.IsNew = false;
                            rlpic.IsModified = false;

                            rl.RentLivingPictures.Add(rlpic);

                        }
                    }

                    // 図面
                    cmd.CommandText = String.Format("SELECT * FROM RentLivingZumenPdf WHERE Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RentLivingPdf rlpdf = new RentLivingPdf(rentId, rentLivingId, Convert.ToString(reader["RentLivingZumenPdf_ID"]));

                            byte[] pdfBytes = (byte[])reader["PdfData"];
                            rlpdf.PdfData = pdfBytes;


                            DateTime dt = new DateTime();

                            dt = DateTime.Parse(Convert.ToString(reader["DateTimeAdded"]));
                            rlpdf.DateTimeAdded = dt.ToLocalTime();
                            dt = DateTime.Parse(Convert.ToString(reader["DateTimePublished"]));
                            rlpdf.DateTimePublished = dt.ToLocalTime();
                            dt = DateTime.Parse(Convert.ToString(reader["DateTimeVerified"]));
                            rlpdf.DateTimeVerified = dt.ToLocalTime();

                            rlpdf.FileSize = Convert.ToInt64(reader["FileSize"]);

                            rlpdf.IsNew = false;

                            rl.RentLivingPdfs.Add(rlpdf);
                        }
                    }

                    // 部屋
                    cmd.CommandText = String.Format("SELECT * FROM RentLivingRoom WHERE Rent_ID = '{0}'", rentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RentLivingRoom room = new RentLivingRoom(rentId, rentLivingId, Convert.ToString(reader["RentLivingRoom_ID"]));
                            room.RentLivingRoomRoomNumber = Convert.ToString(reader["RoomNumber"]);
                            room.RentLivingRoomMadori = Convert.ToString(reader["Madori"]);
                            room.RentLivingRoomPrice = Convert.ToInt32(reader["Price"]);

                            room.IsNew = false;
                            room.IsModified = false;

                            rl.RentLivingRooms.Add(room);
                        }
                    }

                    // 部屋写真
                    foreach (var hoge in rl.RentLivingRooms)
                    {
                        cmd.CommandText = String.Format("SELECT * FROM RentLivingRoomPicture WHERE RentLivingRoom_ID = '{0}'", hoge.RentLivingRoomId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingRoomPicture rlsecpic = new RentLivingRoomPicture(rentId, rentLivingId, hoge.RentLivingRoomId, Convert.ToString(reader["RentLivingRoomPicture_ID"]));

                                byte[] imageBytes = (byte[])reader["PictureData"];
                                rlsecpic.PictureData = imageBytes;

                                byte[] imageThumbBytes = (byte[])reader["PictureThumbData"];
                                rlsecpic.PictureThumbData = imageThumbBytes;


                                rlsecpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                                rlsecpic.Picture = Methods.BitmapImageFromBytes(imageThumbBytes);

                                rlsecpic.IsNew = false;
                                rlsecpic.IsModified = false;

                                hoge.RentLivingRoomPictures.Add(rlsecpic);

                            }
                        }

                    }

                    cmd.Transaction.Commit();
                }
            }

            // 大事
            rl.IsNew = false;
            rl.IsDirty = false;

            res.Data = rl;

            return res;
        }

        // 賃貸住居用物件を追加（INSERT）
        public ResultWrapper RentLivingInsert(RentLiving rl)
        {
            ResultWrapper res = new ResultWrapper();

            string sqlInsertIntoRent = String.Format(
                "INSERT INTO Rent " +
                "(Rent_ID, Name, Type, PostalCode, Location, TrainStation1, TrainStation2) " +
                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                rl.RentId, 
                rl.Name, 
                rl.Type.ToString(), 
                rl.PostalCode, 
                rl.Location, 
                rl.TrainStation1, 
                rl.TrainStation2);

            string sqlInsertIntoRentLiving = String.Format(
                "INSERT INTO RentLiving " +
                "(RentLiving_ID, Rent_ID, Kind, Floors, FloorsBasement, BuiltYear) " +
                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                rl.RentLivingId, 
                rl.RentId, 
                rl.Kind.ToString(), 
                rl.Floors, 
                rl.FloorsBasement, 
                rl.BuiltYear);

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
                                        string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbData, PictureFileExt, PictureType, PictureDescription) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}','{6}','{7}')",
                                            pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbData, pic.PictureFileExt, pic.PictureType, pic.PictureDescription);

                                        // 物件画像の追加
                                        cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                        // ループなので、前のパラメーターをクリアする。
                                        cmd.Parameters.Clear();

                                        SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                        parameter1.Value = pic.PictureData;
                                        cmd.Parameters.Add(parameter1);

                                        SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                        parameter2.Value = pic.PictureThumbData;
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
                            if (rl.RentLivingPdfs.Count > 0)
                            {
                                foreach (var pdf in rl.RentLivingPdfs)
                                {
                                    string sqlInsertIntoRentLivingZumenPdf = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentPdfId, rl.RentLivingId, rl.RentId, pdf.PdfData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumenPdf;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PdfData;
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
                            if (rl.RentLivingRooms.Count > 0)
                            {
                                foreach (var room in rl.RentLivingRooms)
                                {
                                    string sqlInsertIntoRentLivingRoom = String.Format("INSERT INTO RentLivingRoom (RentLivingRoom_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingRoomId, rl.RentLivingId, rl.RentId, room.RentLivingRoomRoomNumber, room.RentLivingRoomPrice, room.RentLivingRoomMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingRoom;
                                    var InsertIntoRentLivingRoomResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingRoomResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsModified = false;
                                    }

                                    // 部屋の写真
                                    if (room.RentLivingRoomPictures.Count > 0)
                                    {
                                        foreach (var roompic in room.RentLivingRoomPictures)
                                        {
                                            string sqlInsertIntoRentLivingRoomPic = String.Format("INSERT INTO RentLivingRoomPicture (RentLivingRoomPicture_ID, RentLivingRoom_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPictureId, roompic.RentLivingRoomId, rl.RentLivingId, rl.RentId, roompic.PictureData, roompic.PictureThumbData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingRoomPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingRoomPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingRoomPicResult > 0)
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

                            res.IsError = false;
                        }
                        catch (Exception e)
                        {
                            res.IsError = true;

                            // ロールバック
                            cmd.Transaction.Rollback();

                            // エラー
                            res.Error.ErrType = ErrorObject.ErrTypes.DB;
                            res.Error.ErrCode = 0;
                            res.Error.ErrText = "「" + e.Message + "」";
                            res.Error.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生し、ロールバックしました。";
                            res.Error.ErrDatetime = DateTime.Now;
                            res.Error.ErrPlace = "@DataAccess::AddRentLiving::Transaction.Commit";

                            Debug.WriteLine(e.Message + " @DataAccess::AddRentLiving::Transaction.Commit");
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("Opps. TargetInvocationException@DataAccess::AddRentLiving");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "TargetInvocationException@DataAccess::AddRentLiving::connection.Open";

                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("Opps. InvalidOperationException@DataAccess::AddRentLiving");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                res.Error.ErrText = "「" + ex.Message + "」";
                res.Error.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "InvalidOperationException@DataAccess::AddRentLiving::connection.Open";

                throw ex.InnerException;
            }
            catch (Exception e)
            {
                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = 0;
                
                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @DataAccess::AddRentLiving");
                    res.Error.ErrText = "「" + e.InnerException.Message + "」";
                }
                else
                {
                    Debug.WriteLine(e.Message + " @DataAccess::AddRentLiving");
                    res.Error.ErrText = "「" + e.Message + "」";
                }
                res.Error.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生しました。";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Exception@DataAccess::AddRentLiving::connection.Open";
            }

            return res;
        }

        // 賃貸住居用物件を更新（UPDATE）
        public ResultWrapper RentLivingUpdate(RentLiving rl)
        {
            ResultWrapper res = new ResultWrapper();

            string sqlUpdateRent = String.Format(
                "UPDATE Rent SET " +
                    "Name = '{1}', " +
                    "Type = '{2}', " +
                    "PostalCode = '{3}', " +
                    "Location = '{4}', " +
                    "TrainStation1 = '{5}', " +
                    "TrainStation2 = '{6}' " +
                        "WHERE Rent_ID = '{0}'",
                rl.RentId, 
                rl.Name, 
                rl.Type.ToString(), 
                rl.PostalCode, 
                rl.Location, 
                rl.TrainStation1, 
                rl.TrainStation2);

            string sqlUpdateRentLiving = String.Format(
                "UPDATE RentLiving SET " +
                    "Kind = '{1}', " +
                    "Floors = '{2}', " +
                    "FloorsBasement = '{3}', " +
                    "BuiltYear = '{4}' " +
                        "WHERE RentLiving_ID = '{0}'",
                rl.RentLivingId, 
                rl.Kind.ToString(), 
                rl.Floors, 
                rl.FloorsBasement, 
                rl.BuiltYear);

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
                                    string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbData, PictureFileExt, PictureType, PictureDescription) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}', '{6}', '{7}')",
                                        pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbData, pic.PictureFileExt, pic.PictureType, pic.PictureDescription);

                                    // 物件画像の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbData;
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
                                    string sqlUpdateRentLivingPicture = String.Format("UPDATE RentLivingPicture SET PictureData = @0, PictureThumbData = @1, PictureFileExt = '{5}', PictureType = '{6}', PictureDescription = '{7}' WHERE RentLivingPicture_ID = '{0}'",
                                        pic.RentPictureId, rl.RentLivingId, rl.RentId, pic.PictureData, pic.PictureThumbData, pic.PictureFileExt, pic.PictureType, pic.PictureDescription);

                                    // 物件画像の更新
                                    cmd.CommandText = sqlUpdateRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbData;
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
                        if (rl.RentLivingPdfs.Count > 0)
                        {
                            foreach (var pdf in rl.RentLivingPdfs)
                            {
                                if (pdf.IsNew)
                                {
                                    string sqlInsertIntoRentLivingZumen = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentPdfId, rl.RentLivingId, rl.RentId, pdf.PdfData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumen;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PdfData;
                                    cmd.Parameters.Add(parameter1);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsModified = false;
                                    }
                                }
                                else if (pdf.IsModified)
                                {
                                    string sqlUpdateRentLivingZumen = String.Format("UPDATE RentLivingZumenPdf SET DateTimePublished = '{1}', DateTimeVerified = '{2}' WHERE RentLivingZumenPdf_ID = '{0}'",
                                        pdf.RentPdfId, pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面アトリビュート情報の更新
                                    cmd.CommandText = sqlUpdateRentLivingZumen;

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsModified = false;
                                    }
                                }
                            }
                        }

                        // 図面の削除リストを処理
                        if (rl.RentLivingPdfsToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in rl.RentLivingPdfsToBeDeletedIDs)
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
                            rl.RentLivingPdfsToBeDeletedIDs.Clear();
                        }

                        // 部屋更新
                        if (rl.RentLivingRooms.Count > 0)
                        {
                            foreach (var room in rl.RentLivingRooms)
                            {
                                if (room.IsNew)
                                {
                                    // 新規追加
                                    string sqlInsertIntoRentLivingRoom = String.Format("INSERT INTO RentLivingRoom (RentLivingRoom_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingRoomId, rl.RentLivingId, rl.RentId, room.RentLivingRoomRoomNumber, room.RentLivingRoomPrice, room.RentLivingRoomMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingRoom;
                                    var InsertIntoRentLivingRoomResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingRoomResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsModified = false;
                                    }
                                }
                                // TODO:
                                //else if (room.IsDirty)
                                else
                                {
                                    // 更新
                                    string sqlUpdateRentLivingRoom = String.Format("UPDATE RentLivingRoom SET RoomNumber = '{1}', Price = '{2}', Madori = '{3}' WHERE RentLivingRoom_ID = '{0}'",
                                            room.RentLivingRoomId, room.RentLivingRoomRoomNumber, room.RentLivingRoomPrice, room.RentLivingRoomMadori);

                                    cmd.CommandText = sqlUpdateRentLivingRoom;
                                    var UpdateoRentLivingRoomResult = cmd.ExecuteNonQuery();
                                    if (UpdateoRentLivingRoomResult > 0)
                                    {
                                        //room.IsNew = false;
                                        room.IsModified = false;
                                    }
                                }

                                // 部屋の写真
                                if (room.RentLivingRoomPictures.Count > 0)
                                {
                                    foreach (var roompic in room.RentLivingRoomPictures)
                                    {
                                        if (roompic.IsNew)
                                        {
                                            string sqlInsertIntoRentLivingRoomPic = String.Format("INSERT INTO RentLivingRoomPicture (RentLivingRoomPicture_ID, RentLivingRoom_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPictureId, roompic.RentLivingRoomId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingRoomPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingRoomPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingRoomPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                        else if (roompic.IsModified)
                                        {
                                            string sqlUpdateRentLivingRoomPic = String.Format("UPDATE RentLivingRoomPicture SET PictureData = @0, PictureThumbData = @1, PictureFileExt = '{6}' WHERE RentLivingRoomPicture_ID = '{0}'",
                                                roompic.RentSectionPictureId, roompic.RentLivingRoomId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlUpdateRentLivingRoomPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbData;
                                            cmd.Parameters.Add(parameter2);

                                            var UpdateRentLivingRoomPicResult = cmd.ExecuteNonQuery();
                                            if (UpdateRentLivingRoomPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                    }
                                }

                                // 部屋画像の削除リストを処理
                                if (room.RentLivingRoomPicturesToBeDeletedIDs.Count > 0)
                                {
                                    foreach (var id in room.RentLivingRoomPicturesToBeDeletedIDs)
                                    {
                                        // 削除
                                        string sqlDeleteRentLivingRoomPicture = String.Format("DELETE FROM RentLivingRoomPicture WHERE RentLivingRoomPicture_ID = '{0}'",
                                                id);

                                        cmd.CommandText = sqlDeleteRentLivingRoomPicture;
                                        var UpdateoRentLivingRoomResult = cmd.ExecuteNonQuery();
                                        if (UpdateoRentLivingRoomResult > 0)
                                        {
                                            //
                                        }
                                    }
                                }

                            }
                        }

                        // 部屋の削除リストを処理
                        if (rl.RentLivingRoomToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in rl.RentLivingRoomToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingRoom = String.Format("DELETE FROM RentLivingRoom WHERE RentLivingRoom_ID = '{0}'", id);

                                cmd.CommandText = sqlDeleteRentLivingRoom;
                                var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPdfResult > 0)
                                {
                                    //
                                }
                            }
                            rl.RentLivingRoomToBeDeletedIDs.Clear();
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
                        // ロールバック
                        cmd.Transaction.Rollback();

                        // エラー
                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = 0;
                        res.Error.ErrText = "「" + e.Message + "」";
                        res.Error.ErrDescription = "賃貸住居用物件の編集更新 (UPDATE)で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "@DataAccess::UpdateRentLiving::Transaction.Commit";

                    }
                }
            }

            return res;
        }

        // 賃貸住居用物件を削除（DELETE）
        public ResultWrapper RentLivingDelete(string rentId)
        {
            ResultWrapper res = new ResultWrapper();

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

                        // エラー
                        res.IsError = true;
                        res.Error.ErrType = ErrorObject.ErrTypes.DB;
                        res.Error.ErrCode = 0;
                        res.Error.ErrText = "「" + e.Message + "」";
                        res.Error.ErrDescription = "賃貸住居用物件の選択アイテム削除（DELETE）で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                        res.Error.ErrDatetime = DateTime.Now;
                        res.Error.ErrPlace = "@DataAccess::DeleteRentLiving::Transaction.Commit";

                    }
                }
            }

            return res;
        }

        // Column存在チェック
        public bool ColumnExists(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false; ;
        }

    }

}
