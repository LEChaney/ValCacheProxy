using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;

namespace ValCacheProxyLib
{
    public class FileServerProxyService : IFileServer
    {
        public const string CACHE_DIR = @".\cache";

        public string[] GetAvailableFiles()
        {
            using (var client = new FileServerService.FileServerClient())
            {
                return client.GetAvailableFiles();
            }
        }

        public Stream GetFile(string filename)
        {
            Trace.TraceInformation("Received request for file: " + filename);

            string filePath = Path.Combine(CACHE_DIR, filename);

            // Retrieve file from server if it doesn't exist in cache
            if (!File.Exists(filePath))
            {
                Trace.TraceInformation("File not found in cache, requesting from server...");
                using (var client = new FileServerService.FileServerClient())
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var outFileStream = new FileStream(filePath, FileMode.Create))
                    {
                        client.GetFile(filename).CopyTo(outFileStream);
                    }
                }
                Trace.TraceInformation("File cached at " + filePath);
            }
            else
            {
                Trace.TraceInformation("File found in cache at " + filePath);
            }

            // Serve file from cache
            Trace.TraceInformation("Serving file from cache...");
            try
            {
                var file = File.OpenRead(filePath);
                return file;
            }
            catch (IOException ex)
            {
                Trace.TraceError(String.Format("An exception was thrown while trying to open file {0}", filePath));
                Trace.TraceError("Exception is: ");
                Trace.TraceError(ex.ToString());
                throw;
            }
        }
    }
}
