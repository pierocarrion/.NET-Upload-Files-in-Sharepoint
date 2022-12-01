
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.Core.LargeFileUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            string webUrl = "";                                    //https://domain.sharepoint.com/sites/sitename
            string username = "";                                  //username@domain.com.pe                         
            string password = "";                                  //password                                       


            ClientContext ctxSite = new ClientContext(webUrl);
            SecureString securePassword = new SecureString();
            foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);
            ctxSite.Credentials = new SharePointOnlineCredentials(username, securePassword);


            //Load Libraries from SharePoint
            ctxSite.Load(ctxSite.Web.Lists);
            ctxSite.ExecuteQuery();

            Web web = ctxSite.Web;
            var docLibs = ctxSite.LoadQuery(web.Lists.Where(l => l.BaseTemplate == 101));                               //DocumentLibrary only
            ctxSite.ExecuteQuery();

            foreach (var list in docLibs)
            {
                Console.WriteLine(list.Title);
                ctxSite.Load(list.RootFolder.Folders);
                ctxSite.ExecuteQuery();

                string listTitle = list.Title;

                string folderName = "Arquitectura";                                                                     //Personal Testing
                string driveName = Environment.CurrentDirectory;

                foreach (Folder folder in list.RootFolder.Folders)
                {
                    ctxSite.Load(folder.Files);
                    ctxSite.ExecuteQuery();
                    if (true)   /*String.Equals(folder.Name, folderName, StringComparison.OrdinalIgnoreCase)*/         // Personal Testing
                    {
                        var folderDestination = Environment.CurrentDirectory;
                        ctxSite.Load(folder.Files);
                        ctxSite.Load(folder.Folders);
                        ctxSite.ExecuteQuery();



                        foreach (var folderT in folder.Folders)
                        {
                            Console.WriteLine("SubFolder: " + folderT.Name);                                            // Personal Testing
                            //Obtain files the require folder
                            ctxSite.Load(folderT.Files);
                            ctxSite.ExecuteQuery();
                            foreach (var file in folderT.Files)
                            {
                                Console.WriteLine("FileName: " + file.Name);                                            // Personal Testing
                                /*
                                FileInformation fileinfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctxSite, file.ServerRelativeUrl);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(file.Name);
                                ctxSite.ExecuteQuery();

                                using (FileStream filestream = new FileStream(driveName + "\\" + file.Name, FileMode.Create))
                                {
                                    fileinfo.Stream.CopyTo(filestream);
                                }*/
                            }
                        }
                        //
                        
                        foreach (var file in folder.Files)
                        {
                            try
                            {
                                var fileName = Path.Combine(folderDestination, file.Name);
                                Console.WriteLine($"* FILENAME: {fileName}");
                                if (!System.IO.File.Exists(fileName))
                                {
                                    Directory.CreateDirectory(folderDestination);
                                    var fileRef = file.ServerRelativeUrl;
                                    var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctxSite, fileRef);
                                    using (var fileStream = System.IO.File.Create(fileName))
                                    {
                                        //fileInfo.Stream.CopyTo(fileStream);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                continue;   
                            }
                        }
                        Console.WriteLine("Downloaded the file in " + folderDestination);
                    }
                }
            }
            Console.ReadLine();
        }
    }
}

