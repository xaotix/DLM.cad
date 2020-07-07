using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class Logger
    {
        public static void Log(string mensagem)
        {
            string caminhoArquivo = @"C:\Temp\log_ferramentas_autocad.txt";
            if (!File.Exists(caminhoArquivo))
            {
                Stream arquivo = File.Create(caminhoArquivo);
                arquivo.Close();
            }

            StreamWriter str = new StreamWriter(caminhoArquivo, true);
            str.WriteLine(string.Format("{0} - {1} ********* Log => {2}", DateTime.Today.ToString("dd/MM/yyyy"), DateTime.Now.ToString("hh:MM"), mensagem));
            str.Close();
        }
    }
}
