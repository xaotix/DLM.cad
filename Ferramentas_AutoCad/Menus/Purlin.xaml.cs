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

            this.correntes_mlstyles.ItemsSource = Core.CADPurlin.CorrenteMLStyles;
            this.tirantes_mlstyles.ItemsSource = Core.CADPurlin.TirantesMLStyles;
            this.tercas_mlstyles.ItemsSource = Core.CADPurlin.TercasMLStyles;

            this.correntes_multilines.ItemsSource = Core.CADPurlin.GetMLCorrentes();
            this.tirantes_multilines.ItemsSource = Core.CADPurlin.GetMLTirantes();
            this.tercas_multilines.ItemsSource = Core.CADPurlin.GetMLPurlins();

            this.eixos_mapeados.ItemsSource = Core.CADPurlin.GetGrade().GetEixosVerticais();
            this.vaos_mapeados.ItemsSource = Core.CADPurlin.GetGrade().GetVaosVerticais();
        }

        private void terca_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarPurlin(null);

            if(s!=null)
            {
                Core.CADPurlin.SetPurlin(s.id_db);
                this.terca.Content = s.COD_DB;
            }
        }

        private void corrente_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarCorrente();

            if (s != null)
            {
                Core.CADPurlin.SetCorrente(s.id_db);
                this.corrente.Content = s.COD_DB;
            }
        }



        private void tirante_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarTirante();

            if (s != null)
            {
                Core.CADPurlin.SetTirante(s.id_db);
                this.tirante.Content = s.COD_DB;
            }
        }

        private void transpasse_Click(object sender, RoutedEventArgs e)
        {
            bool confirmado = false;
            double valor = Conexoes.Utilz.Prompt(Core.CADPurlin.TranspassePadrao, out confirmado);
            if(confirmado)
            {
                Core.CADPurlin.TranspassePadrao = valor;
                this.transpasse.Content = valor;
            }

        }

        private void furo_offset_apoio_Click(object sender, RoutedEventArgs e)
        {
            int valor = Conexoes.Utilz.Prompt(Core.CADPurlin.OffsetApoio);
            Core.CADPurlin.OffsetApoio = valor;
            this.furo_offset_apoio.Content = valor;
        }

        private void ficha_Click(object sender, RoutedEventArgs e)
        {
            string valor = Conexoes.Utilz.Prompt(Core.CADPurlin.FichaDePintura);
            if(valor!=null)
            {
                Core.CADPurlin.FichaDePintura = valor;
                this.ficha.Content = valor;
            }
        }

        private void furos_manuais_layer_Click(object sender, RoutedEventArgs e)
        {
            string valor = Conexoes.Utilz.SelecionarObjeto(Core.CADPurlin.GetLayers(), null, "Selecione");
            if(valor!=null && valor.Length>0)
            {
                this.furos_manuais_layer.Content = valor;
                Core.CADPurlin.MapeiaFurosManuaisLayer = valor;
            }
        }

        private void mapeia(object sender, RoutedEventArgs e)
        {

            if(Core.CADPurlin.GetCorrentePadrao()==null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada a corrente padrão setada: id {Core.CADPurlin.id_corrente}");
                return;
            }
            if (Core.CADPurlin.GetTirantePadrao() == null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrado o tirante padrão setado: id {Core.CADPurlin.id_tirante}");
                return;
            }
            if (Core.CADPurlin.GetPurlinPadrao() == null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrado a purlin padrão setada: id {Core.CADPurlin.id_purlin}");
                return;
            }
            if (Ut.GetCORRENTES().GetPecas().Count==0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhuma corrente da família {Core.CADPurlin.RM_Familia_Corrente}");
                return;
            }
            if (Ut.GetTIRANTES().GetPecas().Count == 0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhum tirante da família {Core.CADPurlin.RM_Familia_Tirante}");
                return;
            }
            if (Ut.GetPURLINS().GetPecas().Count == 0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhuma purlin da família {Core.CADPurlin.RM_Familia_Purlin}");
                return;
            }
            if (correntes_multilines.Items.Count == 0 && (bool)mapeia_correntes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de corrente nos estilos mapeáveis.");
                return;
            }

            if (tirantes_multilines.Items.Count == 0 && (bool)mapeia_tirantes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de corrente nos estilos mapeáveis.");
                return;
            }

            if (tercas_multilines.Items.Count == 0 && (bool)mapeia_tercas.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de corrente nos estilos mapeáveis.");
                return;
            }
            if (Core.CADPurlin.MapeiaFurosManuaisLayer == "" && (bool)mapeia_furos.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear os furos manuais sem definir uma layer padrão");
                return;
            }
            Core.CADPurlin.MapearCorrentes = (bool)this.mapeia_correntes.IsChecked;
            Core.CADPurlin.MapearTercas = (bool)this.mapeia_tercas.IsChecked;
            Core.CADPurlin.MapearTirantes = (bool)this.mapeia_tirantes.IsChecked;
            this.Visibility = Visibility.Collapsed;

            this.Close();
            Core.CADPurlin.Mapear();
        }

        private void editar_transpasse(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetTranspasse();
        }

        private void editar_ficha(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetFicha();
        }

        private void editar_trocar_perfil(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetPerfil();
        }

        private void editar_furacao_suporte(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetSuporte();
        }

        private void editar_ver_croqui(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetSuporte();
        }

        private void editar_criar_manual(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.PurlinManual();
        }

        private void editar_edicao_completa(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.EdicaoCompleta();
        }

        private void editar_corrente(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrente();
        }

        private void editar_corrente_descontar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrenteDescontar();
        }

        private void editar_corrente_fixador(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrenteFixador();
        }

        private void apagar_blocos(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.ApagarBlocosPurlin();
        }

        private void apagar_tudo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.ApagarBlocosPurlin();
        }

        private void desnha_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.desenharmline();
        }

        private void troca_polyline_por_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.substituirpolylinepormultiline();
        }

        private void trocar_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.mudarmultiline();
        }

        private void marcar_inserir_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.Exportar(Conexoes.Utilz.Pergunta("Gerar Tabela?"), Conexoes.Utilz.Pergunta("Exportar arquivo .RM?"));
        }

        private void gerar_croquis(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.GerarCroquis();

        }


    }
}
