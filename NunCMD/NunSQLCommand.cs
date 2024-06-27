using System;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Data.SqlTypes;

namespace NunCMD
{
    public class NunSQLCommand:SqlCompareOptions
    {
        public string Connect { get; set; }

        public NunSQLCommand(string connect)
        {
            this.Connect = connect;
        }

        /// THIS IS USE FOR ADD RECORD

        public void AddNomalRecord(string table_name, string column_name_1, object control_1)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}) VALUES (@column_name_1)", conn);
                    cmd.Parameters.AddWithValue("@column_name_1", control_1);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}) VALUES (@column_name_1, @column_name_2)", conn);
                    cmd.Parameters.AddWithValue("@column_name_1", control_1);
                    cmd.Parameters.AddWithValue("@column_name_2", control_2);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}) VALUES (@col_1, @col_2, @col_3)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}) VALUES (@col_1, @col_2, @col_3, @col_4)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8},{column_name_9}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8, @col_9)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9, string column_name_10, object control_10)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8},{column_name_9},{column_name_10}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8, @col_9, @col_10)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.Parameters.AddWithValue("@col_10", control_10);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9, string column_name_10, object control_10, string column_name_11, object control_11)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8},{column_name_9},{column_name_10},{column_name_11}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8, @col_9, @col_10, @col_11)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.Parameters.AddWithValue("@col_10", control_10);
                    cmd.Parameters.AddWithValue("@col_11", control_11);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9, string column_name_10, object control_10, string column_name_11, object control_11, string column_name_12, object control_12)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8},{column_name_9},{column_name_10},{column_name_11}, {column_name_12}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8, @col_9, @col_10, @col_11, @col_12)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.Parameters.AddWithValue("@col_10", control_10);
                    cmd.Parameters.AddWithValue("@col_11", control_11);
                    cmd.Parameters.AddWithValue("@col_12", control_12);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void AddNomalRecord(string table_name, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9, string column_name_10, object control_10, string column_name_11, object control_11, string column_name_12, object control_12, string column_name_13, object control_13)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}, {column_name_2}, {column_name_3}, {column_name_4}, {column_name_5}, {column_name_6}, {column_name_7},{column_name_8},{column_name_9},{column_name_10},{column_name_11}, {column_name_12}, {column_name_13}) VALUES (@col_1, @col_2, @col_3, @col_4, @col_5, @col_6, @col_7, @col_8, @col_9, @col_10, @col_11, @col_12, @col_13)", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.Parameters.AddWithValue("@col_10", control_10);
                    cmd.Parameters.AddWithValue("@col_11", control_11);
                    cmd.Parameters.AddWithValue("@col_12", control_12);
                    cmd.Parameters.AddWithValue("@col_13", control_13);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }





        /// THIS IS USE FOR ADD WITH OUT PUT RECORD

        public string AddWithOutPutRecord(string table_name, string output, string column_name_1, object control_1)
        {
            string outPut = "";
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"INSERT INTO {table_name} ({column_name_1}) OUTPUT INSERTED.{output} VALUES (@column_name_1)", conn);
                    cmd.Parameters.AddWithValue("@column_name_1", control_1);
                    outPut = cmd.ExecuteScalar().ToString();
                    MessageBox.Show("Add Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return outPut;
        }





        /// THIS IS USE FOR READ DATA FROM DATABASE

        public DataTable SelectByQuery(string sql_text)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(sql_text, conn);
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dt;
        }

        public DataTable SelectAllNomalRecord(string table_name)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {table_name}", conn);
                    adapter.Fill(dt);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dt;
        }

        public DataTable SelectAllNomalRecord(string table_name, string expression, string col_picture_name)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter($"SELECT {expression}, {col_picture_name} FROM {table_name}", conn);
                    adapter.Fill(dt);
                    dt.Columns.Add("pic", Type.GetType("System.Byte[]"));
                    foreach (DataRow row in dt.Rows)
                        row["pic"] = File.ReadAllBytes(row[col_picture_name].ToString());
                    dt.Columns.Remove(col_picture_name);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dt;
        }

        public DataTable SelectOneRowRecord(string table_name, string expression, string column_condition, string value)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter($"SELECT {expression} FROM {table_name} WHERE {column_condition} = {value}", conn);
                    adapter.Fill(dt);
                    dt.Columns.Add("pic", Type.GetType("System.Byte[]"));
                    foreach (DataRow row in dt.Rows)
                        row["pic"] = File.ReadAllBytes(row[""].ToString());
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dt;
        }

        public DataTable SelectOneRowRecord(string table_name, string expression, string col_picture_name, string column_condition, string value)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter($"SELECT {expression}, {col_picture_name} FROM {table_name} WHERE {column_condition} = {value}", conn);
                    adapter.Fill(dt);
                    dt.Columns.Add("pic", Type.GetType("System.Byte[]"));
                    foreach (DataRow row in dt.Rows)
                        row["pic"] = File.ReadAllBytes(row[col_picture_name].ToString());
                    dt.Columns.Remove(col_picture_name);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return dt;
        }


        /// THIS IS USE FOR UPDATE TO DATABASE

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5, {column_name_6} = @col_6 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5, {column_name_6} = @col_6, {column_name_7} = @col_7 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5, {column_name_6} = @col_6, {column_name_7} = @col_7, {column_name_8} = @col_9 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5, {column_name_6} = @col_6, {column_name_7} = @col_7, {column_name_8} = @col_8, {column_name_9} = @col_9 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }

        public void UpdateNomalRecord(string table_name, string column_condition, string exp_value, string column_name_1, object control_1, string column_name_2, object control_2, string column_name_3, object control_3, string column_name_4, object control_4, string column_name_5, object control_5, string column_name_6, object control_6, string column_name_7, object control_7, string column_name_8, object control_8, string column_name_9, object control_9, string column_name_10, object control_10)
        {
            using (SqlConnection conn = new SqlConnection(Connect))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {table_name} SET {column_name_1} = @col_1, {column_name_2} = @col_2, {column_name_3} = @col_3, {column_name_4} = @col_4, {column_name_5} = @col_5, {column_name_6} = @col_6, {column_name_7} = @col_7, {column_name_8} = @col_8, {column_name_9} = @col_9, {column_name_10} = @col_10 WHERE {column_condition} = {exp_value}", conn);
                    cmd.Parameters.AddWithValue("@col_1", control_1);
                    cmd.Parameters.AddWithValue("@col_2", control_2);
                    cmd.Parameters.AddWithValue("@col_3", control_3);
                    cmd.Parameters.AddWithValue("@col_4", control_4);
                    cmd.Parameters.AddWithValue("@col_5", control_5);
                    cmd.Parameters.AddWithValue("@col_6", control_6);
                    cmd.Parameters.AddWithValue("@col_7", control_7);
                    cmd.Parameters.AddWithValue("@col_8", control_8);
                    cmd.Parameters.AddWithValue("@col_9", control_9);
                    cmd.Parameters.AddWithValue("@col_10", control_10);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Update Success");
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return;
        }


        /// THIS IS USE FOR DELETE RECORD
        public void DeleteNomalRecord(string table_name, string column_name, int id_)
        {
            if (MessageBox.Show("Are you sure want to delete this record.", "Warning!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(Connect))
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand($"DELETE FROM {table_name} WHERE {column_name} = {id_}", conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else return;
        }

        public void DeleteWithPictureRecord(object data_grid_name, string table_name, string column_name, int id_)
        {
            if (MessageBox.Show("Are you sure want to delete this record.", "Warning!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(Connect))
                {
                    try
                    {

                        conn.Open();
                        SqlCommand cmd = new SqlCommand($"DELETE FROM {table_name} WHERE {column_name} = {id_}", conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else return;
        }



        /// THIS IS USE FOR 


        public string BrowseImage_Location()
        {
            string imgLocat = "";
            try
            {
                OpenFileDialog dailog = new OpenFileDialog();
                dailog.Filter = "PNG files(*.png)|*.png| jpg files(*.jpg)|*.jpg| All Files(*.*)|*.*";
                if (dailog.ShowDialog() == DialogResult.OK)
                {
                    imgLocat = dailog.FileName.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return imgLocat;
        }

        public string CopyImage_ToDestination(string desFolderName, string ImgLocation)
        {
            string imgDesPath = "", imgLocat = ImgLocation, imgName = Path.GetFileName(ImgLocation);

            if (!Directory.Exists(desFolderName))
                Directory.CreateDirectory(desFolderName);

            try
            {
                if (!string.IsNullOrEmpty(imgLocat))
                {
                    imgDesPath = Path.Combine(Application.StartupPath, desFolderName, DateTime.Now.ToString("dd-MM-yyyy") + DateTime.Now.ToString("HH-ss") + imgName);
                    File.Copy(imgLocat, imgDesPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return imgDesPath;
        }
    }
}
