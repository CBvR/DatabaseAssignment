using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Azure.Storage.Blobs;

namespace Services
{
    public interface IBlobService
    {
        public string AddFile(string fileToUpload, string whereToStore);

        public string GetUri(string fileName);

        public void DownloadFromBlob(string fileName);
    }
    public class BlobSerivce : IBlobService
    {
        static BlobContainerClient blobContainerClient { get; set; }

        public BlobSerivce(ILogger<BlobSerivce> Logger) { }

        public string AddFile(string fileToUpload, string whereToStore)
        {
            string filename_withExtension = Path.GetFileName(fileToUpload);

            blobContainerClient = new BlobContainerClient(ConfigurationManager.AppSettings["blobStorage"], whereToStore);
            blobContainerClient.CreateIfNotExists();
            var blob = blobContainerClient.GetBlobClient(filename_withExtension);

            Stream retrievedStream = File.Open(fileToUpload, FileMode.Open);

            using (var fileStream = retrievedStream)
            {
                blob.Upload(fileStream, overwrite: true);
            }
            return filename_withExtension;
        }

        public string GetUri(string fileName)
        {
            blobContainerClient = new BlobContainerClient(ConfigurationManager.AppSettings["blobStorage"], "mortgages");
            blobContainerClient.CreateIfNotExists();
            var blob = blobContainerClient.GetBlobClient(Path.GetFileName(fileName));
            return blob.Uri.AbsoluteUri;
        }

        public async void DownloadFromBlob(string filetoDownload)
        {
            Console.WriteLine("Inside downloadfromBlob()");

            string storageAccount_connectionString = ConfigurationManager.AppSettings["blobStorage"];

            CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse(storageAccount_connectionString);
            CloudBlobClient blobClient = mycloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("HouseImages");
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(filetoDownload);

            // provide the file download location below            
            Stream file = File.OpenWrite(filetoDownload);

            await cloudBlockBlob.DownloadToStreamAsync(file);

            Console.WriteLine("Download completed!");
        }
    }
}