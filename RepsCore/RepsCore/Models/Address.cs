using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REMS.Models
{
    #region some classes
    public class PrefPart
    {
        public PrefPart(int iD, string name)
        {
            this.ID = iD;
            this.Name = name;
        }

        public int ID { get; private set; } // jis_code の初めの二桁

        public string Name { get; private set; }
    }

    public class CityPart
    {
        public CityPart(int iD, string name)
        {
            this.ID = iD;
            this.Name = name;
        }

        public int ID { get; private set; } // jis_code

        public string Name { get; private set; }
    }

    public class TownPart
    {
        public TownPart(int iD, string name)
        {
            this.ID = iD;
            this.Name = name;
        }

        public int ID { get; private set; } // postal_code

        public string Name { get; private set; }
    }
    #endregion

    public class AddressModel
    {
        #region Fields
        private int postalCode;
        private int prefCode;
        private int cityCode;

        private string prefString;
        private string cityString;
        private string townString;

        #endregion

        // Constructor
        public AddressModel()
        {
            this.PrefParts = new ObservableCollection<PrefPart>();
            this.CityParts = new ObservableCollection<CityPart>();
            this.TownParts = new ObservableCollection<TownPart>();

            this.PrefParts.Add(new PrefPart(1, "北海道"));
            this.PrefParts.Add(new PrefPart(2, "青森県"));
            this.PrefParts.Add(new PrefPart(3, "岩手県"));
            this.PrefParts.Add(new PrefPart(4, "宮城県"));
            this.PrefParts.Add(new PrefPart(5, "秋田県"));
            this.PrefParts.Add(new PrefPart(6, "山形県"));
            this.PrefParts.Add(new PrefPart(7, "福島県"));
            this.PrefParts.Add(new PrefPart(8, "茨城県"));
            this.PrefParts.Add(new PrefPart(9, "栃木県"));
            this.PrefParts.Add(new PrefPart(10, "群馬県"));
            this.PrefParts.Add(new PrefPart(11, "埼玉県"));
            this.PrefParts.Add(new PrefPart(12, "千葉県"));
            this.PrefParts.Add(new PrefPart(13, "東京都"));
            this.PrefParts.Add(new PrefPart(14, "神奈川県"));
            this.PrefParts.Add(new PrefPart(15, "新潟県"));
            this.PrefParts.Add(new PrefPart(16, "富山県"));
            this.PrefParts.Add(new PrefPart(17, "石川県"));
            this.PrefParts.Add(new PrefPart(18, "福井県"));
            this.PrefParts.Add(new PrefPart(19, "山梨県"));
            this.PrefParts.Add(new PrefPart(20, "長野県"));
            this.PrefParts.Add(new PrefPart(21, "岐阜県"));
            this.PrefParts.Add(new PrefPart(22, "静岡県"));
            this.PrefParts.Add(new PrefPart(23, "愛知県"));
            this.PrefParts.Add(new PrefPart(24, "三重県"));
            this.PrefParts.Add(new PrefPart(25, "滋賀県"));
            this.PrefParts.Add(new PrefPart(26, "京都府"));
            this.PrefParts.Add(new PrefPart(27, "大阪府"));
            this.PrefParts.Add(new PrefPart(28, "兵庫県"));
            this.PrefParts.Add(new PrefPart(29, "奈良県"));
            this.PrefParts.Add(new PrefPart(30, "和歌山県"));
            this.PrefParts.Add(new PrefPart(31, "鳥取県"));
            this.PrefParts.Add(new PrefPart(32, "島根県"));
            this.PrefParts.Add(new PrefPart(33, "岡山県"));
            this.PrefParts.Add(new PrefPart(34, "広島県"));
            this.PrefParts.Add(new PrefPart(35, "山口県"));
            this.PrefParts.Add(new PrefPart(36, "徳島県"));
            this.PrefParts.Add(new PrefPart(37, "香川県"));
            this.PrefParts.Add(new PrefPart(38, "愛媛県"));
            this.PrefParts.Add(new PrefPart(39, "高知県"));
            this.PrefParts.Add(new PrefPart(40, "福岡県"));
            this.PrefParts.Add(new PrefPart(41, "佐賀県"));
            this.PrefParts.Add(new PrefPart(42, "長崎県"));
            this.PrefParts.Add(new PrefPart(43, "熊本県"));
            this.PrefParts.Add(new PrefPart(44, "大分県"));
            this.PrefParts.Add(new PrefPart(45, "宮崎県"));
            this.PrefParts.Add(new PrefPart(46, "鹿児島県"));
            this.PrefParts.Add(new PrefPart(47, "沖縄県"));
        }

        #region Properties

        public ObservableCollection<PrefPart> PrefParts { get; }

        public ObservableCollection<CityPart> CityParts { get; }

        public ObservableCollection<TownPart> TownParts { get; }

        public int PostalCode
        {
            get
            {
                return this.postalCode;
            }

            set
            {
                //// TODO： validate postalcode

                this.postalCode = value;
                this.prefCode = 0;
                this.cityCode = -1;

                this.prefString = string.Empty;
                this.cityString = string.Empty;
                this.townString = string.Empty;

                this.CityParts.Clear();
                this.TownParts.Clear();

                if (this.postalCode == 0)
                {
                    return;
                }
/*
                // DBサーバへの接続情報
                var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

                string queryString = @"SELECT pref_code, jis_code, prefecture, city, town FROM Rems_ZipCode WHERE zip_code = @PostalCode";
                //// + _PostalCode.ToString();

                // SqlCeConnection con = null;
                try
                {
                    using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                    {
                        using (SqlCeCommand cmd = new SqlCeCommand(queryString, connection))
                        {
                            SqlCeParameter param = new SqlCeParameter("@PostalCode", SqlDbType.Int, 50);
                            param.Value = this.postalCode;
                            cmd.Parameters.Add(param);

                            connection.Open();
                            using (SqlCeDataReader reader = cmd.ExecuteReader())
                            {
                                //// Check is the reader has any rows at all before starting to read.
                                // if (reader.HasRows)
                                // {
                                // Read advances to the next row.
                                while (reader.Read())
                                {

                                    this.prefCode = reader.GetInt32(reader.GetOrdinal("pref_code"));
                                    this.cityCode = reader.GetInt32(reader.GetOrdinal("jis_code"));

                                    this.prefString = reader.GetString(reader.GetOrdinal("prefecture"));
                                    this.cityString = reader.GetString(reader.GetOrdinal("city"));
                                    this.townString = reader.GetString(reader.GetOrdinal("town"));

                                    // 2060000 returns two result
                                    this.CityParts.Add(new CityPart(this.cityCode, this.cityString));
                                }
                                //// }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Reading from DB in Select: " + ex.Message);
                    return;
                }
*/

                if (!string.IsNullOrEmpty(this.townString))
                {
                    this.TownParts.Add(new TownPart(this.postalCode, this.townString));
                }

                if (this.CityParts.Count > 1)
                {
                    this.cityCode = 0;
                    this.cityString = string.Empty;
                    this.townString = string.Empty;
                }
            }
        }

        public int PrefCode
        {
            get
            {
                return this.prefCode;
            }

            set
            {
                //// 使ってない？

                this.postalCode = -1;
                this.prefCode = value;
                this.cityCode = -1;
                this.CityParts.Clear();
                this.TownParts.Clear();

                //// TODO: DB　ごにょごにょ

                //// this._this.CityParts
            }
        }

        public int CityCode
        {
            get
            {
                return this.cityCode;
            }

            set
            {
                //// 使ってない？

                this.postalCode = -1;
                this.prefCode = 0;
                this.cityCode = value;
                this.CityParts.Clear();
                this.TownParts.Clear();

                //// TODO: DB　ごにょごにょ

                ////_PostalCode = 
                ////_PrefCode

                // this._CityParts
                // this._TownParts
            }
        }

        public string PrefString
        {
            get
            {
                return this.prefString;
            }
        }

        public string CityString
        {
            get
            {
                return this.cityString;
            }
        }

        public string TownString
        {
            get
            {
                return this.townString;
            }
        }

        #endregion

        #region methods

        public void UpdateCityParts(int prefCode)
        {
            this.prefCode = prefCode;
            this.cityCode = -1;
            //// this._TownCode = -1;
            //// this._PostalCode  ??

            this.CityParts.Clear();
            this.TownParts.Clear();
            /*
            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            var commandText = @"SELECT DISTINCT jis_code, city FROM Rems_ZipCode WHERE pref_code = @PrefCode";
            //// + _PrefCode.ToString();

            // SqlCeConnection con = null;
            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    using (SqlCeCommand cmd = new SqlCeCommand(commandText, connection))
                    {
                        SqlCeParameter param = new SqlCeParameter("@PrefCode", SqlDbType.Int, 50);
                        param.Value = this.prefCode;
                        cmd.Parameters.Add(param);

                        connection.Open();
                        using (SqlCeDataReader reader = cmd.ExecuteReader())
                        {
                            // Check is the reader has any rows at all before starting to read.
                            // if (reader.HasRows)
                            // {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                ////_PrefCode = reader.GetInt32(reader.GetOrdinal("pref_code"));

                                // _CityCode = reader.GetInt32(reader.GetOrdinal("jis_code"));
                                this.cityCode = (int)reader["jis_code"];

                                //// this.prefString = reader.GetString(reader.GetOrdinal("prefecture"));

                                // this.cityString = reader.GetString(reader.GetOrdinal("city"));
                                this.cityString = (string)reader["city"];

                                //// this.townString = reader.GetString(reader.GetOrdinal("town"));

                                //// Console.WriteLine(_PrefCode.ToString() + ":" + _CityCode.ToString() + ":"+ this.prefString.ToString() + ":" + this.cityString.ToString() + ":" + this.townString.ToString());

                                this.CityParts.Add(new CityPart(this.cityCode, this.cityString));
                            }
                            //// }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Reading from DB in Select: " + ex.Message);
                return;
            }

            */
        }

        public void UpdateTownParts(int prefCode, int cityCode)
        {
            this.prefCode = prefCode;
            this.cityCode = cityCode;

            // CityParts.Clear();
            this.TownParts.Clear();

            int tempPostalCode = 0;
            int topPostalCode = 0;
            /*
            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            var commandText = @"SELECT DISTINCT zip_code, town FROM Rems_ZipCode WHERE jis_code = @CityCode";
            //// + _CityCode.ToString();

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(connectionString))
                {
                    using (SqlCeCommand cmd = new SqlCeCommand(commandText, connection))
                    {
                        SqlCeParameter param = new SqlCeParameter("@CityCode", SqlDbType.Int, 50);
                        param.Value = this.cityCode;
                        cmd.Parameters.Add(param);

                        connection.Open();
                        using (SqlCeDataReader reader = cmd.ExecuteReader())
                        {
                            // Check is the reader has any rows at all before starting to read.
                            // if (reader.HasRows)
                            // {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                // tempPostalCode = reader.GetInt32(reader.GetOrdinal("zip_code"));
                                tempPostalCode = (int)reader["zip_code"];

                                if (!(topPostalCode > 0))
                                {
                                    topPostalCode = tempPostalCode;
                                    this.postalCode = topPostalCode;
                                }

                                // this.townString = reader.GetString(reader.GetOrdinal("town"));
                                this.townString = (string)reader["town"];

                                //// Console.WriteLine(_PrefCode.ToString() + ":" + _CityCode.ToString() + ":"+ this.prefString.ToString() + ":" + this.cityString.ToString() + ":" + this.townString.ToString());

                                if (this.townString != string.Empty)
                                {
                                    this.TownParts.Add(new TownPart(tempPostalCode, this.townString));
                                }
                            }
                            //// }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Reading from DB in Select: " + ex.Message);
                return;
            }
            */
        }

        public int GetPostalCodeFromFullText(string fullAddress)
        {
            if (string.IsNullOrEmpty(fullAddress))
            {
                Console.WriteLine("ERROR FullAddress is empty.");
                return 0;
            }

            string queryString = @"SELECT zip_code FROM Rems_ZipCode WHERE pref_city_town_joint LIKE @FullAddress";

            int intPostalCode = 0;
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
                            command.Parameters.AddWithValue("@FullAddress", fullAddress);

                            // TODO
                            //// string Result = (string)command.ExecuteScalar(); // returns the first column of the first row

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                // if (reader.HasRows){ 
                                while (reader.Read())
                                {
                                    intPostalCode = (int)reader["zip_code"];
                                    
                                    break;
                                }
                                //// }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error executing SQL (" + queryString + "): " + ex.Message);
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening DB (" + connectionString + "): " + ex.Message);
                return 0;
            }
            */
            return intPostalCode;
        }   
    }
    #endregion
}