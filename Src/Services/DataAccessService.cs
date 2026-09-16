using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Data;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

/*
 * DataAccessService.cs
 * 
 * This class provides data access services for the application, specifically for managing properties and listings and related data.
 * It handles database initialization, insertion, and updating of  properties, rooms, pictures, and PDFs.
 * The service uses SQLite as the underlying database and ensures thread safety with a ReaderWriterLockSlim.
 * 
 * Key functionalities include:
 * - Initializing the database and creating necessary tables if they do not exist.
 * - Inserting new properties and listings along with their associated pictures and PDFs.
 * - Updating existing properties and listings and their related data.
 * 
 * Note: The service is designed to work with the application's models and view models, facilitating seamless data management.
 */

// <summary>
// DataAccessService provides data access functionalities for managing properties and listings and related data in the application.
// </summary>

// TODO:
// * Consider implementing IDisposable to properly dispose of the ReaderWriterLockSlim and any other disposable resources used by this service.

public sealed class DataAccessService : IDataAccessService
{
    private SqliteConnectionStringBuilder connectionStringBuilder = [];

    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public ResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new ResultWrapper();

        try
        {
            connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dataBaseFilePath);//+ ";Pooling=false"

            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

            if (!RuntimeHelper.IsMSIX)
            {
                // https://github.com/dotnet/efcore/issues/38275
                //Debug.WriteLine("* Microsoft.Data.Sqlite SqliteConnection() calls Windows.Storage.ApplicationData.Current.get() results in System.InvalidOperationException \"Operation is not valid due to the current state of the object.\" Since we are in unpackaged, we can safely ignore the exception.");
            }

            connection.Open();

