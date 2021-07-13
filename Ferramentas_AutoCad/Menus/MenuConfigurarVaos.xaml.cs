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
    /// Interação lógica para MenuConfigurarVaos.xam
    /// </summary>
    public partial class MenuConfigurarVaos : Window
    {
        List<VaoObra> vaos { get; set; } = new List<VaoObra>();
        public MenuConfigurarVaos(List<VaoObra> vaos)
        {
            this.vaos = vaos;
            InitializeComponent();
            this.Update();


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

        private void set_tre(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;
            if (ss is ObjetoPurlin)
            {
                ObjetoPurlin sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    bool status = false;
                    var novovao = Conexoes.Utilz.Prompt(sel.TRE, out status);

                    if (status)
                    {
                        sel.TRE = novovao;

                        sel.PurlinEsquerda.TRD = novovao;
                        Update();
                    }
                    return;
                }
            }

        }

        private void Update()
        {
            this.ListaVaos.ItemsSource = null;
            this.ListaHeader.ItemsSource = null;

            this.ListaVaos.ItemsSource = this.vaos;
            this.ListaHeader.ItemsSource = this.vaos;
        }

        private void set_trd(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;
            if (ss is ObjetoPurlin)
            {
                ObjetoPurlin sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    bool status = false;
                    var novovao = Conexoes.Utilz.Prompt(sel.TRD, out status);

                    if (status)
                    {
                        sel.TRD = novovao;
                        sel.PurlinDireita.TRE = novovao;
                        Update();
                    }
                    return;
                }
            }
        }

        private void set_purlin(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;

            if(ss is VaoObra)
            {
                VaoObra sel = ((FrameworkElement)sender).DataContext as VaoObra;
                if (sel != null)
                {
                    var purlin = Utilidades.SelecionarPurlin(sel.CADPurlin.GetPecaPurlin());
                    if (purlin != null)
                    {
                        foreach(var p in sel.GetPurlins())
                        {
                        p.SetPeca(purlin.id_db);

                        }
                        Update();
                    }
                }
            }
            else if(ss is ObjetoPurlin)
            {
                ObjetoPurlin sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    var purlin = Utilidades.SelecionarPurlin(sel.GetPeca());
                    if (purlin != null)
                    {
                        sel.SetPeca(purlin.id_db);
                        Update();
                    }
                }
            }
  

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public bool confirmado { get; set; } = false;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.confirmado = true;
            this.Close();
        }
    }
}
