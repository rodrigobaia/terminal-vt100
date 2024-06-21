using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Serilog;
using System.Threading;

namespace TerminalVT100
{
    /// <summary>
    /// Tipo de Log
    /// </summary>
    public enum TypeLog
    {
        /// <summary>
        /// INformatipo
        /// </summary>
        Info = 1,

        /// <summary>
        /// Atenção
        /// </summary>
        Warn,

        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// Servidor Terminal VT 100
    /// </summary>
    public class TedVT100Server : IDisposable
    {
        private TcpListener _server;
        private int _portNumber;
        private ConcurrentDictionary<string, TcpClient> _connectedClients;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Evento que informa a existe de dados enviados pelo o Terminal para o servidor TEDVT100
        /// </summary>
        public event Action<string, string> ClientDataReceived;

        /// <summary>
        /// Evento que informa a conexão de um terminal no servidor TEDVT100
        /// </summary>
        public event Action<string> ClientConnected;

        private bool _disposed = false;
        private bool _isRunning = false;

        /// <summary>
        /// Construtor
        /// </summary>
        public TedVT100Server()
        {
            _connectedClients = new ConcurrentDictionary<string, TcpClient>();
        }

        /// <summary>
        /// Gravar Log
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="text">Texto</param>
        /// <param name="typeLog">Tipo de Log</param>
        /// <param name="ex">Exception quando for um erro</param>
        public void SaveLog(string ip, string text, TypeLog typeLog = TypeLog.Error, Exception ex = null)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathLog = Path.Combine(path, "logs");

            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }

            var fileFull = Path.Combine(pathLog, $"terminal-vt100-{ip.Replace(".", string.Empty)}-{DateTime.Now:yyyy-MM-dd}.log");

            var _logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.File(fileFull, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
#if DEBUG
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose)
#else
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Error)
#endif
                 .CreateLogger();

