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

namespace Ferramentas_DLM.Menus
{
    /// <summary>
    /// Interação lógica para Purlin.xam
    /// </summary>
    public partial class Purlin : Window
    {

        public Purlin()
        {

            InitializeComponent();
            this.Title = $"xPurlin V." + Conexoes.Utilz.GetVersao(Constantes.DLL_Local) + $" [{Conexoes.Cfg.Init.MySQL_Servidor}]";

            this.correntes_mlstyles.ItemsSource = Comandos.CADPurlin.GetCorrentesMlStylesSelecao();
            this.tirantes_mlstyles.ItemsSource = Comandos.CADPurlin.GetTirantesMLStylesSelecao();
            this.tercas_mlstyles.ItemsSource = Comandos.CADPurlin.GetTercasMLStylesSelecao();
        }

        private void terca_Click(object sender, RoutedEventArgs e)
        {
            var s = Utilidades.SelecionarPurlin(null);

            if(s!=null)
            {
                Comandos.CADPurlin.SetPurlin(s.id_db);
                this.terca.Content = s.COD_DB;
            }
        }

        private void corrente_Click(object sender, RoutedEventArgs e)
        {
            var s = Utilidades.SelecionarCorrente();

            if (s != null)
            {
                Comandos.CADPurlin.SetCorrente(s.id_db);
                this.corrente.Content = s.COD_DB;
            }
        }



        private void tirante_Click(object sender, RoutedEventArgs e)
        {
            var s = Utilidades.SelecionarTirante();

            if (s != null)
            {
                Comandos.CADPurlin.SetTirante(s.id_db);
                this.tirante.Content = s.COD_DB;
            }
        }

        private void transpasse_Click(object sender, RoutedEventArgs e)
        {
            bool confirmado = false;
            double valor = Conexoes.Utilz.Prompt(Comandos.CADPurlin.TranspassePadrao, out confirmado);
            if(confirmado)
            {
                Comandos.CADPurlin.TranspassePadrao = valor;
                this.transpasse.Content = valor;
            }

        }

        private void furo_offset_apoio_Click(object sender, RoutedEventArgs e)
        {
            int valor = Conexoes.Utilz.Prompt(Comandos.CADPurlin.OffsetApoio);
            Comandos.CADPurlin.OffsetApoio = valor;
            this.furo_offset_apoio.Content = valor;
        }

        private void ficha_Click(object sender, RoutedEventArgs e)
        {
            string valor = Conexoes.Utilz.Prompt(Comandos.CADPurlin.FichaDePintura);
            if(valor!=null)
            {
                Comandos.CADPurlin.FichaDePintura = valor;
                this.ficha.Content = valor;
            }
        }

        private void furos_manuais_layer_Click(object sender, RoutedEventArgs e)
        {
            string valor = Conexoes.Utilz.SelecionarObjeto(Comandos.CADPurlin.GetLayers(), null, "Selecione");
            if(valor!=null && valor.Length>0)
            {
                this.furos_manuais_layer.Content = valor;
                Comandos.CADPurlin.MapeiaFurosManuaisLayer = valor;
            }
        }

        private void mapeia(object sender, RoutedEventArgs e)
        {
            if (correntes_mlstyles.Items.Count == 0 && (bool)mapeia_correntes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear as correntes sem ter um estilo de MLinha");
                return;
            }

            if (tirantes_mlstyles.Items.Count == 0 && (bool)mapeia_tirantes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear os tirantes sem ter um estilo de MLinha");
                return;
            }

            if (tercas_mlstyles.Items.Count == 0 && (bool)mapeia_tercas.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear os tercas sem ter um estilo de MLinha");
                return;
            }
            if (Comandos.CADPurlin.MapeiaFurosManuaisLayer == "" && (bool)mapeia_furos.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear os furos manuais sem definir uma layer padrão");
                return;
            }
            Comandos.CADPurlin.MapearCorrentes = (bool)this.mapeia_correntes.IsChecked;
            Comandos.CADPurlin.MapearTercas = (bool)this.mapeia_tercas.IsChecked;
            Comandos.CADPurlin.MapearTirantes = (bool)this.mapeia_tirantes.IsChecked;
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.Mapear();
        }

        private void editar_transpasse(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetTranspasse();
        }

        private void editar_ficha(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetFicha();
        }

        private void editar_trocar_perfil(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetPerfil();
        }

        private void editar_furacao_suporte(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetSuporte();
        }

        private void editar_ver_croqui(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetSuporte();
        }

        private void editar_criar_manual(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.PurlinManual();
        }

        private void editar_edicao_completa(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.EdicaoCompleta();
        }

        private void editar_corrente(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetCorrente();
        }

        private void editar_corrente_descontar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetCorrenteDescontar();
        }

        private void editar_corrente_fixador(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.SetCorrenteFixador();
        }

        private void apagar_blocos(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.ApagarBlocosPurlin();
        }

        private void apagar_tudo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.ApagarBlocosPurlin();
        }

        private void desnha_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.desenharmline();
        }

        private void troca_polyline_por_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.substituirpolylinepormultiline();
        }

        private void trocar_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.mudarmultiline();
        }

        private void marcar_inserir_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.Exportar(Conexoes.Utilz.Pergunta("Gerar Tabela?"), Conexoes.Utilz.Pergunta("Exportar arquivo .RM?"));
        }

        private void gerar_croquis(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Comandos.CADPurlin.GerarCroquis();

        }

        private void ver_propriedades(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;

            Conexoes.Utilz.Propriedades(Comandos.CADPurlin,true,true);

            this.Visibility = Visibility.Visible;
        }
    }
}
