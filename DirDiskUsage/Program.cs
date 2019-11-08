using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Linq;

namespace DirDiskUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Displays sizes and percentages of disk space dirs and files in given directory take up (relative to total size of the specified directory).");
            string rootDir;
            if (args.Length == 0)
            {
                Console.WriteLine("Using current dir {0} since no parameter was specified.", Environment.CurrentDirectory);
                rootDir = Environment.CurrentDirectory;
            }
            else
            {
                rootDir = args[0];
            }
            DirectoryInfo di = new DirectoryInfo(rootDir);

            if (!di.Exists) return;
            FileSystemInfo[] fis = di.GetFileSystemInfos();
            #region Create a uniform array of items in this directory
            DirEntryInfo[] entries = new DirEntryInfo[fis.Length];
            int iterator = 0;

            foreach (FileSystemInfo fi in fis)
            {
                if ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    DirEntryInfo dei = new DirectoryEntryInfo();
                    dei.setItem(fi);
                    entries[iterator] = dei;
                }
                else
                {
                    DirEntryInfo dei = new FileEntryInfo();
                    dei.setItem(fi);
                    entries[iterator] = dei;
                }
                iterator++;
            }
            #endregion
            #region Calculate the total dir size
            long totalSize = entries.Sum(entry => entry.getSize());
            Console.WriteLine("Total size of this dir: {0} bytes.", totalSize);
            #endregion
            #region Display the output data
            int maxNameLength = entries.OrderByDescending(entry => entry.getName().Length).FirstOrDefault().getName().Length;
            //String that we want to get is: "{0,-<maxNameLength>} ==> {1,-10} ({2}%)"
            string entryFormat = string.Format("{0}{1}{2}", "{0,-", maxNameLength, "} ==> {1,-10} ({2}%)");
            if (totalSize == 0) totalSize = 1; //Avoid division by 0 error if total is 0 bytes.
            entries.ToList().ForEach(entry => Console.WriteLine(entryFormat, entry.getName(), entry.getSize().ToString("d", System.Globalization.CultureInfo.CurrentCulture), ((entry.getSize() * 100) / totalSize), maxNameLength));
            #endregion
        }
    }
    interface DirEntryInfo
    {
        void setItem(FileSystemInfo fsi);
        string getName();
        long getSize();
    }
    class DirectoryEntryInfo : DirEntryInfo
    {
        DirectoryInfo dirItem;
        public void setItem(FileSystemInfo fsi)
        {
            dirItem = (DirectoryInfo)fsi;
        }
        public string getName()
        {
            return dirItem.Name;
        }
        public long getSize()
        {
            long totalSize = 0;
            //Easy pass on all files. Non-recursive.
            totalSize += dirItem.GetFiles().Sum(file => file.Length);
            //More complex pass on all subdirs - recursive.
            dirItem.GetDirectories().ToList().ForEach(dir =>
            {
                DirEntryInfo dei = new DirectoryEntryInfo();
                dei.setItem(dir);
                totalSize += dei.getSize(); //Recursive here.
            });
            return totalSize;
        }
    }
    class FileEntryInfo : DirEntryInfo
    {
        FileInfo dirItem;
        public void setItem(FileSystemInfo fsi)
        {
            dirItem = (FileInfo)fsi;
        }
        public string getName()
        {
            return dirItem.Name;
        }
        public long getSize()
        {
            return dirItem.Length;
        }
    }
}
