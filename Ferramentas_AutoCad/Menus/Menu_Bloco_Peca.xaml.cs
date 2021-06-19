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
        public List<CTV_de_para> perfis_mapeaveis { get; set; } = new List<CTV_de_para>();
        public double escala { get; set; } = 10;
        public double comp { get; set; } = 0;
        public double qtd { get; set; } = 1;
        public int arredondamento { get; set; } = 50;

        public int sequencial { get; set; } = 1;

        public Conexoes.RMA rma_sel { get; set; }
        public Conexoes.RME rme_sel { get; set; }
        public Conexoes.RMU rmu_sel { get; set; }
        public Conexoes.RMT rmt_sel { get; set; }

        private List<MarcaTecnoMetal> marcas_tecnometal { get; set; } = new List<MarcaTecnoMetal>();


        public MarcaTecnoMetal marca_sel { get; set; }

        public TecnoMetal TecnoMetal { get; set; }
        public Menu_Bloco_Peca(TecnoMetal tec)
        {
            this.TecnoMetal = tec;
            InitializeComponent();
            this.DataContext = this;
            this.combo_lista_blocos.ItemsSource = Constantes.BlocosIndicacao().Select(x=>Conexoes.Utilz.getNome(x)).ToList();
            this.combo_tipo_de_peca.ItemsSource = new List<string> { "RMA", "RME", "RMU", "RMT","TECNOMETAL","DIGITAR" };
            this.escala = this.TecnoMetal.Getescala();

            if(this.combo_lista_blocos.Items.Count>0)
            {
                this.combo_lista_blocos.SelectedIndex = 0;
            }

            this.combo_tipo_de_peca.SelectedIndex = 0;

            this.perfis_mapeaveis = Constantes.CTVs();
            this.lista_perfis_ctv.ItemsSource = perfis_mapeaveis;


            this.Title = $"Indicações Montagem V." + Conexoes.Utilz.GetVersao(Constantes.DLL_Local);
        }
        public string arquivo { get; set; } = "";
        private void set_imagem(object sender, SelectionChangedEventArgs e)
        {
            var sel = this.combo_lista_blocos.SelectedItem;
            if(sel is string)
            {
                var s = sel as string;
                this.arquivo = Constantes.Raiz_Blocos_Indicacao + s + ".dwg";
                if(File.Exists(arquivo))
                {
                    
                    var pp = Imagem.GetImagePreview(arquivo);
                    caixa_imagem.Source = null;
                    caixa_imagem.Source = pp;
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
            bt_peca_selecionar.Content = "[...]";
            if(combo_tipo_de_peca.SelectedItem is string)
            {
                tipo_selecionado = combo_tipo_de_peca.SelectedItem.ToString();
                set_titulo_peca_selecionar();
            }
        }

        private int id { get; set; } = 0;

        private void set_titulo_peca_selecionar()
        {
            this.id = 0;
            this.txt_codigo.Text = "";
            this.txt_descricao.Text = "";
            this.txt_codigo.Text = "";
            this.txt_descricao.Text = "";
            this.txt_destino.Text = "";
            this.txt_quantidade.Text = "1";
            this.bt_peca_selecionar.Content = "[...]";

            this.txt_destino.Text = tipo_selecionado;

            try
            {
                if (tipo_selecionado == "RMA" && this.rma_sel != null)
                {
                    bt_peca_selecionar.Content = this.rma_sel.ToString();
                    this.txt_codigo.Text = this.rma_sel.SAP;
                    this.txt_descricao.Text = this.rma_sel.DESC;
                }
                else if (tipo_selecionado == "RME" && this.rme_sel != null)
                {
                    var pc = new Conexoes.RME(this.rme_sel);
                    bt_peca_selecionar.Content = this.rme_sel.COD_DB;
                    if (this.rme_sel.VARIAVEL)
                    {
                        this.txt_comprimento.IsEnabled = true;
                        pc.COMP = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);
                    }
                    else
                    {
                        this.txt_comprimento.Text = this.rme_sel.COMP.ToString();
                    }
                    this.txt_codigo.Text = pc.CODIGOFIM;
                    this.txt_descricao.Text = pc.MAKTX;
                }
                else if (tipo_selecionado == "RMU" && this.rmu_sel != null)
                {
                    var selec = this.rmu_sel;
                    var pc = new Conexoes.RMU(selec);
                    bt_peca_selecionar.Content = selec.COD_DB;
                    if (this.rmu_sel.VARIAVEL)
                    {
                        this.txt_comprimento.IsEnabled = true;
                        pc.COMP = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);
                    }
                    else
                    {
                        this.txt_comprimento.Text = this.rme_sel.COMP.ToString();
                    }
                    this.txt_codigo.Text = pc.CODIGOFIM;
                    this.txt_descricao.Text = pc.MAKTX;
                }
                else if (tipo_selecionado == "RMT" && this.rmt_sel != null)
                {
                    bt_peca_selecionar.Content = this.rmt_sel.ToString();
                    var pc = new Conexoes.RMT(this.rmt_sel, Conexoes.DBases.GetBobinaDummyPP());
                    this.rmt_sel.Comprimento = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);
                    this.txt_descricao.Text = rmt_sel.Desc;
                }
                else if (tipo_selecionado == "TECNOMETAL")
                {
                    if (this.marca_sel != null)
                    {
                        this.txt_codigo.Text = this.marca_sel.Marca;
                        this.txt_comprimento.Text = this.marca_sel.Comprimento.ToString();
                        this.txt_descricao.Text = this.marca_sel.Mercadoria;
                        this.txt_destino.Text = "TECNOMETAL";
                        this.txt_quantidade.Text = this.marca_sel.Quantidade.ToString();
                        this.bt_peca_selecionar.Content = this.marca_sel.ToString();
                    }
                }
                else
                {
                }


               
            }
            catch (Exception)
            {
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
            else if(tipo_selecionado == "TECNOMETAL")
            {
                marcas_tecnometal = this.TecnoMetal.GetMarcasPranchas();
                if (marcas_tecnometal.Count > 0)
                {
                    this.marca_sel = Conexoes.Utilz.SelecionarObjeto(marcas_tecnometal, null, "Selecione");
                }
            }
            set_titulo_peca_selecionar();
      
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double qtd = Conexoes.Utilz.Double(this.txt_quantidade.Text);
            double comprimento = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);

            double escala = Conexoes.Utilz.Double(this.txt_escala.Text);
            if (escala <= 0)
            {
                Conexoes.Utilz.Alerta("Escala digitada inválida");
                return;
            }

            if (this.txt_codigo.Text.Length == 0)
            {
                Conexoes.Utilz.Alerta($"Campo código não pode ser em branco.");
                return;
            }

            if (qtd <= 0)
            {
                Conexoes.Utilz.Alerta($"Quantidade digitada [{qtd}] inválida.");
                return;
            }







           



            this.Visibility = Visibility.Collapsed;
            bool cancelado = false;
            Hashtable att = new Hashtable();
            att.Add("N", this.txt_prefix.Text + this.txt_sequencial.Text);
            att.Add("FAMILIA", this.familia.Text);
            att.Add("TIPO", this.tipo_selecionado);
            att.Add("COMP", comprimento.ToString().Replace(",", ""));
            att.Add("CODIGO", txt_codigo.Text);
            att.Add("ID", id);
            att.Add("DESC", txt_descricao.Text);
            att.Add("DESTINO", txt_destino.Text);
            att.Add("QTD", qtd);

            var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
            if (!cancelado)
            {
                while (!cancelado)
                {
                    Blocos.Inserir(CAD.acDoc, arquivo, origem, escala, 0, att);
                    origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                }
            }

            var sequencial = Conexoes.Utilz.Int(txt_sequencial.Text);

            if (sequencial > 0)
            {
                txt_sequencial.Text = (sequencial + 1).ToString();
            }

            this.Visibility = Visibility.Visible;
        }



        private void mapear_contraventos(object sender, RoutedEventArgs e)
        {


            int seq = Conexoes.Utilz.Int(this.txt_sequencial.Text);

            int arredon = Conexoes.Utilz.Int(txt_arredondamento.Text);

            var subs_bloco = (bool)chk_bloco_ctv.IsChecked;
            bool agrupar = (bool)chk_agrupar.IsChecked;
            bool gerar_ctvs = (bool)chk_mapear_contraventos.IsChecked;
            bool gerar_pecas = (bool)chk_mapear_pecas.IsChecked;

            if (arredon < 0 && gerar_ctvs)
            {
                Conexoes.Utilz.Alerta("Valor de arredondamento inválido..");
                return;
            }
            if (this.lista_perfis_ctv.Items.Count == 0 && gerar_ctvs)
            {
                Conexoes.Utilz.Alerta("Arquivo de configuração de perfis mapeáveis não foi carregado. Contacte suporte.");
                return;
            }


            this.Visibility = Visibility.Collapsed;
            seq = this.TecnoMetal.MapearPCsTecnoMetal(seq, arredon, subs_bloco,this.perfis_mapeaveis,Conexoes.Utilz.Double(txt_escala.Text),arquivo, agrupar, gerar_ctvs, gerar_pecas);

            this.Visibility = Visibility.Visible;
        }

        private void atualiza_nome(object sender, TextChangedEventArgs e)
        {
            double qtd = Conexoes.Utilz.Double(this.txt_quantidade.Text);
            double comprimento = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);

            if (tipo_selecionado == "RMA" && rma_sel!=null)
            {
                Conexoes.RMA mm = new Conexoes.RMA(rma_sel, qtd);
                id = mm.id_db;
            }
            else if (tipo_selecionado == "RME" && rme_sel!=null)
            {
                Conexoes.RME mm = new Conexoes.RME(rme_sel);
                if (mm.VARIAVEL)
                {
                    mm.COMP = comprimento;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_db;
                txt_codigo.Text = mm.CODIGOFIM;

            }
            else if (tipo_selecionado == "RMU" && rmu_sel!=null)
            {
                Conexoes.RMU mm = new Conexoes.RMU(rmu_sel);
                if (mm.VARIAVEL)
                {
                    mm.COMP = comprimento;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_db;
                txt_codigo.Text = mm.CODIGOFIM;
            }
            else if (tipo_selecionado == "RMT" && rmt_sel!=null)
            {
                Conexoes.RMT mm = new Conexoes.RMT(rmt_sel, Conexoes.DBases.GetBobinaDummy());
                mm.Qtd = (int)qtd;
                id = mm.id_telha;
                txt_codigo.Text = mm.NomeFim;
            }
        }
    }
}
