using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ProxiServer
{
    public class Listener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Program));
        private readonly HttpListener listener;

        public Listener(int port, string suffix, Func<HttpListenerContext, Task> callbackAsync)
        {
            ThreadPool.SetMinThreads(8, 8);
            CallbackAsync = callbackAsync;
            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://+:{0}{1}/", port,
                suffix != null ? "/" + suffix.TrimStart('/') : ""));
        }

        private Func<HttpListenerContext, Task> CallbackAsync { get; set; }

        public void Start()
        {
            listener.Start();
            StartListen();
        }

        public async void StartListen()
        {
            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    Task.Run(
                        async () =>
                        {
                            HttpListenerContext ctx = context;
                            try
                            {
                                await CallbackAsync(ctx);
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                            }
                            finally
                            {
                                ctx.Response.Close();
                            }
                        }
                        );
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
        }
    }
}