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

namespace Ferramentas_DLM
{
    public static class Utilidades 
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
        public static Point3d AddLeader(double angulo, Point3d pp0, double escala, string nome = "", double multiplicador = 7.5, bool pedir_ponto = false)
        {
            try
            {
                var pt2 = new Coordenada(pp0).Mover(angulo + 45, escala * multiplicador).GetPoint();

                if(pedir_ponto)
                {
                    bool cancelado = false;
                    var pt0 = Utilidades.PedirPonto3D("Selecione o segundo ponto", pp0, out cancelado);
                    if(!cancelado)
                    {
                        pt2 = pt0;
                    }
                }

                AddLeader(pp0, pt2, nome, 2 * escala);

                return pt2;
            }
            catch (System.Exception ex)
            {

                Alerta($"{ex.Message}\n{ex.StackTrace}");
            }
            return pp0;
        }
        public static void AddLeader(Point3d origem, Point3d pt2, string texto, double escala)
        {
            using (DocumentLock docLock = acDoc.LockDocument())
            {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;

                // Create the leader with annotation
                using (Leader acLdr = new Leader())
                {
                    acLdr.AppendVertex(origem);
                    acLdr.AppendVertex(pt2);
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

        public static List<MlineStyle> GetMLineStyles()
        {
            List<MlineStyle> retorno = new List<MlineStyle>();
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    DBDictionary lays = acTrans.GetObject(acCurDb.MLStyleDictionaryId, OpenMode.ForWrite) as DBDictionary;

                    foreach (DBDictionaryEntry item in lays)
                    {
                        MlineStyle acLyrTblRec;
                        acLyrTblRec = acTrans.GetObject(item.Value, OpenMode.ForRead) as MlineStyle;

                        if (acLyrTblRec != null)
                        {
                            retorno.Add(acLyrTblRec);
                        }
                    }
                    acTrans.Abort();
                }
            }
            return retorno;
        }
        public static List<Viewport> GetViewports(string setLayerPadrao = "")
        {
            List<Viewport> retorno = new List<Viewport>();
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

                        }
                    }
                    acTrans.Commit();
                }
            }
            return retorno;
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
                    if(exato)
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
            for (int i = 0; i < pl.NumberOfVertices-1; i++)
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
                    Alerta(i  +"\n" + ex.Message + "\n" + ex.StackTrace);
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
                    Alerta(i + "\n" + ex.Message + "\n" + ex.StackTrace);
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
                        (DBDictionary)acTrans.GetObject(acCurDb.MLStyleDictionaryId,OpenMode.ForRead);

                    MlineStyle acLyrTblRec = acTrans.GetObject(mlineDic.GetAt(nome),OpenMode.ForWrite) as MlineStyle;


                    acTrans.Commit();
                    return acLyrTblRec;
                }
            }
            catch (System.Exception)
            {

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
           

            if(vertices.Count>0)
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
        public static TecnoMetal_Banco GetdbTecnoMetal()
        {
            if (_db == null)
            {
                _db = new Conexoes.TecnoMetal_Banco(Constantes.DBPROF);
            }

            return _db;
        }
        private static Conexoes.TecnoMetal_Banco _db { get; set; }
        public static void GetCoordenadas(Mline s, out Point3d p1, out Point3d p2)
        {
            List<Point3d> lista = new List<Point3d>();
            p1 = new Point3d();
            p2 = new Point3d();
            for (int i = 0; i < s.NumberOfVertices; i++)
            {
                var t = s.VertexAt(i);
                lista.Add(t);
            }
            if (lista.Count > 1)
            {
                p1 = lista.OrderBy(x => x.X).First();
                p2 = lista.OrderBy(x => x.X).Last();

                if(lista.Select(x=>Math.Round(x.X)).Distinct().ToList().Count == 1)
                {
                    //maior Y está mais pra cima
                    p1 = lista.OrderBy(x => x.Y).Last();
                    p2 = lista.OrderBy(x => x.Y).First();
                }
            }
            else if(lista.Count==1)
            {
                p1 = lista[0];
                p2 = lista[0];
            }
        }
        public static List<CTerca> MlinesPassando(Point3d de, Point3d ate, List<CTerca> LS, bool somente_dentro = false, bool dentro_do_eixo = false)
        {
            return MlinesPassando(de, ate, LS.Select(x => x as CCorrente).ToList(), somente_dentro, dentro_do_eixo).Select(x => x as CTerca).ToList();
        }
        public static List<CCorrente> MlinesPassando(Point3d de, Point3d ate, List<CCorrente> LS, bool somente_dentro = false, bool dentro_do_eixo = false)
        {
            List<CCorrente> retorno = new List<CCorrente>();
            foreach (var s in LS)
            {
                Point3d p1 = s.p0.GetPoint();
                Point3d p2 = s.p1.GetPoint();

              
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
                else if(somente_dentro && !dentro_do_eixo)
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
        public static List<Entity> LinhasPassando(Point3d de, Point3d ate, List<Entity> LS, bool somente_dentro = false, bool dentro_do_eixo = false, bool somente_vertical = true)
        {
            List<Entity> retorno = new List<Entity>();
            foreach (var s in LS)
            {
                Point3d p1, p2, centro;
                double angulo, comprimento;
                GetCoordenadas(s, out p1, out p2, out angulo, out comprimento, out centro);

                if(somente_vertical)
                {
                    if(angulo>=85 && angulo <=95)
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
        public static void GetCoordenadas(Entity s, out Point3d p1, out Point3d p2, out double angulo, out double comprimento, out Point3d centro)
        {
            p1 = new Point3d();
            p2 = new Point3d();
            centro = new Point3d();
            comprimento = 0;
            if (s is Mline)
            {
                GetCoordenadas(s as Mline, out p1, out p2);
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
            }

            angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(p1.X, p1.Y, p1.Z), new Calculos.Ponto3D(p2.X, p2.Y, p2.Z))),2);
            if (angulo > 180)
            {
                angulo = angulo - 180;
            }
            Coordenada pp = new Coordenada(p1);
            
            centro = pp.GetCentro(new Coordenada(p2)).GetPoint();


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

                if ((angulo >= 175 | angulo <= 5)&& comp>=comp_min)
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
            foreach (var s in LS.FindAll(x=>x is RotatedDimension).Select(x=> x as RotatedDimension))
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
            if(pts.Count>0)
            {
                se = new Point3d(pts.Min(x => x.X), pts.Max(x => x.Y), 0);
                sd = new Point3d(pts.Max(x => x.X), pts.Max(x => x.Y), 0);

                ie = new Point3d(pts.Min(x => x.X), pts.Min(x => x.Y), 0);
                id = new Point3d(pts.Max(x => x.X), pts.Min(x => x.Y), 0);
            }
        }
        public static List<BlockReference> GetBlocosProximos(List<BlockReference> blocos, Point3d ponto, double tolerancia)
        {
            return blocos.FindAll(x => new Coordenada(x.Position).Distancia(ponto) <= tolerancia);
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
            


                foreach(var p in xlines)
                {
                    var p1 = new Coordenada(coords.First());
                    var ang = Angulo.Get(p);
                    var p2 = p1.Mover(ang + 90, 10000);

                    var a1 = new Coordenada(p.BasePoint);
                    var a2 = new Coordenada(p.BasePoint).Mover(ang, 10000);


                   


                    using (Line ll =new Line(p1.GetPoint(),p2.GetPoint()))
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
            if (distancia<0)
            {
                retorno = null;
            }
            if(distancia>tolerancia)
            {
                retorno = null;
            }
                return retorno;
        }
        public static List<Xline> GetXlinesProximas(Entity objeto, List<Xline> xlines, double tolerancia)
        {
            List<Xline> retorno = new List<Xline>();

            var coords = GetCoordenadas(objeto);
            if(coords.Count>0)
            {
                var min = new Coordenada(new Point3d(coords.Min(x => x.X), coords.Min(x => x.Y), 0));
                var max = new Coordenada(new Point3d(coords.Max(x => x.X), coords.Max(x => x.Y), 0));
                foreach(var p in xlines)
                {
                    double angulo = Angulo.Get(p);
                    var norm = Angulo.Normalizar(angulo);
                    double dist1=0, dist2=0; 
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

            if(objeto is Line)
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
                GetCoordenadas(p, out p1, out p2);
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
        public static List<Mline> MlinesVerticais(List<Mline> LS, double comp_min = 100)
        {
            List<Mline> retorno = new List<Mline>();
            foreach (var s in LS)
            {
                List<Point3d> lista = new List<Point3d>();
                for (int i = 0; i < s.NumberOfVertices; i++)
                {
                    var t = s.VertexAt(i);
                    lista.Add(t);
                }
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(lista.Min(x => x.X), lista.Min(x => x.Y), 0), new Calculos.Ponto3D(lista.Max(x => x.X), lista.Max(x => x.Y), 0))),2);
                    var comp = Math.Abs(Calculos.Trigonometria.Distancia(lista.Max(x=>x.X),lista.Max(x=>x.Y),lista.Min(x=>x.X),lista.Min(x=>x.Y)));
                    if (angulo >= 180)
                    {
                        angulo = angulo - 180;
                    }

                    if ((angulo >= 85 && angulo <= 95) && comp >= comp_min)
                    {
                        retorno.Add(s);
                    }
                }

            }
            return retorno;
        }
        public static List<Mline> MlinesHorizontais(List<Mline> LS, double comp_min = 100)
        {
            List<Mline> retorno = new List<Mline>();
            foreach (var s in LS)
            {
                List<Point3d> lista = new List<Point3d>();
                for (int i = 0; i < s.NumberOfVertices; i++)
                {
                    var t = s.VertexAt(i);
                    lista.Add(t);
                }
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(lista.Min(x => x.X), lista.Min(x => x.Y), 0), new Calculos.Ponto3D(lista.Max(x => x.X), lista.Max(x => x.Y), 0))),2);
                    var comp = lista.Max(x => x.X) - lista.Min(x => x.X);
                    if (angulo >= 180)
                    {
                        angulo = angulo - 180;
                    }

                    if ((angulo >= 175 | angulo <= 5) && comp >= comp_min)
                    {
                        retorno.Add(s);
                    }
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

        public static bool DesenharMLine(string estilo, List<Point3d> pontos)
        {

            var mlst = GetMLineStyles().Find(x => x.Name.ToUpper() == estilo.ToUpper());

            if(mlst==null)
            {
                var arquivo = Constantes.GetArquivosMlStyles().Find(x => Conexoes.Utilz.getNome(x).ToUpper() == estilo.ToUpper());
                var nome = Conexoes.Utilz.getNome(arquivo);
                if (arquivo != null)
                {
                    /*tive que copiar o arquivo pra raiz do CAD, pq se não ele não aceita.*/
                    var destino = System.Environment.CurrentDirectory + $@"\{nome}.mln";
                    Conexoes.Utilz.Copiar(arquivo, destino,false);
                    try
                    {
                        if(File.Exists(destino))
                        {
                            using (Transaction tr = acCurDb.TransactionManager.StartOpenCloseTransaction())
                            {
                                CAD.acCurDb.LoadMlineStyleFile(nome, destino);

                                tr.Commit();
                            }
                        }
                        else
                        {
                            Alerta($"Abortado\n Foi tentado copiar o arquivo {nome} para \n{destino} e não foi possível. \nContacte suporte.");
                            return false;
                        }
   
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Erro tentando carregar a MLStyle {estilo}\n\n{ex.Message}\n{ex.StackTrace}");
                        return false;
                    }
                }
                mlst = GetMLineStyles().Find(x => x.Name.ToUpper() == estilo.ToUpper());

            }


            try
            {

                if (mlst != null)
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        //get the mline style
                        Mline line = new Mline();
                        //get the current mline style
                        line.Style = mlst.ObjectId;
                        line.Normal = Vector3d.ZAxis;
                        foreach (var pt in pontos)
                        {
                            line.AppendSegment(pt);
                        }

                        //open modelpace
                        ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acCurDb);
                        BlockTableRecord acBlkTblRec = tr.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acBlkTblRec.AppendEntity(line);

                        tr.AddNewlyCreatedDBObject(line, true);

                        tr.Commit();

                    }
                }
                else
                {
                    Alerta($"Não foi possível carregar o arquivo de MLStyle: {estilo}");
                    return false;
                }

            }
            catch (System.Exception ex)
            {

                Alerta($"Abortado.\n\nErro ao tentar inserir a MLine: \n{ex.Message}\n\n{ex.StackTrace}");
                return false;
            }

            return true;


        }


        public static List<Point3d> GetContorno(BlockReference s, Transaction tr)
        {
            List<Point3d> pts = new List<Point3d>();
            BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(s.BlockTableRecord, OpenMode.ForRead);
            foreach (ObjectId id in acBlkTblRec)
            {

                var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is Line)
                {
                    var tt = obj as Line;
                    Point3d p1 = tt.StartPoint.TransformBy(s.BlockTransform);
                    Point3d p2 = tt.EndPoint.TransformBy(s.BlockTransform);
                    pts.Add(p1);
                    pts.Add(p2);
                }
                else if (obj is Polyline)
                {
                    var tt = obj as Polyline;
                    int vn = tt.NumberOfVertices;
                    for (int i = 0; i < vn; i++)

                    {

                        // Could also get the 3D point here

                        Point3d pt = tt.GetPoint3dAt(i).TransformBy(s.BlockTransform);

                        pts.Add(pt);

                    }



                }
            }
            return pts;
        }


        public static void AddMensagem(string Mensagem)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Mensagem);
        }
        public static void Alerta(string mensagem, string titulo = "Atenção!", System.Windows.Forms.MessageBoxIcon Icone = System.Windows.Forms.MessageBoxIcon.Information)
        {
            System.Windows.Forms.MessageBox.Show(mensagem,titulo, System.Windows.Forms.MessageBoxButtons.OK, Icone);
        }


        public static double GetSuperficieM2(double espessura_mm, double area_mm2,  double perimetro_mm, int decimais = 4)
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
            return PedirPonto3D(pergunta, new Point3d(), out cancelado,false);
        }
        public static Point3d PedirPonto3D(string pergunta, Point3d origem, out bool cancelado, bool tem_origem = true)
        {
            cancelado = false;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            pPtOpts.Message = "\n" + pergunta;
            if(tem_origem)
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

        public static List<Point3d> PedirPontos3D(string pergunta = "Selecione as origens")
        {
            List<Point3d> retorno = new List<Point3d>();
          bool cancelado = false;
            while (!cancelado)
            {
                var pt = PedirPonto3D(pergunta, retorno.Count > 0 ? retorno.Last() : new Point3d(), out cancelado, retorno.Count>0);
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


    }
}
