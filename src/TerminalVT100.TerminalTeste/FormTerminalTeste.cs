using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TerminalVT100.TerminalTeste
{
    public partial class FormTerminalTeste : Form
    {
        private TedVT100Server TedVT100Service;
        private List<string> _ips;
        private ILogger _logger;

        public FormTerminalTeste()
        {
            InitializeComponent();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            var fileFull = Path.Combine(path, "terminal-teste.log");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

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

        private void BtnConectar_Click(object sender, EventArgs e)
        {
            TxtError.Text = string.Empty;
            richTextBox1.Text = string.Empty;

            BtnConectar.Enabled = false;
            BtnDesconectar.Enabled = true;

            _ips = new List<string>();
            var port = Convert.ToInt32(TxtPorta.Text);

            TedVT100Service = new TedVT100Server();
            TedVT100Service.ClientDataReceived += TedVT100Service_ClientDataReceived;
            TedVT100Service.ClientConnected += TedVT100Service_ClientConnected;
            TedVT100Service.Start(port);
        }

        private void TedVT100Service_ClientConnected(string ip)
        {
            TedVT100Service.ClearDisplay(ip);
            TedVT100Service.SaveLog(ip,"Conectado", TypeLog.Info);
            if (!_ips.Where(x => x == ip).Any())
            {
                Thread.Sleep(300);
                _ips.Add(ip);
                this.Invoke((MethodInvoker)delegate ()
                {
                    try
                    {
                        CboTerminais.DataSource = _ips;
                        CboTerminais.Refresh();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "TedVT100Service_ClientDataReceived");
                    }
                });
            }
        }

        private void TedVT100Service_ClientDataReceived(string ip, string data)
        {

            var str = $"Recebido: [{ip}] - Conteudo [{data}]";
            ReceiveMessage(str);
        }

        private void ReceiveMessage(string data)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                try
                {
                    richTextBox1.Text = $"{data}\n" + richTextBox1.Text;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "ReceiveMessage-1");
                }
            });
        }

        private void BtnClearDisplay_Click(object sender, EventArgs e)
        {
            try
            {
                if (CboTerminais.SelectedIndex < 0)
                {
                    return;
                }

                var ip = _ips[CboTerminais.SelectedIndex];

                TedVT100Service.ClearDisplay(ip);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "BtnClearDisplay_Click");
                TxtError.Text = $"BtnClearDisplay_Click - [{ex.Message}]"; ;
            }
        }

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (CboTerminais.SelectedIndex < 0)
                {
                    return;
                }

                if (string.IsNullOrEmpty(TxtMensagem.Text))
                {
                    MessageBox.Show("Mensagem Obrigatorio", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TxtMensagem.Text = string.Empty;
                    TxtMensagem.Focus();
                    return;
                }

                var ip = _ips[CboTerminais.SelectedIndex];
                var linha = Convert.ToInt32(TxtLinha.Value);
                TedVT100Service.PositionCursor(ip, linha);
                TedVT100Service.SendMessage(ip, TxtMensagem.Text);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "BtnClearDisplay_Click");
                TxtError.Text = $"BtnClearDisplay_Click - [{ex.Message}]"; ;
            }
        }

        private void BtnDesconectar_Click(object sender, EventArgs e)
        {
            BtnDesconectar.Enabled = false;
            BtnConectar.Enabled = true;

            TedVT100Service.Stop();
            TedVT100Service.Dispose();
            BtnConectar.Focus();
            CboTerminais.DataSource = null;
        }

        private void TxtMensagem_TextChanged(object sender, EventArgs e)
        {
            LblCount.Text = TxtMensagem.Text.Length.ToString();
        }

        private void BtnBeep_Click(object sender, EventArgs e)
        {
            try
            {
                if (CboTerminais.SelectedIndex < 0)
                {
                    return;
                }

                var ip = _ips[CboTerminais.SelectedIndex];
                var timeOut = Convert.ToInt32(textBox1.Text);

                //TedVT100Service.Beep(ip, timeOut);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "BtnBeep_Click");
                TxtError.Text = $"BtnBeep_Click - [{ex.Message}]"; ;
            }
        }

        private void FormTerminalTeste_Load(object sender, EventArgs e)
        {
            BtnConectar_Click(sender, e);
        }
    }
}
