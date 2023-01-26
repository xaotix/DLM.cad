using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace DLM.cad
{

    public static class Ext
    {
        public static OpenCloseTransaction acTrans(this Autodesk.AutoCAD.ApplicationServices.Document acDoc)
        {
            return acDoc.Database.TransactionManager.StartOpenCloseTransaction();
        }
        public static Transaction acTransST(this Autodesk.AutoCAD.ApplicationServices.Document acDoc)
        {
            return acDoc.Database.TransactionManager.StartTransaction();
        }
        public static Transaction acTransST(this Database acCurDb)
        {
            return acCurDb.TransactionManager.StartTransaction();
        }
        public static OpenCloseTransaction acTrans(this Database acCurDb)
        {
            return acCurDb.TransactionManager.StartOpenCloseTransaction();
        }

        public static List<Entity> GetAllEntities(this Database acCurDb)
        {
            if(acCurDb==null)
            {
                acCurDb = CAD.acCurDb;
            }
            List<Entity> list = new List<Entity>();
            var dict = new Dictionary<ObjectId, string>();
            using (var acTrans = acCurDb.acTransST())
            {
                var bt = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                foreach (var btrId in bt)
                {
                    var btr = (BlockTableRecord)acTrans.GetObject(btrId, OpenMode.ForRead);
                    if (btr.IsLayout)
                    {
                        foreach (var id in btr)
                        {
                            dict.Add(id, id.ObjectClass.Name);
                        }
                    }
                }
                foreach (var item in dict)
                {
                    Entity ent = (Entity)acTrans.GetObject(item.Key, OpenMode.ForRead);
                    list.Add(ent);
                }
                acTrans.Commit();
            }
            return list;
        }

        public static LinetypeTableRecord GetLineType(this LayerTableRecord entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LinetypeTableRecord layer = (LinetypeTableRecord)acTrans.GetObject(entity.LinetypeObjectId, openMode);
            return layer;
        }
        public static bool Is(this Autodesk.AutoCAD.Colors.Color color, System.Drawing.Color wincolor)
        {
            var cl = color.ColorValue;
            return cl.R == wincolor.R && cl.G == wincolor.G && cl.B == wincolor.B && cl.A == wincolor.A;
        }
        
    }
}
