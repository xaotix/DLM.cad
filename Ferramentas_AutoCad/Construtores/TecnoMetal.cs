using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Ferramentas_DLM.CAD;

namespace Ferramentas_DLM
{
    public class TecnoMetal : ClasseBase
    {
        public void Quantificar()
        {
            var sel = SelecionarObjetos();
            if(sel.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK && this.selecoes.Count>0)
            {
                bool status = false;
                var opt = Conexoes.Utilz.Propriedades(new ConfiguracaoQuantificar(), out status);

                if(status)
                {
                    List<PCQuantificar> pecas = new List<PCQuantificar>();
                    if(opt.Blocos)
                    {
                        foreach(var s in this.Getblocos().FindAll(x=>!
                        x.Name.Contains("*"))
                        .GroupBy(x=>x.Name.ToUpper()
                        .Replace("SUPORTE_","")
                        .Replace("SUPORTE ","")
                        ))
                        {
                            var att = Atributos.GetLinha(s.First());

                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Bloco,s.Key, "",s.ToList().Select(x=> Ferramentas_DLM.Atributos.GetLinha(x)).ToList());

                            if(npc.Nome.StartsWith("PECA_INDICACAO"))
                            {
                                var blcs = npc.Agrupar(new List<string> { "CODIGO", "Nº" });
                                foreach(var bl in blcs)
                                {
                                    bl.SetDescPorAtributo("DESC");
                                    bl.SetNumeroPorAtributo("Nº");
                                    bl.SetDestinoPorAtributo("DESTINO");
                                    bl.SetQtdPorAtributo("QTD");
                                }

                                pecas.AddRange(blcs);
                                    /*
                                     *             Hashtable att = new Hashtable();
            att.Add("Nº", this.numero.Text);
            att.Add("FAMILIA", this.familia.Text);
            att.Add("TIPO", this.peca_selecionar.Content);
            att.Add("COMP", comp.ToString().Replace(",",""));
            att.Add("CODIGO", codigo);
            att.Add("ID", id);
            att.Add("DESC", descricao);
            att.Add("DESTINO", tipo_selecionado);
                                     */
                            }
                            else
                            {
                            pecas.Add(npc);
                            }


                        }
                    }

                    if(opt.Textos)
                    {

                        foreach (var s in this.GetMtexts().GroupBy(x => x.Text.Replace("*", "").Replace("\r", " ").Replace("\t", " ").Replace("\n"," ").TrimStart().TrimEnd().Split(' ')[0].Replace("(", "").Replace(")","")))
                        {
                           

                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto,s.Key,s.First().Text,s.ToList().Select(x=> new DB.Linha(new List<DB.Celula> { new DB.Celula("VALOR", x.Text) })).ToList());
                            pecas.Add(npc);

                        }
                        foreach (var s in this.GetTexts().GroupBy(x => x.TextString.Replace("*", "").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").TrimStart().TrimEnd().Split(' ')[0].Replace("(", "").Replace(")", "")))
                        {
                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto,s.Key, s.First().TextString, s.ToList().Select(x => new DB.Linha(new List<DB.Celula> { new DB.Celula("VALOR", x.TextString) })).ToList());
                            pecas.Add(npc);

                        }
                    }

                    pecas = pecas.FindAll(x => x.Nome != "").OrderBy(x=>x.ToString()).ToList().FindAll(
                        x=>
                        x.Nome!="DETALHE" &&
                        x.Nome !="PEÇA" &&
                        !x.Nome.Contains(".") &&
                        !x.Nome.Contains("@") &&
                        !x.Nome.Contains("$") &&
                        !x.Nome.Contains("+") &&
                        !x.Nome.Contains("?") &&
                        !x.Nome.Contains("%") &&
                        !x.Nome.Contains("*") &&
                        !x.Nome.Contains("3D_INFO") &&
                        !x.Nome.Contains("SELO") &&
                        !x.Nome.Contains("EIXO") &&
                        !x.Nome.Contains("NOTA") &&
                        !x.Nome.Contains("SOLUÇÃO") &&
                        !x.Nome.Contains("FACHADA") &&
                        !x.Nome.Contains("#")
                        );

                    
                    if(pecas.Count>0)
                    {
                        List<PCQuantificar> pcs = Conexoes.Utilz.SelecionarObjetos(new List<PCQuantificar> { }, pecas, "Determine quais peças deseja que apareçam na tabela");


                        Menus.Quantificar_Menu_Configuracao mm = new Menus.Quantificar_Menu_Configuracao(pcs);

                        mm.Show();

                        mm.Closed += FinalizaInsercao;
                        
                    }

                }

                
            }
        }

