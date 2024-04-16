using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SQL_Editor_Test
{
    class SQLConnect
    {
        
        public SqlConnection SQLConnection(string server, string instance, string database, string user, string pass)
        {
            string connetionString = "Data Source=" + server + "\\" + instance + ";Initial Catalog=" + database + ";User ID=" + user + ";Password=" + pass + ";";
            SqlConnection sqlCon;

            sqlCon = new SqlConnection(connetionString);

            try
            {
                sqlCon.Open();
                //MessageBox.Show("Connection Open ! ");
                sqlCon.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! ");
            }

            return sqlCon;
        }
    }
}
