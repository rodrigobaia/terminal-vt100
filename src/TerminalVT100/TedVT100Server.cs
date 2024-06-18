﻿using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TerminalVT100
{
    /// <summary>
    /// Servidor Terminal VT 100
    /// </summary>
    public class TedVT100Server : IDisposable
    {
        private ILogger _logger;
        private TcpListener _server;
        private int _portNumber;
        private ConcurrentDictionary<string, TcpClient> _connectedClients;
        private ConcurrentDictionary<string, StringBuilder> _receiveDatas;

        public event Action<string, string> ClientDataReceived;
        public event Action<string> ClientConnected;

        private bool _disposed = false;

        public TedVT100Server()
        {
            InitializeLogger();
            _connectedClients = new ConcurrentDictionary<string, TcpClient>();
        }

        private void InitializeLogger()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathLog = Path.Combine(path, "logs");

            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }

            var fileFull = Path.Combine(pathLog, "terminal-vt100.log");

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(fileFull, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
#if DEBUG
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose)
#else
                .WriteTo.Console(Serilog.Events.LogEventLevel.Error)
#endif
                .CreateLogger();
        }

        public void Start(int portNumber = 1001)
        {
            _portNumber = portNumber;
            string ipCurrent = BuscarIpCorrente();

            if (string.IsNullOrEmpty(ipCurrent))
            {
                _logger.Error("Failed to determine current IP address. Server cannot start.");
                return;
            }

            _server = new TcpListener(IPAddress.Parse(ipCurrent), _portNumber);
            _server.Start();

            Task.Run(() => AcceptClientsAsync());
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            string clientIp = endPoint.Address.ToString();

            _connectedClients.TryAdd(clientIp, client);
            ClientConnected?.Invoke(clientIp);

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    StringBuilder messageBuilder = new StringBuilder();

                    while (true)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;

                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(data);

                        if (_receiveDatas == null)
                        {
                            _receiveDatas = new ConcurrentDictionary<string, StringBuilder>();
                        }

                        _receiveDatas[clientIp] = messageBuilder;

                        await SendMessageAsync(clientIp, data, false);
                        if (data.Contains("\r"))
                        {
                            string receivedData = _receiveDatas[clientIp].ToString().Replace("\r", "");
                            ClientDataReceived?.Invoke(clientIp, receivedData);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling client.");
            }
            finally
            {
                client.Close();
                _connectedClients.TryRemove(clientIp, out _);
            }
        }

        /// <summary>
        /// Enviar Mensagem
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="message"></param>
        /// <param name="breakLine"></param>
        public async void SendMessage(string ip, string message, bool breakLine = true)
        {
            SendMessageAsync(ip, message, breakLine);
        }

        public void ClearDisplay(string ip)
        {
            ClearDisplayAsync(ip);
        }

        public async Task ClearDisplayAsync(string ip)
        {
            if (!_connectedClients.TryGetValue(ip, out TcpClient client))
            {
                _logger.Warning($"Client not found: {ip}");
                return;
            }

            try
            {
                string message = $"{(char)27}[H{(char)27}[J";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Clear Display to client {ip}");
            }
        }

        public async Task SendMessageAsync(string ip, string message, bool breakLine = true)
        {
            if (!_connectedClients.TryGetValue(ip, out TcpClient client))
            {
                _logger.Warning($"Client not found: {ip}");
                return;
            }

            try
            {
                if (breakLine)
                {
                    // Adiciona uma quebra de linha ao final da mensagem
                    message += "\r\n";  // Ou apenas "\n" dependendo do requisito VT100
                }

                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error sending message to client {ip}");
            }
        }

        /// <summary>
        /// Habilitar porta COM 1
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        public void EnabledCOM1(string ip)
        {
            EnabledCOM1Async(ip);
        }

        public async Task EnabledCOM1Async(string ip)
        {
            if (!_connectedClients.TryGetValue(ip, out TcpClient client))
            {
                _logger.Warning($"Client not found: {ip}");
                return;
            }

            try
            {
                string message = $"{(char)27}[H{(char)27}[J";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error Enable Port Com 1 to client {ip}");
            }
        }

        public void EnabledCOM2(string ip)
        {
            EnabledCOM2Async(ip);
        }

        public async Task EnabledCOM2Async(string ip)
        {
            if (!_connectedClients.TryGetValue(ip, out TcpClient client))
            {
                _logger.Warning($"Client not found: {ip}");
                return;
            }

            try
            {
                string message = $"{(char)27}[H{(char)27}[J";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error Enable Port Com 2 to client {ip}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _server.Stop();
                    foreach (var client in _connectedClients.Values)
                    {
                        client.Close();
                        client.Dispose();
                    }
                    _connectedClients.Clear();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Buscar o endereço IP corrente da máquina.
        /// </summary>
        /// <returns>O endereço IP corrente, ou string vazia se não encontrado.</returns>
        private string BuscarIpCorrente()
        {
            string gatewayAddress = GetDefaultGatewayAddress();
            string[] ipAddresses = GetLocalIPAddresses();
            var ipLocal = new List<string>();

            foreach (string ipAddress in ipAddresses)
            {
                if (IsConnectedToInternet(ipAddress))
                {
                    var ipNet = ipAddress.Substring(0, ipAddress.LastIndexOf('.'));
                    var ipGateway = string.IsNullOrEmpty(gatewayAddress) ? string.Empty : gatewayAddress.Substring(0, gatewayAddress.LastIndexOf('.'));

                    if (ipNet == ipGateway)
                    {
                        ipLocal.Add(ipAddress);
                        break;
                    }
                }
            }

            if (ipLocal.Any())
            {
                return ipLocal.First();
            }

            return string.Empty;
        }

        /// <summary>
        /// Busca o gateway padrão.
        /// </summary>
        /// <returns>O endereço IP do gateway padrão, ou string vazia se não encontrado.</returns>
        private string GetDefaultGatewayAddress()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c route print",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            // Use uma expressão regular para encontrar o endereço IP do gateway padrão
            Regex regex = new Regex(@"0\.0\.0\.0\s+0\.0\.0\.0\s+(?<gateway>\d+\.\d+\.\d+\.\d+)");
            Match match = regex.Match(output);

            if (match.Success)
            {
                return match.Groups["gateway"].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Busca todos os endereços de IP locais da máquina.
        /// </summary>
        /// <returns>Um array contendo os endereços de IP locais.</returns>
        private string[] GetLocalIPAddresses()
        {
            string hostName = System.Net.Dns.GetHostName();
            System.Net.IPAddress[] addresses = System.Net.Dns.GetHostAddresses(hostName);

            var ipAddresses = new List<string>();
            for (int i = 0; i < addresses.Length; i++)
            {
                if (addresses[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ipAddresses.Add(addresses[i].ToString());
            }

            return ipAddresses.ToArray();
        }

        /// <summary>
        /// Verifica se há conexão com a internet em um determinado endereço IP.
        /// </summary>
        /// <param name="ipAddress">O endereço IP a ser verificado.</param>
        /// <returns>True se há conexão com a internet, caso contrário, False.</returns>
        private bool IsConnectedToInternet(string ipAddress)
        {
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = ping.Send(ipAddress, 1000);
                    return (reply.Status == IPStatus.Success);
                }
                catch (PingException)
                {
                    // Lidar com exceção de ping
                }

                return false;
            }
        }
    }
}