        private void FinalizaInsercao(object sender, EventArgs e)
        {
            Menus.Quantificar_Menu_Configuracao mm = sender as Menus.Quantificar_Menu_Configuracao;
            List<PCQuantificar> pcs = mm.original;
            if (mm.confirmado)
            {
                pcs = mm.filtro;
            }

            if (pcs.Count > 0)
            {
                bool cancelado = false;
                var pt = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                if (!cancelado)
                {
                    Tabelas.Pecas(pcs, pt, 0);
                }
            }

        }

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
     
            try
            {
                if (!E_Tecnometal3D()) { return; }

                editor.Command("tec_stsetvar3d", selecao.ObjectId, secao, propriedade, valor);

            }
            catch (Exception ex)
            {
                AddMensagem($"\n{ex.Message}\n{ex.StackTrace}");
            }
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


            if(cfg.DXFs_de_CAMs)
            {
                GerarDXFs();
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
                    Document docToWorkOn = CAD.documentManager.Open(drawing, false);

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
                    foreach(var pos in posicoes)
                    {
                        pos.Pai = marca;
                    }


                    Retorno.Add(marca);
                }
                else
                {
                    erros.Add(new Conexoes.Report("Marcas duplicadas", $" {marcas[0].Prancha} - M: {m}"));
                    return new List<MarcaTecnoMetal>();
                }

            }


            return Retorno;

        }
        public List<MarcaTecnoMetal> GetMarcasCompostas()
        {
            return this.GetMarcas().FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaComposta).ToList();
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


        public void GerarDXFs(List<DLMCam.ReadCam> cams = null)
        {
            if (!E_Tecnometal()) { return; }


            if(cams==null)
            {
                cams = GetCams();
               cams = Conexoes.Utilz.SelecionarObjetos(new List<DLMCam.ReadCam>(), cams);
            }

            if(cams.Count>0)
            {
                var dxfs = cams.Select(x => x.Pasta + x.Nome + ".dxf").ToList();
                Conexoes.Wait w = new Conexoes.Wait(dxfs.Count, "Apagando dxfs...");
                w.Show();

                foreach(var s in dxfs)
                {
                    Conexoes.Utilz.Apagar(s);
                    w.somaProgresso();
                }
                w.Close();


                Conexoes.Utilz.GerarDXF(cams.Select(x => x.Arquivo).ToList());

            }
        }

        public void InserirTabela()
        {

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
            IrLayout();
            ZoomExtend();

            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();

            Point3d pt = new Point3d();
            bool gerar_tabela = false;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                var ultima_edicao = System.IO.File.GetLastWriteTime(this.Arquivo).ToString("dd/MM/yyyy");
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                List<BlockReference> blocos = new List<BlockReference>();
                List<Line> linhas = new List<Line>();
                List<Entity> apagar = new List<Entity>();
                List<Autodesk.AutoCAD.DatabaseServices.DBText> textos = new List<Autodesk.AutoCAD.DatabaseServices.DBText>();
                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForWrite);
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
                    var pts = Utilidades.GetContorno(s, acTrans);
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
                    acTrans.Commit();
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


            if(erros.Count>0)
            {
                foreach(var erro in erros)
                {
                    AddMensagem($"\n{ erro.ToString()}");
                }
            }

     
        }


        public void PreencheSelo(bool limpar = false)
        {
            if (!E_Tecnometal()) { return; }
            IrLayout();
            ZoomExtend();

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                var ultima_edicao = System.IO.File.GetLastWriteTime(this.Arquivo).ToString("dd/MM/yyyy");

                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                List<BlockReference> blocos = new List<BlockReference>();
                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForWrite);
                    if (ent is BlockReference)
                    {
                        var s = ent as BlockReference;
                        blocos.Add(s);
                    }
                }

                List<BlockReference> tabela_tecno = Utilidades.Filtrar(blocos, new List<string> { "TECNOMETAL_TAB" }, false);
                List<BlockReference> selo = Utilidades.Filtrar(blocos, new List<string> { "SELO" }, false);

                var marcas = GetMarcas();

                var nomes_PECAS = marcas.Select(x => x.Marca).Distinct().ToList();

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

                    Atributos.Set(s, acTrans, att);

                }

                if (selo.Count > 0)
                {
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
                else
                {
                    AddMensagem("\nNenhum Selo encontrado no desenho.");
                }
            }


        }
        public DB.Linha GetLinha(BlockReference bloco,  string arquivo, string nome, string ultima_edicao, Database acCurDb = null)
        {
            try
            {
                if(acCurDb==null)
                {
                    acCurDb = CAD.acCurDb;
                }
                var att = Atributos.GetLinha(bloco, acCurDb);
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
                att.Add(Constantes.ATT_DAT, ultima_edicao);
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
            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                var ultima_edicao = System.IO.File.GetLastWriteTime(this.Arquivo).ToString("dd/MM/yyyy");
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                List<BlockReference> blocos = new List<BlockReference>();
                foreach (ObjectId objId in acBlkTblRec)
                {
                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForRead);
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
                    marcas.Linhas.Add(GetLinha(m, this.Arquivo, this.Nome, ultima_edicao));
                }

                foreach (var m in pos)
                {
                    posicoes.Linhas.Add(GetLinha(m, this.Arquivo, this.Nome, ultima_edicao));
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
                    erros.Add(new Conexoes.Report("Erro", "Operação abortada - Nada Selecionado."));
                    return new DB.Tabela();
                }

                Conexoes.Wait w = new Conexoes.Wait(arquivos.Count(), "Carregando...");
                w.Show();
                foreach (FileInfo file in arquivos)
                {
                    w.somaProgresso($"Mapeando peças: {file.Name}");
                    var ultima_edicao = System.IO.File.GetLastWriteTime(file.FullName).ToString("dd/MM/yyyy");

                    var nome_arq = Conexoes.Utilz.getNome(file.FullName);
                    string arquivo = file.FullName;
                    try
                    {
                        using (Database acTmpDb = new Database(false, true))
                        {
                            acTmpDb.ReadDwgFile(arquivo, FileOpenMode.OpenForReadAndAllShare, false, null);
                            using (var acTrans = acTmpDb.TransactionManager.StartOpenCloseTransaction())
                            {

                                BlockTable acBlkTbl = acTrans.GetObject(acTmpDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                                List<BlockReference> blocos = new List<BlockReference>();
                                foreach (ObjectId objId in btr)
                                {
                                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForRead);
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
                                    marcas.Linhas.Add(GetLinha(m, arquivo, nome_arq, ultima_edicao, acTmpDb));
                                }

                                foreach (var m in pos)
                                {
                                    posicoes.Linhas.Add(GetLinha(m, arquivo, nome_arq, ultima_edicao, acTmpDb));
                                }

                            }
                            acTmpDb.CloseInput(true);
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
                    $"Tipo Bloco: {pos.ToList()[0].Tipo_Bloco}\n: " +
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


            var marcas_simples = marcas.FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaSimples);

            var posicoes = marcas.SelectMany(x => x.SubItens).ToList();
            var marcas_compostas = marcas.FindAll(x => x.Tipo_Marca != Tipo_Marca.MarcaSimples);
            var posicoes_perfis = posicoes.FindAll(x => x.TemCadastroDBF).ToList();
            var posicoes_elem_unit = posicoes.FindAll(x => x.Tipo_Bloco == Tipo_Bloco.Elemento_Unitario).ToList();
            var marcas_elemento_unit = marcas.FindAll(x => x.Tipo_Bloco == Tipo_Bloco.Elemento_Unitario).ToList();
            var perfis = posicoes_perfis.GroupBy(x=>x.Perfil.ToUpper().TrimStart().TrimEnd()).ToList();
            var mercs = marcas.GroupBy(x => x.Mercadoria);
            var mats = posicoes.FindAll(x => x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).GroupBy(x => x.Material);
            var posicoes_grp = posicoes.FindAll(x=> x.Tipo_Bloco!= Tipo_Bloco.Arremate && x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).GroupBy(x => x.Posicao).ToList();
           
            erros.AddRange(marcas_compostas.FindAll(x => x.SubItens.Count == 0).Select(x => new Conexoes.Report("Marca Composta sem posições", $"{x.Prancha} - {x.Marca}", Conexoes.TipoReport.Crítico)));

            foreach (var pf in perfis)
            {
                var igual = Utilidades.GetdbTecnoMetal().Get(pf.Key);

                if(igual.Nome=="")
                {
                    erros.Add(new Conexoes.Report($"Perfil não cadastrado.", $"{pf.Key} \n{string.Join("\n", pf.ToList().Select(x =>  $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao} - Bloco: {x.NomeBloco}").Distinct().ToList())}", Conexoes.TipoReport.Crítico));
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

            erros.AddRange(marcas_elemento_unit.FindAll(x => !x.Marca.ToUpper().EndsWith("_A")).Select(x => new Conexoes.Report("Nome Inválido", $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}: Peças com elemento unitário devem ser marcadas com '_A' no fim.")));
            erros.AddRange(posicoes_elem_unit.FindAll(x => !x.Posicao.ToUpper().EndsWith("_A")).Select(x => new Conexoes.Report("Nome Inválido", $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}: Peças com elemento unitário devem ser marcadas com '_A' no fim.")));

            w.somaProgresso();
       

            foreach(var m in mercs)
            {
                var igual = Conexoes.DBases.GetMercadorias().Find(x => x.valor.ToUpper() == m.Key.ToUpper());
                if(igual ==null)
                {
                    erros.Add(new Conexoes.Report("Mercadoria em branco ou inválida", $"Mercadoria: {m.Key} \n Marcas:\n" + string.Join("\n", m.ToList().Select(x=> $"{x.Prancha} - [{x.Marca}]").Distinct().ToList()), Conexoes.TipoReport.Erro));
                }
            }
            w.somaProgresso();
            foreach (var m in mats)
            {
                var igual = Conexoes.DBases.GetMateriais().Find(x => x.valor.ToUpper() == m.Key.ToUpper());
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



        private string material_sel { get; set; }
        private string mercadoria_sel { get; set; }
        private Conexoes.Chapa chapa_sel { get; set; }
        private Conexoes.Bobina bobina_sel { get; set; }
        private List<Conexoes.Bobina> _bobinas { get; set; }
        private List<Conexoes.Chapa> _chapas { get; set; }
        private List<string> _materiais { get; set; }
        private List<string> _mercadorias { get; set; }
        public List<Conexoes.Bobina> GetBobinas()
        {
            if (_bobinas == null)
            {
                _bobinas = Conexoes.DBases.GetBancoRM().GetBobinas();
            }
            return _bobinas;
        }
        public List<Conexoes.Chapa> GetChapas()
        {
            if (_chapas == null)
            {
                _chapas = Conexoes.DBases.GetChapas();
            }
            return _chapas;
        }
        public List<string> GetMateriais()
        {
            if (_materiais == null)
            {
                _materiais = Conexoes.DBases.GetBancoRM().GetMateriais();
            }
            return _materiais;
        }
        public List<string> GetMercadorias()
        {
            if (_mercadorias == null)
            {
                _mercadorias = Conexoes.DBases.GetBancoRM().GetMercadorias();
            }
            return _mercadorias;
        }


        public Conexoes.Bobina PromptBobina(Conexoes.Chapa espessura = null)
        {
            List<Conexoes.Bobina> bobinas = new List<Conexoes.Bobina>();
            bobinas.AddRange(GetBobinas());
            if (espessura != null)
            {
                bobinas = bobinas.FindAll(x => x.Espessura == espessura.valor && x.Corte == espessura.bobina_corte);
            }
            var sel = Conexoes.Utilz.SelecionarObjeto(bobinas, null, "Selecione uma bobina");
            if (sel != null)
            {
                bobina_sel = sel;
            }

            return sel;
        }
        public Conexoes.Chapa PromptChapa(Tipo_Chapa tipo)
        {

            List<Conexoes.Chapa> chapas = new List<Conexoes.Chapa>();
            chapas.AddRange(GetChapas());
            if (tipo == Tipo_Chapa.Fina)
            {
                chapas = chapas.FindAll(x => x.GetChapa_Fina());
            }
            else if (tipo == Tipo_Chapa.Grossa)
            {
                chapas = chapas.FindAll(x => !x.GetChapa_Fina());
            }

            var sel = Conexoes.Utilz.SelecionaCombo(chapas, chapa_sel, "Selecione uma espessura");

            if (sel != null)
            {
                chapa_sel = sel;
            }
            return sel;
        }
        public string PromptMaterial()
        {
            string sel = Conexoes.Utilz.SelecionaCombo(GetMateriais(), material_sel, "Selecione o Material");
            if (sel != null)
            {
                material_sel = sel;
            }
            return sel;
        }
        public string PromptMercadoria()
        {
            var sel = Conexoes.Utilz.SelecionaCombo(GetMercadorias(), mercadoria_sel, "Selecione a Mercadoria");
            if (sel != null)
            {
                mercadoria_sel = sel;
            }
            return sel;
        }


        public void PromptGeometria( out double comprimento, out double largura, out double area, out double perimetro)
        {
            comprimento = 0;
            largura = 0;
            area = 0;
            perimetro = 0;

           var opcao = this.PerguntaString("Selecione", new List<string> { "Digitar", "Polyline" });

            if(opcao =="Polyline")
            {
                var sel = SelecionarObjetos();
                var pols = this.Getpolylinhas();
                if (pols.Count > 0)
                {
                    var pl = pols[0];
                    if (!pl.Closed)
                    {
                        Alerta("Polyline inválida. Somente polylines fechadas representando o contorno da chapa.");
                        return;
                    }
                    Utilidades.GetInfo(pl, out comprimento, out largura, out area, out perimetro);
                    comprimento = Math.Round(comprimento, 4);
                    largura = Math.Round(largura, 4);





                }
            }
            else if(opcao == "Digitar")
            {
                bool cancelado = false;
                comprimento = Math.Abs(Utilidades.PedirDistancia("Defina o comprimento", out cancelado));
                if(!cancelado)
                {
                    largura = Math.Abs(Utilidades.PedirDistancia("Defina a largura", out cancelado));

                    if(!cancelado)
                    {
                        perimetro = 2 * largura + 2 * comprimento;
                        area = largura * comprimento;
                    }
                }
            }

        }
        public  string PromptFicha()
        {
            return  Conexoes.Utilz.Prompt("Digite a ficha de pintura", "Ficha de pintura", "SEM PINTURA", true, "FICHA", false, 20);
        }
        public string PromptMarca(string prefix = "ARR-")
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



        public void InserirArremate(string marca = "", string posicao = "", int quantidade = 1, string ficha = null, Conexoes.Bobina bobina = null, bool chapa_fina = true, string mercadoria = null)
        {

            if (marca == "")
            {
                marca = PromptMarca("ARR-");
            }
            if (marca == null | marca == "") { return; }


            SelecionarObjetos();
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


                double comprimento = Utilidades.PedirDistancia("Defina o comprimento", out status);



                if (bobina == null)
                {
                   
                    bobina = Conexoes.DBases.GetBobinaDummy();
                    var espessura = PromptChapa(Tipo_Chapa.Fina);
                    if (espessura == null)
                    {
                        return;
                    }
                    chapa_fina = espessura.GetChapa_Fina();
                    if (chapa_fina)
                    {

                        bobina = PromptBobina(espessura);
                    }
                    else
                    {
                        bobina.Espessura = espessura.valor;
                        bobina.Material = "CIVIL 350";

                    }
                }

                if (bobina == null)
                {
                    return;
                }

                if (ficha == null)
                {
                    ficha = PromptFicha();
                }
                if (ficha == null)
                {
                    return;
                }


                if (mercadoria == null && posicao == "")
                {
                    mercadoria = PromptMercadoria();
                }

                if (mercadoria == null && posicao == "")
                {
                    return;
                }

                Chapa_Dobrada pa = new Chapa_Dobrada(bobina, corte, comprimento, 0, angulos) { Marca = marca, GerarCam = chapa_fina ? Opcao.Nao : Opcao.Sim, DescontarDobras = !chapa_fina, Ficha = ficha, Quantidade = (int)quantidade ,Mercadoria = mercadoria };
                //pa = Conexoes.Utilz.Propriedades(pa, out status);
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
                            if (this.E_Tecnometal(false))
                            {
                                destino = Conexoes.Utilz.CriarPasta(Conexoes.Utilz.getUpdir(destino), "CAM");
                            }
                            else
                            {
                                return;
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
        public void InserirChapa(string marca = "", string posicao = "", string material =null, int quantidade = 0, string ficha = null, Conexoes.Chapa espessura = null, string mercadoria = null)
        {
            if (marca == "")
            {
                marca = PromptMarca("CH-");
            }
            if (marca == null | marca == "") { return; }


            double comprimento = 0;
            double largura = 0;
            double area = 0;
            double perimetro = 0;
            this.PromptGeometria(out comprimento, out largura, out area, out perimetro);


         if(comprimento>0 && largura>0)
            {
                bool status = false;
                if (espessura == null)
                {
                    espessura = PromptChapa(Tipo_Chapa.Tudo);
                }
                if (espessura != null)
                {
                    Conexoes.Bobina bobina = Conexoes.Utilz.Clonar(Conexoes.DBases.GetBobinaDummy());
                    bool chapa_fina = espessura.GetChapa_Fina();
                    if (chapa_fina)
                    {

                        bobina = PromptBobina(espessura);
                        ficha = "SEM PINTURA";
                    }
                    else
                    {
                        if (material == null)
                        {
                            material = PromptMaterial();
                        }
                        bobina.Espessura = espessura.valor;
                        bobina.Material = material;
                        if (ficha == null)
                        {
                            ficha = PromptFicha();
                        }

                    }


                    if (mercadoria == null && posicao == "")
                    {
                        mercadoria = PromptMercadoria();
                    }

                    if (mercadoria == null && posicao == "")
                    {
                        return;
                    }


                    if (bobina != null)
                    {
                        if(material==null)
                        {
                            material = bobina.Material;
                        }
                        Chapa_Dobrada pa = new Chapa_Dobrada(bobina, largura, comprimento,area, new List<double>()) { Marca = marca, Ficha = ficha, GerarCam = Opcao.Nao,  Quantidade = quantidade, Mercadoria = mercadoria };

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
                                if (this.E_Tecnometal(false))
                                {
                                    destino = Conexoes.Utilz.CriarPasta(Conexoes.Utilz.getUpdir(destino), "CAM");
                                }
                                else
                                {
                                    return;
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
        public void InserirElementoUnitario(string marca = "", string posicao = "", double quantidade = 0, string mercadoria = null, Conexoes.RMA peca = null)
        {
            if (marca == "")
            {
                marca = PromptMarca("PC-");
            }
            if (marca == null | marca == "") { return; }
            
            if(peca==null)
            {
            peca = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione uma peça");
            }

            if (peca != null)
            {
                bool status = false;
            denovo:
                if(quantidade<=0)
                {
                    quantidade = Conexoes.Utilz.Prompt(peca.Multiplo, out status);
                    if (!status)
                    {
                        return;
                    }
                    if (quantidade <= 0 | !peca.MultiploOk(quantidade))
                    {
                        if (Conexoes.Utilz.Pergunta($"Valor setado [{quantidade} é inválido. Precisa ser maior que zero e múltiplo de {peca.Multiplo}\nTentar novamente?"))
                        {
                            goto denovo;
                        }
                        else
                        {
                            return;
                        }
                    }
                }


                if (mercadoria == null && posicao == "")
                {
                    mercadoria = PromptMercadoria();
                }

                if (mercadoria == null && posicao == "")
                {
                    return;
                }

                var origem = Utilidades.PedirPonto3D("Selecione a origem", out status);

                if (status)
                {
                    return;
                }

                Blocos.MarcaElemUnitario(origem, peca, quantidade, marca, this.Getescala(), posicao,mercadoria);
            }
        }
        public void InserirElementoM2(string marca = "", string posicao = "", string material =null, string ficha = null, int quantidade = 0, Conexoes.TecnoMetal_Perfil perfil = null, string mercadoria = null)
        {

            if (marca == "")
            {
                marca = PromptMarca("PC-");
            }
            if (marca == null | marca == "") { return; }



            double comprimento = 0;
            double largura = 0;
            double area = 0;
            double perimetro = 0;
            this.PromptGeometria(out comprimento, out largura, out area, out perimetro);


            


            if (comprimento > 0 && largura > 0)
            {
                if(material==null)
                {
                    material = PromptMaterial();
                }

                if(material!=null)
                {

                    if (quantidade == 0)
                    {
                        quantidade = Conexoes.Utilz.Prompt(1);
                    }

                    if (quantidade > 0)
                    {
                        if (perfil == null)
                        {
                            perfil = Conexoes.Utilz.SelecionarObjeto(Utilidades.GetdbTecnoMetal().GetPerfis(DLMCam.TipoPerfil.Chapa_Xadrez), null, "Selecione um perfil");
                        }

                        if (perfil != null)
                        {
                            if (ficha == null)
                            {
                                ficha = PromptFicha();
                            }

                            if (mercadoria == null && posicao == "")
                            {
                                mercadoria = PromptMercadoria();
                            }

                            if (mercadoria == null && posicao == "")
                            {
                                return;
                            }
                            bool status = true;
                            var ponto = Utilidades.PedirPonto3D("Selecione a origem do bloco", out status);

                            if (!status)
                            {
                                Blocos.MarcaElemM2(ponto, perfil, marca, quantidade, comprimento, largura, area, perimetro, ficha, material, this.Getescala(), posicao,mercadoria);
                            }
                        }
                    }
                }
            }

        }
        public void InserirPerfil(string marca = "", string posicao = "", string material = null, string ficha = null, int quantidade = 0, Conexoes.TecnoMetal_Perfil perfil = null, string mercadoria = null)
        {

            if (marca == "")
            {
                marca = PromptMarca("PF-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                bool status;
                double comprimento = Utilidades.PedirDistancia("Defina o comprimento", out status);

                comprimento = Math.Round(comprimento, 4);




                if (comprimento > 0 && !status)
                {
                    if (material == null)
                    {
                        material = PromptMaterial();
                    }
                    if (material != null)
                    {
                        if (quantidade == 0)
                        {
                            quantidade = Conexoes.Utilz.Prompt(1);
                        }




                        if (quantidade > 0)
                        {
                            if (perfil == null)
                            {
                                perfil = Conexoes.Utilz.SelecionarObjeto(Utilidades.GetdbTecnoMetal().GetPerfis(DLMCam.TipoPerfil.Chapa_Xadrez), null, "Selecione um perfil");
                            }


                            if(mercadoria==null && posicao=="")
                            {
                                mercadoria = PromptMercadoria();
                            }

                            if(mercadoria==null && posicao=="")
                            {
                                return;
                            }


                            if (perfil != null)
                            {
                                if (ficha == null)
                                {
                                    ficha = PromptFicha();
                                }

                                var ponto = Utilidades.PedirPonto3D("Selecione a origem do bloco", out status);

                                if (!status)
                                {
                                    Blocos.MarcaPerfil(ponto, marca, comprimento, perfil, quantidade, material, ficha, 0, 0, this.Getescala(), posicao,mercadoria);
                                }
                            }
                        }
                    }

                }
            }
        }
        public void InserirMarcaComposta(string nome, string mercadoria, double quantidade, string ficha)
        {
            bool cancelado = true;
            var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);

            if(cancelado)
            {
                return;
            }

            Blocos.MarcaComposta(origem, nome, quantidade, ficha, mercadoria, this.Getescala());

        }
        public MarcaTecnoMetal InserirMarcaComposta()
        {

            bool cancelado = true;
            var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);

            if (cancelado)
            {
                return null;
            }
            string nome = this.PromptMarca();
            if (nome != null && nome != "")
            {
                bool status = false;
                double quantidade = Conexoes.Utilz.Prompt(1, out status);
                if (status)
                {
                    string ficha = this.PromptFicha();
                    if (ficha != null)
                    {
                        string mercadoria = this.PromptMercadoria();
                        if (mercadoria != null && mercadoria != "")
                        {
                            Blocos.MarcaComposta(origem, nome, quantidade, ficha, mercadoria, this.Getescala());
                         

                            return this.GetMarcasCompostas().Find(x=>x.Marca == nome);
                        }
                    }
                }
            }
            return null;
        }

        public void Mercadorias()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var selecao = SelecionarObjetos();
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
                    acTrans.Commit();
                    editor.Regen();
                }
               
            }
        }




        public void Materiais()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var selecao = SelecionarObjetos();
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
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }

        public void Tratamentos()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {


                var selecao = SelecionarObjetos();

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
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public TecnoMetal()
        {
        }
    }
}
