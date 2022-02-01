using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DLM.cad
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

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
    }

}
