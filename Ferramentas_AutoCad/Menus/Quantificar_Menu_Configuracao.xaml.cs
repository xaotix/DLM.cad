using Autodesk.AutoCAD.DatabaseServices;
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
    /// Interação lógica para Quantificar_Menu_Configuracao.xam
    /// </summary>
    public partial class Quantificar_Menu_Configuracao : Window
    {
        public bool confirmado { get; set; } = false;
        public List<PCQuantificar> original { get; set; } = new List<PCQuantificar>();
        public List<PCQuantificar> filtro { get; set; } = new List<PCQuantificar>();
        public Quantificar_Menu_Configuracao(List<PCQuantificar> pecas)
        {
            original = pecas;
            filtro.AddRange(pecas);
            InitializeComponent();
            this.Update();
        }

        private void set_nome(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }

            if(sel.Tipo== Tipo_Objeto.Texto)
            {
                return;
            }

            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0)
            {
                var qtd = Conexoes.Utilz.Prompt(sel.Descricao, 30);
                if (qtd != null && qtd != "")
                {
                    sel.Nome = qtd;
                    Update();
                }
                return;
            }


            var cols = sel.GetAtributos();

            if (cols.Count>0)
            {
                string col = Conexoes.Utilz.SelecionaCombo(cols, null);
                if(col!=null && col!="")
                {
                    try
                    {
                        this.lista.ItemsSource = null;
                        var nv = sel.Agrupar(col);
                        this.filtro.Remove(sel);
                        this.filtro.AddRange(nv);
                        this.filtro = this.filtro.OrderBy(x => x.Nome).ToList();
                        this.lista.ItemsSource = this.filtro;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void editar_filtro_quantidade(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }


            if (sel.Tipo== Tipo_Objeto.Texto | sel.GetAtributos().Count==0)
            {
                bool status = false;
                var qtd = Conexoes.Utilz.Prompt(sel.Quantidade, out status);
                if(status)
                {
                    sel.Quantidade = qtd;
                    Update();
                }
                return;
            }


            if (sel.GetAtributos().Count > 0)
            {
                var col = Conexoes.Utilz.SelecionaCombo(sel.GetAtributos(), null);
                if (col != null)
                {
                    lista.ItemsSource = null;
                    sel.SetQtdPorAtributo(col);
                  
                    Update();
                }
            }
        }

        private void Update()
        {
            filtro = filtro.GroupBy(x => x.Nome).Select(x => new PCQuantificar(x.ToList())).ToList().OrderBy(x => x.Nome).ToList();
            this.lista.ItemsSource = null;
            this.lista.ItemsSource = filtro;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // this.DialogResult = false;
            this.Close();

        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;
            this.confirmado = true;
            this.Close();
        }

        private void editar_descricao(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }
            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0)
            {
                var qtd = Conexoes.Utilz.Prompt(sel.Descricao,30);
                if (qtd!=null && qtd!="")
                {
                    sel.Descricao = qtd;
                    Update();
                }
                return;
            }


            if (sel.GetAtributos().Count > 0)
            {
                var col = Conexoes.Utilz.SelecionaCombo(sel.GetAtributos(), null);
                if (col != null)
                {
                    lista.ItemsSource = null;
                    sel.SetDescPorAtributo(col);

                    Update();
                }
              
            }

        }
    }
}
