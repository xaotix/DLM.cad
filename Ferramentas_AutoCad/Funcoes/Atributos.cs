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
        public static void Set(this List<Autodesk.AutoCAD.DatabaseServices.BlockReference> blocos, Transaction acTrans, db.Linha valores)
        {
            using (var docLock = acDoc.LockDocument())
            {
                foreach (var bloco in blocos)
                {
                    Set(bloco, acTrans, valores);
                }
            }
        }
        public static void Set(this BlockReference myBlockRef, Transaction acTrans, string tag, string value)
        {
            if (myBlockRef == null) { return; }
            var linha = new db.Linha();
            linha.Add(tag, value);
            Set(myBlockRef, acTrans, linha);
        }
        public static void Set(this BlockReference myBlockRef, Transaction acTrans, db.Linha atributos)
        {
            using (var docLock = acDoc.LockDocument())
            {
                AttributeCollection attCol = myBlockRef.AttributeCollection;
                foreach (ObjectId attId in attCol)
                {
                    AttributeReference attRef = acTrans.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                    if (atributos.Contem(attRef.Tag))
                    {
                        attRef.UpgradeOpen();
                        var celula = atributos[attRef.Tag.ToUpper()];
                        var valor = celula.ValorCadastro();
                        if (valor == "NULL")
                        {
                            valor = "";
                        }
                        attRef.TextString = valor;
                    }
                }
            }
        }
        public static BlockAttributes GetAttributes(this BlockReference bloco, bool somente_visiveis = true, Database acCurDb = null)
        {
            BlockAttributes retorno = new BlockAttributes(bloco, false);

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
                        retorno.Celulas.Add(new db.Celula(acAttRef.Tag, acAttRef.TextString));
                    }
                }
            }

            return retorno;
        }
        public static db.Celula GetValor(this BlockReference bloco, string atributo)
        {
            var blktag = GetAttributes(bloco);
            return blktag.Get(atributo);
        }
    }
}
