
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
            string webUrl = "https://SITE.sharepoint.com/sites//";

            string username = "";
            string password = "";

            using (ClientContext ctx = new ClientContext(webUrl))
            {
                
                string account = username;
                var secret = new SecureString();
                foreach (char c in password)
                {
                    secret.AppendChar(c);
                }
                ctx.Credentials = new SharePointOnlineCredentials(account, secret);
                ctx.Load(ctx.Web);
                ctx.ExecuteQuery();

                List list = ctx.Web.Lists.GetByTitle("Documentos");

                string rutaFunciona = "/sites/SITE/Documentos%20compartidos/Arquitectura";
                string ruta = "/sites/SITE/Documentos%20compartidos/Arquitectura%2FGeneral";
                FileCollection files = list.RootFolder.Folders.GetByUrl(ruta).Files;

                ctx.Load(files);
                ctx.ExecuteQuery();

                foreach (Microsoft.SharePoint.Client.File file in files)
                {
                    FileInformation fileinfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, file.ServerRelativeUrl);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(file.Name);
                    
                    ctx.ExecuteQuery();

                    using (FileStream filestream = new FileStream(Environment.CurrentDirectory + "\\"+ file.Name, FileMode.Create))
                    {
                        fileinfo.Stream.CopyTo(filestream);
                    }

                }
            };



            ClientContext ctxSite = new ClientContext(webUrl);
            SecureString securePassword = new SecureString();
            foreach (char c in password.ToCharArray()) securePassword.AppendChar(c);
            ctxSite.Credentials = new SharePointOnlineCredentials(username, securePassword);


            //Load Libraries from SharePoint
            ctxSite.Load(ctxSite.Web.Lists);
            ctxSite.ExecuteQuery();

            Web web = ctxSite.Web;
            var docLibs = ctxSite.LoadQuery(web.Lists.Where(l => l.BaseTemplate == 101));  //DocumentLibrary only
            ctxSite.ExecuteQuery();

            foreach (var list in docLibs)
            {
                //Console.WriteLine(list.Title);
                ctxSite.Load(list.RootFolder.Folders);
                ctxSite.ExecuteQuery();

                string listTitle = list.Title;

                string folderName = "Carpeta";
                string driveName = Environment.CurrentDirectory;
                //Console.WriteLine("List Tile ------------------------------- " + listTitle);
                foreach (Folder folder in list.RootFolder.Folders)
                {
                    ctxSite.Load(folder.Files);
                    ctxSite.ExecuteQuery();

                    if (String.Equals(folder.Name, folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        var folderDestination = driveName+@":\Test\SharePoint\" + listTitle + @"\" + folderName + @"\";
                        ctxSite.Load(folder.Files);
                        ctxSite.ExecuteQuery();

                        foreach (var file in folder.Files)
                        {
                            var fileName = Path.Combine(folderDestination, file.Name);
                            if (!System.IO.File.Exists(fileName))
                            {
                                Directory.CreateDirectory(folderDestination);
                                var fileRef = file.ServerRelativeUrl;
                                var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctxSite, fileRef);
                                using (var fileStream = System.IO.File.Create(fileName))
                                {
                                    fileInfo.Stream.CopyTo(fileStream);
                                }
                            }
                        }
                        Console.WriteLine("Downloaded the file in " + folderDestination);
                    }

                }

            }
        }
    }
}