            switch (typeLog)
            {
                case TypeLog.Info:
                    _logger.Information(text);
                    break;
                case TypeLog.Warn:
                    _logger.Warning(text);
                    break;
                case TypeLog.Error:
                    _logger.Error(ex, text);
                    break;
            }
        }

        /// <summary>
        /// Inicia o Servidor
        /// </summary>
        /// <param name="portNumber"></param>
        public void Start(int portNumber = 1001)
        {
            _portNumber = portNumber;
            string ipCurrent = BuscarIpCorrente();

            if (string.IsNullOrEmpty(ipCurrent))
            {
                SaveLog("localhost", "Failed to determine current IP address. Server cannot start.", TypeLog.Warn);
                return;
            }

            _server = new TcpListener(IPAddress.Parse(ipCurrent), _portNumber);
            _server.Start();

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            Task.Run(() => AcceptClientsAsync(_cancellationTokenSource.Token));
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    SaveLog("localhost", "AcceptClientsAsync", TypeLog.Error, ex);
                }
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
                    int bufferSize = client.ReceiveBufferSize;
                    byte[] buffer = new byte[bufferSize];

                    while (_isRunning)
                    {
                        try
                        {
                            int bytesRead = 0;

                            try
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            }
                            catch (Exception ex)
                            {
                                SaveLog(clientIp, "Error reading from client stream", TypeLog.Error, ex);
                                break;
                            }

                            if (bytesRead == 0) // Conexão foi fechada
                            {
                                SaveLog(clientIp, "Connection closed by client.", TypeLog.Warn);
                                break;
                            }

                            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            ClientDataReceived?.Invoke(clientIp, data);
                        }
                        catch (Exception ex)
                        {
                            SaveLog(clientIp, "Error handling client - while", TypeLog.Error, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog(clientIp, "Error handling client.", TypeLog.Error, ex);
            }
            finally
            {
                client.Close();
                _connectedClients.TryRemove(clientIp, out _);
            }
        }

        /// <summary>
        /// Busca Client para envio de dados
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <returns></returns>
        private async Task<TcpClient> GetTcpClientAsync(string ip)
        {
            try
            {
                if (!_connectedClients.TryGetValue(ip, out TcpClient client))
                {
                    SaveLog(ip, "GetTcpClientAsync - Client not found, Attempting to connect.", TypeLog.Warn);

                    try
                    {
                        client = new TcpClient();
                        await client.ConnectAsync(ip, _portNumber);

                        if (!_connectedClients.TryAdd(ip, client))
                        {
                            client.Close();
                            SaveLog(ip, "GetTcpClientAsync - Failed to add new client to the list.", TypeLog.Warn);
                            return null;
                        }

                        SaveLog(ip, "GetTcpClientAsync - Successfully connected to new client.");
                    }
                    catch (Exception ex)
                    {
                        SaveLog(ip, "GetTcpClientAsync - Error connecting to client", TypeLog.Error, ex);
                        throw;
                    }
                }

                return client;
            }
            catch (Exception ex)
            {
                SaveLog(ip, "TedVT100Server - GetTcpClientAsync", TypeLog.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Limpa o Display
        /// </summary>
        // <param name="ip">IP do Terminal</param>
        public void ClearDisplay(string ip)
        {
            Task.Run(() => ClearDisplayAsync(ip));
        }

        /// <summary>
        /// Limpa o Display
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <returns></returns>
        public async Task ClearDisplayAsync(string ip)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "ClearDisplayAsync - Client not found.", TypeLog.Warn);
                return;
            }

            try
            {
                string message = $"{(char)27}[H{(char)27}[J";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
                await PositionCursorAsync(ip);
            }
            catch (Exception ex)
            {
                SaveLog(ip, $"Clear Display to client.", TypeLog.Error, ex);
            }
        }

        /// <summary>
        /// Enviar Mensagem
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="message">Mensagem a ser enviada</param>
        /// <param name="breakLine">Informa se é para quebrar linha no fim da mensagem</param>
        public void SendMessage(string ip, string message, bool breakLine = true)
        {
            Task.Run(() => SendMessageAsync(ip, message, breakLine));
        }

        /// <summary>
        /// Envia mensagem para o Terminal
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="message">Mensagem a ser enviada</param>
        /// <param name="breakLine">Informa se é para quebrar linha no fim da mensagem</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string ip, string message, bool breakLine = false)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "SendMessageAsync - Client not found", TypeLog.Warn);
                return;
            }

            try
            {

                if (breakLine)
                {
                    // Adiciona uma quebra de linha ao final da mensagem
                    message += "\r\n";  // Ou apenas "\n" dependendo do requisito VT100
                }

                var data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                SaveLog(ip, "Error sending message to client.", TypeLog.Error, ex);
            }
        }

        /// <summary>
        /// Posição do Curor
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="row">Numero da Linha</param>
        /// <param name="column">Número da Coluna</param>
        public void PositionCursor(string ip, int row = 1, int column = 1)
        {
            Task.Run(() => PositionCursorAsync(ip, row, column));
        }

        /// <summary>
        /// Posição do Curor
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="row">Numero da Linha</param>
        /// <param name="column">Número da Coluna</param>
        /// <returns></returns>
        public async Task PositionCursorAsync(string ip, int row = 1, int column = 1)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "PositionCursorAsync - Client not found.", TypeLog.Warn);
                return;
            }

            try
            {
                string message = $"\x1B[{row};{column}H";
                var data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                SaveLog(ip, "Error position cursor to client", TypeLog.Error, ex);

            }
        }

        /// <summary>
        /// Enviar Beep para o Terminal
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="timeBeep">Tem po em Milisegundos de duração do Beep</param>
        public void Beep(string ip, int timeBeep = 600)
        {
            Task.Run(() => BeepAsync(ip, timeBeep));
        }

        /// <summary>
        /// Enviar Beep para o Terminal
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="timeBeep">Tem po em Milisegundos de duração do Beep</param>
        /// <returns></returns>
        public async Task BeepAsync(string ip, int timeBeep = 600)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "BeepAsync - Client not found.", TypeLog.Warn);
                return;
            }

            try
            {
                await BeepOnAsync(client);
                Thread.Sleep(timeBeep);
                await BeepOffAsync(client);
            }
            catch (Exception ex)
            {
                SaveLog(ip, "Error Beep to client.", TypeLog.Error, ex);

            }
        }

        /// <summary>
        /// Desliga o Beep
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task BeepOffAsync(TcpClient client)
        {
            try
            {
                // Buzzer
                var acionamentoChar = (char)11;
                var message = $"{(char)27}[?24c{(char)27}[5i{acionamentoChar}{(char)27}[4i";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                string clientIp = endPoint.Address.ToString();
                SaveLog(clientIp, "TedVT100 - BeepOffAsync", TypeLog.Error, ex);
            }
        }

        /// <summary>
        /// Liga o Beep
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task BeepOnAsync(TcpClient client)
        {
            try
            {
                // Buzzer
                var acionamentoChar = (char)7;
                var message = $"{(char)27}[?24c{(char)27}[5i{acionamentoChar}{(char)27}[4i";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);

            }
            catch (Exception ex)
            {
                IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                string clientIp = endPoint.Address.ToString();
                SaveLog(clientIp, "TedVT100 - BeepOnAsync", TypeLog.Error, ex);
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

        /// <summary>
        /// /// Habilitar porta COM 1
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <returns></returns>
        public async Task EnabledCOM1Async(string ip)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "EnabledCOM1Async - Client not found.", TypeLog.Warn);
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
                SaveLog(ip, "Error Enable Port Com 1 to client.", TypeLog.Error, ex);
            }
        }

        /// <summary>
        /// Habilitar porta COM 2
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        public void EnabledCOM2(string ip)
        {
            EnabledCOM2Async(ip);
        }

        /// <summary>
        /// Habilitar porta COM 2
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <returns></returns>
        public async Task EnabledCOM2Async(string ip)
        {
            var client = await GetTcpClientAsync(ip);

            if (client == null)
            {
                SaveLog(ip, "EnabledCOM2Async - Client not found.", TypeLog.Warn);
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
                SaveLog(ip, "Error Enable Port Com 2 to client.", TypeLog.Error, ex);
            }
        }

        /// <summary>
        /// Dispose do Objeto
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _server?.Stop();
                    _isRunning = false;

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