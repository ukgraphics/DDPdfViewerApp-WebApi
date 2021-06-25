using System;
using System.IO;
using System.Threading.Tasks;

namespace SupportApi.Collaboration.Storages
{
    public class FileSystemStorage : ICollaborationStorage
    {
        private readonly string _directoryPath;
        private object _readLock = new object();
        private object _writeLock = new object();

        public FileSystemStorage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        #region ** ICollaborationStorage interface implementation

        public Task<byte[]> ReadData(string key)
        {            
            return Task.Factory.StartNew(() =>
            {
                string filePath = Path.Combine(_directoryPath, $"{key}");
                if (File.Exists(filePath))
                {
                    lock (_readLock)
                    {
                        return File.ReadAllBytes(filePath);
                    }
                }
                return null;
            });
            
        }

        public Task WriteData(string key, byte[] data)
        {
            return Task.Factory.StartNew(() =>
            {
                var filePath = Path.Combine(_directoryPath, $"{key}");
                lock (_writeLock)
                {
                    if (data == null)
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    else
                    {
                        File.WriteAllBytes(filePath, data);
                    }
                }
            });
        }

        #endregion
    }
}
