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
        public void SetVar3D(Entity selecao, string secao = "PROFILEDATA", string propriedade = "DES_PEZ", string valor = "VIGA SOLDADA")
        {
            if (!E_Tecnometal3D()) { return; }
            editor.Command("tec_stsetvar3d", selecao.ObjectId,secao,propriedade, valor);
        }
        public void GerarDBF3D()
        {
            if (!E_Tecnometal3D()) { return; }
            var st = editor.Command("TEC_ST3D2DBF", this.Nome, "t","N","N");
        }
        public void RodarMacros(List<string> Arquivos = null, bool tabela = true, bool preenche_selo = true, bool extrair = true, bool lts = true)
        {
            if (!E_Tecnometal()) { return; }
            if (Arquivos == null)
            {
                Arquivos = SelecionarDWGs();
            }

            if (Arquivos.Count == 0)
            {
                Alerta("Nenhuma prancha DWG selecionada.");
            }


            if(extrair)
            {
              var s =  GerarDBF(null, Arquivos);

                if(!File.Exists(s.Banco))
                {
                    return;
                }
            }


            Conexoes.Wait w = new Conexoes.Wait(Arquivos.Count);
            w.Show();

            foreach (string drawing in Arquivos)
            {
                
                if (drawing.ToUpper() == this.Arquivo.ToUpper())
                {
                    if (tabela)
                    {
                        InserirTabelaAuto();
                    }
                }
                else
                {
                    Document docToWorkOn = documentManager.Open(drawing, false);
                  
                    using (docToWorkOn.LockDocument())
                    {
                        if (tabela)
                        {
                            InserirTabelaAuto();
                        }

                       if(lts)
                        {
                            SetLts();
                        }

                        if (preenche_selo)
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


        private Conexoes.SubEtapaTecnoMetal _subetapa { get; set; }
        private Conexoes.ObraTecnoMetal _obra { get; set; }
        private Conexoes.PedidoTecnoMetal _pedido { get; set; }
        public Conexoes.SubEtapaTecnoMetal GetSubEtapa()
        {
            if(_subetapa==null && Directory.Exists(this.Pasta))
            {
                _subetapa = new Conexoes.SubEtapaTecnoMetal(this.Pasta,null);
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

        public bool E_Tecnometal3D(bool mensagem = true)
        {
            if (!this.Pasta.ToUpper().EndsWith(@".S&G\"))
            {
                if (mensagem)
                {
                    Alerta($"Não é possível rodar esse comando fora de pastas de pedidos (.S&G)" +
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
            if (!this.Pasta.ToUpper().EndsWith(@".TEC\"))
            {
                if(mensagem)
                {
                    Alerta($"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                   $"\nPasta atual: {this.Pasta}");
                }
               
                return false;
            }
            else
            {
                return true;
            }
        }
        public void InserirTabela()
        {
            if (!E_Tecnometal()) { return; }

            bool cancelado = false;
            var pt = Utilidades.PedirPonto3D("Clique na origem", out cancelado);
            if(!cancelado)
            {
                var pcs = GetPecasPranchaAberta();

                if(pcs.Linhas.Count>0)
                {
                    Tabelas.TecnoMetal(pcs.Linhas, pt);
                }

            }
        }
        public void InserirTabelaAuto()
        {
            if (!E_Tecnometal()) { return; }
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

                if(apagar.Count>0)
                {
                    foreach (var s in apagar)
                    {
                        s.Erase(true);
                    }
                    tr.Commit();
                    acDoc.Editor.Regen();
                }
            }

            if(gerar_tabela)
            {
                var pcs = GetPecasPranchaAberta();

                if (pcs.Linhas.Count > 0)
                {
                    Tabelas.TecnoMetal(pcs.Linhas, pt, - 186.47);
                }
            }



        }
        public List<DB.Linha> GetMarcas(Database db, Transaction tr)
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
            List<BlockReference> tabela_tecno = Utilidades.Filtrar(blocos,Constantes.BlocosTecnoMetalMarcas, false);
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

                var marcas = GetMarcas(db, tr);

                var nomes_PECAS = marcas.Select(x => x.Get("MAR_PEZ").valor).Distinct().ToList();

                foreach (var s in selo)
                {
                    Hashtable att = new Hashtable();

                    if(limpar)
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
                   
                    Utilidades.SetAtributo(s, tr, att);

                }

                if (selo.Count > 0)
                {
                    tr.Commit();
                    acDoc.Editor.Regen();
                }
            }


        }
        public DB.Linha GetLinha(BlockReference bloco, Database db, string arquivo, string nome,DateTime ultima_edicao)
        {
            try
            {
                var att = Utilidades.GetAtributos(bloco, db);
                att.Add("ARQUIVO", arquivo);
                att.Add("NUM_COM", this.GetPedido().Codigo);
                att.Add("DES_COM", this.GetObra().Nome);
                att.Add("LOT_COM", this.GetSubEtapa().NomeEtapa);
                att.Add("NUM_DIS", nome);
                att.Add("FLG_DWG", nome);
                att.Add("FLG_REC", att.Get("POS_PEZ").ToString() == "" ? "03" : "04");
                att.Add("DAT_DIS", ultima_edicao.ToShortDateString());

                return att;
            }
            catch (Exception ex)
            {
                DB.Linha att = new DB.Linha();
                att.Add("BLOCO", nome);
                att.Add("ERRO", ex.Message);
                att.Add("ARQUIVO", arquivo);

                return att;
            }
        }
        public DB.Tabela GetPecasPranchaAberta(bool converter_padrao_dbf = true)
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
                    marcas.Linhas.Add(GetLinha(m, db,this.Arquivo,this.Nome, ultima_edicao));
                }

                foreach (var m in pos)
                {
                    posicoes.Linhas.Add(GetLinha(m, db, this.Arquivo, this.Nome, ultima_edicao));
                }

            }


            if (converter_padrao_dbf)
            {
                return GetPadraoDBF(marcas, posicoes);
            }
            else
            {
                var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
                return lista;
            }
        }
        public DB.Tabela GetPecas(List<string> pranchas = null, bool filtrar = true, bool converter_padrao_dbf = true)
        {

            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            try
            {
                List<FileInfo> arquivos = new List<FileInfo>();
                if (pranchas == null)
                {
                    pranchas = this.SelecionarDWGs();
                    if(pranchas.Count>0)
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
                    Alerta("Operação abortada - Nada Selecionado.");
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
                        Alerta(ex.Message + "\n" + ex.StackTrace);
                        return new DB.Tabela();
                    }

                }
                w.Close();
            }
            catch (System.Exception ex)
            {
            }
          

            if(converter_padrao_dbf)
            {
                return GetPadraoDBF(marcas, posicoes);
            }
            else
            {
                var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
                return lista;
            }
            
        }
        public DB.Tabela GerarDBF(string destino = null, List<string> pranchas = null)
        {
            if(!this.Pasta.ToUpper().EndsWith(@".TEC\"))
            {
                Alerta($"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                    $"\nPasta atual: {this.Pasta}");
                return new DB.Tabela();
            }

            var etapa = this.GetSubEtapa();
            if (destino == null | destino == "")
            {
            setar_nome:
                var nome_dbf = Conexoes.Utilz.RemoverCaracteresEspeciais(Conexoes.Utilz.Prompt("Digine o nome do arquivo", "", etapa.Nome, false, "", false, 16)).ToUpper();

                if (!nome_dbf.Contains("ETAPA_") | nome_dbf.Length < 7 | nome_dbf.Length>16)
                {
                    if (Conexoes.Utilz.Pergunta("Nome da DBF deve conter 'ETAPA_'" +
                        "\n Mínimo 7 caracteres." +
                        "\n Máximo 16 caracteres." +
                        "\n Tentar novamente?"))
                    {
                        goto setar_nome;
                    }
                    else
                    {
                        return new DB.Tabela();
                    }
                }
                destino = etapa.PastaDBF + nome_dbf + ".DBF";
            }

            if (!Conexoes.Utilz.Apagar(destino))
            {
                Alerta($"Não é possível substituir o arquivo atual: {destino}");
                return new DB.Tabela();
            }




            var lista_pecas = GetPecas(pranchas);
            if(lista_pecas.Linhas.Count==0)
            {
                return new DB.Tabela();
            }

            if (destino!="" && destino!=null && lista_pecas.Linhas.Count>0)
            {
               if(!Conexoes.Utilz.GerarDBF(lista_pecas, destino))
                {
                    lista_pecas.Banco = "";
                    return lista_pecas;
                }
            }
            lista_pecas.Banco = destino;
            return lista_pecas;
        }
        private DB.Tabela GetPadraoDBF(DB.Tabela marcas, DB.Tabela posicoes)
        {
            var lista = new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
            List<string> colunas = new List<string>();
            colunas.Add("FLG_REC");
            colunas.Add("NUM_COM");
            colunas.Add("DES_COM");
            colunas.Add("LOT_COM");
            colunas.Add("DLO_COM");
            colunas.Add("CLI_COM");
            colunas.Add("IND_COM");
            colunas.Add("DT1_COM");
            colunas.Add("DT2_COM");
            colunas.Add("NUM_DIS");
            colunas.Add("DES_DIS");
            colunas.Add("NOM_DIS");
            colunas.Add("REV_DIS");
            colunas.Add("DAT_DIS");
            colunas.Add("TRA_PEZ");
            colunas.Add("SBA_PEZ");
            colunas.Add("TIP_PEZ");
            colunas.Add("MAR_PEZ");
            colunas.Add("MBU_PEZ");
            colunas.Add("DES_PEZ");
            colunas.Add("POS_PEZ");
            colunas.Add("NOT_PEZ");
            colunas.Add("ING_PEZ");
            colunas.Add("QTA_PEZ");
            colunas.Add("QT1_PEZ");
            colunas.Add("MCL_PEZ");
            colunas.Add("COD_PEZ");
            colunas.Add("COS_PEZ");
            colunas.Add("NOM_PRO");
            colunas.Add("LUN_PRO");
            colunas.Add("LAR_PRO");
            colunas.Add("SPE_PRO");
            colunas.Add("MAT_PRO");
            colunas.Add("TIP_BUL");
            colunas.Add("DIA_BUL");
            colunas.Add("LUN_BUL");
            colunas.Add("PRB_BUL");
            colunas.Add("PUN_LIS");
            colunas.Add("SUN_LIS");
            colunas.Add("PRE_LIS");
            colunas.Add("FLG_DWG");

            DB.Tabela tab_pecas = new DB.Tabela();
            var grp_blocos = lista.Linhas.GroupBy(x => x.Get("MAR_PEZ").ToString()).ToList().ToList();
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
            var agrupado = tab_pecas.Linhas.GroupBy(x => x.Get("MAR_PEZ").ToString()).Select(x => x.ToList()).ToList();


            foreach (var m in agrupado)
            {
                //CRIA A LINHA DA MARCA SIMPLES
                if (m.Count() == 1)
                {

                    var p_simples = m[0].Clonar();
                    var m_simples = m[0].Clonar();

                    m_simples.Set("FLG_REC", "03");
                    m_simples.Set("POS_PEZ", "");
                    p_simples.Set("QTA_PEZ", 1);
                    m_simples.Set("COD_PEZ", "");
                    m_simples.Set("NOM_PRO", "");
                    m_simples.Set("LUN_PRO", "");
                    m_simples.Set("LAR_PRO", "");
                    m_simples.Set("SPE_PRO", "");
                    m_simples.Set("MAT_PRO", "");
                    m_simples.Set("MAT_PRO", "");
                    m_simples.Set("PUN_LIS", "");
                    m_simples.Set("SUN_LIS", "");
                    m_simples.Set("PRE_LIS", "");

                    p_simples.Set("FLG_REC", "04");
                    p_simples.Set("POS_PEZ", p_simples.Get("MAR_PEZ").valor);
                    l_marcas.Add(m_simples);
                    l_marcas.Add(p_simples);

                }
                else
                {
                    //junta posições iguais
                    var marca = m.FindAll(x => x.Get("POS_PEZ").valor == "");
                    if (marca.Count > 1)
                    {
                        Alerta($"Abortado:" +
                            $"\n{marca[0].Get("MAR_PEZ")}" +
                            $"\nMarca se encontra em mais de uma prancha." +
                            $"\nPranchas: \n" +
                            $"{string.Join("\n", marca.Select(x => x.Get("FLG_DWG")))}");
                        return new DB.Tabela();
                    }

                    var posicoes_tbl = m.FindAll(x => x.Get("POS_PEZ").valor != "").GroupBy(x => x.Get("POS_PEZ").valor).Select(X => X.ToList());
                    l_marcas.AddRange(marca);
                    foreach (var pos in posicoes_tbl)
                    {
                        var lfim = pos[0].Clonar();
                        lfim.Set("QTA_PEZ", pos.Sum(x => x.Get("QTA_PEZ").Double()));
                        l_marcas.Add(lfim);
                    }
                }

            }
            foreach (var l in l_marcas)
            {
                l.Descricao = l.Get("MAR_PEZ") + " - P = " + l.Get("POS_PEZ");
            }

            //ordena as peças, colocando as marcas antes das posições
            l_marcas = l_marcas.OrderBy(x => x.Get("FLG_REC").valor.PadLeft(2, '0') + x.Descricao).ToList();

            DB.Tabela lista_convertida = new DB.Tabela();
            lista_convertida.Linhas.AddRange(l_marcas);

            return lista_convertida;
        }
        public TecnoMetal()
        {

        }
    }
}
