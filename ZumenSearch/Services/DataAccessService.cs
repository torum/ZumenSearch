using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Services;

public class DataAccessService : IDataAccessService
{
    // System.Data.SQLite
    //private readonly SQLiteConnectionStringBuilder connectionStringBuilder = new();
    // Microsoft.Data.Sqlite
    private SqliteConnectionStringBuilder connectionStringBuilder = [];

    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new SqliteDataAccessResultWrapper();

        // System.Data.SQLite
        //connectionStringBuilder.DataSource = dataBaseFilePath;
        //connectionStringBuilder.ForeignKeys = true;
        // Microsoft.Data.Sqlite
        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dataBaseFilePath);

        // System.Data.SQLite
        //using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
        // Microsoft.Data.Sqlite
        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            try
            {
                connection.Open();

                using var tableCmd = connection.CreateCommand();
                tableCmd.Transaction = connection.BeginTransaction();   
                try
                {
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rents (" +
                        "rent_id TEXT NOT NULL PRIMARY KEY," +
                        "name TEXT NOT NULL" +
                        ")";
                    tableCmd.ExecuteNonQuery();

                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residentials (" +
                        //"rent_residential_id TEXT NOT NULL PRIMARY KEY," +
                        "rent_id TEXT NOT NULL PRIMARY KEY," +
                        "comment TEXT NOT NULL," +
                        "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                        ")";
                    tableCmd.ExecuteNonQuery();

                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residentials_pictures (" +
                        "picture_id TEXT NOT NULL PRIMARY KEY," +
                        //"rent_residential_id TEXT NOT NULL," +
                        "rent_id TEXT NOT NULL," +
                        "filepath TEXT NOT NULL," +
                        //"data BLOB," +
                        "title TEXT NOT NULL," +
                        "description TEXT NOT NULL," +
                        "is_main INTEGER  NOT NULL," +
                        //"FOREIGN KEY (rent_residential_id) REFERENCES rent_residentials(rent_residential_id) ON DELETE CASCADE," +
                        "FOREIGN KEY (rent_id) REFERENCES rent_residentials(rent_id) ON DELETE CASCADE," +
                        "FOREIGN KEY (rent_id) REFERENCES rents(rent_id) ON DELETE CASCADE" +
                        " )";
                    tableCmd.ExecuteNonQuery();



                    //tableCmd.CommandText = "ALTER TABLE entries ADD COLUMN category TEXT;";
                    //tableCmd.ExecuteNonQuery();

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
        }

        return res;
    }

    public SqliteDataAccessResultWrapper InsertRentResidential(EntryResidentialFull entry)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(entry.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
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
                    cmd.CommandText = "INSERT INTO rents (rent_id, name) VALUES (@RentId, @Name)";
                    
                    cmd.Parameters.AddWithValue("@RentId", entry.Id);
                    cmd.Parameters.AddWithValue("@Name", entry.Name);
                    //cmd.Parameters.AddWithValue("@Updated", updated.ToString("yyyy-MM-dd HH:mm:ss"));

                    res.AffectedCount = cmd.ExecuteNonQuery();

                    // Residentials table
                    cmd.Parameters.Clear();

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO rent_residentials (rent_id, comment) VALUES (@RentId, @Comment)"; //@RentResidentialId,  rent_residential_id, 

                    //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                    cmd.Parameters.AddWithValue("@RentId", entry.Id);
                    cmd.Parameters.AddWithValue("@Comment", "");

                    //res.AffectedCount += cmd.ExecuteNonQuery();
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
                                            "INSERT INTO rent_residentials_pictures (picture_id, rent_id, picture_filepath, picture_data) " +
                                            "VALUES ('{0}', '{1}', '{2}', @0)",
                                            pic.Id, entry.Id, pic.ImageLocation);
                            */
                            /*
                            string sqlInsertIntoRentLivingPicture = String.Format(
                                "INSERT INTO rent_residentials_pictures (picture_id, rent_id, picture_filepath) " +
                                "VALUES ('{0}', '{1}', '{2}')",
                                pic.Id, entry.Id, pic.ImageLocation);
                            */
                            string sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residentials_pictures (picture_id, rent_id, filepath, title, description, is_main) VALUES (@PicId, @RentId, @Path, @Tit, @Desc, @Main)";

                            cmd.CommandText = sqlInsertIntoRentLivingPicture;

                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@PicId", pic.Id);
                            //cmd.Parameters.AddWithValue("@RentResidentialId", entry.Id + "_1");
                            cmd.Parameters.AddWithValue("@RentId", entry.Id);
                            cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                            cmd.Parameters.AddWithValue("@Tit", pic.Title);
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

                    //_readerWriterLock.ExitWriteLock();

                    return res;
                }
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

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return InsertRentResidential(entry);
        }

        return res;
    }

    public SqliteDataAccessResultWrapper UpdateRentResidential(EntryResidentialFull entry)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(entry.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
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
                    cmd.CommandType = CommandType.Text;

                    // Main
                    var sql = "UPDATE rents SET ";
                    sql += String.Format("name = '{0}' ", EscapeSingleQuote(entry.Name));
                    //sql += String.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                    //sql += String.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                    //sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));

                    sql += String.Format(" WHERE rent_id = '{0}'; ", entry.Id);

                    cmd.CommandText = sql;
                    res.AffectedCount = cmd.ExecuteNonQuery();

                    // Residentials
                    sql = "UPDATE rent_residentials SET ";
                    sql += String.Format("comment = '{0}' ", EscapeSingleQuote("some comment"));
                    //sql += String.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                    //sql += String.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                    //sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));

                    sql += String.Format(" WHERE rent_id = '{0}'; ", entry.Id);

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    // Pictures (Building)
                    cmd.Parameters.Clear();

                    // 物件写真の追加または更新
                    if (entry.BuildingPictures.Count > 0)
                    {
                        foreach (var pic in entry.BuildingPictures)
                        {
                            bool exec = false;

                            if (pic.IsNew)
                            {
                                string sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residentials_pictures (picture_id, rent_id, filepath, title, description, is_main) VALUES (@PicId, @RentId, @Path, @Tit, @Desc, @Main)";

                                // 物件画像の追加
                                cmd.CommandText = sqlInsertIntoRentLivingPicture;

                                exec = true;
                            }
                            else if (pic.IsModified)
                            {
                                string sqlUpdateRentLivingPicture = String.Format(
                                    "UPDATE rent_residentials_pictures SET title = @Tit, description = @Desc, is_main = @Main " +
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
                                cmd.Parameters.AddWithValue("@Tit", pic.Title);
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
                            string sqlDeleteRentLivingPicture = String.Format("DELETE FROM rent_residentials_pictures WHERE picture_id = '{0}'", delp.Id);

                            cmd.CommandText = sqlDeleteRentLivingPicture;
                            var DelRentLivingPicResult = cmd.ExecuteNonQuery();
                            if (DelRentLivingPicResult > 0)
                            {
                                // TODO:
                                Debug.WriteLine("Picture deleted");
                            }
                        }
                        entry.BuildingPicturesToBeDeleted.Clear();//RentLivingPicturesToBeDeletedIDs.Clear();
                    }

                    cmd.Parameters.Clear();

                    //

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

                    //_readerWriterLock.ExitWriteLock();

                    return res;
                }
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
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertFeed");

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

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return UpdateRentResidential(entry);
        }

        return res;
    }

    public SqliteDataAccessResultWrapper DeleteRentResidential(string rentId)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(rentId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
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
                    cmd.CommandText = String.Format("DELETE FROM rents WHERE rent_id = '{0}';", rentId);
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

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return DeleteRentResidential(rentId);
        }

        return res;
    }

    public SqliteDataAccessSelectRentResidentialResultWrapper SelectRentResidentialsByNameKeyword(string keyword)
    {
        var res = new SqliteDataAccessSelectRentResidentialResultWrapper();

        if (string.IsNullOrEmpty(keyword))
        {
            keyword = "*";
            //Debug.WriteLine("using *");
        }
        else
        {
            //Debug.WriteLine(keyword);
        }

            try
            {
                _readerWriterLock.EnterReadLock();

                // System.Data.SQLite
                //using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                // Microsoft.Data.Sqlite
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
                    cmd.CommandText = String.Format("SELECT rents.name as feedName, rent_residentials.comment as entryTitle, rents.rent_id as entryId FROM rent_residentials INNER JOIN rents USING (rent_id) WHERE rents.name LIKE '{0}'", keyword);
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

        var entry = new Models.Rent.Residentials.EntryResidentialFull(id);

        if (string.IsNullOrEmpty(id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        try
        {
            _readerWriterLock.EnterReadLock();

            // System.Data.SQLite
            //using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = String.Format("SELECT rents.name as feedName, rent_residentials.comment as entryTitle, rents.rent_id as entryId FROM rent_residentials INNER JOIN rents USING (rent_id) WHERE rents.rent_id = '{0}'", id);

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

                    s = Convert.ToString(reader["feedName"]) ?? "";
                    entry.Name = s;

                    s = Convert.ToString(reader["entryTitle"]);
                    if (!string.IsNullOrEmpty(s))
                    {
                        //
                    }

                    res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            // 物件写真
            cmd.CommandText = String.Format("SELECT * FROM rent_residentials_pictures WHERE rent_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var rlpic = new PictureBuilding(Convert.ToString(reader["filepath"]) ?? string.Empty)
                    {
                        //Convert.ToString(reader["RentLivingPicture_ID"])
                        /*
                        byte[] imageBytes = (byte[])reader["PictureData"];
                        rlpic.PictureData = imageBytes;

                        rlpic.Picture = Methods.BitmapImageFromBytes(imageBytes);
                        */

                        Id = Convert.ToString(reader["picture_id"]), 
                        //ImageLocation = Convert.ToString(reader["filepath"]),
                        Title = Convert.ToString(reader["title"]) ?? string.Empty,
                        Description = Convert.ToString(reader["description"]) ?? string.Empty,

                        IsNew = false,
                        IsModified = false
                    };

                    //if (ColumnExists(reader, "is_main"))
                    //{
                    //}
                    var bln = Convert.ToInt32(reader["is_main"]);
                    if (bln > 0)
                        rlpic.IsMain = true;
                    else
                        rlpic.IsMain = false;

                    entry.BuildingPictures.Add(rlpic);
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


    /*
    public SqliteDataAccessResultWrapper InsertFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri? htmlUri)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(feedId) || (feedUri == null))
        {
            res.IsError = true;
            // TODO:
            return res;
        }
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    cmd.CommandText = "INSERT OR IGNORE INTO feeds (feed_id, url, name, title, description, updated, html_url) VALUES (@FeedId, @Uri, @Name, @Title, @Description, @Updated, @HtmlUri)";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@FeedId", feedId);
                    cmd.Parameters.AddWithValue("@Uri", feedUri.AbsoluteUri);
                    cmd.Parameters.AddWithValue("@Name", feedName);
                    cmd.Parameters.AddWithValue("@Title", feedTitle);
                    cmd.Parameters.AddWithValue("@Description", feedDescription);
                    cmd.Parameters.AddWithValue("@Updated", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (htmlUri != null)
                    {
                        cmd.Parameters.AddWithValue("@HtmlUri", htmlUri.AbsoluteUri);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@HtmlUri", "");
                    }

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
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

                    _readerWriterLock.ExitWriteLock();

                    return res;
                }
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
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertFeed");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::InsertFeed";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));
        
        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return InsertFeed(feedId, feedUri, feedName, feedTitle, feedDescription, updated, htmlUri);
        }

        return res;
    }

    public SqliteDataAccessResultWrapper DeleteFeed(string feedId)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(feedId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = String.Format("DELETE FROM feeds WHERE feed_id = '{0}';", feedId);

                cmd.Transaction = connection.BeginTransaction();
                try
                {
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
                    res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

                    return res;
                }
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
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

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
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

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
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteFeed");
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
                Debug.WriteLine(e.Message + " @DataAccess::DeleteFeed");
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteFeed";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));
        
        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return DeleteFeed(feedId);
        }

        return res;
    }

    // Not really used because of updates on InsertEntries.
    public SqliteDataAccessResultWrapper UpdateFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri? htmlUri)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(feedId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    var sql = "UPDATE feeds SET ";
                    sql += String.Format("name = '{0}', ", EscapeSingleQuote(feedName));
                    sql += String.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                    sql += String.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                    sql += String.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (htmlUri != null)
                    {
                        sql += String.Format(", html_url = '{0}'", EscapeSingleQuote(htmlUri.AbsoluteUri));
                    }
                    sql += String.Format(" WHERE feed_id = '{0}'; ", feedId);

                    cmd.CommandText = sql;

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
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

                    return res;
                }
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
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::UpdateFeed");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

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
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateFeed";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        //Debug.WriteLine(string.Format("{0} feed updated", res.AffectedCount.ToString()));

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return UpdateFeed(feedId, feedUri, feedName, feedTitle, feedDescription, updated, htmlUri);
        }

        return res;
    }

    public SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries, string feedId, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri? htmlUri)
    {
        var res = new SqliteDataAccessInsertResultWrapper();
        var isBreaked = false;

        if (entries is null)
        {
            return res;
        }
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    // update feed info.
                    var sql = "UPDATE feeds SET ";
                    sql += string.Format("name = '{0}', ", EscapeSingleQuote(feedName));
                    sql += string.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                    sql += string.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                    sql += string.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (htmlUri != null)
                    {
                        sql += string.Format(", html_url = '{0}'", EscapeSingleQuote(htmlUri.AbsoluteUri));
                    }
                    sql += string.Format(" WHERE feed_id = '{0}'; ", feedId);

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    // Experimental 
                    // Delete old entries LIMIT 1000.
                    cmd.CommandText = string.Format("DELETE FROM entries WHERE feed_id = '{0}' AND entry_id NOT IN (SELECT entry_id FROM entries WHERE feed_id = '{0}' ORDER BY updated DESC LIMIT 1000);", feedId);//ASC
                    cmd.ExecuteNonQuery();

                    foreach (var entry in entries)
                    {
                        if (entry is not FeedEntryItem)
                        {
                            continue;
                        }

                        //if ((entry.EntryId == null) || (entry.AltHtmlUri == null))
                        if (entry.EntryId == null)
                        {
                            continue;
                        }

                        var sqlInsert = "INSERT OR IGNORE INTO entries (entry_id, feed_id, url, title, published, updated, author, category, summary, content, content_type, image_url, audio_url, source, source_url, comment_url, status, archived) VALUES (@EntryId, @FeedId, @AltHtmlUri, @Title, @Published, @Updated, @Author, @Category, @Summary, @Content, @ContentType, @ImageUri, @AudioUri, @Source, @SourceUri, @CommentUri, @Status, @IsArchived)";

                        cmd.CommandText = sqlInsert;

                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@FeedId", entry.ServiceId);// should be same as "feedId"

                        //cmd.Parameters.AddWithValue("@EntryId", entry.EntryId);/ not good.
                        cmd.Parameters.AddWithValue("@EntryId", entry.Id);// should be "entry.Id" here.

                        if (entry.AltHtmlUri != null)
                        {
                            cmd.Parameters.AddWithValue("@AltHtmlUri", entry.AltHtmlUri.AbsoluteUri);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@AltHtmlUri", string.Empty);
                        }

                        if (entry.Title != null)
                        {
                            cmd.Parameters.AddWithValue("@Title", entry.Title);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Title", string.Empty);
                        }

                        if (entry.Published == default)
                        {
                            if (entry.Updated != default)
                            {
                                cmd.Parameters.AddWithValue("@Published", entry.Updated.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Published", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Published", entry.Published.ToString("yyyy-MM-dd HH:mm:ss"));
                        }

                        if (entry.Updated == default)
                        {
                            cmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Updated", entry.Updated.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        //cmd.Parameters.AddWithValue("@Updated", entry.Updated.ToString("yyyy-MM-dd HH:mm:ss"));

                        if (entry.Author != null)
                        {

                            cmd.Parameters.AddWithValue("@Author", entry.Author);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Author", string.Empty);
                        }

                        if (entry.Category != null)
                        {
                            cmd.Parameters.AddWithValue("@Category", entry.Category);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Category", string.Empty);
                        }

                        if (entry.Summary != null)
                        {
                            cmd.Parameters.AddWithValue("@Summary", entry.Summary);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Summary", string.Empty);
                        }

                        if (entry.Content != null)
                        {
                            cmd.Parameters.AddWithValue("@Content", entry.Content);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Content", string.Empty);
                        }

                        cmd.Parameters.AddWithValue("@ContentType", entry.ContentType.ToString());



                        if (entry.ImageUri != null)
                        {
                            cmd.Parameters.AddWithValue("@ImageUri", entry.ImageUri.AbsoluteUri);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ImageUri", string.Empty);
                        }

                        if (entry.AudioUri != null)
                        {
                            cmd.Parameters.AddWithValue("@AudioUri", entry.AudioUri.AbsoluteUri);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@AudioUri", string.Empty);
                        }

                        //

                        if (entry is FeedEntryItem fei)
                        {
                            if (fei.Source != null)
                            {
                                cmd.Parameters.AddWithValue("@Source", fei.Source);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Source", string.Empty);
                            }

                            if (fei.SourceUri != null)
                            {
                                cmd.Parameters.AddWithValue("@SourceUri", fei.SourceUri.AbsoluteUri);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@SourceUri", string.Empty);
                            }

                            if (fei.CommentUri != null)
                            {
                                cmd.Parameters.AddWithValue("@CommentUri", fei.CommentUri.AbsoluteUri);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@CommentUri", string.Empty);
                            }

                            cmd.Parameters.AddWithValue("@Status", fei.Status.ToString());
                            cmd.Parameters.AddWithValue("@IsArchived", bool.FalseString);//(entry as FeedEntryItem).IsArchived.ToString()
                        }

                        var r = cmd.ExecuteNonQuery();

                        if (r > 0)
                        {
                            //c++;
                            res.AffectedCount++;

                            res.InsertedEntries.Add(entry);
                        }
                        else
                        {
                            // Update
                            var sqlUpdate = "UPDATE entries SET ";
                            sqlUpdate += string.Format("title = '{0}', ", EscapeSingleQuote(entry.Title ?? ""));
                            sqlUpdate += string.Format("author = '{0}', ", EscapeSingleQuote(entry.Author ?? "-"));
                            sqlUpdate += string.Format("category = '{0}', ", EscapeSingleQuote(entry.Category ?? "-"));
                            sqlUpdate += string.Format("summary = '{0}', ", EscapeSingleQuote(entry.Summary ?? ""));
                            sqlUpdate += string.Format("content = '{0}', ", EscapeSingleQuote(entry.Content ?? ""));
                            if (entry.ImageUri != null)
                            {
                                sqlUpdate += string.Format("image_url = '{0}', ", EscapeSingleQuote(entry.ImageUri.AbsoluteUri));
                            }
                            else
                            {
                                sqlUpdate += string.Format("image_url = '{0}', ", "");
                            }
                            if (entry.AudioUri != null)
                            {
                                sqlUpdate += string.Format("audio_url = '{0}', ", EscapeSingleQuote(entry.AudioUri.AbsoluteUri));
                            }
                            else
                            {
                                sqlUpdate += string.Format("audio_url = '{0}', ", "");
                            }

                            if (entry.Updated == default)
                            {
                                sqlUpdate += string.Format("updated = '{0}'", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else
                            {
                                sqlUpdate += string.Format("updated = '{0}'", updated.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            if (entry.AltHtmlUri != null)
                            {
                                sqlUpdate += string.Format(", url = '{0}'", EscapeSingleQuote(entry.AltHtmlUri.AbsoluteUri));
                            }
                            sqlUpdate += string.Format(" WHERE entry_id = '{0}'; ", entry.Id);
                            //Debug.WriteLine(sqlUpdate);
                            cmd.CommandText = sqlUpdate;
                            var c = cmd.ExecuteNonQuery();
                            if (c > 0)
                            {
                                //Debug.WriteLine($"{c} entries updated.");
                            }
                        }
                    }

                    cmd.Transaction.Commit();
                }
                catch (Exception e)
                {
                    cmd.Transaction.Rollback();

                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = "";
                    res.Error.ErrDescription = "Exception";
                    res.Error.ErrText = e.Message;
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::InsertEntries";
                    
                    return res;
                }
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
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::InsertEntries");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

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
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::InsertEntries";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return InsertEntries(entries, feedId, feedName, feedTitle, feedDescription, updated, htmlUri);
        }

        return res;
    }

    public SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

        if (string.IsNullOrEmpty(feedId))
        {
            return res;
        }

        try
        {
            _readerWriterLock.EnterReadLock();

            using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            if (IsUnarchivedOnly)
            {
                //cmd.CommandText = String.Format("SELECT * FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);

                cmd.CommandText = String.Format("SELECT feeds.name as feedName, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.updated as entryUpdated, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.audio_url as entryAudioUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.comment_url as entryCommentUri, entries.archived as entryArchived, entries.status as entryStatus FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' AND archived = '{1}' ORDER BY published DESC LIMIT 1000", feedId, bool.FalseString);
            }
            else
            {
                cmd.CommandText = String.Format("SELECT feeds.name as feedName, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.updated as entryUpdated, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.audio_url as entryAudioUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.comment_url as entryCommentUri, entries.archived as entryArchived, entries.status as entryStatus FROM entries INNER JOIN feeds USING (feed_id) WHERE feed_id = '{0}' ORDER BY published DESC LIMIT 5000", feedId);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["entryTitle"]);
                if (string.IsNullOrEmpty(s))
                {
                    s = "-";
                }
                var entry = new FeedEntryItem(s, feedId, null);

                entry.EntryId = Convert.ToString(reader["entryId"]);

                s = Convert.ToString(reader["entryUrl"]);
                if (!string.IsNullOrEmpty(s))
                {
                    entry.AltHtmlUri = new Uri(RestoreSingleQuote(s));
                }

                s = Convert.ToString(reader["entryPublished"]);
                if (!string.IsNullOrEmpty(s)) 
                {
                    try
                    {
                        entry.Published = DateTime.Parse(s);
                    }
                    catch { }
                }

                s = Convert.ToString(reader["entryUpdated"]);
                if (!string.IsNullOrEmpty(s))
                {
                    try
                    {
                        entry.Updated = DateTime.Parse(s);
                    }
                    catch { }
                }

                s = Convert.ToString(reader["entrySummary"]);
                if (!string.IsNullOrEmpty(s))
                {
                    entry.Summary = s;
                }

                s = Convert.ToString(reader["entryContent"]);
                if (!string.IsNullOrEmpty(s))
                {
                    entry.Content = s;
                }

                var t = Convert.ToString(reader["entryContentType"]);
                if (t == "textHtml")
                {
                    entry.ContentType = EntryItem.ContentTypes.textHtml;
                }
                else if (t == "text")
                {
                    entry.ContentType = EntryItem.ContentTypes.text;
                }
                else if (t == "markdown")
                {
                    entry.ContentType = EntryItem.ContentTypes.markdown;
                }
                else if (t == "hatena")
                {
                    entry.ContentType = EntryItem.ContentTypes.hatena;
                }
                else if (t == "unknown")
                {
                    entry.ContentType = EntryItem.ContentTypes.hatena;
                }
                else if (t == "none")
                {
                    entry.ContentType = EntryItem.ContentTypes.none;
                }
                else
                {
                    entry.ContentType = EntryItem.ContentTypes.unknown;
                }

                entry.Source = Convert.ToString(reader["entrySource"]) ?? "";

                var su = Convert.ToString(reader["entrySourceUri"]);
                if (!string.IsNullOrEmpty(su))
                {
                    entry.SourceUri = new Uri(su);
                }

                var u = Convert.ToString(reader["entryImageUri"]);
                if (!string.IsNullOrEmpty(u))
                {
                    entry.ImageUri = new Uri(RestoreSingleQuote(u));
                }

                var au = Convert.ToString(reader["entryAudioUri"]);
                if (!string.IsNullOrEmpty(au))
                {
                    entry.AudioUri = new Uri(RestoreSingleQuote(au));
                }

                var cu = Convert.ToString(reader["entryCommentUri"]);
                if (!string.IsNullOrEmpty(cu))
                {
                    entry.CommentUri = new Uri(cu);
                }


                //entry.ImageId = Convert.ToString(reader["image_id"]);

                var status = Convert.ToString(reader["entryStatus"]);
                if (!string.IsNullOrEmpty(status))
                {
                    entry.Status = entry.StatusTextToType(status);
                }

                entry.Author = Convert.ToString(reader["entryAuthor"]) ?? "";

                entry.Category = Convert.ToString(reader["entryCategory"]) ?? "";

                var blnstr = Convert.ToString(reader["entryArchived"]);
                if (!string.IsNullOrEmpty(blnstr))
                {
                    if (blnstr == bool.TrueString)
                        entry.IsArchived = true;
                    else
                        entry.IsArchived = false;
                }
                
                if (entry.IsArchived)
                {
                    if (entry.Status == FeedEntryItem.ReadStatus.rsNew)
                        entry.Status = FeedEntryItem.ReadStatus.rsNormal;

                    if (entry.Status == FeedEntryItem.ReadStatus.rsNewVisited)
                        entry.Status = FeedEntryItem.ReadStatus.rsNormalVisited;
                }
                
                if (!entry.IsArchived)
                {
                    res.UnreadCount++;
                }

                //entry.Publisher = Convert.ToString(reader["name"]);
                entry.FeedTitle = Convert.ToString(reader["feedName"]) ?? "";

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
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectEntriesByFeedId");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByFeedId");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByFeedId");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByFeedId";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIds(List<string> feedIds, bool IsUnarchivedOnly = true)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        var before = "SELECT feeds.name as feedName, feeds.feed_id as feedId, entries.title as entryTitle, entries.entry_id as entryId, entries.url as entryUrl, entries.published as entryPublished, entries.updated as entryUpdated, entries.summary as entrySummary, entries.content as entryContent, entries.content_type as entryContentType, entries.image_url as entryImageUri, entries.audio_url as entryAudioUri, entries.source as entrySource, entries.source_url as entrySourceUri, entries.author as entryAuthor, entries.category as entryCategory, entries.comment_url as entryCommentUri, entries.archived as entryArchived, entries.status as entryStatus FROM entries INNER JOIN feeds USING (feed_id) WHERE ";

        var middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle += "OR ";

            middle += String.Format("feed_id = '{0}' ", asdf);
        }

        //string after = string.Format(") AND IsArchived = '{0}' ORDER BY Published DESC LIMIT 1000", bool.FalseString);
        string after;
        if (IsUnarchivedOnly)
        {
            after = string.Format(") AND archived = '{0}' ORDER BY published DESC LIMIT 1000", bool.FalseString);
        }
        else
        {
            after = string.Format(") ORDER BY published DESC LIMIT 10000");
        }

        //Debug.WriteLine(before + middle + after);

        try
        {
            _readerWriterLock.EnterReadLock();

            using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = before + middle + after;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var fid = Convert.ToString(reader["feedId"]);
                if (string.IsNullOrEmpty(fid))
                    continue;

                FeedEntryItem entry = new FeedEntryItem(Convert.ToString(reader["entryTitle"]) ?? "no title", fid, null);

                //entry.MyNodeFeed = ndf;

                entry.EntryId = Convert.ToString(reader["entryId"]);

                //if (!string.IsNullOrEmpty(Convert.ToString(reader["entryUrl"])))
                //    entry.AltHtmlUri = new Uri(Convert.ToString(reader["entryUrl"]));

                var s = Convert.ToString(reader["entryUrl"]);
                if (!string.IsNullOrEmpty(s))
                {
                    entry.AltHtmlUri = new Uri(RestoreSingleQuote(s));
                }

                var pnsd = Convert.ToString(reader["entryPublished"]);
                if (!string.IsNullOrEmpty(pnsd))
                {
                    entry.Published = DateTime.Parse(pnsd);
                }

                try
                {
                    var hoge = Convert.ToString(reader["entryUpdated"]);
                    if (!string.IsNullOrEmpty(hoge))
                    {
                        entry.Updated = DateTime.Parse(hoge);
                    }
                }
                catch { }

                entry.Summary = Convert.ToString(reader["entrySummary"]) ?? "";

                entry.Content = Convert.ToString(reader["entryContent"]) ?? "";

                var t = Convert.ToString(reader["entryContentType"]);
                if (t == "textHtml")
                {
                    entry.ContentType = EntryItem.ContentTypes.textHtml;
                }
                else if (t == "text")
                {
                    entry.ContentType = EntryItem.ContentTypes.text;
                }
                else if (t == "markdown")
                {
                    entry.ContentType = EntryItem.ContentTypes.markdown;
                }
                else if (t == "hatena")
                {
                    entry.ContentType = EntryItem.ContentTypes.hatena;
                }
                else if (t == "unknown")
                {
                    entry.ContentType = EntryItem.ContentTypes.hatena;
                }
                else if (t == "none")
                {
                    entry.ContentType = EntryItem.ContentTypes.none;
                }
                else
                {
                    entry.ContentType = EntryItem.ContentTypes.unknown;
                }

                var u = Convert.ToString(reader["entryImageUri"]);
                if (!string.IsNullOrEmpty(u))
                {
                    entry.ImageUri = new Uri(RestoreSingleQuote(u));
                }

                var au = Convert.ToString(reader["entryAudioUri"]);
                if (!string.IsNullOrEmpty(au))
                {
                    entry.AudioUri = new Uri(RestoreSingleQuote(au));
                }

                var cu = Convert.ToString(reader["entryCommentUri"]);
                if (!string.IsNullOrEmpty(cu))
                {
                    entry.CommentUri = new Uri(cu);
                }


                //entry.ImageId = Convert.ToString(reader["image_id"]);


                var status = Convert.ToString(reader["entryStatus"]);
                if (!string.IsNullOrEmpty(status))
                {
                    entry.Status = entry.StatusTextToType(status);
                }

                entry.Source = Convert.ToString(reader["entrySource"]) ?? "";

                var su = Convert.ToString(reader["entrySourceUri"]);
                if (!string.IsNullOrEmpty(su))
                {
                    entry.SourceUri = new Uri(su);
                }

                entry.Author = Convert.ToString(reader["entryAuthor"]) ?? "";

                entry.Category = Convert.ToString(reader["entryCategory"]) ?? "";

                var blnstr = Convert.ToString(reader["entryArchived"]);
                if (!string.IsNullOrEmpty(blnstr))
                {
                    if (blnstr == bool.TrueString)
                        entry.IsArchived = true;
                    else
                        entry.IsArchived = false;
                }

                if (entry.IsArchived)
                {
                    if (entry.Status == FeedEntryItem.ReadStatus.rsNew)
                        entry.Status = FeedEntryItem.ReadStatus.rsNormal;

                    if (entry.Status == FeedEntryItem.ReadStatus.rsNewVisited)
                        entry.Status = FeedEntryItem.ReadStatus.rsNormalVisited;
                }

                if (!entry.IsArchived)
                {
                    res.UnreadCount++;
                }

                entry.FeedTitle = Convert.ToString(reader["feedName"]) ?? "";

                res.AffectedCount++;

                res.SelectedEntries.Add(entry);
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrText = e.Message + " " + e.InnerException.Message;
                res.Error.ErrDescription = "InnerException";
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectEntriesByMultipleFeedIds");
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectEntriesByMultipleFeedIds";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SqliteDataAccessResultWrapper UpdateAllEntriesAsArchived(List<string> feedIds)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        var before = string.Format("UPDATE entries SET archived = '{0}' WHERE ", bool.TrueString);

        var middle = "(";

        foreach (var asdf in feedIds)
        {
            if (middle != "(")
                middle += "OR ";

            middle += String.Format("feed_id = '{0}' ", asdf);
        }

        var after = string.Format(") AND archived = '{0}'", bool.FalseString);

        //Debug.WriteLine(before + middle + after);
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();

                try
                {
                    cmd.CommandText = before + middle + after;

                    cmd.CommandType = CommandType.Text;

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
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

                    return res;
                }
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

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
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateAllEntriesAsRead";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        //Debug.WriteLine(string.Format("{0} Entries from {1} Updated as read in the DB", c.ToString(), feedId));

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return UpdateAllEntriesAsArchived(feedIds);
        }

        return res;
    }

    public SqliteDataAccessResultWrapper UpdateEntryReadStatus(string? entryId, ReadStatus readStatus)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (string.IsNullOrEmpty(entryId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        var sql = "UPDATE entries SET ";
        sql += string.Format("status = '{0}'", readStatus.ToString());
        sql += string.Format(" WHERE entry_id = '{0}'; ", entryId);
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    cmd.CommandText = sql;

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
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::UpdateEntryReadStatus";

                    return res;
                }
            }
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntryReadStatus";

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
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),BeginTransaction()";
            res.Error.ErrPlaceParent = "DataAccess::UpdateEntryReadStatus";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries from {1} Updated as read in the DB", c.ToString(), feedId));

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return UpdateEntryReadStatus(entryId, readStatus);
        }

        return res;
    }

    // Not really used because of "ON DELETE CASCADE".
    public SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds)
    {
        var res = new SqliteDataAccessResultWrapper();
        var isBreaked = false;

        if (feedIds is null)
            return res;

        if (feedIds.Count == 0)
            return res;

        string sqlDelEntries;// = string.Empty;

        if (feedIds.Count > 1)
        {
            var before = "DELETE FROM entries WHERE ";
            //var before = "DELETE FROM feeds WHERE ";
            var middle = "(";

            foreach (var asdf in feedIds)
            {
                if (middle != "(")
                    middle += "OR ";

                middle += String.Format("feed_id = '{0}' ", asdf);
            }

            var after = ")";

            sqlDelEntries = before + middle + after;
        }
        else
        {
            sqlDelEntries = String.Format("DELETE FROM entries WHERE feed_id = '{0}';", feedIds[0]);
            //sqlDelEntries = String.Format("DELETE FROM feeds WHERE feed_id = '{0}';", feedIds[0]);
        }

        //Debug.WriteLine(sqlDelEntries);
    
            _readerWriterLock.EnterWriteLock();
        try
        {
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isBreaked = true;
            }
            else
            {
                using var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    cmd.CommandText = sqlDelEntries;

                    res.AffectedCount = cmd.ExecuteNonQuery();

                    cmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();

                    res.IsError = true;
                    res.Error.ErrType = ErrorObject.ErrTypes.DB;
                    res.Error.ErrCode = "";
                    res.Error.ErrText = ex.Message;
                    res.Error.ErrDescription = "Exception";
                    res.Error.ErrDatetime = DateTime.Now;
                    res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                    res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

                    return res;
                }
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
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteEntriesByFeedIds");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.Message + " " + e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::DeleteEntriesByFeedIds");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteEntriesByFeedIds";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        Debug.WriteLine(string.Format("{0} Entries Deleted from DB", res.AffectedCount));

        if (isBreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return DeleteEntriesByFeedIds(feedIds);
        }

        return res;
    }
    */
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
