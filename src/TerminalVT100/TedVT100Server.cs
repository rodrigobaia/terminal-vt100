using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TerminalVT100
{
    /// <summary>
    /// Servidor Terminal VT 100
    /// </summary>
    public class TedVT100Server: IDisposable
    {
        private ILogger _logger;

        private TcpListener server;
        private BackgroundWorker backgroundWorker;
        private int _portNumber;

        private Dictionary<string, NetworkStream> terminais;

        public delegate void ClientDataReceivedEventHandler(string ip, string data);
        public event ClientDataReceivedEventHandler ClientDataReceived;
        private bool disposedValue;

        public TedVT100Server()
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

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartServer();
        }

        /// <summary>
        /// Iniciar Servidor
        /// </summary>
        private async void StartServer()
        {
            IPAddress localAddr = IPAddress.Any;

            server = new TcpListener(localAddr, _portNumber);
            server.Start();

            while (!backgroundWorker.CancellationPending)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    // Processar a conexão em uma nova tarefa
                    Task.Run(() => HandleClient(client));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "TedVT100 - BackgroundWorker_DoWork");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            /* Obtendo o IP do cliente */
            IPEndPoint ipEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            var ip = ipEndPoint.Address.ToString();

            if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
            {
                terminais.Add(ip, networkStream);
            }

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int i;
            string entrada = "";

            while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string data = null;
                try
                {
                    data = System.Text.Encoding.ASCII.GetString(buffer, 0, i);

                    if (data.Contains("\r"))
                    {
                        data = data.Replace("\r", "");
                        entrada += data;
                        break;
                    }
                    else if (data.Length > 2)
                    {
                        entrada += data;
                    }
                    else if (Convert.ToChar(data) != Convert.ToChar(13))
                    {
                        entrada += data;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "TedVT100 - HandleClient");
                }
            }

            // Disparar o evento com os dados recebidos
            OnClientDataReceived(ip, entrada);
        }

        protected virtual void OnClientDataReceived(string ip, string data)
        {
            ClientDataReceived?.Invoke(ip, data);
        }

        /// <summary>
        /// Iniciar o Servidor
        /// </summary>
        /// <param name="portNumber">Número da Porta de Acesso</param>
        public void Start(int portNumber = 1001)
        {
            try
            {
                terminais = new Dictionary<string, NetworkStream>();
                _portNumber = portNumber;
                // Inicia o BackgroundWorker
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - Start");
                throw;
            }

        }

        /// <summary>
        /// Parar Servidor
        /// </summary>
        public void Stop()
        {
            try
            {
                // Cancela o BackgroundWorker
                if (backgroundWorker.IsBusy)
                {
                    backgroundWorker.CancelAsync();
                }

                // Para o servidor TCP
                server.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - Stop");
                throw;
            }
        }

        /// <summary>
        /// Limpar Display
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        public void ClearDisplay(string ip)
        {
            try
            {
                if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
                {
                    _logger.Warning($"ClearDisplay - Terminal not found - [{ip}].");
                    return;
                }

                string cmdClear = Convert.ToChar(27) + "[?24h" + Convert.ToChar(27) + "[5i" + "#27'[H'#27'[J'" + Convert.ToChar(27) + "[4i";
                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(cmdClear);
                networkStream.Write(msgs, 0, cmdClear.Length);
                networkStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - ClearDisplay");
            }
        }

        /// <summary>
        /// Enviar messagem para o Terminal
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="msg">Mensagem a ser enviada</param>
        public void SendMessage(string ip, string msg)
        {
            try
            {
                if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
                {
                    _logger.Warning($"SendMessage - Terminal not found - [{ip}].");
                    return;
                }

                var comando = ((char)2).ToString() + 'D' + msg + ((char)3).ToString();

                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(comando);
                networkStream.Write(msgs, 0, msgs.Length);
                networkStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - SendMessage");
            }
        }

        /// <summary>
        /// Envar Beep
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        /// <param name="timeBeep">Tempo do Beep em milisegundos</param>
        public void Beep(string ip, int timeBeep = 200)
        {
            try
            {
                if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
                {
                    _logger.Warning($"Beep - Terminal not found - [{ip}].");
                    return;
                }

                BeepOn(networkStream);
                Thread.Sleep(timeBeep);
                BeepOff(networkStream);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - Beep");
            }
        }

        /// <summary>
        /// Ligar Beep
        /// </summary>
        /// <param name="networkStream"></param>
        private void BeepOn(NetworkStream networkStream)
        {
            try
            {
                // Buzzer
                var acionamentoChar = (char)7;
                var comando = $"{(char)27}[?24c{(char)27}[5i{acionamentoChar}{(char)27}[4i";
                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(comando);
                networkStream.Write(msgs, 0, msgs.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - BeepOn");
            }
        }

        /// <summary>
        /// Desliga Beep
        /// </summary>
        /// <param name="networkStream"></param>
        private void BeepOff(NetworkStream networkStream)
        {
            try
            {
                var acionamentoChar = (char)11;
                var comando = $"{(char)27}[?24c{(char)27}[5i{acionamentoChar}{(char)27}[4i";
                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(comando);
                networkStream.Write(msgs, 0, msgs.Length);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - BeepOff");
            }
        }

        /// <summary>
        /// Habilitar porta COM 1
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        public void EnabledCOM1(string ip)
        {
            try
            {
                if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
                {
                    _logger.Warning($"EnabledCOM1 - Terminal not found - [{ip}].");
                    return;
                }
                var comando = ((char)13).ToString() + ((char)10).ToString();
                var msg = ((char)27).ToString() + "[?24r" + ((char)27).ToString() + "[5i" + comando + ((char)27).ToString() + "[4i";
                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(msg);
                networkStream.Write(msgs, 0, msg.Length);
                networkStream.Dispose();

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - EnabledCOM1");
            }
        }

        /// <summary>
        /// Habilitar porta COM 1
        /// </summary>
        /// <param name="ip">IP do Terminal</param>
        public void EnabledCOM2(string ip)
        {
            try
            {
                if (!terminais.TryGetValue(ip, out NetworkStream networkStream))
                {
                    _logger.Warning($"EnabledCOM2 - Terminal not found - [{ip}].");
                    return;
                }

                var comando = ((char)13).ToString() + ((char)10).ToString();
                var msg = ((char)27).ToString() + "[?24h" + ((char)27).ToString() + "[5i" + comando + ((char)27).ToString() + "[4i";
                byte[] msgs = System.Text.Encoding.ASCII.GetBytes(msg);
                networkStream.Write(msgs, 0, msg.Length);
                networkStream.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "TedVT100 - EnabledCOM2");
            }
        }

        /// <summary>
        /// Buscar IpCorrente
        /// </summary>
        /// <returns></returns>
        public string BuscarIpCorrente()
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
        /// Buscando Geateway padrão
        /// </summary>
        /// <returns></returns>
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
        /// Buscando todos endereços de IP Servidor
        /// </summary>
        /// <returns></returns>
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
        /// Tem conexão com a Internet
        /// </summary>
        /// <param name="ipAddress">Endereço de IP</param>
        /// <returns></returns>
        private bool IsConnectedToInternet(string ipAddress)
        {
            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = ping.Send(ipAddress, 1000);
                    return (reply.Status == IPStatus.Success);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch (PingException)
                {
                    // Lidar com exceção de ping
                }

                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    server?.Stop();
                    backgroundWorker?.Dispose();
                    foreach (var stream in terminais.Values)
                    {
                        stream.Dispose();
                    }
                    terminais.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
