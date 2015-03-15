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
        public static int CheckTime = 100;
        private String[] files;
 
     


        public delegate void TransferCompleteHandler(object sender);
        public event TransferCompleteHandler OnCompleted;
        public delegate void TransferStatusHandler(object sender, String msg);
        public event TransferStatusHandler OnTransferStatus;

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
        public XferJobInfo(string src, string dest)
        {
            retryCount = 0;
            returnCode = -1;
            upload = true;
            fileSize = 0;
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
        public int GetFileCount()
        {
            if (SourceIsDirectory())
            {
                try
                {
                    files = Directory.GetFiles(this.Source, "*.*", SearchOption.AllDirectories);
                }
                catch (Exception)
                {

                }
               
                return files.Length;
                
            }
            else
                return 1;
        }
        /// <summary>
        /// Check if the number of files changed
        /// </summary>
        /// <returns></returns>
        public bool MoreFiles()
        {
            int lastCount = files.Length;
            int curCount = GetFileCount();
            return (lastCount < curCount);
        }

        DateTime[] previousWriteTimes;
        /// <summary>
        /// Wait for files to be completely copied
        /// </summary>
        public void WaitforFiles()
        {
            if (SourceIsDirectory())
            {
                // First time read the time
              
                bool done = false;
                //int lastFileCompleted = 0;
                
                while (!done)
                {
                    previousWriteTimes = new DateTime[files.Length];

                    // read the first set of times 
                    for (int i = 0; i < files.Length; i++)
                        previousWriteTimes[i] = System.IO.File.GetLastWriteTime(files[i]);
                    
                    // wait for a bit
                    System.Threading.Thread.Sleep(CheckTime);
                    // check again
                    // First time read the time
                    for (int i = 0; i < files.Length; i++)
                    {
                        DateTime lastWriteTime = System.IO.File.GetLastWriteTime(files[i]);
                       
                        //lastFileCompleted = i;  /// save the index
 
                        if (lastWriteTime != previousWriteTimes[i])
                        {
                            OnTransferStatus(this, "Wait for " + files[i]);
                           // wait for the file
                            WaitforFile(files[i]);
                        }
                    }
                   
                    // check to see if are different number of files since last time
                    if (!MoreFiles())
                        done = true;
                }

            }
            else
            {
                WaitforFile(this.source);
            }
        }


        /// <summary>
        /// Loop here and wait for the file
        /// </summary>
        /// <param name="path"></param>
        public void WaitforFile(string path)
        {
            bool fileReady = false;
          
            DateTime previousWriteTime = System.IO.File.GetLastWriteTime(path);
            while(!fileReady)
            {
                System.Threading.Thread.Sleep(CheckTime);
                DateTime currentWriteTime = System.IO.File.GetLastWriteTime(path);
                if (previousWriteTime != currentWriteTime)     // still changing
                {
                    previousWriteTime = currentWriteTime;
                    OnTransferStatus(this, "Waiting for " + path);
                    continue;
                }
                else
                {
                    fileReady = true;       // Done
                }
            }
        }

        /// <summary>
        /// Non blocking check if any all files are ready, copying is done
        /// </summary>
        /// <returns></returns>
        private bool filesAreReady()
        {
            bool isReady = false;
            bool fileModified = true;
            int readyCnt = 0;
            if (SourceIsDirectory())
            {
                previousWriteTimes = new DateTime[files.Length];
                // First time read the time
                for (int i = 0; i < files.Length; i++)
                {
                    previousWriteTimes[i] = System.IO.File.GetLastWriteTime(files[i]);
                }
                // wait for a bit
                System.Threading.Thread.Sleep(CheckTime);
                // check again
                // First time read the time
                fileModified = false;
                readyCnt = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    DateTime lastWriteTime = System.IO.File.GetLastWriteTime(files[i]);
                    fileModified = (lastWriteTime != previousWriteTimes[i]);
                    if (fileModified)
                    {
                        readyCnt = i;
                        break;
                    }
                }
                isReady = !fileModified;

            }
            else
            {

                DateTime lastWriteTime = System.IO.File.GetLastWriteTime(this.Source);
                System.Threading.Thread.Sleep(100);
            }

            OnTransferStatus(this, readyCnt.ToString() + " ready out of " + files.Length.ToString());
            return isReady;
        }


        /// <summary>
        /// Check if any of the source is locked
        /// </summary>
        /// <returns></returns>
        public bool SourceIsLocked()
        {
            // if directory, check all files
            if (SourceIsDirectory())
            {
                // get more files
                MoreFiles();
                foreach (string fileName in files)
                {
                    if (FileUtil.IsFileLocked(fileName, 10))
                        return true;
                }
            }
            else
            {
                //if (IsLocked(xInfo.Source) )
                //    return;          // File is lockec cannot transfer
                string ext = Path.GetExtension(this.Source);
                if (!String.IsNullOrEmpty(ext) && (string.Compare(ext, ".msi", true) == 0))
                {   // Check for lock on MSI file only, HTML should not have an issue
                    return (FileUtil.IsFileLocked(this.Source, 3));
                }
               
            }
            return false;       // None of the file is locked

        }

        /// <summary>
        /// transfer file is called in a separate thread to execute the rsync,  it may need to loop and wait for the file to finish copy
        /// 
        /// </summary>
        /// <param name="xInfo"></param>
        public static void XferFile(XferJobInfo xInfo)
        {
            string loginInfo = Program.Settings.LoginInfo;
            string sourcePath = xInfo.Source;
            string destinationPath = xInfo.Destination;
            bool upload = xInfo.Upload;

            // Wait for all files to be copied completely
            //xInfo.WaitforFiles();

            // This is to run asyc
            xInfo.StartTime = DateTime.Now;
            xInfo.ReturnCode = Program.RunRSync(loginInfo, sourcePath, destinationPath, upload);
            xInfo.ConsoleOutput = Program.RsyncResult;
            xInfo.EndTime = DateTime.Now;
            xInfo.OnCompleted(xInfo);
           
        }
       
    }

    
}
