using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Conexoes;
using Ferramentas_DLM.Classes;
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
using static Ferramentas_DLM.Constantes;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using static Ferramentas_DLM.CAD;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using System.Windows;

namespace Ferramentas_DLM
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
        public static Point2d AddLeader(double angulo, Point3d pp0, double escala, string nome = "", double multiplicador = 7.5, bool pedir_ponto = false)
        {
            return AddLeader(angulo, new Point2d(pp0.X, pp0.Y), escala, nome, multiplicador = 7.5, pedir_ponto);
        }
        public static Point2d AddLeader(double angulo, Point2d pp0, double escala, string nome = "", double multiplicador = 7.5, bool pedir_ponto = false)
        {
            try
            {
                var pt2 = new Coordenada(pp0).Mover(angulo + 45, escala * multiplicador).GetPoint2d();

                if (pedir_ponto)
                {
                    bool cancelado = false;
                    var pt0 = Ut.PedirPonto2D("Selecione o segundo ponto", pp0, out cancelado);
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

                Conexoes.Utilz.Alerta($"{ex.Message}\n{ex.StackTrace}");
            }
            return pp0;
        }
        public static void AddLeader(Point2d origem, Point2d pt2, string texto, double escala)
        {
            Point3d p1 = new Point3d(origem.X, origem.Y, 0);
            Point3d p2 = new Point3d(pt2.X, pt2.Y, 0);
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
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
                                catch (System.Exception)
                                {

                                }


                            }
                        }

                    }
                    acTrans.Commit();
                }
            }
        }
        public static List<Layout> GetLayouts()
        {
            List<Layout> retorno = new List<Layout>();
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    DBDictionary lays = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                    foreach (DBDictionaryEntry item in lays)
                    {
                        Layout acLyrTblRec;
                        acLyrTblRec = acTrans.GetObject(item.Value, OpenMode.ForWrite) as Layout;

                        if (acLyrTblRec != null)
                        {
                            retorno.Add(acLyrTblRec);


                            var views = acLyrTblRec.GetViewports();
                            foreach (ObjectId view in views)
                            {
                                Viewport vp = acTrans.GetObject(view, OpenMode.ForWrite) as Viewport;
                            }

                        }
                    }
                    acTrans.Abort();
                }
            }
            return retorno;
        }

        public static List<Entity> SelecionarTudoPrancha()
        {
            List<Entity> retorno = new List<Entity>();
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
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
                using (DocumentLock docLock = acDoc.LockDocument())
                {
                    using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
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
                                    catch (System.Exception)
                                    {

                                    }
   
                                }

                            }
                        }
                        acTrans.Commit();
                    }
                }
            }
            catch (System.Exception)
            {

            }

            return retorno;
        }


        public static Point3d Centro(Point3d p1, Point3d p2)
        {
            return new Coordenada(p1).GetCentro(p2).GetPoint3D();
        }
        public static Point3d Mover(Point3d p, double angulo, double distancia)
        {
            return new Coordenada(p).Mover(angulo, distancia).GetPoint3D();
        }

        public static void ListarQuantidadeBlocos()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);

                var brclass = RXObject.GetClass(typeof(BlockReference));

                var blocks = acBlkTblRec
                    .Cast<ObjectId>()
                    .Where(id => id.ObjectClass == brclass)
                    .Select(id => (BlockReference)tr.GetObject(id, OpenMode.ForRead))
                    .GroupBy(br => ((BlockTableRecord)tr.GetObject(
                        br.DynamicBlockTableRecord, OpenMode.ForRead)).Name);

                foreach (var group in blocks.OrderBy(x => x.Key))
                {
                    ed.WriteMessage($"\n{group.Key}: {group.Count()}");
                }
                tr.Commit();
            }
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
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
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
        public static List<BlockReference> Filtrar(List<BlockReference> blocos, List<string> nomes, bool exato = true)
        {
            List<BlockReference> marcas = new List<BlockReference>();

            foreach (var b in blocos)
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

            return marcas;
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
                    Conexoes.Utilz.Alerta(i + "\n" + ex.Message + "\n" + ex.StackTrace);
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
                    Conexoes.Utilz.Alerta(i + "\n" + ex.Message + "\n" + ex.StackTrace);
                }

            }
            return segmentos;
        }
        public static void SetOrtho(bool valor)
        {
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = valor;
        }
        public static MlineStyle GetEstilo(string nome)
        {
            try
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    DBDictionary mlineDic =
                        (DBDictionary)acTrans.GetObject(acCurDb.MLStyleDictionaryId, OpenMode.ForRead);

                    MlineStyle acLyrTblRec = acTrans.GetObject(mlineDic.GetAt(nome), OpenMode.ForWrite) as MlineStyle;


                    return acLyrTblRec;
                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

            return null;

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
        public static void GetInfo(Polyline lwp, out double comprimento, out double largura, out double area, out double perimetro)
        {

            var vertices = GetVertices(lwp);
            comprimento = 0;
            largura = 0;
            area = lwp.Area;
            perimetro = lwp.Length;


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

            }

        }


        public static void Apagar(List<Entity> entities)
        {
            if (entities.Count == 0) { return; }
            using (acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (var b in entities)
                    {
                        Entity acEnt = acTrans.GetObject(b.ObjectId, OpenMode.ForWrite) as Entity;
                        acEnt.Erase(true);
                    }
                    acTrans.Commit();

                }
                acDoc.Editor.Regen();
            }
        }


        public static List<CADMline> MlinesPassando(Point2d de, Point2d ate, List<CADMline> LS,  bool dentro_do_eixo = false, double tol_X = 0)
        {
            List<CADMline> retorno = new List<CADMline>();
            Point3d nde = new Point3d(de.X - tol_X, de.Y,0);
            Point3d nate = new Point3d(ate.X + tol_X, ate.Y, 0);


            foreach (var corrente in LS)
            {
                Point2d p1 = corrente.Inicio;
                Point2d p2 = corrente.Fim;


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
        public static List<Entity> LinhasPassando(Point2d de, Point2d ate, List<Entity> LS, bool somente_dentro = false, bool dentro_do_eixo = false, bool somente_vertical = true)
        {
            List<Entity> retorno = new List<Entity>();
            foreach (var s in LS)
            {
                Point3d p1, p2, centro;
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
        public static void GetCoordenadas(Entity s, out Point3d p1, out Point3d p2, out double angulo, out double comprimento, out Point3d centro, out double largura)
        {
            p1 = new Point3d();
            p2 = new Point3d();
            centro = new Point3d();
            comprimento = 0;
            largura = 0;
            if (s is Mline)
            {
                Multiline.GetOrigens(s as Mline, out p1, out p2, out largura);
                comprimento = Calculos.Trigonometria.Distancia(p1.X, p1.Y, p2.X, p2.Y);
            }
            else if (s is Line)
            {
                var l = s as Line;
                p1 = l.StartPoint;
                p2 = l.EndPoint;
                comprimento = l.Length;
            }
            else if (s is Polyline)
            {
                var l = s as Polyline;
                p1 = l.StartPoint;
                p2 = l.EndPoint;
                comprimento = l.Length;
                largura = ((double)l.LineWeight);
            }

            angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(p1.X, p1.Y, p1.Z), new Calculos.Ponto3D(p2.X, p2.Y, p2.Z))), 2);
            if (angulo > 180)
            {
                angulo = angulo - 180;
            }
            Coordenada pp = new Coordenada(p1);

            centro = pp.GetCentro(new Coordenada(p2)).GetPoint3D();


        }

        public static Point2d Centro(Point2d p1, Point2d p2)
        {
            return new Point2d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        public static List<Line> LinhasHorizontais(List<Line> LS, double comp_min = 100)
        {
            List<Line> retorno = new List<Line>();
            foreach (var s in LS)
            {
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z), new Calculos.Ponto3D(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z)));
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
        public static List<Xline> XLinesHorizontais(List<Xline> LS, double comp_min = 0)
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
        public static List<Polyline> PolylinesVerticais(List<Polyline> LS, double comp_min = 100)
        {
            List<Polyline> retorno = new List<Polyline>();
            foreach (var s in LS)
            {
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.StartPoint.X, s.StartPoint.Y, 0), new Calculos.Ponto3D(s.EndPoint.X, s.EndPoint.Y, 0)));
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
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.XLine1Point.X, s.XLine1Point.Y, 0), new Calculos.Ponto3D(s.XLine2Point.X, s.XLine2Point.Y, 0)));
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
        public static List<BlockReference> GetBlocosProximos(List<BlockReference> blocos, Point2d ponto, double tolerancia)
        {
            return blocos.FindAll(x => Math.Abs(new Point2d(x.Position.X,x.Position.Y).GetDistanceTo(ponto)) <= tolerancia);
        }
        public static List<Polyline> GetPolylinesProximas(List<Polyline> blocos, Point3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => new Coordenada(x.StartPoint).Distancia(ponto) <= tolerancia | new Coordenada(x.EndPoint).Distancia(ponto) <= tolerancia);
        }
        public static List<RotatedDimension> GetCotasProximas(List<RotatedDimension> blocos, Point3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => new Coordenada(x.XLine1Point).Distancia(ponto) <= tolerancia | new Coordenada(x.XLine2Point).Distancia(ponto) <= tolerancia);
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
                    var p1 = new Coordenada(coords.First());
                    var ang = Angulo.Get(p);
                    var p2 = p1.Mover(ang + 90, 10000);

                    var a1 = new Coordenada(p.BasePoint);
                    var a2 = new Coordenada(p.BasePoint).Mover(ang, 10000);





                    using (Line ll = new Line(p1.GetPoint3D(), p2.GetPoint3D()))
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



                            AddMensagem("\nLine: origem: " + p1);
                            AddMensagem("\nLine: Intersecção: " + pt);
                            AddMensagem("\nLine: Distância: " + dist);

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
            AddMensagem("\nXline Menor distância: " + distancia);
            AddMensagem("\nXline tolerância: " + tolerancia);
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
                var min = new Coordenada(new Point3d(coords.Min(x => x.X), coords.Min(x => x.Y), 0));
                var max = new Coordenada(new Point3d(coords.Max(x => x.X), coords.Max(x => x.Y), 0));
                foreach (var p in xlines)
                {
                    double angulo = Angulo.Get(p);
                    var norm = Angulo.Normalizar(angulo);
                    double dist1 = 0, dist2 = 0;
                    if (norm == 0 | norm == 180)
                    {
                        dist1 = new Coordenada(min).DistanciaX(p.BasePoint);
                        dist2 = new Coordenada(max).DistanciaX(p.BasePoint);
                    }





                    Coordenada pmin = new Coordenada(min).Mover(angulo, dist1);
                    Coordenada pmax = new Coordenada(min).Mover(angulo, dist2);
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
        public static List<Point3d> GetCoordenadas(Entity objeto)
        {
            List<Point3d> retorno = new List<Point3d>();

            if (objeto is Line)
            {
                var p = (objeto as Line);
                retorno.Add(p.StartPoint);
                retorno.Add(p.EndPoint);
            }
            else if (objeto is Polyline)
            {
                var p = (objeto as Polyline);
                retorno.Add(p.StartPoint);
                retorno.Add(p.EndPoint);
            }
            else if (objeto is RotatedDimension)
            {
                var p = (objeto as RotatedDimension);
                retorno.Add(p.XLine1Point);
                retorno.Add(p.XLine2Point);
            }
            else if (objeto is BlockReference)
            {
                var p = (objeto as BlockReference);
                retorno.Add(p.Position);
            }
            else if (objeto is Mline)
            {
                var p = (objeto as Mline);
                Point3d p1;
                Point3d p2;
                double largura = 0;
                Multiline.GetOrigens(p, out p1, out p2, out largura);
                retorno.Add(p1);
                retorno.Add(p1);
            }

            return retorno;
        }
        public static List<RotatedDimension> CotasHorizontais(List<Entity> LS, double comp_min = 100)
        {
            List<RotatedDimension> retorno = new List<RotatedDimension>();
            foreach (var s in LS.FindAll(x => x is RotatedDimension).Select(x => x as RotatedDimension))
            {
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.XLine1Point.X, s.XLine1Point.Y, s.XLine1Point.Z), new Calculos.Ponto3D(s.XLine2Point.X, s.XLine2Point.Y, s.XLine2Point.Z)));
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
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.StartPoint.X, s.StartPoint.Y, 0), new Calculos.Ponto3D(s.EndPoint.X, s.EndPoint.Y, 0)));
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
                var angulo = Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z), new Calculos.Ponto3D(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z)));
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
            var cor = Conexoes.FuncoesCanvas.Cores.White;
            List<UIElement> retorno = new List<UIElement>();
            if (obj is BlockReference)
            {
                var s = obj as BlockReference;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(s.BlockTableRecord, OpenMode.ForRead);

                foreach (ObjectId id in acBlkTblRec)
                {

                    var obj1 = tr.GetObject(id, OpenMode.ForRead);

                   var  ptss= Ut.GetPontosAgrupados(obj1,tr);

                    cor = Ut.GetCor(obj1);

                   foreach(var pts in ptss)
                    {
                        if (pts.Count > 1)
                        {

                            for (int i = 1; i < pts.Count; i++)
                            {
                                var pt1 = pts[i - 1].TransformBy(s.BlockTransform);
                                var pt2 = pts[i].TransformBy(s.BlockTransform);
                                if(Math.Abs(pt1.DistanceTo(pt2))>=comp_min)
                                {
                                    var p1 = new Coordenada(pt1).GetWinPoint();
                                    var p2 = new Coordenada(pt2).GetWinPoint();
                                    p1 = new Point((p1.X - p0.X) * escala, (p1.Y - p0.Y) * escala);
                                    p2 = new Point((p2.X - p0.X) * escala, (p2.Y - p0.Y) * escala);

                                    retorno.Add(Conexoes.FuncoesCanvas.Linha(p1, p2, cor));
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

                        if(Math.Abs(pt1.DistanceTo(pt2))>=comp_min)
                        {
                            var p1 = new Coordenada(pt1).GetWinPoint();
                            var p2 = new Coordenada(pt2).GetWinPoint();
                            p1 = new Point((p1.X - p0.X) * escala, (p1.Y - p0.Y) * escala);
                            p2 = new Point((p2.X - p0.X) * escala, (p2.Y - p0.Y) * escala);
                            cor.Opacity = opacidade;
                            retorno.Add(Conexoes.FuncoesCanvas.Linha(p1, p2, cor));
                        }

                    }
                }
            }
            return retorno;
        }



        public static void GetInfos()
        {
            string msg = "";
            var selecao = editor.GetEntity("\nSelecione: ");
            if (selecao.Status != PromptStatus.OK)
                return;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                Entity obj = acTrans.GetObject(selecao.ObjectId, OpenMode.ForRead) as Entity;

                msg = string.Format("Propriedades de {0}:\n", selecao.GetType().Name);



                msg += "\n\nPropriedades custom\n\n";

                msg += RetornaCustomProperties(obj.ObjectId, editor);

                var props = GetOPMProperties(obj.ObjectId);

                foreach (var pair in props)
                {
                    msg += string.Format("\t{0} = {1}\n", pair.Key, pair.Value);

                    if (Marshal.IsComObject(pair.Value))
                        Marshal.ReleaseComObject(pair.Value);
                }


                msg += "\n\nPropriedades padrao\n\n";
                PropertyInfo[] piArr = obj.GetType().GetProperties();
                foreach (PropertyInfo pi in piArr)
                {
                    object value = null;
                    try
                    {
                        value = pi.GetValue(obj, null);
                    }
                    catch (System.Exception ex)
                    {
                        //if (ex.InnerException is Autodesk.AutoCAD.Runtime.Exception &&
                        //    (ex.InnerException as Autodesk.AutoCAD.Runtime.Exception).ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.NotApplicable)
                        //    continue;
                        //else
                        //    throw;
                        msg += string.Format("\t{0}: {1}\n", pi.Name, "Erro ao tentar ler: " + ex.Message);
                    }

                    msg += string.Format("\t{0}: {1}\n", pi.Name, value);
                }



                //AddMensagem("\n" + msg);
            }



            Conexoes.Utilz.JanelaTexto(msg, "Propriedades");
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
        private static string RetornaCustomProperties(ObjectId id, Editor ed)
        {
            string msg = "";
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var acTrans = id.Database.TransactionManager.StartOpenCloseTransaction())
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

        public static System.Windows.Point GetWPoint(Point2d pt)
        {
            return new Point(pt.X, pt.Y);
        }
        public static Point3d GetP3d(Point2d pt)
        {
            return new Point3d(pt.X, pt.Y,0);
        }
        public static Point2d Mover(Point2d origem, double angulo, double distancia)
        {
            double angleRadians = (Math.PI * (angulo) / 180.0);
            return  new Point2d(((double)origem.X + (Math.Cos(angleRadians) * distancia)), ((double)origem.Y + (Math.Sin(angleRadians) * distancia)));
        }


        public static void AddMensagem(string Mensagem)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Mensagem);
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
        public static Point3d PedirPonto3D(string pergunta, out bool cancelado)
        {
            cancelado = false;
            return PedirPonto3D(pergunta, new Point3d(), out cancelado, false);
        }
        public static Point2d PedirPonto2D(string pergunta, out bool cancelado)
        {
            cancelado = false;
            return PedirPonto2D(pergunta, new Point2d(), out cancelado, false);
        }
        public static Point3d PedirPonto3D(string pergunta, Point3d origem, out bool cancelado, bool tem_origem = true)
        {
            cancelado = false;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            pPtOpts.Message = "\n" + pergunta;
            if (tem_origem)
            {
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = origem;
            }
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);

            Point3d ptStart = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel)
            {
                cancelado = true;
                return new Point3d();
            }

            return ptStart;

        }

        public static Point2d PedirPonto2D(string pergunta, Point2d origem, out bool cancelado, bool tem_origem = true)
        {
            cancelado = false;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            pPtOpts.Message = "\n" + pergunta;
            if (tem_origem)
            {
                pPtOpts.UseBasePoint = true;
                pPtOpts.BasePoint = new Point3d(origem.X, origem.Y,0);
            }
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);

            Point3d ptStart = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel)
            {
                cancelado = true;
                return new Point2d();
            }

            return new Point2d(ptStart.X, ptStart.Y);

        }

        public static List<Point3d> PedirPontos3D(string pergunta = "Selecione as origens")
        {
            List<Point3d> retorno = new List<Point3d>();
            bool cancelado = false;
            while (!cancelado)
            {
                var pt = PedirPonto3D(pergunta, retorno.Count > 0 ? retorno.Last() : new Point3d(), out cancelado, retorno.Count > 0);
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

            resultado = acDoc.Editor.GetDistance(opcoes);

            var retorno = resultado.Value;

            if (resultado.Status == PromptStatus.Cancel)
            {
                cancelado = true;
                return 0;
            }

            return retorno;

        }

        public static System.Collections.Generic.List<ObjectId> getLayoutIds(Database db)
        {
            System.Collections.Generic.List<ObjectId> layoutIds =
                new System.Collections.Generic.List<ObjectId>();
            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDic = Tx.GetObject(
                    db.LayoutDictionaryId,
                    OpenMode.ForRead, false)
                        as DBDictionary;
                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    layoutIds.Add(entry.Value);
                }
            }
            return layoutIds;
        }


        public static void LimparDesenho()
        {

            editor.Command("-SCALELISTEDIT", "R", "Y", "e","");
            editor.Command("-SCALELISTEDIT", "d", "*", "e","");
            editor.Command("-purge", "all", "*", "N","");
            Ut.IrModel();
            editor.Command("-overkill", "all", "", "");
            Ut.IrLayout();
            editor.Command("-overkill", "all", "", "");
            editor.Command("AUDIT", "Y", "");
            
            //Ut.Purge();
        }
        public static void IrLayout()
        {
            var lista = Ut.GetLayouts().Select(x => x.LayoutName).ToList().FindAll(x => x.ToUpper() != "MODEL");
            if (lista.Count > 0)
            {
                using (acDoc.LockDocument())
                {
                    LayoutManager.Current.CurrentLayout = lista[0];
                }
            }

        }
        public static void ZoomExtend()
        {
            CAD.acadApp.ZoomExtents();
        }
        public static void SetLts(int valor = 10)
        {
            var st = editor.Command("LTSCALE", valor, "");
        }
        public static void IrModel()
        {
            LayoutManager.Current.CurrentLayout = "Model";
        }
        public static int Purge(Database acCurDb = null)
        {
            int idCount = 0;
            if(acCurDb == null)
            {
                acCurDb = CAD.acCurDb;
            }
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Create the list of objects to "purge"
                ObjectIdCollection idsToPurge = new ObjectIdCollection();

                // Add all the Registered Application names
                RegAppTable rat = (RegAppTable)acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead);
                foreach (ObjectId raId in rat)
                {
                    if (raId.IsValid)
                    {
                        idsToPurge.Add(raId);
                    }
                }


                // Call the Purge function to filter the list
                acCurDb.Purge(idsToPurge);

                CAD.editor.WriteMessage("\nObjetos que foram eliminados com o purge: ");


                // Erase each of the objects we've been
                // allowed to
                foreach (ObjectId id in idsToPurge)
                {
                    DBObject obj = acTrans.GetObject(id, OpenMode.ForWrite);

                    RegAppTableRecord ratr = obj as RegAppTableRecord;
                    if (ratr != null)
                    {
                        CAD.editor.WriteMessage("\"{0}\" ", ratr.Name);
                    }
                    obj.Erase();
                }

                idCount = idsToPurge.Count;
                acTrans.Commit();
            }
            return idCount;
        }





        public static List<Point3d> GetPontos(object obj, Transaction tr = null)
        {
            List<Point3d> ptss = new List<Point3d>();
            if(tr==null)
            {
                using (DocumentLock docLock = CAD.acDoc.LockDocument())
                {
                    // Start a transaction
                    using (Transaction acTrans = CAD.acCurDb.TransactionManager.StartTransaction())
                    {
                        ptss.AddRange(GetPontosAgrupados(obj,acTrans).SelectMany(x => x));
                    }
                }
            }
            else
            {
                ptss.AddRange(GetPontosAgrupados(obj,tr).SelectMany(x => x));
            }


            return ptss;
        }
        /// <summary>
        /// Retorna pontos dos vértices dos objetos
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static List<List<Point3d>> GetPontosAgrupados(object obj, Transaction tr = null)
        {
            List<List<Point3d>> pts2 = new List<List<Point3d>>();
            List<Point3d> pts = new List<Point3d>();
            var cor = Conexoes.FuncoesCanvas.Cores.White;
            if (obj is Line)
            {
                var tt = obj as Line;
                cor = Ut.GetCor(tt.Color);
                pts.Add(tt.StartPoint);
                pts.Add(tt.EndPoint);
            }
            else if (obj is Polyline)
            {
                var tt = obj as Polyline;
                for (int i = 0; i < tt.NumberOfVertices; i++)
                {
                    var t = tt.GetPoint3dAt(i);
                    pts.Add(t);
                }

                if(tt.Closed && pts.Count>0)
                {
                    pts.Add(pts[0]);
                }
                cor = Ut.GetCor(tt.Color);
            }
            else if (obj is Circle)
            {
                var tt = obj as Circle;
                cor = Ut.GetCor(tt.Color);
                pts.AddRange(GetPontosCirculo(tt.Center, tt.Diameter, 16).Select(x=>x.GetPoint3D()));

                if(pts.Count>0)
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

                if(tt.IsClosed)
                {
                    pts.Add(pts[0]);
                }
            }
            else if (obj is BlockReference && tr!=null)
            {
                var s = obj as BlockReference;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(s.BlockTableRecord, OpenMode.ForRead);

                foreach (ObjectId id in acBlkTblRec)
                {
                    var obj1 = tr.GetObject(id, OpenMode.ForRead);
                    pts2.AddRange(Ut.GetPontosAgrupados(obj1, tr).Select(x => x.Select(y=>y.TransformBy(s.BlockTransform)).ToList()).ToList());
                }
            }
            pts2.Add(pts);
            return pts2;
        }
        public static void PoliLinha(List<Point3d> points)
        {
            using (acDoc.LockDocument())
            using (var tr = acDoc.TransactionManager.StartTransaction())
            using (var pline = new Polyline())
            {
                var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
                for (int i = 0; i < points.Count; i++)
                {
                    pline.AddVertexAt(i, points[i].Convert2d(plane), 0.0, 0.0, 0.0);
                }
                var ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(acCurDb), OpenMode.ForWrite);
                ms.AppendEntity(pline);
                tr.AddNewlyCreatedDBObject(pline, true);
                tr.Commit();
            }
        }
        public static System.Windows.Media.SolidColorBrush GetCor(object obj)
        {
            Color color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.White);

            if(obj is Circle)
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


            var p = new  System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(color.ColorValue.R, color.ColorValue.G, color.ColorValue.B));

            return p;
        }
        public static List<Coordenada> GetPontosCirculo(Point3d centro, double Diametro, int Num_Faces)
        {
            List<Point3d> retorno = new List<Point3d>();
            double ang0 = (double)360 / (double)Num_Faces;
            List<Coordenada> tamp = new List<Coordenada>();
            double ang = ang0;
            for (int i = 0; i < Num_Faces; i++)
            {

                Coordenada pt = new Coordenada(centro).Mover(ang, Diametro / 2);

                ang = ang + ang0;
                retorno.Add(pt.GetPoint3D());
            }
            return retorno.Select(x=> new Coordenada(x)).ToList();
        }


        public static RMLiteFamilia GetFBs()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_FB.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }

        public static RMLiteFamilia GetPURLINS()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Purlin.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetTIRANTES()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Tirante.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetCORRENTES()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Corrente.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }

        public static RMLiteFamilia GetSUPORTES_CORRENTES()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Corrente_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetSUPORTES_PURLIN()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Purlin_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLiteFamilia GetSUPORTES_TIRANTE()
        {
            var fam = Conexoes.DBases.GetPecasLite().Find(x => x.FAMILIA.ToUpper() == Core.CADPurlin.RM_Familia_Tirante_Suporte.ToUpper());

            if (fam != null)
            {
                return fam;
            }
            return new RMLiteFamilia();
        }
        public static RMLite SelecionarPurlin(RMLite purlin)
        {
            List<RMLite> parecidas = new List<RMLite>();
            if(purlin ==null)
            {
                parecidas = GetPURLINS().GetPecas();
            }
            else
            {
            parecidas = GetPURLINS().GetPecas().FindAll(x => x.GRUPO == x.GRUPO);

            }
            return Conexoes.Utilz.Selecao.SelecionarObjeto(parecidas);
        }
        public static RMLite SelecionarPurlinSuporte()
        {
            return Conexoes.Utilz.Selecao.SelecionarObjeto(GetSUPORTES_PURLIN().GetPecas().FindAll(x => x.COMP_VAR));
        }
        public static RMLite SelecionarCorrente()
        {
            return Conexoes.Utilz.Selecao.SelecionarObjeto(GetCORRENTES().GetPecas().FindAll(x=>x.COMP_VAR));
        }
        public static RMLite SelecionarCorrenteSuporte()
        {
            return Conexoes.Utilz.Selecao.SelecionarObjeto(GetSUPORTES_CORRENTES().GetPecas().FindAll(x => x.COMP_VAR));
        }
        public static RMLite SelecionarTirante()
        {
            return Conexoes.Utilz.Selecao.SelecionarObjeto(GetTIRANTES().GetPecas().FindAll(x => x.COMP_VAR));
        }
        public static RMLite SelecionarTiranteSuporte()
        {
            return Conexoes.Utilz.Selecao.SelecionarObjeto(GetSUPORTES_TIRANTE().GetPecas().FindAll(x => x.COMP_VAR));
        }
    }
}

