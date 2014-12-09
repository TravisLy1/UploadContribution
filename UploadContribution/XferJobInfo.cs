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
        public delegate void TransferCompleteHandler(object sender);
        public event TransferCompleteHandler OnCompleted;
        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { 
                    startTime = value;
                    FileInfo fInfo = new FileInfo(this.Source);             // calculate size at start of transfer
                    fileSize = fInfo.Length;
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

       
        public static void XferFile(XferJobInfo xInfo)
        {
            string loginInfo = Program.LoginInfo;
            string sourcePath = xInfo.Source;
            string destinationPath = xInfo.Destination;
            bool upload = xInfo.Upload;

            xInfo.StartTime = DateTime.Now;
            xInfo.ReturnCode = Program.RunRSync(loginInfo, sourcePath, destinationPath, upload);
            xInfo.EndTime = DateTime.Now;
            xInfo.OnCompleted(xInfo);
           
        }
       
    }

    //class XferTask : Task
    //{
    //    public XferTask(Action act): Task(act)
    //    {
            
    //    }
    //    private XferJobInfo jobInfo;

    //    public XferJobInfo JobInfo
    //    {
    //        get { return jobInfo; }
    //        set { jobInfo = value; }
    //    }

    //}
}
