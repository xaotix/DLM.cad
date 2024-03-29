﻿using Conexoes;
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
    public partial class TercasMenu : Form
    {
        public TercasMenu()
        {
            InitializeComponent();
            DBases.GetBancoRM().GetPurlins();
            this.Text = $"xPurlin V.{Conexoes.Utilz.GetVersao(Cfg.Init.CAD_DLL_Local)} [{DLM.vars.Cfg_User.Init.MySQL_Servidor}]";
        }
        public string acao { get; set; } = "";
        public int id_terca { get; set; } = 1763;
        public int id_corrente { get; set; } = 27;
        public int id_tirante { get; set; } = 1407;
        private void button1_Click(object sender, EventArgs e)
        {

            if (correntes_mlstyles.Items.Count==0&& mapeia_correntes.Checked)
            {
                "Não é possivel mapear as correntes sem ter um estilo de MLinha".Alerta();
                return;
            }

            if (tirantes_mlstyles.Items.Count== 0 &&  mapeia_tirantes.Checked)
            {
                "Não é possivel mapear os tirantes sem ter um estilo de MLinha".Alerta();
                return;
            }

            if (tercas_mlstyles.Items.Count == 0 && mapeia_tercas.Checked)
            {
                "Não é possivel mapear os tercas sem ter um estilo de MLinha".Alerta();
                return;
            }
            if (furos_manuais_layer.Text == "" && mapeia_furos_manuais.Checked)
            {
                "Não é possivel mapear os furos manuais sem definir uma layer padrão".Alerta();
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
            var s = Ut.SelecionarPurlin(null);
            if (s!=null)
            {
                this.id_terca = s.id_codigo;
                this.terca.Text = s.COD_DB;
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

        private void button16_Click(object sender, EventArgs e)
        {
            acao = "gerarcroqui";
            this.Close();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var s = Ut.SelecionarCorrente();
            if (s != null)
            {
                this.id_corrente = s.id_codigo;
                this.corrente.Text = s.COD_DB;
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            var s = Ut.SelecionarTirante();
            if (s != null)
            {
                this.id_tirante = s.id_codigo;
                this.tirante.Text = s.COD_DB;
            }
        }
    }
}
