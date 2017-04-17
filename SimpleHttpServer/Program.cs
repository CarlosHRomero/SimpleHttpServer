using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimpleHttpServer;
using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;

namespace SimpleHttpServer
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            var route_config = new List<Models.Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = @"/\d{5}$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        var usuario = request.Url.Substring(1);
                        var f = new frmAutenticar();
                        f.Usuario = usuario;
                        //Application.Run(f);
                        f.ShowDialog();
                        if (f.completado)
                            f.Close();
                        if(f.resultado)
                        { 
                            return new HttpResponse()
                            {
                                ContentAsUTF8 = "Autenticado",
                                ReasonPhrase = "OK",
                                StatusCode = "200"
                            };
                        }
                        else
                        { 
                            return new HttpResponse()
                            {
                                ContentAsUTF8 = "No autenticado",
                                ReasonPhrase = "OK",
                                StatusCode = "200"
                            };
                        }
                     }
                }, 

          

                //new Route {   
                //    Name = "FileSystem Static Handler",
                //    UrlRegex = @"^/Static/(.*)$",
                //    Method = "GET",
                //    Callable = new FileSystemRouteHandler() { BasePath = @"C:\Tmp", ShowDirectories=true }.Handle,
                //},
            };

            HttpServer httpServer = new HttpServer(5000, route_config);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
