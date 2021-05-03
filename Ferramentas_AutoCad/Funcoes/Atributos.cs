using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static void Set(BlockReference myBlockRef, Transaction tr, string tag, string valor)
        {
           
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
        public static DB.Linha GetLinha(BlockReference bloco, Database acCurDb = null)
        {
            DB.Linha retorno = new DB.Linha();

            if(acCurDb==null)
            {
                acCurDb = CAD.acCurDb;
            }

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    var attCol = bloco.AttributeCollection;
                    foreach (ObjectId objID in attCol)
                    {
                        DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                        AttributeReference acAttRef = dbObj as AttributeReference;
                        retorno.Add(acAttRef.Tag, acAttRef.TextString);
                    }
                }
            }
            retorno.Tabela = bloco.Name;
            return retorno;
        }
        public static DB.Valor Get(BlockReference bloco, string atributo)
        {
            var s = GetLinha(bloco);
            return s.Get(atributo);
        }
    }
}
