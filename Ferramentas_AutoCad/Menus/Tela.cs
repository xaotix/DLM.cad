using DLM.vars;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DLM.cad
{
    public partial class Tela : Form
    {
        public CADCotagem mm { get; set; }
        public Tela(CADCotagem mm)
        {
            this.mm = mm;
            InitializeComponent();
            this.Text = $"Cotagem V." + Conexoes.Utilz.GetVersao(Cfg.Init.CAD_DLL_Local) + $" [{DLM.vars.Cfg_User.Init.MySQL_Servidor}]";
            opcoes.SelectedObject = this.mm;
            getvars(this.mm);

        }

        private void getvars(CADCotagem mm)
        {
            if (mm.Tipo_Desenho.StartsWith("V") | mm.Tipo_Desenho == "")
            {
                this.tipo_desenho_vista.Checked = true;
                this.tipo_desenho_corte.Checked = false;
            }
            else
            {
                this.tipo_desenho_vista.Checked = false;
                this.tipo_desenho_corte.Checked = true;
            }

            base_esquerda.Checked = mm.Base_Esquerda;
            base_direita.Checked = mm.Base_Direita;

            cotar_emabaixo.Checked = mm.Cotar_Embaixo;
            cotar_emcima.Checked = mm.Cotar_Em_Cima;

            acumuladas_cima.Checked = mm.Acumuladas_Cima;
            acumuladas_embaixo.Checked = mm.Acumuladas_Baixo;

            cotas_esquerda.Checked = mm.Cotar_Esquerda;
            cotas_direita.Checked = mm.Cotar_Direita;

            profundidade_direita.Value = (decimal)mm.Profundidade_Direita;
            profundidade_esquerda.Value = (decimal)mm.Profundidade_Direita;
        }

        private void Tela_Load(object sender, EventArgs e)
        {
         //   this.Text = "Medabil Cotagem " + " v.1.0.2 - 07/02/2020";
        }

        private void Tela_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetVars();
        }

        private void SetVars()
        {
            if (this.tipo_desenho_corte.Checked)
            {
                mm.Tipo_Desenho = "Corte";
            }
            else if (this.tipo_desenho_vista.Checked)
            {
                mm.Tipo_Desenho = "Vista";
            }

            mm.Base_Direita = base_direita.Checked;
            mm.Base_Esquerda = base_esquerda.Checked;


            mm.Cotar_Embaixo = cotar_emabaixo.Checked;
            
            mm.Acumuladas_Cima = acumuladas_cima.Checked;
            mm.Acumuladas_Baixo = acumuladas_embaixo.Checked;
            mm.Cotar_Em_Cima = cotar_emcima.Checked;
            mm.Cotar_Direita = cotas_direita.Checked;
            mm.Cotar_Esquerda = cotas_esquerda.Checked;

            mm.Profundidade_Direita = (double)this.profundidade_direita.Value;
            mm.Profundidade_Esquerda = (double)this.profundidade_esquerda.Value;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
