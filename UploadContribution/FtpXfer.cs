using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace UploadContribution
{
    class FtpXfer
    {
        private static string ftpServerName = "fileserver.skytapbuilddb.local";

        public static string FtpServerName
        {
            get { return FtpXfer.ftpServerName; }
            set { FtpXfer.ftpServerName = value; }
        }
        private static string ftpUser = "suite";

        public static string FtpUser
        {
            get { return FtpXfer.ftpUser; }
            set { FtpXfer.ftpUser = value; }
        }
        private static string ftpPwd = "suite";

        public static string FtpPwd
        {
            get { return FtpXfer.ftpPwd; }
            set { FtpXfer.ftpPwd = value; }
        }
        private static string hostFingerPrint = "ssh-rsa 2048 d1:d6:8f:38:b3:4b:e0:c2:40:2f:f1:67:bd:eb:21:57";

        public static string HostFingerPrint
        {
            get { return FtpXfer.hostFingerPrint; }
            set { FtpXfer.hostFingerPrint = value; }
        }

        private string lastStatus;

        public  string LastStatus
        {
            get { return lastStatus; }
            set { lastStatus = value; }
        }

        public List<String> GetFolderList(string folderPath)
        {
            List<String> folders = new List<string>();
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = FtpServerName, //hostname e.g. IP: 192.54.23.32, or mysftpsite.com
                UserName = FtpUser,
                Password = FtpPwd,
                SshHostKeyFingerprint = HostFingerPrint
            };
            try
            {
                using (Session session = new Session())
                {
                    session.Open(sessionOptions); //Attempts to connect to your sFtp site
                    RemoteDirectoryInfo rdInfo = session.ListDirectory(folderPath);
                    // Find the MSI File

                    foreach (RemoteFileInfo rfInfo in rdInfo.Files)
                    {
                        if (rfInfo.IsDirectory)
                        {
                            // Don't add . and ..
                            if ((rfInfo.Name !=".") && (rfInfo.Name !=".."))
                                folders.Add(rfInfo.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastStatus = ex.Message;
            }
            return folders;
        }
        /// <summary>
        /// Find the MSI file and rename it to the new file so 
        /// </summary>
        /// <param name="newFileName"></param>
        public void renameMSIFile(string folderPath, string newFileName)
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = FtpServerName, //hostname e.g. IP: 192.54.23.32, or mysftpsite.com
                UserName = FtpUser,
                Password = FtpPwd,
                SshHostKeyFingerprint = HostFingerPrint
            };
            try
            {
                using (Session session = new Session())
                {
                    session.Open(sessionOptions); //Attempts to connect to your sFtp site
                    RemoteDirectoryInfo rdInfo = session.ListDirectory(folderPath);
                    // Find the MSI File

                    foreach (RemoteFileInfo rfInfo in rdInfo.Files)
                    {
                        if (!rfInfo.IsDirectory)
                        {
                            // check for MSI extension
                            if (rfInfo.Name.ToLower().EndsWith(".msi"))
                            {
                                if (rfInfo.Name != newFileName)
                                { 
                                    // rename it to the new MSI Name
                                    string cmd = string.Format("mv {0} {1}", folderPath + "/" + rfInfo.Name, folderPath + "/" + newFileName);
                                    session.ExecuteCommand(cmd);
                                    LastStatus = "Renamed " + rfInfo.Name + " to " + newFileName;
                                }
                                break;              // Done!  Exit
                            }
                        }
                    }                      
                }
            }
            catch (Exception ex)
            {
                LastStatus= ex.Message;
            }
        }


        //public static void getFile(string source, string destination)
        //{
        //    SessionOptions sessionOptions = new SessionOptions
        //    {
        //        Protocol = Protocol.Sftp,
        //        HostName = FtpServerName, //hostname e.g. IP: 192.54.23.32, or mysftpsite.com
        //        UserName = FtpUser,
        //        Password = FtpPwd,
        //        SshHostKeyFingerprint = HostFingerPrint
        //    };
        //    //Get Files - ideally you would want to wrap this into a try...catch 
        //    //and possible execute more than once if you can't connect the first time. 
        //    try
        //    {

        //        using (Session session = new Session())
        //        {
        //            session.Open(sessionOptions); //Attempts to connect to your sFtp site
        //            RemoteDirectoryInfo rdInfo = session.ListDirectory("/mnt/share/Share/Xfer/");

        //            //Get Ftp File
        //            TransferOptions transferOptions = new TransferOptions();
        //            transferOptions.TransferMode = TransferMode.Binary; //The Transfer Mode - Automatic, Binary, or Ascii 
        //            //transferOptions.FilePermissions = null;  //Permissions applied to remote files; 
        //            //<em style="font-size: 9pt;">null for default permissions.  
        //            //Can set <em style="font-size: 9pt;">user, Group, or other Read/Write/Execute permissions.  
        //            transferOptions.PreserveTimestamp = false;  //Set last write time of destination file 
        //            //to that of source file - basically change the timestamp to match destination and source files.    
        //            //transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

        //            TransferOperationResult transferResult;
        //            //the parameter list is: remote Path, Local Path with filename 
        //            //(optional - if different from remote path), Delete source file?, transfer Options  
        //            // transferResult = session.GetFiles(source, destination, false, transferOptions);
        //            //Throw on any error 
        //            // transferResult.Check();
        //            //Log information and break out if necessary  
        //            string cmd = "mv /mnt/share/Share/Xfer/Oracle/Queue/userListIn2.txt /mnt/share/Share/Xfer/Oracle/Queue/userListIn3.txt";
        //            session.ExecuteCommand(cmd);

        //            source = @"c:\temp\userListIn2.txt";
        //            destination = @"/mnt/share/Share/Xfer/Oracle/Queue/";
        //            transferOptions.FilePermissions = new FilePermissions();
        //            transferOptions.FilePermissions.Octal = "777";
        //            transferResult = session.PutFiles(source, destination, false, transferOptions);
        //            transferResult.Check();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.Message;
        //    }
        //}
    }
}
