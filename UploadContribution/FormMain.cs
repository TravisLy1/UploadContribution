using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace UploadContribution
{
    
    public partial class FormMain : Form
    {
        private string m_machineName;

        //private List<Task> m_tasks;
        private List<XferJobInfo> m_jobs;
        private List<XferJobInfo> m_jobQue;
        private Dictionary<String, String> m_files;               // List of files to be added to build wxi
        private IFileNameMapping m_fileMapping;

        private FileSystemWatcher m_watcher;
        public FormMain()
        {
            InitializeComponent();

            m_jobs = new List<XferJobInfo>();
            m_jobQue = new List<XferJobInfo>();
            m_files = new Dictionary<String, String>();
            m_fileMapping = new SimpleNameMapping(); // FileNameMapping();

            startWatching();
            updateStatus();
            tsLabelDestination.Text = Program.DestinationFolder;
            tsLabelWatchFolder.Text = Program.WatchFolder;
            m_machineName = Environment.MachineName;
            string ver = string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
            this.Text =  ver + " - " + m_machineName;
       }


    
        /// <summary>
        /// When a new file is created, copied into this folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_watcher_Created(object sender, FileSystemEventArgs e)
        {                      
            // Create destination by decoding the source File Name           
            string destPath = Program.DestinationFolder +  "/Products/" +  m_fileMapping.CreateDestination(e.Name);
            
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
        /// Replace the value if the key match
        ///  example   <?define ToadForOracle_x64_EN_Package_Define="ToadForOracle_12.6_x64_EN.msi"?>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private  string replaceValue(string s)
        {
            string replacement = s;
            try
            {
                foreach (string key in m_files.Keys)
                {
                    if (s.Contains(key))
                    {
                        string oldValue = s.Split(new char[] { '"' })[1];
                        replacement = s.Replace(oldValue, m_files[key]);
                        m_files.Remove(key);        // Once replace it, remove it from the list
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return replacement;
        }
        /// <summary>
        /// Read the file and replace any matching key with new value
        /// </summary>
        /// <param name="path"></param>
        private void updateBuildFile(string path)
        {
            // Open the file, read all lines
            string[] readText = File.ReadAllLines(path, Encoding.UTF8);

            for (int i = 0; i < readText.Length; i++)
            {
                if (m_files.Count == 0)
                    break;
                if (readText[i].Contains("?define"))
                    readText[i] = replaceValue(readText[i]);
            }
            // overwrite the file
            File.WriteAllLines(path, readText, Encoding.UTF8);
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
                // Add to list of files to be used for update build file
                m_files.Add(m_fileMapping.CreateDestination(xInfo.Source), Path.GetFileName(xInfo.Source));  
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
                    // Now need to get the PackageNames.wxi
                    addLine("Getting  Build File");
                    string fileName = Program.GetBuildFile();
                    addLine(Program.RsyncResult);
                    if (String.IsNullOrEmpty(fileName))
                    {
                        // raise some errors
                        addLine("ERROR - Getting Build File.  Abort", Color.Red);
                    }
                    else
                    {
                        // Process the file to add new Msi file into it
                        updateBuildFile(fileName);
                        // Upload Build File
                        addLine("Updating Build File");
                        Program.SendBuildFile(fileName);
                        addLine(Program.RsyncResult);

                        //get the tag file
                        addLine("Getting Tag File");
                        Program.GetTagFile();
                        addLine(Program.RsyncResult);

                        // upload tag file
                        addLine("Updating Tag File");
                        Program.SendTagFile();          // Send Tag file
                        addLine(Program.RsyncResult);
                    }
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

        /// <summary>
        /// Open a file and catch the exception.
        /// If the file is not ready to be opened, it's still being copied.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
