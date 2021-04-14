using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class Monitoramento :ClasseBase
    {
        public void MonitorarRotinas()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;
            doc.CommandEnded += new CommandEventHandler(CapturarEventoFinalizado);
            doc.CommandWillStart += new CommandEventHandler(CapturarEventoIniciado);
            doc.LispWillStart += MonitoraLisp;
            doc.LispEnded += MonitoraLispFim;
           
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
        static void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            DBObject dbObj = e.DBObject;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Object pOwner = tr.GetObject(dbObj.OwnerId, OpenMode.ForRead);

                Application.ShowAlertDialog(string.Format("Nome do Objeto: {0}, ID: {1}", e.DBObject.GetType().ToString(), dbObj.OwnerId.ToString()));
                tr.Commit();
            }
        }

        public Monitoramento()
        {
            MonitorarRotinas();
        }
    }
}
