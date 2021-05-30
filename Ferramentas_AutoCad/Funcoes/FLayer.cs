using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.CAD;

namespace Ferramentas_DLM
{
   public static class FLayer
    {
        public static void Ligar(List<string> layers)
        {
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {

                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;


                    foreach (var layer in layers)
                    {
                        if (acLyrTbl.Has(layer))
                        {
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite) as LayerTableRecord;

                            acLyrTblRec.IsOff = false;
                            acLyrTblRec.IsFrozen = false;
                            acLyrTblRec.IsHidden = false;
                            acLyrTblRec.IsLocked = false;
                        }
                    }


                    acTrans.Commit();
                }
            }
        }
        public static void Desligar(List<string> layers, bool congelar = true)
        {
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForWrite) as LayerTable;

                    foreach (var layer in layers)
                    {
                        if (acLyrTbl.Has(layer))
                        {
                            LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite) as LayerTableRecord;

                            // Turn the layer off
                            acLyrTblRec.IsOff = true;
                            acLyrTblRec.IsFrozen = congelar;
                        }
                    }
                    acTrans.Commit();
                }
            }
        }
        public static string GetAtual()
        {
            return (string)Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("clayer");
        }
        public static void Set(string layer, bool on = true, bool criar_senao_existe = false)
        {
            var layers = Get();
            // Get the current document and database

            if (layers.Find(x => x.ToUpper() == layer) == null && criar_senao_existe)
            {
                Criar(layer, System.Drawing.Color.White);
            }

            using (DocumentLock docLock = acDoc.LockDocument())
            {
                // Start a transaction
                // Open the Layer table for read
                LayerTable acLyrTbl;
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                   
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                    LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[layer], OpenMode.ForWrite) as LayerTableRecord;

                    // Turn the layer off
                    try
                    {
                        acLyrTblRec.IsOff = !on;

                        acLyrTblRec.IsFrozen = false;
                        acLyrTblRec.IsLocked = false;
                    }
                    catch (Exception)
                    {

                    }
  

                    acTrans.Commit();
                }
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {

                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                    acCurDb.Clayer = acLyrTbl[layer];
                    acTrans.Commit();
                }
            }
        }
        public static void Criar(string nome, System.Drawing.Color cor)
        {
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Layer table for read
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                       OpenMode.ForRead) as LayerTable;
                    string sLayerName = nome;
                    if (acLyrTbl.Has(sLayerName) == false)
                    {
                        LayerTableRecord acLyrTblRec = new LayerTableRecord();
                        // Assign the layer the ACI color 1 and a name
                        acLyrTblRec.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor);
                        acLyrTblRec.Name = sLayerName;
                        // Upgrade the Layer table for write
                        acLyrTbl.UpgradeOpen();
                        // Append the new layer to the Layer table and the transaction
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                    }
                    // Open the Block table for read
                    BlockTable acBlkTbl;
                    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                       OpenMode.ForRead) as BlockTable;
                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;
                    // Create a circle object
                    Circle acCirc = new Circle();
                    acCirc.SetDatabaseDefaults();
                    acCirc.Center = new Point3d(2, 2, 0);
                    acCirc.Radius = 1;
                    acCirc.Layer = sLayerName;
                    acBlkTblRec.AppendEntity(acCirc);
                    acTrans.AddNewlyCreatedDBObject(acCirc, true);
                    // Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
        }
        public static List<string> Get()
        {
            List<string> lstlay = new List<string>();

            LayerTableRecord layer;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                LayerTable lt = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layerId in lt)
                {
                    layer = acTrans.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                    lstlay.Add(layer.Name);
                }

            }
            return lstlay;
        }
    }
}
