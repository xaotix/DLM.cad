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
using static DLM.cad.CAD;

namespace DLM.cad
{
    public static class Atributos
    {
        public static List<List<string>> GetStr(this BlockReference blck)
        {
            List<List<string>> retorno = new List<List<string>>();
            using (var acTrans = acDoc.acTrans())
            {
                var attCol = blck.AttributeCollection;
                foreach (ObjectId objID in attCol)
                {
                    DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                    AttributeReference acAttRef = dbObj as AttributeReference;
                    retorno.Add(new List<string> { acAttRef.Tag, acAttRef.TextString });
                }
            }
            return retorno;
        }
        public static void Set(this  List<Autodesk.AutoCAD.DatabaseServices.BlockReference> blocos, Transaction acTrans, Hashtable valores)
        {

            using (var docLock = acDoc.LockDocument())
            {
                foreach (var bloco in blocos)
                {
                    AttributeCollection attCol = bloco.AttributeCollection;
                    foreach (ObjectId attId in attCol)
                    {

                        AttributeReference att = acTrans.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                        if (valores.ContainsKey(att.Tag.ToUpper()))
                        {
                            att.UpgradeOpen();
                            att.TextString = valores[att.Tag.ToUpper()].ToString();
                        }
                    }
                }
            }
        }
        public static void Set(this BlockReference myBlockRef, Transaction acTrans, string tag, string valor)
        {
            if (myBlockRef == null) { return; }
            using (var docLock = acDoc.LockDocument())
            {
                AttributeCollection attCol = myBlockRef.AttributeCollection;
                foreach (ObjectId attId in attCol)
                {
                    AttributeReference att = acTrans.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                    if (att.Tag.ToUpper().Replace(" ", "") == tag.ToUpper().Replace(" ", ""))
                    {
                        att.UpgradeOpen();
                        att.TextString = valor;
                    }
                }
            }
        }
        public static void Set(this BlockReference myBlockRef, Transaction acTrans, Hashtable atributos)
        {
            using (var docLock = acDoc.LockDocument())
            {
                AttributeCollection attCol = myBlockRef.AttributeCollection;
                foreach (ObjectId attId in attCol)
                {
                    AttributeReference att = acTrans.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                    if (atributos.ContainsKey(att.Tag.ToUpper()))
                    {
                        att.UpgradeOpen();
                        att.TextString = atributos[att.Tag.ToUpper()].ToString();
                    }
                }
            }
        }
        public static BlocoTag GetBlocoTag(this BlockReference bloco, bool somente_visiveis = true, Database acCurDb = null)
        {

            BlocoTag retorno = new BlocoTag(bloco, false);

            if (acCurDb == null)
            {
                acCurDb = CAD.acCurDb;
            }

            using (var acTrans = acCurDb.acTrans())
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
                        retorno.Atributos.Add(new db.Celula(acAttRef.Tag, acAttRef.TextString));
                    }
                }
            }

            return retorno;
        }
        public static db.Celula GetValor(this BlockReference bloco, string atributo)
        {
            var blktag = GetBlocoTag(bloco);
            return blktag.Get(atributo);
        }
    }
}
