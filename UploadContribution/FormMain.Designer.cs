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
            this.tsTestText = new System.Windows.Forms.ToolStripTextBox();
            this.sendMailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsLabelWatchFolder = new System.Windows.Forms.ToolStripLabel();
            this.tsTextTodos = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tsTextTransferring = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tsLabelDestination = new System.Windows.Forms.ToolStripLabel();
            this.m_textTrace = new System.Windows.Forms.RichTextBox();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.openFolderDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.timerTransfer = new System.Windows.Forms.Timer(this.components);
            this.getFolderListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getFolderListToolStripMenuItem,
            this.sendMailToolStripMenuItem,
            this.tsTestText});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(678, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsTestText
            // 
            this.tsTestText.Name = "tsTestText";
            this.tsTestText.Size = new System.Drawing.Size(400, 23);
            this.tsTestText.Text = "TestName";
            this.tsTestText.Visible = false;
            this.tsTestText.TextChanged += new System.EventHandler(this.tsTestText_TextChanged);
            // 
            // sendMailToolStripMenuItem
            // 
            this.sendMailToolStripMenuItem.Name = "sendMailToolStripMenuItem";
            this.sendMailToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.sendMailToolStripMenuItem.Text = "SendMail";
            this.sendMailToolStripMenuItem.Click += new System.EventHandler(this.sendMailToolStripMenuItem_Click);
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
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
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
            // 
            // m_textTrace
            // 
            this.m_textTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_textTrace.Location = new System.Drawing.Point(0, 49);
            this.m_textTrace.Name = "m_textTrace";
            this.m_textTrace.Size = new System.Drawing.Size(678, 428);
            this.m_textTrace.TabIndex = 2;
            this.m_textTrace.Text = "";
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
            // getFolderListToolStripMenuItem
            // 
            this.getFolderListToolStripMenuItem.Name = "getFolderListToolStripMenuItem";
            this.getFolderListToolStripMenuItem.Size = new System.Drawing.Size(158, 20);
            this.getFolderListToolStripMenuItem.Text = "Update Remote Folder List";
            this.getFolderListToolStripMenuItem.ToolTipText = "Get Remote Folder List";
            this.getFolderListToolStripMenuItem.Click += new System.EventHandler(this.getFolderListToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 477);
            this.Controls.Add(this.m_textTrace);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.RichTextBox m_textTrace;
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
    }
}