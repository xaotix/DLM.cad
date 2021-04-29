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
        public List<DB.Valor> lista_mercadorias { get; set; } = new List<DB.Valor>();
        public string NomeFim
        {
            get
            {
                return this.prefix.Text + this.sufix.Text;
            }
        }
        public string Prefix { get; set; } = "P";
        public string Sufix { get; set; } = "01";
        public double Quantidade { get; set; } = 1;
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
        public Conexoes.Chapa db_chapa { get; set; }
        public Conexoes.RMA db_unitario { get; set; }
        public Conexoes.Bobina db_bobina { get; set; }
        public Conexoes.TecnoMetal_Perfil db_perfil { get; set; }
        public Conexoes.TecnoMetal_Perfil db_perfil_m2 { get; set; }
        public DB.Valor db_mercadoria
        {
            get
            {
                if(mercadoria.SelectedItem is DB.Valor)
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
        public TecnoMetal TecnoMetal { get; set; }

        public MenuMarcas(TecnoMetal tecnoMetal)
        {

            InitializeComponent();

            try
            {

                this.mercadoria.ItemsSource = Conexoes.DBases.GetMercadorias();
                this.material.ItemsSource = Conexoes.DBases.GetMateriais();


                tipo_marca_combo.ItemsSource = Conexoes.Utilz.GetLista_Enumeradores<Tipo_Bloco>();

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

        public void Update(TecnoMetal tecnoMetal)
        {
            this.TecnoMetal = tecnoMetal;
            this.seleciona_marca_composta.Visibility = Visibility.Visible;
            this.seleciona_marca_composta.ItemsSource = this.TecnoMetal.GetMarcasCompostas();
            if(this.seleciona_marca_composta.Items.Count>0)
            {
                this.seleciona_marca_composta.SelectedIndex = 0;
            }
            else
            {
                this.seleciona_marca_composta.Visibility = Visibility.Collapsed;
            }
        }
        private void selecionar_perfil(object sender, RoutedEventArgs e)
        {

            perfil.Content = "...";
            this.Visibility = Visibility.Collapsed;
            switch (this.tipo)
            {

                case Tipo_Bloco.Chapa:
                    db_chapa = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetChapas(), null, "Selecione");
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
                    db_bobina = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetBobinas().FindAll(x => x.Corte == 1200).ToList(), null, "Selecione");
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
        private void mercadoria_Click(object sender, RoutedEventArgs e)
        {

        }
        private void material_Click(object sender, RoutedEventArgs e)
        {

        }
        private void criar_bloco(object sender, RoutedEventArgs e)
        {
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

            if(Conexoes.Utilz.Double(this.quantidade.Text)<=0)
            {
                Conexoes.Utilz.Alerta($"{this.quantidade.Text} quantidade inválida.");
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


            if (this.TecnoMetal.GetMarcas().FindAll(x => x.Marca == NomeFim | x.Posicao == NomeFim).Count > 0)
            {
                if (this.marca_selecionada == null)
                {
                    Conexoes.Utilz.Alerta($"{NomeFim} - Já existe uma marca / posição com o mesmo nome no desenho.");
                    return;
                }
            }



            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (this.db_chapa == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione uma espessura.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    if (this.db_perfil == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (this.db_perfil_m2 == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil m2.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (this.db_unitario == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um item.");
                        return;
                    }

                    var qtd = Conexoes.Utilz.Double(this.quantidade.Text);
                    if(!db_unitario.MultiploOk(qtd))
                    {
                        Conexoes.Utilz.Alerta($"Quantidade inválida: {qtd}. Precisa ser múltiplo de {db_unitario.Multiplo}");
                        return;
                    }

                    break;
                case Tipo_Bloco.Arremate:
                    if (this.db_bobina == null)
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
                    this.TecnoMetal.InserirChapa(nomeMarca, nomePos, this.db_material.valor,Conexoes.Utilz.Int(this.quantidade.Text),this.tratamento.Text,this.db_chapa, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Perfil:
                    this.TecnoMetal.InserirPerfil(nomeMarca, nomePos, this.db_material.valor, this.tratamento.Text, Conexoes.Utilz.Int(this.quantidade.Text), this.db_perfil, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Elemento_M2:
                    this.TecnoMetal.InserirElementoM2(nomeMarca, nomePos,this.db_material.valor,this.tratamento.Text,Conexoes.Utilz.Int(this.quantidade.Text),this.db_perfil,this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    this.TecnoMetal.InserirElementoUnitario(nomeMarca, nomePos, Conexoes.Utilz.Double(this.quantidade.Text),this.db_mercadoria.valor, this.db_unitario);
                    break;
                case Tipo_Bloco.Arremate:
                    this.TecnoMetal.InserirArremate(nomeMarca, nomePos, Conexoes.Utilz.Int(this.quantidade.Text), this.tratamento.Text, this.db_bobina,true, this.db_mercadoria.valor);
                    break;
                case Tipo_Bloco._:
                    break;
            }

            this.Update(new TecnoMetal());
            this.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }
        private void nova_marca_Click(object sender, RoutedEventArgs e)
        {

            this.Visibility = Visibility.Collapsed;
            var nova = this.TecnoMetal.InserirMarcaComposta();
            if (nova != null)
            {
                this.Update(new TecnoMetal());
                this.seleciona_marca_composta.SelectedItem = nova;
                this.tratamento.Text = nova.Tratamento;
      
            }
            this.Visibility = Visibility.Visible;

        }
        private void tipo_marca_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            perfil.Content = "...";
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
        private void seleciona_marca_composta_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}