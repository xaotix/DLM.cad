using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ferramentas_DLM
{
    public partial class Tela : Form
    {
        public Cotagem mm { get; set; }
        public Tela(Cotagem mm)
        {
            this.mm = mm;
            InitializeComponent();
            this.Text = $"Cotagem V." + Conexoes.Utilz.GetVersao(Constantes.DLL_Local);
            opcoes.SelectedObject = this.mm;
            getvars(this.mm);

        }

        private void getvars(Cotagem mm)
        {
            if (mm.tipo_desenho.StartsWith("V") | mm.tipo_desenho == "")
            {
                this.tipo_desenho_vista.Checked = true;
                this.tipo_desenho_corte.Checked = false;
            }
            else
            {
                this.tipo_desenho_vista.Checked = false;
                this.tipo_desenho_corte.Checked = true;
            }

            base_esquerda.Checked = mm.base_esquerda;
            base_direita.Checked = mm.base_direita;

            cotar_emabaixo.Checked = mm.cotar_embaixo;
            cotar_emcima.Checked = mm.cotar_emcima;

            acumuladas_cima.Checked = mm.acumuladas_cima;
            acumuladas_embaixo.Checked = mm.acumuladas_baixo;

            cotas_esquerda.Checked = mm.cotar_esquerda;
            cotas_direita.Checked = mm.cotar_direita;

            profundidade_direita.Value = (decimal)mm.profundidade_direita;
            profundidade_esquerda.Value = (decimal)mm.profundidade_direita;
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
                mm.tipo_desenho = "Corte";
            }
            else if (this.tipo_desenho_vista.Checked)
            {
                mm.tipo_desenho = "Vista";
            }

            mm.base_direita = base_direita.Checked;
            mm.base_esquerda = base_esquerda.Checked;


            mm.cotar_embaixo = cotar_emabaixo.Checked;
            
            mm.acumuladas_cima = acumuladas_cima.Checked;
            mm.acumuladas_baixo = acumuladas_embaixo.Checked;
            mm.cotar_emcima = cotar_emcima.Checked;
            mm.cotar_direita = cotas_direita.Checked;
            mm.cotar_esquerda = cotas_esquerda.Checked;

            mm.profundidade_direita = (double)this.profundidade_direita.Value;
            mm.profundidade_esquerda = (double)this.profundidade_esquerda.Value;

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
