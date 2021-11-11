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
            string valor = Conexoes.Utilz.Selecao.SelecionarObjeto(Core.CADPurlin.GetLayers(), null, "Selecione");
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

        private void terca_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarPurlinSuporte();

            if (s != null)
            {
                Core.CADPurlin.SetPurlinSuporte(s.id_db);
                this.terca_suporte.Content = s.COD_DB;
            }
        }

        private void corrente_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarCorrenteSuporte();

            if (s != null)
            {
                Core.CADPurlin.SetCorrenteSuporte(s.id_db);
                this.corrente_suporte.Content = s.COD_DB;
            }
        }

        private void tirante_suporte_Click(object sender, RoutedEventArgs e)
        {
            var s = Ut.SelecionarTiranteSuporte();

            if (s != null)
            {
                Core.CADPurlin.SetTiranteSuporte(s.id_db);
                this.tirante_suporte.Content = s.COD_DB;
            }
        }
    }
}
