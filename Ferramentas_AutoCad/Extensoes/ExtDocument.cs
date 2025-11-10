using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Conexoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DLM.cad.CAD;

namespace DLM.cad
{
    public static class ExtDocument
    {

        public static void Apagar(this Document acDoc, List<Entity> entities)
        {
            try
            {
                if (acDoc == null)
                {
                    acDoc = CAD.acDoc;
                }
                if (entities.Count == 0) { return; }
                using (var docLock = acDoc.LockDocument())
                {
                    using (var acTrans = acDoc.acTransST())
                    {
                        foreach (var b in entities)
                        {
                            Entity acEnt = acTrans.GetObject(b.ObjectId, OpenMode.ForWrite) as Entity;
                            acEnt.Erase(true);
                        }
                        acTrans.Commit();

                    }
                    editor.Regen();
                }
            }
            catch (System.Exception ex)
            {
                ex.Alerta();
            }
        }
        private static bool estaRodando { get; set; } = false;
        public static int Comando(this Document doc, params object[] comando)
        {

            /*DEPOIS DE UMA CARALHADA DE TENTATIVAS E ERROS, CHEGUEI NESSA SOLUÇÃO SIMPLES.*/
            try
            {
                //doc.CommandWillStart += new CommandEventHandler(ComandoIniciou);
                //doc.CommandEnded += new CommandEventHandler(ComandoFinalizou);
                //estaRodando = true;
                //var sw = new Stopwatch();
                //sw.Start();
                doc.AcadDocument.GetType().InvokeMember("SendCommand", System.Reflection.BindingFlags.InvokeMethod, null, doc.AcadDocument, new object[] { string.Join("\n", comando) });
                //doc.SendStringToExecute(string.Join("\n", comando), false, false, false);
                //while (estaRodando)
                //{
                //    System.Threading.Thread.Sleep(100);
                //    if (sw.Elapsed.Seconds > 20)
                //    {
                //        //envia um ESC
                //        doc.SendStringToExecute("^c^c ", true, false, false);
                //        estaRodando = false;
                //        break;
                //    }
                //}
                //sw.Stop();
                //doc.CommandWillStart -= new CommandEventHandler(ComandoIniciou);
                //doc.CommandEnded -= new CommandEventHandler(ComandoFinalizou);
            }
            catch (System.Exception ex)
            {
                DLM.log.Log(ex);
                return -1;
            }

            return 1;
        }


        public static void SalvarEFechar(this Document doc, params object[] comando)
        {
            doc.AcadDocument.GetType().InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, doc.AcadDocument, null);
            doc.CloseAndDiscard();
        }


        private static void ComandoIniciou(object sender, CommandEventArgs e)
        {
            estaRodando = true;
        }
        private static void ComandoFinalizou(object sender, CommandEventArgs e)
        {
            estaRodando = false;
        }

        public static List<Layout> GetLayouts(this Document acDoc)
        {
            if (acDoc == null)
            {
                acDoc = CAD.acDoc;
            }
            List<Layout> retorno = new List<Layout>();
            using (var acTrans = acDoc.acTrans())
            {
                DBDictionary lays = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                foreach (DBDictionaryEntry item in lays)
                {
                    Layout acLyrTblRec;
                    acLyrTblRec = acTrans.GetObject(item.Value, OpenMode.ForWrite) as Layout;

                    if (acLyrTblRec != null)
                    {
                        retorno.Add(acLyrTblRec);


                        var views = acLyrTblRec.GetViewports();
                        foreach (ObjectId view in views)
                        {
                            Viewport vp = acTrans.GetObject(view, OpenMode.ForWrite) as Viewport;
                        }

                    }
                }
                acTrans.Abort();
            }
            return retorno;
        }


        public static void SetLts(this Document doc, int valor = 10)
        {
            SetVar(doc, "LTSCALE", valor);
        }
        public static void SetVar(this Document doc, string var, object valor)
        {
            dynamic Ddoc = doc.AcadDocument;
            Ddoc.SetVariable(var, valor);
        }
        public static void IrLayout(this Document acDoc)
        {
            using (var docLock = acDoc.LockDocument())
            {
                var lista = acDoc.GetLayouts().Select(x => x.LayoutName).ToList().FindAll(x => x.ToUpper() != "MODEL");
                if (lista.Count > 0)
                {
                    LayoutManager.Current.CurrentLayout = lista[0];
                }
            }
        }
    }
}
