using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.CAD;

namespace Ferramentas_DLM
{
   public static class Atributos
    {
        public static List<List<string>> GetStr(BlockReference bloco)
        {
            List<List<string>> retorno = new List<List<string>>();

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {

                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    var attCol = bloco.AttributeCollection;
                    foreach (ObjectId objID in attCol)
                    {
                        DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                        AttributeReference acAttRef = dbObj as AttributeReference;
                        retorno.Add(new List<string> { acAttRef.Tag, acAttRef.TextString });
                    }
                }
            }
            return retorno;
        }
        public static void Set(List<Autodesk.AutoCAD.DatabaseServices.BlockReference> blocos, Transaction tr, Hashtable valores)
        {

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                foreach(var bloco in blocos)
                {
                    AttributeCollection attCol = bloco.AttributeCollection;
                    foreach (ObjectId attId in attCol)
                    {

                        AttributeReference att = tr.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                        if (valores.ContainsKey(att.Tag.ToUpper()))
                        {
                            att.UpgradeOpen();
                            att.TextString = valores[att.Tag.ToUpper()].ToString();
                        }
                    }
                }
            }
        }
        public static void Set(BlockReference myBlockRef, Transaction tr, string tag, string valor)
        {
           if(myBlockRef == null) { return; }
            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                AttributeCollection attCol = myBlockRef.AttributeCollection;
                foreach (ObjectId attId in attCol)
                {
                    AttributeReference att = tr.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                    if (att.Tag.ToUpper().Replace(" ", "") == tag.ToUpper().Replace(" ", ""))
                    {
                        att.UpgradeOpen();
                        att.TextString = valor;
                    }
                }
            }
        }
        public static void Set(BlockReference myBlockRef, Transaction tr, Hashtable t)
        {

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                AttributeCollection attCol = myBlockRef.AttributeCollection;
                foreach (ObjectId attId in attCol)
                {

                    AttributeReference att = tr.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                    if (t.ContainsKey(att.Tag.ToUpper()))
                    {
                        att.UpgradeOpen();
                        att.TextString = t[att.Tag.ToUpper()].ToString();
                    }
                }
            }
        }
        public static BlocoTag GetBlocoTag(BlockReference bloco, bool somente_visiveis = true, Database acCurDb = null)
        {

            BlocoTag retorno = new BlocoTag(bloco,false);

            if (acCurDb == null)
            {
                acCurDb = CAD.acCurDb;
            }



            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                //var attCol = btr.AttributeCollection;

                var attCol = bloco.AttributeCollection;
                foreach (ObjectId objID in attCol)
                {
                    AttributeReference acAttRef = acTrans.GetObject(objID, OpenMode.ForRead) as AttributeReference;

                    if (!acAttRef.Visible && somente_visiveis)
                    {
                        /*é pra evitar de puxar os dados de atributos ocultos das sets do bloco dinamico*/
                    }
                    else
                    {
                        retorno.Atributos.Add(new CelulaTag(acAttRef.Tag, acAttRef.TextString, acAttRef));
                    }
                }
            }

            return retorno;
        }
        public static CelulaTag GetValor(BlockReference bloco, string atributo)
        {
            var s = GetBlocoTag(bloco);
            return s.Get(atributo);
        }
    }
}
