using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DLM.cad
{
    public static class ExtBlock
    {
        public static BlockTableRecord GetParent(this BlockReference block, OpenCloseTransaction acTrans = null)
        {
            try
            {
                if(acTrans==null)
                {
                    acTrans = CAD.acCurDb.acTrans();
                }
                using (acTrans)
                {
                    BlockReference blk = acTrans.GetObject(block.ObjectId, OpenMode.ForRead) as BlockReference;
                    BlockTableRecord acBlkTblRec = blk.IsDynamicBlock ?
                         acTrans.GetObject(blk.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord
                         :
                         acTrans.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;


                    return acBlkTblRec;
                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                return null;
            }

        }
        public static List<Entity> GetEntities(this BlockReference block)
        {

            List<Entity> retorno = new List<Entity>();

            try
            {
                DBObjectCollection acDBObjColl = new DBObjectCollection();
                block.Explode(acDBObjColl);

                foreach (Entity acEnt in acDBObjColl)
                {
                    try
                    {
                        retorno.Add(acEnt);
                    }
                    catch (Exception)
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
            }

            retorno.AddRange(retorno.Filter<BlockReference>().SelectMany(x => x.GetEntities()));

            return retorno.FindAll(x => x != null);
        }
        /// <summary>
        /// Returns the entities inside the block, without positions in inserts
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="acTrans"></param>
        /// <param name="openMode"></param>
        /// <returns></returns>
        public static List<Entity> GetBlockTableRecordEntities(this List<Entity> entities, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            var blocks = entities.Filter<BlockReference>();
            var blocktablerecord = blocks.GetTableRecords(acTrans, openMode);
            List<Entity> list = new List<Entity>();

            foreach (var block in blocktablerecord)
            {
                list.AddRange(block.GetBlockTableRecordEntities(acTrans));
            }
            return list;
        }
        public static List<Entity> GetBlockTableRecordEntities(this BlockTableRecord blockTableRecord, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            List<Entity> list = new List<Entity>();
            foreach (var item in blockTableRecord)
            {
                Entity acEnt = acTrans.GetObject(item, openMode) as Entity;
                list.Add(acEnt);
            }
            return list;
        }

        public static List<BlockTableRecord> GetTableRecords(this List<BlockReference> BlockReferences, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            List<BlockTableRecord> retorno = new List<BlockTableRecord>();
            var blocos_inseridos = BlockReferences.Select(x => x.BlockTableRecord).ToList();
            var tudo = blocos_inseridos.GroupBy(x => x.OldIdPtr.ToString()).Select(x => x.First()).ToList();

            foreach (var item in tudo)
            {
                BlockTableRecord ent = (BlockTableRecord)acTrans.GetObject(item, openMode);
                retorno.Add(ent);
            }
            return retorno;
        }
        public static List<BlockReference> Filter(this List<BlockReference> blocos, List<string> nomes, bool exato = true)
        {
            List<BlockReference> marcas = new List<BlockReference>();

            foreach (var b in blocos)
            {
                try
                {
                    var nome = b.Name.ToUpper();
                    foreach (var s in nomes)
                    {
                        if (exato)
                        {
                            if (nome.ToUpper() == s.ToUpper())
                            {
                                marcas.Add(b);
                                break;
                            }
                        }
                        else
                        {
                            if (nome.ToUpper().Contains(s.ToUpper()))
                            {
                                marcas.Add(b);
                                break;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex, $"Erro ao tentar ler um bloco.");
                }

            }

            return marcas;
        }
    }
}
