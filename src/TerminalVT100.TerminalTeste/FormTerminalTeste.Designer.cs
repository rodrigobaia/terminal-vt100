namespace TerminalVT100.TerminalTeste
{
    partial class FormTerminalTeste
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTerminalTeste));
            this.BtnConectar = new System.Windows.Forms.Button();
            this.BtnDesconectar = new System.Windows.Forms.Button();
            this.BtnClearDisplay = new System.Windows.Forms.Button();
            this.CboTerminais = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtPorta = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TxtMensagem = new System.Windows.Forms.TextBox();
            this.BtnSendMessage = new System.Windows.Forms.Button();
            this.TxtError = new System.Windows.Forms.TextBox();
            this.LblCount = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnConectar
            // 
            this.BtnConectar.Location = new System.Drawing.Point(191, 21);
            this.BtnConectar.Name = "BtnConectar";
            this.BtnConectar.Size = new System.Drawing.Size(108, 56);
            this.BtnConectar.TabIndex = 0;
            this.BtnConectar.Text = "Conectar";
            this.BtnConectar.UseVisualStyleBackColor = true;
            this.BtnConectar.Click += new System.EventHandler(this.BtnConectar_Click);
            // 
            // BtnDesconectar
            // 
            this.BtnDesconectar.Enabled = false;
            this.BtnDesconectar.Location = new System.Drawing.Point(320, 21);
            this.BtnDesconectar.Name = "BtnDesconectar";
            this.BtnDesconectar.Size = new System.Drawing.Size(108, 56);
            this.BtnDesconectar.TabIndex = 1;
            this.BtnDesconectar.Text = "Desconectar";
            this.BtnDesconectar.UseVisualStyleBackColor = true;
            this.BtnDesconectar.Click += new System.EventHandler(this.BtnDesconectar_Click);
            // 
            // BtnClearDisplay
            // 
            this.BtnClearDisplay.Location = new System.Drawing.Point(341, 257);
            this.BtnClearDisplay.Name = "BtnClearDisplay";
            this.BtnClearDisplay.Size = new System.Drawing.Size(131, 43);
            this.BtnClearDisplay.TabIndex = 2;
            this.BtnClearDisplay.Text = "Clear Display";
            this.BtnClearDisplay.UseVisualStyleBackColor = true;
            this.BtnClearDisplay.Click += new System.EventHandler(this.BtnClearDisplay_Click);
            // 
            // CboTerminais
            // 
            this.CboTerminais.BackColor = System.Drawing.SystemColors.Info;
            this.CboTerminais.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CboTerminais.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboTerminais.FormattingEnabled = true;
            this.CboTerminais.Location = new System.Drawing.Point(21, 267);
            this.CboTerminais.Name = "CboTerminais";
            this.CboTerminais.Size = new System.Drawing.Size(299, 33);
            this.CboTerminais.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.TxtPorta);
            this.groupBox1.Controls.Add(this.BtnConectar);
            this.groupBox1.Controls.Add(this.BtnDesconectar);
            this.groupBox1.Location = new System.Drawing.Point(21, 130);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(451, 102);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Servidor";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Porta :";
            // 
            // TxtPorta
            // 
            this.TxtPorta.BackColor = System.Drawing.SystemColors.Info;
            this.TxtPorta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPorta.Location = new System.Drawing.Point(25, 47);
            this.TxtPorta.MaxLength = 5;
            this.TxtPorta.Name = "TxtPorta";
            this.TxtPorta.Size = new System.Drawing.Size(96, 30);
            this.TxtPorta.TabIndex = 2;
            this.TxtPorta.Text = "1001";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 244);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Terminal (is) :";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(499, 104);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(339, 323);
            this.richTextBox1.TabIndex = 30;
            this.richTextBox1.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(496, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 16);
            this.label3.TabIndex = 29;
            this.label3.Text = "Log de Conexão:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(21, 317);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 20);
            this.label4.TabIndex = 32;
            this.label4.Text = "Mensagem :";
            // 
            // TxtMensagem
            // 
            this.TxtMensagem.BackColor = System.Drawing.SystemColors.Info;
            this.TxtMensagem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtMensagem.Location = new System.Drawing.Point(21, 343);
            this.TxtMensagem.MaxLength = 200;
            this.TxtMensagem.Name = "TxtMensagem";
            this.TxtMensagem.Size = new System.Drawing.Size(299, 30);
            this.TxtMensagem.TabIndex = 31;
            this.TxtMensagem.Text = "Enviar mensagem terminal";
            this.TxtMensagem.TextChanged += new System.EventHandler(this.TxtMensagem_TextChanged);
            // 
            // BtnSendMessage
            // 
            this.BtnSendMessage.Location = new System.Drawing.Point(341, 330);
            this.BtnSendMessage.Name = "BtnSendMessage";
            this.BtnSendMessage.Size = new System.Drawing.Size(131, 43);
            this.BtnSendMessage.TabIndex = 33;
            this.BtnSendMessage.Text = "Send Message";
            this.BtnSendMessage.UseVisualStyleBackColor = true;
            this.BtnSendMessage.Click += new System.EventHandler(this.BtnSendMessage_Click);
            // 
            // TxtError
            // 
            this.TxtError.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TxtError.ForeColor = System.Drawing.Color.Red;
            this.TxtError.Location = new System.Drawing.Point(0, 438);
            this.TxtError.Multiline = true;
            this.TxtError.Name = "TxtError";
            this.TxtError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtError.Size = new System.Drawing.Size(850, 134);
            this.TxtError.TabIndex = 34;
            // 
            // LblCount
            // 
            this.LblCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblCount.Location = new System.Drawing.Point(229, 376);
            this.LblCount.Name = "LblCount";
            this.LblCount.Size = new System.Drawing.Size(91, 25);
            this.LblCount.TabIndex = 35;
            this.LblCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::TerminalVT100.TerminalTeste.Properties.Resources.chip_128x128;
            this.pictureBox1.Location = new System.Drawing.Point(21, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(121, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 36;
            this.pictureBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(-6, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(850, 53);
            this.label5.TabIndex = 37;
            this.label5.Text = "TESTA TERMINAL TCP - VT100";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(13, 407);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 20);
            this.label6.TabIndex = 38;
            this.label6.Text = "Error :";
            // 
            // FormTerminalTeste
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 572);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LblCount);
            this.Controls.Add(this.TxtError);
            this.Controls.Add(this.BtnSendMessage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TxtMensagem);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CboTerminais);
            this.Controls.Add(this.BtnClearDisplay);
            this.Controls.Add(this.label5);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormTerminalTeste";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Testa Servidor TCP - VT100";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnConectar;
        private System.Windows.Forms.Button BtnDesconectar;
        private System.Windows.Forms.Button BtnClearDisplay;
        private System.Windows.Forms.ComboBox CboTerminais;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtPorta;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TxtMensagem;
        private System.Windows.Forms.Button BtnSendMessage;
        private System.Windows.Forms.TextBox TxtError;
        private System.Windows.Forms.Label LblCount;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}

