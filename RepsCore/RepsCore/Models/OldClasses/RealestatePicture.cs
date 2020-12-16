using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Configuration;
using System.Data;

namespace Reps.Models
{
    public class RealestatePicture
    {
        #region フィールド

        private string pictureGUID;
        private string buildingGUID;


        #endregion

        #region プロパティ

        public string GUID
        {
            get { return this.pictureGUID; }
            set { this.pictureGUID = value; }
        }

        public string BuildingGUID
        {
            get { return this.buildingGUID; }
            set { this.buildingGUID = value; }
        }

        public string UpdateGUID { get; set; }

        protected string ErrorString { get; set; }

        


        public int PictureKind { get; set; }
        public string PictureLabel { get; set; }
        public string PictureMemo { get; set; }
        public byte[] PictureContent { get; set; }
        public byte[] PictureThumb { get; set; }
        public string PictureFileExt { get; set; }

        //createdOn
        //updatedOn

        #endregion

        #region メソッド

        // コンストラクタ
        public RealestatePicture()
        {
            this.pictureGUID = string.Empty;
        }

        public bool PictureSaveAsNew(string buildingGUID)
        {
            this.buildingGUID = buildingGUID;



            string queryString = @"
INSERT INTO Rems_Building_Picture 
    (PictureGUID,Picture,PictureKind,PictureLabel,PictureMemo,PictureFileExt,CreatedOn,UpdatedOn,BuildingGUID,UpdateGUID) 
VALUES
    (@PictureGUID,@Ociture,@PictureKind,@PictureLabel,@PictureMemo,@PictureFileExt,@CreatedOn,@UpdatedOn,@BuildingGUID,@UpdateGUID)";

            return this.ExecuteInsertOrUpdatePicture(queryString, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        protected bool ExecuteInsertOrUpdatePicture(string queryString, string tmpPictureGUID, string tmpUpdateGUID)
        {
            if (buildingGUID == string.Empty)
            {
                Console.WriteLine("ERROR buildingGUID is empty.");
                return false;
            }

            //TODO
            Console.WriteLine("ExecuteInsertOrUpdatePicture");
            return true;

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

                            command.Parameters.AddWithValue("@Memo", ((object)this.Memo) ?? DBNull.Value);

                            command.Parameters.AddWithValue("@BuildingGUID", this.BuildingGUID);

                            command.Parameters.AddWithValue("@PictureGUID", tmpPictureGUID);

                            command.Parameters.Add(new SqlCeParameter("@UpdateGUID", tmpUpdateGUID));

                            // execute 
                            int result = command.ExecuteNonQuery();

                            //// check affected row count
                            //// if (result == 1) {return true;} else { return false; }
                            if (result > 0)
                            {
                                this.GUID = tmpPictureGUID;
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
        }


        #endregion

    }
}
