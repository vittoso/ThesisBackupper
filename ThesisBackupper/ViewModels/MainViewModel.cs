using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ThesisBackupper.ViewModels
{
    public class MainViewModel : Screen
    {
        public MainViewModel()
        {
            this.DisplayName = Properties.Resources.AppName;
            this.CurrentProgressPerc = 0;
            this.LocalFilesLocation = @"C:\Users\Vittorio\Desktop\Blogracy\tesi";
            this.DropBoxFilesLocation = @"C:\Users\Vittorio\Dropbox\Tesi";
            this.DropBoxExeLocation = @"C:\Users\Vittorio\AppData\Roaming\Dropbox\bin\Dropbox.exe";
        }

        public override string DisplayName
        {
            get;
            set;
        }

        private string localFilesLocation;

        public string LocalFilesLocation
        {
            get { return localFilesLocation; }
            set
            {
                localFilesLocation = value;
                NotifyOfPropertyChange(() => LocalFilesLocation);
                NotifyOfPropertyChange(() => CanStartBackup);
            }
        }

        private string dropBoxFilesLocation;

        public string DropBoxFilesLocation
        {
            get { return dropBoxFilesLocation; }
            set
            {
                dropBoxFilesLocation = value;
                NotifyOfPropertyChange(() => DropBoxFilesLocation);
                NotifyOfPropertyChange(() => CanStartBackup);
            }
        }


        private string dropBoxExeLocation;

        public string DropBoxExeLocation
        {
            get { return dropBoxExeLocation; }
            set
            {
                dropBoxExeLocation = value;
                NotifyOfPropertyChange(() => DropBoxExeLocation);
                NotifyOfPropertyChange(() => CanStartBackup);
            }
        }




        private int currentProgressPerc;

        public int CurrentProgressPerc
        {
            get { return currentProgressPerc; }
            set
            {
                currentProgressPerc = value;
                NotifyOfPropertyChange(() => CurrentProgressPerc);
            }
        }


        public void OpenDropboxApplication()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = this.DropBoxExeLocation;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var runningProcessByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(startInfo.FileName));
            if (!runningProcessByName.Any())
            {
                using (Process proc = Process.Start(startInfo))
                {
                    proc.WaitForInputIdle();
                }
            }
        }

        public async void StartBackup()
        {
            try
            {
                OpenDropboxApplication();

                await Task.Run(() =>
                 {
                     int total = CountFiles(this.LocalFilesLocation, this.DropBoxFilesLocation);
                     int files = 0;
                     CopyStructure(this.LocalFilesLocation, this.DropBoxFilesLocation, () =>
                     {
                         this.CurrentProgressPerc = (++files / total);
                     });
                 });

                MessageBox.Show(Properties.Resources.lblBackupOK, Properties.Resources.lblInformation, MessageBoxButton.OK, MessageBoxImage.Information);
                this.CurrentProgressPerc = 0;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, Properties.Resources.lblError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CountFiles(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            int count = 0;
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (!File.Exists(Path.Combine(targetDir, Path.GetFileName(file))))
                    ++count;
                else if (File.GetLastWriteTime(file).CompareTo(File.GetLastWriteTime(Path.Combine(targetDir, Path.GetFileName(file)))) > 0)
                    ++count;
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
                count += CountFiles(directory, Path.Combine(targetDir, Path.GetFileName(directory)));

            return count;
        }

        public bool CanStartBackup
        {
            get
            {
                if (!Directory.Exists(this.LocalFilesLocation))
                    return false;

                if (!Directory.Exists(this.DropBoxFilesLocation))
                    return false;

                if (!File.Exists(this.DropBoxExeLocation))
                    return false;

                return true;
            }
        }


        void CopyStructure(string sourceDir, string targetDir, System.Action notifyCopy)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (!File.Exists(Path.Combine(targetDir, Path.GetFileName(file))))
                {
                    File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
                    if (notifyCopy != null)
                        notifyCopy.Invoke();
                }
                else if (File.GetLastWriteTime(file).CompareTo(File.GetLastWriteTime(Path.Combine(targetDir, Path.GetFileName(file)))) > 0)
                {
                    File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
                    if (notifyCopy != null)
                        notifyCopy.Invoke();
                }

            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyStructure(directory, Path.Combine(targetDir, Path.GetFileName(directory)), notifyCopy);
        }
    }
}
