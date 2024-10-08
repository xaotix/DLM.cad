﻿using Conexoes;
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
    /// Interação lógica para MenuConfigurarVaos.xam
    /// </summary>
    public partial class MenuConfigurarVaos : Window
    {
        GradeEixos grade { get; set; }
        public MenuConfigurarVaos(GradeEixos grade)
        {
            this.grade = grade;
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
                var sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    var valor = sel.TRE.Prompt();

                    if (valor!=null)
                    {
                        sel.TRE = valor.Value;

                        sel.PurlinEsquerda.TRD = valor.Value;
                        Update();
                    }
                    return;
                }
            }

        }

        private void Update()
        {
            var pp = grade.GetCanvasVertical(this.pranchazoom.GetCanvas());
           
        }

        private void set_trd(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;
            if (ss is ObjetoPurlin)
            {
                var sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    var valor = sel.TRD.Prompt();

                    if (valor!=null)
                    {
                        sel.TRD = valor.Value;
                        sel.PurlinDireita.TRE = valor.Value;
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
                    var purlin = Ut.SelecionarPurlin(Core.GetCADPurlin().GetPurlinPadrao());
                    if (purlin != null)
                    {
                        foreach(var p in sel.GetPurlins())
                        {
                        p.SetPeca(purlin);

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
                    var purlin = Ut.SelecionarPurlin(sel.GetPeca());
                    if (purlin != null)
                    {
                        sel.SetPeca(purlin);
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
            Core.GetCADPurlin().InserirBlocos(this.grade);
        }

        private void set_corrente(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;

            if (ss is VaoObra)
            {
                VaoObra sel = ((FrameworkElement)sender).DataContext as VaoObra;
                if (sel != null)
                {
                    var purlin = Ut.SelecionarCorrente();
                    if (purlin != null)
                    {
                        foreach (var p in sel.GetCorrentes())
                        {
                            p.SetPeca(purlin);

                        }
                        Update();
                    }
                }
            }
            else if (ss is ObjetoCorrente)
            {
                ObjetoCorrente sel = ((FrameworkElement)sender).DataContext as ObjetoCorrente;
                if (sel != null)
                {
                    var purlin = Ut.SelecionarCorrente();
                    if (purlin != null)
                    {
                        sel.SetPeca(purlin);
                        Update();
                    }
                }
            }
        }

        private void set_tirante(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;

            if (ss is VaoObra)
            {
                VaoObra sel = ((FrameworkElement)sender).DataContext as VaoObra;
                if (sel != null)
                {
                    var purlin = Ut.SelecionarTirante();
                    if (purlin != null)
                    {
                        foreach (var p in sel.GetTirantes())
                        {
                            p.SetPeca(purlin);

                        }
                        Update();
                    }
                }
            }
            else if (ss is ObjetoTirante)
            {
                ObjetoTirante sel = ((FrameworkElement)sender).DataContext as ObjetoTirante;
                if (sel != null)
                {
                    var purlin = Ut.SelecionarTirante();
                    if (purlin != null)
                    {
                        sel.SetPeca(purlin);
                        Update();
                    }
                }
            }
        }

        private void set_fbd(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;

            if (ss is VaoObra)
            {
                VaoObra sel = ((FrameworkElement)sender).DataContext as VaoObra;
                if (sel != null)
                {
                    var valor = 0.Prompt();
                    if (valor != null)
                    {
                        foreach (var p in sel.GetPurlins())
                        {
                            p.FBD_Comp = valor.Value;

                        }
                        Update();
                    }
                }
            }
            else if (ss is ObjetoPurlin)
            {
                ObjetoPurlin sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    var valor = 0.Prompt();
                    if (valor != null)
                    {
                        sel.FBD_Comp = valor.Value;
                        Update();
                    }
                }
            }
        }

        private void set_fbe(object sender, RoutedEventArgs e)
        {
            var ss = ((FrameworkElement)sender).DataContext;

            if (ss is VaoObra)
            {
                VaoObra sel = ((FrameworkElement)sender).DataContext as VaoObra;
                if (sel != null)
                {
                    var valor = 0.Prompt();
                    if (valor!=null)
                    {
                        foreach (var p in sel.GetPurlins())
                        {
                            p.FBE_Comp = valor.Value;

                        }
                        Update();
                    }
                }
            }
            else if (ss is ObjetoPurlin)
            {
                ObjetoPurlin sel = ((FrameworkElement)sender).DataContext as ObjetoPurlin;
                if (sel != null)
                {
                    var valor = 0.Prompt();
                    if (valor!=null)
                    {
                        sel.FBE_Comp = valor.Value;
                        Update();
                    }
                }
            }
        }
    }
}