            using var tableCmd = connection.CreateCommand();
            tableCmd.Transaction = connection.BeginTransaction();
            try
            {
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS property (" +
                    "property_id TEXT NOT NULL PRIMARY KEY," +
                    "property_kind TEXT NOT NULL," +
                    "name TEXT NOT NULL," +
                    "thumbnail_path TEXT," +
                    "loc_pref_id TEXT," +
                    "loc_prefecture TEXT," +
                    "loc_machiaza_id TEXT," +
                    "loc_county TEXT," +
                    "loc_city TEXT," +
                    "loc_ward TEXT," +
                    "loc_oaza_cho TEXT," +
                    "loc_choume TEXT," +
                    "loc_edaban TEXT," +
                    "loc_location_full TEXT," +


                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residentials (" +
                    "property_id TEXT NOT NULL PRIMARY KEY," +
                    //"residential_id TEXT NOT NULL," +
                    "building_kind TEXT NOT NULL," +
                    "is_unit_ownership INTEGER  NOT NULL," +
                    "building_structure TEXT NOT NULL," +
                    "aboveground_floor_count INTEGER NOT NULL," +
                    "basement_floor_count INTEGER NOT NULL," +
                    "total_unit_count INTEGER NOT NULL," +
                    "built_year_month TEXT NOT NULL," +
                    "fudousan_id TEXT NOT NULL," +
                    "fudousan_id_additional_code TEXT NOT NULL," +
                    "remarks TEXT NOT NULL," +


                    //"updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "type TEXT NOT NULL," + 
                    "description TEXT NOT NULL," +
                    "is_main INTEGER NOT NULL," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pdfs (" +
                    "pdf_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "thumbnail_path TEXT NOT NULL," +
                    "type TEXT NOT NULL," +
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_rooms (" +
                    "room_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL," +
                    "name TEXT NOT NULL," +
                    "chinryou INTEGER NOT NULL DEFAULT 0," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_room_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "room_id TEXT NOT NULL," +
                    "property_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "type TEXT NOT NULL," + 
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "FOREIGN KEY (room_id) REFERENCES rent_residential_rooms(room_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_room_pdfs (" +
                    "pdf_id TEXT NOT NULL PRIMARY KEY," +
                    "room_id TEXT NOT NULL," +
                    "property_id TEXT NOT NULL," +
                    "file_path TEXT NOT NULL," +
                    "thumbnail_path TEXT NOT NULL," +
                    "type TEXT NOT NULL," +
                    "description TEXT NOT NULL," +
                    "is_main INTEGER  NOT NULL," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (room_id) REFERENCES rent_residential_rooms(room_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES property(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();


                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_lessors (" +
                    "lessor_id TEXT NOT NULL PRIMARY KEY," +
                    "name TEXT NOT NULL," +
                    "name_last TEXT NOT NULL," +
                    "name_first TEXT NOT NULL," +
                    "remarks TEXT," +

                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                /*
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
                */
                AddColumnsIfNotExist(connection);

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

    private static void AddColumnsIfNotExist(SqliteConnection conn)
    {
        //var cmd = conn.CreateCommand();
        //bool exists = false;

        #region == add to property ==
        /*
        cmd.CommandText = "PRAGMA table_info(property);";
        
        using (var reader = cmd.ExecuteReader())
        {
            // created_at
            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE property ADD COLUMN created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'));";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE property ADD COLUMN updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'));";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residentials ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residentials);";
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residentials ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_rooms ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_rooms);";
        
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "chinryou")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_rooms ADD COLUMN chinryou INTEGER NOT NULL DEFAULT 0;";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN chinryou @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_rooms ADD COLUMN created_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_rooms ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_pdfs ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_pdfs);";
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;

            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN created_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "type")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN type TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN type @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_room_pictures ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_room_pictures);";
        exists = false;
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                if (reader.GetString(1) == "room_id")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_room_pictures ADD COLUMN room_id TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN room_id @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion
    }

    public ResultWrapper InsertRentResidential(Models.Rent.Residentials.Property building)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(building.Id))
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
                cmd.CommandText = "INSERT INTO property (property_id, name, property_kind, thumbnail_path, loc_pref_id, loc_prefecture, loc_machiaza_id, loc_county, loc_city, loc_ward, loc_oaza_cho, loc_choume, loc_edaban, loc_location_full, updated_at) " +
                    "VALUES (@RentId, @Name, @PropertyKind, @Thumb, @LocPrefId, @LocPrefecture, @LocMachiazaId, @LocCounty, @LocCity, @LocWard, @LocOazaCho, @LocChoume, @LocEdaban, @LocLocationFull, @updated_at)";

                cmd.Parameters.AddWithValue("@RentId", building.Id);
                cmd.Parameters.AddWithValue("@Name", building.Name);
                cmd.Parameters.AddWithValue("@PropertyKind", building.PropertyKind.ToString());
                cmd.Parameters.AddWithValue("@Thumb", building.ThumbnailImageFilePath);
                cmd.Parameters.AddWithValue("@LocPrefId", building.LocPrefId);
                cmd.Parameters.AddWithValue("@LocPrefecture", building.LocPrefecture);
                cmd.Parameters.AddWithValue("@LocMachiazaId", building.LocMachiazaId);
                cmd.Parameters.AddWithValue("@LocCounty", building.LocCounty);
                cmd.Parameters.AddWithValue("@LocCity", building.LocCity);
                cmd.Parameters.AddWithValue("@LocWard", building.LocWard);
                cmd.Parameters.AddWithValue("@LocOazaCho", building.LocOazaCho);
                cmd.Parameters.AddWithValue("@LocChoume", building.LocChoume);
                cmd.Parameters.AddWithValue("@LocEdaban", building.LocEdaban);
                cmd.Parameters.AddWithValue("@LocLocationFull", building.LocLocationFull);
                // TODO: more

                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // Residentials table
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO rent_residentials (property_id, building_kind, is_unit_ownership, building_structure, aboveground_floor_count, basement_floor_count, total_unit_count, built_year_month, fudousan_id, fudousan_id_additional_code, remarks) " +
                    "VALUES (@RentId, @BuildingKind, @IsUnitOwnership, @BuildingStructure, @AboveGroundFloorCount, @BasementFloorCount, @TotalUnitCount, @BuiltYearMonth, @FudousanId, @FudousanIdAdditionalCode, @Remarks)";

                cmd.Parameters.AddWithValue("@RentId", building.Id);

                cmd.Parameters.AddWithValue("@BuildingKind", building.BuildingKind.Key.ToString());
                cmd.Parameters.AddWithValue("@IsUnitOwnership", building.IsUnitOwnership ? 1 : 0); // bool to int
                cmd.Parameters.AddWithValue("@BuildingStructure", building.BuildingStructure.Key.ToString());
                cmd.Parameters.AddWithValue("@AboveGroundFloorCount", building.AboveGroundFloorCount);// int
                cmd.Parameters.AddWithValue("@BasementFloorCount", building.BasementFloorCount);// int
                cmd.Parameters.AddWithValue("@TotalUnitCount", building.TotalUnitCount);// int
                cmd.Parameters.AddWithValue("@BuiltYearMonth", building.BuiltYearAndMonth.ToString("s"));
                cmd.Parameters.AddWithValue("@FudousanId", building.FudousanId);
                cmd.Parameters.AddWithValue("@FudousanIdAdditionalCode", building.FudousanIdAdditionalCode);
                cmd.Parameters.AddWithValue("@Remarks", building.Remarks);
                // TODO: more

                cmd.ExecuteNonQuery();

                // Picture (building) table
                if (building.Pictures.Count > 0)
                {
                    foreach (var pic in building.Pictures)
                    {
                        // Insertなので全てIsNewのはず・・・
                        //if (pic.IsNew)
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                                        "INSERT INTO rent_residential_pictures (picture_id, property_id, picture_filepath, picture_data) " +
                                        "VALUES ('{0}', '{1}', '{2}', @0)",
                                        pic.Id, building.Id, pic.ImageLocation);
                        */
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                            "INSERT INTO rent_residential_pictures (picture_id, property_id, picture_filepath) " +
                            "VALUES ('{0}', '{1}', '{2}')",
                            pic.Id, building.Id, pic.ImageLocation);
                        */
                        var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pictures (picture_id, property_id, file_path, type, description, is_main) " +
                            "VALUES (@PicId, @RentId, @Path, @Type, @Desc, @Main)";

                        cmd.CommandText = sqlInsertIntoRentLivingPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@PicId", pic.Id);
                        //cmd.Parameters.AddWithValue("@RentResidentialId", building.Id + "_1");
                        cmd.Parameters.AddWithValue("@RentId", building.Id);
                        cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                        cmd.Parameters.AddWithValue("@Type", pic.PictureType.Key.ToString());
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
                if (building.Pdfs.Count > 0)
                {
                    foreach (var pic in building.Pdfs)
                    {
                        // Insertなので全てIsNewのはず・・・
                        //if (pic.IsNew)
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                                        "INSERT INTO rent_residential_pictures (picture_id, property_id, picture_filepath, picture_data) " +
                                        "VALUES ('{0}', '{1}', '{2}', @0)",
                                        pic.Id, building.Id, pic.ImageLocation);
                        */
                        /*
                        string sqlInsertIntoRentLivingPicture = String.Format(
                            "INSERT INTO rent_residential_pictures (picture_id, property_id, picture_filepath) " +
                            "VALUES ('{0}', '{1}', '{2}')",
                            pic.Id, building.Id, pic.ImageLocation);
                        */
                        var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pdfs (pdf_id, property_id, file_path, thumbnail_path, type, description, is_main) " +
                            "VALUES (@PdfId, @RentId, @Path, @Thumb, @Type, @Desc, @Main)";

                        cmd.CommandText = sqlInsertIntoRentLivingPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@PdfId", pic.Id);
                        //cmd.Parameters.AddWithValue("@RentResidentialId", building.Id + "_1");
                        cmd.Parameters.AddWithValue("@RentId", building.Id);
                        cmd.Parameters.AddWithValue("@Path", pic.PdfLocation);
                        cmd.Parameters.AddWithValue("@Thumb", pic.ThumbnailLocation);
                        cmd.Parameters.AddWithValue("@Type", pic.PdfType.Key.ToString());
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
                if (building.Rooms.Count > 0)
                {
                    foreach (var unit in building.Rooms)
                    {
                        var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, property_id, name, chinryou) VALUES (@RoomId, @RentId, @Name, @Chinryou)";

                        cmd.CommandText = sqlInsertIntoRentLivingRoom;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@RoomId", unit.Id);
                        cmd.Parameters.AddWithValue("@RentId", building.Id);
                        cmd.Parameters.AddWithValue("@Name", unit.Name);
                        cmd.Parameters.AddWithValue("@Chinryou", unit.Chinryou);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            unit.PropertyStatus = EnumEntryStatus.Saved;
                            unit.Status = EnumEntryStatus.Saved;
                            //unit.IsNew = false;
                            unit.IsModified = false;
                        }

                        // Room Pics
                        if (unit.Pictures.Count > 0)
                        {
                            foreach (var pic in unit.Pictures)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, property_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
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
                                    cmd.Parameters.AddWithValue("@RentId", building.Id);
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

                        // Room PDF
                        if (unit.Pdfs.Count > 0)
                        {
                            foreach (var pdf in unit.Pdfs)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pdfs (pdf_id, room_id, property_id, file_path, thumbnail_path, type, description, is_main) VALUES (@PdfId, @roomId, @RentId, @Path, @Thumb, @type, @Desc, @Main) ";
                                sqlUpsertRoom += "ON CONFLICT(pdf_id) ";
                                sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                                var exec = true;

                                cmd.CommandText = sqlUpsertRoom;

                                if (exec)
                                {
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@PdfId", pdf.Id);
                                    cmd.Parameters.AddWithValue("@roomId", unit.Id);
                                    cmd.Parameters.AddWithValue("@RentId", building.Id);
                                    cmd.Parameters.AddWithValue("@Path", pdf.PdfLocation);
                                    cmd.Parameters.AddWithValue("@Thumb", pdf.ThumbnailLocation);
                                    cmd.Parameters.AddWithValue("@type", pdf.PdfType.Key.ToString());
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

        building.IsModified = false;
        building.Status = EnumEntryStatus.Saved;

        return res;
    }

    public ResultWrapper UpdateRentResidential(Models.Rent.Residentials.Property building)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(building.Id))
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

                // property table
                var sql = "UPDATE property SET ";
                sql += string.Format("name = '{0}', ", EscapeSingleQuote(building.Name));
                sql += string.Format("property_kind = '{0}', ", EscapeSingleQuote(building.PropertyKind.ToString()));
                sql += string.Format("thumbnail_path = '{0}', ", EscapeSingleQuote(building.ThumbnailImageFilePath));
                sql += string.Format("loc_pref_id = '{0}', ", EscapeSingleQuote(building.LocPrefId));
                sql += string.Format("loc_prefecture = '{0}', ", EscapeSingleQuote(building.LocPrefecture));
                sql += string.Format("loc_machiaza_id = '{0}', ", EscapeSingleQuote(building.LocMachiazaId));
                sql += string.Format("loc_county = '{0}', ", EscapeSingleQuote(building.LocCounty));
                sql += string.Format("loc_city = '{0}', ", EscapeSingleQuote(building.LocCity));
                sql += string.Format("loc_ward = '{0}', ", EscapeSingleQuote(building.LocWard));
                sql += string.Format("loc_oaza_cho = '{0}', ", EscapeSingleQuote(building.LocOazaCho));
                sql += string.Format("loc_choume = '{0}', ", EscapeSingleQuote(building.LocChoume));
                sql += string.Format("loc_edaban = '{0}', ", EscapeSingleQuote(building.LocEdaban));
                sql += string.Format("loc_location_full = '{0}', ", EscapeSingleQuote(building.LocLocationFull));
                // TODO: more

                sql += String.Format("updated_at = '{0}' ", DateTimeOffset.UtcNow.ToString("s")); // 最後カンマ無し 注意
                sql += string.Format(" WHERE property_id = '{0}'; ", building.Id);

                cmd.CommandText = sql;
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // Residentials table
                /*
                sql = "UPDATE rent_residentials SET ";
                sql += string.Format("remarks = '{0}', ", EscapeSingleQuote(entry.Remarks));
                //sql += String.Format("title = '{0}', ", EscapeSingleQuote(feedTitle));
                //sql += String.Format("description = '{0}', ", EscapeSingleQuote(feedDescription));
                sql += String.Format("updated_at = '{0}' ", DateTimeOffset.UtcNow.ToString("s"));//ToString("yyyy-MM-dd HH:mm:ss"));

                sql += string.Format(" WHERE property_id = '{0}'; ", entry.Id);
                */
                sql = "UPDATE rent_residentials SET " +
                    "building_kind = @building_kind, is_unit_ownership = @is_unit_ownership, building_structure = @building_structure, aboveground_floor_count = @aboveground_floor_count, basement_floor_count = @basement_floor_count, total_unit_count = @total_unit_count, built_year_month = @built_year_month, fudousan_id = @fudousan_id, fudousan_id_additional_code = @fudousan_id_additional_code, remarks = @remarks " + // 最後カンマ無し 注意
                    //"updated_at = @updated_at " + // 最後カンマ無し 注意
                    "WHERE property_id = @property_id;";

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@property_id", building.Id);

                cmd.Parameters.AddWithValue("@building_kind", building.BuildingKind.Key.ToString());
                cmd.Parameters.AddWithValue("@is_unit_ownership", building.IsUnitOwnership ? 1 : 0);// bool to int
                cmd.Parameters.AddWithValue("@building_structure", building.BuildingStructure.Key.ToString());
                cmd.Parameters.AddWithValue("@aboveground_floor_count", building.AboveGroundFloorCount);// int
                cmd.Parameters.AddWithValue("@basement_floor_count", building.BasementFloorCount);// int
                cmd.Parameters.AddWithValue("@total_unit_count", building.TotalUnitCount);// int
                cmd.Parameters.AddWithValue("@built_year_month", building.BuiltYearAndMonth.ToString("s"));
                cmd.Parameters.AddWithValue("@fudousan_id", building.FudousanId);
                cmd.Parameters.AddWithValue("@fudousan_id_additional_code", building.FudousanIdAdditionalCode);
                cmd.Parameters.AddWithValue("@remarks", building.Remarks);
                // more

                //cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));

                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // 写真（建物）Residentials pictures table - Insert or Update
                if (building.Pictures.Count > 0)
                {
                    foreach (var pic in building.Pictures)
                    {
                        var exec = false;

                        if (pic.IsNew)
                        {
                            var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pictures (picture_id, property_id, file_path, type, description, is_main) " +
                                "VALUES (@PicId, @RentId, @Path, @Type, @Desc, @Main)";

                            // 物件画像の追加
                            cmd.CommandText = sqlInsertIntoRentLivingPicture;

                            exec = true;
                        }
                        else if (pic.IsModified)
                        {
                            var sqlUpdateRentLivingPicture = string.Format(
                                "UPDATE rent_residential_pictures SET file_path = @Path, type = @Type, description = @Desc, is_main = @Main " +
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
                            //cmd.Parameters.AddWithValue("@RentResidentialId", building.Id + "_1");
                            cmd.Parameters.AddWithValue("@RentId", building.Id);
                            cmd.Parameters.AddWithValue("@Path", pic.ImageLocation);
                            cmd.Parameters.AddWithValue("@Type", pic.PictureType.Key.ToString());
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
                if (building.BuildingPicturesToBeDeleted.Count > 0)
                {
                    foreach (var delp in building.BuildingPicturesToBeDeleted)
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
                if (building.Pdfs.Count > 0)
                {
                    foreach (var pdf in building.Pdfs)
                    {
                        var exec = false;

                        if (pdf.IsNew)
                        {
                            var sqlInsertIntoRentLivingPdf = "INSERT INTO rent_residential_pdfs (pdf_id, property_id, file_path, thumbnail_path, type, description, is_main) " +
                                "VALUES (@PdfId, @RentId, @Path, @Thumb, @Type, @Desc, @Main)";

                            // PDFの追加
                            cmd.CommandText = sqlInsertIntoRentLivingPdf;

                            exec = true;
                        }
                        else if (pdf.IsModified)
                        {
                            var sqlUpdateRentLivingPdf = string.Format(
                                "UPDATE rent_residential_pdfs SET file_path = @Path, thumbnail_path = @Thumb, type = @Type, description = @Desc, is_main = @Main, updated_at = @Updated " +
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
                            cmd.Parameters.AddWithValue("@RentId", building.Id);
                            cmd.Parameters.AddWithValue("@Path", pdf.PdfLocation);
                            cmd.Parameters.AddWithValue("@Type", pdf.PdfType.Key.ToString());
                            cmd.Parameters.AddWithValue("@Thumb", pdf.ThumbnailLocation);
                            cmd.Parameters.AddWithValue("@Desc", pdf.Description);
                            cmd.Parameters.AddWithValue("@Updated", DateTimeOffset.UtcNow.ToString("s"));
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
                if (building.BuildingPdfsToBeDeleted.Count > 0)
                {
                    foreach (var delp in building.BuildingPdfsToBeDeleted)
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
                if (building.Rooms.Count > 0)
                {
                    foreach (var room in building.Rooms)
                    {
                        var exec = false;

                        if (room.PropertyStatus == EnumEntryStatus.New)
                        {
                            var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, property_id, name, chinryou) VALUES (@roomId, @RentId, @Nam, @Chinryou)";

                            // 追加
                            cmd.CommandText = sqlInsertIntoRentLivingRoom;
                            exec = true;
                        }
                        else if (room.IsModified || room.PropertyStatus == EnumEntryStatus.Saved)
                        {
                            //var sqlUpdateRentLivingRoom = string.Format("UPDATE rent_residential_rooms SET name = @Nam WHERE room_id = '{0}'", room.Id);
                            var sqlUpdateRentLivingRoom = "UPDATE rent_residential_rooms SET name = @Nam, chinryou = @Chinryou, updated_at = @Updated WHERE room_id = @roomId";
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
                            cmd.Parameters.AddWithValue("@RentId", building.Id);
                            cmd.Parameters.AddWithValue("@Nam", room.Name);
                            cmd.Parameters.AddWithValue("@Chinryou", room.Chinryou);
                            cmd.Parameters.AddWithValue("@Updated", DateTimeOffset.UtcNow.ToString("s"));

                            var result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                room.PropertyStatus = EnumEntryStatus.Saved;
                                room.Status = EnumEntryStatus.Saved;
                                //room.IsNew = false;
                                room.IsModified = false;
                            }
                        }

                        // room Pic
                        if (room.Pictures.Count > 0)
                        {
                            foreach (var pic in room.Pictures)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, property_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
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
                                    cmd.Parameters.AddWithValue("@RentId", building.Id);
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

                        // TODO:削除リスト

                        // room pdf
                        if (room.Pdfs.Count > 0)
                        {
                            foreach (var pdf in room.Pdfs)
                            {
                                // Upsert
                                var sqlUpsertRoom = "INSERT INTO rent_residential_room_pdfs (pdf_id, room_id, property_id, file_path, thumbnail_path, type, description, is_main) VALUES (@PdfId, @roomId, @RentId, @Path, @Thumb, @type, @Desc, @Main) ";
                                sqlUpsertRoom += "ON CONFLICT(pdf_id) ";
                                sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                                exec = true;

                                cmd.CommandText = sqlUpsertRoom;

                                if (exec)
                                {
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@PdfId", pdf.Id);
                                    cmd.Parameters.AddWithValue("@roomId", room.Id);
                                    cmd.Parameters.AddWithValue("@RentId", building.Id);
                                    cmd.Parameters.AddWithValue("@Path", pdf.PdfLocation);
                                    cmd.Parameters.AddWithValue("@Thumb", pdf.ThumbnailLocation);
                                    cmd.Parameters.AddWithValue("@type", pdf.PdfType.Key.ToString());
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

                        // TODO:削除リスト
                    }
                }

                cmd.Parameters.Clear();

                // 部屋の削除リストを処理
                if (building.RoomsToBeDeleted.Count > 0)
                {
                    foreach (var delr in building.RoomsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingRoom = string.Format("DELETE FROM rent_residential_rooms WHERE room_id = '{0}'", delr.Id);

                        cmd.CommandText = sqlDeleteRentLivingRoom;
                        var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingRoomResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Room deleted @UpdateRentResidential in DataAccessService");
                        }
                    }
                    // Let's not
                    //building.RoomsToBeDeleted.Clear();
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

    public ResultWrapper DeleteRentResidential(string rentId)
    {
        var res = new ResultWrapper();

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
                cmd.CommandText = string.Format("DELETE FROM property WHERE property_id = '{0}';", rentId);
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
                Debug.WriteLine(e.Message + " @DataAccess::DeleteRentResidential");
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

    public SelectPropertiesResultWrapper SelectRecentProperties()
    {
        var res = new SelectPropertiesResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM property ORDER BY updated_at DESC LIMIT 10"; // limit 10 for now.

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(reader.GetOrdinal("property_id")) ?? string.Empty; //Convert.ToString(reader["property_id"]);
                if (string.IsNullOrEmpty(id))
                {
                    Debug.WriteLine("DataAccess::SelectRecentProperties: property_id is null or empty .");
                    continue;
                }

                var enumKind = EnumPropertyKind.Unknown;
                var kind = reader.GetString(reader.GetOrdinal("property_kind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<Models.Base.EnumPropertyKind>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                    else
                    {
                        Debug.WriteLine($"property_kind ({kind}) parse failed. @SelectRecentProperties()");
                    }
                }

                var entry = new Models.Common.PropertySearchResultItem(id, enumKind);

                var name = reader.GetString(reader.GetOrdinal("name")) ?? string.Empty;//Convert.ToString(reader["name"]) ?? "";
                entry.Name = name;

                var thumb = reader.GetString(reader.GetOrdinal("thumbnail_path")) ?? string.Empty;
                entry.ThumbnailImageFilePath = thumb;

                var createdAt = reader.GetString(reader.GetOrdinal("created_at")) ?? string.Empty;//Convert.ToString(reader["created_at"]) ?? string.Empty;
                entry.CreatedAt = createdAt;
                var updatedAt = reader.GetString(reader.GetOrdinal("updated_at")) ?? string.Empty;//Convert.ToString(reader["updated_at"]) ?? string.Empty;
                entry.UpdatedAt = updatedAt;

                //res.AffectedCount++;

                res.PropertySearchResult.Add(entry);
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
            res.Error.ErrPlaceParent = "DataAccess::SelectRecentProperties";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRecentProperties");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRecentProperties";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRecentProperties");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRecentProperties");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRecentProperties";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SelectPropertiesResultWrapper SelectRentResidentialsByNameKeyword(string keyword)
    {
        var res = new SelectPropertiesResultWrapper();

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
                cmd.CommandText = "SELECT property.name as propertyName, property.property_kind as propertyKind, rent_residentials.remarks as remarks, property.property_id as propertyId FROM rent_residentials INNER JOIN property USING (property_id)";
            }
            else
            {
                cmd.CommandText = string.Format("SELECT property.name as propertyName, property.property_kind as propertyKind, rent_residentials.remarks as remarks, property.property_id as propertyId FROM rent_residentials INNER JOIN property USING (property_id) WHERE property.name LIKE '%{0}%'", keyword);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["propertyId"]);
                if (string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: propertyId is null or empty for a rent residential.");
                    continue;
                }

                var enumKind = EnumPropertyKind.Unknown;
                var kind = reader.GetString(reader.GetOrdinal("propertyKind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<Models.Base.EnumPropertyKind>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                }

                var entry = new Models.Common.PropertySearchResultItem(s, enumKind);

                s = Convert.ToString(reader["propertyName"]) ?? "";
                entry.Name = s;

                //Debug.WriteLine($"Found rent residential entry: {entry.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["remarks"]);
                if (!string.IsNullOrEmpty(s))
                {
                    //
                }

                // Reset entry Isdirty flag.
                entry.IsModified = false;

                //res.AffectedCount++;

                res.PropertySearchResult.Add(entry);
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

    public SelectRentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id)
    {
        var res = new SelectRentResidentialBuildingSingleResultWrapper();

        var entry = new Models.Rent.Residentials.Property(id, EnumEntryStatus.Saved, EnumPropertyKind.RentResidential);

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
            cmd.CommandText = string.Format("SELECT property.name as propertyName, " +
                "property.property_kind as propertyKind, " +
                "property.loc_pref_id as locPrefId, " +
                "property.loc_prefecture as locPrefecture, " +
                "property.loc_machiaza_id as locMachiazaId, " +
                "property.loc_county as locCounty, " +
                "property.loc_city as locCity, " +
                "property.loc_ward as locWard, " +
                "property.loc_oaza_cho as locOazaCho, " +
                "property.loc_choume as locChoume, " +
                "property.loc_edaban as locEdaban, " +
                "property.loc_location_full as locLocationFull, " +
                "property.updated_at as UpdatedAt, " +

                "rent_residentials.building_kind as resiBuildingKind, " +
                "rent_residentials.is_unit_ownership as resiUnitOwnership, " +
                "rent_residentials.building_structure as resiBuildingStructure, " +
                "rent_residentials.aboveground_floor_count as resiAboveGroundFloorCount, " +
                "rent_residentials.basement_floor_count as resiBasementFloorCount, " +
                "rent_residentials.total_unit_count as resiTotalUnitCount, " +
                "rent_residentials.built_year_month as resiBuiltYearMonth, " +
                "rent_residentials.fudousan_id as resiFudousanId, " +
                "rent_residentials.fudousan_id_additional_code as resiFudousanIdAdditionalCode, " +
                "rent_residentials.remarks as resiRemarks, " +
                // TODO: more fields to be added here.

                //"rent_residentials.updated_at as UpdatedAt, " +
                "property.property_id as propertyId " +
                "FROM rent_residentials INNER JOIN property USING (property_id) WHERE property.property_id = '{0}'", id);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var s = Convert.ToString(reader["propertyId"]);
                    if (string.IsNullOrEmpty(s))
                    {
                        Debug.WriteLine("DataAccess::SelectRentResidentialsById: propertyId is null or empty for a rent residential entry.");
                        continue;
                    }

                    /*
                    var enumKind = EnumPropertyKind.Unknown;
                    var kind = reader.GetString(reader.GetOrdinal("propertyKind")) ?? string.Empty;
                    if (!string.IsNullOrEmpty(kind))
                    {
                        if (Enum.TryParse<Models.Base.EnumPropertyKind>(kind, out var parsedKind))
                        {
                            enumKind = parsedKind;
                        }
                    }
                    */

                    s = Convert.ToString(reader["propertyName"]) ?? "";
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

                    // TODO: more.


                    s = Convert.ToString(reader["resiBuildingKind"]) ?? "";
                    entry.SetKindTypeFromString(s);

                    var bln = Convert.ToInt32(reader["resiUnitOwnership"]);
                    entry.IsUnitOwnership = bln != 0;

                    s = Convert.ToString(reader["resiBuildingStructure"]) ?? "";
                    entry.SetStructureTypeFromString(s);

                    int intValue = Convert.ToInt32(reader["resiAboveGroundFloorCount"]);
                    entry.AboveGroundFloorCount = intValue;

                    intValue = Convert.ToInt32(reader["resiBasementFloorCount"]);
                    entry.BasementFloorCount = intValue;

                    intValue = Convert.ToInt32(reader["resiTotalUnitCount"]);
                    entry.TotalUnitCount = intValue;

                    s = Convert.ToString(reader["resiBuiltYearMonth"]) ?? "";
                    entry.SetBuildYearMonthFromString(s);

                    s = Convert.ToString(reader["resiFudousanId"]) ?? "";
                    entry.FudousanId = s;

                    s = Convert.ToString(reader["resiFudousanIdAdditionalCode"]) ?? "";
                    entry.FudousanIdAdditionalCode = s;

                    s = Convert.ToString(reader["resiRemarks"]) ?? "";
                    entry.Remarks = s;

                    // TODO: more.

                    //res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            // 物件写真（建物）
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_pictures WHERE property_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var picid = Convert.ToString(reader["picture_id"]) ?? string.Empty;
                    var picpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                    if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                    {
                        var rlpic = new Models.Rent.Residentials.Picture(picid, picpath)
                        {
                            Description = Convert.ToString(reader["description"]) ?? string.Empty,

                            IsNew = false,
                            IsModified = false
                        };

                        var strType = Convert.ToString(reader["type"]);
                        if (!string.IsNullOrEmpty(strType))
                        {
                            rlpic.SetLabelFromString(strType);
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

                        entry.Pictures.Add(rlpic);
                    }
                    else
                    {
                        Debug.WriteLine("picture_id or file_path is null/empty.");
                    }
                }
            }

            // PDF（建物）
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_pdfs WHERE property_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pdfid = Convert.ToString(reader["pdf_id"]) ?? string.Empty;
                    var pdfpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                    var thumbpath = Convert.ToString(reader["thumbnail_path"]) ?? string.Empty;
                    if (!string.IsNullOrEmpty(pdfid) && !string.IsNullOrEmpty(pdfpath) && !string.IsNullOrEmpty(thumbpath))
                    {
                        var rlpdf = new Models.Rent.Residentials.Pdf(pdfid, pdfpath, thumbpath)
                        {
                            Description = Convert.ToString(reader["description"]) ?? string.Empty,
                            IsNew = false,
                            IsModified = false
                        };

                        var strType = Convert.ToString(reader["type"]);
                        if (!string.IsNullOrEmpty(strType))
                        {
                            rlpdf.SetTypeFromString(strType);
                        }

                        var bln = Convert.ToInt32(reader["is_main"]);
                        if (bln > 0)
                        {
                            rlpdf.IsMain = true;
                        }
                        else
                        {
                            rlpdf.IsMain = false;
                        }

                        entry.Pdfs.Add(rlpdf);
                    }
                    else
                    {
                        Debug.WriteLine("pdf_id or file_path or thumbnail_path is null/empty.");
                    }
                }
            }

            // 部屋
            cmd.CommandText = string.Format("SELECT * FROM rent_residential_rooms WHERE property_id = '{0}'", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var roomId = Convert.ToString(reader["room_id"]) ?? string.Empty;
                    var room = new Models.Rent.Residentials.Listing.Listing(roomId, entry.Id, EnumEntryStatus.Saved, EnumEntryStatus.Saved, entry.Name)
                    {
                        Name = Convert.ToString(reader["name"]) ?? string.Empty,
                        Chinryou = Convert.ToInt32(reader["chinryou"]),
                        Status = EnumEntryStatus.Saved,
                        //IsNew = false,
                        IsModified = false
                    };

                    //Debug.WriteLine($"Room ID: {room.Id}, Room Name: {room.RoomName}");
                    
                    entry.Rooms.Add(room);
                }
            }

            foreach (var room in entry.Rooms)
            {
                // 部屋写真
                cmd.CommandText = string.Format("SELECT * FROM rent_residential_room_pictures WHERE room_id = '{0}'", room.Id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var picid = Convert.ToString(reader["picture_id"]) ?? string.Empty;
                        var picpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                        if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                        {
                            var rlpic = new Models.Rent.Residentials.Listing.Picture(picid, picpath)
                            {
                                Description = Convert.ToString(reader["description"]) ?? string.Empty,

                                IsNew = false,
                                IsModified = false
                            };

                            var strType = Convert.ToString(reader["type"]);
                            if (!string.IsNullOrEmpty(strType))
                            {
                                rlpic.SetLabelFromString(strType);
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

                            room.Pictures.Add(rlpic);
                        }
                        else
                        {
                            Debug.WriteLine("picture_id or file_path is null/empty.");
                        }
                    }
                }

                // 部屋PDF
                cmd.CommandText = string.Format("SELECT * FROM rent_residential_room_pdfs WHERE room_id = '{0}'", room.Id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pdfid = Convert.ToString(reader["pdf_id"]) ?? string.Empty;
                        var pdfpath = Convert.ToString(reader["file_path"]) ?? string.Empty;
                        var thumbpath = Convert.ToString(reader["thumbnail_path"]) ?? string.Empty;
                        if (!string.IsNullOrEmpty(pdfid) && !string.IsNullOrEmpty(pdfpath) && !string.IsNullOrEmpty(thumbpath))
                        {
                            var rlpdf = new Models.Rent.Residentials.Listing.Pdf(pdfid, pdfpath, thumbpath)
                            {
                                Description = Convert.ToString(reader["description"]) ?? string.Empty,
                                IsNew = false,
                                IsModified = false
                            };

                            var strType = Convert.ToString(reader["type"]);
                            if (!string.IsNullOrEmpty(strType))
                            {
                                rlpdf.SetTypeFromString(strType);
                            }

                            var bln = Convert.ToInt32(reader["is_main"]);
                            if (bln > 0)
                            {
                                rlpdf.IsMain = true;
                            }
                            else
                            {
                                rlpdf.IsMain = false;
                            }

                            room.Pdfs.Add(rlpdf);
                        }
                        else
                        {
                            Debug.WriteLine("pdf_id or file_path or thumbnail_path is null/empty.");
                        }
                    }
                }


            }

            // Reset entry Isdirty flag.
            entry.Status = EnumEntryStatus.Saved;
            entry.IsModified = false;

            res.Building = entry;
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
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialById";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentResidentialById");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialById";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentResidentialById");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentResidentialById");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialById";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Listing.Listing room)
    {
        var res = new ResultWrapper();

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

                // Update updated_at in the property table.
                var sql = "UPDATE property SET ";
                sql += String.Format("updated_at = '{0}' ", DateTimeOffset.UtcNow.ToString("s"));
                sql += string.Format(" WHERE property_id = '{0}'; ", rentId);

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // Upsert into rent_residential_rooms
                var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, property_id, name, chinryou) VALUES (@roomId, @RentId, @Nam, @Chinryou) ";
                sqlInsertIntoRentLivingRoom += "ON CONFLICT(room_id) ";
                //sqlInsertIntoRentLivingRoom += string.Format("DO UPDATE SET name = '{0}'", EscapeSingleQuote(room.RoomName));
                sqlInsertIntoRentLivingRoom += "DO UPDATE SET name = @Nam, chinryou = @Chinryou"; //, updated_at = @Updated

                cmd.CommandText = sqlInsertIntoRentLivingRoom;

                /*
                if (room.IsNew)
                {
                    var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_residential_rooms (room_id, property_id, name) VALUES (@roomId, @RentId, @Nam)";

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
                //cmd.Parameters.AddWithValue("@Updated", DateTimeOffset.UtcNow.ToString("s"));

                var result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    //room.IsNew = false;
                    room.IsModified = false;
                    room.PropertyStatus = EnumEntryStatus.Saved;
                    room.Status = EnumEntryStatus.Saved;
                }
                res.AffectedCount = result;

                cmd.Parameters.Clear();

                // 部屋写真 rent_residential_room_pictures table - Insert or Update
                if (room.Pictures.Count > 0)
                {
                    foreach (var pic in room.Pictures)
                    {
                        /*
                        var exec = false;

                        if (pic.IsNew)
                        {
                            var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_room_pictures (picture_id, property_id, file_path, label, description, is_main) " +
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
                        var sqlUpsertRoom = "INSERT INTO rent_residential_room_pictures (picture_id, room_id, property_id, file_path, type, description, is_main) VALUES (@PicId, @roomId, @RentId, @Path, @type, @Desc, @Main) ";
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

                // 部屋写真の削除リストを処理
                if (room.PicturesToBeDeleted.Count > 0)
                {
                    foreach (var delr in room.PicturesToBeDeleted)
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

                // 部屋図面 rent_residential_room_pdfs table - Insert or Update
                if (room.Pdfs.Count > 0)
                {
                    foreach (var pdf in room.Pdfs)
                    {
                        // Upsert
                        var sqlUpsertRoom = "INSERT INTO rent_residential_room_pdfs (pdf_id, room_id, property_id, file_path, thumbnail_path, type, description, is_main) VALUES (@PdfId, @roomId, @RentId, @Path, @Thumb, @type, @Desc, @Main) ";
                        sqlUpsertRoom += "ON CONFLICT(pdf_id) ";
                        sqlUpsertRoom += "DO UPDATE SET file_path = @Path, type = @type, description = @Desc, is_main = @Main";
                        var exec = true;

                        cmd.CommandText = sqlUpsertRoom;

                        if (exec)
                        {
                            // ループなので、前のパラメーターをクリアする。
                            cmd.Parameters.Clear();

                            cmd.Parameters.AddWithValue("@PdfId", pdf.Id);
                            cmd.Parameters.AddWithValue("@roomId", room.Id);
                            cmd.Parameters.AddWithValue("@RentId", rentId);
                            cmd.Parameters.AddWithValue("@Path", pdf.PdfLocation);
                            cmd.Parameters.AddWithValue("@Thumb", pdf.ThumbnailLocation);
                            cmd.Parameters.AddWithValue("@type", pdf.PdfType.Key.ToString());
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

                            result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                pdf.IsNew = false;
                                pdf.IsModified = false;
                            }
                        }

                    }
                }

                // 部屋図面の削除リストを処理
                if (room.PdfsToBeDeleted.Count > 0)
                {
                    foreach (var delr in room.PdfsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingRoom = string.Format("DELETE FROM rent_residential_room_pdfs WHERE pdf_id = '{0}'", delr.Id);

                        cmd.CommandText = sqlDeleteRentLivingRoom;
                        var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingRoomResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Room Pdf deleted @UpdateRentResidential in DataAccessService");
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

                Debug.WriteLine($"Exception@UpsertRentResidentialListing {e}");

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

    public SelectListingResultWrapper SelectRentResidentialListings()
    {
        var res = new SelectListingResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = "SELECT property.name as propertyName, rent_residential_rooms.name as roomName, rent_residential_rooms.room_id as roomId, property.property_id as propertyId FROM rent_residential_rooms INNER JOIN property USING (property_id) INNER JOIN rent_residentials USING (property_id)";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var eid = Convert.ToString(reader["propertyId"]);
                if (string.IsNullOrEmpty(eid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: propertyId is null or empty for a rent residential.");
                    continue;
                }

                var rid = Convert.ToString(reader["roomId"]);
                if (string.IsNullOrEmpty(rid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialsByNameKeyword: roomId is null or empty for a rent residential room.");
                    continue;
                }

                var unit = new Models.Common.ListingSearchResultItem(rid, eid);

                var s = Convert.ToString(reader["roomName"]) ?? "";
                unit.Name = s;

                //Debug.WriteLine($"Found rent residential entry: {entry.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["propertyName"]);
                if (!string.IsNullOrEmpty(s))
                {
                    unit.PropertyName = s;
                }

                // Reset entry Isdirty flag.
                unit.IsModified = false;

                //res.AffectedCount++;

                res.ListingSearchResult.Add(unit);
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

    public SelectRentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId)
    {
        var res = new SelectRentResidentialRoomSingleResultWrapper();

        if (string.IsNullOrEmpty(roomId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        if (string.IsNullOrEmpty(rentId))
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
            cmd.CommandText = "SELECT property.property_id as buildingId, property.name as buildingName, rent_residential_rooms.room_id as roomId, rent_residential_rooms.name as roomName, rent_residential_rooms.chinryou as chinryou FROM rent_residential_rooms INNER JOIN property USING (property_id)";//INNER JOIN rent_residentials USING (property_id)

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                //var Id = Convert.ToString(reader["roomId"]) ?? string.Empty;
                var Id = reader.GetString(reader.GetOrdinal("roomId")) ?? string.Empty;
                if (Id.Equals(roomId))
                {
                    var room = new Models.Rent.Residentials.Listing.Listing(roomId, rentId, EnumEntryStatus.Saved, EnumEntryStatus.Saved, reader.GetString(reader.GetOrdinal("buildingName")) ?? string.Empty)
                    {
                        Name = reader.GetString(reader.GetOrdinal("roomName")) ?? string.Empty,
                        Chinryou = reader.GetInt32(reader.GetOrdinal("chinryou")),
                        Status = EnumEntryStatus.Saved,
                        //IsNew = false,
                        IsModified = false
                    };

                    //Debug.WriteLine($"Room ID: {room.Id}, Room Name: {room.RoomName}");

                    // TODO:

                    // pics? 

                    // pdfs?



                    res.Room = room;
                    // break;
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
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialRoomById";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentResidentialRoomById");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialRoomById";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentResidentialRoomById");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentResidentialRoomById");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentResidentialRoomById";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteRentResidentialListing(string roomId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(roomId))
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
                cmd.CommandText = string.Format("DELETE FROM rent_residential_rooms WHERE room_id = '{0}';", roomId);
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
                res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidentialListing";

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
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidentialListing";

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
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidentialListing";

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
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::DeleteRentResidentialListing");
            }
            else
            {
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
                Debug.WriteLine(e.Message + " @DataAccess::DeleteRentResidentialListing");
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),cmd.ExecuteNonQuery()";
            res.Error.ErrPlaceParent = "DataAccess::DeleteRentResidentialListing";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    public ResultWrapper UpsertRentLessor(Models.Rent.Lessors.Person lessor)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(lessor.Id))
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

                // Upsert into rent_lessor
                var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_lessors (lessor_id, name, name_last, name_first, remarks) VALUES (@lessor_id, @name, @name_last, @name_first, @remarks) ";
                sqlInsertIntoRentLivingRoom += "ON CONFLICT(lessor_id) ";
                sqlInsertIntoRentLivingRoom += "DO UPDATE SET name = @name, name_last = @name_last, name_first = @name_first, remarks = @remarks, updated_at = @updated_at";

                cmd.CommandText = sqlInsertIntoRentLivingRoom;

                cmd.Parameters.AddWithValue("@lessor_id", lessor.Id);
                cmd.Parameters.AddWithValue("@name", lessor.Name);
                cmd.Parameters.AddWithValue("@name_last", lessor.NameLast);
                cmd.Parameters.AddWithValue("@name_first", lessor.NameFirst);
                cmd.Parameters.AddWithValue("@remarks", lessor.Remarks);
                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));

                var result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    lessor.IsModified = false;
                    lessor.Status = EnumEntryStatus.Saved;
                }
                res.AffectedCount = result;

                // Commit
                cmd.Transaction.Commit();
            }
            catch (Exception e)
            {
                cmd.Transaction.Rollback();

                Debug.WriteLine($"Exception@UpsertRentLessor {e}");

                res.IsError = true;
                res.Error.ErrType = ErrorObject.ErrTypes.DB;
                res.Error.ErrCode = "";
                res.Error.ErrText = e.Message;
                res.Error.ErrDescription = "Exception";
                res.Error.ErrDatetime = DateTime.Now;
                res.Error.ErrPlace = "connection.Open(),Transaction.Commit";
                res.Error.ErrPlaceParent = "DataAccess::UpsertRentLessor";

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
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentLessor";

            return res;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::UpsertRentLessor");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentLessor";

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
            res.Error.ErrPlaceParent = "DataAccess::UpsertRentLessor";

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public SelectPersonsResultWrapper SelectRentLessorByKeyword(string keyword)
    {
        var res = new SelectPersonsResultWrapper();

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
                cmd.CommandText = "SELECT lessor_id, name, remarks FROM rent_lessors";
            }
            else
            {
                cmd.CommandText = string.Format("SELECT lessor_id, name, remarks FROM rent_lessors WHERE REPLACE(REPLACE(name, ' ', ''), '　', '') LIKE '%{0}%'", keyword);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["lessor_id"]);
                if (string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine("DataAccess::SelectRentLessorByKeyword: lessor_id is null or empty.");
                    continue;
                }

                var entry = new Models.Common.PersonSearchResultItem(s);

                s = Convert.ToString(reader["name"]) ?? "";
                entry.Name = s;

                //Debug.WriteLine($"Found rent residential entry: {entry.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["remarks"]);
                if (!string.IsNullOrEmpty(s))
                {
                    //
                }

                // Reset entry Isdirty flag.
                entry.IsModified = false;

                //res.AffectedCount++;

                res.PersonSearchResult.Add(entry);
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
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorByKeyword";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentLessorByKeyword");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorByKeyword";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentLessorByKeyword");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentLessorByKeyword");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorByKeyword";
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public SelectRentLessorSingleResultWrapper SelectRentLessorById(string id)
    {
        var res = new SelectRentLessorSingleResultWrapper();

        var entry = new Models.Rent.Lessors.Person(id, EnumEntryStatus.Saved);

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

            cmd.CommandText = $"SELECT lessor_id, name, name_last, name_first, remarks FROM rent_lessors WHERE lessor_id = '{id}'";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var s = Convert.ToString(reader["lessor_id"]);
                    if (string.IsNullOrEmpty(s))
                    {
                        Debug.WriteLine("DataAccess::SelectRentLessorById: lessor_id is null or empty for a lessor entry.");
                        continue;
                    }

                    s = Convert.ToString(reader["name"]) ?? "";
                    entry.Name = s;

                    s = Convert.ToString(reader["name_last"]) ?? "";
                    entry.NameLast = s;

                    s = Convert.ToString(reader["name_first"]) ?? "";
                    entry.NameFirst = s;


                    s = Convert.ToString(reader["remarks"]) ?? "";
                    entry.Remarks = s;


                    // TODO: more.

                    //res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            // Reset entry Isdirty flag.
            entry.Status = EnumEntryStatus.Saved;
            entry.IsModified = false;

            res.Lessor = entry;
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
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorById";
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::SelectRentLessorById");

            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            res.Error.ErrDescription = "InvalidOperationException";
            res.Error.ErrText = ex.Message;
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorById";
        }
        catch (Exception e)
        {
            res.IsError = true;
            res.Error.ErrType = ErrorObject.ErrTypes.DB;
            res.Error.ErrCode = "";
            if (e.InnerException != null)
            {
                Debug.WriteLine(e.InnerException.Message + " @DataAccess::SelectRentLessorById");
                res.Error.ErrDescription = "InnerException";
                res.Error.ErrText = e.InnerException.Message;
            }
            else
            {
                Debug.WriteLine(e.Message + " @DataAccess::SelectRentLessorById");
                res.Error.ErrDescription = "Exception";
                res.Error.ErrText = e.Message;
            }
            res.Error.ErrDatetime = DateTime.Now;
            res.Error.ErrPlace = "connection.Open(),ExecuteReader()";
            res.Error.ErrPlaceParent = "DataAccess::SelectRentLessorById";
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
