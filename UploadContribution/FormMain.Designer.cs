namespace UploadContribution
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.getFolderListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendMailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsTestText = new System.Windows.Forms.ToolStripTextBox();
            this.checkDropFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsLabelWatchFolder = new System.Windows.Forms.ToolStripLabel();
            this.tsTextTodos = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tsTextTransferring = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tsLabelDestination = new System.Windows.Forms.ToolStripLabel();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.openFolderDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.timerTransfer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_textTrace = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getFolderListToolStripMenuItem,
            this.sendMailToolStripMenuItem,
            this.getToolStripMenuItem,
            this.tsTestText,
            this.checkDropFolderToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(678, 27);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // getFolderListToolStripMenuItem
            // 
            this.getFolderListToolStripMenuItem.Name = "getFolderListToolStripMenuItem";
            this.getFolderListToolStripMenuItem.Size = new System.Drawing.Size(158, 23);
            this.getFolderListToolStripMenuItem.Text = "Update Remote Folder List";
            this.getFolderListToolStripMenuItem.ToolTipText = "Get Remote Folder List";
            this.getFolderListToolStripMenuItem.Click += new System.EventHandler(this.getFolderListToolStripMenuItem_Click);
            // 
            // sendMailToolStripMenuItem
            // 
            this.sendMailToolStripMenuItem.Name = "sendMailToolStripMenuItem";
            this.sendMailToolStripMenuItem.Size = new System.Drawing.Size(68, 23);
            this.sendMailToolStripMenuItem.Text = "SendMail";
            this.sendMailToolStripMenuItem.Visible = false;
            this.sendMailToolStripMenuItem.Click += new System.EventHandler(this.sendMailToolStripMenuItem_Click);
            // 
            // getToolStripMenuItem
            // 
            this.getToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tagFileToolStripMenuItem,
            this.buildFileToolStripMenuItem});
            this.getToolStripMenuItem.Name = "getToolStripMenuItem";
            this.getToolStripMenuItem.Size = new System.Drawing.Size(37, 23);
            this.getToolStripMenuItem.Text = "Get";
            // 
            // tagFileToolStripMenuItem
            // 
            this.tagFileToolStripMenuItem.Name = "tagFileToolStripMenuItem";
            this.tagFileToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.tagFileToolStripMenuItem.Text = "Tag File";
            this.tagFileToolStripMenuItem.Click += new System.EventHandler(this.tagFileToolStripMenuItem_Click);
            // 
            // buildFileToolStripMenuItem
            // 
            this.buildFileToolStripMenuItem.Name = "buildFileToolStripMenuItem";
            this.buildFileToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.buildFileToolStripMenuItem.Text = "Build File";
            this.buildFileToolStripMenuItem.Click += new System.EventHandler(this.buildFileToolStripMenuItem_Click);
            // 
            // tsTestText
            // 
            this.tsTestText.Name = "tsTestText";
            this.tsTestText.Size = new System.Drawing.Size(200, 23);
            this.tsTestText.Text = "TestName";
            this.tsTestText.Click += new System.EventHandler(this.tsTestText_Click);
            this.tsTestText.DoubleClick += new System.EventHandler(this.tsTestText_DoubleClick);
            // 
            // checkDropFolderToolStripMenuItem
            // 
            this.checkDropFolderToolStripMenuItem.Name = "checkDropFolderToolStripMenuItem";
            this.checkDropFolderToolStripMenuItem.Size = new System.Drawing.Size(117, 23);
            this.checkDropFolderToolStripMenuItem.Text = "Check Drop Folder";
            this.checkDropFolderToolStripMenuItem.Click += new System.EventHandler(this.checkDropFolderToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.tsLabelWatchFolder,
            this.tsTextTodos,
            this.toolStripLabel1,
            this.tsTextTransferring,
            this.toolStripLabel2,
            this.tsLabelDestination});
            this.toolStrip1.Location = new System.Drawing.Point(0, 27);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(678, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsLabelWatchFolder
            // 
            this.tsLabelWatchFolder.Name = "tsLabelWatchFolder";
            this.tsLabelWatchFolder.Size = new System.Drawing.Size(86, 22);
            this.tsLabelWatchFolder.Text = "toolStripLabel3";
            // 
            // tsTextTodos
            // 
            this.tsTextTodos.Name = "tsTextTodos";
            this.tsTextTodos.ReadOnly = true;
            this.tsTextTodos.Size = new System.Drawing.Size(20, 25);
            this.tsTextTodos.Text = "0";
            this.tsTextTodos.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tsTextTodos.ToolTipText = "number of files in queue";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(55, 22);
            this.toolStripLabel1.Text = "In Queue";
            // 
            // tsTextTransferring
            // 
            this.tsTextTransferring.Name = "tsTextTransferring";
            this.tsTextTransferring.ReadOnly = true;
            this.tsTextTransferring.Size = new System.Drawing.Size(20, 25);
            this.tsTextTransferring.Text = "0";
            this.tsTextTransferring.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tsTextTransferring.ToolTipText = "number of files in transfer";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(63, 22);
            this.toolStripLabel2.Text = "In Transfer";
            // 
            // tsLabelDestination
            // 
            this.tsLabelDestination.Name = "tsLabelDestination";
            this.tsLabelDestination.Size = new System.Drawing.Size(86, 22);
            this.tsLabelDestination.Text = "toolStripLabel3";
            this.tsLabelDestination.Click += new System.EventHandler(this.tsLabelDestination_Click);
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "openFileDialog1";
            // 
            // timerTransfer
            // 
            this.timerTransfer.Enabled = true;
            this.timerTransfer.Interval = 5000;
            this.timerTransfer.Tick += new System.EventHandler(this.timerTransfer_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabel,
            this.toolStatusText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 455);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(678, 22);
            this.statusStrip1.TabIndex = 3;
            // 
            // tsLabel
            // 
            this.tsLabel.Name = "tsLabel";
            this.tsLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStatusText
            // 
            this.toolStatusText.Name = "toolStatusText";
            this.toolStatusText.Size = new System.Drawing.Size(13, 17);
            this.toolStatusText.Text = "..";
            // 
            // m_textTrace
            // 
            this.m_textTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_textTrace.Location = new System.Drawing.Point(0, 52);
            this.m_textTrace.Name = "m_textTrace";
            this.m_textTrace.Size = new System.Drawing.Size(678, 403);
            this.m_textTrace.TabIndex = 4;
            this.m_textTrace.Text = "";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 477);
            this.Controls.Add(this.m_textTrace);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.FolderBrowserDialog openFolderDlg;
        private System.Windows.Forms.Timer timerTransfer;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tsTextTodos;
        private System.Windows.Forms.ToolStripLabel tsLabelWatchFolder;
        private System.Windows.Forms.ToolStripTextBox tsTextTransferring;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel tsLabelDestination;
        private System.Windows.Forms.ToolStripTextBox tsTestText;
        private System.Windows.Forms.ToolStripMenuItem sendMailToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getFolderListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkDropFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tagFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildFileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.RichTextBox m_textTrace;
        private System.Windows.Forms.ToolStripStatusLabel tsLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStatusText;
    }
}