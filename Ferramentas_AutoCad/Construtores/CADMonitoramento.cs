using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.CAD;

namespace Ferramentas_DLM
{
    public class CADMonitoramento :CADBase
    {
        public void MonitorarRotinas()
        {

            acDoc.CommandEnded += new CommandEventHandler(CapturarEventoFinalizado);
            acDoc.CommandWillStart += new CommandEventHandler(CapturarEventoIniciado);
            acDoc.LispWillStart += MonitoraLisp;
            acDoc.LispEnded += MonitoraLispFim;
           
        }

        private void MonitoraLispFim(object sender, EventArgs e)
        {
            Log.Add(e.ToString());
            Log.Add("Lisp finalizado");
        }

        private void MonitoraLisp(object sender, LispWillStartEventArgs e)
        {
            Log.Add(e.FirstLine);
            Log.Add(e.ToString());
        }

        public List<string> Log { get; set; } = new List<string>();

        public void CapturarEventoFinalizado(object e, CommandEventArgs eventArgs)
        {
            Log.Add(eventArgs.GlobalCommandName);
           
        }

        public void CapturarEventoIniciado(object e, CommandEventArgs eventArgs)
        {
            Log.Add(eventArgs.GlobalCommandName);
           
        }

        public void SalvarLog()
        {

            var destino = Conexoes.Utilz.SalvarArquivo();
            if(destino!="" && destino!=null)
            {
                Conexoes.Utilz.Arquivo.Gravar(destino, Log);
                Log.Clear();
                Conexoes.Utilz.Abrir(destino);
            }
        }


        public CADMonitoramento()
        {
            MonitorarRotinas();
        }
    }
}
