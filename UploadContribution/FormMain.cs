﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Security.AccessControl;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.DirectoryServices;

namespace UploadContribution
{
    
    public partial class FormMain : Form
    {
        private string m_machineName;

        //private List<Task> m_tasks;
        private List<XferJobInfo> m_jobs;
        private List<XferJobInfo> m_jobQue;
        private Dictionary<String, String> m_files;               // List of files to be added to build wxi
        

        private FileSystemWatcher m_watcher;
        public FormMain()
        {
            InitializeComponent();

            m_jobs = new List<XferJobInfo>();
            m_jobQue = new List<XferJobInfo>();
            m_files = new Dictionary<String, String>();
 
            startWatching();
            updateStatus();
            tsLabelDestination.Text = Program.DestinationFolder;
            tsLabelWatchFolder.Text = Program.WatchFolder;
            m_machineName = Environment.MachineName;
            string ver = string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
            this.Text =  ver + " - " + m_machineName;

            getFolderListToolStripMenuItem_Click(null, new EventArgs());
            
       }

        /// <summary>
        /// Validate the name
        /// </summary>
        /// <param name="xInfo"></param>
        /// <returns></returns>
        private bool ValidateFileNamingConvention(XferJobInfo xInfo)
        {
            string destinationFolder = FileNameMapping.CreateDestination(xInfo.Source);
            string validateFolder = FileNameMapping.VerifyDestinationPath(destinationFolder);
            string ownerEmail = FindOwnerEmail(Path.GetFullPath(xInfo.Source));
            // Check the name of the file against the list of remote folders
            // Findout the owner of the file
            // Get the email address of the owner
            if (String.IsNullOrEmpty(validateFolder))
            {
                // send an error email
                string body = "Invalid Destination Path: " + destinationFolder + "\n\r";
                body += "FileName = " + xInfo.Source;

                string subject = "UploadContribution ERROR";
                if (!String.IsNullOrEmpty(ownerEmail))
                    sendMail(subject, body, ownerEmail);
                postErrorLog("ERROR", body, xInfo.Source);   // Always
                addLine("ERROR: " + body, Color.Red);
                return false;
            }
            else
                return true;
        }

    
        /// <summary>
        /// When a new file is created, copied into this folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (Path.GetExtension(e.Name) == ".log")
                return;
            if (IsDirectory(e.FullPath))
                return;

            string destinationFolder =  FileNameMapping.CreateDestination(e.Name);
            string validateFolder = FileNameMapping.VerifyDestinationPath(destinationFolder);

            string ownerEmail =  FindOwnerEmail(Path.GetFullPath(e.FullPath));
            // Check the name of the file against the list of remote folders
            // Findout the owner of the file
            // Get the email address of the owner
            if (String.IsNullOrEmpty(validateFolder))
            {
                // send an error email
                string body = "Invalid Destination Path: " + destinationFolder + "\n\r" + "FileName = " + e.FullPath;
                string subject = "UploadContribution ERROR";
                //if (!String.IsNullOrEmpty(ownerEmail))
                sendMail(subject, body, ownerEmail);
                postErrorLog(subject, body, e.FullPath);
                addLine("ERROR: " + body, Color.Red);
            }
            else
            {
                // Create destination by decoding the source File Name           
                string destPath = Program.DestinationFolder + "/Products/" + validateFolder;

                // File may be started to created, but not completely copied
                XferJobInfo xInfo = new XferJobInfo(e.FullPath, destPath);
                xInfo.OwnerEmail = ownerEmail;          // save for later
                lock (m_jobQue)
                {
                    if (m_jobQue.Find(x => x.Source == xInfo.Source) == null)
                        m_jobQue.Add(xInfo);
                }
                addLine("Add to Queue: " + e.FullPath, Color.DarkBlue);
               
                timerTransfer.Interval = 5000;
                timerTransfer.Start();
            }
            updateStatus();
        }

