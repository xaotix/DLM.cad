using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
   public static class Atributos
    {
        //public static void SynchronizeAttributes(BlockTableRecord target)
        //{
        //    if (target == null)
        //    {
        //        return;
        //    }

        //    Transaction tr = target.Database.TransactionManager.TopTransaction;
        //    if (tr == null)
        //    {
        //        return;
        //    }
        //    RXClass attDefClass = RXClass.GetClass(typeof(AttributeDefinition));
        //    List<AttributeDefinition> attDefs = new List<AttributeDefinition>();
        //    foreach (ObjectId id in target)
        //    {
        //        if (id.ObjectClass == attDefClass)
        //        {
        //            AttributeDefinition attDef = (AttributeDefinition)tr.GetObject(id, OpenMode.ForRead);
        //            attDefs.Add(attDef);
        //        }
        //    }
        //    foreach (ObjectId id in target.GetBlockReferenceIds(true, false))
        //    {
        //        BlockReference br = (BlockReference)tr.GetObject(id, OpenMode.ForWrite);
        //        ResetAttributes(br, attDefs);
        //    }
        //    if (target.IsDynamicBlock)
        //    {
        //        foreach (ObjectId id in target.GetAnonymousBlockIds())
        //        {
        //            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
        //            foreach (ObjectId brId in btr.GetBlockReferenceIds(true, false))
        //            {
        //                BlockReference br = (BlockReference)tr.GetObject(brId, OpenMode.ForWrite);
        //                ResetAttributes(br, attDefs);
        //            }
        //        }
        //    }
        //}
        //public static void ResetAttributes(BlockReference br, List<AttributeDefinition> attDefs)
        //{
        //    Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = br.Database.TransactionManager;
        //    Dictionary<string, string> attValues = new Dictionary<string, string>();
        //    foreach (ObjectId id in br.AttributeCollection)
        //    {
        //        if (!id.IsErased)
        //        {
        //            AttributeReference attRef = (AttributeReference)tm.GetObject(id, OpenMode.ForWrite);
        //            attValues.Add(attRef.Tag, attRef.TextString);
        //            attRef.Erase(true);
        //        }
        //    }
        //    foreach (AttributeDefinition attDef in attDefs)
        //    {
        //        AttributeReference attRef = new AttributeReference();
        //        attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
        //        if (attValues.ContainsKey(attDef.Tag))
        //        {
        //            attRef.TextString = attValues[attDef.Tag.ToUpper()];
        //        }
        //        br.AttributeCollection.AppendAttribute(attRef);
        //        tm.AddNewlyCreatedDBObject(attRef, true);
        //    }
        //}
        public static List<List<string>> GetStr(BlockReference bloco)
        {
            List<List<string>> retorno = new List<List<string>>();
            Database acCurDb;
            acCurDb = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var attCol = bloco.AttributeCollection;
                foreach (ObjectId objID in attCol)
                {
                    DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                    AttributeReference acAttRef = dbObj as AttributeReference;
                    retorno.Add(new List<string> { acAttRef.Tag, acAttRef.TextString });
                }
            }
            return retorno;
        }
        public static void Set(BlockReference myBlockRef, Transaction tr, string tag, string valor)
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
        public static void Set(BlockReference myBlockRef, Transaction tr, Hashtable t)
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
        public static DB.Linha GetLinha(BlockReference bloco, Database acCurDb = null)
        {
            DB.Linha retorno = new DB.Linha();
            if (acCurDb == null)
            {
                acCurDb = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            }

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var attCol = bloco.AttributeCollection;
                foreach (ObjectId objID in attCol)
                {
                    DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                    AttributeReference acAttRef = dbObj as AttributeReference;
                    retorno.Add(acAttRef.Tag, acAttRef.TextString);
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
