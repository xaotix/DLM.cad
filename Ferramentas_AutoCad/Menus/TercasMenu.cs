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
    public partial class TercasMenu : Form
    {
        public TercasMenu()
        {
            InitializeComponent();
        }
        public string acao { get; set; } = "";
        public int id_terca { get; set; } = 1763;
        private void button1_Click(object sender, EventArgs e)
        {

            if (correntes_mlstyle.Text == "" && mapeia_correntes.Checked)
            {
                Utilidades.Alerta("Não é possivel mapear as correntes sem definir um estilo de MLinha");
                return;
            }

            if (tirantes_mlstyle.Text== "" &&  mapeia_tirantes.Checked)
            {
                Utilidades.Alerta("Não é possivel mapear os tirantes sem definir um estilo de MLinha");
                return;
            }

            if (tercas_mlstyles.Items.Count == 0 && mapeia_tercas.Checked)
            {
                Utilidades.Alerta("Não é possivel mapear os tercas sem definir um estilo de MLinha");
                return;
            }
            if (furos_manuais_layer.Text == "" && mapeia_furos_manuais.Checked)
            {
                Utilidades.Alerta("Não é possivel mapear os furos manuais sem definir uma layer padrão");
                return;
            }

            acao = "mapear";
            this.Close();
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            acao = "perfil";

            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            acao = "transpasse";
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            acao = "ficha";
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            acao = "tabela";
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            acao = "exportar";
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            acao = "ver";
            this.Close();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            acao = "ver";
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            acao = "fixacao";

            this.Close();
        }

        private void TercasMenu_Load(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            var s = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetTercas(),null,"Selecione");
            if(s!=null)
            {
                this.id_terca = s.id_db;
                this.terca.Text = s.TIPO;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            acao = "troca_corrente";

            this.Close();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            acao = "descontar_corrente";

            this.Close();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            acao = "fixador_corrente";

            this.Close();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            acao = "excluir";

            this.Close();
        }

        private void button14_Click(object sender, EventArgs e)
        {

            acao = "marcacao_purlin";

            this.Close();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            acao = "purlin_edicao_completa";

            this.Close();
        }
    }
}
