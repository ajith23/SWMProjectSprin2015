using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Utility
{
    public static class MySqlAccess
    {
        public static string ConnectingString = "Server=xxxx;Port=3306;Database=xxxx;Uid=xxxx;Pwd=xxxx;";

        private static MySqlConnection EstablishConnection()
        {
            return new MySqlConnection(ConnectingString);
        }

        public static DataSet ExecuteQuery(string query)
        {
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);
                command.CommandType = CommandType.Text;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);
            }
            return dataSet;
        }

        public static void ExecuteNonQuery(string query)
        {
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        public static DataSet ExecuteStoredProcedure(string procedureName)
        {
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand(procedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);
            }
            return dataSet;
        }

        public static DataSet GetItemListFromSP(int categoryId)
        {
            var procedureName = "sp_getItemList";
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand(procedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new MySqlParameter { ParameterName = "category_id", Value = categoryId });
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);
            }
            return dataSet;
        }

        public static DataSet GetItemFeatureFromSP(int itemId)
        {
            var procedureName = "sp_getFeatureList";
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand(procedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new MySqlParameter { ParameterName = "in_item_id", Value = itemId });
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);
            }
            return dataSet;
        }

        public static DataSet GetFeatureReviewList(int itemId, int featureId)
        {
            var dataSet = new DataSet();
            using (var connection = new MySqlConnection(ConnectingString))
            {
                connection.Open();
                var command = new MySqlCommand("SELECT `review_text` FROM `review` WHERE `item_ref_id` = "+ itemId + " AND `feature_ref_id` = "+featureId, connection);
                command.CommandType = CommandType.Text;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);
            }
            return dataSet;
        }

        public static int GetCategoryId(string categoryName)
        {
            var categoryId = 0;

            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand("select category_id from category where category_name='" + categoryName + "'", connection);
                command.CommandType = CommandType.Text;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);

                if(dataSet.Tables[0].Rows.Count == 0)
                {
                    ExecuteNonQuery("insert into category (category_name) values ('" + categoryName + "')");
                    command = new MySqlCommand("select category_id from category where category_name='" + categoryName + "'", connection);
                    command.CommandType = CommandType.Text;
                    adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataSet);
                }
            }

            categoryId = Convert.ToInt32(dataSet.Tables[0].Rows[0][0]);
            return categoryId;
        }

        public static int GetProductId(string productName, int categoryId, double productCost)
        {
            var dataSet = new DataSet();
            using (var connection = EstablishConnection())
            {
                connection.Open();
                var command = new MySqlCommand("select item_id from item where item_name='" + productName + "'", connection);
                command.CommandType = CommandType.Text;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataSet);

                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    ExecuteNonQuery("insert into item (item_name, category_ref_id, item_cost) values ('"+productName+"', "+categoryId+", "+productCost+")");
                    command = new MySqlCommand("select item_id from item where item_name='" + productName + "'", connection);
                    command.CommandType = CommandType.Text;
                    adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataSet);
                }
            }

            return Convert.ToInt32(dataSet.Tables[0].Rows[0][0]);
        }

    }
}