using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace HashServer
{
	class Program
	{
		static void Main(string[] args)
		{
			XmlConfigurator.Configure();
			try
			{
			    var listeners = Enumerable.Range(0, 5).Select(x => new Listener(ports[x], "method", OnContextAsync)).ToArray();
			    for (int i = 0; i < listeners.Length; i++)
			    {
			        listeners[i].Start();
				    log.InfoFormat("Server started on port{0}!", ports[i]);
			    }
//				var listener = new Listener(port, "method", OnContextAsync);
//				listener.Start();

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

			var hash = Convert.ToBase64String(CalcHash(Encoding.UTF8.GetBytes(query)));
			var encryptedBytes = Encoding.UTF8.GetBytes(hash);

			await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
			context.Response.OutputStream.Close();
			log.InfoFormat("{0}: {1} sent back to {2}", requestId, hash, remoteEndPoint);
		}

		private static byte[] CalcHash(byte[] data)
		{
			using(var hasher = new HMACMD5(Key))
				return hasher.ComputeHash(data);
		}

		private static readonly int[] ports = 
	    {
	        20000,
	        20001,
	        20002,
	        20003,
	        20004,
	        20005
	    };
		private static readonly byte[] Key = Encoding.UTF8.GetBytes("Контур.Шпора");
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
	}
}