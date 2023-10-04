using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CadDb = Autodesk.AutoCAD.DatabaseServices;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.BoundaryRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;

namespace DLM.cad
{
    public class Contorno
    {
        private Document _dwg;

        public Contorno(Document dwg)
        {
            _dwg = dwg;
        }

        public Entity DesenhaContorno(IEnumerable<ObjectId> entIds)
        {
            using (var polyline = GetContorno(entIds))
            {
                using (var acTrans = _dwg.TransactionManager.StartTransaction())
                {
                    var space = (BlockTableRecord)acTrans.GetObject(
                        _dwg.Database.CurrentSpaceId, OpenMode.ForWrite);
                    space.AppendEntity(polyline as Entity);
                    acTrans.AddNewlyCreatedDBObject(polyline as Entity, true);
                    acTrans.Commit();
                }
                return polyline;
            }
        }

        public Entity GetContorno(IEnumerable<ObjectId> entIds)
        {
            var regions = new List<Region>();

            using (var acTrans = _dwg.TransactionManager.StartTransaction())
            {
                foreach (var entId in entIds)
                {
                    var poly = acTrans.GetObject(entId, OpenMode.ForRead) as Polyline;
                    if (poly != null)
                    {
                        var rgs = GetRegionFromPolyline(poly);
                        regions.AddRange(rgs);
                    }

                }

                acTrans.Commit();
            }

            using (var region = JuntarRegioes(regions))
            {
                if (region != null)
                {
                    var brep = new Brep(region);
                    var points = new List<Point2d>();
                    var faceCount = brep.Faces.Count();
                    var face = brep.Faces.First();
                    foreach (var loop in face.Loops)
                    {
                        if (loop.LoopType == LoopType.LoopExterior)
                        {
                            foreach (var vertex in loop.Vertices)
                            {
                                points.Add(new Point2d(vertex.Point.X, vertex.Point.Y));
                            }
                            break;
                        }
                    }

                    return CriarPolilinha(points);
                }
                else
                {
                    return null;
                }
            }
        }

        #region private methods

        private List<Region> GetRegionFromPolyline(CadDb.Polyline poly)
        {
            var regions = new List<Region>();

            var sourceCol = new DBObjectCollection();
            var dbObj = poly.Clone() as CadDb.Polyline;
            dbObj.Closed = true;
            sourceCol.Add(dbObj);

            var dbObjs = Region.CreateFromCurves(sourceCol);
            foreach (var obj in dbObjs)
            {
                if (obj is Region) regions.Add(obj as Region);
            }

            return regions;
        }

        private Region JuntarRegioes(List<Region> regions)
        {
            if (regions.Count == 0) return null;
            if (regions.Count == 1) return regions[0];

            var region = regions[0];
            for (int i = 1; i < regions.Count; i++)
            {
                var rg = regions[i];
                region.BooleanOperation(BooleanOperationType.BoolUnite, rg);
                rg.Dispose();
            }

            return region;
        }

        private CadDb.Polyline CriarPolilinha(List<Point2d> points)
        {
            var poly = new CadDb.Polyline(points.Count());

            for (int i = 0; i < points.Count; i++)
            {
                poly.AddVertexAt(i, points[i], 0.0, 0.3, 0.3);
            }

            poly.SetDatabaseDefaults(_dwg.Database);
            poly.ColorIndex = 1;

            poly.Closed = true;

            return poly;
        }

        #endregion





        public static void GetContornoPolyLines()
        {
            var dwg = CAD.acDoc;
            var ed = dwg.Editor;

            try
            {
                var ids = SelectPolylines(ed);
                if (ids != null)
                {
                    var liner = new Contorno(dwg);
                    liner.DesenhaContorno(ids);
                }
                else
                {
                    Ut.AddMensagem("\n*Cancelado*");
                }
            }
            catch (System.Exception ex)
            {
                Ut.AddMensagem($"\nCommand falhado:\n{ex.Message}");
                Ut.AddMensagem($"\n*Cancelado*");
            }
        }

        private static ObjectId[] SelectPolylines(Editor ed)
        {
            var vals = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "LWPOLYLINE")
            };

            var res = ed.GetSelection(new SelectionFilter(vals));
            if (res.Status == PromptStatus.OK)
                return res.Value.GetObjectIds();
            else
                return null;
        }
    }
}
