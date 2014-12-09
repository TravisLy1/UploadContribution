﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadContribution
{
    
    public partial class FormMain : Form
    {
        private string m_machineName;

        //private List<Task> m_tasks;
        private List<XferJobInfo> m_jobs;
        private List<XferJobInfo> m_jobQue;
        private IFileNameMapping m_fileMapping;

        private FileSystemWatcher m_watcher;
        public FormMain()
        {

            InitializeComponent();
            //m_tasks = new List<Task>();
            m_jobs = new List<XferJobInfo>();
            m_jobQue = new List<XferJobInfo>();
            m_fileMapping = new FileNameMapping();

            startWatching();
            updateStatus();
            tsLabelDestination.Text = Program.DestinationFolder;
            tsLabelWatchFolder.Text = Program.WatchFolder;
            m_machineName = Environment.MachineName;
            this.Text = m_machineName;

       }


    
        /// <summary>
        /// When a new file is created, copied into this folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_watcher_Created(object sender, FileSystemEventArgs e)
        {                      
            // Create destination by decoding the source File Name           
            string destPath = Program.DestinationFolder +  "/" +  m_fileMapping.CreateDestination(e.Name);
            
            // File may be started to created, but not completely copied
            XferJobInfo xInfo = new XferJobInfo(e.FullPath, destPath);
            lock (m_jobQue)
            {
                m_jobQue.Add(xInfo);
            }
            addLine("Add to Queue: " + e.FullPath, Color.DarkBlue);
            updateStatus();
               
            timerTransfer.Interval = 5000;
            timerTransfer.Start();
        }


        private void doTransfer(XferJobInfo xInfo)
        {
            if (IsLocked(xInfo.Source) )
                return;          // File is lockec cannot transfer
            lock (this)
            {
                m_jobQue.Remove(xInfo);   // Move from Que to Jobs
                if (!m_jobs.Contains(xInfo))
                    m_jobs.Add(xInfo);
            }
            addLine("Transfering: " + xInfo.Source + " to " + xInfo.Destination, Color.Blue);
            xInfo.OnCompleted += OnTransferCompleted;
           
            Action doUpload = () => XferJobInfo.XferFile(xInfo);
            Task t = new Task(doUpload);
            t.Start();
            updateStatus();
        }

       

        /// <summary>
        /// Transfer completed, could be success or failure
        /// </summary>
        /// <param name="sender"></param>
        void OnTransferCompleted(object sender)
        {
            XferJobInfo xInfo = (XferJobInfo)sender;
            lock (m_jobs)
            {
                m_jobs.Remove(xInfo);
                xInfo.OnCompleted -= OnTransferCompleted;           // removed handler , added on doTransfer
            }

            if (xInfo.ReturnCode == 0)
            {
                addLine(xInfo.Source + " transferred successfully", Color.DarkGreen);
                try
                {
                     // Delete file if return code is 0 
                    File.Delete(xInfo.Source);
                }
                catch (Exception ex)
                {
                    addLine(ex.Message, Color.Red);
                }

                // update the xferJobInfo associated with this file
                Program.TransferLog += xInfo.ToString();
                Program.TransferLog += "\r\n";

                // Complete transfer and nothing in the queue
                if ((m_jobs.Count == 0) && (m_jobQue.Count == 0))
                {
                    //get the tag file
                    addLine("Getting Tag File", Color.DarkGray);
                    Program.GetTagFile();

                    // upload tag file
                    addLine("Updating Tag File", Color.DarkGray);
                    Program.SendTagFile();          // Send Tag file
                }
            }
            else
            {
                // Failed,  file is not removed
                addLine(xInfo.Source + " Failed to upload", Color.Red);
                // Add back to todos
                //m_todoFiles.Add(xInfo.Source);
                if (xInfo.RetryCount < Program.TransferMaxRetries)
                {
                    lock (m_jobQue)
                    {
                        if (!m_jobQue.Contains(xInfo))
                            m_jobQue.Add(xInfo);
                    }
                    ++xInfo.RetryCount;
                    addLine("Re-Queing " + xInfo.Source + ", retry# " + xInfo.RetryCount.ToString(), Color.Orange);
                  
                }
                else
                {
                    addLine("Retries Exceeded on " + xInfo.Source);
                }
            }

            
            updateStatus();
        }


        private bool IsLocked(string fileName)
        {
            try
            {
                Stream s = new FileStream(fileName, FileMode.Open);
                s.Close();
                return false;
            }
            catch
            {
                return true;
            }
        }

       

        private void startWatching()
        {
            // start watch folder
            if (Directory.Exists(Program.WatchFolder))
            {
                // start watching
                m_watcher = new FileSystemWatcher(Program.WatchFolder);
                m_watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                        | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                m_watcher.Created += m_watcher_Created;
            
                m_watcher.EnableRaisingEvents = true;
                addLine("Monitor " + Program.WatchFolder);
                
            }
            else
            {
                addLine(Program.WatchFolder + " Does not exist!");
            }
        }

      
        private void tsButtonBrowse_Click(object sender, EventArgs e)
        {
            // Open dialog 
            if (openFolderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // set the folder
               Program.WatchFolder = openFolderDlg.SelectedPath;
               startWatching();
            }
        }

        private delegate void UpdateStatusCallBack();
        private void updateStatus()
        {
            if (InvokeRequired)
            {
                UpdateStatusCallBack u = new UpdateStatusCallBack(updateStatus);
                this.BeginInvoke(u);
            }
            else
            {
                tsTextTodos.Text = m_jobQue.Count.ToString(); // m_todoFiles.Count.ToString();
                tsTextTransferring.Text = m_jobs.Count.ToString();
            }
        }

        private delegate void AddLineCallBack(string Value, Color? c = null);
        private void addLine(string line, Color? c = null)
        {
            Color color = c ?? Color.Black;

            if (InvokeRequired)
            {
                AddLineCallBack d = new AddLineCallBack(addLine);
                this.BeginInvoke(d, line, color);
            }
            else
            {

                string msg = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString();
                msg = msg + " " + line + "\n";
                AppendTrace(m_textTrace, msg, color);
           
                Application.DoEvents();
            }
        }

        private void timerTransfer_Tick(object sender, EventArgs e)
        {
            // check to see if todos a
            if (m_jobQue.Count > 0)
            {                
                //string fileName = m_jobQue[0];
                doTransfer(m_jobQue[0]);
            }
            
        }

        /// <summary>
        /// timestamp the message and add to the textbox.
        /// To prevent runtime faults should the amount
        /// of data become too large, trim text when it reaches a certain size.
        /// </summary>
        /// <param name="text"></param>
        private void AppendTrace(RichTextBox rtb, string text, Color textcolor)
        {
            // keep textbox trimmed and avoid overflow
            // when kiosk has been running for awhile

            Int32 maxsize = 1024000;
            Int32 dropsize = maxsize / 4;

            if (rtb.Text.Length > maxsize)
            {
                // this method preserves the text colouring
                // find the first end-of-line past the endmarker

                Int32 endmarker = rtb.Text.IndexOf('\n', dropsize) + 1;
                if (endmarker < dropsize)
                    endmarker = dropsize;

                rtb.Select(0, endmarker);
                rtb.Cut();
            }

            try
            {
                // trap exception which occurs when processing
                // as application shutdown is occurring

                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = textcolor;
                rtb.AppendText(text);
                rtb.ScrollToCaret();
                  //System.DateTime.Now.ToString("HH:mm:ss.mmm") + " " + text);
            }
            catch (Exception)
            {
            }
        }
      
        private void tsTestText_TextChanged(object sender, EventArgs e)
        {
            // Convert the fileName
            addLine(tsTestText.Text + " -> " + m_fileMapping.CreateDestination(tsTestText.Text));   
        }
    }
}