        private static bool IsDirectory(string path)
        {
            System.IO.FileAttributes fa = System.IO.File.GetAttributes(path);
            bool isDirectory = false;
            if ((fa & FileAttributes.Directory) != 0)
            {
                isDirectory = true;
            }
            return isDirectory;
        }
        /// <summary>
        /// Actual Transfer method, threaded
        /// </summary>
        /// <param name="xInfo"></param>
        private void doTransfer(XferJobInfo xInfo)
        {
            if (IsLocked(xInfo.Source) )
                return;          // File is lockec cannot transfer
           
            lock(m_jobQue)
                m_jobQue.Remove(xInfo);   // Move from Que to Jobs
            if (File.Exists(xInfo.Source))
            {
                if (!m_jobs.Contains(xInfo))
                        m_jobs.Add(xInfo);
                // before transferring, need to look at the destination
                if (Path.GetExtension(xInfo.Source) == ".msi")
                {
                    FtpXfer ftp = new FtpXfer();
                    ftp.renameMSIFile(xInfo.Destination, Path.GetFileName(xInfo.Source));
                    addLine(ftp.LastStatus);
                }
                addLine("Transfering: " + xInfo.Source + " to " + xInfo.Destination, Color.Blue);
                xInfo.OnCompleted += OnTransferCompleted;

                Action doUpload = () => XferJobInfo.XferFile(xInfo);
                Task t = new Task(doUpload);
                t.Start();
                updateStatus();
            }
            else
            {
                // Something happened,  Abort transfer
                addLine(xInfo.Source + " does not exist", Color.Red);
                return;
            }
            
            
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
                if (readText[i].Contains("?define"))
                    readText[i] = replaceValue(readText[i]);
                // replaceValue remove items from m_files.
                if (m_files.Count == 0)
                    break;
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
                if (!String.IsNullOrEmpty(xInfo.ConsoleOutput))
                     addLine(xInfo.ConsoleOutput);

                addLine(xInfo.Source + " uploaded successfully", Color.DarkGreen);
                string body = "File " + xInfo.Source + " uploaded successfully!" + "\r\n";
                sendMail("UploadContribution - File Uploaded Sucessfully", body, xInfo.OwnerEmail);
                // Add to list of files to be used for update build file ONLY if it's an MSI file
                string ext = Path.GetExtension(xInfo.Source);
                if (!String.IsNullOrEmpty(ext) && (string.Compare(ext, ".msi", true) == 0))
                {
                    // keep track of the files to be added to the build file
                    string destFolder = FileNameMapping.CreateDestination(xInfo.Source);
                    destFolder = FileNameMapping.VerifyDestinationPath(destFolder);     // Get verified folder name
                    if (!m_files.ContainsKey(destFolder))
                        m_files.Add(destFolder, Path.GetFileName(xInfo.Source));
                    else
                        m_files[destFolder] = xInfo.Source;
                }
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
                    if (m_files.Count > 0)              /// optionally update the remote BUild file
                        UpdateRemoteBuildFile();
                    UpdateRemoteTagFile();
                }
            }
            else
            {
                // Failed,  file is not removed
                addLine(xInfo.Source + " Failed to upload", Color.Red);
                // Add back to todos
                //m_todoFiles.Add(xInfo.Source);
                if (xInfo.RetryCount < Program.Settings.TransferMaxRetries)
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
                    string body = "File " + xInfo.Source + " Failed to upload!" + "\r\n";
                    body = body + "\r\n" + Program.TransferLog;
                    sendMail("UploadContribution - File Uploaded FAILURE", body, xInfo.OwnerEmail);
                    postErrorLog("UploadContribution - File Uploaded FAILURE", body, xInfo.Source);
                }
            }          
            updateStatus();
        }

        
        private void UpdateRemoteBuildFile()
        {
            try
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
                }
            }
            catch (System.Exception ex)
            {
                addLine("Error UpdateRemoteBuildFile. " + ex.Message, Color.Red);
            }
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
                    addLine(Program.RsyncResult);

                    // upload tag file
                    addLine("Updating Tag File");
                    Program.SendTagFile();          // Send Tag file
                    addLine(Program.RsyncResult);
                }
            }
            catch (System.Exception ex)
            {
                addLine("Error UpdateRemoteTagFile. " + ex.Message, Color.Red);
            }
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
                if (File.Exists(fileName))
                {
                    return true;
                }
                else
                {
                    // Need to remove from Queue
                    addLine("File " + fileName + " is missing!  Must have been removed or drop was cancelled.");
                    return false;
                }

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
            addLine(tsTestText.Text + " -> " + FileNameMapping.CreateDestination(tsTestText.Text));
        }

        private void sendMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            string user = Program.GetFileOwner(@"C:\tag.txt");
            // Find the owner information
            //System.Security.AccessControl.FileSecurity fs = File.GetAccessControl(@"C:\tag.txt");
            //string user = fs.GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
            addLine("Owner is " + user);
            // Partse the username
            if (user.Contains('\\'))
            {
                int loc = user.LastIndexOf('\\');
                user = user.Substring(loc+1);
            }
            DirectoryEntry entry = new DirectoryEntry("LDAP://PROD");
            DirectorySearcher searcher = new DirectorySearcher(entry);

            searcher.Filter = "(&(objectClass=user)(samAccountName=" + user + "))";

            SearchResult sr = searcher.FindOne();

            if (sr != null)
            {
                ResultPropertyValueCollection rpc = sr.Properties["mail"];
                if (rpc != null && rpc.Count > 0) 
                {
                    string sendTo =  rpc[0].ToString();
                    sendMail("Testng", "Test Body", sendTo);
                }
            }
         }

        /// <summary>
        /// Find the owner's email 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string FindOwnerEmail(string fileName)
        {
            string emailAddr="";
            try
            {
                string user = Program.GetFileOwner(fileName);

                addLine("Owner: " + user);

                if (!String.IsNullOrEmpty(user))
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://PROD");
                    DirectorySearcher searcher = new DirectorySearcher(entry);
                    searcher.Filter = "(&(objectClass=user)(samAccountName=" + user + "))";

                    SearchResult sr = searcher.FindOne();

                    if (sr != null)
                    {
                        ResultPropertyValueCollection rpc = sr.Properties["mail"];
                        if (rpc != null && rpc.Count > 0)
                        {
                            emailAddr = rpc[0].ToString();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                addLine("FindOwnerEmail Exception: " + ex.Message);
            }
            
            return emailAddr;           
        }


        private void postErrorLog(string subject, string body, string FileName)
        {
            try
            {
                string logFileName = Path.GetFullPath(FileName) + "_error.log";
                string stuff = subject + "\r\n" + body;
                File.AppendAllText(logFileName, stuff);
            }
            catch (SystemException ex)
            {
                addLine("Failed writing error Log. " + ex.Message, Color.Red);
            }
        }
        /// <summary>
        /// Send email for notificaiton of success or failure when the user drops the file to the contribution folder
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="sendTo"></param>
        private void sendMail(string subject, string body, string sendTo)
        {
            try
            {
                // MailMessage is used to represent the e-mail being sent
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(Program.Settings.DefaultSender,"DO NOT REPLY");
                    message.Subject = subject;
                    message.Body = body;

                    if (!String.IsNullOrEmpty(sendTo))
                        message.To.Add(new MailAddress(sendTo));
                    else
                        message.To.Add(Program.Settings.DefaultSubscribers);
                    // SmtpClient is used to send the e-mail
                    SmtpClient mailClient = new SmtpClient(Program.Settings.MailServer);
                    mailClient.EnableSsl = false;
                    //mailClient.Credentials = new NetworkCredential("irvSwTest", "swTest");
                    // UseDefaultCredentials tells the mail client to use the 
                    // Windows credentials of the account (i.e. user account) 
                    // being used to run the application
                    mailClient.UseDefaultCredentials = true;
                    mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Send delivers the message to the mail server
                    mailClient.Send(message);
                    string s = message.To.ToString();
                    addLine("Email Sent to " + s);
                }
            }
            catch (FormatException ex)
            {
                //throw new System.Exception("SendMail Failed", ex);
                addLine("SendMail Failed: " + ex.Message);
            }
            catch (SmtpException ex)
            {
                //throw new System.Exception("SendMail Failed", ex);
                addLine("SendMail Failed: " + ex.Message);
            }
        }

        private void getFolderListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addLine("Retrieving Folder Information...", Color.Blue);
            FtpXfer ftp = new FtpXfer();
            string destPath = Program.DestinationFolder + "/Products/";
            FileNameMapping.FolderList = ftp.GetFolderList(destPath);
            addLine("Remote Folder Information Updated", Color.Blue);
        }

        private void checkDropFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check the files in the drop folder
            string [] Files = Directory.GetFiles(Program.WatchFolder);
            foreach (string f in Files)
            {
                if (Path.GetExtension(f) == ".log")
                    continue;
                string owner = Program.GetFileOwner(f);
                string email = FindOwnerEmail(f);
                string s = string.Format("{0}, owner={1}, email={2}", f, owner, email);
                addLine(s);
                FileSystemEventArgs fse = new FileSystemEventArgs(WatcherChangeTypes.Created, Program.WatchFolder, Path.GetFileName(f));
                m_watcher_Created(this, fse);              
            }
        }        

    }
}
