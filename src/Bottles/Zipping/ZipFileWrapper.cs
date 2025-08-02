using System.IO;
using System.IO.Compression;

namespace Bottles.Zipping
{
    public class ZipFileWrapper : IZipFile
    {
        private readonly ZipArchive _file;

        public ZipFileWrapper(ZipArchive file)
        {
            _file = file;
        }

        public void AddFile(string fileName)
        {
            _file.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
        }

        public void AddFile(string fileName, string zipFolder)
        {
            _file.CreateEntryFromFile(fileName, Path.Combine(zipFolder, Path.GetFileName(fileName)));
        }

        public void AddFiles(ZipFolderRequest request)
        {
            if (!Directory.Exists(request.RootDirectory)) return;
            if (request.FileSet == null) return;

            request.WriteToZipFile(this);
        }
    }
}