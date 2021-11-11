using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
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
using static Ferramentas_DLM.CAD;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;

namespace Ferramentas_DLM
{
    [Serializable]
    public class CADBase
    {
        public void ApagarCotas()
        {
            var sel = SelecionarObjetos(Tipo_Selecao.Dimensoes);
            List<Entity> remover = new List<Entity>();
            foreach (var acEnt in selecoes)
            {
                if (
                          acEnt is AlignedDimension |
                          acEnt is OrdinateDimension |
                          acEnt is RadialDimension |
                          acEnt is RotatedDimension |
                          acEnt is MLeader |
                          acEnt is Leader |
                          acEnt is MText |
                          acEnt is DBText |
                          acEnt is Dimension
                          )
                {
                    remover.Add(acEnt);
                }
            }

            Ut.Apagar(remover);


        }
        public  void SetViewport(bool block = true, string layer = "MV")
        {

            Ut.IrLayout();
            FLayer.Criar(layer, System.Drawing.Color.Gray);
            FLayer.Set("0");
            var view = Ut.GetViewports(layer);
            editor.Command("mview", "lock", block ? "ON" : "OFF", "all", "");
            editor.Command("layer", block ? "off":"on", layer, "");
            editor.Command("pspace", "");
            editor.Command("zoom", "e","");

        }
        public bool E_Tecnometal3D(bool mensagem = true)
        {
            if (!this.Pasta.ToUpper().Replace(@"\", "").EndsWith(@".S&G"))
            {
                if (mensagem)
                {
                    Conexoes.Utilz.Alerta($"Não é possível rodar esse comando fora de pastas de pedidos (.S&G)" +
                   $"\nPasta atual: {this.Pasta}");
                }

                return false;
            }
            else
            {
                return true;
            }
        }
        public bool E_Tecnometal(bool mensagem = true)
        {
            if (!this.Pasta.ToUpper().Replace(@"\", "").EndsWith(".TEC"))
            {
                if (mensagem)
                {
                    Conexoes.Utilz.Alerta($"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                   $"\nPasta atual: {this.Pasta}");
                }

                return false;
            }
            else
            {
                return true;
            }
        }
        public void CriarLayersPadrao()
        {
            string nome = "LAYERS_PADRAO";
            
            Blocos.Inserir(CAD.acDoc, nome, new Point3d(), 0.001, 0, new Hashtable());
            Ut.Apagar(Blocos.GetBlocosPrancha(nome).Select(x=> x as Entity).ToList());
        }
        public List<Conexoes.Arquivo> SelecionarDWGs(bool dxfs_tecnometal = false)
        {
            var arqs = Conexoes.Utilz.GetArquivos(this.Pasta, "*.dwg").Select(x=>new Conexoes.Arquivo(x)).ToList();

            var selecao = arqs.FindAll(x => x.Nome.ToUpper().Contains("-FA-") && x.Nome.Length>8);
            var ultimas_revs = selecao.GroupBy(x => x.Nome.Substring(0, x.Nome.Length - 3)).Select(x => x.ToList().OrderByDescending(y => y.Nome)).Select(x => x.First()).ToList();
            selecao = ultimas_revs;

            var resto = arqs.FindAll(x => selecao.Find(y=> y.Nome == x.Nome) == null);
            if(dxfs_tecnometal && E_Tecnometal(false))
            {
                var etapa = new Conexoes.SubEtapaTecnoMetal(this.Pasta);

                selecao.AddRange(Conexoes.Utilz.GetArquivos(etapa.PastaCAM, "*.DXF").Select(x=>new Conexoes.Arquivo(x)));
            }
            var arquivos = Conexoes.Utilz.Selecao.SelecionarObjetos(resto, selecao, "Selecione as pranchas.");


            return arquivos;
        }



        private List<Entity> _entities_blocos { get; set; }
        public List<Entity> GetEntitiesdeBlocos()
        {
            if(_entities_blocos==null)
            {
                _entities_blocos = new List<Entity>();
                foreach (var bl in this.GetBlocos())
                {
                    _entities_blocos.AddRange(Blocos.GetEntities(bl));
                }
            }

            return _entities_blocos;
        }

        [Category("Mapeamento")]
        [DisplayName("Buscar objetos em Blocos")]
        public bool BuscarEmBlocos { get; set; } = true;

        [Category("Configuração")]
        [DisplayName("Layer Blocos")]
        public string LayerBlocos { get; set; } = "BLOCOS";

        [Category("RM")]
        [DisplayName("Família Purlin")]
        public string RM_Familia_Purlin { get; set; } = "TERCA";
        [Category("RM")]
        [DisplayName("Família FB")]
        public string RM_Familia_FB { get; set; } = "FLANGE BRACE";

        [Category("RM")]
        [DisplayName("Família Tirante")]
        public string RM_Familia_Tirante { get; set; } = "TIRANTE";

        [Category("RM")]
        [DisplayName("Família Corrente")]
        public string RM_Familia_Corrente { get; set; } = "DLD";
        [Category("RM")]
        [DisplayName("Família Corrente Suporte")]
        public string RM_Familia_Corrente_Suporte { get; set; } = "SUPORTE CORRENTE";
        [Category("RM")]
        [DisplayName("Família Purlin Suporte")]
        public string RM_Familia_Purlin_Suporte { get; set; } = "SUPORTE PURLIN";
        [Category("RM")]
        [DisplayName("Família Purlin Tirante")]
        public string RM_Familia_Tirante_Suporte { get; set; } = "SUPORTE TIRANTE";

        [Category("Configuração")]
        [DisplayName("Layer Eixos")]
        public string LayerEixos { get; set; } = "EIXO";

        [Category("Configuração")]
        [DisplayName("Layer Eixos Comprimento Mínimo")]
        public double LayerEixosCompMin { get; set; } = 2500;

        [Category("Configuração")]
        [DisplayName("Bloco Eixos")]
        public string BlocoEixos { get; set; } = "EIXO";
        [Category("Configuração")]
        [DisplayName("Descrição FB")]
        public string DescFB { get; set; } = "FLANGE BRACE";

        [Category("Eixos")]
        [DisplayName("Distancia Mínima")]
        public double DistanciaMinimaEixos { get; set; } = 1000;

        [Category("Informações")]
        [DisplayName("Pasta Arquivo")]
        public string Pasta
        {
            get
            {
                var pasta = Conexoes.Utilz.getPasta(acDoc.Name).ToUpper();

                if (!Directory.Exists(pasta))
                {
                    pasta = Conexoes.Utilz.RaizAppData();
                }
                return pasta;
            }
        }

        [Category("Informações")]
        [DisplayName("Nome Arquivo")]
        public string Nome
        {
            get
            {
                return Conexoes.Utilz.getNome(acDoc.Name).ToUpper().Replace(".DWG","");
            }
        }

        [Category("Informações")]
        [DisplayName("Endereço")]
        public string Endereco
        {
            get
            {
                return acDoc.Name;
            }
        }

        [Browsable(false)]
        public List<Entity> selecoes { get; set; } = new List<Entity>();

       
        public void SetUCSParaWorld()
        {
            acDoc.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;
            acDoc.Editor.Regen();
        }

        public void Comando(params object[] comando)
        {
            Extensoes.Command(acDoc.Editor, comando);
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
            
            // Start a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl= acTrans.GetObject(acCurDb.BlockTableId,OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
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
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId,OpenMode.ForRead) as LinetypeTable;
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(inicio.GetPoint3D(),
                                              fim.GetPoint3D()))
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
        public RotatedDimension AddCotaVertical(Coordenada inicio, Coordenada fim, string texto, Point3d posicao, bool dimtix =false, double tam = 0, bool juntar_cotas =false, bool ultima_cota =false)
        {
            RotatedDimension acRotDim;
            // Get the current database
            // Start a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,  OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

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
            // Start a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;


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
            OrdinateDimension acOrdDim;

            // Start a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
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

        public List<CADLine> GetLinhas_Eixos()
        {
            return GetLinhas().FindAll(x => x.Comprimento >= this.LayerEixosCompMin && x.Layer.ToUpper().Contains(this.LayerEixos) && (x.Linetype.ToUpper() == Constantes.LineType_Eixos | x.Linetype.ToUpper() == Constantes.LineType_ByLayer));
        }

        public List<Polyline> GetPolyLines_Verticais(List<Polyline> polylines)
        {
            return Ut.PolylinesVerticais(polylines);
        }
        public List<Polyline> GetPolyLines_Horizontais(List<Polyline> polylines)
        {
            return Ut.PolylinesHorizontais(polylines);
        }
        #endregion


        #region listas de itens selecionados
        public List<CADLine> GetLinhas()
        {
            return selecoes.FindAll(x => x is Line).Select(x => x as Line).ToList().Select(x=>new CADLine(x)).ToList();
        }
        public List<CADLine> GetLinhas_Verticais()
        {
            
            return GetLinhas().FindAll(x=>x.Sentido == Sentido.Vertical).OrderBy(x => x.StartPoint.X).ToList();
        }
        public List<CADLine> GetLinhas_Horizontais()
        {
            return GetLinhas().FindAll(x => x.Sentido == Sentido.Horizontal).OrderBy(x => x.StartPoint.X).ToList();
        }
        public List<Polyline> GetPolyLines()
        {
            return selecoes.FindAll(x => x is Polyline).Select(x => x as Polyline).ToList();
        }
        public List<MText> GetMtexts()
        {
            return selecoes.FindAll(x => x is MText).Select(x => x as MText).ToList();
        }
        public List<DBText> GetTexts()
        {
            return selecoes.FindAll(x => x is Autodesk.AutoCAD.DatabaseServices.DBText).Select(x => x as DBText).ToList();
        }
        public List<BlockReference> GetBlocos()
        {
            return selecoes.FindAll(x => x is BlockReference).Select(x => x as BlockReference).ToList();
        }

        public List<Mline> GetMultilines()
        {
            List<Mline> lista = new List<Mline>();
            lista.AddRange(selecoes.FindAll(x => x is Mline).Select(x => x as Mline).ToList());

            if (BuscarEmBlocos)
            {
                lista.AddRange(this.GetEntitiesdeBlocos().FindAll(x => x is Mline).Select(x => x as Mline));
            }

            return lista;
        }
        public List<Xline> GetXlines()
        {
            return selecoes.FindAll(x => x is Xline).Select(x => x as Xline).ToList();
        }
        public List<Entity> GetCotas()
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

        private List<BlocoTag> _blocos_eixo { get; set; }
        public List<BlocoTag> GetBlocos_Eixos(bool update = false)
        {
            /*pega blocos dinâmicos*/
           if(_blocos_eixo==null | update)
            {
                _blocos_eixo = this.GetBlocos().FindAll(x => Blocos.GetNome(x).ToUpper().Contains(this.BlocoEixos)).Select(x => new BlocoTag(x)).ToList();
            }

            return _blocos_eixo;
        }
        public List<BlocoTag> GetBlocos_Nivel()
        {
            /*pega blocos dinâmicos*/
            return this.GetBlocos().FindAll(x => Blocos.GetNome(x).ToUpper().Contains("NIVEL") | Blocos.GetNome(x).ToUpper().Contains("NÍVEL")).Select(x=> new BlocoTag(x)).ToList();
        }

        public List<PCQuantificar> GetBlocos_IndicacaoPecas()
        {
            List<PCQuantificar> pcs = new List<PCQuantificar>();
            var blocos = this.GetBlocos().FindAll(x => x.Name.ToUpper().StartsWith(Constantes.PC_Quantificar)).GroupBy(x => x.Name);


            foreach(var s in blocos)
            {
                PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Bloco, s.Key.ToUpper(), "", s.Key.ToUpper(), s.ToList().Select(x => Ferramentas_DLM.Atributos.GetBlocoTag(x)).ToList());
                if (npc.Nome.StartsWith(Constantes.PC_Quantificar))
                {
                    var blcs = npc.Agrupar(new List<string> { Constantes.ATT_Codigo, Constantes.ATT_N }, npc.Nome_Bloco);
                    foreach (var bl in blcs)
                    {
                        bl.SetDescPorAtributo(Constantes.ATT_Descricao);
                        bl.SetNumeroPorAtributo(Constantes.ATT_N);
                        bl.SetDestinoPorAtributo(Constantes.ATT_Destino);
                        bl.SetQtdPorAtributo(Constantes.ATT_Quantidade);
                        bl.SetIdPorAtributo(Constantes.ATT_id);
                        bl.SetFamiliaPorAtributo(Constantes.ATT_Familia);
                    }
                    pcs.AddRange(blcs);
                }
            }

            return pcs;
    
        }

        #endregion
        public double GetEscala()
        {
            return acCurDb.Dimscale;
        }
        public void SetEscala(double valor)
        {
            if(valor>0)
            {
                using (DocumentLock acLckDoc = acDoc.LockDocument())
                {
                    acCurDb.Dimscale = valor;
                }
            }
        }
        public void AddMensagem(string Mensagem)
        {
            acDoc.Editor.WriteMessage(Mensagem);
        }

        public void AddBarra()
        {
            AddMensagem("\n=====================================================================\n");

        }
        private List<string> _layers { get; set; }

        public List<string> GetLayers()
        {
            if (_layers == null)
            {
                _layers = FLayer.Get();
            }
            return _layers;
        }

        public PromptSelectionResult SelecionarObjetos(Tipo_Selecao tipo = Tipo_Selecao.Tudo)
        {
            PromptSelectionOptions pp = new PromptSelectionOptions();
            pp.RejectObjectsOnLockedLayers = true;
            pp.RejectPaperspaceViewport = true;
            pp.RejectObjectsFromNonCurrentSpace = true;
            pp.AllowDuplicates = false;
          
            //pp.MessageForAdding = "Item adicionado à seleção";
            //pp.MessageForRemoval = "Item removido da seleção";



            var lista_filtro = new List<TypedValue>();

            switch (tipo)
            {
                case Tipo_Selecao.Tudo:
                    lista_filtro.Add(new TypedValue(0, "LINE,POLYLINE,LWPOLYLINE,TEXT,MTEXT,DIMENSION,LEADER,INSERT,MLINE"));

                    break;
                case Tipo_Selecao.Blocos:
                    lista_filtro.Add(new TypedValue(0, "INSERT"));
                    break;
                case Tipo_Selecao.Textos:
                    lista_filtro.Add(new TypedValue(0, "TEXT,MTEXT,LEADER,MLEADER"));
                    break;
                case Tipo_Selecao.Blocos_Textos:
                    lista_filtro.Add(new TypedValue(0, "INSERT,TEXT,MTEXT,LEADER"));
                    break;
                case Tipo_Selecao.Dimensoes:
                    lista_filtro.Add(new TypedValue(0, "DIMENSION,TEXT,MTEXT,LEADER,MLEADER"));
                    break;
                case Tipo_Selecao.Polyline:
                    lista_filtro.Add(new TypedValue(0, "POLYLINE,LWPOLYLINE"));
                    break;
            }


            SelectionFilter filtro =   new SelectionFilter(lista_filtro.ToArray());
            PromptSelectionResult acSSPrompt = null;

            if (lista_filtro.Count>0)
            {
                acSSPrompt = acDoc.Editor.GetSelection(pp, filtro);
            }
            else
            {
                acSSPrompt = acDoc.Editor.GetSelection(pp);
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {


                if (acSSPrompt.Status == PromptStatus.OK)
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
                                Entity acEnt = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as Entity;

                                if (acEnt != null)
                                {
                                    selecoes.Add(acEnt);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Conexoes.Utilz.Alerta($"{ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
            }

            return acSSPrompt;
        }
        public void AbrePasta()
        {
            if(Directory.Exists(this.Pasta))
            {
                Conexoes.Utilz.Abrir(this.Pasta);
            }
        }



        public void RenomeiaBlocos()
        {
          var sel =  SelecionarObjetos(Tipo_Selecao.Blocos);

            if(sel.Status == PromptStatus.OK)
            {
                var blocos = this.GetBlocos();

                var nomes = blocos.GroupBy(x => x.Name);
                foreach(var nome in nomes)
                {
                    var novo_nome = Conexoes.Utilz.Prompt($"Digite o novo nome para o bloco \n[{nome.Key}]");
                    if(novo_nome!=null && novo_nome.Length>0)
                    {
                        novo_nome = novo_nome.Replace(" ", "_").ToUpper();
                        if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja renomear o bloco \n[{nome.Key}] para [{novo_nome}]"))
                        {
                            Blocos.Renomear(nome.Key, novo_nome);
                        }

                    }
                }


            }
        }

        public CADBase()
        {
            Constantes.VerificarVersao();
            SetUCSParaWorld();
            GetLayers();
            Multiline.GetMLStyles();
        }
    }
}
