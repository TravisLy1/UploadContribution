using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing;

namespace UploadContribution
{
    public interface FormReport
    {
        void addLine(string line, Color? c = null, bool? notime = false);
    }
    static class Program
    {
       // public static string LoginInfo;
        public static string WatchFolder;
        public static string DestinationFolder;
        public static string ProductFamily;
        public static string WorkingDir;
        public static string TransferLog;
        public static string LocalTagFile;
        public static FormReport Report;

        public static string RsyncResult;
        //public static FormMain mainForm;
        private static UploadContribution.Properties.Settings settings;

        public static UploadContribution.Properties.Settings Settings
        {
            get { return Program.settings; }
            set { Program.settings = value; }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {       

            // Get loginInfo from Applicaiton Setitings
            Settings = new UploadContribution.Properties.Settings();
            Settings.Reload();
            //LoginInfo = Settings.LoginInfo;
            // = Settings.TransferMaxRetries;
            Settings.Verbose = false;
            // Set up FTP attributes
            FtpXfer.FtpServerName = Settings.FtpServerName;
            FtpXfer.FtpUser = Settings.FtpUser;
            FtpXfer.FtpPwd = Settings.FtpPwd;
            FtpXfer.HostFingerPrint = Settings.HostFingerPrint;

            Form f;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           

            // 3 arguments go to Sync Page  
            if (args.Length > 2)
            {
                WatchFolder = args[0];
                DestinationFolder = args[1];
                f = new FolderSync();
                Program.Settings.Verbose = false;
            }
            else
            {
                // arg0 - watch folder
                // arg1 - destination folder
                if (args.Length < 2)
                {

                    // Usage message
                    string msg = string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name,
                                      "<monitor folder> <destination path> ");
                    MessageBox.Show(msg);
                    Environment.Exit(2);
                }
                WatchFolder = args[0];
                DestinationFolder = args[1];
                // exract the product family from the folder path 
                DirectoryInfo di = new DirectoryInfo(WatchFolder);
                ProductFamily = di.Name;
                Program.Settings.Verbose = true;
                f = new FormMain();
            }

            Report = (FormReport) f;
            Application.Run(f);
        }

        public static string GetFileOwner(string path)
        {
            string arg = string.Format("dir /q \"{0}\"", path);
            string s = CmdDir(arg);
            string owner = "";
            if (!String.IsNullOrEmpty(s))
            {
                Match match = Regex.Match(s, @"PROD\\([\S+]*)", RegexOptions.IgnoreCase);
                if (match.Success)
                    owner =  match.Groups[1].Value;
            }
            return owner;
        }

        /// <summary>
        /// Execute a CMD.exe 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CmdDir(string arg)
        {
            string result;
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            info.WorkingDirectory = Path.GetDirectoryName(uri.LocalPath);
            WorkingDir = info.WorkingDirectory;
            //info.Arguments = string.Format("/C dir /q {0}", fileName);
            info.Arguments = string.Format(" /C {0}", arg);
            
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(info);
            result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception(String.Format("CmdDir returns non zero exit code {0}", process.ExitCode));
           
            return result;
        }
        /// <summary>
        /// Get a CygPath necessary for executing the Rsync function.  Again assume all the necessary files are in the same place
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetCygPath(string path)
        {
            string cygPath = "";

            // excecute cygPath command and return the new path
            ProcessStartInfo info = new ProcessStartInfo("cygpath.exe");
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            info.WorkingDirectory = Path.GetDirectoryName(uri.LocalPath);
            WorkingDir = info.WorkingDirectory;
            info.Arguments = string.Format("{0}", path);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(info);
            cygPath = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception(String.Format("Cygpath return non zero exit code {0}", process.ExitCode));
            // get the console output
            if (cygPath.Length > 0)
            {   // Sometimes when the string is longer than the Standard output, a \n substitube a space at the last position of the would be screen.  Need to fix it up
                // This also take care of the \n char at the end of the Standard output.           
                cygPath = cygPath.Replace('\n', ' ');
                cygPath = cygPath.Trim();
            }
            return cygPath;
        }
        /// <summary>
        /// Create a temp file base on destination folder
        /// </summary>
        /// <returns></returns>
        private static string GetTagFileName(string fileName)
        {
            string tmpFile = DestinationFolder.Replace('/', '_');
            tmpFile = WorkingDir + "\\" + tmpFile + "_" + fileName;
            return tmpFile;
        }

        // make this verbose
        public static int GetTagFile()
        {
            int status = -1;
            int retry = Program.settings.TransferMaxRetries;
            //productDestinationPath = Program.DestinationFolder + "/Bundle/
            string remotetagFile = Program.DestinationFolder + "/Bundle/tag.txt";
            LocalTagFile = GetTagFileName("tag.txt");//WorkingDir + @"\tag.txt";
            while ((status != 0) && (retry-- > 0))
            {
                status = RunRSync(Program.Settings.LoginInfo, remotetagFile, LocalTagFile, false);
                Report.addLine(RsyncResult);
            }
            return status;
        }

