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

namespace UploadContribution
{
    static class Program
    {
        public static string LoginInfo;
        public static string WatchFolder;
        public static string DestinationFolder;
        public static string ProductFamily;
        public static string WorkingDir;
        public static string TransferLog;
        public static int TransferMaxRetries;
        public static string RsyncResult;
        private static UploadContribution.Properties.Settings m_settings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {       

            // Get loginInfo from Applicaiton Setitings
            m_settings = new UploadContribution.Properties.Settings();
            m_settings.Reload();
            LoginInfo = m_settings.LoginInfo;
            TransferMaxRetries = m_settings.TransferMaxRetries;
            m_settings.Verbose = false;
            // Set up FTP attributes
            FtpXfer.FtpServerName = m_settings.FtpServerName;
            FtpXfer.FtpUser = m_settings.FtpUser;
            FtpXfer.FtpPwd = m_settings.FtpPwd;
            FtpXfer.HostFingerPrint = m_settings.HostFingerPrint;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
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
            string remotetagFile = Program.DestinationFolder + "/tag.txt";
            string localTagFile = GetTagFileName("tag.txt");//WorkingDir + @"\tag.txt";
            m_settings.Verbose = true;
            if (RunRSync(Program.LoginInfo, remotetagFile, localTagFile, false) != 0)
            {
                // may file does not exist, create and empty file, when sendTag is called

            }
            m_settings.Verbose = false;
            return 0;      // download the file
        }

        /// <summary>
        /// Send the tag file,  Make this verbose
        /// </summary>
        /// <returns></returns>
        public static int SendTagFile()
        {
            string remotetagFile = Program.DestinationFolder + "/tag.txt";
            string localTagFile = GetTagFileName("tag.txt");
            // Need to add to the file
            // Append the Transfer Log
            File.AppendAllText(localTagFile, TransferLog);
            // Clear out the String
            Program.TransferLog = "";
            m_settings.Verbose = true;
            int status = RunRSync(Program.LoginInfo, localTagFile, remotetagFile, true);      // upload the file
            m_settings.Verbose = false;
            return status;
        }

        /// <summary>
        ///  download the latest build file from the server for update
        ///  PackageNames.wxi is in the bundle folder 
        /// </summary>
        /// <returns> the full local filename</returns>
        public static string GetBuildFile()
        {            
            string remoteFile = Program.DestinationFolder + "/PackageNames.wxi";
            string localFile = GetTagFileName("PackageNames.wxi");//WorkingDir + @"\tag.txt";
            m_settings.Verbose = true;
            if (RunRSync(Program.LoginInfo, remoteFile, localFile, false) != 0)
            {
                // may file does not exist, create and empty file, when sendTag is called

            }
            m_settings.Verbose = false;           
            return localFile;
        }

        /// <summary>
        /// Save the build file back to the server
        /// </summary>
        /// <returns></returns>
        public static int SendBuildFile(string localFile)
        {
            string remoteFile = Program.DestinationFolder + "/PackageNames.wxi";          
            m_settings.Verbose = true;
            int status = RunRSync(Program.LoginInfo, localFile, remoteFile, true);      // upload the file
            m_settings.Verbose = false;
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
            info.RedirectStandardOutput = m_settings.Verbose;
            
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
