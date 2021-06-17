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

            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
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
                        var nv = sel.Agrupar(new List<string> { col },sel.Nome_Bloco);
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


            if (sel.Tipo== Tipo_Objeto.Texto | sel.GetAtributos().Count==0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
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
            filtro = filtro.GroupBy(x => x.Numero + "|" + x.Nome).Select(x => new PCQuantificar(x.ToList())).ToList().OrderBy(x => x.Nome).ToList();
            this.lista.ItemsSource = null;
            this.lista.ItemsSource = filtro.OrderBy(x => x.Numero + "|" + x.Nome).ToList(); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // this.DialogResult = false;
            this.Close();

        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;
            this.filtro = this.lista.Items.Cast<PCQuantificar>().ToList();
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
            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
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



        private void editar_filtro_numero(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }
            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
            {
                var qtd = Conexoes.Utilz.Prompt(sel.Numero, 4);
                if (qtd != null && qtd != "")
                {
                    sel.Numero = qtd;
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
                    sel.SetNumeroPorAtributo(col);

                    Update();
                }

            }
        }

        private void editar_filtro_Destino(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }
            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
            {
                var qtd = Conexoes.Utilz.Prompt(sel.Destino, 4);
                if (qtd != null && qtd != "")
                {
                    sel.Destino = qtd;
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
                    sel.SetDestinoPorAtributo(col);

                    Update();
                }

            }
        }

        private void editar_filtro_familia(object sender, RoutedEventArgs e)
        {
            PCQuantificar sel = ((FrameworkElement)sender).DataContext as PCQuantificar;
            if (sel == null)
            {
                return;
            }
            if (sel.Tipo == Tipo_Objeto.Texto | sel.GetAtributos().Count == 0 | sel.Nome_Bloco == Constantes.Bloco_3D_Montagem_Tecnometal)
            {
                var qtd = Conexoes.Utilz.Prompt(sel.Destino, 4);
                if (qtd != null && qtd != "")
                {
                    sel.Destino = qtd;
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
                    sel.SetFamiliaPorAtributo(col);

                    Update();
                }

            }
        }

        private void set_familia(object sender, RoutedEventArgs e)
        {
           if(selecoes.Count>0)
            {
                var nova = Conexoes.Utilz.Prompt("Digite", "", selecoes[0].Familia);
                if(nova==null | nova == "") { return; }
                foreach(var s in selecoes)
                {
                    s.Familia = nova;
                }
                Update();
            }
        }
        public List<PCQuantificar> selecoes { get; set; } = new List<PCQuantificar>();
        private void lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selecoes = lista.SelectedItems.Cast<object>().ToList().FindAll(x => x is PCQuantificar).Select(x=> x as PCQuantificar).ToList();
        }

        private void set_descricao(object sender, RoutedEventArgs e)
        {
            if (selecoes.Count > 0)
            {
                var nova = Conexoes.Utilz.Prompt("Digite", "", selecoes[0].Descricao);
                if (nova == null | nova == "") { return; }
                foreach (var s in selecoes)
                {
                    s.Descricao = nova;
                }
                Update();
            }

        }

        private void set_quantidade(object sender, RoutedEventArgs e)
        {
            if (selecoes.Count > 0)
            {
                var nova = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite", "", selecoes[0].Quantidade.ToString()));
                if (nova<=0) { return; }
                foreach (var s in selecoes)
                {
                    s.Quantidade = nova;
                }
                Update();
            }
        }

        private void set_numero(object sender, RoutedEventArgs e)
        {
            if (selecoes.Count > 0)
            {
                var nova = Conexoes.Utilz.Prompt("Digite", "", selecoes[0].Numero);
                if (nova == null | nova == "") { return; }
                foreach (var s in selecoes)
                {
                    s.Numero = nova;
                }
                Update();
            }
        }
    }
}
