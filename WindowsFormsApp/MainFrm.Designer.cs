namespace WindowsFormsApp
{
    partial class MainFrm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnSync = new System.Windows.Forms.Button();
            this.tvFiles = new System.Windows.Forms.TreeView();
            this.lvFileExplorer = new System.Windows.Forms.ListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnTest = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblTip = new System.Windows.Forms.Label();
            this.pbFile = new System.Windows.Forms.ProgressBar();
            this.lvTask = new System.Windows.Forms.ListView();
            this.t_ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.t_fileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.t_SyncStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_Ext = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_LastWriteTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.e_Length = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1149, 640);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage1.Size = new System.Drawing.Size(1141, 606);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Explorer";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 5);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvFiles);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvFileExplorer);
            this.splitContainer1.Size = new System.Drawing.Size(1131, 596);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnSync
            // 
            this.btnSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSync.Location = new System.Drawing.Point(4, 643);
            this.btnSync.Margin = new System.Windows.Forms.Padding(4);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(71, 35);
            this.btnSync.TabIndex = 1;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // tvFiles
            // 
            this.tvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvFiles.Location = new System.Drawing.Point(0, 0);
            this.tvFiles.Margin = new System.Windows.Forms.Padding(4);
            this.tvFiles.Name = "tvFiles";
            this.tvFiles.Size = new System.Drawing.Size(280, 596);
            this.tvFiles.TabIndex = 0;
            this.tvFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvFiles_NodeMouseClick);
            // 
            // lvFileExplorer
            // 
            this.lvFileExplorer.CheckBoxes = true;
            this.lvFileExplorer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.e_Name,
            this.e_Ext,
            this.e_LastWriteTime,
            this.e_Length});
            this.lvFileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFileExplorer.HideSelection = false;
            this.lvFileExplorer.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.lvFileExplorer.Location = new System.Drawing.Point(0, 0);
            this.lvFileExplorer.Margin = new System.Windows.Forms.Padding(4);
            this.lvFileExplorer.Name = "lvFileExplorer";
            this.lvFileExplorer.Size = new System.Drawing.Size(849, 596);
            this.lvFileExplorer.TabIndex = 1;
            this.lvFileExplorer.UseCompatibleStateImageBehavior = false;
            this.lvFileExplorer.View = System.Windows.Forms.View.Details;
            this.lvFileExplorer.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvFileExplorer_ColumnClick);
            this.lvFileExplorer.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvFileExplorer_ItemChecked);
            this.lvFileExplorer.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvFileExplorer_ItemSelectionChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lvTask);
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage2.Size = new System.Drawing.Size(1141, 606);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Detail";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(386, 398);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 32);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "button1";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnTest);
            this.tabPage3.Location = new System.Drawing.Point(4, 30);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1141, 606);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Setting";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(142, 650);
            this.lblTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(55, 21);
            this.lblTime.TabIndex = 8;
            this.lblTime.Text = "label1";
            // 
            // lblSpeed
            // 
            this.lblSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(205, 650);
            this.lblSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(55, 21);
            this.lblSpeed.TabIndex = 7;
            this.lblSpeed.Text = "label1";
            // 
            // lblTip
            // 
            this.lblTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTip.AutoSize = true;
            this.lblTip.Location = new System.Drawing.Point(79, 650);
            this.lblTip.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(55, 21);
            this.lblTip.TabIndex = 6;
            this.lblTip.Text = "label1";
            // 
            // pbFile
            // 
            this.pbFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFile.Location = new System.Drawing.Point(293, 645);
            this.pbFile.Margin = new System.Windows.Forms.Padding(4);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(855, 30);
            this.pbFile.TabIndex = 5;
            // 
            // lvTask
            // 
            this.lvTask.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.t_ID,
            this.t_fileName,
            this.t_SyncStatus});
            this.lvTask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTask.FullRowSelect = true;
            this.lvTask.HideSelection = false;
            this.lvTask.Location = new System.Drawing.Point(5, 5);
            this.lvTask.Name = "lvTask";
            this.lvTask.Size = new System.Drawing.Size(1131, 596);
            this.lvTask.TabIndex = 0;
            this.lvTask.UseCompatibleStateImageBehavior = false;
            this.lvTask.View = System.Windows.Forms.View.Details;
            // 
            // t_ID
            // 
            this.t_ID.Text = "ID";
            // 
            // t_fileName
            // 
            this.t_fileName.Text = "FileName";
            // 
            // t_SyncStatus
            // 
            this.t_SyncStatus.Text = "Status";
            // 
            // e_Name
            // 
            this.e_Name.Text = "Name";
            // 
            // e_Ext
            // 
            this.e_Ext.Text = "Ext";
            this.e_Ext.Width = 40;
            // 
            // e_LastWriteTime
            // 
            this.e_LastWriteTime.Text = "LastWriteTime";
            this.e_LastWriteTime.Width = 120;
            // 
            // e_Length
            // 
            this.e_Length.Text = "Length";
            this.e_Length.Width = 75;
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1149, 681);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.pbFile);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "MainFrm";
            this.ShowIcon = false;
            this.Text = "FilesExplorer";
            this.Load += new System.EventHandler(this.MainFrm_Load);
            this.SizeChanged += new System.EventHandler(this.MainFrm_SizeChanged);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvFiles;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.ListView lvFileExplorer;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.ProgressBar pbFile;
        private System.Windows.Forms.ListView lvTask;
        private System.Windows.Forms.ColumnHeader t_ID;
        private System.Windows.Forms.ColumnHeader t_fileName;
        private System.Windows.Forms.ColumnHeader t_SyncStatus;
        private System.Windows.Forms.ColumnHeader e_Name;
        private System.Windows.Forms.ColumnHeader e_Ext;
        private System.Windows.Forms.ColumnHeader e_LastWriteTime;
        private System.Windows.Forms.ColumnHeader e_Length;
    }
}