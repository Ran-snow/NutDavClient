namespace NutDavClient
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.tvFiles = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(313, 0);
            this.listView1.Name = "lvFileExplorer";
            this.listView1.Size = new System.Drawing.Size(660, 607);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(3, 628);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(970, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(93, 557);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 36);
            this.button1.TabIndex = 2;
            this.button1.Text = "Sync";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // tvFiles
            // 
            this.tvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvFiles.Location = new System.Drawing.Point(3, 0);
            this.tvFiles.Name = "tvFiles";
            this.tvFiles.Size = new System.Drawing.Size(304, 536);
            this.tvFiles.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 608);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Tip";
            // 
            // MainFrm
            // 
            this.ClientSize = new System.Drawing.Size(973, 655);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tvFiles);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.listView1);
            this.Name = "MainFrm";
            this.Text = "Nut Webdav";
            this.Load += new System.EventHandler(this.MainFrm_Load);

        }

        #endregion

        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TreeView tvFiles;
        private System.Windows.Forms.Label label1;
    }
}