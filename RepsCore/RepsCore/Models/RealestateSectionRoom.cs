using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reps.Models
{
    public class RealestateSectionRoom : RealestateSection
    {
        #region フィールド

        #endregion

        // コンストラクタ
        public RealestateSectionRoom()
        {
            this.IsVacant = false;
        }

        #region プロパティ
        
        #endregion

        #region method

        public override bool SectionOpen(string tmpRoomGUID)
        {
            if (string.IsNullOrEmpty(tmpRoomGUID))
            {
                Console.WriteLine("ERROR tmpRoomGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR tmpRoomGUID is empty.\n- " + " @SectionOpen() in RealestateSectionExRoomM \n- " + DateTime.Now + "\n\n";
                return false;
            }

            string queryString = @"SELECT * FROM Rems_Building_Room WHERE RoomGUID = @RoomGUID";

            if (this.ExecuteSelectSection(queryString, tmpRoomGUID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SectionReOpen()
        {
            // TODO:
            return true;
        }

        public override bool SectionSave()
        {
            // TODO
            if (this.RealestateGUID == string.Empty)
            {
                Console.WriteLine("ERROR this.buildingGUID is empty.");
                return false;
            }


            string queryString = @"
UPDATE Rems_Building_Room 
SET 
    Name=@Name, 
    Memo=@Memo,
    UpdateGUID=@UpdateGUID
WHERE 
    RoomGUID = @RoomGUID";

            return ExecuteInsertOrUpdateSection(queryString, this.GUID, Guid.NewGuid().ToString());
        }

        public override bool SectionSaveAsNew()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                // TODO: exit
            }

            if (string.IsNullOrEmpty(this.RealestateGUID))
            {
                // TODO: exit
            }

            string queryString = @"
INSERT INTO Rems_Building_Room 
    (Name,IsVacant,Memo,BuildingGUID,RoomGUID,UpdateGUID) 
VALUES
    (@Name,@IsVacant,@Memo,@BuildingGUID,@RoomGUID,@UpdateGUID)";

            return this.ExecuteInsertOrUpdateSection(queryString, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public override bool SectionDelete()
        {
            // TODO
            return true;
        }

        public override void SectionReset()
        {
            // TODO
        }

        protected override bool ExecuteInsertOrUpdateSection(string queryString, string tmpRoomGUID, string tmpUpdateGUID)
        {
            if (tmpRoomGUID == string.Empty)
            {
                Console.WriteLine("ERROR tmpRoomGUID is empty.");
                return false;
            }

            if (this.RealestateGUID == string.Empty)
            {
                Console.WriteLine("ERROR this.buildingGUID is empty.");
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

                            command.Parameters.AddWithValue("@IsVacant", ((object)this.IsVacant) ?? DBNull.Value);

                            // command.Parameters.Add(new SqlCeParameter("@Memo", _BuildingMemo));// _BuildingMemoが空の場合エラー
                            command.Parameters.AddWithValue("@Memo", ((object)this.Memo) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@BuildingGUID", this.RealestateGUID);
                            //// param.SqlDbType = SqlDbType.UniqueIdentifier;
                            //// param.Value = this.buildingGUID;
                            //// command.Parameters.Add(param);

                            command.Parameters.AddWithValue("@RoomGUID", tmpRoomGUID);

                            // TODO:
                            command.Parameters.Add(new SqlCeParameter("@UpdateGUID", tmpUpdateGUID));

                            // execute 
                            int result = command.ExecuteNonQuery();

                            //// check affected row count
                            //// if (result == 1) {return true;} else { return false; }
                            if (result > 0)
                            {
                                this.SectionGUID = tmpRoomGUID;
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
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                return false;
            }
            */
            return true;

        }

        protected override bool ExecuteSelectSection(string queryString, string tmpRoomGUID)
        {
            if (string.IsNullOrEmpty(tmpRoomGUID))
            {
                Console.WriteLine("ERROR tmpRoomGUID is empty.");
                this.ErrorString = this.ErrorString + "ERROR tmpRoomGUID is empty.\n- " + " @executeSelect() in RealestateSectionExRoomM \n- " + DateTime.Now + "\n\n";
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
                            command.Parameters.AddWithValue("@RoomGUID", tmpRoomGUID);

                            // TODO
                            //// string Result = (string)command.ExecuteScalar(); // returns the first column of the first row

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                bool result = false;
                                //// if (reader.HasRows){ 
                                while (reader.Read())
                                {
                                    this.Name = (string)reader["Name"];

                                    if (!reader.IsDBNull(reader.GetOrdinal("Memo"))) { 
                                        this.Memo = (string)reader["Memo"];
                                    }
                                    //this.Memo = (string)reader["Memo"] ?? string.Empty;

                                    // var tmpGUID = new Guid();
                                    // tmpGUID = (Guid)reader["UpdateGUID"];
                                    Guid myGuid;
                                    myGuid = (Guid)reader["UpdateGUID"];
                                    this.UpdateGUID = myGuid.ToString();
                                    //// this.updateGUID = ((Guid)reader["UpdateGUID"]).ToString();//tmpGUID.ToString();

                                    myGuid = (Guid)reader["BuildingGUID"];
                                    this.RealestateGUID = myGuid.ToString();

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
        
        #endregion

    }
}
