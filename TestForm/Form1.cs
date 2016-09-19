using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MercuryGISData;
using System.Data.OleDb;

namespace TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private OleDbConnection conn;
        private OleDbCommand cmd;
        private OleDbDataReader dr;
        private string sqlStr = "";
        private DataSet myDataSet;
        private OleDbDataAdapter myAdapter;

        private void button1_Click(object sender, EventArgs e)
        {
            conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Users\\Dibel\\Documents\\Visual Studio 2015\\MercuryGIS\\;Extended Properties=DBASE III;");
            conn.Open();
            sqlStr = "Select * from STATES.DBF";
            //Make a DataSet object
            myDataSet = new DataSet();
            //Using the OleDbDataAdapter execute the query
            myAdapter = new OleDbDataAdapter(sqlStr, conn);
            //Build the Update and Delete SQL Statements
            OleDbCommandBuilder myBuilder = new OleDbCommandBuilder(myAdapter);


            //Fill the DataSet with the Table 'bookstock'
            myAdapter.Fill(myDataSet, "somename");
            // Get  a FileStream object
            // Use the WriteXml method of DataSet object to write XML file from the   DataSet
            //  myDs.WriteXml(myFs);
            conn.Close();
        }
    }
}
