using Conexoes;
using DLM.vars;
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

namespace DLM.cad.Menus
{
    /// <summary>
    /// Interação lógica para Purlin.xam
    /// </summary>
    public partial class Purlin : Window
    {

        public Purlin()
        {

            InitializeComponent();
            this.Title = $"xPurlin V." + Conexoes.Utilz.GetVersao(Cfg.Init.CAD_DLL_Local) + $" [{Cfg_User.Init.MySQL_Servidor}]";
            
        }

        private void terca_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarPurlin(null);

            if(s!=null)
            {
                Core.GetCADPurlin().SetPurlin(s.id_codigo);
                this.terca.Content = s.COD_DB;
            }
        }

        private void corrente_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarCorrente();

            if (s != null)
            {
                Core.GetCADPurlin().SetCorrente(s.id_codigo);
                this.corrente.Content = s.COD_DB;
            }
        }



        private void tirante_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarTirante();

            if (s != null)
            {
                Core.GetCADPurlin().SetTirante(s.id_codigo);
                this.tirante.Content = s.COD_DB;
            }
        }

        private void transpasse_Click(object sender, RoutedEventArgs e)
        {
            bool confirmado = false;
            double valor = Core.GetCADPurlin().TranspassePadrao.Prompt(out confirmado);
            if(confirmado)
            {
                Core.GetCADPurlin().TranspassePadrao = valor;
                this.transpasse.Content = valor;
            }

        }

        private void furo_offset_apoio_Click(object sender, RoutedEventArgs e)
        {
            int valor = Core.GetCADPurlin().OffsetApoio.Prompt();
            Core.GetCADPurlin().OffsetApoio = valor;
            this.furo_offset_apoio.Content = valor;
        }

        private void ficha_Click(object sender, RoutedEventArgs e)
        {
            string valor = Core.GetCADPurlin().FichaDePintura.Prompt();
            if(valor!=null)
            {
                Core.GetCADPurlin().FichaDePintura = valor;
                this.ficha.Content = valor;
            }
        }

        private void furos_manuais_layer_Click(object sender, RoutedEventArgs e)
        {
            string valor = Core.GetCADPurlin().GetLayers().ListaSelecionar();
            if(valor!=null && valor.Length>0)
            {
                this.furos_manuais_layer.Content = valor;
                Core.GetCADPurlin().MapeiaFurosManuaisLayer = valor;
            }
        }

        private void mapeia(object sender, RoutedEventArgs e)
        {

            if(Core.GetCADPurlin().GetCorrentePadrao()==null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada a corrente padrão setada: id {Core.GetCADPurlin().id_corrente}");
                return;
            }
            if (Core.GetCADPurlin().GetTirantePadrao() == null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrado o tirante padrão setado: id {Core.GetCADPurlin().id_tirante}");
                return;
            }
            if (Core.GetCADPurlin().GetPurlinPadrao() == null)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrado a purlin padrão setada: id {Core.GetCADPurlin().id_purlin}");
                return;
            }
            if (Ut.GetCORRENTES().GetPecas().Count==0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhuma corrente da família {Core.GetCADPurlin().RM_Familia_Corrente}");
                return;
            }
            if (Ut.GetTIRANTES().GetPecas().Count == 0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhum tirante da família {Core.GetCADPurlin().RM_Familia_Tirante}");
                return;
            }
            if (Ut.GetPURLINS().GetPecas().Count == 0)
            {
                Conexoes.Utilz.Alerta($"Não foi encontrada nenhuma purlin da família {Core.GetCADPurlin().RM_Familia_Purlin}");
                return;
            }




            if (correntes_multilines.Items.Count == 0 && (bool)mapeia_correntes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de [corrente] nos estilos mapeáveis.");
                return;
            }

            if (tirantes_multilines.Items.Count == 0 && (bool)mapeia_tirantes.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de [tirante] nos estilos mapeáveis.");
                return;
            }

            if (tercas_multilines.Items.Count == 0 && (bool)mapeia_tercas.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não foi encontrada nenhuma multiline de [purlin] nos estilos mapeáveis.");
                return;
            }
            if (Core.GetCADPurlin().MapeiaFurosManuaisLayer == "" && (bool)mapeia_furos.IsChecked)
            {
                Conexoes.Utilz.Alerta("Não é possivel mapear os furos manuais sem definir uma layer padrão");
                return;
            }
            Core.GetCADPurlin().MapearCorrentes = (bool)this.mapeia_correntes.IsChecked;
            Core.GetCADPurlin().MapearTercas = (bool)this.mapeia_tercas.IsChecked;
            Core.GetCADPurlin().MapearTirantes = (bool)this.mapeia_tirantes.IsChecked;
            this.Visibility = Visibility.Collapsed;

            this.Close();
            Core.GetCADPurlin().Mapear();
        }

        private void terca_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarPurlinSuporte();

            if (s != null)
            {
                Core.GetCADPurlin().SetPurlinSuporte(s.id_codigo);
                this.terca_suporte.Content = s.COD_DB;
            }
        }

        private void corrente_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarCorrenteSuporte();

            if (s != null)
            {
                Core.GetCADPurlin().SetCorrenteSuporte(s.id_codigo);
                this.corrente_suporte.Content = s.COD_DB;
            }
        }

        private void tirante_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarTiranteSuporte();

            if (s != null)
            {
                Core.GetCADPurlin().SetTiranteSuporte(s.id_codigo);
                this.tirante_suporte.Content = s.COD_DB;
            }
        }
    }
}
