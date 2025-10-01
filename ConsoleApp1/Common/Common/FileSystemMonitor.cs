using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class FileSystemMonitor
    {
        private readonly string _baseDir;
        private readonly string _filter;
        private FileSystemWatcher? _watcher;
        public FileSystemMonitor(string baseDir,string filter)
        {
            _baseDir = baseDir;
            _filter = filter;
        }

        public void Start()
        {
             _watcher = new(_baseDir)
            {
                Filter = _filter,
                EnableRaisingEvents = true
            };
            _watcher.Created += (sender, e) =>
            {
                FileSystemChanged?.Invoke(sender, e);
            };

            _watcher.Changed += (sender, e) =>
            {
                FileSystemChanged?.Invoke(sender, e);
            };
        }

        public event EventHandler<FileSystemEventArgs>? FileSystemChanged;

        public void Stop()
        {
            _watcher?.Dispose();
        }
    }
}
