using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reps.Models
{
    public class RealestateBuilding : Realestate
    {
        #region フィールド

        #endregion

        // コンストラクタ
        public RealestateBuilding()
        {
            // TODO
            //this.Rooms = new ObservableCollection<RealestateSectionExRoomM>();
            //this.Pictures = new ObservableCollection<RealestatePictureM>();
        }

        #region プロパティ

        //public ObservableCollection<RealestateSectionExRoomM> Rooms { get; }
        //public ObservableCollection<RealestatePictureM> Pictures { get; }

        #endregion

        #region メソッド

        public override bool RealestateOpen(string tmpBuildingGUID)
        {
            if (string.IsNullOrEmpty(tmpBuildingGUID))
            {
                Console.WriteLine("ERROR tmpBuildingGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @buildingOpen() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            string queryString = @"SELECT * FROM Rems_Building WHERE BuildingGUID = @BuildingGUID";

            if (this.ExecuteSelectRealestate(queryString, tmpBuildingGUID))
            {
                // select get Rooms
                queryString = @"SELECT * FROM Rems_Building_Room WHERE BuildingGUID = @BuildingGUID";
                if (this.ExecuteSelectSections(queryString))
                {
                    //return true;
                }
                else
                {
                    //TODO: error?
                    //this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @buildingOpen() in BuildingModel \n- " + DateTime.Now + "\n\n";
                    //return false;
                }

                // select get Pictures
                queryString = @"SELECT * FROM Rems_Building_Picture WHERE BuildingGUID = @BuildingGUID";
                if (this.ExecuteSelectPictures(queryString))
                {
                    //return true;
                }
                else
                {
                    //TODO: error?
                    //this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @buildingOpen() in BuildingModel \n- " + DateTime.Now + "\n\n";
                    //return false;
                }

                return true;
            }
            else
            {
                //TODO: error?
                //this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @buildingOpen() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }
        }

        public override bool RealestateReOpen()
        {
            //// TODO:
            return true;
        }

        public override bool RealestateSectionReOpen()
        {
            // select get Rooms
            string queryString = @"SELECT * FROM Rems_Building_Room WHERE BuildingGUID = @BuildingGUID";

            if (this.ExecuteSelectSections(queryString))
            {
                return true;
            }
            else
            {
                // this.errStr = this.ErrorString + "ERROR this.buildingName is empty.\n- " + " @buildingSaveAsNew() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }
        }

        public override bool RealestateSave()
        {
            if (string.IsNullOrEmpty(this.GUID))
            {
                Console.WriteLine("ERROR this.buildingGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR this.buildingGUID is empty.\n- " + " @buildingSave() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            string queryString = @"
UPDATE Rems_Building 
SET 
    Name=@Name, 
    Name_Kana=@Name_Kana,
    AddressPostalCode=@AddressPostalCode,
    AddressPrefCode=@AddressPrefCode,
    AddressCityCode=@AddressCityCode,
    AddressChoume=@AddressChoume,
    AddressBanchi=@AddressBanchi,
    AddressFullString=@AddressFullString,
    Memo=@Memo,
    UpdateGUID=@UpdateGUID
WHERE 
    BuildingGUID = @BuildingGUID";

            return this.ExecuteInsertOrUpdateRealestate(queryString, this.RealestateGUID, Guid.NewGuid().ToString());
        }

        public override bool RealestateSaveAsNew()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                Console.WriteLine("ERROR this.buildingName is empty.");
                this.ErrorString = this.ErrorString + "ERROR this.buildingName is empty.\n- " + " @buildingSaveAsNew() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            string queryString = @"
INSERT INTO Rems_Building 
    (Name,Name_Kana,AddressPostalCode,AddressPrefCode,AddressCityCode,AddressChoume,AddressBanchi,AddressFullString,Memo,BuildingGUID,UpdateGUID) 
VALUES
    (@Name,@Name_Kana,@AddressPostalCode,@AddressPrefCode,@AddressCityCode,@AddressChoume,@AddressBanchi,@AddressFullString,@Memo,@BuildingGUID,@UpdateGUID)";

            return this.ExecuteInsertOrUpdateRealestate(queryString, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public override bool RealestateDelete()
        {
            //// TODO
            return true;
        }

        public override void RealestateReset()
        {
            //// TODO
        }

        protected override bool ExecuteSelectRealestate(string queryString, string tmpBuildingGUID)
        {
            if (string.IsNullOrEmpty(tmpBuildingGUID))
            {
                Console.WriteLine("ERROR tmpBuildingGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @executeSelect() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            // TODO:
            //// http://stackoverflow.com/questions/30392251/executing-sql-query-from-c-sharp-code
            /*
            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCeCommand command = new SqlCeCommand(queryString, connection))
                        {
                            // command.Parameters.AddWithValue("@BuildingGUID", (System.Guid)tmpBuildingGUID);
                            command.Parameters.AddWithValue("@BuildingGUID", tmpBuildingGUID);

                            // TODO
                            //// string Result = (string)command.ExecuteScalar(); // returns the first column of the first row

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                bool result = false;
                                //// if (reader.HasRows){ 
                                while (reader.Read())
                                {
                                    this.Name = (string)reader["Name"];
                                    this.NameKana = (string)reader["Name_Kana"];
                                    this.AddressPostalCode = (int)reader["AddressPostalCode"];
                                    this.AddressPrefCode = (int)reader["AddressPrefCode"];
                                    this.AddressCityCode = (int)reader["AddressCityCode"];
                                    this.AddressFullString = (string)reader["AddressFullString"] ?? string.Empty;
                                    this.AddressChoume = (string)reader["AddressChoume"] ?? string.Empty;
                                    this.AddressBanchi = (string)reader["AddressBanchi"] ?? string.Empty;
                                    this.Memo = (string)reader["Memo"] ?? string.Empty;

                                    // var tmpGUID = new Guid();
                                    // tmpGUID = (Guid)reader["UpdateGUID"];
                                    Guid myGuid = (Guid)reader["UpdateGUID"];
                                    this.UpdateGUID = myGuid.ToString();
                                    //// this.updateGUID = ((Guid)reader["UpdateGUID"]).ToString();//tmpGUID.ToString();

                                    this.RealestateGUID = tmpBuildingGUID;

                                    result = true;
                                    break;
                                }
                                //// }
                                if (result)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing SQL (" + queryString + "): " + ex.Message);
                        this.ErrorString = this.ErrorString + "ERROR executing SQL (" + queryString + ")\n- " + ex.Message + "\n- @executeSelect() in BuildingModel; \n- " + DateTime.Now + "\n\n";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                this.ErrorString = this.ErrorString + "ERROR opening DB (" + queryString + ")\n- " + ex.Message + "\n- @executeSelect() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            */
            return true;
        }

        protected override bool ExecuteSelectSections(string queryString)
        {
            if (string.IsNullOrEmpty(this.RealestateGUID))
            {
                Console.WriteLine("ERROR this.buildingGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR this.buildingGUID is empty.\n- " + " @executeSelect_Rooms() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }
            /*
            this.Rooms.Clear();

            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCeCommand command = new SqlCeCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@BuildingGUID", this.RealestateGUID);

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                // if (reader.HasRows){ 
                                while (reader.Read())
                                {
                                    REMS.Models.RealestateSectionExRoomM room = new REMS.Models.RealestateSectionExRoomM();

                                    // Guid myGuid = (Guid)reader["BuildingGUID"];
                                    // room.BuildingGUID = myGuid.ToString;
                                    //room.GUID = this.RealestateGUID; //// ?

                                    Guid myGuid = (Guid)reader["RoomGUID"];
                                    room.GUID = myGuid.ToString();

                                    // room.Name = (string)reader["Name"];
                                    room.Name = reader["Name"] as string ?? default(string);

                                    room.IsVacant = (bool)reader["IsVacant"];

                                    // room.Memo = ((string)reader["Memo"] ?? "");
                                    room.Memo = reader["Memo"] as string ?? default(string);

                                    myGuid = (Guid)reader["UpdateGUID"];
                                    room.UpdateGUID = myGuid.ToString();

                                    this.Rooms.Add(room);
                                }
                                ////}
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing SQL (" + queryString + "): " + ex.Message);
                        this.ErrorString = this.ErrorString + "ERROR executing SQL (" + queryString + ")\n- " + ex.Message + "\n- @executeSelect_Rooms() in BuildingModel \n- " + DateTime.Now + "\n\n";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                this.ErrorString = this.ErrorString + "ERROR opening DB (" + queryString + ")\n- " + ex.Message + "\n- @executeSelect_Rooms() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }
            */
            return true;
        }

        protected override bool ExecuteSelectPictures(string queryString)
        {
            if (string.IsNullOrEmpty(this.RealestateGUID))
            {
                this.ErrorString = this.ErrorString + "ERROR this.buildingGUID is empty.\n- " + " @executeSelect_Rooms() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }
            /*
            this.Pictures.Clear();

            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCeCommand command = new SqlCeCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@BuildingGUID", this.RealestateGUID);

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    REMS.Models.RealestatePictureM picture = new REMS.Models.RealestatePictureM();

                                    // Guid myGuid = (Guid)reader["BuildingGUID"];
                                    // room.BuildingGUID = myGuid.ToString;
                                    //room.GUID = this.RealestateGUID; //// ?

                                    Guid myGuid = (Guid)reader["PictureGUID"];
                                    picture.GUID = myGuid.ToString();

                                    // room.Name = (string)reader["Name"];
                                    picture.PictureLabel = reader["PictureLabel"] as string ?? default(string);

                                    //room.IsVacant = (bool)reader["IsVacant"];

                                    // room.Memo = ((string)reader["Memo"] ?? "");
                                    picture.PictureMemo = reader["PictureMemo"] as string ?? default(string);

                                    //TODO picture binary etc

                                    //myGuid = (Guid)reader["UpdateGUID"];
                                    //room.UpdateGUID = myGuid.ToString();

                                    this.Pictures.Add(picture);
                                }

                                //test
                                REMS.Models.RealestatePictureM pictureTMP = new REMS.Models.RealestatePictureM();
                                pictureTMP.GUID = "hogehoge1";
                                pictureTMP.PictureLabel = "test data1";
                                pictureTMP.PictureMemo = "asdf";
                                this.Pictures.Add(pictureTMP);

                                pictureTMP = new REMS.Models.RealestatePictureM();
                                pictureTMP.GUID = "hogehoge2";
                                pictureTMP.PictureLabel = "test data2";
                                pictureTMP.PictureMemo = "asdfasdf";
                                this.Pictures.Add(pictureTMP);

                                pictureTMP = new REMS.Models.RealestatePictureM();
                                pictureTMP.GUID = "hogehoge3";
                                pictureTMP.PictureLabel = "test data3";
                                pictureTMP.PictureMemo = "asdfasdf";
                                this.Pictures.Add(pictureTMP);

                                pictureTMP = new REMS.Models.RealestatePictureM();
                                pictureTMP.GUID = "hogehoge4";
                                pictureTMP.PictureLabel = "test data4";
                                pictureTMP.PictureMemo = "asdfasdf";
                                this.Pictures.Add(pictureTMP);

                                pictureTMP = new REMS.Models.RealestatePictureM();
                                pictureTMP.GUID = "hogehoge5";
                                pictureTMP.PictureLabel = "test data5";
                                pictureTMP.PictureMemo = "asdfasdf";
                                this.Pictures.Add(pictureTMP);

                                Console.WriteLine("RealestatePictureM");
                                //

                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing SQL (" + queryString + "): " + ex.Message);
                        this.ErrorString = this.ErrorString + "ERROR executing SQL (" + queryString + ")\n- " + ex.Message + "\n- @ExecuteSelectPictures() in BuildingModel \n- " + DateTime.Now + "\n\n";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                this.ErrorString = this.ErrorString + "ERROR opening DB (" + queryString + ")\n- " + ex.Message + "\n- @ExecuteSelectPictures() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            */
            return true;
        }

        protected override bool ExecuteInsertOrUpdateRealestate(string queryString, string tmpBuildingGUID, string tmpUpdateGUID)
        {
            if (string.IsNullOrEmpty(tmpBuildingGUID))
            {
                Console.WriteLine("ERROR tmpBuildingGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR tmpBuildingGUID is empty.\n- " + " @executeInsertOrUpdate() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            /*
            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCeCommand command = new SqlCeCommand(queryString, connection))
                        {
                            SqlCeParameter param;

                            param = new SqlCeParameter("@Name", this.Name);
                            param.SqlDbType = SqlDbType.NVarChar;
                            command.Parameters.Add(param);

                            command.Parameters.AddWithValue("@Name_Kana", (object)this.NameKana ?? DBNull.Value); // SqlString.Null                           ;

                            command.Parameters.AddWithValue("@AddressPostalCode", ((object)this.AddressPostalCode) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@AddressPrefCode", ((object)this.AddressPrefCode) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@AddressCityCode", ((object)this.AddressCityCode) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@AddressChoume", ((object)this.AddressChoume) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@AddressBanchi", (object)this.AddressBanchi ?? DBNull.Value);

                            command.Parameters.AddWithValue("@AddressFullString", ((object)this.AddressFullString) ?? DBNull.Value);

                            // ommand.Parameters.Add(new SqlCeParameter("@Memo", this.buildingMemo));// this.buildingMemoが空の場合エラー
                            command.Parameters.AddWithValue("@Memo", ((object)this.Memo) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@BuildingGUID", tmpBuildingGUID);
                            //// param.SqlDbType = SqlDbType.UniqueIdentifier;
                            //// param.Value = this.buildingGUID;
                            //// command.Parameters.Add(param);

                            // TODO:
                            command.Parameters.Add(new SqlCeParameter("@UpdateGUID", tmpUpdateGUID));

                            // execute 
                            int result = command.ExecuteNonQuery();

                            // int insertedId = (int)cmd.ExecuteScalar();

                            // check affected row count
                            // if (result == 1) {return true;} else { return false; }
                            if (result > 0)
                            {
                                this.RealestateGUID = tmpBuildingGUID;
                                this.UpdateGUID = tmpUpdateGUID;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing SQL (" + queryString + "): " + ex.Message);
                        this.ErrorString = this.ErrorString + "ERROR executing SQL (" + queryString + ")\n- " + ex.Message + "\n- @executeInsertOrUpdate() in BuildingModel \n- " + DateTime.Now + "\n\n";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                this.ErrorString = this.ErrorString + "ERROR opening DB (" + queryString + ")\n- " + ex.Message + "\n- @executeInsertOrUpdate() in BuildingModel \n- " + DateTime.Now + "\n\n";
                return false;
            }

            */
            return true;
        }

        #endregion
    }
}
