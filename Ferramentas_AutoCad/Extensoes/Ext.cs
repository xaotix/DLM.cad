using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public static class Ext
    {
        public static List<Entity> GetEntities(this BlockTableRecord blockTableRecord, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            List<Entity> list = new List<Entity>();
            foreach (var item in blockTableRecord)
            {
                Entity acEnt = acTrans.GetObject(item, openMode) as Entity;
                list.Add(acEnt);
            }
            return list;
        }
        public static List<Entity> GetBlockTableRecordEntities(this List<Entity> entities, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            var blocks = entities.Filter<BlockReference>();
            var blocktablerecord = blocks.GetTableRecords(acTrans, openMode);
            List<Entity> list = new List<Entity>();

            foreach(var bl in blocktablerecord)
            {
                list.AddRange(bl.GetEntities(acTrans));
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

        public static bool Is<T>(this Entity en)
        {
            return en is T;
        }
        public static bool IsDimmension(this Entity x)
        {
            return x is AlignedDimension
                                | x is ArcDimension
                                | x is Dimension
                                | x is DiametricDimension
                                | x is LineAngularDimension2
                                | x is Point3AngularDimension
                                | x is OrdinateDimension
                                | x is RadialDimension
                                | x is Leader
                                | x is MLeader
                                | x is MText
                                | x is DBText
                                ;
        }
        public static bool IsText(this Entity x)
        {
            return                x is DBText
                                | x is MText;
        }

        public static List<Entity> GetDimmensions(this List<Entity> List)
        {
            return List.FindAll(x => x.IsDimmension());
        }
        public static LayerTableRecord GetLayer(this Entity entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LayerTableRecord layer = (LayerTableRecord)acTrans.GetObject(entity.LayerId, openMode);
            return layer;
        }
        public static LinetypeTableRecord GetLineType(this Entity entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LinetypeTableRecord layer = (LinetypeTableRecord)acTrans.GetObject(entity.LinetypeId, openMode);
            return layer;
        }
        public static LinetypeTableRecord GetLineType(this LayerTableRecord entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LinetypeTableRecord layer = (LinetypeTableRecord)acTrans.GetObject(entity.LinetypeObjectId, openMode);
            return layer;
        }
        public static List<T> Filter<T>(this List<Entity> List)
        {
            List<T> lista = new List<T>();
            lista.AddRange(List.FindAll(x => x is T).Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList());
            return lista;
        }
        public static bool Is(this Autodesk.AutoCAD.Colors.Color color, System.Drawing.Color wincolor)
        {
            var cl = color.ColorValue;
            return cl.R == wincolor.R && cl.G == wincolor.G && cl.B == wincolor.B && cl.A == wincolor.A;
        }
        
    }
}
