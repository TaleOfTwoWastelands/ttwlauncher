using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace com.taleoftwowastelands.patchmaker
{
    public static class FileInfoExtension
    {
        public static string GetFullNameRelative(this FileInfo fileInfo, string rootPath)
        {
            return fileInfo.FullName.Replace(rootPath, "").TrimStart('\\');
        }

        public static string GetPathRelative(this FileInfo fileInfo, string rootPath)
        {
            return fileInfo.DirectoryName.Replace(rootPath, "").TrimStart('\\');
        }
    }
}
