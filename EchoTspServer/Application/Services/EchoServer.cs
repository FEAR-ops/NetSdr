using EchoTspServer.Application.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer.Application.Services
{
    public class EchoServer : IEchoServer
    {
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly IClientHandler _clientHandler;
        private TcpListener? _listener;
        private readonly CancellationTokenSource _cts = new();

        public EchoServer(int port, ILogger logger, IClientHandler clientHandler)
        {
            _port = port;
            _logger = logger;
            _clientHandler = clientHandler;
        }

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _logger.Info($"Server started on port {_port}.");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.Info("Client connected.");
                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client, _cts.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            _logger.Info("Server shutdown.");
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener?.Stop();
            _cts.Dispose();
            _logger.Info("Server stopped.");
        }
    }
}
