using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.cad;
using DLM.cam;
using DLM.desenho;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static DLM.cad.CAD;

namespace DLM.cad
{
    [Serializable]
    public class CADBase
    {
        private List<Entity> _Entities_Blocos { get; set; }
        private List<CADLine> _Linhas { get; set; }
        private List<string> _Layers { get; set; }
        private List<Furo> _Furos { get; set; }
        private List<BlockAttributes> _Atributos_Eixos { get; set; }
        private List<PolyInfo> _Polies { get; set; }

        public void ApagarCotas()
        {
            var sel = SelecionarObjetos(Tipo_Selecao.Dimensoes);
            List<Entity> remover = new List<Entity>();
            foreach (var acEnt in Selecoes)
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
            acDoc.Apagar(remover);
        }
        public void SetViewport(bool block = true, string layer = "MV")
        {
            acDoc.IrLayout();
            FLayer.Criar(layer, System.Drawing.Color.Gray);
            FLayer.Set("0");
            var view = Ut.GetViewports(layer);
            acDoc.Comando("mview", "lock", block ? "ON" : "OFF", "all", "");
            acDoc.Comando("layer", block ? "off" : "on", layer, "");
            acDoc.Comando("pspace", "");
            Ut.ZoomExtend();
        }
        public bool E_Tecnometal3D(bool mensagem = true)
        {
            if (!this.Pasta.ToUpper().Replace(@"\", "").EndsW($@".{Cfg.Init.EXT_Pedido}"))
            {
                if (mensagem)
                {
                    $"Não é possível rodar esse comando fora de pastas de pedidos (.{Cfg.Init.EXT_Pedido})\nPasta atual: {this.Pasta}".Alerta();
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
            if (!this.Pasta.ToUpper().Replace(@"\", "").EndsW($".{Cfg.Init.EXT_Etapa}"))
            {
                if (mensagem)
                {
                    $"Não é possível rodar esse comando fora de pastas de etapas (.{Cfg.Init.EXT_Etapa})\nPasta atual: {this.Pasta}".Alerta();
                }

                return false;
            }
            else
            {
                return true;
            }
        }
        public void AjustarLayers()
        {
            CriarLayersPadrao();

            this.SelecionarTudo();
            var objetos = this.Selecoes;

            using (var acTrans = acCurDb.acTrans())
            {
                var lista = this.Selecoes;
                foreach (var obj in lista)
                {
                    obj.AjustarLayer(acTrans);
                }
                foreach (var obj in lista.GetBlockTableRecordEntities(acTrans))
                {
                    obj.AjustarLayer(acTrans);
                }

                acTrans.Commit();
            }
        }



        public void CriarLayersPadrao()
        {
            string nome = "LAYERS_PADRAO";

            Blocos.Inserir(acDoc, nome, new P3d(), 0.001, 0, new db.Linha());
            acDoc.Apagar(Blocos.GetBlocosPrancha(nome).Select(x => x as Entity).ToList());
        }
        public List<Conexoes.Arquivo> SelecionarDWGs(bool dxfs_tecnometal = false)
        {
            var arqs = this.Pasta.GetArquivos("*.dwg");

            var selecao = arqs.FindAll(x => x.Nome.Contem(Cfg.Init.DWG_FAB_FILTRO) && x.Nome.Length > 8);
            var ultimas_revs = selecao.GroupBy(x => x.Nome.Substring(0, x.Nome.Length - 3)).Select(x => x.ToList().OrderByDescending(y => y.Nome)).Select(x => x.First()).ToList();
            selecao = ultimas_revs;

            var resto = arqs.FindAll(x => selecao.Find(y => y.Nome == x.Nome) == null);
            if (dxfs_tecnometal && E_Tecnometal(false))
            {
                var etapa = new Conexoes.SubEtapaTecnoMetal(this.Pasta);

                selecao.AddRange(etapa.PastaCAM_Pedido.GetArquivos("*.DXF"));
                selecao.AddRange(etapa.PastaCAM_Etapa.GetArquivos("*.DXF"));
            }
            var arquivos = resto.ListaSelecionarVarios(selecao, "Selecione as pranchas.");


            return arquivos;
        }





        public List<BlocoFuro> GetFurosVista()
        {
            return this.GetBlocos_Furos_Vista().Select(x => new BlocoFuro(x)).ToList();
        }
        public List<BlockReference> GetBlocos_Furos_Vista()
        {
            var blocos = Selecoes.Filter<BlockReference>();
            return blocos.FindAll(x =>
                 x.Name.ToUpper() == "M8"
                | x.Name.ToUpper() == "M10"
                | x.Name.ToUpper() == "M12"
                | x.Name.ToUpper() == "M14"
                | x.Name.ToUpper() == "M14_"
                | x.Name.ToUpper() == "M16"
                | x.Name.ToUpper() == "M18"
                | x.Name.ToUpper() == "M20"
                | x.Name.ToUpper() == "M22"
                | x.Name.ToUpper() == "M24"
                | x.Name.ToUpper() == "M27"
                | x.Name.ToUpper() == "M30"
                | x.Name.ToUpper() == "M33"
                | x.Name.ToUpper() == "M36"
                | x.Name.ToUpper() == "M39"
                | x.Name.ToUpper() == "M42"
                | x.Name.ToUpper() == "M45"
                | x.Name.ToUpper() == "M48"
                | x.Name.ToUpper() == "M52"
                | x.Name.ToUpper() == "M56"
                | x.Name.ToUpper() == "M60"
                | x.Name.ToUpper() == "M64"
                | x.Name.ToUpper() == "M68"
                | x.Name.ToUpper() == "M72"
                | x.Name.ToUpper() == "M76"
                | x.Name.ToUpper() == "M80"

                | x.Name.ToUpper() == "3D_INFOHOLE1"
                | x.Name.ToUpper() == "MA"
                );
        }
        public List<Entity> GetEntitiesdeBlocos()
        {
            if (_Entities_Blocos == null)
            {
                _Entities_Blocos = new List<Entity>();
                foreach (var block in Selecoes.Filter<BlockReference>())
                {
                    _Entities_Blocos.AddRange(block.GetEntities());
                }
            }

            return _Entities_Blocos;
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
                var pasta = Utilz.getPasta(acDoc.Name).ToUpper();

                if (!Directory.Exists(pasta))
                {
                    pasta = Cfg.Init.DIR_APPDATA;
                }
                return pasta;
            }
        }

        public string PastaCAM
        {
            get
            {
                string destino = this.Pasta;
                destino = destino.GetSubPasta(Cfg.Init.EXT_CAM);

                return destino;
            }
        }

        [Category("Informações")]
        [DisplayName("Nome Arquivo")]
        public string Nome
        {
            get
            {
                return acDoc.Name.getNome().ToUpper().Replace(".DWG", "");
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
        public List<Entity> Selecoes { get; private set; } = new List<Entity>();

        public void SetUCSParaWorld()
        {
            acDoc.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;
            editor.Regen();
        }



        #region Prompts usuário

        #endregion

        #region Desenho
        public void AddPolyLine(List<P3d> pontos, bool fechar = true)
        {
            AddPolyLine(pontos, 0, 0, System.Drawing.Color.White, fechar);
        }
        public void AddPolyLine(List<P3d> pontos, double largura, double largura_fim, System.Drawing.Color cor, bool fechar = true)
        {
            AddBarra();
            AddMensagem("\nAdicionando Polyline");
            AddMensagem(string.Join("\n", pontos));
            if (pontos.Count < 2)
            {
                return;
            }

            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                List<P3d> lista = pontos.RemoverRepetidos();
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
                    acTrans.AddNewlyCreatedDBObject(p, fechar);

                    // Commit the changes and dispose of the transaction
                    acTrans.Commit();
                }

            }
        }
        public void AddLinha(P3d inicio, P3d fim, string tipo = null, System.Drawing.Color? cor = null)
        {


            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (var acLine = new Line(inicio.GetPoint3dCad(), fim.GetPoint3dCad()))
                {
                    if (tipo != null)
                    {
                        if (acLineTypTbl.Has(tipo) == false)
                        {
                            // Load the Center Linetype
                            acCurDb.LoadLineTypeFile(tipo, "acad.lin");
                        }
                        if (acLineTypTbl.Has(tipo))
                        {
                            acLine.Linetype = tipo;
                        }
                    }

                    if (cor != null)
                    {
                        acLine.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor.Value);
                    }
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
        }


        public void AddXline(P3d origem, string tipo = null, System.Drawing.Color? cor = null, double angulo = 90)
        {

            var p2 = origem.Mover(angulo, 10);
            using (var acTrans = acCurDb.acTransST())
            {
                var acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                // Open the Block table for read
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                using (var nXLine = new Xline())
                {
                    nXLine.BasePoint = origem.GetPoint3dCad();
                    nXLine.SecondPoint = p2.GetPoint3dCad();

                    if (tipo != null)
                    {
                        if (acLineTypTbl.Has(tipo) == false)
                        {
                            // Load the Center Linetype
                            acCurDb.LoadLineTypeFile(tipo, "acad.lin");
                        }
                        if (acLineTypTbl.Has(tipo))
                        {
                            nXLine.Linetype = tipo;
                        }
                    }

                    if (cor != null)
                    {
                        nXLine.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor.Value);
                    }
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(nXLine);
                    acTrans.AddNewlyCreatedDBObject(nXLine, true);



                }
                acTrans.Commit();
            }
        }

        #endregion


        #region Cotas
        public void AddMtext(P3d origem, string texto, double angulo = 0, double tam = 0, System.Drawing.Color? cor = null)
        {
            // Starts a new transaction with the Transaction Manager
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Block table record for read
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                // Open the Block table record Model space for write
                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                /* Creates a new MText object and assigns it a location,
                text value and text style */
                var objText = new MText();
                objText.SetDatabaseDefaults();
                objText.Location = origem.GetPoint3dCad();

                objText.Contents = texto;
                objText.Rotation = angulo.GrausParaRadianos();
                objText.TextStyleId = acCurDb.Textstyle;

                if (cor != null)
                {
                    objText.Color = Autodesk.AutoCAD.Colors.Color.FromColor(cor.Value);
                }

                if (tam > 0)
                {
                    objText.TextHeight = tam;
                }

                acBlkTblRec.AppendEntity(objText);
                acTrans.AddNewlyCreatedDBObject(objText, true);


                acTrans.Commit();
            }
        }
        public RotatedDimension AddCotaVertical(P3d inicio, P3d fim, string texto, P3d posicao, bool dimtix = false, double tam = 0, bool juntar_cotas = false, bool ultima_cota = false)
        {
            RotatedDimension acRotDim;
            // Get the current database
            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

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
        public RotatedDimension AddCotaHorizontal(P3d inicio, P3d fim, string texto, P3d posicao, bool dimtix = false, double tam = 0, bool juntar_cotas = false, bool ultima_cota = false)
        {
            RotatedDimension acRotDim;
            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Block table for read
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


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
        public void AddCotaOrdinate(P3d pontozero, P3d ponto, P3d posicao, double tam)
        {
            OrdinateDimension acOrdDim;

            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
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
            return GetCADLines().FindAll(x => x.Comprimento >= this.LayerEixosCompMin && x.Layer.ToUpper().Contem(this.LayerEixos) && (x.Linetype.ToUpper() == Cfg.Init.CAD_LineType_Eixos | x.Linetype.ToUpper() == Cfg.Init.CAD_LineType_ByLayer));
        }
        #endregion

        #region listas de itens selecionados
        public List<CADLine> GetCADLines()
        {
            if (_Linhas == null)
            {
                _Linhas = new List<CADLine>();
                _Linhas.AddRange(Selecoes.FindAll(x => x is Line).Select(x => x as Line).ToList().Select(x => new CADLine(x)).ToList());
            }
            return _Linhas;
        }
        public List<CADLine> GetLinhas_Verticais()
        {
            return GetCADLines().FindAll(x => x.Sentido == Sentido.Vertical).OrderBy(x => x.P1.X).ToList();
        }
        public List<CADLine> GetLinhas_Horizontais()
        {
            return GetCADLines().FindAll(x => x.Sentido == Sentido.Horizontal).OrderBy(x => x.P1.X).ToList();
        }


        public List<PolyInfo> GetPolies()
        {
            if (_Polies == null)
            {
                _Polies = this.Selecoes.Filter<Polyline>().OrderByDescending(x => x.Length).ToList().Select(x => new PolyInfo(x)).ToList();
            }
            return _Polies;
        }

        public void CriarPoliLyneSelecao()
        {
            this.SelecionarObjetos(CAD_TYPE.LINE, CAD_TYPE.POLYLINE);
            var lines = this.Selecoes.Filter<Line>().ToList();
            var polis = this.Selecoes.Filter<Polyline>().ToList();



            var ordenar = lines.GetLinhasConectadas();
            ordenar.CriarPolyLine();
        }



        public List<Furo> GetFurosSelecao()
        {
            if (_Furos == null)
            {
                _Furos = new List<Furo>();

                var furos_blocos = this.GetFurosVista();
                var furos_circles = Selecoes.Filter<Circle>();
                var furos_hatches = Selecoes.Filter<Hatch>();
                var polis = this.GetPolies().FindAll(x => x.Polyline.Closed);
                var furos_polis = polis.FindAll(x => x.Arcos.Count == 2);

                var furos_hatch_bloco = Selecoes.Filter<BlockReference>();
                //hachuras tekla em escala
                foreach (var blk in furos_hatch_bloco)
                {


                    var entH = blk.GetEntities().Filter<Hatch>();
                    if (entH.Count > 0)
                    {
                        var fr = entH.First().GetFuro();
                        if (fr != null)
                        {
                            _Furos.Add(fr);
                        }
                    }
                    else
                    {
                        var furo = blk.GetFuro();
                        if (furo != null)
                        {
                            _Furos.Add(furo);
                        }
                    }
                }

                foreach (var furo in furos_blocos.FindAll(x => x.Block.Name != "MA"))
                {
                    var p3d = furo.Block.Position.P3d();
                    _Furos.Add(new cam.Furo(p3d.X, p3d.Y, furo.Diametro, furo.Oblongo));
                }
                foreach (var hatch in furos_hatches)
                {
                    var furo = hatch.GetFuro();
                    if (furo != null)
                    {
                        _Furos.Add(furo);
                    }
                }
                foreach (var furo in furos_circles)
                {
                    var p0 = furo.Center.P3d();
                    _Furos.Add(new cam.Furo(p0.X, p0.Y, furo.Diameter));
                }
                foreach (var furo_poli in furos_polis)
                {
                    var arc0 = furo_poli.Arcos[0];
                    var arc1 = furo_poli.Arcos[1];
                    var pa = new P3d(arc0.Center.X, arc0.Center.Y);
                    var pb = new P3d(arc1.Center.X, arc1.Center.Y);

                    var diam = (arc0.Radius * 2).Round(0);
                    var angulo = pa.GetAngulo(pb).Round(0);
                    var dist = pa.Distancia(pb).Abs().Round(0);
                    var d0 = dist - diam;
                    var p0 = pa.Centro(pb);
                    _Furos.Add(new cam.Furo(p0.X, p0.Y, diam, dist, angulo));
                }
            }

            //remove furos sobrepostos
            _Furos = _Furos.GroupBy(x => x.Origem.ToString2D()).Select(x => x.First()).ToList();
            return _Furos;
        }

        private List<MlClass> _multiLines { get; set; }
        public List<MlClass> GetMultiLines()
        {
            if (_multiLines == null)
            {
                _multiLines = new List<MlClass>();

                var mlines = this.GetMls().Select(x => new CADMline(x, Tipo_Multiline.Ignorar)).ToList();

                if (mlines.Count == 0)
                {
                    this.AddMensagem("Nenhuma multiline encontrada na seleção");
                    return new List<MlClass>();
                }

                _multiLines.AddRange(mlines.GroupBy(x => x.GetStyle().Name).Select(x => new MlClass(x.Key, x.ToList())).ToList());
                foreach (var ml in _multiLines)
                {
                    var ig = MlClass.GetSetup().Find(x => x.Nome == ml.Nome);
                    if (ig != null)
                    {
                        ml.Tipo = ig.Tipo;
                    }
                }
            }

            return _multiLines;
        }
        public List<Mline> GetMls()
        {
            var lista = new List<Mline>();
            lista.AddRange(Selecoes.Filter<Mline>());

            if (BuscarEmBlocos)
            {
                lista.AddRange(this.GetEntitiesdeBlocos().Filter<Mline>());
            }

            return lista;
        }
        public List<Line> GetLines()
        {
            var lista = new List<Line>();
            lista.AddRange(Selecoes.Filter<Line>());

            if (BuscarEmBlocos)
            {
                lista.AddRange(this.GetEntitiesdeBlocos().Filter<Line>());
            }

            return lista;
        }


        public List<BlockAttributes> GetAtributosEixos(bool update = false)
        {
            /*pega blocos dinâmicos*/
            if (_Atributos_Eixos == null | update)
            {
                var blks = Selecoes.Filter<BlockReference>();
                var lbksnms = blks.GroupBy(x => Blocos.GetNome(x).ToUpper()).ToList();
                var eixos = lbksnms.FindAll(x => x.Key.Contem(this.BlocoEixos)).ToList().SelectMany(x => x.ToList()).ToList();

                _Atributos_Eixos = eixos.Select(x => x.GetAttributes()).ToList();
            }

            return _Atributos_Eixos;
        }
        public List<BlockAttributes> GetAtributosNivel()
        {
            /*pega blocos dinâmicos*/
            return Selecoes.Filter<BlockReference>().FindAll(x => Blocos.GetNome(x).ToUpper().Contem("NIVEL", "NÍVEL")).Select(x => x.GetAttributes()).ToList();
        }
        public List<PCQuantificar> GetBlocos_IndicacaoPecas()
        {
            List<PCQuantificar> pcs = new List<PCQuantificar>();
            var blocos = Selecoes.Filter<BlockReference>().FindAll(x => x.Name.ToUpper().StartsW(Cfg.Init.CAD_PC_Quantificar)).GroupBy(x => x.Name);


            foreach (var s in blocos)
            {
                PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Bloco, s.Key.ToUpper(), "", s.Key.ToUpper(), s.ToList().Select(x => x.GetAttributes()).ToList());
                if (npc.Nome.StartsW(Cfg.Init.CAD_PC_Quantificar))
                {
                    var blcs = npc.Agrupar(new List<string> { Cfg.Init.CAD_ATT_Codigo, Cfg.Init.CAD_ATT_N }, npc.Nome_Bloco);
                    foreach (var bl in blcs)
                    {
                        bl.SetDescPorAtributo(Cfg.Init.CAD_ATT_Descricao);
                        bl.SetNumeroPorAtributo(Cfg.Init.CAD_ATT_N);
                        bl.SetDestinoPorAtributo(Cfg.Init.CAD_ATT_Destino);
                        bl.SetQtdPorAtributo(Cfg.Init.CAD_ATT_Quantidade);
                        bl.SetIdPorAtributo(Cfg.Init.CAD_ATT_id);
                        bl.SetFamiliaPorAtributo(Cfg.Init.CAD_ATT_Familia);
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
            if (valor > 0)
            {
                using (var docLock = acDoc.LockDocument())
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

        public List<string> GetLayers()
        {
            if (_Layers == null)
            {
                _Layers = FLayer.Listar();
            }
            return _Layers;
        }

        public PromptSelectionResult SelecionarObjetos(Tipo_Selecao tipo = Tipo_Selecao.Tudo)
        {
            var lista_filtro = new List<TypedValue>();

            switch (tipo)
            {
                case Tipo_Selecao.Blocos:
                    return SelecionarObjetos(CAD_TYPE.INSERT);
                case Tipo_Selecao.PolyLine_Blocos:
                    return SelecionarObjetos(CAD_TYPE.INSERT, CAD_TYPE.POLYLINE, CAD_TYPE.LWPOLYLINE);
                case Tipo_Selecao.Textos:
                    return SelecionarObjetos(CAD_TYPE.TEXT, CAD_TYPE.MTEXT, CAD_TYPE.LEADER, CAD_TYPE.MLEADER);
                case Tipo_Selecao.Blocos_Textos:
                    return SelecionarObjetos(CAD_TYPE.INSERT, CAD_TYPE.TEXT, CAD_TYPE.MTEXT, CAD_TYPE.LEADER);
                case Tipo_Selecao.Dimensoes:
                    return SelecionarObjetos(CAD_TYPE.DIMENSION, CAD_TYPE.TEXT, CAD_TYPE.MTEXT, CAD_TYPE.LEADER, CAD_TYPE.MLEADER);
                case Tipo_Selecao.Polyline:
                    return SelecionarObjetos(CAD_TYPE.POLYLINE, CAD_TYPE.LWPOLYLINE);
                case Tipo_Selecao.Linhas:
                    return SelecionarObjetos(CAD_TYPE.LINE);
                case Tipo_Selecao.PolyLine_Linhas:
                    return SelecionarObjetos(CAD_TYPE.LINE, CAD_TYPE.POLYLINE, CAD_TYPE.LWPOLYLINE);
                case Tipo_Selecao.MultiLines:
                    return SelecionarObjetos(CAD_TYPE.MLINE);
                default:
                case Tipo_Selecao.Tudo:
                    return SelecionarObjetos(Conexoes.Utilz.GetLista_Enumeradores<CAD_TYPE>().ToArray());
            }
        }



        public void SelecionarTudo()
        {
            this.Selecoes.Clear();
            this.Selecoes.AddRange(acCurDb.GetAllEntities());
        }

        public PromptSelectionResult SelecionarObjetos(params CAD_TYPE[] filtros)
        {
            PromptSelectionOptions pp = new PromptSelectionOptions();
            pp.RejectObjectsOnLockedLayers = true;
            pp.RejectPaperspaceViewport = true;
            pp.RejectObjectsFromNonCurrentSpace = true;
            pp.AllowDuplicates = false;


            var lista_filtro = new List<TypedValue>();
            lista_filtro.Add(new TypedValue(0, string.Join(",", filtros.ToList().Select(x => x.ToString()).Distinct().ToList())));


            SelectionFilter filtro = new SelectionFilter(lista_filtro.ToArray());
            PromptSelectionResult acSSPrompt = null;

            if (lista_filtro.Count > 0)
            {
                acSSPrompt = acDoc.Editor.GetSelection(pp, filtro);
            }
            else
            {
                acSSPrompt = acDoc.Editor.GetSelection(pp);
            }

            /*Reseta a Seleção Atual*/
            _Polies = null;
            _Furos = null;
            _Atributos_Eixos = null;
            _Entities_Blocos = null;
            _Layers = null;
            _Linhas = null;
            Selecoes.Clear();

            using (var acTrans = acCurDb.acTrans())
            {
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;

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
                                    Selecoes.Add(acEnt);
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ex.Alerta();
                        }
                    }
                }
            }

            return acSSPrompt;
        }
        public void AbrePasta()
        {
            this.Pasta.Abrir();
        }



        public void RenomeiaBlocos()
        {
            var sel = SelecionarObjetos(Tipo_Selecao.Blocos);

            if (sel.Status == PromptStatus.OK)
            {
                var blocos = Selecoes.Filter<BlockReference>();

                var nomes = blocos.GroupBy(x => x.Name);
                foreach (var nome in nomes)
                {
                    var novo_nome = nome.Key.Prompt($"Digite o novo nome para o bloco \n[{nome.Key}]");
                    if (novo_nome != null && novo_nome.Length > 0)
                    {
                        novo_nome = novo_nome.Replace(" ", "_").ToUpper();
                        if ($"Tem certeza que deseja renomear o bloco \n[{nome.Key}] para [{novo_nome}]".Pergunta())
                        {
                            Blocos.Renomear(nome.Key, novo_nome);
                        }
                    }
                }
            }
        }

        public CADBase()
        {
            if (Cfg.Init.GetUser().MA.ToUpper() != "MA1516")
            {
                Cfg.Init.CAD_VerificarVersao();
            }
            Cfg.Init.JanelaWaitMultiThread = false;
            SetUCSParaWorld();
            GetLayers();
            Multiline.GetMLStyles();
        }
    }
}
