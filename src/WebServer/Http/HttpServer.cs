﻿using Devkoes.HttpMessage;
using Devkoes.HttpMessage.RequestParsers;
using Devkoes.Restup.WebServer.Models.Contracts;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Devkoes.Restup.WebServer.Http
{
    public abstract class HttpServer : IDisposable
    {
        private readonly int _port;
        private readonly StreamSocketListener _listener;
        private HttpRequestParser _requestParser;

        internal HttpServer(int serverPort)
        {
            _listener = new StreamSocketListener();
            _requestParser = new HttpRequestParser();
            _port = serverPort;
            _listener.ConnectionReceived += ProcessRequestAsync;
        }

        internal abstract Task<IHttpResponse> HandleRequest(HttpRequest request);

        public async Task StartServerAsync()
        {
            await _listener.BindServiceNameAsync(_port.ToString());

            Debug.WriteLine($"Webserver started on port {_port}");
        }

        public void StopServer()
        {
            ((IDisposable)this).Dispose();

            Debug.WriteLine($"Webserver on port {_port} stopped");
        }

        private async void ProcessRequestAsync(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var request = await _requestParser.ParseRequestStream(args.Socket);

                    var httpResponse = await HandleRequest(request);

                    await WriteResponseAsync(httpResponse, args.Socket);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception while handling process: {ex.Message}");
                }
                finally
                {
                    try
                    {
                        args.Socket.Dispose();
                    }
                    catch { }
                }
            });
        }

        private async Task WriteResponseAsync(IHttpResponse response, StreamSocket socket)
        {
            using (IOutputStream output = socket.OutputStream)
            {
                await output.WriteAsync(response.RawResponse.AsBuffer());
                await output.FlushAsync();
            }
        }

        void IDisposable.Dispose()
        {
            _listener.Dispose();
        }
    }
}
