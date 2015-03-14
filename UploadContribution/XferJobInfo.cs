using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace UploadContribution
{
    class XferJobInfo
    {
        private String[] files;

        public delegate void TransferCompleteHandler(object sender);
        public event TransferCompleteHandler OnCompleted;
        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { 
                    startTime = value;
                    fileSize = GetSourceSize();
                }
        }
        private DateTime endTime;

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        private long fileSize;

        public long FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }

        private string source;

        public string Source
        {
            get { return source; }
            set { source = value; }
        }
        private string destination;

        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }
        private bool upload;

        public bool Upload
        {
            get { return upload; }
            set { upload = value; }
        }
        private string consoleOutput;

        public string ConsoleOutput
        {
            get { return consoleOutput; }
            set { consoleOutput = value; }
        }

        private int returnCode;

        public int ReturnCode
        {
            get { return returnCode; }
            set { returnCode = value; }
        }

        private int retryCount;

        public int RetryCount
        {
            get { return retryCount; }
            set { retryCount = value; }
        }

        private string ownerEmail;

        public string OwnerEmail
        {
            get { return ownerEmail; }
            set { ownerEmail = value; }
        }

        public long GetSourceSize()
        {
            if (SourceIsDirectory())
                return GetDirectorySize(this.Source);
            else
                return (new FileInfo(this.Source)).Length ;             // calculate size at start of transfer
                
        }

        static long GetDirectorySize(string p)
        {
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);
            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }
        private string GetFileSize(double byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        public override string ToString()
        {
            long secs = 0;        
            long rateSec;        // rate per second
            string stringPerSec = "zero";
            TimeSpan span;

            try
            {
                span = this.endTime - this.startTime;
                secs = (long) span.TotalSeconds;
                rateSec = this.FileSize / secs;
                stringPerSec = GetFileSize(rateSec);                
            }
            catch (Exception)
            {
            }
            string details = string.Format("{0}, {1}, {2}, {3}, {4}, {5} secs, {6}/sec, {7}",
                                    this.startTime, this.source, this.destination, GetFileSize(this.FileSize), this.EndTime, secs, stringPerSec,Environment.MachineName);
            return details;
        }

        public XferJobInfo()
        {
            retryCount = 0;
            returnCode = -1;
            upload = true;
            fileSize = 0;
        }
        public XferJobInfo(string src, string dest):base()
        {
            Source = src;
            Destination = dest;

            Upload = true;
        }
        public bool SourceIsDirectory()
        {
            return IsDirectory(this.Source);
        }

        public static bool IsDirectory(string path)
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
        ///  remove the file or the folder
        /// </summary>
        public void ClearSource()
        {
            if (SourceIsDirectory())
            {
                DirectoryInfo di = new DirectoryInfo(this.Source);
                di.Delete(true);
            }
            else
            {
                File.Delete(this.Source);
            }
        }
        public bool SourceExists()
        {
            if (IsDirectory(Source))
                return Directory.Exists(Source);
            else
                return File.Exists(Source);
        }

        /// <summary>
        ///  Count all files in the folder
        /// </summary>
        /// <returns></returns>
        private int GetFiles()
        {
            files = Directory.GetFiles(this.Source, "*.*", SearchOption.AllDirectories);
            return files.Length;
        }
        /// <summary>
        /// Check if the number of files changed
        /// </summary>
        /// <returns></returns>
        public bool MoreFiles()
        {
            int lastCount = files.Length;
            int curCount = GetFiles();
            return (lastCount < curCount);
        }


        public static void XferFile(XferJobInfo xInfo)
        {
            string loginInfo = Program.Settings.LoginInfo;
            string sourcePath = xInfo.Source;
            string destinationPath = xInfo.Destination;
            bool upload = xInfo.Upload;

            xInfo.StartTime = DateTime.Now;
            xInfo.ReturnCode = Program.RunRSync(loginInfo, sourcePath, destinationPath, upload);
            xInfo.ConsoleOutput = Program.RsyncResult;
            xInfo.EndTime = DateTime.Now;
            xInfo.OnCompleted(xInfo);
           
        }
       
    }

    
}
