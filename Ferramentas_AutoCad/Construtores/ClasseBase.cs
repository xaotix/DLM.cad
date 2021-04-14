using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using Autodesk.AutoCAD.Runtime;
using Ferramentas_DLM.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ferramentas_DLM
{
    [Serializable]
    public class ClasseBase
    {
       [Category("Configuração")]
       [DisplayName("Layer Blocos")]
        public string LayerBlocos { get; set; } = "BLOCOS";

        [Category("Configuração")]
        [DisplayName("Pasta Arquivo")]
        public string Pasta
        {
            get
            {
                var pasta = Conexoes.Utilz.getPasta(this.acDoc.Name).ToUpper();

                if (!Directory.Exists(pasta))
                {
                    pasta = Conexoes.Utilz.RaizAppData();
                }
                return pasta;
            }
        }

        public List<string> GetMLStyles()
        {
            List<string> estilos = new List<string>();
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.MLStyleDictionaryId,
                                             OpenMode.ForRead) as DBDictionary;

                foreach (var acObjId in acLyrTbl)
                {
                    MlineStyle acLyrTblRec;
                    acLyrTblRec = acTrans.GetObject(acObjId.Value,
                                                    OpenMode.ForRead) as MlineStyle;

                    estilos.Add(acLyrTblRec.Name);
                }

            }
            return estilos;
        }
        public List<string> GetLayers()
        {
            List<string> lstlay = new List<string>();

            LayerTableRecord layer;
            using (Transaction tr = this.acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                LayerTable lt = tr.GetObject(this.acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layerId in lt)
                {
                    layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                    lstlay.Add(layer.Name);
                }

            }
            return lstlay;
        }
        public void SetUCSParaWorld()
        {
            acDoc.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;
            acDoc.Editor.Regen();
        }
        public void GetInfos()
        {
            string msg = "";
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            var selecao = ed.GetEntity("\nSelecione: ");
            if (selecao.Status != PromptStatus.OK)
                return;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                Entity obj = acTrans.GetObject(selecao.ObjectId, OpenMode.ForRead) as Entity;

                msg = string.Format("Propriedades de {0}:\n", selecao.GetType().Name);



                msg += "\n\nPropriedades custom\n\n";

                msg += RetornaCustomProperties(obj.ObjectId, ed);

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

        private string RetornaCustomProperties(ObjectId id, Editor ed)
        {
            string msg = "";
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var dbObj = tr.GetObject(id, OpenMode.ForRead);
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
                tr.Commit();
            }

            return msg;
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


        #region Tive que adicionar isso por causa do leader - no cad 2012 dá pau
        //isso daqui está aqui só por causa do Leader.
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Unicode, EntryPoint = "?attachAnnotation@AcDbLeader@@UAE?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@@Z")]
        private static extern ErrorStatus attachAnnotation32(IntPtr thisPtr, ref ObjectId annoId);
        [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Unicode, EntryPoint = "?attachAnnotation@AcDbLeader@@UEAA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@@Z")]
        private static extern ErrorStatus attachAnnotation64(IntPtr thisPtr, ref ObjectId annoId);
        private static ErrorStatus attachAnnotation(IntPtr thisPtr, ref ObjectId annoId)

        {

            if (Marshal.SizeOf(IntPtr.Zero) > 4)

                return attachAnnotation64(thisPtr, ref annoId);

            return attachAnnotation32(thisPtr, ref annoId);

        }
        #endregion
        [XmlIgnore]
        [Browsable(false)]
        public Document acDoc
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            }
        }
        [XmlIgnore]
        [Browsable(false)]
        public Database acCurDb
        {
            get
            {
                return acDoc.Database;
            }
        }

        public void Alerta(string mensagem)
        {
            System.Windows.Forms.MessageBox.Show(mensagem);
        }
        public void Comando(params object[] comando)
        {
            Extensoes.Command(this.acDoc.Editor, comando);
        }
        public List<Coordenada> RemoverRepetidos(List<Coordenada> pts)
        {
            List<Coordenada> lista = new List<Coordenada>();
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                if (i > 0 && lista.Count > 0)
                {
                    var p0 = lista[lista.Count - 1];

                    if (p0.X == p.X && p0.Y == p.Y)
                    {

                    }
                    else
                    {
                        lista.Add(p);
                    }
                }
                else
                {
                    lista.Add(p);
                }
            }

            return lista;
        }
        #region Prompts usuário
        public string PerguntaString(string Titulo, List<string> Opcoes)
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
        public double PergundaDouble(string Titulo, double padrao = 0)
        {
            PromptDoubleOptions tipo_vista = new PromptDoubleOptions(Titulo);
            tipo_vista.Message = "\n" + Titulo;
            tipo_vista.DefaultValue = padrao;

            tipo_vista.AllowNone = false;

            PromptDoubleResult selecao_tipo_vista = acDoc.Editor.GetDouble(tipo_vista);
            if (selecao_tipo_vista.Status != PromptStatus.OK) return padrao;

            return selecao_tipo_vista.Value;
        }
        public int PerguntaInteger(string Titulo, int padrao = 0)
        {
            PromptIntegerOptions tipo_vista = new PromptIntegerOptions(Titulo);
            tipo_vista.Message = "\n" + Titulo;
            tipo_vista.DefaultValue = padrao;

            tipo_vista.AllowNone = false;

            PromptIntegerResult selecao_tipo_vista = acDoc.Editor.GetInteger(tipo_vista);
            if (selecao_tipo_vista.Status != PromptStatus.OK) return padrao;

            return selecao_tipo_vista.Value;
        }
        #endregion

        #region Desenho
        public void AddPolyLine(List<Coordenada> pts, double largura, double largura_fim, System.Drawing.Color cor)
        {
            AddBarra();
            AddMensagem("\nAdicionando Polyline");
            AddMensagem(string.Join("\n", pts));
            if (pts.Count < 2)
            {
                return;
            }
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                List<Coordenada> lista = RemoverRepetidos(pts);
                // Create the rotated dimension
                using (Polyline p = new Polyline(lista.Count))
                {

                    for (int i = 0; i < lista.Count; i++)
                    {
                        if (i > 0)
                        {
                            var p0 = lista[i - 1];
                            var p1 = lista[i];
                            if (p0.X == p1.X && p0.Y == p1.Y)
                            {
                                continue;
                            }
                        }

                        p.AddVertexAt(i, new Point2d(lista[i].X, lista[i].Y), 0, largura, largura_fim);
                        //adicionar o fechamento da polyline

                        //if(i == lista.Count-1 && lista.Count>2)
                        //{
                        //    p.AddVertexAt(i, new Point2d(lista[0].X, lista[0].Y), 0, largura, largura);

                        //}
                    }
                    p.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor);
                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(p);
                    acTrans.AddNewlyCreatedDBObject(p, true);

                    // Commit the changes and dispose of the transaction
                    acTrans.Commit();
                }

            }
        }
        public void AddLinha(Coordenada inicio, Coordenada fim, string tipo, System.Drawing.Color cor)
        {


            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId,
                                                    OpenMode.ForRead) as LinetypeTable;
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(inicio.GetPoint(),
                                              fim.GetPoint()))
                {
                    if (acLineTypTbl.Has(tipo) == false)
                    {
                        // Load the Center Linetype
                        acCurDb.LoadLineTypeFile(tipo, "acad.lin");
                    }
                    if (acLineTypTbl.Has(tipo))
                    {
                        acLine.Linetype = tipo;
                        acLine.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor);
                    }
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
        }
        #endregion


        #region Cotas
        public void AddLeader(double angulo, Point3d pp0, string nome, double multiplicador = 7.5)
        {
            AddLeader(pp0, new Coordenada(pp0).Mover(angulo + 45, this.Getescala() * multiplicador).GetPoint(), nome, 2);
        }
        public void AddLeader(Point3d origem, Point3d pt2, string texto, double size)
        {
            // Get the current database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

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
                    using (MText acMText = new MText())
                    {
                        acMText.Contents = texto;
                        acMText.Location = acLdr.EndPoint;
                        acMText.TextStyleId = acCurDb.Textstyle;
                        double tam = Getescala() * size;

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
                        ErrorStatus es = attachAnnotation(acLdr.UnmanagedObject, ref textId);
                        acLdr.EvaluateLeader();
                      
                    }
                }
                // Create the MText annotation


                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }

        }
        public RotatedDimension AddCotaVertical(Coordenada inicio, Coordenada fim, string texto, Point3d posicao, bool dimtix =false, double tam = 0, bool juntar_cotas =false, bool ultima_cota =false)
        {
            RotatedDimension acRotDim;
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                //if (tipo == Tipo_Cota.RotatedDimension)
                //{
                // Create the rotated dimension


                using (acRotDim = new RotatedDimension())
                {
                    acRotDim.XLine1Point = new Point3d(inicio.X, inicio.Y, inicio.Z);
                    acRotDim.XLine2Point = new Point3d(fim.X, fim.Y, fim.Z);
                    acRotDim.DimLinePoint = new Point3d(posicao.X, posicao.Y, posicao.Z);

                    //acRotDim.TextPosition = new Point3d(posicao.X, posicao.Y, posicao.Z);
                    acRotDim.Dimtix = dimtix;


                    if (texto != "")
                    {
                        acRotDim.DimensionText = texto;
                    }
                    else if (fim.PegarIguaisY().Count > 0 && juntar_cotas /*&& sequencia>0*/ && fim.id - 1 != inicio.id && !ultima_cota)
                    {
                        acRotDim.DimensionText = (fim.PegarIguaisY().Count + 1).ToString() + "@" + Math.Round(fim.GetDist_proximaY());
                    }
                    //else if (inicio.PegarIguaisY().Count > 0 && juntar_cotas && sequencia == 0 && !ultima_cota)
                    //{
                    //    acRotDim.DimensionText = (inicio.PegarIguaisY().Count + 1).ToString() + "@" + Math.Round(inicio.GetDist_proximaY());
                    //}
                    //tam_texto
                    if (tam > 0)
                    {
                        acRotDim.Dimtxt = tam;
                    }
                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
                    acRotDim.Rotation = Math.PI / 2.0;

                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acRotDim);
                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                }
                acTrans.Commit();
            }

            return acRotDim;
        }
        public RotatedDimension AddCotaHorizontal(Coordenada inicio, Coordenada fim, string texto, Point3d posicao, bool dimtix, double tam, bool juntar_cotas, bool ultima_cota)
        {
            RotatedDimension acRotDim;
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;


                using (acRotDim = new RotatedDimension())
                {
                    acRotDim.XLine1Point = new Point3d(inicio.X, inicio.Y, inicio.Z);
                    acRotDim.XLine2Point = new Point3d(fim.X, fim.Y, fim.Z);
                    acRotDim.DimLinePoint = new Point3d(posicao.X, posicao.Y, posicao.Z);

                    acRotDim.Dimtix = dimtix;

                    //tam_texto
                    if (tam > 0)
                    {
                        acRotDim.Dimtxt = tam;

                    }
                    acRotDim.DimensionStyle = acCurDb.Dimstyle;
                    if (texto != "")
                    {
                        acRotDim.DimensionText = texto;
                    }
                    else if (inicio.PegarIguaisX().Count > 0 && juntar_cotas /*&& sequencia>0*/ && fim.id - 1 != inicio.id && !ultima_cota)
                    {
                        double distp = (inicio.PegarIguaisX().Count + 1) * inicio.Getdist_proximaX();
                        acRotDim.DimensionText = (inicio.PegarIguaisX().Count + 1).ToString() + "@" + Math.Round(inicio.Getdist_proximaX());

                    }
                    //else if (fim.PegarIguaisX().Count > 0 && juntar_cotas && sequencia == 0 && !ultima_cota)
                    //{
                    //    double distp = (fim.PegarIguaisX().Count + 1) * fim.Getdist_proximaX();
                    //    acRotDim.DimensionText = (fim.PegarIguaisX().Count + 1).ToString() + "@" + Math.Round(fim.Getdist_proximaX());
                    //}
                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acRotDim);

                    acTrans.AddNewlyCreatedDBObject(acRotDim, true);
                }

                acTrans.Commit();

            }

            return acRotDim;
        }
        public void AddCotaOrdinate(Point3d pontozero, Coordenada ponto, Point3d posicao, double tam)
        {
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            OrdinateDimension acOrdDim;




            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                // Create an ordinate dimension
                using (acOrdDim = new OrdinateDimension())
                {
                    acOrdDim.UsingXAxis = true;
                    acOrdDim.DefiningPoint = new Point3d(ponto.X, ponto.Y, ponto.Z);
                    acOrdDim.LeaderEndPoint = new Point3d(posicao.X, posicao.Y, posicao.Z);

                    acOrdDim.DimensionStyle = acCurDb.Dimstyle;
                    acOrdDim.Origin = new Point3d(pontozero.X, pontozero.Y, pontozero.Z);


                    //tam_texto
                    if (tam > 0)
                    {
                        acOrdDim.Dimtxt = tam;

                    }

                    // Add the new object to Model space and the transaction
                    acBlkTblRec.AppendEntity(acOrdDim);
                    acTrans.AddNewlyCreatedDBObject(acOrdDim, true);
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();


            }
        }

        #endregion


        #region mapeamento de objetos a serem usados


        public List<Line> Getlinhas_eixo()
        {
            return Utilidades.LinhasVerticais(this.Getlinhas()).FindAll(x =>
                x.Linetype.ToUpper() == "DASHDOT" |
                (x.Layer.ToUpper().Contains("EIXO"))
                ).ToList().GroupBy(x => Math.Round(x.StartPoint.X)).Select(x => x.First()).OrderBy(x => x.StartPoint.X).ToList();
        }

        public List<Polyline> Getpolylines_eixo()
        {
            return Utilidades.PolylinesVerticais(this.Getpolylinhas()).FindAll(x =>
                x.Linetype.ToUpper() == "DASHDOT" |
                (x.Layer.ToUpper().Contains("EIXO"))
                ).ToList().GroupBy(x => Math.Round(x.StartPoint.X)).Select(x => x.First()).OrderBy(x => x.StartPoint.X).ToList();
        }
        public List<Polyline> GetPolylinesVerticais(List<Polyline> polylines)
        {
            return Utilidades.PolylinesVerticais(polylines);
        }
        public List<Polyline> GetPolylinesHorizontais(List<Polyline> polylines)
        {
            return Utilidades.PolylinesHorizontais(polylines);
        }
        #endregion

        public Point3d Centro(Point3d p1, Point3d p2)
        {
            return new Coordenada(p1).GetCentro(p2).GetPoint();
        }
        public Point3d Mover(Point3d p, double angulo, double distancia)
        {
            return new Coordenada(p).Mover(angulo, distancia).GetPoint();
        }
        #region listas de itens selecionados
        public List<Line> Getlinhas()
        {
            return selecoes.FindAll(x => x is Line).Select(x => x as Line).ToList();
        }

        public List<Line> Getlinhas_Verticais()
        {
            return Getlinhas().FindAll(x=> Math.Round(Conexoes.Utilz.RadianosParaGraus(x.Angle)) == 90 | Math.Round(Conexoes.Utilz.RadianosParaGraus(x.Angle)) == 270);
        }
        public List<Line> Getlinhas_Horizontais()
        {
            return Getlinhas().FindAll(x => 
            Math.Round(Conexoes.Utilz.RadianosParaGraus(x.Angle)) == 0 
            | Math.Round(Conexoes.Utilz.RadianosParaGraus(x.Angle)) == 360
            |  Math.Round(Conexoes.Utilz.RadianosParaGraus(x.Angle)) == 180);
        }
        public List<Polyline> Getpolylinhas()
        {
            return selecoes.FindAll(x => x is Polyline).Select(x => x as Polyline).ToList();
        }

        public List<BlockReference> Getblocos()
        {
            return selecoes.FindAll(x => x is BlockReference).Select(x => x as BlockReference).ToList();
        }
        [Browsable(false)]
        public List<Entity> selecoes { get; set; } = new List<Entity>();

        public List<Mline> Getmultilines()
        {
            return selecoes.FindAll(x => x is Mline).Select(x => x as Mline).ToList();
        }

        public List<Xline> Getxlines()
        {
            return selecoes.FindAll(x => x is Xline).Select(x => x as Xline).ToList();
        }

        public List<Entity> Getcotas()
        {
            return selecoes.FindAll(x =>

                              x is AlignedDimension
                            | x is ArcDimension
                            | x is Dimension
                            | x is DiametricDimension
                            | x is LineAngularDimension2
                            | x is Point3AngularDimension
                            | x is OrdinateDimension
                            | x is RadialDimension
                            | x is MText
                            | x is Leader
                            | x is MLeader

            );
        }

        public List<BlockReference> Getblocostexto()
        {
            return this.Getblocos().FindAll(x => x.Name == Conexoes.Utilz.getNome(Constantes.Texto));
        }

        #endregion


        public double Getescala()
        {
            return acCurDb.Dimscale;
        }

        public void AddMensagem(string Mensagem)
        {
            acDoc.Editor.WriteMessage(Mensagem);
        }
        public void MoverBloco(BlockReference bloco, Point3d posicao, Transaction trans)
        {
            bloco.Erase(true);
            ClonarBloco(bloco, posicao);

        }

        public void ClonarBloco(BlockReference bloco, Point3d posicao)
        {
            var atributos = Utilidades.GetAtributos(bloco);

            Hashtable pp = new Hashtable();
            foreach (var cel in atributos.Celulas)
            {
                pp.Add(cel.Coluna, cel.Valor);
            }
            Utilidades.InserirBloco(acDoc, bloco.Name, posicao, bloco.ScaleFactors.X, bloco.Rotation, pp);
        }

        public void AddBarra()
        {
            AddMensagem("\n=====================================================================\n");

        }
        public void UpdateEscala(List<BlockReference> blocos)
        {
            if (Getescala() > 0)
            {
                foreach (var bl in blocos)
                {
                    bl.TransformBy(Matrix3d.Scaling(Getescala() / bl.ScaleFactors.X, bl.Position));
                }
            }

        }
        public static void LimparCotas(OpenCloseTransaction acTrans, SelectionSet acSSet)
        {
            if (acTrans == null | acSSet == null)
            {
                return;
            }
            // Step through the objects in the selection set
            foreach (SelectedObject acSSObj in acSSet)
            {
                //System.Windows.Forms.MessageBox.Show(acSSObj.ToString());
                // Check to make sure a valid SelectedObject object was returned
                if (acSSObj != null)
                {
                    // Open the selected object for write
                    Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                                                        OpenMode.ForWrite) as Entity;

                    if (acEnt != null)
                    {
                        if (acEnt is AlignedDimension)
                        {
                            var s = acEnt as AlignedDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is OrdinateDimension)
                        {
                            var s = acEnt as OrdinateDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is RadialDimension)
                        {
                            var s = acEnt as RadialDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is RotatedDimension)
                        {
                            var s = acEnt as RotatedDimension;
                            s.Erase(true);
                        }
                        //else if (acEnt is Leader)
                        //{
                        //    var s = acEnt as Leader;
                        //    s.Erase(true);
                        //}
                        else if (acEnt is MLeader)
                        {
                            var s = acEnt as MLeader;
                            s.Erase(true);
                        }
                        else if (acEnt is MText)
                        {
                            var s = acEnt as MText;
                            s.Erase(true);
                        }

                        else if (acEnt is Dimension)
                        {
                            var s = acEnt as Dimension;
                            s.Erase(true);
                        }
                    }
                }
            }
        }
        public PromptSelectionResult SelecionarObjetos(OpenCloseTransaction acTrans, bool selecionar_tudo = false)
        {
            //TypedValue[] acTypValAr = new TypedValue[1];
            //acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "CIRCLE"), 0);
            //acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "LINE"), 0);
            //acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "XLINE"), 0);
            //acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "BLOCK"), 0);
            //acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "POLYLINE"), 0);
            //SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);


            PromptSelectionResult acSSPrompt;
            if(!selecionar_tudo)
            {
                acSSPrompt = acDoc.Editor.GetSelection();
            }
            else
            {
                acSSPrompt = acDoc.Editor.SelectAll();
             
            }

        

            if (acSSPrompt.Status == PromptStatus.OK | selecionar_tudo)
            {
                SelectionSet acSSet = acSSPrompt.Value;
                selecoes.Clear();
                // Step through the objects in the selection set
                foreach (SelectedObject acSSObj in acSSet)
                {
                    try
                    {
                        if (acSSObj != null)
                        {
                            Entity acEnt = acTrans.GetObject(acSSObj.ObjectId, (selecionar_tudo? OpenMode.ForRead: OpenMode.ForWrite)) as Entity;

                            if (acEnt != null)
                            {
                                selecoes.Add(acEnt);
                            }
                        }
                    }
                    catch (System.Exception)
                    {

                    }
                   
                }

            }
            var tps = selecoes.GroupBy(x => x.GetType().ToString()).Select(x => x.First()).ToList();

            AddMensagem("\nTipos de objetos selecionados:\n" + string.Join("\n", tps));
            return acSSPrompt;
        }
        public ClasseBase SelecionarTudo()
        {
            ClasseBase p = new ClasseBase();
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                p.SelecionarObjetos(acTrans, true);
            }
                return p;
        }

        public double Angulo(Point3d p1, Point3d p2)
        {
            return new Coordenada(p1).Angulo(p2);
        }

        public ClasseBase()
        {
            SetUCSParaWorld();
        }
    }
}
