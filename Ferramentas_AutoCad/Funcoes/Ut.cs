using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Conexoes;
using DLM.cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using static DLM.cad.CAD;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using System.Windows;
using System.Diagnostics;
using DLM.desenho;

[assembly: CommandClass(typeof(DLM.cad.Ut))]
namespace DLM.cad
{
    public static class Ut
    {
        #region Tive que adicionar isso por causa do leader - no cad 2012 dá pau
        //isso daqui está aqui só por causa do Leader.
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Unicode, EntryPoint = "?attachAnnotation@AcDbLeader@@UAE?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@@Z")]
        private static extern Autodesk.AutoCAD.Runtime.ErrorStatus attachAnnotation32(IntPtr thisPtr, ref ObjectId annoId);
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Unicode, EntryPoint = "?attachAnnotation@AcDbLeader@@UEAA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@@Z")]
        private static extern Autodesk.AutoCAD.Runtime.ErrorStatus attachAnnotation64(IntPtr thisPtr, ref ObjectId annoId);
        private static Autodesk.AutoCAD.Runtime.ErrorStatus attachAnnotation(IntPtr thisPtr, ref ObjectId annoId)
        {

            if (Marshal.SizeOf(IntPtr.Zero) > 4)
                return attachAnnotation64(thisPtr, ref annoId);
            return attachAnnotation32(thisPtr, ref annoId);

        }
        #endregion

        public static P3d AddLeader(double angulo, P3d pp0, double escala, string nome = "", double multiplicador = 7.5, bool pedir_ponto = false)
        {
            try
            {
                var pt2 = pp0.Mover(angulo + 45, escala * multiplicador);

                if (pedir_ponto)
                {
                    bool cancelado = false;
                    var pt0 = Ut.PedirPonto("Selecione o segundo ponto", pp0, out cancelado);
                    if (!cancelado)
                    {
                        pt2 = pt0;
                    }
                }

                AddLeader(pp0, pt2, nome, 2 * escala);

                return pt2;
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }
            return pp0;
        }
        public static void AddLeader(P3d origem, P3d pt2, string texto, double escala)
        {
            Point3d p1 = new Point3d(origem.X, origem.Y, 0);
            Point3d p2 = new Point3d(pt2.X, pt2.Y, 0);
            using (var docLock = acDoc.LockDocument())
            {
                using (var acTrans = acDoc.acTransST())
                {
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Create the leader with annotation
                    using (Leader acLdr = new Leader())
                    {
                        acLdr.AppendVertex(p1);
                        acLdr.AppendVertex(p2);
                        acLdr.HasArrowHead = true;


                        acLdr.TextStyleId = acCurDb.Textstyle;

                        // Add the new object to Model space and the transaction
                        var id = acBlkTblRec.AppendEntity(acLdr);
                        acTrans.AddNewlyCreatedDBObject(acLdr, true);

                        // Attach the annotation after the leader object is added
                        if (texto.Length > 0)
                        {
                            using (MText acMText = new MText())
                            {
                                acMText.Contents = texto;
                                acMText.Location = acLdr.EndPoint;
                                acMText.TextStyleId = acCurDb.Textstyle;
                                double tam = escala;

                                if (tam > 0)
                                {
                                    acMText.TextHeight = tam;
                                }

                                acMText.Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Cyan);
                                acMText.UseBackgroundColor = false;
                                acMText.BackgroundFill = true;
                                acMText.BackgroundFillColor = Color.FromColorIndex(ColorMethod.ByAci, 1);
                                if (Math.Cos(acLdr.GetFirstDerivative(acLdr.EndParam).AngleOnPlane(new Plane())) >= 0.0)
                                    acMText.Attachment = AttachmentPoint.MiddleLeft;
                                else
                                    acMText.Attachment = AttachmentPoint.MiddleRight;
                                // Add the new object to Model space and the transaction
                                var textId = acBlkTblRec.AppendEntity(acMText);
                                acTrans.AddNewlyCreatedDBObject(acMText, true);


                                //essa função nao está funcionando no CAD 2012
                                //acLdr.UpgradeOpen();
                                //acLdr.Annotation = id;
                                //acLdr.EvaluateLeader();

                                //alternativa
                                try
                                {
                                    Autodesk.AutoCAD.Runtime.ErrorStatus es = attachAnnotation(acLdr.UnmanagedObject, ref textId);
                                    acLdr.EvaluateLeader();
                                }
                                catch (System.Exception ex)
                                {
                                    Conexoes.Utilz.Alerta(ex);
                                }


                            }
                        }

                    }
                    acTrans.Commit();
                }
            }
        }


        public static List<Entity> SelecionarTudoPrancha()
        {
            List<Entity> retorno = new List<Entity>();
            using (var acTrans = acCurDb.acTrans())
            {

                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                /*blocos*/
                BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId objId in acBlkTblRec)
                {
                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForRead);
                    retorno.Add(ent);
                }


            }
            return retorno;
        }
        public static List<Viewport> GetViewports(string setLayerPadrao = "")
        {
            List<Viewport> retorno = new List<Viewport>();

            try
            {
                using (var acTrans = acDoc.acTransST())
                {
                    DBDictionary lays = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                    foreach (DBDictionaryEntry item in lays)
                    {
                        Layout acLyrTblRec;
                        acLyrTblRec = acTrans.GetObject(item.Value, OpenMode.ForWrite) as Layout;

                        if (acLyrTblRec != null)
                        {
                            //retorno.Add(acLyrTblRec);


                            var views = acLyrTblRec.GetViewports();
                            foreach (ObjectId view in views)
                            {
                                try
                                {
                                    Viewport vp = acTrans.GetObject(view, OpenMode.ForWrite) as Viewport;
                                    if (vp != null)
                                    {
                                        retorno.Add(vp);
                                        if (setLayerPadrao != "")
                                        {
                                            vp.Layer = setLayerPadrao;
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    Conexoes.Utilz.Alerta(ex);
                                }

                            }

                        }
                    }
                    acTrans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

            return retorno;
        }






        public static string[] listarcomandos(Assembly asm, bool markedOnly)
        {
            StringCollection sc = new StringCollection();
            object[] objs = asm.GetCustomAttributes(typeof(CommandClassAttribute), true);
            Type[] tps;
            int numTypes = objs.Length;
            if (numTypes > 0)
            {
                tps = new Type[numTypes];
                for (int i = 0; i < numTypes; i++)
                {
                    CommandClassAttribute cca =
                      objs[i] as CommandClassAttribute;
                    if (cca != null)
                    {
                        tps[i] = cca.Type;
                    }
                }
            }
            else
            {
                if (markedOnly)
                    tps = new Type[0];
                else
                    tps = asm.GetExportedTypes();
            }

            foreach (Type tp in tps)
            {
                MethodInfo[] meths = tp.GetMethods();
                foreach (MethodInfo meth in meths)
                {
                    objs =
                      meth.GetCustomAttributes(typeof(CommandMethodAttribute), true);
                    foreach (object obj in objs)
                    {
                        CommandMethodAttribute attb = (CommandMethodAttribute)obj;
                        sc.Add(attb.GlobalName);
                    }
                }
            }
            string[] ret = new string[sc.Count];
            sc.CopyTo(ret, 0);
            return ret;
        }
        public static void InterSectionPoint()
        {
            Xline pl1 = null;
            Entity pl2 = null;
            Entity ent = null;
            PromptEntityOptions peo = null;
            PromptEntityResult per = null;
            using (var acTrans = acCurDb.acTransST())
            {
                //Select first polyline
                peo = new PromptEntityOptions("Seleciona a Xline:");
                per = editor.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                {
                    return;
                }
                //Get the polyline entity
                ent = (Entity)acTrans.GetObject(per.ObjectId, OpenMode.ForRead);
                if (ent is Xline)
                {
                    pl1 = ent as Xline;
                }
                //Select 2nd polyline
                peo = new PromptEntityOptions("\n Selecione o objeto:");
                per = editor.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                {
                    return;
                }
                ent = (Entity)acTrans.GetObject(per.ObjectId, OpenMode.ForRead);
                pl2 = ent;
                //if (ent is Xline)
                //{
                //    pl2 = ent as Xline;
                //}
                Point3dCollection pts3D = new Point3dCollection();
                //Get the intersection Points between line 1 and line 2
                pl1.IntersectWith(pl2, Intersect.ExtendBoth, pts3D, IntPtr.Zero, IntPtr.Zero);
                foreach (Point3d pt in pts3D)
                {
                    // ed.WriteMessage("\n intersection point :",pt);
                    // ed.WriteMessage("Point number: ", pt.X, pt.Y, pt.Z);

                    Application.ShowAlertDialog("\n Intersection Point: " + "\nX = " + pt.X + "\nY = " + pt.Y + "\nZ = " + pt.Z);
                }

                acTrans.Commit();
            }

        }

        public static List<LineSegment3d> GetSegmentos3D(Polyline pl, SegmentType type = SegmentType.Line)
        {
            List<LineSegment3d> segmentos = new List<LineSegment3d>();
            for (int i = 0; i < pl.NumberOfVertices - 1; i++)
            {
                try
                {
                    var s = pl.GetLineSegmentAt(i);
                    if (s != null)
                    {
                        if (pl.GetSegmentType(i) == type)
                        {
                            segmentos.Add(s);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }

            }
            return segmentos;
        }
        public static List<LineSegment2d> GetSegmentos(Polyline pl, SegmentType type = SegmentType.Line)
        {
            List<LineSegment2d> segmentos = new List<LineSegment2d>();
            for (int i = 0; i < pl.NumberOfVertices - 1; i++)
            {
                try
                {
                    var s = pl.GetLineSegment2dAt(i);
                    if (s != null)
                    {
                        if (pl.GetSegmentType(i) == type)
                        {
                            segmentos.Add(s);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }

            }
            return segmentos;
        }
        public static void SetOrtho(bool valor)
        {
            acDoc.Database.Orthomode = valor;
        }
        public static MlineStyle GetEstilo(string nome)
        {
            using (var acTrans = acCurDb.acTrans())
            {
                DBDictionary mlineDic =
                    (DBDictionary)acTrans.GetObject(acCurDb.MLStyleDictionaryId, OpenMode.ForRead);

                MlineStyle acLyrTblRec = acTrans.GetObject(mlineDic.GetAt(nome), OpenMode.ForWrite) as MlineStyle;


                return acLyrTblRec;
            }
        }
        public static List<Point2d> GetVertices(Polyline lwp)
        {
            List<Point2d> retorno = new List<Point2d>();
            if (lwp != null)
            {
                int vn = lwp.NumberOfVertices;
                for (int i = 0; i < vn; i++)
                {
                    Point2d pt = lwp.GetPoint2dAt(i);
                    retorno.Add(pt);
                }
            }
            return retorno;
        }
        public static List<Point3d> GetInfo(Polyline polyline, out double comprimento, out double largura, out double area, out double perimetro)
        {

            var vertices = Ut.GetPontos(polyline);
            comprimento = 0;
            largura = 0;
            area = polyline.Area;
            perimetro = polyline.Length;


            if (vertices.Count > 0)
            {
                var xmin = vertices.Min(x => x.X);
                var xmax = vertices.Max(x => x.X);
                var ymin = vertices.Min(x => x.Y);
                var ymax = vertices.Max(x => x.Y);

                double cmp1 = Math.Abs(xmax - xmin);
                double cmp2 = Math.Abs(ymax - ymin);
                comprimento = cmp1 > cmp2 ? cmp1 : cmp2;
                largura = cmp1 < cmp2 ? cmp1 : cmp2;
                return vertices;
            }
            return new List<Point3d>();
        }




        public static List<CADMline> MlinesPassando(P3d de, P3d ate, List<CADMline> Linhas, bool dentro_do_eixo = false, double tol_X = 0)
        {
            List<CADMline> retorno = new List<CADMline>();
            P3d nde = new P3d(de.X - tol_X, de.Y, 0);
            P3d nate = new P3d(ate.X + tol_X, ate.Y, 0);


            foreach (var corrente in Linhas)
            {
                var p1 = corrente.Inicio;
                var p2 = corrente.Fim;


                if (!dentro_do_eixo)
                {
                    if (
                       (p1.X <= nde.X && p2.X >= nate.X) //se passa
                    | (p1.X >= nde.X && p2.X <= nate.X) //se os dois lados estão dentro

                    | (p1.X >= nde.X && p2.X >= nate.X && p1.X < nate.X) //se a esquerda está dentro
                    | (p1.X <= nde.X && p2.X <= nate.X && p2.X > nde.X) //se a direita está dentro
                                        )
                    {
                        retorno.Add(corrente);
                    }
                }
                else if (dentro_do_eixo)
                {
                    if (p1.X > nde.X && p2.X < nate.X) //se os dois lados estão somente dentro
                    {
                        retorno.Add(corrente);
                    }
                }


            }
            return retorno;
        }

        /// <summary>
        /// Pega multiline, poyline e line
        /// </summary>
        /// <param name="de"></param>
        /// <param name="ate"></param>
        /// <param name="LS"></param>
        /// <param name="somente_dentro"></param>
        /// <param name="dentro_do_eixo"></param>
        /// <param name="somente_vertical"></param>
        /// <returns></returns>
        public static List<Entity> LinhasPassando(P3d de, P3d ate, List<Entity> LS, bool somente_dentro = false, bool dentro_do_eixo = false, bool somente_vertical = true)
        {
            List<Entity> retorno = new List<Entity>();
            foreach (var s in LS)
            {
                P3d p1, p2, centro;
                double angulo, comprimento, largura;
                GetCoordenadas(s, out p1, out p2, out angulo, out comprimento, out centro, out largura);

                if (somente_vertical)
                {
                    if (angulo >= 85 && angulo <= 95)
                    {

                    }
                    else
                    {
                        continue;
                    }
                }

                if (!somente_dentro)
                {
                    if (
                                        (p1.X <= de.X && p2.X >= ate.X) //se passa
                                     | (p1.X >= de.X && p2.X <= ate.X) //se os dois lados estão dentro

                                     | (p1.X >= de.X && p2.X >= ate.X && p1.X < ate.X) //se a esquerda está dentro
                                     | (p1.X <= de.X && p2.X <= ate.X && p2.X > de.X) //se a direita está dentro
                                        )
                    {
                        retorno.Add(s);
                    }
                }
                else if (somente_dentro && !dentro_do_eixo)
                {
                    if (p1.X >= de.X && p2.X <= ate.X) //se os dois lados estão dentro
                    {
                        retorno.Add(s);
                    }
                }
                else if (somente_dentro && dentro_do_eixo)
                {
                    if (p1.X > de.X && p2.X < ate.X) //se os dois lados estão somente dentro
                    {
                        retorno.Add(s);
                    }
                }


            }
            return retorno;
        }
        public static void GetCoordenadas(Entity s, out P3d p1, out P3d p2, out double angulo, out double comprimento, out P3d centro, out double largura)
        {
            p1 = new P3d();
            p2 = new P3d();
            centro = new P3d();
            comprimento = 0;
            largura = 0;
            if (s is Mline)
            {
                (s as Mline).GetOrigens(out p1, out p2, out largura);
                comprimento = p1.Distancia(p2);
            }
            else if (s is Line)
            {
                var l = s as Line;
                p1 = l.StartPoint.P3d();
                p2 = l.EndPoint.P3d();
                comprimento = l.Length;
            }
            else if (s is Polyline)
            {
                var l = s as Polyline;
                p1 = l.StartPoint.P3d();
                p2 = l.EndPoint.P3d();
                comprimento = l.Length;
                largura = ((double)l.LineWeight);
            }

            angulo = Math.Round(Math.Abs(new DLM.desenho.P3d(p1.X, p1.Y, p1.Z).GetAngulo(new DLM.desenho.P3d(p2.X, p2.Y, p2.Z))), 2);
            if (angulo > 180)
            {
                angulo = angulo - 180;
            }

            centro = p1.Centro(p2);
        }



        public static List<Line> LinhasHorizontais(this List<Line> LS, double comp_min = 100)
        {
            List<Line> retorno = new List<Line>();
            foreach (var s in LS)
            {
                var angulo = Math.Abs(new DLM.desenho.P3d(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z).GetAngulo(new DLM.desenho.P3d(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z)));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.StartPoint.X - s.EndPoint.X);

                if ((angulo >= 175 | angulo <= 5) && comp >= comp_min)
                {
                    retorno.Add(s);
                }
            }
            return retorno;
        }
        public static List<Xline> XLinesHorizontais(this List<Xline> LS, double comp_min = 0)
        {
            List<Xline> retorno = new List<Xline>();
            foreach (var s in LS)
            {
                double angulo = Angulo.Get(s);
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }

                if ((angulo >= 175 | angulo <= 5))
                {
                    retorno.Add(s);
                }
            }
            return retorno;
        }
        public static List<Polyline> PolylinesVerticais(this List<Polyline> LS, double comp_min = 100)
        {
            List<Polyline> retorno = new List<Polyline>();
            foreach (var s in LS)
            {
                var angulo = new DLM.desenho.P3d(s.StartPoint.X, s.StartPoint.Y, 0).GetAngulo(new DLM.desenho.P3d(s.EndPoint.X, s.EndPoint.Y, 0));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.StartPoint.Y - s.EndPoint.Y);
                if ((angulo >= 80 | angulo <= 100) && comp >= comp_min)
                {
                    retorno.Add(s);
                }

            }
            return retorno;
        }
        public static List<RotatedDimension> CotasVerticais(List<Entity> LS, double comp_min = 100)
        {
            List<RotatedDimension> retorno = new List<RotatedDimension>();
            foreach (var s in LS.FindAll(x => x is RotatedDimension).Select(x => x as RotatedDimension))
            {
                var angulo = Math.Abs(new DLM.desenho.P3d(s.XLine1Point.X, s.XLine1Point.Y, 0).GetAngulo(new DLM.desenho.P3d(s.XLine2Point.X, s.XLine2Point.Y, 0)));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.XLine1Point.Y - s.XLine2Point.Y);
                if ((angulo >= 80 | angulo <= 100) && comp >= comp_min)
                {
                    retorno.Add(s);
                }

            }
            return retorno;
        }
        public static void GetBordas(List<Entity> entities, out Point3d se, out Point3d sd, out Point3d ie, out Point3d id)
        {
            se = new Point3d();
            sd = new Point3d();
            ie = new Point3d();
            id = new Point3d();
            var pts = entities.SelectMany(x => GetCoordenadas(x)).ToList();
            if (pts.Count > 0)
            {
                se = new Point3d(pts.Min(x => x.X), pts.Max(x => x.Y), 0);
                sd = new Point3d(pts.Max(x => x.X), pts.Max(x => x.Y), 0);

                ie = new Point3d(pts.Min(x => x.X), pts.Min(x => x.Y), 0);
                id = new Point3d(pts.Max(x => x.X), pts.Min(x => x.Y), 0);
            }
        }
        public static List<BlockReference> GetBlocosProximos(List<BlockReference> blocos, P3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => Math.Abs(new P3d(x.Position.X, x.Position.Y).Distancia(ponto)) <= tolerancia);
        }
        public static List<Polyline> GetPolylinesProximas(List<Polyline> blocos, P3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => new P3dCAD(x.StartPoint).Distancia(ponto) <= tolerancia | new P3dCAD(x.EndPoint).Distancia(ponto) <= tolerancia);
        }
        public static List<RotatedDimension> GetCotasProximas(List<RotatedDimension> blocos, P3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => new P3dCAD(x.XLine1Point).Distancia(ponto) <= tolerancia | new P3dCAD(x.XLine2Point).Distancia(ponto) <= tolerancia);
        }
        public static Xline GetXlineMaisProxima(Entity objeto, List<Xline> xlines, double tolerancia)
        {
            Xline retorno = new Xline();

            double distancia = -1;
            var coords = GetCoordenadas(objeto);
            if (coords.Count > 0)
            {



                foreach (var p in xlines)
                {
                    var p1 = coords.First();
                    var ang = Angulo.Get(p);
                    var p2 = p1.Mover(ang + 90, 10000);

                    //var a1 = new P3dCAD(p.BasePoint);
                    //var a2 = new P3dCAD(p.BasePoint).Mover(ang, 10000);





                    using (var ll = new Line(p1.GetPoint3dCad(), p2.GetPoint3dCad()))
                    {
                        Point3dCollection pts3D = new Point3dCollection();
                        //Get the intersection Points between line 1 and line 2
                        p.IntersectWith(ll, Intersect.ExtendBoth, pts3D, IntPtr.Zero, IntPtr.Zero);
                        foreach (Point3d pt in pts3D)
                        {
                            // ed.WriteMessage("\n intersection point :",pt);
                            // ed.WriteMessage("Point number: ", pt.X, pt.Y, pt.Z);
                            var dist = p1.Distancia(pt);
                            //Application.ShowAlertDialog("\n Ponto: " + "\nX = " + pt.X + "\nY = " + pt.Y + "\nZ = " + pt.Z + "\nDistância: " + dist);



                            AddMensagem($"\nLine: origem: {p1}");
                            AddMensagem($"\nLine: Intersecção: {pt}");
                            AddMensagem($"\nLine: Distância: {dist}");

                            //AddMensagem("\nXline distância: " + dist1);
                            //AddMensagem("\nXline distância: " + dist2);
                            if (distancia < 0)
                            {
                                distancia = dist;
                                retorno = p;
                            }
                            else if (dist < distancia)
                            {
                                distancia = dist;
                                distancia = Math.Abs(distancia);
                                retorno = p;
                            }
                        }
                    }



                    //foreach (Point3d pt in pts)
                    //{
                    //    var dist1 = Math.Round(Math.Abs(p1.Distancia(pt)));

                    //    AddMensagem("\nLine: origem: " + p1);
                    //    AddMensagem("\nLine: Intersecção: " + pt);

                    //    //AddMensagem("\nXline distância: " + dist1);
                    //    //AddMensagem("\nXline distância: " + dist2);
                    //    if (distancia < 0)
                    //    {
                    //        distancia = dist1;
                    //        retorno = p;
                    //    }
                    //    else if (dist1 < distancia)
                    //    {
                    //        distancia = dist1;
                    //        distancia = Math.Abs(distancia);
                    //        retorno = p;
                    //    }
                    //}




                }
            }
            AddMensagem($"\nXline Menor distância: {distancia}");
            AddMensagem($"\nXline tolerância: {tolerancia}");
            if (distancia < 0)
            {
                retorno = null;
            }
            if (distancia > tolerancia)
            {
                retorno = null;
            }
            return retorno;
        }
        public static List<Xline> GetXlinesProximas(Entity objeto, List<Xline> xlines, double tolerancia)
        {
            List<Xline> retorno = new List<Xline>();

            var coords = GetCoordenadas(objeto);
            if (coords.Count > 0)
            {
                var min = new P3d(coords.Min(x => x.X), coords.Min(x => x.Y), 0);
                var max = new P3d(coords.Max(x => x.X), coords.Max(x => x.Y), 0);
                foreach (var p in xlines)
                {
                    double angulo = Angulo.Get(p);
                    var norm = Angulo.Normalizar(angulo);
                    double dist1 = 0, dist2 = 0;
                    if (norm == 0 | norm == 180)
                    {
                        dist1 = min.DistanciaX(p.BasePoint);
                        dist2 = max.DistanciaX(p.BasePoint);
                    }


                    var pmin = min.Mover(angulo, dist1);
                    var pmax = min.Mover(angulo, dist2);
                    double ds1 = min.Distancia(pmin);
                    double ds2 = max.Distancia(pmax);

                    if (ds1 <= tolerancia | ds2 <= tolerancia)
                    {
                        retorno.Add(p);
                    }


                }
            }

            return retorno;
        }
        public static List<P3d> GetCoordenadas(Entity objeto)
        {
            List<P3d> retorno = new List<P3d>();

            if (objeto is Line)
            {
                var p = (objeto as Line);
                retorno.Add(p.StartPoint.P3d());
                retorno.Add(p.EndPoint.P3d());
            }
            else if (objeto is Polyline)
            {
                var p = (objeto as Polyline);
                retorno.Add(p.StartPoint.P3d());
                retorno.Add(p.EndPoint.P3d());
            }
            else if (objeto is RotatedDimension)
            {
                var p = (objeto as RotatedDimension);
                retorno.Add(p.XLine1Point.P3d());
                retorno.Add(p.XLine2Point.P3d());
            }
            else if (objeto is BlockReference)
            {
                var p = (objeto as BlockReference);
                retorno.Add(p.Position.P3d());
            }
            else if (objeto is Mline)
            {
                var p = (objeto as Mline);
                P3d p1;
                P3d p2;
                double largura = 0;
                p.GetOrigens(out p1, out p2, out largura);
                retorno.Add(p1);
                retorno.Add(p2);
            }

            return retorno;
        }
        public static List<RotatedDimension> CotasHorizontais(List<Entity> LS, double comp_min = 100)
        {
            List<RotatedDimension> retorno = new List<RotatedDimension>();
            foreach (var s in LS.FindAll(x => x is RotatedDimension).Select(x => x as RotatedDimension))
            {
                var angulo = Math.Abs(new DLM.desenho.P3d(s.XLine1Point.X, s.XLine1Point.Y, s.XLine1Point.Z).GetAngulo(new DLM.desenho.P3d(s.XLine2Point.X, s.XLine2Point.Y, s.XLine2Point.Z)));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.XLine1Point.X - s.XLine2Point.X);

                if ((angulo >= 175 | angulo <= 5) && comp >= comp_min)
                {
                    retorno.Add(s);
                }
            }
            return retorno;
        }


        public static List<Line> LinhasVerticais(List<Line> LS, double comp_min = 100)
        {
            List<Line> retorno = new List<Line>();
            foreach (var s in LS)
            {
                var angulo = Math.Abs(new DLM.desenho.P3d(s.StartPoint.X, s.StartPoint.Y, 0).GetAngulo(new DLM.desenho.P3d(s.EndPoint.X, s.EndPoint.Y, 0)));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.StartPoint.Y - s.EndPoint.Y);
                if ((angulo >= 85 && angulo <= 95) && comp >= comp_min)
                {
                    retorno.Add(s);
                }

            }
            return retorno;
        }
        public static List<Polyline> PolylinesHorizontais(List<Polyline> LS, double comp_min = 100)
        {
            List<Polyline> retorno = new List<Polyline>();
            foreach (var s in LS)
            {
                var angulo = Math.Abs(new DLM.desenho.P3d(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z).GetAngulo(new DLM.desenho.P3d(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z)));
                if (angulo >= 180)
                {
                    angulo = angulo - 180;
                }
                double comp = Math.Abs(s.StartPoint.X - s.EndPoint.X);

                if ((angulo >= 175 | angulo <= 5) && comp >= comp_min)
                {
                    retorno.Add(s);
                }
            }
            return retorno;
        }

        public static List<UIElement> GetCanvas(object obj, Point p0, double escala, Transaction tr, double opacidade, double comp_min = 50)
        {
            var cor = System.Windows.Media.Brushes.White;
            List<UIElement> retorno = new List<UIElement>();
            if (obj is BlockReference)
            {
                var s = obj as BlockReference;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(s.BlockTableRecord, OpenMode.ForRead);

                foreach (ObjectId id in acBlkTblRec)
                {

                    var obj1 = tr.GetObject(id, OpenMode.ForRead);

                    var ptss = Ut.GetPontosAgrupados(obj1, tr);

                    cor = Ut.GetCor(obj1);

                    foreach (var pts in ptss)
                    {
                        if (pts.Count > 1)
                        {

                            for (int i = 1; i < pts.Count; i++)
                            {
                                var pt1 = pts[i - 1].TransformBy(s.BlockTransform);
                                var pt2 = pts[i].TransformBy(s.BlockTransform);
                                if (Math.Abs(pt1.DistanceTo(pt2)) >= comp_min)
                                {
                                    var p1 = new P3dCAD(pt1).Origem;
                                    var p2 = new P3dCAD(pt2).Origem;
                                    p1 = new P3d((p1.X - p0.X) * escala, (p1.Y - p0.Y) * escala);
                                    p2 = new P3d((p2.X - p0.X) * escala, (p2.Y - p0.Y) * escala);

                                    retorno.Add(p1.Linha(p2, cor));
                                }

                            }
                        }
                    }
                }
            }
            else
            {
                List<Point3d> pts = Ut.GetPontos(obj);
                if (pts.Count > 1)
                {
                    cor = Ut.GetCor(obj);
                    for (int i = 1; i < pts.Count; i++)
                    {
                        var pt1 = pts[i - 1];
                        var pt2 = pts[i];

                        if (Math.Abs(pt1.DistanceTo(pt2)) >= comp_min)
                        {
                            var p1 = new P3dCAD(pt1).Origem;
                            var p2 = new P3dCAD(pt2).Origem;
                            p1 = new P3d((p1.X - p0.X) * escala, (p1.Y - p0.Y) * escala);
                            p2 = new P3d((p2.X - p0.X) * escala, (p2.Y - p0.Y) * escala);
                            cor.Opacity = opacidade;
                            retorno.Add(p1.Linha(p2, cor));
                        }

                    }
                }
            }
            return retorno;
        }



        public static IDictionary<string, object> GetOPMProperties(ObjectId id)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            IntPtr pUnk = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(id);
            if (pUnk != IntPtr.Zero)
            {
                using (CollectionVector properties = ObjectPropertyManagerProperties.GetProperties(id, false, false))
                {
                    int cnt = properties.Count();
                    if (cnt != 0)
                    {
                        using (CategoryCollectable category = properties.Item(0) as CategoryCollectable)
                        {
                            CollectionVector props = category.Properties;
                            int propCount = props.Count();
                            for (int j = 0; j < propCount; j++)
                            {
                                using (PropertyCollectable prop = props.Item(j) as PropertyCollectable)
                                {
                                    if (prop == null)
                                        continue;
                                    object value = null;
                                    if (prop.GetValue(pUnk, ref value) && value != null)
                                    {
                                        if (!map.ContainsKey(prop.Name))
                                            map[prop.Name] = value;
                                    }
                                }
                            }
                        }
                    }
                }
                Marshal.Release(pUnk);
            }
            return map;
        }
        public static string RetornaCustomProperties(ObjectId id)
        {
            string msg = "";
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var acTrans = acCurDb.acTransST())
            {
                var dbObj = acTrans.GetObject(id, OpenMode.ForRead);
                var types = new List<Type>();
                types.Add(dbObj.GetType());
                while (true)
                {
                    var type = types[0].BaseType;
                    types.Insert(0, type);
                    if (type == typeof(RXObject))
                        break;
                }
                foreach (Type t in types)
                {
                    msg += ($"\n\n - {t.Name} -");
                    foreach (var prop in t.GetProperties(flags))
                    {
                        msg += "\n" + prop.Name;
                        try
                        {
                            msg += " = " + (prop.GetValue(dbObj, null));
                        }
                        catch (System.Exception e)
                        {
                            msg += (e.Message);
                        }
                    }
                }
                acTrans.Commit();
            }

            return msg;
        }


        public static Point3d GetP3d(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }
        public static Point2d Mover(this Point2d origem, double angulo, double distancia)
        {
            double angleRadians = (Math.PI * (angulo) / 180.0);
            return new Point2d(((double)origem.X + (Math.Cos(angleRadians) * distancia)), ((double)origem.Y + (Math.Sin(angleRadians) * distancia)));
        }


        public static void AddMensagem(string Mensagem)
        {
            editor.WriteMessage(Mensagem);
        }



        public static double GetSuperficieM2(double espessura_mm, double area_mm2, double perimetro_mm, int decimais = 4)
        {
            return Math.Round(((area_mm2 * 2) + (perimetro_mm * espessura_mm)) / 1000 / 1000, decimais);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pergunta"></param>
        /// <param name="cancelado">se false: o usuário clicou</param>
        /// <returns></returns>
        public static P3d PedirPonto(string pergunta, out bool cancelado)
        {
            cancelado = false;
            return PedirPonto(pergunta, new P3d(), out cancelado, false);
        }
        //public static Point2d PedirPonto2D(string pergunta, out bool cancelado)
        //{
        //    cancelado = false;
        //    return PedirPonto2D(pergunta, new P3d(), out cancelado, false);
        //}

        public static P3d PedirPonto(string pergunta, P3d origem, out bool cancelado, bool tem_origem = true)
        {
            cancelado = false;
            PromptPointResult pPtRes;
            var pPtOpts = new PromptPointOptions("");

            pPtOpts.Message = "\n" + pergunta;
            if (tem_origem)
            {
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = origem.GetPoint3dCad();
            }
            pPtRes = editor.GetPoint(pPtOpts);

            var ptStart = pPtRes.Value.P3d();

            if (pPtRes.Status == PromptStatus.Cancel)
            {
                cancelado = true;
                return new P3d();
            }
            return ptStart;
        }
        //public static P3d PedirPonto2D(string pergunta, P3d origem, out bool cancelado, bool tem_origem = true)
        //{
        //    cancelado = false;
        //    PromptPointResult pPtRes;
        //    PromptPointOptions pPtOpts = new PromptPointOptions("");

        //    pPtOpts.Message = "\n" + pergunta;
        //    if (tem_origem)
        //    {
        //        pPtOpts.UseBasePoint = true;
        //        pPtOpts.BasePoint = new Point3d(origem.X, origem.Y, 0);
        //    }
        //    pPtRes = editor.GetPoint(pPtOpts);

        //    Point3d ptStart = pPtRes.Value;

        //    if (pPtRes.Status == PromptStatus.Cancel)
        //    {
        //        cancelado = true;
        //        return new P3d();
        //    }

        //    return new P3d(ptStart.X, ptStart.Y);

        //}
        public static List<P3d> PedirPontos3D(string pergunta = "Selecione as origens")
        {
            List<P3d> retorno = new List<P3d>();
            bool cancelado = false;
            while (!cancelado)
            {
                var pt = PedirPonto(pergunta, retorno.Count > 0 ? retorno.Last() : new P3d(), out cancelado, retorno.Count > 0);
                if (!cancelado)
                {
                    retorno.Add(pt);
                }
            }
            return retorno;
        }
        public static double PedirDistancia(string pergunta, out bool cancelado)
        {
            cancelado = false;
            PromptDoubleResult resultado;
            PromptDistanceOptions opcoes = new PromptDistanceOptions("");

            opcoes.Message = "\n" + pergunta;
            opcoes.AllowArbitraryInput = true;
            opcoes.AllowNegative = false;
            opcoes.AllowNone = false;
            opcoes.AllowZero = false;
            opcoes.UseDashedLine = true;
           

            resultado = editor.GetDistance(opcoes);

            var retorno = resultado.Value;

            if (resultado.Status == PromptStatus.Cancel)
            {
                cancelado = true;
                return 0;
            }

            return retorno;

        }

        public static double PedirDistancia(string pergunta, double distancia = 0)
        {
            PromptDoubleResult resultado;
            PromptDistanceOptions opcoes = new PromptDistanceOptions("");

            opcoes.Message = "\n" + pergunta;
            opcoes.AllowArbitraryInput = true;
            opcoes.AllowNegative = false;
            opcoes.AllowNone = false;
            opcoes.AllowZero = false;
            opcoes.UseDashedLine = true;
            opcoes.DefaultValue = distancia;

            resultado = editor.GetDistance(opcoes);


            if (resultado.Status == PromptStatus.Cancel)
            {
                return 0;
            }
            else
            {
                return resultado.Value;
            }

        }

        public static string PedirString(string Titulo, List<string> Opcoes)
        {
            PromptKeywordOptions tipo_vista = new PromptKeywordOptions("");
            tipo_vista.Message = "\n" + Titulo;
            foreach (var s in Opcoes)
            {
                tipo_vista.Keywords.Add(s);
            }
            tipo_vista.AppendKeywordsToMessage = true;

            tipo_vista.AllowNone = false;

            PromptResult selecao_tipo_vista = acDoc.Editor.GetKeywords(tipo_vista);
            if (selecao_tipo_vista.Status != PromptStatus.OK) return "";

            return selecao_tipo_vista.StringResult;
        }
        public static double PedirDouble(string Titulo, double padrao = 0)
        {
            PromptDoubleOptions tipo_vista = new PromptDoubleOptions(Titulo);
            tipo_vista.Message = "\n" + Titulo;
            tipo_vista.DefaultValue = padrao;

            tipo_vista.AllowNone = false;

            PromptDoubleResult selecao_tipo_vista = acDoc.Editor.GetDouble(tipo_vista);
            if (selecao_tipo_vista.Status != PromptStatus.OK) return padrao;

            return selecao_tipo_vista.Value;
        }
        public static int PedirInteger(string Titulo, int padrao = 0)
        {
            PromptIntegerOptions tipo_vista = new PromptIntegerOptions(Titulo);
            tipo_vista.Message = "\n" + Titulo;
            tipo_vista.DefaultValue = padrao;

            tipo_vista.AllowNone = false;

            PromptIntegerResult selecao_tipo_vista = acDoc.Editor.GetInteger(tipo_vista);
            if (selecao_tipo_vista.Status != PromptStatus.OK) return padrao;

            return selecao_tipo_vista.Value;
        }

        public static System.Collections.Generic.List<ObjectId> getLayoutIds(Database acCurDb)
        {
            System.Collections.Generic.List<ObjectId> layoutIds = new System.Collections.Generic.List<ObjectId>();
            using (var acTrans = acCurDb.acTrans())
            {
                DBDictionary layoutDic = acTrans.GetObject(
                    acCurDb.LayoutDictionaryId,
                    OpenMode.ForRead, false)
                        as DBDictionary;
                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    layoutIds.Add(entry.Value);
                }
            }
            return layoutIds;
        }




        public static void IrModel()
        {
            LayoutManager.Current.CurrentLayout = "Model";
        }
        public static void ZoomExtend()
        {
            CAD.acadApp.ZoomExtents();
        }


        public static void Purge()
        {
            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                             OpenMode.ForRead) as LayerTable;

                // Create an ObjectIdCollection to hold the object ids for each table record
                ObjectIdCollection acObjIdColl = new ObjectIdCollection();

                // Step through each layer and add iterator to the ObjectIdCollection
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    acObjIdColl.Add(acObjId);
                }

                // Remove the layers that are in use and return the ones that can be erased
                acCurDb.Purge(acObjIdColl);

                // Step through the returned ObjectIdCollection
                // and erase each unreferenced layer
                foreach (ObjectId acObjId in acObjIdColl)
                {
                    SymbolTableRecord acSymTblRec;
                    acSymTblRec = acTrans.GetObject(acObjId,
                                                    OpenMode.ForWrite) as SymbolTableRecord;

                    try
                    {
                        // Erase the unreferenced layer
                        acSymTblRec.Erase(true);
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception Ex)
                    {
                        // Layer could not be deleted
                        Conexoes.Utilz.Alerta(Ex);
                    }
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }


        public static List<Point3d> GetPontos(List<Entity> objs, Transaction tr = null)
        {
            List<Point3d> ptss = new List<Point3d>();
            if (tr == null)
            {
                // Start a transaction
                using (var acTrans = acCurDb.acTrans())
                {
                    foreach (var obj in objs)
                    {
                        ptss.AddRange(GetPontosAgrupados(obj, acTrans).SelectMany(x => x));
                    }
                }
            }
            else
            {
                foreach (var obj in objs)
                {
                    ptss.AddRange(GetPontosAgrupados(obj, tr).SelectMany(x => x));
                }
            }

            return ptss;
        }


        public static List<Point3d> GetPontos(object obj, Transaction tr = null)
        {
            var ptss = new List<Point3d>();
            if (tr == null)
            {
                using (var acTrans = acCurDb.acTrans())
                {
                    ptss.AddRange(GetPontosAgrupados(obj, acTrans).SelectMany(x => x));
                }
            }
            else
            {
                ptss.AddRange(GetPontosAgrupados(obj, tr).SelectMany(x => x));
            }

            return ptss;
        }
        /// <summary>
        /// Retorna pontos dos vértices dos objetos
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="acTrans"></param>
        /// <returns></returns>
        public static List<List<Point3d>> GetPontosAgrupados(object obj, Transaction acTrans = null)
        {
            var pts2 = new List<List<Point3d>>();
            var pts = new List<Point3d>();
            var cor = System.Windows.Media.Brushes.White;
            if (obj is Line)
            {
                var tt = obj as Line;
                cor = Ut.GetCor(tt.Color);
                pts.Add(tt.StartPoint);
                pts.Add(tt.EndPoint);
            }
            else if (obj is Polyline)
            {
                var poly = obj as Polyline;
                for (int i = 0; i < poly.NumberOfVertices; i++)
                {
                    var t = poly.GetPoint3dAt(i);
                    pts.Add(t);
                }

                if (poly.Closed && pts.Count > 0)
                {
                    pts.Add(pts[0]);
                }
                cor = Ut.GetCor(poly.Color);
            }
            else if (obj is Circle)
            {
                var tt = obj as Circle;
                cor = Ut.GetCor(tt.Color);
                pts.AddRange(tt.Center.P3d().GetPontosCirculo(tt.Diameter, 16).Select(x => x.GetPoint3dCad()));

                if (pts.Count > 0)
                {
                    pts.Add(pts[0]);
                }


            }
            //else if (obj is Arc)
            //{
            //    var tt = obj as Arc;
            //    cor = Utilidades.GetCor(tt.Color);
            //    var center = new Coordenada(tt.Center);
            //    var p1 = new Coordenada(tt.StartPoint);
            //    var p2 = new Coordenada(tt.EndPoint);
            //    pts.Add(p1.GetPoint());
            //    pts.Add(center.GetPoint());
            //    pts.Add(p2.GetPoint());
            //}
            else if (obj is Mline)
            {
                var tt = obj as Mline;
                cor = Ut.GetCor(tt.Color);
                for (int i = 0; i < tt.NumberOfVertices; i++)
                {
                    var t = tt.VertexAt(i);
                    pts.Add(t);
                }

                if (tt.IsClosed)
                {
                    pts.Add(pts[0]);
                }
            }
            else if (obj is BlockReference && acTrans != null)
            {
                var tt = obj as BlockReference;
                var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(tt.BlockTableRecord, OpenMode.ForRead);

                foreach (ObjectId id in acBlkTblRec)
                {
                    var obj1 = acTrans.GetObject(id, OpenMode.ForRead);
                    pts2.AddRange(Ut.GetPontosAgrupados(obj1, acTrans).Select(x => x.Select(y => y.TransformBy(tt.BlockTransform)).ToList()).ToList());
                }
            }
            pts2.Add(pts);
            return pts2;
        }
        public static void PoliLinha(List<Point3d> points)
        {
            using (var docLock = acDoc.LockDocument())
            using (var acTrans = acCurDb.acTransST())
            using (var pline = new Polyline())
            {
                var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
                for (int i = 0; i < points.Count; i++)
                {
                    pline.AddVertexAt(i, points[i].Convert2d(plane), 0.0, 0.0, 0.0);
                }
                var ms = (BlockTableRecord)acTrans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(acCurDb), OpenMode.ForWrite);
                ms.AppendEntity(pline);
                acTrans.AddNewlyCreatedDBObject(pline, true);
                acTrans.Commit();
            }
        }
        public static System.Windows.Media.SolidColorBrush GetCor(object obj)
        {
            Color color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.White);

            if (obj is Circle)
            {
                var tt = obj as Circle;
                color = tt.Color;
            }
            else if (obj is Line)
            {
                var tt = obj as Line;
                color = tt.Color;
            }
            else if (obj is Polyline)
            {
                var tt = obj as Polyline;
                color = tt.Color;
            }
            else if (obj is Xline)
            {
                var tt = obj as Xline;
                color = tt.Color;
            }
            else if (obj is Mline)
            {
                var tt = obj as Mline;
                color = tt.Color;
            }


            var p = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(color.ColorValue.R, color.ColorValue.G, color.ColorValue.B));

            return p;
        }



        public static RMLiteFamilia GetFBs()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_FB.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }

        public static RMLiteFamilia GetPURLINS()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Purlin.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetTIRANTES()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Tirante.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetCORRENTES()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Corrente.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }

        public static RMLiteFamilia GetSUPORTES_CORRENTES()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Corrente_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetSUPORTES_PURLIN()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Purlin_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetSUPORTES_TIRANTE()
        {
            var fam = RM.GetFamilias().Find(x => x.FAMILIA.ToUpper() == Core.GetCADPurlin().RM_Familia_Tirante_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLite SelecionarPurlin(RMLite purlin)
        {
            var parecidas = new List<RMLite>();
            if (purlin == null)
            {
                parecidas = GetPURLINS().GetPecas();
            }
            else
            {
                parecidas = GetPURLINS().GetPecas().FindAll(x => x.GRUPO == x.GRUPO);

            }
            return parecidas.ListaSelecionar();
        }
        public static RMLite SelecionarPurlinSuporte()
        {
            return GetSUPORTES_PURLIN().GetPecas().FindAll(x => x.COMP_VAR).ListaSelecionar();
        }
        public static RMLite SelecionarCorrente()
        {
            return GetCORRENTES().GetPecas().FindAll(x => x.COMP_VAR).ListaSelecionar();
        }
        public static RMLite SelecionarCorrenteSuporte()
        {
            return GetSUPORTES_CORRENTES().GetPecas().FindAll(x => x.COMP_VAR).ListaSelecionar();
        }
        public static RMLite SelecionarTirante()
        {
            return GetTIRANTES().GetPecas().FindAll(x => x.COMP_VAR).ListaSelecionar();
        }
        public static RMLite SelecionarTiranteSuporte()
        {
            return GetSUPORTES_TIRANTE().GetPecas().FindAll(x => x.COMP_VAR).ListaSelecionar();
        }
    }
}

