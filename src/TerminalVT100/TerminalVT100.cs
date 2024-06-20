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
    /// Servidor Terminal VT 100
    /// </summary>
    public class TedVT100Server : IDisposable
    {
        private ILogger _logger;
        private TcpListener _server;
        private int _portNumber;
        private ConcurrentDictionary<string, TcpClient> _connectedClients;
        private ConcurrentDictionary<string, StringBuilder> _receiveDatas;

        /// <summary>
        /// Evento que informa a existe de dados enviados pelo o Terminal para o servidor TEDVT100
        /// </summary>
        public event Action<string, string> ClientDataReceived;

        /// <summary>
        /// Evento que informa a conexão de um terminal no servidor TEDVT100
        /// </summary>
        public event Action<string> ClientConnected;

        private bool _disposed = false;

        /// <summary>
        /// Construtor
        /// </summary>
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
                try
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "AcceptClientsAsync");
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
                    StringBuilder messageBuilder = new StringBuilder();
                    var row = 1;
                    var column = 0;

                    while (true)
                    {
                        try
                        {
                            byte[] buffer = new byte[client.ReceiveBufferSize];
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                                continue;

                            string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            if (_receiveDatas == null)
                            {
                                _receiveDatas = new ConcurrentDictionary<string, StringBuilder>();
                            }

                            if (Convert.ToChar(data) == (char)8)
                            {
                                if (string.IsNullOrEmpty(messageBuilder.ToString()))
                                {
                                    continue;
                                }
                                var len = messageBuilder.Length;
                                var str = messageBuilder.Remove(len - 1, 1);
                                messageBuilder = str;

                                await SendMessageAsync(clientIp, ((char)8).ToString() + " " + ((char)8).ToString(), false);
                                _receiveDatas[clientIp] = messageBuilder;
                                column--;
                                if (column <= 0)
                                {
                                    column = 0;
                                }
                                continue;
                            }
                            if (Convert.ToChar(data) == (char)127 || Convert.ToChar(data) == (char)27)
                            {
                                await PositionCursorAsync(clientIp);
                                await SendMessageAsync(clientIp, "\x1B[H\x1B[J", false);
                                column = 0;
                                continue;
                            }
                            else if (Convert.ToChar(data) != (char)8)
                            {
                                messageBuilder.Append(data);

                            }
                            _receiveDatas[clientIp] = messageBuilder;

                            if (Convert.ToChar(data) == (char)13)
                            {
                                string receivedData = _receiveDatas[clientIp].ToString().Replace("\r", "");
                                ClientDataReceived?.Invoke(clientIp, receivedData);
                                messageBuilder.Clear();
                                _receiveDatas[clientIp] = messageBuilder;
                                row++;
                                if (row > 4)
                                {
                                    row = 1;
                                }
                                Row = row;
                                column = 0;
                                continue;
                            }
                            row = Row != row ? Row : row;
                            column++;

                            if (column > 20)
                            {
                                column = 1;
                                row++;
                            }

                            await PositionCursorAsync(clientIp, row, column);
                            await SendMessageAsync(clientIp, data, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error handling client - while");
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
                    _logger.Warning($"GetTcpClientAsync - Client not found: {ip}. Attempting to connect.");

                    try
                    {
                        client = new TcpClient();
                        await client.ConnectAsync(ip, _portNumber);

                        if (!_connectedClients.TryAdd(ip, client))
                        {
                            client.Close();
                            _logger.Error($"GetTcpClientAsync - Failed to add new client to the list: {ip}");
                            return null;
                        }

                        _logger.Information($"GetTcpClientAsync - Successfully connected to new client: {ip}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"GetTcpClientAsync - Error connecting to client: {ip}, Exception: {ex.Message}");
                        throw;
                    }
                }

                return client;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100Server - GetTcpClientAsync");
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
                _logger.Warning($"ClearDisplayAsync - Client not found: {ip}");
                return;
            }

            try
            {
                if (_receiveDatas != null && _receiveDatas.ContainsKey(ip))
                {
                    _receiveDatas[ip] = null;
                }

                string message = $"{(char)27}[H{(char)27}[J";
                byte[] data = Encoding.ASCII.GetBytes(message);
                await client.GetStream().WriteAsync(data, 0, data.Length);
                await PositionCursorAsync(ip);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Clear Display to client {ip}");
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
                _logger.Warning($"SendMessageAsync - Client not found: {ip}");
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
                _logger.Error(ex, $"Error sending message to client {ip}");
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
                _logger.Warning($"PositionCursorAsync - Client not found: {ip}");
                return;
            }

            try
            {
                Row = row > 4 ? 1 : row;
                Column = column;
                var strline = ((char)27).ToString() + "[" + Row.ToString("D2") + ";" + Column.ToString("D2") + "H";

                byte[] data = Encoding.ASCII.GetBytes(strline);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error position cursor to client {ip}");

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
                _logger.Warning($"BeepAsync - Client not found: {ip}");
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
                _logger.Error(ex, $"Error Beep to client {ip}");

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
                _logger.Error(ex, "TedVT100 - BeepOffAsync");
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
                _logger.Error(ex, "TedVT100 - BeepOnAsync");
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
                _logger.Warning($"EnabledCOM1Async - Client not found: {ip}");
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
                _logger.Warning($"EnabledCOM2Async - Client not found: {ip}");
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

        /// <summary>
        /// Linha do Cursor
        /// </summary>
        public int Row { get; set; } = 1;

        /// <summary>
        /// Coluna Cursor
        /// </summary>
        public int Column { get; set; } = 1;

    }
}
