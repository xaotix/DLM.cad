namespace DLM.cad
{
    partial class Tela
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.profundidade_direita = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.profundidade_esquerda = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cotas_direita = new System.Windows.Forms.CheckBox();
            this.cotas_esquerda = new System.Windows.Forms.CheckBox();
            this.cotar_emcima = new System.Windows.Forms.CheckBox();
            this.cotar_emabaixo = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.acumuladas_cima = new System.Windows.Forms.CheckBox();
            this.acumuladas_embaixo = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.base_direita = new System.Windows.Forms.CheckBox();
            this.base_esquerda = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tipo_desenho_corte = new System.Windows.Forms.RadioButton();
            this.tipo_desenho_vista = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.opcoes = new System.Windows.Forms.PropertyGrid();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.profundidade_direita)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.profundidade_esquerda)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(270, 441);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tabControl2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(262, 415);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Ferramentas";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Location = new System.Drawing.Point(6, 6);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(249, 401);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Controls.Add(this.groupBox4);
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(241, 375);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Cotagem";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.profundidade_direita);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.profundidade_esquerda);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Location = new System.Drawing.Point(6, 233);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(229, 75);
            this.groupBox5.TabIndex = 7;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Profundidade";
            // 
            // profundidade_direita
            // 
            this.profundidade_direita.Location = new System.Drawing.Point(76, 47);
            this.profundidade_direita.Maximum = new decimal(new int[] {
            35000,
            0,
            0,
            0});
            this.profundidade_direita.Name = "profundidade_direita";
            this.profundidade_direita.Size = new System.Drawing.Size(145, 20);
            this.profundidade_direita.TabIndex = 3;
            this.profundidade_direita.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Direita";
            // 
            // profundidade_esquerda
            // 
            this.profundidade_esquerda.Location = new System.Drawing.Point(76, 21);
            this.profundidade_esquerda.Maximum = new decimal(new int[] {
            35000,
            0,
            0,
            0});
            this.profundidade_esquerda.Name = "profundidade_esquerda";
            this.profundidade_esquerda.Size = new System.Drawing.Size(145, 20);
            this.profundidade_esquerda.TabIndex = 1;
            this.profundidade_esquerda.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Esquerda";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.cotas_direita);
            this.groupBox4.Controls.Add(this.cotas_esquerda);
            this.groupBox4.Controls.Add(this.cotar_emcima);
            this.groupBox4.Controls.Add(this.cotar_emabaixo);
            this.groupBox4.Location = new System.Drawing.Point(6, 111);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(229, 66);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Cotas";
            // 
            // cotas_direita
            // 
            this.cotas_direita.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.cotas_direita.AutoSize = true;
            this.cotas_direita.Location = new System.Drawing.Point(91, 42);
            this.cotas_direita.Name = "cotas_direita";
            this.cotas_direita.Size = new System.Drawing.Size(68, 17);
            this.cotas_direita.TabIndex = 7;
            this.cotas_direita.Text = "Direita ->";
            this.cotas_direita.UseVisualStyleBackColor = true;
            // 
            // cotas_esquerda
            // 
            this.cotas_esquerda.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.cotas_esquerda.AutoSize = true;
            this.cotas_esquerda.Checked = true;
            this.cotas_esquerda.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cotas_esquerda.Location = new System.Drawing.Point(91, 19);
            this.cotas_esquerda.Name = "cotas_esquerda";
            this.cotas_esquerda.Size = new System.Drawing.Size(83, 17);
            this.cotas_esquerda.TabIndex = 6;
            this.cotas_esquerda.Text = "<- Esquerda";
            this.cotas_esquerda.UseVisualStyleBackColor = true;
            // 
            // cotar_emcima
            // 
            this.cotar_emcima.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.cotar_emcima.AutoSize = true;
            this.cotar_emcima.Checked = true;
            this.cotar_emcima.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cotar_emcima.Location = new System.Drawing.Point(13, 19);
            this.cotar_emcima.Name = "cotar_emcima";
            this.cotar_emcima.Size = new System.Drawing.Size(49, 17);
            this.cotar_emcima.TabIndex = 5;
            this.cotar_emcima.Text = "Cima";
            this.cotar_emcima.UseVisualStyleBackColor = true;
            // 
            // cotar_emabaixo
            // 
            this.cotar_emabaixo.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.cotar_emabaixo.AutoSize = true;
            this.cotar_emabaixo.Location = new System.Drawing.Point(13, 42);
            this.cotar_emabaixo.Name = "cotar_emabaixo";
            this.cotar_emabaixo.Size = new System.Drawing.Size(52, 17);
            this.cotar_emabaixo.TabIndex = 4;
            this.cotar_emabaixo.Text = "Baixo";
            this.cotar_emabaixo.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.acumuladas_cima);
            this.groupBox3.Controls.Add(this.acumuladas_embaixo);
            this.groupBox3.Location = new System.Drawing.Point(6, 183);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(229, 44);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cotas Acumuladas";
            // 
            // acumuladas_cima
            // 
            this.acumuladas_cima.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.acumuladas_cima.AutoSize = true;
            this.acumuladas_cima.Location = new System.Drawing.Point(13, 19);
            this.acumuladas_cima.Name = "acumuladas_cima";
            this.acumuladas_cima.Size = new System.Drawing.Size(49, 17);
            this.acumuladas_cima.TabIndex = 5;
            this.acumuladas_cima.Text = "Cima";
            this.acumuladas_cima.UseVisualStyleBackColor = true;
            // 
            // acumuladas_embaixo
            // 
            this.acumuladas_embaixo.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.acumuladas_embaixo.AutoSize = true;
            this.acumuladas_embaixo.Location = new System.Drawing.Point(94, 19);
            this.acumuladas_embaixo.Name = "acumuladas_embaixo";
            this.acumuladas_embaixo.Size = new System.Drawing.Size(52, 17);
            this.acumuladas_embaixo.TabIndex = 6;
            this.acumuladas_embaixo.Text = "Baixo";
            this.acumuladas_embaixo.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(4, 317);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(223, 52);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cotar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.base_direita);
            this.groupBox2.Controls.Add(this.base_esquerda);
            this.groupBox2.Location = new System.Drawing.Point(6, 55);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(229, 50);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Placa Base";
            // 
            // base_direita
            // 
            this.base_direita.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.base_direita.AutoSize = true;
            this.base_direita.Checked = true;
            this.base_direita.CheckState = System.Windows.Forms.CheckState.Checked;
            this.base_direita.Location = new System.Drawing.Point(91, 19);
            this.base_direita.Name = "base_direita";
            this.base_direita.Size = new System.Drawing.Size(68, 17);
            this.base_direita.TabIndex = 9;
            this.base_direita.Text = "Direita ->";
            this.base_direita.UseVisualStyleBackColor = true;
            // 
            // base_esquerda
            // 
            this.base_esquerda.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.base_esquerda.AutoSize = true;
            this.base_esquerda.Checked = true;
            this.base_esquerda.CheckState = System.Windows.Forms.CheckState.Checked;
            this.base_esquerda.Location = new System.Drawing.Point(6, 19);
            this.base_esquerda.Name = "base_esquerda";
            this.base_esquerda.Size = new System.Drawing.Size(83, 17);
            this.base_esquerda.TabIndex = 8;
            this.base_esquerda.Text = "<- Esquerda";
            this.base_esquerda.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tipo_desenho_corte);
            this.groupBox1.Controls.Add(this.tipo_desenho_vista);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 46);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tipo de Desenho";
            // 
            // tipo_desenho_corte
            // 
            this.tipo_desenho_corte.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.tipo_desenho_corte.AutoSize = true;
            this.tipo_desenho_corte.Location = new System.Drawing.Point(61, 20);
            this.tipo_desenho_corte.Name = "tipo_desenho_corte";
            this.tipo_desenho_corte.Size = new System.Drawing.Size(50, 17);
            this.tipo_desenho_corte.TabIndex = 1;
            this.tipo_desenho_corte.Text = "Corte";
            this.tipo_desenho_corte.UseVisualStyleBackColor = true;
            // 
            // tipo_desenho_vista
            // 
            this.tipo_desenho_vista.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.tipo_desenho_vista.AutoSize = true;
            this.tipo_desenho_vista.Checked = true;
            this.tipo_desenho_vista.Location = new System.Drawing.Point(7, 20);
            this.tipo_desenho_vista.Name = "tipo_desenho_vista";
            this.tipo_desenho_vista.Size = new System.Drawing.Size(48, 17);
            this.tipo_desenho_vista.TabIndex = 0;
            this.tipo_desenho_vista.TabStop = true;
            this.tipo_desenho_vista.Text = "Vista";
            this.tipo_desenho_vista.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.opcoes);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(262, 415);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Configurações";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // opcoes
            // 
            this.opcoes.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.opcoes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.opcoes.CommandsVisibleIfAvailable = false;
            this.opcoes.HelpVisible = false;
            this.opcoes.Location = new System.Drawing.Point(6, 6);
            this.opcoes.Name = "opcoes";
            this.opcoes.Size = new System.Drawing.Size(250, 401);
            this.opcoes.TabIndex = 1;
            // 
            // Tela
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(271, 442);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Tela";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ferramentas AutoCAD";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Tela_FormClosing);
            this.Load += new System.EventHandler(this.Tela_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.profundidade_direita)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.profundidade_esquerda)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.PropertyGrid opcoes;
        public System.Windows.Forms.RadioButton tipo_desenho_vista;
        public System.Windows.Forms.RadioButton tipo_desenho_corte;
        public System.Windows.Forms.CheckBox cotar_emabaixo;
        public System.Windows.Forms.CheckBox acumuladas_cima;
        public System.Windows.Forms.CheckBox acumuladas_embaixo;
        private System.Windows.Forms.GroupBox groupBox4;
        public System.Windows.Forms.CheckBox cotar_emcima;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.CheckBox cotas_direita;
        public System.Windows.Forms.CheckBox cotas_esquerda;
        public System.Windows.Forms.CheckBox base_direita;
        public System.Windows.Forms.CheckBox base_esquerda;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.NumericUpDown profundidade_esquerda;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown profundidade_direita;
        private System.Windows.Forms.Label label2;
    }
}