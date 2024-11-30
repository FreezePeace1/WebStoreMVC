using System.Diagnostics;
using CG.Web.MegaApiClient;
using Microsoft.Extensions.Configuration;

namespace Backup
{
    class Program
    {
        private static readonly string _fileName = "pg_dump" + DateTime.Now.ToString("yyyy") + "_" + DateTime.Now.ToString("MM") + "_" + DateTime.Now.ToString("dd");
        private static readonly string _filePath = @"/home/alexander/RiderProjects/WebStoreMVC/Backup/backups/pg_dump" + DateTime.Now.ToString("yyyy") + "_" + DateTime.Now.ToString("MM") + "_" + DateTime.Now.ToString("dd") + ".backup";
        private static readonly string _backupPath = $@"/home/alexander/RiderProjects/WebStoreMVC/Backup/backups/";
        private static readonly string _fileNameForCreatingBackup = "pg_dump";
        
        public static async Task UploadFileToMega()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            var email = config["BackupString:email"];
            var accountPassword = config["BackupString:accountPassword"];
            
            try
            {
                var client = new MegaApiClient();
                await client.LoginAsync(email, accountPassword);

                // Specify the folder where you want to upload the file
                var rootNode = await client.GetNodesAsync();
                var uploadsFolder = rootNode.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == "backups");

                // Get the list of all folders within the uploads folder
                var uploadsFolders = await client.GetNodesAsync(uploadsFolder);

                // Search for the uploads folder within the list of folders
                var backupsFolder = uploadsFolders.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == "backups");

                if (backupsFolder == null)
                {
                    // Create uploads folder if it does not exist
                    backupsFolder = await client.CreateFolderAsync("backups", uploadsFolder);
                }

                // Generate a unique filename using GUID
                string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(_fileName);

                await using (var stream = new FileStream(_filePath, FileMode.Open))
                {
                    // Upload the file to the uploads folder with the unique filename
                    await client.UploadAsync(stream, uniqueFileName, backupsFolder);   
                }

                await client.LogoutAsync();
                
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                Console.WriteLine($"An error occurred during file upload: {ex.Message}");
            }
        }
        
        public static async Task BackupDatabase()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            var dbPassword = config["BackupString:databasePassword"];
            
            try
            {
                Environment.SetEnvironmentVariable("PGPASSWORD", dbPassword);

                string backupFile = _backupPath + _fileNameForCreatingBackup + DateTime.Now.ToString("yyyy") + "_" + DateTime.Now.ToString("MM") + "_" + DateTime.Now.ToString("dd") + ".backup";
                string backupString = "-bv -Z3 -f \"" + backupFile + "\" -Fc -h " + "127.0.0.1" + " -U " + "postgres" + " -p " + "5432" + " " + "WebStore";

                Process proc = new Process();
                proc.StartInfo.FileName = "/usr/bin/pg_dump";
                proc.StartInfo.Arguments = backupString;

                proc.Start();
                await proc.WaitForExitAsync();
                proc.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        static async Task Main()
        {
            
            
            await BackupDatabase();

            await UploadFileToMega();

        }
    }
}