using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.DirectoryServices;

namespace UploadContribution
{

    public partial class FormMain : Form, FormReport
    {
        private string m_machineName;

        //private List<Task> m_tasks;
        private List<XferJobInfo> m_jobs;
        private List<XferJobInfo> m_jobQue;
        private Dictionary<String, String> m_files;               // List of files to be added to build wxi
        private String  productDestinationPath;
        private FileSystemWatcher m_watcher;
        public FormMain()
        {
            InitializeComponent();

            m_jobs = new List<XferJobInfo>();
            m_jobQue = new List<XferJobInfo>();
            m_files = new Dictionary<String, String>();
 
            startWatching();   // watch in one folder
                     
            updateStatus("");
            tsLabelDestination.Text = Program.DestinationFolder;
            tsLabelWatchFolder.Text = Program.WatchFolder;
            m_machineName = Environment.MachineName;
            string ver = string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
            this.Text =  ver + " - " + m_machineName;

            productDestinationPath = Program.DestinationFolder + "/Data/Products/";
            getFolderListToolStripMenuItem_Click(null, new EventArgs());
            
       }

       
       

        /// <summary>
        /// Add more details to error Log
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private String buildEmailMessage (string dest, string source)
        {
            // send an error email
            string body = "Invalid Destination Path: " + dest + "\n" + "FileName = " + source + "\n\r";
            body = body + "This Error was caused by an improper file name format or invalid destination folder on the file server\n\r";
            // Add link to format page
            body = body + "For formatting rules please see " ;
            body = body + "https://toadjira.labs.dell.com/browse/CREQ-65\n\r";
            body = body + "\nValid Destination Folders are\n";
            // Add current directory
            foreach (string f in FileNameMapping.FolderList)
            {
                body = body + "\t" + f + "\n";
            }

            return body;

        }
        /// <summary>
        /// Add the file type to 
        /// </summary>
        /// <param name="e"></param>
        void addFileToQueue(FileSystemEventArgs e)
        {

            string destinationFolder = FileNameMapping.CreateDestination(e.Name);
            string validateFolder = FileNameMapping.VerifyDestinationPath(destinationFolder);

            string ownerEmail = FindOwnerEmail(Path.GetFullPath(e.FullPath));
            // Check the name of the file against the list of remote folders
            // Findout the owner of the file
            // Get the email address of the owner
            if (String.IsNullOrEmpty(validateFolder))
            {
                // send an error email
                string body = buildEmailMessage(destinationFolder, e.FullPath); // "Invalid Destination Path: " + destinationFolder + "\n\r" + "FileName = " + e.FullPath;
                
                string subject = "UploadContribution ERROR";
                //if (!String.IsNullOrEmpty(ownerEmail))
                sendMail(subject, body, ownerEmail);
                postErrorLog(subject, body, e.FullPath);
                addLine("ERROR: Invalid Destination Path: " + destinationFolder + "\n" + "FileName = " + e.FullPath, Color.Red);
            }
            else
            {
                // Create destination by decoding the source File Name           
                string destPath = productDestinationPath  + validateFolder;

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
            updateStatus("add to queue");

        }


        private FileSystemWatcher folderWatch;
        int changeCount = 0;
        /// <summary>
        /// Add the folder to the queue, but first make sure it stop changing
        /// </summary>
        /// <param name="e"></param>
        void addFolderToQueue(FileSystemEventArgs e)
        {
            string destPath = Program.DestinationFolder; // +"/" + e.Name;
            // File may be started to created, but not completely copied
            XferJobInfo xInfo = new XferJobInfo(e.FullPath, destPath);
            xInfo.OwnerEmail = FindOwnerEmail(Path.GetFullPath(e.FullPath));

            timerTransfer.Interval = 5000;      // 5 secs
          
            updateStatus("Add Folder to queue");

            folderWatch = new FileSystemWatcher(xInfo.Source);
            
            folderWatch.EnableRaisingEvents = true;
            folderWatch.NotifyFilter = NotifyFilters.LastWrite;
            folderWatch.IncludeSubdirectories = true;
            folderWatch.Changed += (sender, ex) =>
            {

                timerTransfer.Stop();
                timerTransfer.Enabled = false;
                
                lock (xInfo)
                {
                    // Get the time the file was modified
                    // Check it again in 100 ms
                    // When it has gone a while without modification, it's done.

                    changeCount++;
                    
                    if (!XferJobInfo.IsDirectory(ex.FullPath))
                    {
                            updateStatus(e.ChangeType.ToString() + " # of events = " + changeCount.ToString() + ", file count = " + xInfo.GetFileCount());
                            addLine(ex.FullPath);
                          
                    }
                }

                timerTransfer.Interval = 5000;      // 5 secs
                timerTransfer.Start();
                timerTransfer.Enabled = true;
              
            };

            
            
            // should wait for the file before start this



            lock (m_jobQue)
            {
                if (m_jobQue.Find(x => x.Source == xInfo.Source) == null)
                    m_jobQue.Add(xInfo);
            }
            addLine("Add to Queue: " + e.FullPath, Color.DarkBlue);

            timerTransfer.Interval = 5000;      // 5 secs
            timerTransfer.Start();
            updateStatus("Add Folder to queue");
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
            if (!XferJobInfo.IsDirectory(e.FullPath))
                  addFileToQueue(e);

        }

       
        /// <summary>
        /// Actual Transfer method, threaded
        /// </summary>
        /// <param name="xInfo"></param>
        private void doTransfer(XferJobInfo xInfo)
        {
            addLine(xInfo.Source  + ". File Count = " + xInfo.GetFileCount());
            if (xInfo.SourceIsDirectory())
            {
                if (xInfo.MoreFiles())      // Still copying files
                {
                    return;
                }
            }

            if (xInfo.SourceIsLocked())
            {
                addLine(xInfo.Source + " is locked");
                return;
            }
            lock(m_jobQue)
                m_jobQue.Remove(xInfo);   // Move from Que to Jobs


            if (xInfo.SourceExists())
            {
                if (!m_jobs.Contains(xInfo))
                        m_jobs.Add(xInfo);

                if (!xInfo.SourceIsDirectory())
                {
                    // before transferring, need to look at the destination
                    if (Path.GetExtension(xInfo.Source) == ".msi")
                    {
                        addLine("Renaming Remote File at " + xInfo.Destination);
                        FtpXfer ftp = new FtpXfer();
                        ftp.renameMSIFile(xInfo.Destination, Path.GetFileName(xInfo.Source));
                        addLine(ftp.LastStatus);
                    }
                }
                addLine("Transfering: " + xInfo.Source + " to " + xInfo.Destination, Color.Blue);
                xInfo.OnCompleted += OnTransferCompleted;
                xInfo.OnTransferStatus += xInfo_OnTransferStatus;
                Action doUpload = () => XferJobInfo.XferFile(xInfo);
                Task t = new Task(doUpload);
                t.Start();
                updateStatus("transferring...");
            }
            else
            {
                // Something happened,  Abort transfer
                addLine(xInfo.Source + " does not exist. May have been deleted...", Color.Red);
                return;
            }
            
            
        }

        void xInfo_OnTransferStatus(object sender, string msg)
        {
            addLine(msg);
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
                    if (s.Contains(key.ToUpper()) || s.Contains(key))
                    {
                        string oldValue = s.Split(new char[] { '"' })[1];
                        replacement = s.Replace(oldValue, m_files[key]);
                        addLine("Replaced " + oldValue + " with " + m_files[key]);
                        m_files.Remove(key);        // Once replace it, remove it from the list
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                addLine(ex.Message, Color.Red);
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
                string body = xInfo.Source + " uploaded successfully!" + "\r\n";
                sendMail("UploadContribution - File Uploaded Sucessfully", body, xInfo.OwnerEmail);
                if (!xInfo.SourceIsDirectory())
                {
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
                            m_files[destFolder] = Path.GetFileName(xInfo.Source);
                    }
                }


                try
                {
                    xInfo.ClearSource();
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
                    //string path = Program.GetVersionFile();
                    //// Send VersionInfo file
                    //File.WriteAllLines(path, m_files.Values, Encoding.UTF8);
                    //Program.SendVersion(path);

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
            updateStatus("transfer completed..");
        }

        private void updateVersionInfo(string path)
        {
           File.WriteAllLines(path, m_files.Values, Encoding.UTF8); 
        }    
        private void UpdateRemoteBuildFile()
        {
            try
            {
                // Now need to get the PackageNames.wxi
                addLine("Getting  Build File");
                string fileName = Program.GetBuildFile();
                //addLine(Program.RsyncResult);
                if (String.IsNullOrEmpty(fileName))
                {
                    // raise some errors
                    addLine("ERROR - Getting Build File.  Abort", Color.Red);
                }
                else
                {
                    // Dump the build file
                    dumpBuildFile(fileName);
                    // Process the file to add new Msi file into it
                    updateBuildFile(fileName);
                    dumpBuildFile(fileName);
                    // Upload Build File
                    addLine("Updating Build File");
                    Program.SendBuildFile(fileName);
                    //addLine(Program.RsyncResult);
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
                    //addLine(Program.RsyncResult);

                    // upload tag file
                    addLine("Updating Tag File");
                    Program.SendTagFile();          // Send Tag file
                    //addLine(Program.RsyncResult);
                }
            }
            catch (System.Exception ex)
            {
                addLine("Error UpdateRemoteTagFile. " + ex.Message, Color.Red);
            }
        }

        //void BlockingFileCopySync(FileInfo copyPath)
        //{
        //    bool ready = false;

        //    FileSystemWatcher watcher = new FileSystemWatcher();
        //    watcher.NotifyFilter = NotifyFilters.LastWrite;
        //    watcher.Path = copyPath.Directory.FullName;
        //    watcher.Filter = "*" + copyPath.Extension;
        //    watcher.EnableRaisingEvents = true;

        //    bool fileReady = false;
        //    bool firsttime = true;
        //    DateTime previousLastWriteTime = new DateTime();

        //    // modify this as you think you need to...
        //    int waitTimeMs = 100;

        //    watcher.Changed += (sender, e) =>
        //    {
        //        // Get the time the file was modified
        //        // Check it again in 100 ms
        //        // When it has gone a while without modification, it's done.
        //        while (!fileReady)
        //        {
        //            // We need to initialize for the "first time", 
        //            // ie. when the file was just created.
        //            // (Really, this could probably be initialized off the
        //            // time of the copy now that I'm thinking of it.)
        //            if (firsttime)
        //            {
        //                previousLastWriteTime = System.IO.File.GetLastWriteTime(copyPath.FullName);
        //                firsttime = false;
        //                System.Threading.Thread.Sleep(waitTimeMs);
        //                continue;
        //            }

        //            DateTime currentLastWriteTime = System.IO.File.GetLastWriteTime(copyPath.FullName);

        //            bool fileModified = (currentLastWriteTime != previousLastWriteTime);

        //            if (fileModified)
        //            {
        //                previousLastWriteTime = currentLastWriteTime;
        //                System.Threading.Thread.Sleep(waitTimeMs);
        //                continue;
        //            }
        //            else
        //            {
        //                fileReady = true;
        //                break;
        //            }
        //        }
        //    };

        
        //    // This guy here chills out until the filesystemwatcher 
        //    // tells him the file isn't being writen to anymore.
        //    while (!fileReady)
        //    {
        //        System.Threading.Thread.Sleep(waitTimeMs);
        //    }
        //}
        /// <summary>
        /// Open a file and catch the exception.
        /// If the file is not ready to be opened, it's still being copied.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //private bool IsLocked(string fileName)
        //{
        //    try
        //    {
        //        Stream s = new FileStream(fileName, FileMode.Open);
        //        s.Close();
                
        //        return false;
        //    }
        //    catch
        //    {
        //        // Findout who is locking
        //        List<Process> procs = FileUtil.WhoIsLocking(fileName);
        //        foreach (Process p in procs)
        //        {
        //            addLine(fileName + " locked by " + p.ToString() + " - " + p.Handle);
        //        }

        //        if (File.Exists(fileName))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            // Need to remove from Queue
        //            addLine("File " + fileName + " is missing!  Must have been removed or drop was cancelled.");
        //            return false;
        //        }

        //    }
        //}
        ///// <summary>
        /// Monitor any changes to the folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void folderContent_Changed(object sender, FileSystemEventArgs e)
        {
            // if folder changed, rsync the entire content.  But need to check the files that changed to see if they are accessible
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                

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
                addLine("Monitoring " + Program.WatchFolder + " for contributions...");
                
            }
            else
            {
                addLine(Program.WatchFolder + " Does not exist! Not monitoring, please check path and restart program");
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

        private delegate void UpdateStatusCallBack(string message);
        private void updateStatus(string message)
        {
            if (InvokeRequired)
            {
                UpdateStatusCallBack u = new UpdateStatusCallBack(updateStatus);
                this.BeginInvoke(u, message);
            }
            else
            {
                tsTextTodos.Text = m_jobQue.Count.ToString(); // m_todoFiles.Count.ToString();
                tsTextTransferring.Text = m_jobs.Count.ToString();
                toolStatusText.Text = message;
            }
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
                string msg =" ";
                if (!noTimestamp)
                    msg = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString() + " ";
                msg = msg + line + "\n";
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
        public static void AppendTrace(RichTextBox rtb, string text, Color textcolor)
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
                string stuff =  "\r\n"+ subject + "\r\n" + body;
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
                    message.From = new MailAddress(Program.Settings.DefaultSender);
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
           
            FileNameMapping.FolderList = ftp.GetFolderList(productDestinationPath);
            FileNameMapping.FolderList.Sort();
            if (!String.IsNullOrEmpty(ftp.LastStatus))
                addLine("Ftp Error: " + ftp.LastStatus);
            foreach (string f in FileNameMapping.FolderList)
            {
                addLine("\t" + f, Color.Blue, true);
            }
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

        private void tagFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.GetTagFile() == 0)
            { 
                int start=0;
                string[] readText = File.ReadAllLines(Program.LocalTagFile, Encoding.UTF8);
                if (readText.Length > 10)
                    start = readText.Length - 10;
                for (int i = start; i < readText.Length; i++)
                {
                    addLine(readText[i], Color.BlueViolet, true);
                }
            }

        }

        private void dumpBuildFile(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                // dump the file to the screen
                // Open the file, read all lines
                string[] readText = File.ReadAllLines(path, Encoding.UTF8);

                for (int i = 0; i < readText.Length; i++)
                {
                    addLine(readText[i], Color.BlueViolet, true);
                }
            }
        }

        private void buildFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Program.GetBuildFile();
            dumpBuildFile(path);
        }

        private void tsLabelDestination_Click(object sender, EventArgs e)
        {
           
        }

        private void validFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the folders from 
        }

        private void tsTestText_DoubleClick(object sender, EventArgs e)
        {
            string fileName = tsTestText.Text;
            addLine("Folder = " +  FileNameMapping.CreateDestination(fileName));
        }

        private void tsTestText_Click(object sender, EventArgs e)
        {

        }

       

     

    }
}
