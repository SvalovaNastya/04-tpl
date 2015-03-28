using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace Balancer
{
    class Program
    {
        private const int port = 19999;
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        private static Random r = new Random();
        private static readonly string[] ports = { "20000", "20001", "20002", "20003", "20004" };

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            try
            {
                var listener = new Listener(port, "method", OnContextAsync);
                listener.Start();

                log.InfoFormat("Server started!");
                new ManualResetEvent(false).WaitOne();
            }
            catch (Exception e)
            {
                log.Fatal(e);
                throw;
            }
        }

        private static async Task OnContextAsync(HttpListenerContext context)
        {
            Guid requestId = Guid.NewGuid();
            string query = context.Request.QueryString["query"];
            IPEndPoint remoteEndPoint = context.Request.RemoteEndPoint;
            log.InfoFormat("{0}: received {1} from {2}", requestId, query, remoteEndPoint);
            context.Request.InputStream.Close();

            var curport = ports[r.Next(5)];
            log.InfoFormat("I sent request to {0}", curport);
            MemoryStream ms = await DownloadWebPageAsync(query, curport);
            byte[] encryptedBytes = ms.ToArray();

            await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
            context.Response.OutputStream.Close();
            log.InfoFormat("{0}: {1} sent back to {2}", requestId, encryptedBytes, remoteEndPoint);
        }

        public static async Task<MemoryStream> DownloadWebPageAsync(string query, string curport)
        {
            Stopwatch sw = Stopwatch.StartNew();
            HttpWebRequest request = CreateRequest(String.Format("http://10.113.119.35:{0}/method?query={1}", curport, query));
            WebResponse response = await request.GetResponseAsync();
            using (Stream stream = response.GetResponseStream())
            {
                var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Console.WriteLine("Got {0} bytes in {1} ms", ms.Position, sw.ElapsedMilliseconds);
                return ms;
            }
        }

        private static HttpWebRequest CreateRequest(string uriStr, int timeout = 30*1000)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uriStr);
            request.Timeout = timeout;
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }
    }
}