        /// <summary>
        /// Send the tag file,  Make this verbose
        /// </summary>
        /// <returns></returns>
        public static int SendTagFile()
        {
            int status = -1;
            int retry = Program.settings.TransferMaxRetries;
            string remotetagFile = Program.DestinationFolder + "/Bundle/tag.txt";
            string localTagFile = GetTagFileName("tag.txt");
            // Need to add to the file
            // Append the Transfer Log
            File.AppendAllText(localTagFile, TransferLog);
            // Clear out the String
            Program.TransferLog = "";
            while ((status != 0) && (retry-- > 0))
            {
                status = RunRSync(Program.Settings.LoginInfo, localTagFile, remotetagFile, true);      // upload the file
                Report.addLine(RsyncResult);
            }
            return status;
        }

        /// <summary>
        ///  download the latest build file from the server for update
        ///  PackageNames.wxi is in the bundle folder 
        /// </summary>
        /// <returns> the full local filename</returns>
        public static string GetBuildFile()
        {
            int status = -1;
            int retry = Program.settings.TransferMaxRetries;
            string remoteFile = Program.DestinationFolder + "/Bundle/PackageNames.wxi";
            string localFile = GetTagFileName("PackageNames.wxi");

            while ((status != 0) && (retry-- > 0))
            {
                status =  RunRSync(Program.Settings.LoginInfo, remoteFile, localFile, false);
                Report.addLine(RsyncResult);
            }
            if (status == 0)
                return localFile;
            else
                return null;
        }

        /// <summary>
        /// Get a versionn File
        /// </summary>
        /// <returns></returns>
        public static string GetVersionFile()
        {
            string remoteFile = Program.DestinationFolder + "/Bundle/versionInfo.txt";
            string localFile = GetTagFileName("versionInfo.txt");
            return localFile;
        }

        /// <summary>
        /// Send the version File to remote, may be used for label
        /// </summary>
        /// <param name="localFile"></param>
        /// <returns></returns>
        public static int SendVersion(string localFile)
        {
            int status = -1;
            int retry = Program.settings.TransferMaxRetries;
            string remoteFile = Program.DestinationFolder + "/Bundle/versionInfo.txt";
            while ((status != 0) && (retry-- > 0))
            {
                status = RunRSync(Program.Settings.LoginInfo, localFile, remoteFile, true);      // upload the file
                Report.addLine(RsyncResult);
            }
            return status;
        }
        /// <summary>
        /// Save the build file back to the server
        /// </summary>
        /// <returns></returns>
        public static int SendBuildFile(string localFile)
        {
            int status = -1;
            int retry = Program.settings.TransferMaxRetries;
            string remoteFile = Program.DestinationFolder + "/Bundle/PackageNames.wxi";
            while ((status != 0) && (retry-- > 0))
            {
                status = RunRSync(Program.Settings.LoginInfo, localFile, remoteFile, true);      // upload the file
                Report.addLine(RsyncResult);
            }
            return status;
        }
    
    
        /// <summary>
        /// Run the Rsync process.  
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// 
        /// string loginInfo = string.Format("rsyncjob@skytapbuilddb.com");
        public static int RunRSync(string loginInfo, string sourcePath, string destinationPath, bool upload)
        {
            
            // Download
            // rsync -avz -e "ssh -i /cygdrive/c/sshKey/rsyncKey" rsyncjob@skytapbuilddb.com:/home/rsyncjob /tmp

            // Upload -remove-source-files (option)
            // rsync -avz  -r --progress -e "ssh -i /cygdrive/c/sshKey/rsyncKey" "/cygdrive/c/temp" "suite@fileserver.skytapbuilddb.local:/home/suite/temp2/"
            ProcessStartInfo info = new ProcessStartInfo("rsync.exe");
            RsyncResult = "";
            info.RedirectStandardOutput =  Program.Settings.Verbose;
                
            Uri uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            info.WorkingDirectory = Path.GetDirectoryName(uri.LocalPath);
           

            // Find the SSH path
            string sshPath = Path.GetDirectoryName(uri.LocalPath) + "\\ssh";
            string sshKeyPath = Path.GetDirectoryName(uri.LocalPath) + "\\rsynckey";
            if (!File.Exists(sshKeyPath))
                throw new Exception(String.Format("Invalid Keyfile. {0} is Missing", sshKeyPath));
            sshPath = GetCygPath(sshPath);
            sshKeyPath = GetCygPath(sshKeyPath);

            // SSH File should be in the same directory as the utility
            string sshInfo = string.Format("-e \"{0} -i {1}\"", sshPath, sshKeyPath);
            if (upload)
            {
                sourcePath = GetCygPath(sourcePath);   // source is local
                info.Arguments = string.Format("-vrz -hh --stats --protect-args --progress --perms --chmod=a+rwx {0} \"{1}\" {2}:\"{3}\" ",
                                         sshInfo, sourcePath, loginInfo, destinationPath);
            }
            else
            {
                destinationPath = GetCygPath(destinationPath);  // destination is local
                info.Arguments = string.Format("-vrz -hh --stats --protect-args --progress {0} {1}:\"{2}\" \"{3}\" ",
                                         sshInfo, loginInfo, sourcePath, destinationPath);
            }

           
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.UseShellExecute = false;

            Process process = Process.Start(info);
                     
            if (info.RedirectStandardOutput)
            {
                RsyncResult = process.StandardOutput.ReadToEnd();
            }
            process.WaitForExit();
            return process.ExitCode;
  
        }
  
    }
}
