using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class FileManager
    {
        public static string ReadFile(string filePath)
        {
            return System.Text.Encoding.Default.GetString(GetFileData(filePath));
        }

        private static byte[] GetFileData(string filePath)
        {
            byte[] buffer;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                var length = (int)fileStream.Length; // get file length
                buffer = new byte[length]; // create buffer
                var count = 0; // actual number of bytes read
                var sum = 0; // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

    }
}
