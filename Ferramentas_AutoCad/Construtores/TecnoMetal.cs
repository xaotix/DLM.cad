using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ferramentas_DLM
{
    public class TecnoMetal : ClasseBase
    {
        private List<DLMCam.ReadCam> _cams { get; set; } = new List<DLMCam.ReadCam>();
        public List<DLMCam.ReadCam> GetCams(bool atualizar = false)
        {
            if(_cams.Count==0 && this.E_Tecnometal(false) | atualizar && this.E_Tecnometal(false))
            {
                var sub = this.GetSubEtapa();
                var cams = Conexoes.Utilz.GetArquivos(sub.PastaCAM, "*.CAM");

                foreach(var CAM in cams)
                {
                    _cams.Add(new DLMCam.ReadCam(CAM));
                }
            }
            return _cams;
        }
        /*
         Section 'PROFILEDATA' contain:
                    MAT_PRO = MATERIAL
                    TRA_PEZ = FICHA DE PINTURA
                    NOT_PEZ
                    TINSPROF
                    POS_PEZ = POSICAO
                    MAR_PEZ = MARCA
                    DES_PEZ = MERCADORIA
                    REV_DIS
                    COS_PEZ
                    TIP_PEZ
                    SBA_PEZ
                    POS_REV
                    MAR_REV
         */
        public void SetVar3D(Entity selecao, string secao, string propriedade, string valor)
        {
            if (!E_Tecnometal3D()) { return; }
            editor.Command("tec_stsetvar3d", selecao.ObjectId, secao, propriedade, valor);
        }
        public void GerarDBF3D()
        {
            if (!E_Tecnometal3D()) { return; }
            var st = editor.Command("TEC_ST3D2DBF", this.Nome, "t", "N", "N");
        }
        public void RodarMacros(List<string> Arquivos = null)
        {
            if (!E_Tecnometal()) { return; }
            if (Arquivos == null)
            {
                Arquivos = SelecionarDWGs();
            }

            if (Arquivos.Count == 0)
            {
                Alerta("Nenhuma prancha DWG selecionada.");
                return;
            }

            ConfiguracaoMacro cfg = new ConfiguracaoMacro();
            bool status = false;
            Conexoes.Utilz.Propriedades(cfg, out status);

            if (!status) { return; }

            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            if (cfg.GerarDBF)
            {
                var s = GerarDBF(ref erros,cfg.AtualizarCams, null, Arquivos);
            }

            if(erros.Count>0)
            {
                Conexoes.Utilz.ShowReports(erros);
                return;
            }


            Conexoes.Wait w = new Conexoes.Wait(Arquivos.Count);
            w.Show();

            foreach (string drawing in Arquivos)
            {

                if (drawing.ToUpper() == this.Arquivo.ToUpper())
                {
                    if (cfg.GerarTabela)
                    {
                        InserirTabelaAuto(ref erros);
                    }

                    if (cfg.AjustarMViews)
                    {
                        SetViewport();
                    }

                    if (cfg.AjustarLTS)
                    {
                        SetLts();
                    }

                    if (cfg.PreencheSelos)
                    {
                        PreencheSelo();
                    }
                }
                else
                {
                    Document docToWorkOn = documentManager.Open(drawing, false);

                    using (docToWorkOn.LockDocument())
                    {
                        if (cfg.GerarTabela)
                        {
                            InserirTabelaAuto(ref erros);
                        }

                        if(cfg.AjustarMViews)
                        {
                            SetViewport();
                        }

                        if (cfg.AjustarLTS)
                        {
                            SetLts();
                        }

                        if (cfg.PreencheSelos)
                        {
                            PreencheSelo();
                        }
                    }
                    docToWorkOn.CloseAndSave(drawing);
                }
                w.somaProgresso(drawing);
            }
            w.Close();

            Alerta("Finalizado", MessageBoxIcon.Information);
        }

        public List<MarcaTecnoMetal> GetMarcas(DB.Tabela pcs = null)
        {
            List<MarcaTecnoMetal> Retorno = new List<MarcaTecnoMetal>();

            List<MarcaTecnoMetal> mm = new List<MarcaTecnoMetal>();

            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            if (pcs == null)
            {
            pcs = GetPecas(ref erros, false);
            }

            foreach(var pc in pcs.Linhas)
            {
                mm.Add(new MarcaTecnoMetal(pc));
            }

            var ms = mm.Select(x => x.Marca).Distinct().ToList();

            foreach(var m in ms)
            {
                var iguais = mm.FindAll(x => x.Marca == m);

                var marcas = iguais.FindAll(x => x.Posicao == "");
                var posicoes = iguais.FindAll(x => x.Posicao != "");

                if(marcas.Count==1)
                {
                    var marca = marcas[0];
                    marca.SubItens.AddRange(posicoes);
                    Retorno.Add(marca);
                }
                else
                {
                    erros.Add(new Conexoes.Report("Marcas duplicadas no mesmo desenho", $" {marcas[0].Prancha} - M: {m}"));
                    return new List<MarcaTecnoMetal>();
                }

            }


            return Retorno;

        }

        private Conexoes.SubEtapaTecnoMetal _subetapa { get; set; }
        private Conexoes.ObraTecnoMetal _obra { get; set; }
        private Conexoes.PedidoTecnoMetal _pedido { get; set; }
        public Conexoes.SubEtapaTecnoMetal GetSubEtapa()
        {
            if (_subetapa == null && Directory.Exists(this.Pasta))
            {
                _subetapa = new Conexoes.SubEtapaTecnoMetal(this.Pasta, null);
            }
            return _subetapa;
        }
        public Conexoes.ObraTecnoMetal GetObra()
        {
            if (_obra == null && Directory.Exists(this.Pasta))
            {
                _obra = GetSubEtapa().GetObra();
            }
            return _obra;
        }
        public Conexoes.PedidoTecnoMetal GetPedido()
        {
            if (_pedido == null && Directory.Exists(this.Pasta))
            {
                _pedido = GetSubEtapa().GetPedido();
            }
            return _pedido;
        }


        public void InserirTabela()
        {
            //if (!E_Tecnometal()) { return; }

            bool cancelado = false;
            var pt = Utilidades.PedirPonto3D("Clique na origem", out cancelado);
            if (!cancelado)
            {
                List<Conexoes.Report> erros = new List<Conexoes.Report>();
                var pcs = GetPecas(ref erros);
                Conexoes.Utilz.ShowReports(erros);
                if (pcs.Linhas.Count > 0 && erros.Count==0)
                {
                    Tabelas.TecnoMetal(pcs.Linhas, pt);
                }

            }
        }
        public void InserirTabelaAuto(ref List<Conexoes.Report> erros)
        {
            //if (!E_Tecnometal()) { return; }
            IrLayout();
            ZoomExtend();

            var db = this.acCurDb;
            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();

            Point3d pt = new Point3d();
            bool gerar_tabela = false;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                DateTime ultima_edicao = System.IO.File.GetLastWriteTime(this.Pasta);
                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForWrite);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                List<BlockReference> blocos = new List<BlockReference>();
                List<Line> linhas = new List<Line>();
                List<Entity> apagar = new List<Entity>();
                List<Autodesk.AutoCAD.DatabaseServices.DBText> textos = new List<Autodesk.AutoCAD.DatabaseServices.DBText>();
                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForWrite);
                    if (ent is BlockReference)
                    {
                        var s = ent as BlockReference;
                        blocos.Add(s);
                    }
                    else if (ent is Line)
                    {
                        var s = ent as Line;
                        if (s.Color.ColorNameForDisplay.ToUpper() == "RED" && (Angulo.E_Horizontal(s.Angle) | Angulo.E_Vertical(s.Angle)))
                        {
                            linhas.Add(s);
                            apagar.Add(s);
                        }
                        else
                        {

                        }
                    }
                    else if (ent is DBText)
                    {
                        var s = ent as DBText;
                        if (
                            s.TextStyleName.ToUpper() == "TB1" |
                            s.TextStyleName.ToUpper() == "TB2" |
                            s.TextStyleName.ToUpper() == "TB3"
                            )
                        {
                            textos.Add(s);
                            apagar.Add(s);
                        }
                    }

                }




                apagar.AddRange(Utilidades.Filtrar(blocos, new List<string> { "TECNOMETAL_TAB" }, false));

                var selo = Utilidades.Filtrar(blocos, new List<string> { "SELO" }, false);

                foreach (var s in selo)
                {
                    var pts = Utilidades.GetContorno(s, tr);
                    pt = new Point3d(pts.Max(x => x.X) - 7.01, pts.Max(x => x.Y) - 7.01, 0);
                    gerar_tabela = true;
                    break;
                }

                if (apagar.Count > 0)
                {
                    foreach (var s in apagar)
                    {
                        s.Erase(true);
                    }
                    tr.Commit();
                    acDoc.Editor.Regen();
                }
            }

            if (gerar_tabela)
            {
                List<Conexoes.Report> err = new List<Conexoes.Report>();
                var pcs = GetPecas(ref err);
                erros.AddRange(err);

                if (pcs.Linhas.Count > 0 && err.Count==0)
                {
                    Tabelas.TecnoMetal(pcs.Linhas, pt, -186.47);
                }
            }


     
        }

        public List<DB.Linha> GetMarcasLinhas(Database db, Transaction tr)
        {
            var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForWrite);
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
            List<BlockReference> blocos = new List<BlockReference>();
            foreach (ObjectId objId in btr)
            {
                Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForWrite);
                if (ent is BlockReference)
                {
                    var s = ent as BlockReference;
                    blocos.Add(s);
                }
            }
            List<BlockReference> tabela_tecno = Utilidades.Filtrar(blocos, Constantes.BlocosTecnoMetalMarcas, false);
            List<DB.Linha> retorno = new List<DB.Linha>();

            retorno.AddRange(tabela_tecno.Select(x => GetLinha(x, db, this.Arquivo, this.Nome, DateTime.Now)));

            return retorno;


        }
        public void PreencheSelo(bool limpar = false)
        {
            if (!E_Tecnometal()) { return; }
            IrLayout();
            ZoomExtend();

            var db = this.acCurDb;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                DateTime ultima_edicao = System.IO.File.GetLastWriteTime(this.Pasta);
                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForWrite);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                List<BlockReference> blocos = new List<BlockReference>();
                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForWrite);
                    if (ent is BlockReference)
                    {
                        var s = ent as BlockReference;
                        blocos.Add(s);
                    }
                }

                List<BlockReference> tabela_tecno = Utilidades.Filtrar(blocos, new List<string> { "TECNOMETAL_TAB" }, false);
                List<BlockReference> selo = Utilidades.Filtrar(blocos, new List<string> { "SELO" }, false);

                var marcas = GetMarcasLinhas(db, tr);

                var nomes_PECAS = marcas.Select(x => x.Get(Constantes.ATT_MAR).valor).Distinct().ToList();

                foreach (var s in selo)
                {
                    Hashtable att = new Hashtable();

                    if (limpar)
                    {
                        att.Add("TIPO_DE_PROJETO", "");
                        att.Add("TITULO_DA_PRANCHA", "");
                        att.Add("OBRA", "");
                        att.Add("PREDIO", "");
                        att.Add("CLIENTE", "");
                        att.Add("LOCAL", "");
                        att.Add("PEDIDO", "");
                        att.Add("ETAPA", "");
                        att.Add("ESCALA", ""); ;
                        att.Add("UNIDADE", "");
                        att.Add("COORDENAÇÃO", "");
                        att.Add("PROJETO", "");
                        att.Add("DESENHO", "");
                        att.Add("RESPONSAVEL_TECNICO", "");
                        att.Add("CREA", "");
                    }
                    else
                    {
                        att.Add("TIPO_DE_PROJETO", this.Nome.Contains("-FA-") ? "PROJETO DE FABRICAÇÃO" : "PROJETO DE MONTAGEM");
                        att.Add("TITULO_DA_PRANCHA", $"DETALHAMENTO {string.Join(", ", marcas)}");
                        att.Add("OBRA", this.GetObra().Descrição);
                        att.Add("PREDIO", this.GetSubEtapa().Predio);
                        att.Add("CLIENTE", this.GetObra().Cliente);
                        att.Add("LOCAL", this.GetObra().Lugar);
                        att.Add("PEDIDO", this.GetPedido().PedidoID);
                        att.Add("ETAPA", this.GetSubEtapa().NomeEtapa);
                        att.Add("ESCALA", "1/" + Math.Round(this.Getescala(), 1));
                        att.Add("UNIDADE", "MILÍMETROS");
                        att.Add("COORDENAÇÃO", this.GetSubEtapa().Coordenador);
                        att.Add("PROJETO", this.GetSubEtapa().Projetista);
                        att.Add("DESENHO", Conexoes.Vars.UsuarioAtual);
                        att.Add("RESPONSAVEL_TECNICO", this.GetSubEtapa().Calculista);
                        att.Add("CREA", this.GetSubEtapa().CalculistaCREA);
                    }

                    Atributos.Set(s, tr, att);

                }

                if (selo.Count > 0)
                {
                    tr.Commit();
                    acDoc.Editor.Regen();
                }
            }


        }
        public DB.Linha GetLinha(BlockReference bloco, Database db, string arquivo, string nome, DateTime ultima_edicao)
        {
            try
            {
                var att = Atributos.GetLinha(bloco, db);
                att.Add(Constantes.ATT_ARQ, arquivo);
                if (this.E_Tecnometal(false))
                {
                    try
                    {
                        att.Add(Constantes.ATT_PED, this.GetPedido().Codigo);
                        att.Add(Constantes.ATT_OBR, this.GetObra().Nome);
                        att.Add(Constantes.ATT_ETP, this.GetSubEtapa().NomeEtapa);
                    }
                    catch (Exception)
                    {
                    }
                }

                att.Add(Constantes.ATT_NUM, nome);
                att.Add(Constantes.ATT_DWG, nome);
                att.Add(Constantes.ATT_REC, att.Get(Constantes.ATT_POS).ToString() == "" ? Constantes.ATT_REC_MARCA : Constantes.ATT_REC_POSICAO);
                att.Add(Constantes.ATT_DAT, ultima_edicao.ToShortDateString());
                att.Add(Constantes.ATT_BLK, bloco.Name.ToUpper());

                return att;
            }
            catch (Exception ex)
            {
                DB.Linha att = new DB.Linha();
                att.Add(Constantes.ATT_BLK, bloco.Name.ToUpper());
                att.Add(Constantes.ATT_DWG, nome);
                att.Add("ERRO", ex.Message);
                att.Add(Constantes.ATT_ARQ, arquivo);

                return att;
            }
        }
        public DB.Tabela GetPecas(ref List<Conexoes.Report> erros, bool converter_padrao_dbf = true)
        {
            var db = this.acCurDb;
            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                DateTime ultima_edicao = System.IO.File.GetLastWriteTime(this.Pasta);
                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                List<BlockReference> blocos = new List<BlockReference>();
                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                    if (ent is BlockReference)
                    {
                        var s = ent as BlockReference;
                        blocos.Add(s);
                    }
                }

                var nomes = blocos.Select(x => x.Name).Distinct().ToList();
                List<BlockReference> ms = Utilidades.Filtrar(blocos, Constantes.BlocosTecnoMetalMarcas);
                List<BlockReference> pos = Utilidades.Filtrar(blocos, Constantes.BlocosTecnoMetalPosicoes);


                foreach (var m in ms)
                {
                    marcas.Linhas.Add(GetLinha(m, db, this.Arquivo, this.Nome, ultima_edicao));
                }

                foreach (var m in pos)
                {
                    posicoes.Linhas.Add(GetLinha(m, db, this.Arquivo, this.Nome, ultima_edicao));
                }

            }


            if (converter_padrao_dbf)
            {
                return GetDBF(ref erros ,marcas, posicoes);
            }
            else
            {
                var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
                return lista;
            }
        }
        public DB.Tabela GetPecasPranchas(ref List<Conexoes.Report> erros, List<string> pranchas = null, bool filtrar = true, bool converter_padrao_dbf = true)
        {
  
            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            try
            {
                List<FileInfo> arquivos = new List<FileInfo>();
                if (pranchas == null)
                {
                    pranchas = this.SelecionarDWGs();
                    if (pranchas.Count > 0)
                    {
                        arquivos = pranchas.Select(x => new FileInfo(x)).ToList();
                    }
                }
                else
                {
                    pranchas = pranchas.FindAll(x => x.ToUpper().EndsWith(".DWG")).ToList();
                    arquivos = pranchas.Select(x => new FileInfo(x)).ToList();
                }



                if (arquivos.Count == 0)
                {
                    erros.Add(new Conexoes.Report("Abortado", "Operação abortada - Nada Selecionado."));
                    return new DB.Tabela();
                }

                Conexoes.Wait w = new Conexoes.Wait(arquivos.Count(), "Carregando...");
                w.Show();
                foreach (FileInfo file in arquivos)
                {
                    w.somaProgresso($"Mapeando peças: {file.Name}");
                    DateTime ultima_edicao = System.IO.File.GetLastWriteTime(file.FullName);

                    var nome_arq = Conexoes.Utilz.getNome(file.FullName);
                    string arquivo = file.FullName;
                    try
                    {
                        using (Database db = new Database(false, true))
                        {
                            db.ReadDwgFile(arquivo, FileOpenMode.OpenForReadAndAllShare, false, null);
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {

                                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                                List<BlockReference> blocos = new List<BlockReference>();
                                foreach (ObjectId objId in btr)
                                {
                                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                                    if (ent is BlockReference)
                                    {
                                        var s = ent as BlockReference;
                                        blocos.Add(s);
                                    }
                                }

                                var nomes = blocos.Select(x => x.Name).Distinct().ToList();
                                List<BlockReference> ms = Utilidades.Filtrar(blocos, Constantes.BlocosTecnoMetalMarcas);
                                List<BlockReference> pos = Utilidades.Filtrar(blocos, Constantes.BlocosTecnoMetalPosicoes);


                                foreach (var m in ms)
                                {
                                    marcas.Linhas.Add(GetLinha(m, db, arquivo, nome_arq, ultima_edicao));
                                }

                                foreach (var m in pos)
                                {
                                    posicoes.Linhas.Add(GetLinha(m, db, arquivo, nome_arq, ultima_edicao));
                                }

                            }
                            db.CloseInput(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        w.Close();
                        erros.Add(new Conexoes.Report("Erro fatal", ex.Message + "\n" + ex.StackTrace, Conexoes.TipoReport.Crítico));
                        return new DB.Tabela();
                    }

                }
                w.Close();
            }
            catch (System.Exception ex)
            {
            }


            if (converter_padrao_dbf)
            {
                return GetDBF(ref erros, marcas, posicoes);
            }
            else
            {
                var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
                return lista;
            }

        }

        private static void AddDivergenciaPOS(ref List<Conexoes.Report> erros, IGrouping<string, MarcaTecnoMetal> pos, List<IGrouping<string, MarcaTecnoMetal>> divergencias, string tipo_erro)
        {
            if (divergencias.Count > 1)
            {
                erros.Add(new Conexoes.Report($"Divergência de {tipo_erro}",
                    $"Pos: {pos.Key}\n: " +
                    $"{string.Join("\n", divergencias.Select(x => $"{string.Join("\n", x.Select(y => y.Prancha + " Valor: " + x.Key))}"))}"
                    , Conexoes.TipoReport.Crítico));
            }
        }

        private DB.Tabela GetDBF(ref List<Conexoes.Report> erros, DB.Tabela marcas, DB.Tabela posicoes)
        {
            var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
            List<string> colunas = new List<string>();
            colunas.Add(Constantes.ATT_REC);
            colunas.Add(Constantes.ATT_PED);
            colunas.Add(Constantes.ATT_OBR);
            colunas.Add(Constantes.ATT_ETP);
            colunas.Add("DLO_COM");
            colunas.Add(Constantes.ATT_CLI);
            colunas.Add("IND_COM");
            colunas.Add("DT1_COM");
            colunas.Add("DT2_COM");
            colunas.Add(Constantes.ATT_NUM);
            colunas.Add("DES_DIS");
            colunas.Add("NOM_DIS");
            colunas.Add("REV_DIS");
            colunas.Add(Constantes.ATT_DAT);
            colunas.Add(Constantes.ATT_FIC);
            colunas.Add("SBA_PEZ");
            colunas.Add(Constantes.ATT_TPC);
            colunas.Add(Constantes.ATT_MAR);
            colunas.Add("MBU_PEZ");
            colunas.Add(Constantes.ATT_MER);
            colunas.Add(Constantes.ATT_POS);
            colunas.Add(Constantes.ATT_NOT);
            colunas.Add(Constantes.ATT_VOL);
            colunas.Add(Constantes.ATT_QTD);
            colunas.Add("QT1_PEZ");
            colunas.Add(Constantes.ATT_CIC);
            colunas.Add(Constantes.ATT_SAP);
            colunas.Add(Constantes.ATT_CCC);
            colunas.Add(Constantes.ATT_PER);
            colunas.Add(Constantes.ATT_CMP);
            colunas.Add(Constantes.ATT_LRG);
            colunas.Add(Constantes.ATT_ESP);
            colunas.Add(Constantes.ATT_MAT);
            colunas.Add("TIP_BUL");
            colunas.Add("DIA_BUL");
            colunas.Add("LUN_BUL");
            colunas.Add("PRB_BUL");
            colunas.Add(Constantes.ATT_PES);
            colunas.Add(Constantes.ATT_SUP);
            colunas.Add(Constantes.ATT_PRE);
            colunas.Add(Constantes.ATT_DWG);


            //propriedades que não vão para a DBF
            colunas.Add(Constantes.ATT_ARQ);
            colunas.Add(Constantes.ATT_BLK);


            DB.Tabela tab_pecas = new DB.Tabela();
            var grp_blocos = lista.Linhas.GroupBy(x => x.Get(Constantes.ATT_MAR).ToString()).ToList().ToList();
            foreach (var s in lista.Linhas)
            {
                DB.Linha l = new DB.Linha();
                foreach (var c in colunas)
                {
                    var igual = s.Get(c);
                    l.Add(c, igual.valor);
                }
                tab_pecas.Linhas.Add(l);
            }

            tab_pecas.Linhas = tab_pecas.Linhas.OrderBy(x => x.Descricao).ToList();

            List<DB.Linha> l_marcas = new List<DB.Linha>();
            var agrupado = tab_pecas.Linhas.GroupBy(x => x.Get(Constantes.ATT_MAR).ToString()).Select(x => x.ToList()).ToList();


            foreach (var m in agrupado)
            {
                //CRIA A LINHA DA MARCA SIMPLES
                if (m.Count() == 1)
                {

                    var p_simples = m[0].Clonar();
                    var m_simples = m[0].Clonar();

                    m_simples.Set(Constantes.ATT_REC, Constantes.ATT_REC_MARCA);
                    m_simples.Set(Constantes.ATT_POS, "");
                    m_simples.Set(Constantes.ATT_SAP, "");
                    m_simples.Set(Constantes.ATT_PER, "");
                    m_simples.Set(Constantes.ATT_CMP, "");
                    m_simples.Set(Constantes.ATT_LRG, "");
                    m_simples.Set(Constantes.ATT_ESP, "");
                    m_simples.Set(Constantes.ATT_MAT, "");
                    m_simples.Set(Constantes.ATT_MAT, "");
                    m_simples.Set(Constantes.ATT_PES, "");
                    m_simples.Set(Constantes.ATT_SUP, "");
                    m_simples.Set(Constantes.ATT_PRE, "");
                    
                    p_simples.Set(Constantes.ATT_QTD, 1);
                    p_simples.Set(Constantes.ATT_REC, Constantes.ATT_REC_POSICAO);
                    p_simples.Set(Constantes.ATT_POS, p_simples.Get(Constantes.ATT_MAR).valor);
                    p_simples.Set(Constantes.ATT_BLK, "DUMMY");

                    l_marcas.Add(m_simples);
                    l_marcas.Add(p_simples);


                }
                else
                {
                    //junta posições iguais
                    var marca = m.FindAll(x => x.Get(Constantes.ATT_POS).valor == "");
                    if (marca.Count > 1)
                    {
                        string mm = marca[0].Get(Constantes.ATT_MAR).ToString();
                        erros.Add(new Conexoes.Report("Marca Duplicada",
                            $"\n{mm}" +
                            $"\nMarca duplicada ou se encontra em mais de uma prancha." +
                            $"\nOcorrências: {marca.Count} x\n" +
                            $"{string.Join("\n", marca.Select(x => x.Get("FLG_DWG")).Distinct().ToList())}",
                           Conexoes.TipoReport.Crítico
                            ));
                    }
                    else
                    {
                        var posicoes_tbl = m.FindAll(x => x.Get(Constantes.ATT_POS).valor != "").GroupBy(x => x.Get(Constantes.ATT_POS).valor).Select(X => X.ToList());
                        l_marcas.AddRange(marca);
                        foreach (var pos in posicoes_tbl)
                        {
                            var lfim = pos[0].Clonar();
                            lfim.Set(Constantes.ATT_QTD, pos.Sum(x => x.Get(Constantes.ATT_QTD).Double()));
                            l_marcas.Add(lfim);
                        }
                    }
                }
            }
            foreach (var l in l_marcas)
            {
                l.Descricao = l.Get(Constantes.ATT_MAR) + " - P = " + l.Get(Constantes.ATT_POS);
            }

            //ordena as peças, colocando as marcas antes das posições
            l_marcas = l_marcas.OrderBy(x => x.Get("FLG_REC").valor.PadLeft(2, '0') + x.Descricao).ToList();

            DB.Tabela lista_convertida = new DB.Tabela();
            lista_convertida.Linhas.AddRange(l_marcas);

            return lista_convertida;
        }

        public DB.Tabela GerarDBF(ref List<Conexoes.Report> erros, bool atualizar_cams,string destino = null, List<string> pranchas = null)
        {
            if(!E_Tecnometal())
            {
                return new DB.Tabela();
            }
            if (!this.Pasta.ToUpper().EndsWith(@".TEC\"))
            {
                erros.Add(new Conexoes.Report("Pasta Inválida", $"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                    $"\nPasta atual: {this.Pasta}", Conexoes.TipoReport.Crítico));
                return new DB.Tabela();
            }

            var etapa = this.GetSubEtapa();
            if (destino == null | destino == "")
            {
            setar_nome:
                var nome_dbf = Conexoes.Utilz.RemoverCaracteresEspeciais(Conexoes.Utilz.Prompt("Digine o nome do arquivo", "", etapa.Nome, false, "", false, 16)).ToUpper();

                if (!nome_dbf.Contains("ETAPA_") | nome_dbf.Length < 7 | nome_dbf.Length > 16)
                {
                    if (Conexoes.Utilz.Pergunta(
                        
                        "Nome da DBF deve conter 'ETAPA_'" +
                        "\n Mínimo 7 caracteres." +
                        "\n Máximo 16 caracteres." +
                        "\n Tentar novamente?"))
                    {
                        goto setar_nome;
                    }
                    else
                    {
                        erros.Add(new Conexoes.Report("Cancelado", "Nome da DBF inválido", Conexoes.TipoReport.Crítico));
                        return new DB.Tabela();
                    }
                }
                destino = etapa.PastaDBF + nome_dbf + ".DBF";
            }

            if (!Conexoes.Utilz.Apagar(destino))
            {
                erros.Add(new Conexoes.Report("Erro", $"Não é possível substituir o arquivo atual: {destino}", Conexoes.TipoReport.Crítico));
                return new DB.Tabela();
            }

           


            var lista_pecas = GetPecasPranchas(ref erros, pranchas);


            Conexoes.Wait w = new Conexoes.Wait(5, "Fazendo Verificações...");
            w.Show();

            if (lista_pecas.Linhas.Count == 0)
            {
                erros.Add(new Conexoes.Report("Erro", "Nenhuma peça encontrada nas pranchas selecionadas", Conexoes.TipoReport.Crítico));
                return new DB.Tabela();
            }

            var marcas = GetMarcas(lista_pecas);
            w.somaProgresso();


            var posicoes = marcas.SelectMany(x => x.SubItens).ToList();
            var marcas_simples = marcas.FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaSimples);
            posicoes.AddRange(marcas_simples);
            var posicoes_perfis = posicoes.FindAll(x => x.Tipo_Bloco == Tipo_Bloco.Perfil | x.Tipo_Bloco == Tipo_Bloco.Elemento_M2).ToList();
            var posicoes_elem_unit = posicoes.FindAll(x => x.Tipo_Bloco == Tipo_Bloco.Elemento_Unitario).ToList();
            var perfis = posicoes_perfis.GroupBy(x=>x.Perfil.ToUpper().TrimStart().TrimEnd()).ToList();
            var mercs = marcas.GroupBy(x => x.Mercadoria);
            var mats = posicoes.FindAll(x => x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).GroupBy(x => x.Material);
            var posicoes_grp = posicoes.FindAll(x=> x.Tipo_Bloco!= Tipo_Bloco.Arremate && x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).GroupBy(x => x.Posicao).ToList();


            foreach (var pf in perfis)
            {
                var igual = Utilidades.GetdbTecnoMetal().Get(pf.Key);

                if(igual.Nome=="")
                {
                    erros.Add(new Conexoes.Report($"Perfil não cadastrado.", $"{pf.Key} \n{string.Join("\n", pf.ToList().Select(x =>  $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}").Distinct().ToList())}", Conexoes.TipoReport.Crítico));
                }
            }
            w.somaProgresso();

            //valida as divergências entre as posições
            foreach(var pos in posicoes_grp)
            {
                AddDivergenciaPOS(ref erros, pos, pos.GroupBy(x => x.Espessura.ToString("N2")).ToList(), "espessura");
                AddDivergenciaPOS(ref erros, pos, pos.GroupBy(x => Math.Round(x.Largura).ToString("N2")).ToList(), "largura");
                AddDivergenciaPOS(ref erros, pos, pos.GroupBy(x => Math.Round(x.Comprimento).ToString("N2")).ToList(), "comprimento");
                AddDivergenciaPOS(ref erros, pos, pos.GroupBy(x => x.Material).ToList(), "material");
                AddDivergenciaPOS(ref erros, pos, pos.GroupBy(x => x.Perfil).ToList(), "perfil");
            }

            erros.AddRange(posicoes_elem_unit.FindAll(x => !x.Posicao.ToUpper().EndsWith("_A")).Select(x => new Conexoes.Report("Nome Pos. Inválido", $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}: Peças com elemento unitário devem ser marcadas com '_A' no fim.")));

            w.somaProgresso();
       

            foreach(var m in mercs)
            {
                var igual = Conexoes.DBases.GetMercadorias().Find(x => x.ToUpper() == m.Key.ToUpper());
                if(igual ==null)
                {
                    erros.Add(new Conexoes.Report("Mercadoria em branco ou inválida", $"Mercadoria: {m.Key} \n Marcas:\n" + string.Join("\n", m.ToList().Select(x=> $"{x.Prancha} - [{x.Marca}]").Distinct().ToList()), Conexoes.TipoReport.Erro));
                }
            }
            w.somaProgresso();
            foreach (var m in mats)
            {
                var igual = Conexoes.DBases.GetMateriais().Find(x => x.ToUpper() == m.Key.ToUpper());
                if (igual == null)
                {
                    erros.Add(new Conexoes.Report("Material em branco ou inválido", $"Material: {m.Key} \n Peças:\n" + string.Join("\n", m.ToList().Select(x => $"{x.Prancha} - [{x.Marca} - P: {x.Posicao}]").Distinct().ToList()), Conexoes.TipoReport.Erro));
                }
            }

            w.somaProgresso();
            if(atualizar_cams)
            {
                var cams = GetCams();

                foreach (var pos in posicoes_grp)
                {
                    var cam = cams.Find(x => x.Nome.ToUpper() == pos.Key.ToUpper());
                    var p0 = pos.ToList()[0];



                    if (cam != null)
                    {

                        if (Math.Round(p0.Comprimento) != Math.Round(cam.Comprimento) && Math.Round(p0.Comprimento) != Math.Round(cam.Largura))
                        {
                            erros.Add(new Conexoes.Report("CAM x Projeto: Comprimento divergente", $"Pos: {pos.Key} Projeto: C: {Math.Round(p0.Comprimento)} L: {Math.Round(p0.Largura)} x  CAM: C: {Math.Round(cam.Comprimento)} Larg.: {Math.Round(cam.Largura)}", Conexoes.TipoReport.Erro));
                        }

                        if (cam.Familia == DLMCam.Familia.Chapa)
                        {
                            if (Math.Round(p0.Largura) != Math.Round(cam.Largura) && Math.Round(p0.Largura) != Math.Round(cam.Comprimento))
                            {
                                erros.Add(new Conexoes.Report("CAM x Projeto: Largura divergente", $"Pos: {pos.Key} Projeto: C: {Math.Round(p0.Comprimento)} L: {Math.Round(p0.Largura)} x  CAM: C: {Math.Round(cam.Comprimento)} Larg.: {Math.Round(cam.Largura)}", Conexoes.TipoReport.Erro));
                            }

                            if (Math.Round(p0.Espessura, 2) != Math.Round(cam.Espessura, 2))
                            {
                                erros.Add(new Conexoes.Report("CAM x Projeto: Espessura divergente", $"Pos: {pos.Key} Projeto: {p0.Espessura} x  CAM: {cam.Espessura}", Conexoes.TipoReport.Erro));
                            }
                        }


                        cam.Quantidade = (int)Math.Round(pos.Sum(x => x.Quantidade));
                        cam.Obra = this.GetObra().Descrição;
                        cam.Pedido = this.GetPedido().Codigo;
                        cam.Etapa = this.GetSubEtapa().Nome;
                        cam.Material = p0.Material;
                        cam.Tratamento = p0.Tratamento;
                        cam.Prancha = cam.Etapa;
                        cam.Peso = p0.PesoUnit;

                        foreach (var s in cam.SubCams)
                        {
                            var arq = this.GetSubEtapa().PastaCAM + s + ".CAM";
                            if (!File.Exists(arq))
                            {
                                erros.Add(new Conexoes.Report("Falta CAM Desmembrado", $"{s}.CAM \n {string.Join("\n", pos.Select(x => $"{x.Prancha} - M: {x.Marca}"))}", Conexoes.TipoReport.Crítico));
                            }
                            else
                            {
                                DLMCam.ReadCam sub = new DLMCam.ReadCam(arq);
                                sub.Obra = this.GetObra().Descrição;
                                sub.Pedido = this.GetPedido().Codigo;
                                sub.Etapa = this.GetSubEtapa().Nome;
                                sub.Material = p0.Material;
                                sub.Tratamento = p0.Tratamento;
                                sub.Salvar();
                            }


                        }
                        cam.Salvar();
                    }
                    else
                    {
                        if (p0.Espessura > 1)
                        {
                            erros.Add(new Conexoes.Report("Falta CAM", $"{p0.Posicao}.CAM \n {string.Join("\n", pos.Select(x => $"{x.Prancha} - M: {x.Marca}"))}", Conexoes.TipoReport.Alerta));
                        }
                    }
                }
            }
           

            w.Close();

            if (destino != "" && destino != null && lista_pecas.Linhas.Count > 0)
            {
                if (!Conexoes.Utilz.GerarDBF(lista_pecas, destino))
                {
                    lista_pecas.Banco = "";
                    return lista_pecas;
                }
            }
            lista_pecas.Banco = destino;
            return lista_pecas;
        }


        private string PromptMarca(string prefix = "ARR-")
        {
            var marcas = this.GetMarcas();
            var nnn = marcas.FindAll(x => x.Marca.StartsWith(prefix)).Count +1;
            retentar:
            var m = Conexoes.Utilz.Prompt("Digite o nome da Marca", "Nome da marca", prefix + nnn.ToString().PadLeft(2,'0'), false, "", false, 12).ToUpper().Replace(" ","");

            if(m.Length==0)
            {
                if (Conexoes.Utilz.Pergunta("Nome não pode ser em branco. \nTentar Novamente?"))
                {
                    goto retentar;
                }
                else
                {
                    return "";
                }
            }
            var iguais = marcas.FindAll(x => x.Marca == m);
            if (iguais.Count>0)
            {
                if (Conexoes.Utilz.Pergunta($"[{m}]Já existe uma marca com o mesmo nome. É necessário trocar. \nTentar Novamente?"))
                {
                    goto retentar;
                }
                else
                {
                    return "";
                }
            }
            if(Conexoes.Utilz.CaracteresEspeciais(m))
            {
                if (Conexoes.Utilz.Pergunta($"[{m}] Nome não pode conter caracteres especiais. É necessário trocar. \nTentar Novamente?"))
                {
                    goto retentar;
                }
                else
                {
                    return "";
                }
            }
            return m;
        }
        private  string PromptFicha()
        {
            return  Conexoes.Utilz.Prompt("Digite a ficha de pintura", "Ficha de pintura", "SEM PINTURA", true, "FICHA", false, 20);
        }
        public  string PromptMaterial()
        {
            var mat = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetMateriais(), null, "Selecione");
            return mat;
        }
        public string PromptMercadoria()
        {
            var mat = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetMercadorias(), null, "Selecione");
            return mat;
        }

        public void InserirArremate(string marca = "", string posicao = "")
        {

            if (marca == "")
            {
                marca = PromptMarca("ARR-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos(acTrans);
                if (sel.Status == PromptStatus.OK)
                {
                    var pols = this.Getpolylinhas();
                    if (pols.Count > 0)
                    {

                        bool status = false;

                        var pl = pols[0];

                        if (pl.Closed)
                        {
                            Alerta("Polyline inválida. Somente polylines abertas representando o corte da chapa.");
                            return;
                        }


                        double comprimento = Utilidades.PedirDistancia("Defina o comprimento", out status);

                        if (status)
                        {
                            return;
                        }


                        List<LineSegment3d> segmentos = new List<LineSegment3d>();
                        var angulos = Angulo.GetAngulos(pl, out segmentos);

                        foreach (var s in angulos)
                        {
                            AddMensagem($"\nAngulo:" + s);
                        }


                        double corte = Math.Round(pl.Length);
                        var chapas = Conexoes.DBases.GetChapas();


                        var espessura = Conexoes.Utilz.SelecionarObjeto(chapas, null, "Selecione uma espessura");
                        if (espessura == null)
                        {
                            return;
                        }
                        string ficha = "SEM PINTURA";
                        Conexoes.Bobina bobina = Conexoes.DBases.GetBobinaDummy();
                        bool chapa_fina = espessura.GetChapa_Fina();
                        if (chapa_fina)
                        {
                            var bobinas = Conexoes.DBases.GetBancoRM().GetBobinas();
                            bobina = Conexoes.Utilz.SelecionarObjeto(bobinas.FindAll(x => x.Espessura == espessura.valor && x.Corte == espessura.bobina_corte), null, "Selecione uma espessura");
                        }
                        else
                        {
                            bobina.Espessura = espessura.valor;
                            bobina.Material = "CIVIL 350";
                            ficha = PromptFicha();
                        }

                        if (bobina == null)
                        {
                            return;
                        }

                        Chapa_Dobrada pa = Conexoes.Utilz.Propriedades(new Chapa_Dobrada(bobina, corte, comprimento, angulos) { Marca = marca, GerarCam = chapa_fina ? Opcao.Nao : Opcao.Sim, DescontarDobras = !chapa_fina, Ficha = ficha }, out status);
                        if (status)
                        {
                            if (pa.Comprimento > 0 && pa.Espessura > 0 && pa.Marca.Replace(" ", "") != "" && pa.Quantidade > 0)
                            {
                                bool cancelado = true;
                                var origem = Utilidades.PedirPonto3D("Selecione o ponto de inserção do bloco.", out cancelado);
                                if (!cancelado)
                                {
                                    if (chapa_fina)
                                    {
                                        Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Arremate, this.Getescala(), posicao);
                                    }
                                    else
                                    {
                                        Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Chapa, this.Getescala(), posicao);
                                    }

                                    if (pa.GerarCam == Opcao.Sim)
                                    {
                                        string destino = this.Pasta;
                                        if (this.Pasta.EndsWith(".TEC"))
                                        {
                                            destino = Conexoes.Utilz.CriarPasta(Conexoes.Utilz.getUpdir(destino), "CAM");
                                        }
                                        if (Directory.Exists(destino))
                                        {
                                            DLMCam.Chapa pp = new DLMCam.Chapa(pa.Comprimento, pa.Largura, pa.Espessura);

                                            string arquivo = destino + pa.Marca + ".CAM";

                                            DLMCam.Cam pcam = new DLMCam.Cam(arquivo, pp);
                                            double x = 0;

                                            for (int i = 0; i < angulos.Count; i++)
                                            {
                                                var s = segmentos[i];
                                                x = x + s.Length - (chapa_fina ? 0 : pa.Espessura);
                                                var a = angulos[i];
                                                pcam.Dobras.Liv1.Add(new DLMCam.Estrutura.Dobra(a, x, pcam, false));
                                            }

                                            pcam.Cabecalho.TRA_PEZ = pa.Ficha;
                                            pcam.Cabecalho.Quantidade = pa.Quantidade;
                                            pcam.Cabecalho.Material = pa.Material;
                                            pcam.Cabecalho.Marca = pa.Marca;
                                            pcam.Nota = "PARA DOBRAS = SEGUIR DESENHO DA PRANCHA DE FABRICAÇÃO.";
                                            pcam.Gerar();
                                            Conexoes.Utilz.Abrir(destino);

                                        }
                                    }
                                }

                            }
                        }

                    }
                }
            }
        }
        public void InserirChapa(string marca = "", string posicao = "")
        {
            if (marca == "")
            {
                marca = PromptMarca("CH-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                bool status = false;
                double comprimento = Utilidades.PedirDistancia("Defina o comprimento:", out status);
                if (!status)
                {
                    double largura = Utilidades.PedirDistancia("Defina a largura:", out status);
                    if (!status)
                    {
                        var chapas = Conexoes.DBases.GetChapas();
                        var espessura = Conexoes.Utilz.SelecionarObjeto(chapas, null, "Selecione a espessura");
                        string ficha = "SEM PINTURA";
                        if (espessura != null)
                        {
                            Conexoes.Bobina bobina = Conexoes.DBases.GetBobinaDummy();
                            bool chapa_fina = espessura.GetChapa_Fina();
                            if (chapa_fina)
                            {
                                var bobinas = Conexoes.DBases.GetBancoRM().GetBobinas();
                                bobina = Conexoes.Utilz.SelecionarObjeto(bobinas.FindAll(x => x.Espessura == espessura.valor && x.Corte == espessura.bobina_corte), null, "Selecione uma espessura");
                            }
                            else
                            {
                                bobina.Espessura = espessura.valor;
                                bobina.Material = "CIVIL 350";
                                ficha = PromptFicha();
                              
                            }

                            if (bobina != null)
                            {

                                Chapa_Dobrada pa = Conexoes.Utilz.Propriedades(new Chapa_Dobrada(bobina, comprimento, largura) { Marca = marca, GerarCam = chapa_fina ? Opcao.Nao : Opcao.Sim, Ficha = ficha }, out status);
                                if (status)
                                {
                                    var origem = Utilidades.PedirPonto3D("Selecione a origem", out status);
                                    if (!status)
                                    {
                                        if (chapa_fina)
                                        {
                                            Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Arremate, this.Getescala(), posicao);
                                        }
                                        else
                                        {
                                            Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Chapa, this.Getescala(), posicao);
                                        }

                                        if (pa.GerarCam == Opcao.Sim)
                                        {
                                            string destino = this.Pasta;
                                            if (this.Pasta.EndsWith(".TEC"))
                                            {
                                                destino = Conexoes.Utilz.CriarPasta(Conexoes.Utilz.getUpdir(destino), "CAM");
                                            }
                                            if (Directory.Exists(destino))
                                            {
                                                DLMCam.Chapa pp = new DLMCam.Chapa(pa.Comprimento, pa.Largura, pa.Espessura);

                                                string arquivo = destino + pa.Marca + ".CAM";

                                                DLMCam.Cam pcam = new DLMCam.Cam(arquivo, pp);

                                                pcam.Cabecalho.TRA_PEZ = pa.Ficha;
                                                pcam.Cabecalho.Quantidade = pa.Quantidade;
                                                pcam.Cabecalho.Material = pa.Material;
                                                pcam.Cabecalho.Marca = pa.Marca;
                                                pcam.Gerar();
                                                Conexoes.Utilz.Abrir(destino);

                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                }

            }
        }
        public void InserirElementoUnitario(string marca = "", string posicao = "")
        {
            if (marca == "")
            {
                marca = PromptMarca("CH-");
            }
            if (marca == null | marca == "") { return; }
            Conexoes.RMA peca = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione uma peça");

            if (peca != null)
            {
                bool status = false;
            denovo:
                double qtd = Conexoes.Utilz.Prompt(peca.Multiplo, out status);
                if (!status)
                {
                    return;
                }
                if (qtd <= 0 | !peca.MultiploOk(qtd))
                {
                    if (Conexoes.Utilz.Pergunta($"Valor setado [{qtd} é inválido. Precisa ser maior que zero e múltiplo de {peca.Multiplo}\nTentar novamente?"))
                    {
                        goto denovo;
                    }
                    else
                    {
                        return;
                    }
                }

                var origem = Utilidades.PedirPonto3D("Selecione a origem", out status);

                if (status)
                {
                    return;
                }

                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {

                    Blocos.MarcaElemUnitario(origem, peca, qtd, marca, this.Getescala(), posicao);

                }
            }
        }
        public void InserirElementoM2(string marca = "", string posicao = "")
        {

            if (marca == "")
            {
                marca = PromptMarca("ARR-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos(acTrans);
                if (sel.Status == PromptStatus.OK)
                {
                    var pols = this.Getpolylinhas();
                    if (pols.Count > 0)
                    {


                        var pl = pols[0];

                        if (!pl.Closed)
                        {
                            Alerta("Polyline inválida. Somente polylines fechadas representando o contorno da chapa.");
                            return;
                        }

                        double comprimento = 0;
                        double largura = 0;
                        double area = 0;
                        double perimetro = 0;
                        Utilidades.GetInfo(pl, out comprimento, out largura, out area, out perimetro);

                        comprimento = Math.Round(comprimento, 4);
                        largura = Math.Round(largura, 4);

                        bool status = true;
                        string material = "CIVIL 350";


                        if (comprimento > 0 && largura > 0)
                        {

                            double quantidade = Conexoes.Utilz.Prompt(1, out status, 0);

                            if (quantidade > 0)
                            {
                                var perfil = Conexoes.Utilz.SelecionarObjeto(Utilidades.GetdbTecnoMetal().GetPerfis(DLMCam.TipoPerfil.Chapa_Xadrez), null, "Selecione um perfil");
                                if (perfil != null)
                                {
                                    var ficha = PromptFicha();

                                    var ponto = Utilidades.PedirPonto3D("Selecione a origem do bloco", out status);
                                    if (!status)
                                    {
                                        Blocos.MarcaElemM2(ponto, perfil, marca, quantidade, comprimento, largura, area,perimetro, ficha,material, this.Getescala(), posicao);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Mercadorias()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var selecao = SelecionarObjetos(acTrans);
                var marcas = Utilidades.Filtrar(this.Getblocos(), Constantes.BlocosTecnoMetalMarcas);

                if(marcas.Count>0)
                {
                    var mercadoria =PromptMercadoria();
                    if(mercadoria!=null && mercadoria!="")
                    {
                        foreach(var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, Constantes.ATT_MER, mercadoria);
                        }
                    }
                }
            }
        }

        public void Materiais()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var selecao = SelecionarObjetos(acTrans);
                var marcas = Utilidades.Filtrar(this.Getblocos(), Constantes.BlocosTecnoMetalMarcas);

                if (marcas.Count > 0)
                {
                    var mercadoria = PromptMaterial();
                    if (mercadoria != null && mercadoria != "")
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, Constantes.ATT_MAT, mercadoria);
                        }
                    }
                }
            }
        }

        public void Tratamentos()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {


                var selecao = SelecionarObjetos(acTrans);

                var marcas = Utilidades.Filtrar(this.Getblocos(), Constantes.BlocosTecnoMetalMarcas);

                if (marcas.Count > 0)
                {
                    var mercadoria = this.PromptFicha();
                    if (mercadoria != null && mercadoria != "")
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, Constantes.ATT_FIC, mercadoria);
                        }
                    }
                }
            }
        }
        public TecnoMetal()
        {

        }
    }
}
