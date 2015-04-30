using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace UploadContribution
{
    public partial class FolderSync : Form, FormReport
    {
        public FolderSync()
        {
            InitializeComponent();
            // fill up the text box
            textDestinationFolder.Text = Program.Settings.DestinationPath + "/";
            textSourceFolder.Text = Program.Settings.SourceFolder;

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDlg.ShowDialog() == DialogResult.OK)
            {
                textSourceFolder.Text = folderBrowserDlg.SelectedPath;
                Program.Settings.SourceFolder = folderBrowserDlg.SelectedPath;
                Program.Settings.Save();
            }
        }

        private void getFolderListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addLine("Retrieving Folder Information...", Color.Blue);
            FtpXfer ftp = new FtpXfer();

            FileNameMapping.FolderList = ftp.GetFolderList(textDestinationFolder.Text);
            FileNameMapping.FolderList.Sort();
            if (!String.IsNullOrEmpty(ftp.LastStatus))
                addLine("Ftp Error: " + ftp.LastStatus);
            foreach (string f in FileNameMapping.FolderList)
            {
                addLine("\t" + f, Color.Blue, true);
            }
            addLine("Remote Folder Information Updated", Color.Blue);
        }


        private delegate void AddLineCallBack(string Value, Color? c = null, bool? notime = false);
        public void addLine(string line, Color? c = null, bool? notime = false)
        {
            Color color = c ?? Color.Black;
            bool noTimestamp = notime ?? true;

            if (InvokeRequired)
            {
                AddLineCallBack d = new AddLineCallBack(addLine);
                this.BeginInvoke(d, line, color, noTimestamp);
            }
            else
            {
                string msg = " ";
                if (!noTimestamp)
                    msg = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString() + " ";
                msg = msg + line + "\n";
                FormMain.AppendTrace(m_textTrace, msg, color);

                Application.DoEvents();
            }
        }

        private void buttonSync_Click(object sender, EventArgs e)
        {
          

            // Sync the folders
            XferJobInfo xInfo = new XferJobInfo(textSourceFolder.Text, textDestinationFolder.Text);
            xInfo.OnCompleted += OnTransferCompleted;
            XferJobInfo.XferFile(xInfo);

          
        }
        /// <summary>
        /// Retrieve the tag file, add some info and upload it to trigger a new build
        /// </summary>
        private void UpdateRemoteTagFile()
        {
            //get the tag file
            addLine("Getting Tag File");
            try
            {
                if (Program.GetTagFile() != 0)
                {
                    addLine("ERROR - Getting Tag File", Color.Red);
                }
                else
                {
                    // upload tag file
                    addLine("Updating Tag File");
                    Program.SendTagFile();          // Send Tag file
                   
                }
            }
            catch (System.Exception ex)
            {
                addLine("Error UpdateRemoteTagFile. " + ex.Message, Color.Red);
            }
        }

        /// <summary>
        /// Transfer completed, could be success or failure
        /// </summary>
        /// <param name="sender"></param>
        void OnTransferCompleted(object sender)
        {
            XferJobInfo xInfo = (XferJobInfo)sender;
           
            if (xInfo.ReturnCode == 0)
            {
                if (!String.IsNullOrEmpty(xInfo.ConsoleOutput))
                    addLine(xInfo.ConsoleOutput);

                addLine(xInfo.Source + " uploaded successfully", Color.DarkGreen);
                
                // update the xferJobInfo associated with this file
                Program.TransferLog += xInfo.ToString();
                Program.TransferLog += "\r\n";

                addLine(xInfo.ToString());
                UpdateRemoteTagFile();
                              
            }
            else
            {
                // Failed,  file is not removed
                addLine(xInfo.Source + " Failed to upload", Color.Red);
                // Add back to todos
                //m_todoFiles.Add(xInfo.Source);
                if (xInfo.RetryCount < Program.Settings.TransferMaxRetries)
                {
                  
                    ++xInfo.RetryCount;
                    addLine("Re-Queing " + xInfo.Source + ", retry# " + xInfo.RetryCount.ToString(), Color.Orange);

                }
                else
                {
                    addLine("Retries Exceeded on " + xInfo.Source);
                    string body = "File " + xInfo.Source + " Failed to upload!" + "\r\n";
                    body = body + "\r\n" + Program.TransferLog;
                 
                }
            }
            
        }

        private void textSourceFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textSourceFolder_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (Directory.Exists(files[0]))
                textSourceFolder.Text = files[0];

        }

    }
}
