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
    public class SearchRealestate : Search
    {
        // constructor
        public SearchRealestate()
        {
            this.Results = new ObservableCollection<SearchRealestateResult>();
        }

        public ObservableCollection<SearchRealestateResult> Results { get; }

        #region method
        public override bool DoSearch(string searchText)
        {
            this.SearchText = searchText;
            this.Results.Clear();
            /*
            // DBサーバへの接続情報
            var connectionString = ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString;

            string queryString;

            if (this.SearchText == "*")
            {
                queryString = @"SELECT Name, BuildingGUID FROM Rems_Building";
            }
            else
            {
                queryString = @"SELECT Name, BuildingGUID FROM Rems_Building WHERE Name LIKE @Search";
            }

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
                            command.Parameters.AddWithValue("@Search", "%" + this.SearchText + "%");

                            // TODO
                            //// string Result = (string)command.ExecuteScalar(); // returns the first column of the first row

                            using (SqlCeDataReader reader = command.ExecuteReader())
                            {
                                // if (reader.HasRows){ 
                                while (reader.Read())
                                {
                                    REMS.Models.SearchRealestateResultBaseM resultRow = new REMS.Models.SearchRealestateResultBaseM();

                                    resultRow.Name = (string)reader["Name"];
                                    Guid gu = (Guid)reader["BuildingGUID"];
                                    resultRow.GUID = gu.ToString();

                                    this.Results.Add(resultRow);
                                }
                                //// }
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

            /*
            REMS.Models.SearchRealestateResultModel resultRow = new REMS.Models.SearchRealestateResultModel();
            resultRow.BuildingName = "テスト物件";
            resultRow.RoomName = "1号室";
            resultRow.BuildingAddress = "東京都多摩市（住所）";
            this.Results.Add(resultRow);

            Console.WriteLine("SearchModel.doSearch");
            */
            return true;
        }
        #endregion

    }
}
