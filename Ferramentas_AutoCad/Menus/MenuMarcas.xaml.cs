using System;
using System.Collections.Generic;
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

namespace Ferramentas_DLM
{
    /// <summary>
    /// Interação lógica para MenuMarcas.xam
    /// </summary>
    public partial class MenuMarcas : Window
    {
        public string Prefix { get; set; } = "P";
        public string Sufix { get; set; } = "01";
        public double Quantidade { get; set; } = 1;

        public static List<DB.Valor> lista_mercadorias { get; set; } = new List<DB.Valor>();
        public string NomeFim
        {
            get
            {
                return this.prefix.Text + this.sufix.Text;
            }
        }

        private Tipo_Bloco tipo
        {
            get
            {
                var s = tipo_marca_combo.SelectedItem;
                if (s is Tipo_Bloco)
                {
                    return (Tipo_Bloco)s;
                }

                return Tipo_Bloco._;
            }
        }
        public DB.Valor db_mercadoria
        {
            get
            {
                if (mercadoria.SelectedItem is DB.Valor)
                {
                    return mercadoria.SelectedItem as DB.Valor;
                }
                return null;
            }
        }
        public DB.Valor db_material
        {
            get
            {
                if (material.SelectedItem is DB.Valor)
                {
                    return material.SelectedItem as DB.Valor;
                }
                return null;
            }
        }
        public MarcaTecnoMetal marca_selecionada
        {
            get
            {
                if(this.seleciona_marca_composta.SelectedItem is MarcaTecnoMetal)
                {
                    return this.seleciona_marca_composta.SelectedItem as MarcaTecnoMetal;
                }
                return null;
            }
        }
        public static Conexoes.Chapa db_chapa { get; set; }
        public static Conexoes.RMA db_unitario { get; set; }
        public static Conexoes.Bobina db_bobina { get; set; }
        public static Conexoes.TecnoMetal_Perfil db_perfil { get; set; }
        public static Conexoes.TecnoMetal_Perfil db_perfil_m2 { get; set; }

        public static TecnoMetal TecnoMetal { get; set; }

        public MenuMarcas(TecnoMetal tecnoMetal)
        {

            InitializeComponent();

            try
            {

                this.mercadoria.ItemsSource = Conexoes.DBases.GetMercadorias();
                this.material.ItemsSource = Conexoes.DBases.GetMateriais();


                tipo_marca_combo.ItemsSource = Conexoes.Utilz.GetLista_Enumeradores<Tipo_Bloco>().ToList().FindAll(x=> x!= Tipo_Bloco._ && x!= Tipo_Bloco.DUMMY);

                tipo_marca_combo.SelectedIndex = 0;

                this.mercadoria.SelectedIndex = 0;
                this.material.SelectedIndex = 0;

                Update(tecnoMetal);
                this.DataContext = this;
            }
            catch (Exception)
            {

            }


        }

        public int Sufix_Count { get; set; } = 1;
        public void Update(TecnoMetal tecnoMetal)
        {
            MenuMarcas.TecnoMetal = tecnoMetal;
            this.seleciona_marca_composta.Visibility = Visibility.Visible;
            this.seleciona_marca_composta.ItemsSource = MenuMarcas.TecnoMetal.GetMarcasCompostas();

            var ms = MenuMarcas.TecnoMetal.GetMarcas().ToList();
            var pos = ms.SelectMany(x => x.GetPosicoes()).ToList();
            this.Sufix_Count = ms.Count + pos.Count +1;

        

            if(this.seleciona_marca_composta.Items.Count>0 && this.seleciona_marca_composta.SelectedItem==null)
            {
                this.seleciona_marca_composta.SelectedIndex = 0;
            }
            else
            {
                this.seleciona_marca_composta.Visibility = Visibility.Collapsed;
            }



            SetTextos();


        }
        private void selecionar_perfil(object sender, RoutedEventArgs e)
        {

            perfil.Content = "...";
            this.Visibility = Visibility.Collapsed;
            switch (this.tipo)
            {

                case Tipo_Bloco.Chapa:
                    db_chapa = MenuMarcas.TecnoMetal.PromptChapa(Tipo_Chapa.Tudo);
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    db_perfil = Conexoes.Utilz.SelecionarPerfil();
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    this.Visibility = Visibility.Collapsed;
                    db_perfil_m2 = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetdbTecnoMetal().GetPerfis().FindAll(x => x.Tipo == DLMCam.TipoPerfil.Chapa_Xadrez), null, "Selecione");
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    db_unitario = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione");
                    if (db_perfil != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }
                    break;
                case Tipo_Bloco.Arremate:
                    var esp = TecnoMetal.PromptChapa(Tipo_Chapa.Fina);
                    if(esp!=null)
                    {
                        db_bobina = TecnoMetal.PromptBobina(esp);
                    }
                    
                    if (db_bobina != null)
                    {
                        perfil.Content = db_bobina.ToString();
                    }
                    break;
                case Tipo_Bloco._:
                    break;
            }
            this.Visibility = Visibility.Visible;

        }

