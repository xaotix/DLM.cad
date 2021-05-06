using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferramentas_DLM.Menus
{
    /// <summary>
    /// Interação lógica para Menu_Bloco_Peca.xam
    /// </summary>
    public partial class Menu_Bloco_Peca : Window
    {
        public double escala { get; set; } = 0;
        public double comp { get; set; } = 0;
        public double qtd { get; set; } = 1;

        public Conexoes.RMA rma_sel { get; set; }
        public Conexoes.RME rme_sel { get; set; }
        public Conexoes.RMU rmu_sel { get; set; }
        public Conexoes.RMT rmt_sel { get; set; }

        public TecnoMetal TecnoMetal { get; set; }
        public Menu_Bloco_Peca(TecnoMetal tec)
        {
            this.TecnoMetal = tec;
            InitializeComponent();
            this.DataContext = this;
            this.lista_blocos.ItemsSource = Constantes.BlocosIndicacao().Select(x=>Conexoes.Utilz.getNome(x)).ToList();
            this.tipo_de_peca.ItemsSource = new List<string> { "RMA", "RME", "RMU", "RMT","DIGITAR" };
            this.escala = this.TecnoMetal.Getescala();

            if(this.lista_blocos.Items.Count>0)
            {
                this.lista_blocos.SelectedIndex = 0;
            }

            this.tipo_de_peca.SelectedIndex = 0;
        }
        public string arquivo { get; set; } = "";
        private void set_imagem(object sender, SelectionChangedEventArgs e)
        {
            var sel = this.lista_blocos.SelectedItem;
            if(sel is string)
            {
                var s = sel as string;
                this.arquivo = Constantes.RaizIndicacaoBlocos + s + ".dwg";
                if(File.Exists(arquivo))
                {
                    
                    var pp = Imagem.GetImagePreview(arquivo);
                    imagem.Source = null;
                    imagem.Source = pp;
                }
            }
        }

        private void seleciona_tudo(object sender, RoutedEventArgs e)
        {
            TextBox textBox = null;
            if (sender is TextBox)
            {
                textBox = ((TextBox)sender);

            }


            if (textBox != null)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }
        public string tipo_selecionado { get; set; } = "";
        private void tipo_de_peca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            peca_selecionar.Content = "[...]";
            if(tipo_de_peca.SelectedItem is string)
            {
                tipo_selecionado = tipo_de_peca.SelectedItem.ToString();
                set_titulo_peca_selecionar();
            }
        }

        private void set_titulo_peca_selecionar()
        {
            this.codigo.Text = "";
            this.descricao_txt.Text = "";
            this.comprimento.IsEnabled = false;
            this.codigo.IsEnabled = false;
            this.descricao_txt.IsEnabled = false;
            this.destino_txt.IsEnabled = false;

            this.destino_txt.Text = tipo_selecionado;
           
            if (tipo_selecionado == "RMA" && this.rma_sel != null)
            {
                peca_selecionar.Content = this.rma_sel.ToString();
                this.codigo.Text = this.rma_sel.SAP;
                this.descricao_txt.Text = this.rma_sel.DESC;
            }
            else if (tipo_selecionado == "RME" && this.rme_sel != null)
            {
                var pc = new Conexoes.RME(this.rme_sel);
                peca_selecionar.Content = this.rme_sel.COD_DB;
                if(this.rme_sel.VARIAVEL)
                {
                    this.comprimento.IsEnabled = true;
                    pc.COMP = Conexoes.Utilz.Double(this.comprimento.Text, 0);
                }
                else
                {
                    this.comprimento.Text = this.rme_sel.COMP.ToString();
                }
                this.codigo.Text = pc.CODIGOFIM;
                this.descricao_txt.Text = pc.MAKTX;
            }
            else if (tipo_selecionado == "RMU" && this.rmu_sel != null)
            {
                var selec = this.rmu_sel;
                var pc = new Conexoes.RMU(selec);
                peca_selecionar.Content = selec.COD_DB;
                if (this.rme_sel.VARIAVEL)
                {
                    this.comprimento.IsEnabled = true;
                    pc.COMP = Conexoes.Utilz.Double(this.comprimento.Text, 0);
                }
                else
                {
                    this.comprimento.Text = this.rme_sel.COMP.ToString();
                }
                this.codigo.Text = pc.CODIGOFIM;
                this.descricao_txt.Text = pc.MAKTX;
            }
            else if (tipo_selecionado == "RMT" && this.rmt_sel != null)
            {
                peca_selecionar.Content = this.rmt_sel.ToString();
                this.rmt_sel.Comprimento = Conexoes.Utilz.Double(this.comprimento.Text, 0);
                this.comprimento.IsEnabled = true;
                this.descricao_txt.Text = rmt_sel.Desc;
            }
            else if(tipo_selecionado == "TECNOMETAL")
            {
                this.quantidade.IsEnabled = true;
            }
            else
            {
                this.codigo.IsEnabled = true;
                this.descricao_txt.IsEnabled = true;
                this.destino_txt.IsEnabled = true;
                this.comprimento.IsEnabled = true;
            }
        }

        private void peca_selecionar_Click(object sender, RoutedEventArgs e)
        {
            if (tipo_selecionado == "RMA")
            {
                var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione");
                if (sel != null)
                {
                    this.rma_sel = sel;
                }
            }
            else if (tipo_selecionado == "RME")
            {
                var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMEs(), null, "Selecione");
                if (sel != null)
                {
                    this.rme_sel = sel;
                }
            }
            else if (tipo_selecionado == "RMU")
            {
                var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMUs(), null, "Selecione");
                if (sel != null)
                {
                    this.rmu_sel = sel;
                }
            }
            else if (tipo_selecionado == "RMT")
            {
                var sel = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMTs(), null, "Selecione");
                if (sel != null)
                {
                    this.rmt_sel = sel;
                }
            }
            set_titulo_peca_selecionar();
      
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double qtd = Conexoes.Utilz.Double(this.quantidade.Text);
            double comprimento = Conexoes.Utilz.Double(this.comprimento.Text,0);

            double escala = Conexoes.Utilz.Double(this.escala_txt.Text);
            if(escala<=0)
            {
                Conexoes.Utilz.Alerta("Escala digitada inválida");
                return;
            }

            if(qtd<=0)
            {
                Conexoes.Utilz.Alerta($"Quantidade digitada [{qtd}] inválida.");
                return;
            }

            if (tipo_selecionado == "RMA")
            {
             if(rma_sel ==null) { Conexoes.Utilz.Alerta("Selecione uma peça antes de continuar."); return; };

                if(!rma_sel.MultiploOk(qtd))
                {
                    Conexoes.Utilz.Alerta($"Quantidade digitada [{qtd}] não é múltipla da quantidade padrão da peça: {rma_sel.Multiplo}");
                    return;
                }
               
            }
            else if (tipo_selecionado == "RME")
            {
                if (rme_sel == null) { Conexoes.Utilz.Alerta("Selecione uma peça antes de continuar."); return; };

                if(rme_sel.VARIAVEL)
                {
                    if(comp>rme_sel.COMP_MAX)
                    {
                        Conexoes.Utilz.Alerta($"Comprimento digitado [{comp}] é maior que o comprimento máximo da peça: {rme_sel.COMP_MAX}");
                        return;
                    }
                    else if (comp < rme_sel.COMP_MIN)
                    {
                        Conexoes.Utilz.Alerta($"Comprimento digitado [{comp}] é menor que o comprimento mínimo da peça: {rme_sel.COMP_MIN}");
                        return;
                    }
                }

            }
            else if (tipo_selecionado == "RMU")
            {
                if (rmu_sel == null) { Conexoes.Utilz.Alerta("Selecione uma peça antes de continuar."); return; };

            }
            else if (tipo_selecionado == "RMT")
            {
                if (rmt_sel == null) { Conexoes.Utilz.Alerta("Selecione uma peça antes de continuar."); return; };

            }

            if(tipo_selecionado!="RMA" && !Conexoes.Utilz.E_Multiplo(qtd,1))
            {
                Conexoes.Utilz.Alerta("Quantidade inválida. Deve ser um número inteiro.");
                return;
            }


            int id = 0;


            if(tipo_selecionado=="RMA")
            {
                Conexoes.RMA mm = new Conexoes.RMA(rma_sel, qtd);
                id = mm.id_db;
            }
            else if (tipo_selecionado == "RME")
            {
                Conexoes.RME mm = new Conexoes.RME(rme_sel);
                if (mm.VARIAVEL)
                {
                mm.COMP = comp;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_db;
            }
            else if (tipo_selecionado == "RMU")
            {
                Conexoes.RMU mm = new Conexoes.RMU(rmu_sel);
                if (mm.VARIAVEL)
                {
                    mm.COMP = comp;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_db;
            }
            else if (tipo_selecionado == "RMT")
            {
                Conexoes.RMT mm = new Conexoes.RMT(rmt_sel,Conexoes.DBases.GetBobinaDummy());
                mm.Qtd = (int)qtd;
                id = mm.id_telha;
            }



            this.Visibility = Visibility.Collapsed;
            bool cancelado = false;
            Hashtable att = new Hashtable();
            att.Add("Nº", this.numero.Text);
            att.Add("FAMILIA", this.familia.Text);
            att.Add("TIPO", this.peca_selecionar.Content);
            att.Add("COMP", comp.ToString().Replace(",",""));
            att.Add("CODIGO", codigo.Text);
            att.Add("ID", id);
            att.Add("DESC", descricao_txt.Text);
            att.Add("DESTINO", destino_txt.Text);
            att.Add("QTD", qtd);

            var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
            if (!cancelado)
            {
               while(!cancelado)
                {
                    Blocos.Inserir(CAD.acDoc, arquivo, origem, escala, 0, att);
                    origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                }
            }

            this.Visibility = Visibility.Visible;
        }
    }
}
