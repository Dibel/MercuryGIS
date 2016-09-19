using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace MercuryGISData
{
    public class PropertyDataSet
    {
        public DataTable table;
        private string TableName = "Property";
        private SQLiteConnection conn;

        public void CreateDataset(string filename)
        {
            SQLiteConnection.CreateFile(filename);
            conn = new SQLiteConnection("Data Source=" + filename);
            conn.Open();
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("CREATE TABLE {0}(ID integer NOT NULL PRIMARY KEY)", TableName);
            cmd.ExecuteNonQuery();
            cmd.CommandText = string.Format("SELECT * FROM {0}", TableName);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
            adapter.AcceptChangesDuringFill = false;
            table = new DataTable();
            adapter.Fill(table);
            table.TableName = TableName;
            foreach (DataRow row in table.Rows)
            {
                row.AcceptChanges();
            }
        }

        public List<int> QueryByAttri(string query)
        {
            var found = table.Select(query);
            if (found.Length == 0)
            {
                return null;
            }
            List<int> idList = new List<int>();
            for (int i = 0; i < found.Length; i++)
            {
                idList.Add((int)found[i]["ID"]);
            }
            return idList;
        }

        public List<string> GetFields()
        {
            List<string> fields = new List<string>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                fields.Add(table.Columns[i].ColumnName);
            }
            return fields;
        }

        public bool ModifyData(int id, string field, object data)
        {
            if (!table.Columns.Contains(field))
            {
                return false;
            }
            var datarow = table.Select("ID=\'" + id.ToString() + "\'");
            if (datarow.Length == 0)
            {
                return false;
            }

            DataColumn column = table.Columns[field];
            datarow[0][column] = data;
            return true;
        }

        public DataRow QueryByID(int id)
        {
            var datarow = table.Select("ID=\'" + id.ToString() + "\'");
            if (datarow.Length == 0)
            {
                return null;
            }
            return datarow[0];
        }

        public bool AddRow(int id)
        {
            DataRow row = table.NewRow();
            row["ID"] = id;
            table.Rows.Add(row);
            return true;
        }

        public bool AddRow(DataRow row)
        {
            table.Rows.Add(row);
            return true;
        }

        public bool DeleteRow(int id)
        {
            var datarow = table.Select("ID=\'" + id.ToString() + "\'");
            if (datarow.Length == 0)
            {
                return false;
            }
            table.Rows.Remove(datarow[0]);
            return true;
        }

        public bool AddField(string field, Type type)
        {
            table.Columns.Add(field, type);
            return true;
        }

        public bool DeleteField(string field)
        {
            try
            {
                if (!table.Columns.Contains(field))
                {
                    return false;
                }
                table.Columns.Remove(field);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void New()
        {
            table = new DataTable();
            table.Columns.Add("ID", Type.GetType("System.Int32"));
        }

        public bool Open(string path)
        {
            try
            {
                conn = new SQLiteConnection("Data Source=" + path);
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM {0}", TableName);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                adapter.AcceptChangesDuringFill = false;
                table = new DataTable();
                adapter.Fill(table);
                table.TableName = TableName;
                foreach (DataRow row in table.Rows)
                {
                    row.AcceptChanges();
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool Save(string path)
        {
            try
            {
                conn = new SQLiteConnection("Data Source=" + path);
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT * FROM {0}", TableName);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                adapter.Update(table);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

    }
}