        private void criar_bloco(object sender, RoutedEventArgs e)
        {
            var ms = MenuMarcas.TecnoMetal.GetMarcas().ToList();
            var pos = ms.SelectMany(x => x.GetPosicoes()).ToList();

            var qtd_double = Conexoes.Utilz.Double(this.quantidade.Text);
            if ((bool)m_composta.IsChecked)
            {
                if (this.marca_selecionada == null)
                {
                    Conexoes.Utilz.Alerta("Selecione uma marca na lista ou crie uma marca nova para poder criar essa posição.");
                    return;
                }
            }
            else
            {
                if(db_mercadoria==null)
                {
                    Conexoes.Utilz.Alerta($"Selecione uma mercadoria.");
                    return;
                }
            }

            if(qtd_double <= 0)
            {
                Conexoes.Utilz.Alerta($"{qtd_double} quantidade inválida.");
                return;
            }


            if(NomeFim.Replace(" ","").Replace("_","").Length==0)
            {
                Conexoes.Utilz.Alerta($"Nome inválido.");
                return;
            }

           if(tipo!= Tipo_Bloco.Elemento_Unitario && NomeFim.Contains("_"))
            {
                Conexoes.Utilz.Alerta($"Nome inválido.");
                return;
            }

           if(tipo == Tipo_Bloco.Elemento_Unitario && !NomeFim.EndsWith("_A"))
            {
                Conexoes.Utilz.Alerta($"Nome inválido: para elemento unitário deve sempre terminar com '_A'");
                return;
            }



            if (Conexoes.Utilz.CaracteresEspeciais(NomeFim) | NomeFim.Contains(" "))
            {
                Conexoes.Utilz.Alerta($"{NomeFim} - Nome não pode conter caracteres especiais ou espaços.");
                return;
            }



            if (ms.FindAll(x => x.Marca == NomeFim).Count > 0 | pos.FindAll(x => x.Posicao == NomeFim).Count > 0)
            {
                if (this.marca_selecionada == null)
                {
                    Conexoes.Utilz.Alerta($"{NomeFim} - Já existe uma marca / posição com o mesmo nome no desenho.");
                    return;
                }
            }

            if(tipo!= Tipo_Bloco.Elemento_Unitario)
            {
                if(!Conexoes.Utilz.E_Multiplo(qtd_double,1))
                {
                    Conexoes.Utilz.Alerta($"Quantidade inválida: {qtd_double}. Quantidades com números quebrados somente para elemento unitário.");
                    return;
                }
            }

            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (MenuMarcas.db_chapa == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione uma espessura.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    if (MenuMarcas.db_perfil == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (MenuMarcas.db_perfil_m2 == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil m2.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (MenuMarcas.db_unitario == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um item.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Arremate:
                    if (MenuMarcas.db_bobina == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione uma bobina.");
                        return;
                    }
                    break;
                case Tipo_Bloco._:
                    Conexoes.Utilz.Alerta("Seleção inválida.");
                    return;
            }



            this.Visibility = Visibility.Collapsed;

            string nomeMarca = "";
            string nomePos = "";

            if((bool)m_simples.IsChecked)
            {
                nomeMarca = this.NomeFim;
            }
            else
            {
                nomeMarca = this.marca_selecionada.Marca;
                nomePos = this.NomeFim;
            }

            switch (this.tipo)
            {
                case Tipo_Bloco.Chapa:
                    MenuMarcas.TecnoMetal.InserirChapa(nomeMarca, nomePos, this.db_material.valor, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_chapa, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Perfil:
                    MenuMarcas.TecnoMetal.InserirPerfil(nomeMarca, nomePos, this.db_material.valor, this.tratamento.Text, (int)qtd_double, MenuMarcas.db_perfil, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Elemento_M2:
                    MenuMarcas.TecnoMetal.InserirElementoM2(nomeMarca, nomePos,this.db_material.valor,this.tratamento.Text, (int)qtd_double, MenuMarcas.db_perfil,this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    MenuMarcas.TecnoMetal.InserirElementoUnitario(nomeMarca, nomePos, qtd_double, this.db_mercadoria.valor, MenuMarcas.db_unitario);
                    break;
                case Tipo_Bloco.Arremate:
                    MenuMarcas.TecnoMetal.InserirArremate(nomeMarca, nomePos, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_bobina,true, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco._:
                    break;
            }

            this.Update(MenuMarcas.TecnoMetal);
            this.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            //this.Visibility = Visibility.Collapsed;
        }
        private void nova_marca_Click(object sender, RoutedEventArgs e)
        {

            this.Visibility = Visibility.Collapsed;
            var nova = MenuMarcas.TecnoMetal.InserirMarcaComposta();
            if (nova != null)
            {
                this.Update(MenuMarcas.TecnoMetal);
                this.seleciona_marca_composta.SelectedItem = nova;
                this.tratamento.Text = nova.Tratamento;
      
            }
            this.Visibility = Visibility.Visible;

        }
        private void tipo_marca_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTextos();
        }

        private void SetTextos()
        {
            perfil.Content = "...";


            this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0');

            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (db_perfil_m2 != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();

                    }

                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (db_unitario != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }
                    this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0') + "_A";
                    break;
                case Tipo_Bloco.Arremate:
                    if (db_bobina != null)
                    {
                        perfil.Content = db_bobina.ToString();
                    }
                    break;
                case Tipo_Bloco._:
                    break;
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

        private void desliga_layer(object sender, RoutedEventArgs e)
        {
           
            Utilidades.DesligarLayers(Constantes.LayersMarcasDesligar);
            this.Close();
        }

        private void liga_layer(object sender, RoutedEventArgs e)
        {
            Utilidades.LigarLayers(Constantes.LayersMarcasDesligar);
            this.Close();
        }

        private void insere_tabela(object sender, RoutedEventArgs e)
        {
            this.Close();
            MenuMarcas.TecnoMetal.InserirTabela();
        }

        private void insere_tabela_auto(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            MenuMarcas.TecnoMetal.InserirTabelaAuto(ref erros);
        }

        private void gerar_dbf(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.gerardbf();
        }

        private void gerar_dbf_3d(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.gerardbf3d();
        }

        private void mercadorias(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.mercadorias();
        }

        private void materiais(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.materiais();
        }

        private void tratamentos(object sender, RoutedEventArgs e)
        {
            this.Close();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.tratamentos();
        }
    }
}