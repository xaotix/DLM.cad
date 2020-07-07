using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Conexoes;
using Ferramentas_DLM.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ferramentas_DLM
{
    public class Utilidades
    {
        public static void InterSectionPoint()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Xline pl1 = null;
            Entity pl2 = null;
            Entity ent = null;
            PromptEntityOptions peo = null;
            PromptEntityResult per = null;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                //Select first polyline
                peo = new PromptEntityOptions("Seleciona a Xline:");
                per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                {
                    return;
                }
                //Get the polyline entity
                ent = (Entity)tx.GetObject(per.ObjectId, OpenMode.ForRead);
                if (ent is Xline)
                {
                    pl1 = ent as Xline;
                }
                //Select 2nd polyline
                peo = new PromptEntityOptions("\n Selecione o objeto:");
                per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                {
                    return;
                }
                ent = (Entity)tx.GetObject(per.ObjectId, OpenMode.ForRead);
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

                tx.Commit();
            }

        }
        public static void CriarLayer(string nome, System.Drawing.Color cor, bool setar = true)
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr =
              db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the layer table from the drawing
                LayerTable lt =
                  (LayerTable)tr.GetObject(
                    db.LayerTableId,
                    OpenMode.ForRead
                  );

                try
                {
                    // Validate the provided symbol table name
                    SymbolUtilityServices.ValidateSymbolName(
                      nome,
                      false
                    );
                    // Only set the layer name if it isn't in use
                    if (lt.Has(nome))
                    {
                        ed.WriteMessage(
                          "\nA Já existe uma layer com esse nome: " + nome

                        );
                        if (setar)
                        {
                            SetLayer(nome);
                        }
                        return;
                    }


                }
                catch
                {
                    // An exception has been thrown, indicating the
                    // name is invalid
                    ed.WriteMessage(
                      "\nNome inválido de layer: " + nome
                    );
                    return;
                }
                // Create our new layer table record...
                LayerTableRecord ltr = new LayerTableRecord();
                // ... and set its properties
                ltr.Name = nome;
                ltr.Color =
                 Autodesk.AutoCAD.Colors.Color.FromColor(cor);
                // Add the new layer to the layer table
                lt.UpgradeOpen();
                ObjectId ltId = lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);
                // Set the layer to be current for this drawing
                db.Clayer = ltId;
                // Commit the transaction
                tr.Commit();
                // Report what we've done
                ed.WriteMessage(
                  "\nLayer Criada \"{0}\" ",
                  nome
                );
                if(setar)
                {
                    SetLayer(nome);
                }
            }
        }


        public static string GetLayerAtual()
        {
            return (string)Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("clayer");
        }
        public static void SetOrtho(bool valor)
        {
            Application.DocumentManager.MdiActiveDocument.Database.Orthomode = valor;
        }
        public static MlineStyle GetEstilo(string nome)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;
            Database db = doc.Database;
            try
            {
                using (Transaction Tx =
               db.TransactionManager.StartTransaction())
                {
                    DBDictionary mlineDic =
                        (DBDictionary)Tx.GetObject(db.MLStyleDictionaryId,
                                                          OpenMode.ForRead);

                    MlineStyle acLyrTblRec = Tx.GetObject(mlineDic.GetAt(nome),
                                                                  OpenMode.ForWrite) as MlineStyle;


                    Tx.Commit();
                    return acLyrTblRec;
                }
            }
            catch (System.Exception)
            {

            }

            return null;
           
        }
        public static void EditarAtributo(BlockReference myBlockRef, Transaction tr, string tag, string valor)
        {
            AttributeCollection attCol = myBlockRef.AttributeCollection;
            foreach (ObjectId attId in attCol)
            {
                AttributeReference att = tr.GetObject(attId, OpenMode.ForRead, false) as AttributeReference;
                if (att.Tag.ToUpper().Replace(" ","") == tag.ToUpper().Replace(" ", ""))
                {
                    att.UpgradeOpen();
                    att.TextString = valor;
                }
            }
        }

        public static void EditarAtributos(BlockReference myBlockRef, Transaction tr, Hashtable t)
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
        public static Conexoes.BancoTecnoMetal db
        {
            get
            {
                if(_db ==null)
                {
                    _db = new Conexoes.BancoTecnoMetal(@"R:\DB2011\DBPROF.dbf");
                }

                return _db;
            }
        }
        private static Conexoes.BancoTecnoMetal _db { get; set; }

        public static double RadianosParaGraus(double angle, int decimais = 0)
        {
            return Math.Round(angle * (180.0 / Math.PI), decimais);
        }
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

        public static void SetLayer(string layer, bool on =true ,bool criar_senao_existe = false)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                OpenMode.ForRead) as LayerTable;


                if (acLyrTbl.Has(layer) == false && criar_senao_existe)
                {
                    using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                    {
                        // Assign the layer a name
                        acLyrTblRec.Name = layer;

                        // Upgrade the Layer table for write
                        acLyrTbl.UpgradeOpen();

                        // Append the new layer to the Layer table and the transaction
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);

                        // Turn the layer off
                        acLyrTblRec.IsOff = !on;
                        acDoc.Editor.WriteMessage("\nLayer criada e setada: " + layer);
                    }
                }
                else
                {
                    LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[layer],
                                                    OpenMode.ForWrite) as LayerTableRecord;

                    // Turn the layer off
                    acLyrTblRec.IsOff = !on;
                }

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                acCurDb.Clayer = acLyrTbl[layer];
                // Save the changes and dispose of the transaction
                acDoc.Editor.WriteMessage("\nLayer setada: " + layer);

                acTrans.Commit();
            }
        }

        private void ChangeLayer(string sLayerName)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database acCurDb = HostApplicationServices.WorkingDatabase;
            LayerTable acLyrTbl = null;
            ed.WriteMessage("\nchange layer: {0}", sLayerName);
            LayerTableRecord acLyrTblRec = null;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                acLyrTblRec = acTrans.GetObject(acLyrTbl[sLayerName], OpenMode.ForWrite) as LayerTableRecord;

                if (acLyrTblRec.IsFrozen) acLyrTblRec.IsFrozen = false;
                acLyrTblRec.IsOff = false;
                acCurDb.Clayer = acLyrTbl[sLayerName];

                // Save the changes
                acTrans.Commit();
            }

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                // turn all layers off
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    acLyrTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as LayerTableRecord;

                    if (acLyrTbl.Has(acLyrTblRec.Name))
                    {
                       if ((acLyrTblRec.Name == sLayerName) && acLyrTblRec.IsFrozen)
                        {
                            acLyrTblRec.IsFrozen = false;
                            acLyrTblRec.IsOff = false;
                        }
                    }
                }

                // Save the changes
                acTrans.Commit();

                ed.WriteMessage("\ncurrent layer: {0}", (string)Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("clayer"));
            }
        }

        public static void SetLayerEBloquear(bool bloquear, string exceto = "", bool bloquear_exceto = false)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database acCurDb = HostApplicationServices.WorkingDatabase;
            LayerTable acLyrTbl = null;
            LayerTableRecord acLyrTblRec = null;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                acLyrTblRec = acTrans.GetObject(acLyrTbl[exceto], OpenMode.ForWrite) as LayerTableRecord;

                if (acLyrTblRec.IsFrozen) acLyrTblRec.IsFrozen = false;
                acLyrTblRec.IsOff = false;
                if(acLyrTbl.Has(exceto))
                {
                acCurDb.Clayer = acLyrTbl[exceto];
                }
                // Save the changes
                acTrans.Commit();
            }

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                // turn all layers off
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    try
                    {
                        acLyrTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as LayerTableRecord;

                        if (acLyrTbl.Has(acLyrTblRec.Name))
                        {
                            if ((acLyrTblRec.Name == exceto) && acLyrTblRec.IsFrozen)
                            {
                                acLyrTblRec.IsFrozen = bloquear_exceto;
                            }
                            else
                            {
                                acLyrTblRec.IsFrozen = bloquear;
                            }
                        }
                    }
                    catch (System.Exception)
                    {

                    }
                   
                }

                // Save the changes
                acTrans.Commit();

                ed.WriteMessage("\ncurrent layer: {0}", (string)Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("clayer"));
            }
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
                double angulo = Angulo(s);
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

        public static double Angulo(Xline s)
        {
            return Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(0, 0, 0), new Calculos.Ponto3D(s.UnitDir.X, s.UnitDir.Y, 0)));
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
        public static void SynchronizeAttributes(BlockTableRecord target)
        {
            if (target == null)
            {
                return;
            }

            Transaction tr = target.Database.TransactionManager.TopTransaction;
            if (tr == null)
            {
                return;
            }
                //throw new AcRx.Exception(ErrorStatus.NoActiveTransactions);


            RXClass attDefClass = RXClass.GetClass(typeof(AttributeDefinition));
            List<AttributeDefinition> attDefs = new List<AttributeDefinition>();
            foreach (ObjectId id in target)
            {
                if (id.ObjectClass == attDefClass)
                {
                    AttributeDefinition attDef = (AttributeDefinition)tr.GetObject(id, OpenMode.ForRead);
                    attDefs.Add(attDef);
                }
            }
            foreach (ObjectId id in target.GetBlockReferenceIds(true, false))
            {
                BlockReference br = (BlockReference)tr.GetObject(id, OpenMode.ForWrite);
                ResetAttributes(br,attDefs);
            }
            if (target.IsDynamicBlock)
            {
                foreach (ObjectId id in target.GetAnonymousBlockIds())
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    foreach (ObjectId brId in btr.GetBlockReferenceIds(true, false))
                    {
                        BlockReference br = (BlockReference)tr.GetObject(brId, OpenMode.ForWrite);
                        ResetAttributes(br, attDefs);
                    }
                }
            }
        }

        public static void ResetAttributes(BlockReference br, List<AttributeDefinition> attDefs)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = br.Database.TransactionManager;
            Dictionary<string, string> attValues = new Dictionary<string, string>();
            foreach (ObjectId id in br.AttributeCollection)
            {
                if (!id.IsErased)
                {
                    AttributeReference attRef = (AttributeReference)tm.GetObject(id, OpenMode.ForWrite);
                    attValues.Add(attRef.Tag, attRef.TextString);
                    attRef.Erase();
                }
            }
            foreach (AttributeDefinition attDef in attDefs)
            {
                AttributeReference attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                if (attValues.ContainsKey(attDef.Tag))
                {
                    attRef.TextString = attValues[attDef.Tag.ToUpper()];
                }
                br.AttributeCollection.AppendAttribute(attRef);
                tm.AddNewlyCreatedDBObject(attRef, true);
            }
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
                    var ang = Angulo(p);
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
        public static void AddMensagem(string Mensagem)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Mensagem);
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
                    double angulo = Angulo(p);
                    var norm = NormalizarAngulo(angulo);
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
        
        public static double NormalizarAngulo(double angulo)
        {
            if (angulo < 0)
            {
                angulo = 360 + angulo;
            }
            if (angulo>=0 &&  angulo<45)
            {
                return 0;
            }
            else if(angulo>=45 && angulo<=90)
            {
                return 90;
            }
            else if (angulo > 90 && angulo < 135)
            {
                return 90;
            }
            else if (angulo >= 135 && angulo <= 180)
            {
                return 180;
            }
            else if (angulo > 180 && angulo <225)
            {
                return 180;
            }
            else if (angulo >= 225 && angulo < 270)
            {
                return 270;
            }
            else if(angulo>=270)
            {
                return 0;
            }
            return 0;

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
        public static List<List<string>> LerAtributosStr(BlockReference bloco)
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

        public static DB.Linha GetAtributos(BlockReference bloco)
        {
            DB.Linha retorno = new DB.Linha();
            Database acCurDb;
            acCurDb = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

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
            return retorno;
        }
        public static DB.Valor GetAtributo(BlockReference bloco, string atributo)
        {
            var s = GetAtributos(bloco);
            return s.Get(atributo);
        }
        public static void Alerta(string mensagem)
        {
            System.Windows.Forms.MessageBox.Show(mensagem);
        }

        public static void InserirBloco(Document acDoc,  string nome, Point3d origem, double escala, double rotacao, Hashtable atributos)
        {
            List<string> bibliotecas = new List<string>();
            string bloco = "";
            if(File.Exists(nome))
            {
                bloco = nome;
            }
            else
            {
                var s = Conexoes.Utilz.GetArquivos(@"R:\Simbologias", Conexoes.Utilz.getNome(nome) + "*.dwg");

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(@"R:\Blocos\SELO A2", Conexoes.Utilz.getNome(nome) + "*.dwg", SearchOption.AllDirectories);
                }
                if (s.Count == 0)
                {
                    return;
                }
                bloco = s[0];
            }

            
            InserirBloco(acDoc, bloco,Conexoes.Utilz.getNome(bloco), origem, escala,rotacao, atributos);
        }

        public static void InserirBloco(Document doc, string endereco, string nome, Point3d origem, double escala, double rotacao, Hashtable atributos)
        {
            Database curdb = doc.Database;
            Editor ed = doc.Editor;
            DocumentLock loc = doc.LockDocument();
            using (loc)
            {
                ObjectId blkid = ObjectId.Null;
                Database db = new Database(false, true);
                using (db)
                {
                    
                    using (Transaction tr = doc.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(curdb.BlockTableId, OpenMode.ForRead);
                        if (bt.Has(nome))
                        {
                            using (DocumentLock acLckDoc = doc.LockDocument())
                            {
                                //ed.WriteMessage("\nBloco já existe, adicionando atual...\n");

                                blkid = bt[nome];
                                BlockReference bref = new BlockReference(origem, blkid);
                                BlockTableRecord btr2 = (BlockTableRecord)tr.GetObject(curdb.CurrentSpaceId, OpenMode.ForWrite);
                                using (BlockTableRecord bdef =
                                           (BlockTableRecord)tr.GetObject(bref.BlockTableRecord, OpenMode.ForWrite))
                                {
                                    bref.ScaleFactors = new Scale3d(escala,escala,escala);
                                    bref.Rotation = Conexoes.Utilz.GrausParaRadianos(rotacao);
                                    bref.TransformBy(ed.CurrentUserCoordinateSystem);
                                    bref.RecordGraphicsModified(true);
                                    if (bdef.Annotative == AnnotativeStates.True)
                                    {
                                        ObjectContextCollection contextCollection = curdb.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                                        Autodesk.AutoCAD.Internal.ObjectContexts.AddContext(bref, contextCollection.GetContext("1:1"));
                                    }
                                    btr2.AppendEntity(bref);
                                    tr.AddNewlyCreatedDBObject(bref, true);

                                    foreach (ObjectId eid in bdef)
                                    {
                                        DBObject obj = (DBObject)tr.GetObject(eid, OpenMode.ForRead);
                                        if (obj is AttributeDefinition)
                                        {
                                            AttributeDefinition atdef = (AttributeDefinition)obj;
                                            AttributeReference atref = new AttributeReference();
                                            if (atdef != null)
                                            {
                                                atref = new AttributeReference();
                                                atref.SetAttributeFromBlock(atdef, bref.BlockTransform);
                                               //atref.Position = atdef.Position + bref.Position.GetAsVector();
                                                atref.Position = atdef.Position.TransformBy(bref.BlockTransform);
                                                if (atributos.ContainsKey(atdef.Tag.ToUpper()))
                                                {
                                                    atref.TextString = atributos[atdef.Tag.ToUpper()].ToString();
                                                }
                                            }
                                            bref.AttributeCollection.AppendAttribute(atref);
                                            tr.AddNewlyCreatedDBObject(atref, true);
                                        }
                                    }
                                    bref.DowngradeOpen();
                                }

                                tr.TransactionManager.QueueForGraphicsFlush();
                                doc.TransactionManager.FlushGraphics();
                                tr.Commit();
                                //doc.Editor.Regen();
                                return;
                            }
                        }



                        bt.UpgradeOpen();

                        //se nao tem, ai ele vai tentar abrir e inserir
                        db.ReadDwgFile(endereco, System.IO.FileShare.Read, true, "");
                        blkid = curdb.Insert(endereco, db, true);

                        BlockTableRecord btrec = (BlockTableRecord)blkid.GetObject(OpenMode.ForRead);
                        btrec.UpgradeOpen();
                        btrec.Name = nome;
                        btrec.DowngradeOpen();



                        BlockTableRecord btr = (BlockTableRecord)curdb.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                        using (btr)
                        {
                            using (BlockReference bref = new BlockReference(origem, blkid))
                            {
                                Matrix3d mat = Matrix3d.Identity;
                                bref.TransformBy(mat);
                                bref.ScaleFactors = new Scale3d(escala,escala, escala);
                                bref.Rotation = Conexoes.Utilz.GrausParaRadianos(rotacao);
                                bref.Position = origem;
                                btr.AppendEntity(bref);
                                tr.AddNewlyCreatedDBObject(bref, true);

                                using (BlockTableRecord btAttRec = (BlockTableRecord)bref.BlockTableRecord.GetObject(OpenMode.ForRead))
                                {
                                    Autodesk.AutoCAD.DatabaseServices.AttributeCollection atcoll = bref.AttributeCollection;

                                    foreach (ObjectId subid in btAttRec)
                                    {
                                        Entity ent = (Entity)subid.GetObject(OpenMode.ForRead);
                                        AttributeDefinition attDef = ent as AttributeDefinition;

                                        if (attDef != null)
                                        {
                                            // ed.WriteMessage("\nValue: " + attDef.TextString);
                                            AttributeReference attRef = new AttributeReference();
                                            attRef.SetPropertiesFrom(attDef);
                                            attRef.Visible = attDef.Visible;
                                            attRef.SetAttributeFromBlock(attDef, bref.BlockTransform);
                                            attRef.HorizontalMode = attDef.HorizontalMode;
                                            attRef.VerticalMode = attDef.VerticalMode;
                                            attRef.Rotation = attDef.Rotation;
                                            attRef.TextStyleId = attDef.TextStyleId;

                                            attRef.Position = attDef.Position.TransformBy(bref.BlockTransform);


                                            //attRef.Position = attDef.Position + origem.GetAsVector();
                                            attRef.Tag = attDef.Tag;
                                            attRef.FieldLength = attDef.FieldLength;
                                            attRef.TextString = attDef.TextString;

                                            attRef.AdjustAlignment(curdb);//?

                                            if (atributos.ContainsKey(attRef.Tag.ToUpper()))
                                            {
                                                attRef.TextString = atributos[attRef.Tag.ToUpper()].ToString();
                                            }

                                            atcoll.AppendAttribute(attRef);

                                            tr.AddNewlyCreatedDBObject(attRef, true);
                                        }

                                    }

                                }

                                bref.DowngradeOpen();
                            }
                        }

                        btrec.DowngradeOpen();

                        bt.DowngradeOpen();

                        ed.Regen();

                        tr.Commit();
                    }
                }
            }
        }

        public static void InserirPerfil(Point3d p0, string marca, double comprimento, Conexoes.Perfil perfil, int quantidade, string material, string tratamento, double peso = 0, double area = 0)
        {
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:

                ht.Add("MAR_PEZ", marca);
                ht.Add("NOM_PRO", perfil.Nome);
                ht.Add("QTA_PEZ", quantidade);
                ht.Add("LUN_PRO", comprimento);
                ht.Add("MAT_PRO", material);
                ht.Add("TRA_PEZ", tratamento);
                if (peso == 0)
                {
                    ht.Add("PUN_LIS", perfil.PESO * comprimento / 1000);
                }
                else
                {
                    ht.Add("PUN_LIS", peso);
                }

                if (peso == 0)
                {
                    ht.Add("SUN_LIS", perfil.SUPERFICIE * comprimento / 1000);
                }
                else
                {
                    ht.Add("SUN_LIS", area);
                }
                ht.Add("ING_PEZ", perfil.H + "*" + perfil.ABA_1 + "*" + comprimento);
                ht.Add("DIM_PRO", perfil.DIM_PRO);

                Utilidades.InserirBloco(doc, "m8_pro", p0, 10, 0, ht);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);

            }
        }


        public static void InserirChapa(Point3d p0, string marca, double comprimento, double largura, double espessura, int quantidade, string material, string tratamento, double peso = 0, double area = 0)
        {
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:

                ht.Add("MAR_PEZ", marca);
                ht.Add("QTA_PEZ", quantidade);
                ht.Add("LUN_PRO", comprimento);
                ht.Add("LAR_PRO", largura);
                ht.Add("SPE_PRO", espessura);
                ht.Add("MAT_PRO", material);
                ht.Add("TRA_PEZ", tratamento);

                if (peso == 0)
                {
                    ht.Add("PUN_LIS", Math.Round(largura * comprimento * espessura * 0.00000785, 3));
                }
                else
                {
                    ht.Add("PUN_LIS", peso);
                }

                if (peso == 0)
                {
                    ht.Add("SUN_LIS", Math.Round(largura * comprimento * espessura / 1000 / 1000 / 1000, 3));
                }
                else
                {
                    ht.Add("SUN_LIS", area);
                }


                Utilidades.InserirBloco(doc, "m8_lam", p0, 10, 0, ht);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            
        }
        public static void InserirMarcaSimplesCam(TecnoUtilz.ReadCam cam, Point3d origem)
        {

            if (cam.Familia == TecnoUtilz.Familia.Dobrado | cam.Familia == TecnoUtilz.Familia.Laminado | cam.Familia == TecnoUtilz.Familia.Soldado && !cam.Nome.Contains("_"))
            {
                Perfil perfil = db.Get(cam.Descricao);
                if(perfil!=null)
                {
                    if(perfil.Nome == "")
                    {
                        MessageBox.Show("Perfil não cadastrado: " + cam.Descricao + "\nTipo: " + cam.TipoPerfil + "\nCadastre o perfil no tecnometal e tente novamente.");
                    }
                    else
                    {
                        InserirPerfil(origem, cam.Posicao, cam.Comprimento, perfil, cam.Quantidade, cam.Material, cam.Tratamento, cam.Peso, cam.Superficie);
                    }
                }


            }
            else if(cam.Familia == TecnoUtilz.Familia.Chapa)
            {
                InserirChapa(origem, cam.Nome, cam.Comprimento, cam.Largura, cam.Espessura, cam.Quantidade, cam.Marca, cam.Tratamento, cam.Peso, cam.Superficie);
            }
            else
            {
                MessageBox.Show("Tipo de CAM inválido ou não suportado:\n" + cam.Nome + "\n" + cam.TipoPerfil);
            }
        }

        public static Point3d PedirPonto3D(string pergunta, out bool cancelado)
        {
            cancelado = false;
            return PedirPonto3D(pergunta, new Point3d(), out cancelado,false);
        }
        public static Point3d PedirPonto3D(string pergunta, Point3d origem, out bool cancelado, bool tem_origem = true)
        {
            cancelado = false;
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

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
    }
}
