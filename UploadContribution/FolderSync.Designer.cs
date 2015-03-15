namespace UploadContribution
{
    partial class FolderSync
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.folderBrowserDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textSourceFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSync = new System.Windows.Forms.Button();
            this.textDestinationFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_textTrace = new System.Windows.Forms.RichTextBox();
            this.getFolderListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 408);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(763, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getFolderListToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(763, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.buttonSync);
            this.splitContainer1.Panel1.Controls.Add(this.textDestinationFolder);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.buttonBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.textSourceFolder);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.m_textTrace);
            this.splitContainer1.Size = new System.Drawing.Size(763, 384);
            this.splitContainer1.SplitterDistance = 111;
            this.splitContainer1.TabIndex = 2;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(638, 13);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(63, 29);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textSourceFolder
            // 
            this.textSourceFolder.AllowDrop = true;
            this.textSourceFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSourceFolder.Location = new System.Drawing.Point(117, 18);
            this.textSourceFolder.Name = "textSourceFolder";
            this.textSourceFolder.Size = new System.Drawing.Size(489, 20);
            this.textSourceFolder.TabIndex = 1;
            this.textSourceFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.textSourceFolder_DragDrop);
            this.textSourceFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.textSourceFolder_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Folder";
            // 
            // buttonSync
            // 
            this.buttonSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSync.Location = new System.Drawing.Point(638, 64);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(75, 28);
            this.buttonSync.TabIndex = 7;
            this.buttonSync.Text = "SYNC";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // textDestinationFolder
            // 
            this.textDestinationFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textDestinationFolder.Location = new System.Drawing.Point(117, 57);
            this.textDestinationFolder.Name = "textDestinationFolder";
            this.textDestinationFolder.Size = new System.Drawing.Size(489, 20);
            this.textDestinationFolder.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Destination Folder";
            // 
            // m_textTrace
            // 
            this.m_textTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_textTrace.Location = new System.Drawing.Point(0, 0);
            this.m_textTrace.Name = "m_textTrace";
            this.m_textTrace.Size = new System.Drawing.Size(763, 269);
            this.m_textTrace.TabIndex = 5;
            this.m_textTrace.Text = "";
            // 
            // getFolderListToolStripMenuItem
            // 
            this.getFolderListToolStripMenuItem.Name = "getFolderListToolStripMenuItem";
            this.getFolderListToolStripMenuItem.Size = new System.Drawing.Size(141, 20);
            this.getFolderListToolStripMenuItem.Text = "Get  Remote Folder List";
            this.getFolderListToolStripMenuItem.ToolTipText = "Get Remote Folder List";
            this.getFolderListToolStripMenuItem.Click += new System.EventHandler(this.getFolderListToolStripMenuItem_Click);
            // 
            // FolderSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 430);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FolderSync";
            this.Text = "FolderSync";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDlg;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textSourceFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.TextBox textDestinationFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox m_textTrace;
        private System.Windows.Forms.ToolStripMenuItem getFolderListToolStripMenuItem;
    }
}