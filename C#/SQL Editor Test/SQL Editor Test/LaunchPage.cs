using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SQL_Editor_Test
{
    public partial class launchPage : Form
    {
        public string servername { get; set; }
        public string instancename { get; set; }
        public string databasename { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        private SQLConnect sql = new SQLConnect();
        private SqlConnection conn { get; set; }
        private DataTable dt = new DataTable();
        private DataSet ds = new DataSet();
        SqlDataAdapter da;

        public launchPage()
        {
            InitializeComponent();
        }

        private void getTablesBtn_Click(object sender, EventArgs e)
        {
            servername = this.serverBox.Text;
            instancename = this.instanceBox.Text;
            databasename = this.databaseBox.Text;
            user = this.userBox.Text;
            pass = this.passBox.Text;

            conn = sql.SQLConnection(servername,instancename,databasename,user,pass);

            string query = "select * from INFORMATION_SCHEMA.TABLES";

            conn.Open();
            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                tableList.Items.Add(dr["TABLE_NAME"]);
            }
            conn.Close();
        }

        private void tableList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "Select * from " + tableList.SelectedItem;
            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;

            da = new SqlDataAdapter(cmd);
            conn.Open();
            da.Fill(ds,tableList.SelectedItem.ToString());
            tableDGV.DataSource = ds.Tables[tableList.SelectedItem.ToString()];
            conn.Close();
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure to save Changes", "Message", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            if (dr == DialogResult.Yes)
            {
                conn.Open();
                // da = new SqlDataAdapter(cmd);
                SqlCommandBuilder cmd = new SqlCommandBuilder(da);
                try
                {
                    da.Update(ds, tableList.SelectedItem.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                conn.Close();
                tableDGV.Refresh();
                MessageBox.Show("Record Updated");
            }
        }
    }
}
