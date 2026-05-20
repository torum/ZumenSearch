using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using ZumenSearch.Helpers;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ZumenSearch.Services;

public class DataAccessService : IDataAccessService
{
    private SqliteConnectionStringBuilder connectionStringBuilder = [];

    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new SqliteDataAccessResultWrapper();

        try
        {
            connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dataBaseFilePath);//+ ";Pooling=false"

            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

            if (!RuntimeHelper.IsMSIX)
            {
                Debug.WriteLine("* Microsoft.Data.Sqlite SqliteConnection() calls Windows.Storage.ApplicationData.Current.get() results in System.InvalidOperationException \"Operation is not valid due to the current state of the object.\" Since we are in unpackaged, we can safely ignore the exception.");
            }

            connection.Open();

            using var tableCmd = connection.CreateCommand();
            tableCmd.Transaction = connection.BeginTransaction();
            try
            {
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rents (" +
                    "rent_id TEXT NOT NULL PRIMARY KEY," +
                    "name TEXT NOT NULL," +
                    "loc_pref_id TEXT," +
                    "loc_prefecture TEXT," +
                    "loc_machiaza_id TEXT," +
                    "loc_county TEXT," +
                    "loc_city TEXT," +
                    "loc_ward TEXT," +
                    "loc_oaza_cho TEXT," +
                    "loc_choume TEXT," +
                    "loc_edaban TEXT," +
                    "loc_location_full TEXT" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residentials (" +
                    "rent_id TEXT NOT NULL PRIMARY KEY," +
                    //"residential_id TEXT NOT NULL," +
                    "comment TEXT NOT NULL," +
                    "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "rent_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "label TEXT NOT NULL," + // TODO: wanna change this to "type".
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "FOREIGN KEY (rent_id) REFERENCES rent_residentials(rent_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pdfs (" +
                    "pdf_id TEXT NOT NULL PRIMARY KEY," +
                    "rent_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "thumb_path TEXT NOT NULL," + 
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "FOREIGN KEY (rent_id) REFERENCES rent_residentials(rent_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_rooms (" +
                    "room_id TEXT NOT NULL PRIMARY KEY," +
                    "rent_id TEXT NOT NULL," +
                    "name TEXT NOT NULL," +
                    "chinryou INTEGER NOT NULL DEFAULT 0," +
                    "FOREIGN KEY (rent_id) REFERENCES rent_residentials(rent_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                // ADD COLUMN chinryou.
                try
                {
                    tableCmd.CommandText = "ALTER TABLE rent_residential_rooms ADD COLUMN chinryou INTEGER NOT NULL DEFAULT 0;";
                    tableCmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    // SQLite does not support "IF NOT EXISTS" for ADD COLUMN.
                    // need to catch "duplicate column name" errors.
                    Debug.WriteLine("SqliteException on ADD COLUMN chinryou @InitializeDatabase: " + ex.Message);
                }

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_room_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "room_id TEXT NOT NULL," +
                    "rent_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "type TEXT NOT NULL," + // This uses "type" instead of label.
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "FOREIGN KEY (room_id) REFERENCES rent_residential_rooms(room_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (rent_id) REFERENCES rent_residentials(rent_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                // ADD COLUMN room_id.
                try
                {
                    tableCmd.CommandText = "ALTER TABLE rent_residential_room_pictures ADD COLUMN room_id TEXT NOT NULL DEFAULT '';";
                    tableCmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    // SQLite does not support "IF NOT EXISTS" for ADD COLUMN.
                    // need to catch "duplicate column name" errors.
                    Debug.WriteLine("SqliteException on ADD COLUMN room_id @InitializeDatabase: " + ex.Message);
                }

                //
                //tableCmd.CommandText = "drop trigger if exists trigger_delete_old_entries";
                //tableCmd.ExecuteNonQuery();
                /*
                tableCmd.CommandText = "CREATE TRIGGER IF NOT EXISTS trigger_delete_old_entries AFTER INSERT ON entries";
                tableCmd.CommandText += " BEGIN";
                tableCmd.CommandText += " delete from entries where";
                tableCmd.CommandText += " entry_id = (select min(entry_id) from entries)";
                tableCmd.CommandText += " and (select count(*) from entries) > 1000;";
                tableCmd.CommandText += " END;";
                tableCmd.ExecuteNonQuery();
                */
                /*
                tableCmd.CommandText = "CREATE TRIGGER IF NOT EXISTS trigger_delete_old_entries AFTER INSERT ON entries";
                tableCmd.CommandText += " WHEN (SELECT COUNT(*) FROM entries) > 1000";
                tableCmd.CommandText += " BEGIN";
                tableCmd.CommandText += " DELETE FROM entries WHERE entry_id NOT IN (SELECT entry_id FROM entries ORDER BY published DESC LIMIT 1000);";
                tableCmd.CommandText += " END;";
                tableCmd.ExecuteNonQuery();
                */
                tableCmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                tableCmd.Transaction.Rollback();

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception while executing SQL queries";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "Transaction.Commit";
                res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

                return res;
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "TargetInvocationException while connecting to a SQL database file";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open";
            res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException while connecting to a SQL database file";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open";
            res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrDescription = "InnerException while connecting to a SQL database file";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                res.Error.ErrDescription = "Exception while connecting to a SQL database file";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open";
            res.Error.ErrPlaceParent = "DataAccess::InitializeDatabase";

            return res;
        }

        return res;
    }

    public SqliteDataAccessResultWrapper InsertRentResidential(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(entry.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                // Main rent table
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO rents (rent_id, name, loc_pref_id, loc_prefecture, loc_machiaza_id, loc_county, loc_city, loc_ward, loc_oaza_cho, loc_choume, loc_edaban, loc_location_full) " +
                    "VALUES (@RentId, @Name, @LocPrefId, @LocPrefecture, @LocMachiazaId, @LocCounty, @LocCity, @LocWard, @LocOazaCho, @LocChoume, @LocEdaban, @LocLocationFull)";

                cmd.Parameters.AddWithValue("@RentId", entry.Id);
                cmd.Parameters.AddWithValue("@Name", entry.Name);
                //cmd.Parameters.AddWithValue("@Updated", updated.ToString("yyyy-MM-dd HH:mm:ss"));

                cmd.Parameters.AddWithValue("@LocPrefId", entry.LocPrefId);
                cmd.Parameters.AddWithValue("@LocPrefecture", entry.LocPrefecture);
                cmd.Parameters.AddWithValue("@LocMachiazaId", entry.LocMachiazaId);
                cmd.Parameters.AddWithValue("@LocCounty", entry.LocCounty);
                cmd.Parameters.AddWithValue("@LocCity", entry.LocCity);
                cmd.Parameters.AddWithValue("@LocWard", entry.LocWard);
                cmd.Parameters.AddWithValue("@LocOazaCho", entry.LocOazaCho);
                cmd.Parameters.AddWithValue("@LocChoume", entry.LocChoume);
                cmd.Parameters.AddWithValue("@LocEdaban", entry.LocEdaban);
                cmd.Parameters.AddWithValue("@LocLocationFull", entry.LocLocationFull);

                // TODO: more

                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // Residentials table
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO rent_residentials (rent_id, comment) VALUES (@RentId, @Comment)"; //@RentResidentialId,  rent_residential_id, 

                //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                cmd.Parameters.AddWithValue("@RentId", entry.Id);
                cmd.Parameters.AddWithValue("@Comment", "");

                cmd.ExecuteNonQuery();

                // Picture (building) table
                if (entry.BuildingPictures.Count > 0)
                {
                    foreach (var pic in entry.BuildingPictures)
                    {
                        // Insertなので全てIsNewのはず・・・
                        //if (pic.IsNew)
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                                        "INSERT INTO rent_residential_pictures (picture_id, rent_id, picture_filepath, picture_data) " +
                                        "VALUES ('{0}', '{1}', '{2}', @0)",
                                        pic.Id, entry.Id, pic.ImageLocation);
                        */
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                            "INSERT INTO rent_residential_pictures (picture_id, rent_id, picture_filepath) " +
                            "VALUES ('{0}', '{1}', '{2}')",
                            pic.Id, entry.Id, pic.ImageLocation);
                        */
                        var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pictures (picture_id, rent_id, file_path, label, description, is_main) " + 
                            "VALUES (@PicId, @RentId, @Path, @Tit, @Desc, @Main)";

                        cmd.CommandText = sqlInsertIntoRentLivingPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@PicId", pic.Id);
                        //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                        cmd.Parameters.AddWithValue("@RentId", entry.Id);
                        cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                        cmd.Parameters.AddWithValue("@Tit", pic.PictureType.Key.ToString());
                        cmd.Parameters.AddWithValue("@Desc", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentResidential");

                        var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                        if (pic.IsMain)
                        {
                            paramIsMain.Value = 1;
                        }
                        else
                        {
                            paramIsMain.Value = 0;
                        }
                        cmd.Parameters.Add(paramIsMain);

                        /*
                        var parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary)
                        {
                            Value = pic.PictureData;
                        };
                        cmd.Parameters.Add(parameter1);
                        */

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            pic.IsNew = false;
                            pic.IsModified = false;
                        }
                    }
                }

                // PDF (building) table
                if (entry.BuildingPdfs.Count > 0)
                {
                    foreach (var pic in entry.BuildingPdfs)
                    {
                        // Insertなので全てIsNewのはず・・・
                        //if (pic.IsNew)
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                                        "INSERT INTO rent_residential_pictures (picture_id, rent_id, picture_filepath, picture_data) " +
                                        "VALUES ('{0}', '{1}', '{2}', @0)",
                                        pic.Id, entry.Id, pic.ImageLocation);
                        */
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                            "INSERT INTO rent_residential_pictures (picture_id, rent_id, picture_filepath) " +
                            "VALUES ('{0}', '{1}', '{2}')",
                            pic.Id, entry.Id, pic.ImageLocation);
                        */
                        var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pdfs (pdf_id, rent_id, file_path, thumb_path, description, is_main) " +
                            "VALUES (@PdfId, @RentId, @Path, @Thumb, @Desc, @Main)";

                        cmd.CommandText = sqlInsertIntoRentLivingPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@PdfId", pic.Id);
                        //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                        cmd.Parameters.AddWithValue("@RentId", entry.Id);
                        cmd.Parameters.AddWithValue("@Path", pic.PdfLocation);
                        cmd.Parameters.AddWithValue("@Thumb", pic.ThumbnailLocation);
                        cmd.Parameters.AddWithValue("@Desc", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentResidential");

                        var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                        if (pic.IsMain)
                        {
                            paramIsMain.Value = 1;
                        }
                        else
                        {
                            paramIsMain.Value = 0;
                        }
                        cmd.Parameters.Add(paramIsMain);

                        /*
                        var parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary)
                        {
                            Value = pic.PictureData;
                        };
                        cmd.Parameters.Add(parameter1);
                        */

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            pic.IsNew = false;
                            pic.IsModified = false;
                        }
                    }
                }

                // Room table
                if (entry.Rooms.Count > 0)
                {
                    foreach (var unit in entry.Rooms)
                    {
                        var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, rent_id, name, chinryou) VALUES (@RoomId, @RentId, @Name, @Chinryou)";

                        cmd.CommandText = sqlInsertIntoRentLivingRoom;   

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@RoomId", unit.Id);
                        cmd.Parameters.AddWithValue("@RentId", entry.Id);
                        cmd.Parameters.AddWithValue("@Name", unit.Name);
                        cmd.Parameters.AddWithValue("@Chinryou", unit.Chinryou);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            unit.IsNew = false;
                            unit.IsModified = false;
                        }

                        // Room Pics
                        if (unit.UnitPictures.Count > 0)
                        {
                            foreach (var pic in unit.UnitPictures)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, rent_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
                                sqlUpsertRoom += "ON CONFLICT(picture_id) ";
                                sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                                var exec = true;

                                cmd.CommandText = sqlUpsertRoom;

                                if (exec)
                                {
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@PicId", pic.Id);
                                    cmd.Parameters.AddWithValue("@roomId", unit.Id);
                                    cmd.Parameters.AddWithValue("@RentId", entry.Id);
                                    cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                                    cmd.Parameters.AddWithValue("@type", pic.PictureType.Key.ToString());
                                    cmd.Parameters.AddWithValue("@Desc", pic.Description);
                                    var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                                    if (pic.IsMain)
                                    {
                                        paramIsMain.Value = 1;
                                    }
                                    else
                                    {
                                        paramIsMain.Value = 0;
                                    }
                                    cmd.Parameters.Add(paramIsMain);

                                    var result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }
                            }
                        }
                    }
                }


                // commit
                cmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                cmd.Transaction.Rollback();

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                res.Error.ErrPlaceParent = "DataAccess::InsertRentResidential";

                return res;
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertRentResidential";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertRentResidential");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertRentResidential";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = e.InnerException.Message;
                res.Error.ErrDescription = "InnerException";
            }
            else
            {
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::InsertRentResidential";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public SqliteDataAccessResultWrapper UpdateRentResidential(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(entry.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandType = CommandType.Text;

                // Rents table
                var sql = "UPDATE rents SET ";
                sql += string.Format("name = '{0}', ", EscapeSingleQuote(entry.Name));
                sql += string.Format("loc_pref_id = '{0}', ", EscapeSingleQuote(entry.LocPrefId));
                sql += string.Format("loc_prefecture = '{0}', ", EscapeSingleQuote(entry.LocPrefecture));
                sql += string.Format("loc_machiaza_id = '{0}', ", EscapeSingleQuote(entry.LocMachiazaId));
                sql += string.Format("loc_county = '{0}', ", EscapeSingleQuote(entry.LocCounty));
                sql += string.Format("loc_city = '{0}', ", EscapeSingleQuote(entry.LocCity));
                sql += string.Format("loc_ward = '{0}', ", EscapeSingleQuote(entry.LocWard));
                sql += string.Format("loc_oaza_cho = '{0}', ", EscapeSingleQuote(entry.LocOazaCho));
                sql += string.Format("loc_choume = '{0}', ", EscapeSingleQuote(entry.LocChoume));
                sql += string.Format("loc_edaban = '{0}', ", EscapeSingleQuote(entry.LocEdaban));
                sql += string.Format("loc_location_full = '{0}' ", EscapeSingleQuote(entry.LocLocationFull)); // 最後カンマ無し 注意

                // TODO: more

                sql += string.Format(" WHERE rent_id = '{0}'; ", entry.Id);

                cmd.CommandText = sql;
                res.AffectedCount = cmd.ExecuteNonQuery();

                // Residentials table
                sql = "UPDATE rent_residentials SET ";
                sql += string.Format("comment = '{0}' ", EscapeSingleQuote("some comment"));
                //sql += String.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                //sql += String.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                //sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));

                sql += string.Format(" WHERE rent_id = '{0}'; ", entry.Id);

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                
                cmd.Parameters.Clear();

                // 写真（建物）Residentials pictures table - Insert or Update
                if (entry.BuildingPictures.Count > 0)
                {
                    foreach (var pic in entry.BuildingPictures)
                    {
                        var exec = false;

                        if (pic.IsNew)
                        {
                            var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pictures (picture_id, rent_id, file_path, label, description, is_main) " +
                                "VALUES (@PicId, @RentId, @Path, @Tit, @Desc, @Main)";

                            // 物件画像の追加
                            cmd.CommandText = sqlInsertIntoRentLivingPicture;

                            exec = true;
                        }
                        else if (pic.IsModified)
                        {
                            var sqlUpdateRentLivingPicture = string.Format(
                                "UPDATE rent_residential_pictures SET file_path = @Path, label = @Tit, description = @Desc, is_main = @Main " +
                                "WHERE picture_id = '{0}'", pic.Id);

                            // 物件画像の更新
                            cmd.CommandText = sqlUpdateRentLivingPicture;

                            exec = true;
                        }

                        if (exec)
                        {
                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@PicId", pic.Id);
                            //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                            cmd.Parameters.AddWithValue("@RentId", entry.Id);
                            cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                            cmd.Parameters.AddWithValue("@Tit", pic.PictureType.Key.ToString());
                            cmd.Parameters.AddWithValue("@Desc", pic.Description);
                            var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                            if (pic.IsMain)
                            {
                                paramIsMain.Value = 1;
                            }
                            else
                            {
                                paramIsMain.Value = 0;
                            }
                            cmd.Parameters.Add(paramIsMain);

                            var result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                pic.IsNew = false;
                                pic.IsModified = false;
                            }
                        }

                    }
                }

                cmd.Parameters.Clear();

                // 物件写真の削除リストを処理
                if (entry.BuildingPicturesToBeDeleted.Count > 0)
                {
                    foreach (var delp in entry.BuildingPicturesToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingPicture = string.Format("DELETE FROM rent_residential_pictures WHERE picture_id = '{0}'", delp.Id);

                        cmd.CommandText = sqlDeleteRentLivingPicture;
                        var DelRentLivingPicResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingPicResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Picture deleted");
                        }
                    }

                    // let's not. Needs this to clean up the file.
                    //entry.BuildingPicturesToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // PDF（建物）Residentials pdfs table - Insert or Update
                if (entry.BuildingPdfs.Count > 0)
                {
                    foreach (var pdf in entry.BuildingPdfs)
                    {
                        var exec = false;

                        if (pdf.IsNew)
                        {
                            var sqlInsertIntoRentLivingPdf = "INSERT INTO rent_residential_pdfs (pdf_id, rent_id, file_path, thumb_path, description, is_main) " +
                                "VALUES (@PdfId, @RentId, @Path, @Thumb, @Desc, @Main)";

                            // PDFの追加
                            cmd.CommandText = sqlInsertIntoRentLivingPdf;

                            exec = true;
                        }
                        else if (pdf.IsModified)
                        {
                            var sqlUpdateRentLivingPdf = string.Format(
                                "UPDATE rent_residential_pdfs SET file_path = @Path, thumb_path = @Thumb, description = @Desc, is_main = @Main " +
                                "WHERE pdf_id = '{0}'", pdf.Id);

                            // PDFの更新
                            cmd.CommandText = sqlUpdateRentLivingPdf;

                            exec = true;
                        }

                        if (exec)
                        {
                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@PdfId", pdf.Id);
                            cmd.Parameters.AddWithValue("@RentId", entry.Id);
                            cmd.Parameters.AddWithValue("@Path", pdf.PdfLocation);
                            cmd.Parameters.AddWithValue("@Thumb", pdf.ThumbnailLocation);
                            cmd.Parameters.AddWithValue("@Desc", pdf.Description);
                            var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                            if (pdf.IsMain)
                            {
                                paramIsMain.Value = 1;
                            }
                            else
                            {
                                paramIsMain.Value = 0;
                            }
                            cmd.Parameters.Add(paramIsMain);

                            var result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                pdf.IsNew = false;
                                pdf.IsModified = false;
                            }
                        }

                    }
                }

                cmd.Parameters.Clear();

                // PDF（建物）の削除リストを処理
                if (entry.BuildingPdfsToBeDeleted.Count > 0)
                {
                    foreach (var delp in entry.BuildingPdfsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingPdf = string.Format("DELETE FROM rent_residential_pdfs WHERE pdf_id = '{0}'", delp.Id);

                        cmd.CommandText = sqlDeleteRentLivingPdf;
                        var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingPdfResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Pdf deleted");
                        }
                    }

                    // let's not. Needs this to clean up the file.
                    //entry.BuildingPdfsToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // This currently may no be called since rooms are independently updated.(insert is a different story.)
                // 部屋 Rooms table - Insert, Update, Delete
                if (entry.Rooms.Count > 0)
                {
                    foreach (var room in entry.Rooms)
                    {
                        var exec = false;

                        if (room.IsNew)
                        {
                            var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, rent_id, name, chinryou) VALUES (@roomId, @RentId, @Nam, @Chinryou)";

                            // 追加
                            cmd.CommandText = sqlInsertIntoRentLivingRoom;
                            exec = true;
                        }
                        else if (room.IsModified)
                        {
                            //var sqlUpdateRentLivingRoom = string.Format("UPDATE rent_residential_rooms SET name = @Nam WHERE room_id = '{0}'", room.Id);
                            var sqlUpdateRentLivingRoom = "UPDATE rent_residential_rooms SET name = @Nam, chinryou = @Chinryou WHERE room_id = @roomId";
                            // 更新
                            cmd.CommandText = sqlUpdateRentLivingRoom;

                            exec = true;
                        }

                        if (exec)
                        {
                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@roomId", room.Id);
                            //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                            cmd.Parameters.AddWithValue("@RentId", entry.Id);
                            cmd.Parameters.AddWithValue("@Nam", room.Name);
                            cmd.Parameters.AddWithValue("@Chinryou", room.Chinryou);

                            var result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                room.IsNew = false;
                                room.IsModified = false;
                            }
                        }

                        // room Pic
                        if (room.UnitPictures.Count > 0)
                        {
                            foreach (var pic in room.UnitPictures)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, rent_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
                                sqlUpsertRoom += "ON CONFLICT(picture_id) ";
                                sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                                exec = true;

                                cmd.CommandText = sqlUpsertRoom;

                                if (exec)
                                {
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@PicId", pic.Id);
                                    cmd.Parameters.AddWithValue("@roomId", room.Id);
                                    cmd.Parameters.AddWithValue("@RentId", entry.Id);
                                    cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                                    cmd.Parameters.AddWithValue("@type", pic.PictureType.Key.ToString());
                                    cmd.Parameters.AddWithValue("@Desc", pic.Description);
                                    var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                                    if (pic.IsMain)
                                    {
                                        paramIsMain.Value = 1;
                                    }
                                    else
                                    {
                                        paramIsMain.Value = 0;
                                    }
                                    cmd.Parameters.Add(paramIsMain);

                                    var result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }
                            }
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 部屋の削除リストを処理
                if (entry.RoomsToBeDeleted.Count > 0)
                {
                    foreach (var delr in entry.RoomsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingRoom = string.Format("DELETE FROM rent_residential_rooms WHERE room_id = '{0}'", delr.Id);

                        cmd.CommandText = sqlDeleteRentLivingRoom;
                        var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingRoomResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Room deleted");
                        }
                    }
                    entry.RoomsToBeDeleted.Clear();
                }

                // Commit
                cmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                cmd.Transaction.Rollback();

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                res.Error.ErrPlaceParent = "DataAccess::UpdateRentResidential";

                return res;
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateRentResidential";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::UpdateRentResidential");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateRentResidential";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = e.InnerException.Message;
                res.Error.ErrDescription = "InnerException";
            }
            else
            {
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateRentResidential";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public SqliteDataAccessResultWrapper DeleteRentResidential(string rentId)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(rentId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandText = string.Format("DELETE FROM rents WHERE rent_id = '{0}';", rentId);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                cmd.Transaction.Rollback();

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "cmd.ExecuteNonQuery(),Transaction.Commit()";
                res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidential";

                return res;
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidential";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidential";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.Message + " " + e.InnerException.Message;
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteRentResidential");
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
                Debug.WriteLine(e.Message + " @DataAccess::DeleteFeed");
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidential";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    public SqliteDataAccessSelectRentResidentialResultWrapper SelectRentResidentialsByNameKeyword(string keyword)
    {
        var res = new SqliteDataAccessSelectRentResidentialResultWrapper();

        if (string.IsNullOrEmpty(keyword))
        {
            keyword = "*";
        }

        //Debug.WriteLine($"keyword is {keyword} @SelectRentResidentialsByNameKeyword() in DataAccessService");

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            if (keyword == "*")
            {
                //cmd.CommandText = String.Format("SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);

                cmd.CommandText = "SELECT rents.name as feedName, rent_residentials.comment as entryTitle, rents.rent_id as entryId FROM rent_residentials INNER JOIN rents USING (rent_id)";
            }
            else
            {
                cmd.CommandText = string.Format("SELECT rents.name as feedName, rent_residentials.comment as entryTitle, rents.rent_id as entryId FROM rent_residentials INNER JOIN rents USING (rent_id) WHERE rents.name LIKE '%{0}%'", keyword);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["entryId"]);
                if (string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: entryId is null or empty for a rent residential entry.");
                    continue;
                }

                var entry = new Models.Rent.Residentials.EntryResidentialSearchResult(s);

                s = Convert.ToString(reader["feedName"]) ?? "";
                entry.Name = s;

                //Debug.WriteLine($"Found rent residential entry: {entry.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["entryTitle"]);
                if (!string.IsNullOrEmpty(s))
                {
                    //
                }

                // Reset entry Isdirty flag.
                entry.IsDirty = false;

                res.AffectedCount++;

                res.SelectedEntries.Add(entry);
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentResidentialsByNameKeyword");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SqliteDataAccessSelectRentResidentialFullResultWrapper SelectRentResidentialById(string id)
    {
        var res = new SqliteDataAccessSelectRentResidentialFullResultWrapper();

        var entry = new Models.Rent.Residentials.EntryResidentialFull(id, EnumEntryStatus.Saved);

        if (string.IsNullOrEmpty(id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT rents.name as entryName, " +
                "rents.loc_pref_id as locPrefId, " +
                "rents.loc_prefecture as locPrefecture, " +
                "rents.loc_machiaza_id as locMachiazaId, " +
                "rents.loc_county as locCounty, " +
                "rents.loc_city as locCity, " +
                "rents.loc_ward as locWard, " +
                "rents.loc_oaza_cho as locOazaCho, " +
                "rents.loc_choume as locChoume, " +
                "rents.loc_edaban as locEdaban, " +
                "rents.loc_location_full as locLocationFull, " +
                "rent_residentials.comment as resiComment, " +
                "rents.rent_id as entryId " +
                "FROM rent_residentials INNER JOIN rents USING (rent_id) WHERE rents.rent_id = '{0}'", id);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var s = Convert.ToString(reader["entryId"]);
                    if (string.IsNullOrEmpty(s))
                    {
                        Debug.WriteLine("DataAccess::SelectRentResidentialsById: entryId is null or empty for a rent residential entry.");
                        continue;
                    }

                    s = Convert.ToString(reader["entryName"]) ?? "";
                    entry.Name = s;

                    s = Convert.ToString(reader["locPrefId"]) ?? "";
                    entry.LocPrefId = s;

                    s = Convert.ToString(reader["locPrefecture"]) ?? "";
                    entry.LocPrefecture = s;

                    s = Convert.ToString(reader["locMachiazaId"]) ?? "";
                    entry.LocMachiazaId = s;

                    s = Convert.ToString(reader["locCounty"]) ?? "";
                    entry.LocCounty = s;

                    s = Convert.ToString(reader["locCity"]) ?? "";
                    entry.LocCity = s;

                    s = Convert.ToString(reader["locWard"]) ?? "";
                    entry.LocWard = s;

                    s = Convert.ToString(reader["locOazaCho"]) ?? "";
                    entry.LocOazaCho = s;

                    s = Convert.ToString(reader["locChoume"]) ?? "";
                    entry.LocChoume = s;

                    s = Convert.ToString(reader["locEdaban"]) ?? "";
                    entry.LocEdaban = s;

                    s = Convert.ToString(reader["locLocationFull"]) ?? "";
                    entry.LocLocationFull = s;

                    // TODO; more.


                    s = Convert.ToString(reader["resiComment"]);
                    if (!string.IsNullOrEmpty(s))
                    {
                        //
                    }

                    res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            // 物件写真（建物）
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_pictures WHERE rent_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var picid = Convert.ToString(reader["picture_id"]) ?? string.Empty;
                    var picpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                    if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                    {
                        var rlpic = new PictureBldg(picid, picpath)
                        {
                            Description = Convert.ToString(reader["description"]) ?? string.Empty,

                            IsNew = false,
                            IsModified = false
                        };

                        var strTitle = Convert.ToString(reader["label"]);
                        if (!string.IsNullOrEmpty(strTitle))
                        {
                            rlpic.SetLabelFromString(strTitle);
                        }

                        var bln = Convert.ToInt32(reader["is_main"]);
                        if (bln > 0)
                        {
                            rlpic.IsMain = true;
                        }
                        else
                        {
                            rlpic.IsMain = false;
                        }

                        entry.BuildingPictures.Add(rlpic);
                    }
                    else
                    {
                        Debug.WriteLine("picture_id or file_path is null/empty.");
                    }
                }
            }

            // PDF（建物）
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_pdfs WHERE rent_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pdfid = Convert.ToString(reader["pdf_id"]) ?? string.Empty;
                    var pdfpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                    var thumbpath = Convert.ToString(reader["thumb_path"]) ?? string.Empty;
                    if (!string.IsNullOrEmpty(pdfid) && !string.IsNullOrEmpty(pdfpath) && !string.IsNullOrEmpty(thumbpath))
                    {
                        var rlpdf = new PdfBldg(pdfid, pdfpath, thumbpath)
                        {
                            Description = Convert.ToString(reader["description"]) ?? string.Empty,
                            IsNew = false,
                            IsModified = false
                        };

                        var bln = Convert.ToInt32(reader["is_main"]);
                        if (bln > 0)
                        {
                            rlpdf.IsMain = true;
                        }
                        else
                        {
                            rlpdf.IsMain = false;
                        }

                        entry.BuildingPdfs.Add(rlpdf);
                    }
                    else
                    {
                        Debug.WriteLine("pdf_id or file_path or thumb_path is null/empty.");
                    }
                }
            }

            // 部屋
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_rooms WHERE rent_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var roomId = Convert.ToString(reader["room_id"]) ?? string.Empty;
                    var room = new UnitResidential(roomId)
                    {
                        Name = Convert.ToString(reader["name"]) ?? string.Empty,
                        Chinryou = Convert.ToInt32(reader["chinryou"]),
                        IsNew = false,
                        IsModified = false
                    };

                    //Debug.WriteLine($"Room ID: {room.Id}, Room Name: {room.RoomName}");

                    entry.Rooms.Add(room);
                }
            }

            foreach(var room in entry.Rooms)
            {
                // 物件写真（部屋）
                cmd.CommandText = string.Format("SELECT * FROM rent_residential_room_pictures WHERE room_id = '{0}'", room.Id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var picid = Convert.ToString(reader["picture_id"]) ?? string.Empty;
                        var picpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                        if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                        {
                            var rlpic = new PictureUnit(picid, picpath)
                            {
                                Description = Convert.ToString(reader["description"]) ?? string.Empty,

                                IsNew = false,
                                IsModified = false
                            };

                            var strTitle = Convert.ToString(reader["type"]); // type not label.
                            if (!string.IsNullOrEmpty(strTitle))
                            {
                                rlpic.SetLabelFromString(strTitle);
                            }

                            var bln = Convert.ToInt32(reader["is_main"]);
                            if (bln > 0)
                            {
                                rlpic.IsMain = true;
                            }
                            else
                            {
                                rlpic.IsMain = false;
                            }

                            room.UnitPictures.Add(rlpic);
                        }
                        else
                        {
                            Debug.WriteLine("picture_id or file_path is null/empty.");
                        }
                    }
                }
            }

            // Reset entry Isdirty flag.
            entry.IsDirty = false;

            res.EntryFull = entry;
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentResidentialsByNameKeyword");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SqliteDataAccessResultWrapper UpsertRentResidentialUnit(string rentId, Models.Rent.Residentials.UnitResidential room)
    {
        var res = new SqliteDataAccessResultWrapper();

        if (string.IsNullOrEmpty(rentId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandType = CommandType.Text;

                // Upsert
                var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, rent_id, name, chinryou) VALUES (@roomId, @RentId, @Nam, @Chinryou) ";
                sqlInsertIntoRentLivingRoom += "ON CONFLICT(room_id) ";
                //sqlInsertIntoRentLivingRoom += string.Format("DO UPDATE SET name = '{0}'", EscapeSingleQuote(room.RoomName));
                sqlInsertIntoRentLivingRoom += "DO UPDATE SET name = @Nam, chinryou = @Chinryou";

                cmd.CommandText = sqlInsertIntoRentLivingRoom;

                /*
                if (room.IsNew)
                {
                    var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, rent_id, name) VALUES (@roomId, @RentId, @Nam)";

                    // 追加
                    cmd.CommandText = sqlInsertIntoRentLivingRoom;

                }
                else if (room.IsModified)
                {
                    var sqlUpdateRentLivingRoom = string.Format("UPDATE rent_residential_rooms SET name = @Nam WHERE room_id = '{0}'", room.Id);
                    // 更新
                    cmd.CommandText = sqlUpdateRentLivingRoom;

                }
                */
                cmd.Parameters.AddWithValue("@roomId", room.Id);
                cmd.Parameters.AddWithValue("@RentId", rentId);
                cmd.Parameters.AddWithValue("@Nam", room.Name);
                cmd.Parameters.AddWithValue("@Chinryou", room.Chinryou);

                var result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    room.IsNew = false;
                    room.IsModified = false;
                }
                
                cmd.Parameters.Clear();

                // 写真（部屋）rent_residential_room_pictures table - Insert or Update
                if (room.UnitPictures.Count > 0)
                {
                    foreach (var pic in room.UnitPictures)
                    {
                        /*
                        var exec = false;

                        if (pic.IsNew)
                        {
                            var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_room_pictures (picture_id, rent_id, file_path, label, description, is_main) " +
                                "VALUES (@PicId, @RentId, @Path, @Tit, @Desc, @Main)";

                            // 物件画像の追加
                            cmd.CommandText = sqlInsertIntoRentLivingPicture;

                            exec = true;
                        }
                        else if (pic.IsModified)
                        {
                            var sqlUpdateRentLivingPicture = string.Format(
                                "UPDATE rent_residential_room_pictures SET file_path = @Path, label = @Tit, description = @Desc, is_main = @Main " +
                                "WHERE picture_id = '{0}'", pic.Id);

                            // 物件画像の更新
                            cmd.CommandText = sqlUpdateRentLivingPicture;

                            exec = true;
                        }
                        */

                        // Upsert
                        var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, rent_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
                        sqlUpsertRoom += "ON CONFLICT(picture_id) ";
                        sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                        var exec = true;

                        cmd.CommandText = sqlUpsertRoom;

                        if (exec)
                        {
                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@PicId", pic.Id);
                            cmd.Parameters.AddWithValue("@roomId", room.Id);
                            cmd.Parameters.AddWithValue("@RentId", rentId);
                            cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                            cmd.Parameters.AddWithValue("@type", pic.PictureType.Key.ToString());
                            cmd.Parameters.AddWithValue("@Desc", pic.Description);
                            var paramIsMain = new SqliteParameter("@Main", System.Data.DbType.Int32);
                            if (pic.IsMain)
                            {
                                paramIsMain.Value = 1;
                            }
                            else
                            {
                                paramIsMain.Value = 0;
                            }
                            cmd.Parameters.Add(paramIsMain);

                            result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                pic.IsNew = false;
                                pic.IsModified = false;
                            }
                        }

                    }
                }

                // 写真（部屋）の削除リストを処理
                if (room.UnitPicturesToBeDeleted.Count > 0)
                {
                    foreach (var delr in room.UnitPicturesToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingRoom = string.Format("DELETE FROM rent_residential_room_pictures WHERE picture_id = '{0}'", delr.Id);

                        cmd.CommandText = sqlDeleteRentLivingRoom;
                        var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingRoomResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Room Pic deleted");
                        }
                    }
                    // let's not do this.
                    //room.UnitPicturesToBeDeleted.Clear();
                }

                // Commit
                cmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                cmd.Transaction.Rollback();

                Debug.WriteLine($"asdf {e}");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                res.Error.ErrPlaceParent = "DataAccess::UpsertRentResidentialRoom";

                return res;
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentResidentialRoom";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::UpsertRentResidentialRoom");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentResidentialRoom";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";

            if (e.InnerException != null)
            {
                res.Error.ErrText = e.InnerException.Message;
                res.Error.ErrDescription = "InnerException";
            }
            else
            {
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentResidentialRoom";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public SqliteDataAccessSelectRentResidentialUnitsResultWrapper SelectRentResidentialUnits()
    {
        var res = new SqliteDataAccessSelectRentResidentialUnitsResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = "SELECT rents.name as entryName, rent_residential_rooms.name as unitName, rent_residential_rooms.room_id as roomId, rents.rent_id as entryId FROM rent_residential_rooms INNER JOIN rents USING (rent_id)  INNER JOIN rent_residentials USING (rent_id)";
            //cmd.CommandText = string.Format("SELECT rents.name as feedName, rent_residentials.comment as entryTitle, rents.rent_id as entryId FROM rent_residentials INNER JOIN rents USING (rent_id) WHERE rents.name LIKE '{0}'", keyword);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var eid = Convert.ToString(reader["entryId"]);
                if (string.IsNullOrEmpty(eid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: entryId is null or empty for a rent residential entry.");
                    continue;
                }

                var rid = Convert.ToString(reader["roomId"]);
                if (string.IsNullOrEmpty(rid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: roomId is null or empty for a rent residential room.");
                    continue;
                }

                var unit = new Models.Rent.Residentials.UnitResidentialSearchResult(rid,eid);

                var s = Convert.ToString(reader["unitName"]) ?? "";
                unit.Name = s;

                //Debug.WriteLine($"Found rent residential entry: {entry.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["entryName"]);
                if (!string.IsNullOrEmpty(s))
                {
                    unit.EntryName = s;
                }

                // Reset entry Isdirty flag.
                unit.IsModified = false;

                res.AffectedCount++;

                res.SelectedUnits.Add(unit);
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "TargetInvocationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentResidentialsByNameKeyword");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentResidentialsByNameKeyword");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialsByNameKeyword";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    // ColumnExists check
    private static bool ColumnExists(IDataRecord dr, string columnName)
    {
        for (var i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false; ;
    }

    private static string EscapeSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("'", "''");
    }

    private static string RestoreSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("''", "'");
    }
}
