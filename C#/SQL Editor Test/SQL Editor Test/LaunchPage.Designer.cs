namespace SQL_Editor_Test
{
    partial class launchPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableDGV = new System.Windows.Forms.DataGridView();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.serverBox = new System.Windows.Forms.TextBox();
            this.instanceBox = new System.Windows.Forms.TextBox();
            this.instanceLabel = new System.Windows.Forms.Label();
            this.tableList = new System.Windows.Forms.ListBox();
            this.getTablesBtn = new System.Windows.Forms.Button();
            this.updateBtn = new System.Windows.Forms.Button();
            this.passBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.userBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.databaseBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tableDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // tableDGV
            // 
            this.tableDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableDGV.Location = new System.Drawing.Point(138, 100);
            this.tableDGV.Name = "tableDGV";
            this.tableDGV.Size = new System.Drawing.Size(650, 368);
            this.tableDGV.TabIndex = 9;
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(9, 9);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(38, 13);
            this.ServerLabel.TabIndex = 1;
            this.ServerLabel.Text = "Server";
            // 
            // serverBox
            // 
            this.serverBox.Location = new System.Drawing.Point(12, 26);
            this.serverBox.Name = "serverBox";
            this.serverBox.Size = new System.Drawing.Size(136, 20);
            this.serverBox.TabIndex = 1;
            // 
            // instanceBox
            // 
            this.instanceBox.Location = new System.Drawing.Point(154, 26);
            this.instanceBox.Name = "instanceBox";
            this.instanceBox.Size = new System.Drawing.Size(189, 20);
            this.instanceBox.TabIndex = 2;
            // 
            // instanceLabel
            // 
            this.instanceLabel.AutoSize = true;
            this.instanceLabel.Location = new System.Drawing.Point(151, 9);
            this.instanceLabel.Name = "instanceLabel";
            this.instanceLabel.Size = new System.Drawing.Size(48, 13);
            this.instanceLabel.TabIndex = 3;
            this.instanceLabel.Text = "Instance";
            // 
            // tableList
            // 
            this.tableList.FormattingEnabled = true;
            this.tableList.Location = new System.Drawing.Point(12, 100);
            this.tableList.Name = "tableList";
            this.tableList.Size = new System.Drawing.Size(120, 368);
            this.tableList.TabIndex = 8;
            this.tableList.SelectedIndexChanged += new System.EventHandler(this.tableList_SelectedIndexChanged);
            // 
            // getTablesBtn
            // 
            this.getTablesBtn.Location = new System.Drawing.Point(527, 12);
            this.getTablesBtn.Name = "getTablesBtn";
            this.getTablesBtn.Size = new System.Drawing.Size(114, 42);
            this.getTablesBtn.TabIndex = 6;
            this.getTablesBtn.Text = "Get Tables";
            this.getTablesBtn.UseVisualStyleBackColor = true;
            this.getTablesBtn.Click += new System.EventHandler(this.getTablesBtn_Click);
            // 
            // updateBtn
            // 
            this.updateBtn.Location = new System.Drawing.Point(647, 12);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(141, 42);
            this.updateBtn.TabIndex = 7;
            this.updateBtn.Text = "Update Records";
            this.updateBtn.UseVisualStyleBackColor = true;
            this.updateBtn.Click += new System.EventHandler(this.updateBtn_Click);
            // 
            // passBox
            // 
            this.passBox.Location = new System.Drawing.Point(157, 66);
            this.passBox.Name = "passBox";
            this.passBox.Size = new System.Drawing.Size(189, 20);
            this.passBox.TabIndex = 5;
            this.passBox.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(154, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Password";
            // 
            // userBox
            // 
            this.userBox.Location = new System.Drawing.Point(15, 66);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(136, 20);
            this.userBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "User";
            // 
            // databaseBox
            // 
            this.databaseBox.Location = new System.Drawing.Point(349, 26);
            this.databaseBox.Name = "databaseBox";
            this.databaseBox.Size = new System.Drawing.Size(138, 20);
            this.databaseBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(346, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Database";
            // 
            // launchPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.databaseBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.passBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.updateBtn);
            this.Controls.Add(this.getTablesBtn);
            this.Controls.Add(this.tableList);
            this.Controls.Add(this.instanceBox);
            this.Controls.Add(this.instanceLabel);
            this.Controls.Add(this.serverBox);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.tableDGV);
            this.Name = "launchPage";
            this.Text = "SQL Editor Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tableDGV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView tableDGV;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.TextBox serverBox;
        private System.Windows.Forms.TextBox instanceBox;
        private System.Windows.Forms.Label instanceLabel;
        private System.Windows.Forms.ListBox tableList;
        private System.Windows.Forms.Button getTablesBtn;
        private System.Windows.Forms.Button updateBtn;
        private System.Windows.Forms.TextBox passBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox userBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox databaseBox;
        private System.Windows.Forms.Label label3;
    }
}

