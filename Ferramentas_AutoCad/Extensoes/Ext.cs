using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.cam;
using DLM.desenho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;


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
        public static void CriarCirculosDosFuros(this List<Furo> furos)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (Furo furo in furos)
                {
                    Circle circulo = new Circle
                    {
                        Center = furo.Origem.GetPoint3dCad(),
                        Radius = furo.Diametro/2,
                        Normal = Vector3d.ZAxis
                    };

                    btr.AppendEntity(circulo);
                    tr.AddNewlyCreatedDBObject(circulo, true);
                }

                tr.Commit();
            }
        }
        public static cam.Furo GetFuro(this Hatch hatch)
        {
            Extents3d extents = hatch.GeometricExtents;
            double largura = extents.MaxPoint.X - extents.MinPoint.X;
            double altura = extents.MaxPoint.Y - extents.MinPoint.Y;
            largura = largura.Round(0);
            altura = altura.Round(0);
            var dist = 0.0;
            var ang = 0.0;
            if (largura > altura)
            {
                dist = largura - altura;
            }
            else if (altura > largura)
            {
                dist = altura - largura;
                ang = 90;
            }
                var p0 = new Point3d(
                                      (extents.MinPoint.X + extents.MaxPoint.X) / 2,
                                      (extents.MinPoint.Y + extents.MaxPoint.Y) / 2,
                                      (extents.MinPoint.Z + extents.MaxPoint.Z) / 2
                                      );
            return new cam.Furo(p0.X, p0.Y, largura,dist,ang);
        }
        public static cam.Furo GetFuro(this BlockReference hatch)
        {
            Extents3d extents = hatch.GeometricExtents;
            double largura = extents.MaxPoint.X - extents.MinPoint.X;
            double altura = extents.MaxPoint.Y - extents.MinPoint.Y;
            largura = largura.Round(0);
            altura = altura.Round(0);
            var dist = 0.0;
            var ang = 0.0;
            if (largura > altura)
            {
                dist = largura - altura;
            }
            else if (altura > largura)
            {
                dist = altura - largura;
                ang = 90;
            }
            var p0 = new Point3d(
                                  (extents.MinPoint.X + extents.MaxPoint.X) / 2,
                                  (extents.MinPoint.Y + extents.MaxPoint.Y) / 2,
                                  (extents.MinPoint.Z + extents.MaxPoint.Z) / 2
                                  );
            return new cam.Furo(p0.X, p0.Y, largura, dist, ang);
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
        public static List<Line> GetLinhasConectadas(this List<Line> linhas)
        {
            var ordenadas = new List<Line>();
            var restantes = new List<Line>(linhas);

            // Começa com a primeira linha arbitrária
            retentar:
            Line atual = restantes[0];
            ordenadas.Add(atual);
            restantes.Remove(atual);

            while (restantes.Count > 0)
            {
                bool encontrou = false;

                foreach (Line linha in restantes)
                {
                    if (linha.StartPoint.P3d().ToString2D() == atual.EndPoint.P3d().ToString2D())
                    {
                        ordenadas.Add(linha);
                        atual = linha;
                        restantes.Remove(linha);
                        encontrou = true;
                        break;
                    }
                    else if (linha.EndPoint.P3d().ToString2D() == atual.EndPoint.P3d().ToString2D())
                    {
                        // Inverte a linha para conectar
                        Line invertida = new Line(linha.EndPoint, linha.StartPoint);
                        ordenadas.Add(invertida);
                        atual = invertida;
                        restantes.Remove(linha);
                        encontrou = true;
                        break;
                    }
                }

                if (!encontrou)
                {
                    // Não encontrou conexão direta, pode ser necessário aplicar tolerância ou encerrar
                    break;
                }
            }
            //volta novamente pro início pra tentar achar outro agrupamento de linhas
            if(ordenadas.Count == 1 && restantes.Count>1)
            {
                var v1 = ordenadas.First();

                ordenadas.Clear();
                goto retentar;
            }

            return ordenadas;
        }

        public static Polyline CriarPolyLine(this List<Line> linhas, bool fechar = true)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Polyline poly = null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                poly = new Polyline();

                int index = 0;
                foreach (Line linha in linhas)
                {
                    // Usa o ponto inicial da linha como vértice
                    Point2d pt = new Point2d(linha.StartPoint.X, linha.StartPoint.Y);
                    poly.AddVertexAt(index++, pt, 0, 0, 0);
                }

                // Adiciona o último ponto final da última linha
                if (linhas.Count > 0)
                {
                    Point2d ptFinal = new Point2d(linhas[linhas.Count - 1].EndPoint.X, linhas[linhas.Count - 1].EndPoint.Y);
                    poly.AddVertexAt(index, ptFinal, 0, 0, 0);
                }

                poly.Closed = true; // ou true se quiser fechar a polylinha

                btr.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, fechar);

                tr.Commit();
            }
            return poly;
        }

    }
}
