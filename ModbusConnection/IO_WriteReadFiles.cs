using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusConnection
{

    public class IO_WriteReadFiles
    {
        public static BlockQueue<WriteIOClassQueue> writeQueue = new BlockQueue<WriteIOClassQueue>();

        private static readonly object LockFile = new object();
        public IO_WriteReadFiles()
        {
            Task.Factory.StartNew(IOMethod, TaskCreationOptions.LongRunning);
        }

        private void IOMethod()
        {
            while (true)
            {
                SpinWait.SpinUntil(() => false, 5);
                var Info = writeQueue.DeQueue();
                WriteFiles(Info.Content, Info.Path);
            }
        }

        public static void CreateFolder(string path)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        public static void WriteFiles(string content, string path)
        {
            lock (LockFile)
            {
                var FolderName = path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                var FolderFileName = FolderName[0] + "\\" + FolderName[1] + "\\" + FolderName[2];
                if (!Directory.Exists(FolderFileName))
                {
                    Directory.CreateDirectory(FolderFileName);
                }
                var FileName = FolderFileName + "\\" + string.Format("{0} _ {1}", DateTime.Now.ToString("yyyyMMdd"), FolderName[3]);
                if (!File.Exists(FileName))
                {
                    using (FileStream fileStream = new FileStream(FileName, FileMode.Create))
                    {

                    }
                }

                using (var fs = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var log = new StreamWriter(fs))
                    {
                        log.WriteLine(content);
                    }
                }
            }
        }
    }
    public class WriteIOClassQueue
    {

        private string _content;

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        private string _Path;

        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }
    }
}
