using DLM.encoder;
using DLM.vars;
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
using Conexoes;

namespace DLM.cad.Menus
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
        public RME rmu_sel { get; set; }
        public Conexoes.RMT rmt_sel { get; set; }

        private List<MarcaTecnoMetal> marcas_tecnometal { get; set; } = new List<MarcaTecnoMetal>();


        public MarcaTecnoMetal marca_sel { get; set; }

        public CADTecnoMetal TecnoMetal { get; set; }
        public Menu_Bloco_Peca(CADTecnoMetal tec)
        {
            this.TecnoMetal = tec;
            InitializeComponent();
            this.DataContext = this;
            this.combo_lista_blocos.ItemsSource = Cfg.Init.BlocosIndicacao().Select(x=>Conexoes.Utilz.getNome(x)).ToList();
            this.combo_tipo_de_peca.ItemsSource = new List<string> { Cfg.Init.CAD_ATT_RMA, Cfg.Init.CAD_ATT_RME, Cfg.Init.CAD_ATT_RMU, Cfg.Init.CAD_ATT_RMT, Cfg.Init.CAD_ATT_TECNOMETAL, "DIGITAR" };
            this.escala = this.TecnoMetal.GetEscala();

            if(this.combo_lista_blocos.Items.Count>0)
            {
                this.combo_lista_blocos.SelectedIndex = 0;
            }

            this.combo_tipo_de_peca.SelectedIndex = 0;

            this.perfis_mapeaveis = FuncoesCAD.CTVs();
            this.lista_perfis_ctv.ItemsSource = perfis_mapeaveis;


            this.Title = $"Indicações Montagem V." + Conexoes.Utilz.GetVersao(Cfg.Init.CAD_DLL_Local) + $" [{DLM.vars.Cfg.Init.MySQL_Servidor}]";
        }
        public string arquivo { get; set; } = "";
        private void set_imagem(object sender, SelectionChangedEventArgs e)
        {
            var sel = this.combo_lista_blocos.SelectedItem;
            if(sel is string)
            {
                var s = sel as string;
                this.arquivo = Cfg.Init.CAD_Raiz_Blocos_Indicacao + s + ".dwg";
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
                if (tipo_selecionado == Cfg.Init.CAD_ATT_RMA && this.rma_sel != null)
                {
                    bt_peca_selecionar.Content = this.rma_sel.ToString();
                    this.txt_codigo.Text = this.rma_sel.SAP;
                    this.txt_descricao.Text = this.rma_sel.DESC;
                }
                else if (tipo_selecionado == Cfg.Init.CAD_ATT_RME && this.rme_sel != null)
                {
                    var pc = this.rme_sel.Clonar();
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
                    this.txt_descricao.Text = pc.DESC;
                }
                else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMU && this.rmu_sel != null)
                {
                    var selec = this.rmu_sel;
                    var pc = new RME(selec);
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
                    this.txt_descricao.Text = pc.DESC;
                }
                else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMT && this.rmt_sel != null)
                {
                    bt_peca_selecionar.Content = this.rmt_sel.ToString();
                    var pc = new Conexoes.RMT(this.rmt_sel, Conexoes.DBases.GetBobinaDummyPP());
                    this.rmt_sel.Comprimento = Conexoes.Utilz.Double(this.txt_comprimento.Text, 0);
                    this.txt_descricao.Text = rmt_sel.Desc;
                }
                else if (tipo_selecionado == Cfg.Init.CAD_ATT_TECNOMETAL)
                {
                    if (this.marca_sel != null)
                    {
                        this.txt_codigo.Text = this.marca_sel.Marca;
                        this.txt_comprimento.Text = this.marca_sel.Comprimento.ToString();
                        this.txt_descricao.Text = this.marca_sel.Mercadoria;
                        this.txt_destino.Text = Cfg.Init.CAD_ATT_TECNOMETAL;
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
            if (tipo_selecionado == Cfg.Init.CAD_ATT_RMA)
            {
                var sel = Conexoes.DBases.GetBancoRM().GetRMAs().ListaSelecionar();
                if (sel != null)
                {
                    this.rma_sel = sel;
                }
            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RME)
            {
                var sel = Conexoes.DBases.GetBancoRM().GetRMEs().ListaSelecionar();
                if (sel != null)
                {
                    this.rme_sel = sel;
                }
            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMU)
            {
                var sel = Conexoes.DBases.GetBancoRM().GetRMUs().ListaSelecionar();
                if (sel != null)
                {
                    this.rmu_sel = sel;
                }
            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMT)
            {
                var sel = Conexoes.DBases.GetBancoRM().GetRMTs().ListaSelecionar();
                if (sel != null)
                {
                    this.rmt_sel = sel;
                }
            }
            else if(tipo_selecionado == Cfg.Init.CAD_ATT_TECNOMETAL)
            {
                List<Report> erros = new List<Report>();
                marcas_tecnometal = this.TecnoMetal.GetMarcasPranchas(ref erros);
                if (marcas_tecnometal.Count > 0)
                {
                    this.marca_sel = marcas_tecnometal.ListaSelecionar();
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
            Hashtable ht = new Hashtable();
            ht.Add(Cfg.Init.CAD_ATT_N, this.txt_prefix.Text + this.txt_sequencial.Text);
            ht.Add(Cfg.Init.CAD_ATT_Familia, this.familia.Text);
            ht.Add(Cfg.Init.CAD_ATT_Tipo, this.tipo_selecionado);
            ht.Add(Cfg.Init.CAD_ATT_Comprimento, comprimento.ToString().Replace(",", ""));
            ht.Add("CODIGO", txt_codigo.Text);
            ht.Add(Cfg.Init.CAD_ATT_id, id);
            ht.Add(Cfg.Init.CAD_ATT_Descricao, txt_descricao.Text);
            ht.Add("DESTINO", txt_destino.Text);
            ht.Add(Cfg.Init.CAD_ATT_Quantidade, qtd);

            var origem = Ut.PedirPonto("Selecione a origem", out cancelado);
            if (!cancelado)
            {
                while (!cancelado)
                {
                    Blocos.Inserir(CAD.acDoc, arquivo, origem, escala, 0, ht);
                    origem = Ut.PedirPonto("Selecione a origem", out cancelado);
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

            if (tipo_selecionado == Cfg.Init.CAD_ATT_RMA && rma_sel!=null)
            {
                Conexoes.RMA mm = new Conexoes.RMA(rma_sel, qtd);
                id = mm.id_db;
            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RME && rme_sel!=null)
            {
                Conexoes.RME mm = rme_sel.Clonar();
                if (mm.VARIAVEL)
                {
                    mm.COMP = comprimento;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_codigo;
                txt_codigo.Text = mm.CODIGOFIM;

            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMU && rmu_sel!=null)
            {
                RME mm = new RME(rmu_sel);
                if (mm.VARIAVEL)
                {
                    mm.COMP = comprimento;
                }
                mm.Quantidade = (int)qtd;
                id = mm.id_codigo;
                txt_codigo.Text = mm.CODIGOFIM;
            }
            else if (tipo_selecionado == Cfg.Init.CAD_ATT_RMT && rmt_sel!=null)
            {
                Conexoes.RMT mm = new Conexoes.RMT(rmt_sel, Conexoes.DBases.GetBobinaDummy());
                mm.Qtd = (int)qtd;
                id = mm.id_telha;
                txt_codigo.Text = mm.NomeFim;
            }
        }
    }
}
