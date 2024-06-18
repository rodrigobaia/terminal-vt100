namespace TerminalVT100.TerminalTeste
{
    partial class Form1
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
            this.BtnConectar = new System.Windows.Forms.Button();
            this.BtnDesconectar = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.CboTerminais = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TxtPorta = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnConectar
            // 
            this.BtnConectar.Location = new System.Drawing.Point(149, 21);
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
            this.BtnDesconectar.Location = new System.Drawing.Point(279, 21);
            this.BtnDesconectar.Name = "BtnDesconectar";
            this.BtnDesconectar.Size = new System.Drawing.Size(108, 56);
            this.BtnDesconectar.TabIndex = 1;
            this.BtnDesconectar.Text = "Desconectar";
            this.BtnDesconectar.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(69, 347);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 56);
            this.button1.TabIndex = 2;
            this.button1.Text = "Conectar";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // CboTerminais
            // 
            this.CboTerminais.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CboTerminais.FormattingEnabled = true;
            this.CboTerminais.Location = new System.Drawing.Point(47, 248);
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
            this.groupBox1.Location = new System.Drawing.Point(22, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 102);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Servidor";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(896, 537);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CboTerminais);
            this.Controls.Add(this.button1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Testa Servidor TCP - VT100";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnConectar;
        private System.Windows.Forms.Button BtnDesconectar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox CboTerminais;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtPorta;
    }
}

