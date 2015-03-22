using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.IO;
using System.Diagnostics;

namespace HashServer
{
	class Program
	{
		static void Main(string[] args)
		{


			XmlConfigurator.Configure();
			try
			{
				var listener = new Listener(port, "method", OnContextAsync);
				listener.Start();

				log.InfoFormat("Server started!");
				new ManualResetEvent(false).WaitOne();
			}
			catch(Exception e)
			{
				log.Fatal(e);
				throw;
			}
		}

		private static async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.QueryString["query"];
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: received {1} from {2}", requestId, query, remoteEndPoint);
			context.Request.InputStream.Close();

            var ms = await DownloadWebPageAsync(query);
            var encryptedBytes = ms.ToArray();

			await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
			context.Response.OutputStream.Close();
			log.InfoFormat("{0}: {1} sent back to {2}", requestId, encryptedBytes.ToString(), remoteEndPoint);
		}



		private static byte[] CalcHash(byte[] data)
		{
			using(var hasher = new HMACMD5(Key))
				return hasher.ComputeHash(data);
		}

        public static async Task<MemoryStream> DownloadWebPageAsync(string query)
        {
            var sw = Stopwatch.StartNew();
            var request = CreateRequest("http://10.10.80.108:20000/method?query=" + query);
            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Console.WriteLine("Got {0} bytes in {1} ms", ms.Position, sw.ElapsedMilliseconds);
                return ms;
            }
        }

        private static HttpWebRequest CreateRequest(string uriStr, int timeout = 30 * 1000)
        {
            var request = WebRequest.CreateHttp(uriStr);
            request.Timeout = timeout;
            request.Proxy = null;
            request.KeepAlive = true;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 100500;
            return request;
        }

		private const int port = 20000;
		private static readonly byte[] Key = Encoding.UTF8.GetBytes("Контур.Шпора");
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
	}
}