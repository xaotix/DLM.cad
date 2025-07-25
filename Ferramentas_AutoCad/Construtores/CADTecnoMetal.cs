﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Conexoes;
using DLM.desenho;
using DLM.encoder;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static DLM.cad.CAD;

namespace DLM.cad
{
    public class CADTecnoMetal : CADBase
    {
        private Conexoes.SubEtapaTecnoMetal _subetapa { get; set; }
        private Conexoes.ObraTecnoMetal _obra { get; set; }
        private Conexoes.PedidoTecnoMetal _pedido { get; set; }

        private string _Mercadoria_sel { get; set; }
        private Conexoes.Bobina _Chapa_sel { get; set; }
        private Conexoes.Bobina _Bobina_sel { get; set; }
        private List<Conexoes.Bobina> _Chapas { get; set; }
        private List<string> _Materiais { get; set; }


        public List<MarcaTecnoMetal> Getposicoes(ref List<Report> erros, bool update)
        {
            var marcas = GetMarcas(ref erros);
            var pos = marcas.SelectMany(x => x.GetPosicoes()).GroupBy(x => x.Nome_Posicao).Select(x => x.First()).ToList();
            return pos;
        }
        public List<Conexoes.Filete> InserirSoldaComposicao()
        {
            var retorno = new List<Conexoes.Filete>();
            var erros = new List<Report>();

            var pos = Getposicoes(ref erros, true);
            var pos_soldados_desmembrados = pos.FindAll(x => !x.Nome_Posicao.Contains("_")).FindAll(y => y.GetPerfil().Familia == DLM.vars.CAM_FAMILIA.Soldado).ToList();

            var montar_desmembrado = pos.FindAll(x =>
            x.Nome_Posicao.GetTipoDesmembrado() == CAM_TIPO_DESMEMBRADO.Alma |
            x.Nome_Posicao.GetTipoDesmembrado() == CAM_TIPO_DESMEMBRADO.Mesa_S |
            x.Nome_Posicao.GetTipoDesmembrado() == CAM_TIPO_DESMEMBRADO.Mesa_I
            );
            var marcas_desmembrados = montar_desmembrado.GroupBy(x => x.Nome_Posicao.Substring(0, x.Nome_Posicao.Length - 2));


            foreach (var m in marcas_desmembrados)
            {
                var almas = m.ToList().FindAll(x => x.Nome_Posicao.GetTipoDesmembrado() == CAM_TIPO_DESMEMBRADO.Alma).ToList();
                var resto = m.ToList().FindAll(x => almas.Find(y => y.Nome_Posicao == x.Nome_Posicao) == null).ToList();

                if (almas.Count > 0 && resto.Count > 0)
                {
                    var esp_m = resto.Max(x => x.Espessura);
                    var esp_alm = almas.Max(x => x.Espessura);
                    var altura = almas.Max(x => x.Largura) + (2 * esp_m);
                    var mesa = resto.Max(x => x.Largura);


                    var cmp = DBases.GetSoldaComposicao().Get(esp_m, esp_alm, altura, mesa, false).Clonar();
                    cmp.Perfil = DLM.cam.Perfil.I_Soldado("", altura, mesa, mesa, esp_m, esp_m, esp_alm);
                    cmp.Nome_Pos = m.Key;
                    retorno.Add(cmp);
                }
                else
                {
                    erros.Add(new Report("Não foi possível montar o perfil. não há mesas / almas suficientes", m.Key, TipoReport.Critico));
                }
            }


            var perfis = pos_soldados_desmembrados.GroupBy(x => x.Perfil).ToList().FindAll(x => x.Key.Replace(" ", "") != "");
            foreach (var pf in perfis)
            {
                var pp = pf.First().GetPerfil();
                if (pp.Familia == DLM.vars.CAM_FAMILIA.Soldado)
                {

                    var cmp = DBases.GetSoldaComposicao().Get(pp.Esp_M, pp.Esp, pp.Altura, pp.Aba, false);
                    foreach (var p in pf.ToList())
                    {
                        var np = cmp.Clonar();
                        np.Perfil = pp;
                        np.Nome_Pos = p.Nome_Posicao;
                        retorno.Add(np);
                    }
                }

                else if (pp.Familia == DLM.vars.CAM_FAMILIA._Desconhecido)
                {
                    erros.Add(new Report("Perfil não encontrado.", $"{pf.Key} {pp.Descricao}", TipoReport.Critico));
                }
            }





            erros.Show();

            if (retorno.Count > 0)
            {
                acDoc.IrLayout();
                Ut.ZoomExtend();
                Menus.SoldaComposicao mm = new Menus.SoldaComposicao();
                mm.lista.ItemsSource = retorno;

                mm.ShowDialog();
                if ((bool)mm.DialogResult)
                {
                    double largura = 63.69;
                    var filetes = retorno.GroupBy(x => x.ToString()).ToList();
                    bool cancelado = false;
                    var pt = Ut.PedirPonto("Selecione a origem", out cancelado);
                    var origem = pt.Mover(-largura, 0, 0);
                    if (!cancelado)
                    {
                        foreach (var filete in filetes)
                        {
                            string nome = Cfg.Init.CAD_BL_Solda_1;
                            if (filete.First().Filete_Duplo)
                            {
                                nome = Cfg.Init.CAD_BL_Solda_2;
                            }
                            /*agrupa as posições de 1 em 1 para inserir o bloco*/
                            var pcs = filete.ToList().Select(x => x.Nome_Pos).ToList().Quebrar(1).Select(x => string.Join(",", x)).ToList();

                            foreach (var pc in pcs)
                            {
                                var ht = new db.Linha();
                                ht.Add("MBPERFIL", pc);
                                ht.Add("MBFILETE", filete.First().Filete_Minimo);
                                Blocos.Inserir(acDoc, nome, origem, 1, 0, ht);

                                origem = origem.Mover(-largura, 0, 0);
                            }

                        }
                    }

                }
            }
            else
            {
                Conexoes.Utilz.Alerta("Nenhum perfil soldado encontrado no desenho.");
            }






            return retorno;
        }
        public void GerarPDF(List<Conexoes.Arquivo> arqs = null, bool gerar_dxf = false)
        {
            List<Conexoes.Arquivo> arquivos = new List<Conexoes.Arquivo>();
            int pranchas_por_page_setup = 50;
            string config_layout = "PDF-A3-PAISAGEM";
            string config_model = "PDF_A3_PAISAGEM";
            string arquivo_dwg = DLM.vars.Cfg.Init.DIR_RAIZ_BINARIOS_REDE + @"Lisps\Plot_pdf\CFG\Selo_2017.dwg";

            if (!File.Exists(arquivo_dwg))
            {
                Conexoes.Utilz.Alerta($"Abortado.\nArquivo de pagesetup não encontrado: {arquivo_dwg}");
                return;
            }



            if (arqs == null)
            {
                arqs = this.SelecionarDWGs(true);

            }
            else
            {
                arquivos.AddRange(arqs);
            }


            if (gerar_dxf)
            {
                arquivos.AddRange(GerarDXFs());
            }

            if (arquivos.Count == 0)
            {
                return;
            }


            string destino = this.Pasta + @"PDF";

            string pasta_dsd = destino + @"\DSD";

            try
            {
                if (!Directory.Exists(destino))
                {
                    Directory.CreateDirectory(destino);
                }

                if (!Directory.Exists(pasta_dsd))
                {
                    Directory.CreateDirectory(pasta_dsd);
                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
                return;
            }


            /*
             Value	PLOT	    PUBLISH
              0	    Foreground	Foreground
              1	    Background	Foreground
              2	    Foreground	Background
              3	    Background	Background
             */
            Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("BACKGROUNDPLOT", 0);
            var dsds = pasta_dsd.GetArquivos("*.dsd");
            foreach (var arq in dsds)
            {
                if (!arq.Delete())
                {
                    return;
                }
            }


            if (arquivos.Count == 0)
            {
                return;
            }
            var pacotes = arquivos.Quebrar(pranchas_por_page_setup);


            int c = 1;


            List<string> arquivos_dsd = new List<string>();
            Core.Getw().SetProgresso(1, arquivos.Count, $"Gerando PDF (Pacote) {c}/{pacotes.Count}");
            foreach (var pacote in pacotes)
            {
                string arquivo_dsd = pasta_dsd + $@"\Plotagem_{c}.dsd";
                try
                {
                    if (!arquivo_dsd.Delete())
                    {
                        return;
                    }

                    DsdData dsdData = new DsdData();

                    DsdEntryCollection collection = new DsdEntryCollection();
                    /*todo = adicionar tela para configurar qual layout o usuário quer gerar*/
                    foreach (var arquivo in pacote)
                    {
                        string extensao = arquivo.Extensao;
                        try
                        {
                            var pdf = destino + @"\" + arquivo.Nome + ".PDF";
                            if (!pdf.Delete())
                            {
                                Core.Getw().Close();
                                return;
                            }
                            string nome = "";
                            if (extensao.ToUpper().EndsWith("DXF"))
                            {
                                nome = "Model";
                            }
                            else if (arquivo.Endereco == this.Endereco)
                            {


                                var layouts = acDoc.GetLayouts();
                                if (layouts.Count > 0)
                                {
                                    nome = layouts[0].LayoutName;
                                }
                            }
                            else
                            {
                                Database db = new Database(false, true);
                                db.ReadDwgFile(arquivo.Endereco, System.IO.FileShare.Read, true, "");

                                using (var acTrans = db.acTransST())
                                {
                                    var layouts = Ut.getLayoutIds(db);
                                    if (layouts.Count > 0)
                                    {
                                        Layout layout = acTrans.GetObject(layouts[0], OpenMode.ForRead) as Layout;
                                        nome = layout.LayoutName;
                                    }
                                    acTrans.Commit();
                                }
                            }

                            if (nome != "")
                            {
                                collection.Add(GetEntradaDSD(arquivo.Endereco, nome));
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Conexoes.Utilz.Alerta(ex, $"Erro ao tentar ler o arquivo {arquivo}");
                        }


                        Core.Getw().somaProgresso();
                    }



                    dsdData.SheetType = SheetType.SinglePdf;
                    dsdData.NoOfCopies = 1;
                    dsdData.ProjectPath = destino;
                    dsdData.LogFilePath = pasta_dsd + $@"\Plotagem_{c}.log";
                    dsdData.SheetSetName = @"PublisherSet";

                    dsdData.SetDsdEntryCollection(collection);








                    dsdData.WriteDsd(arquivo_dsd);


                    /*tem q fazer essa gambiarra pra setar o pagesetup*/
                    var txt = Conexoes.Utilz.Arquivo.Ler(arquivo_dsd);
                    for (int i = 0; i < txt.Count; i++)
                    {
                        string setup = $@"PDF-A3-PAISAGEM|{arquivo_dwg}";
                        if (txt[i].Contains("Setup="))
                        {
                            string pagesetup = $@"{config_layout}|{arquivo_dwg}";
                            var opt = txt[i - 1];
                            if (opt.ToUpper().Contains("MODEL"))
                            {
                                pagesetup = $@"{config_model}|{arquivo_dwg}";
                            }
                            txt[i] = $"Setup={pagesetup}";
                        }
                    }
                    Conexoes.Utilz.Arquivo.Gravar(arquivo_dsd, txt, null, true);

                    arquivos_dsd.Add(arquivo_dsd);
                }
                catch (Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                c++;
            }

            Core.Getw().SetProgresso(1, arquivos_dsd.Count, "Gerando PDFs...");
            foreach (var arquivo_dsd in arquivos_dsd)
            {
                PlotConfig plotConfig = Autodesk.AutoCAD.PlottingServices.PlotConfigManager.SetCurrentConfig("DWG To PDF.pc3");

                Autodesk.AutoCAD.Publishing.Publisher publisher = Autodesk.AutoCAD.ApplicationServices.Application.Publisher;

                DsdData dsdData = new DsdData();
                dsdData.ReadDsd(arquivo_dsd);

                dsdData.SheetType = SheetType.SinglePdf;
                dsdData.NoOfCopies = 1;
                dsdData.ProjectPath = destino;
                dsdData.DestinationName = destino + @"\arquivo.pdf";
                publisher.PublishExecute(dsdData, plotConfig);

                Core.Getw().somaProgresso();
            }


            Core.Getw().Close();
            Conexoes.Utilz.Alerta("Plotagem finalizada!", "Finalizado", System.Windows.MessageBoxImage.Information);


        }

        private static DsdEntry GetEntradaDSD(string arquivo, string layout)
        {

            DsdEntry entry = new DsdEntry();
            entry.DwgName = arquivo;
            entry.Layout = layout;
            entry.Title = arquivo.getNome();
            entry.NpsSourceDwg = "";
            entry.Nps = "";
            return entry;
        }
        public int MapearPCsTecnoMetal(int seq, int arredon, bool subs_bloco, List<CTV_de_para> perfis_mapeaveis, double escala, string arquivo_bloco, bool agrupar_proximos = true, bool contraventos = true, bool mapear_pecas = true)
        {
            this.SetEscala(escala);
            string nome_bloco = arquivo_bloco.getNome();

            List<PCQuantificar> pecas = this.Quantificar(false, false, false, false, true);
            if (pecas.Count == 0)
            {
                Conexoes.Utilz.Alerta($"Nenhuma peça mapeável encontrada na seleção.");
                return seq;
            }
            List<PCQuantificar> tirantes = new List<PCQuantificar>();
            List<PCQuantificar> outros = new List<PCQuantificar>();
            if (contraventos)
            {
                foreach (var pc in pecas)
                {
                    var nome = pc.Nome.ToUpper().Replace(" ", "");
                    var perfil = pc.Perfil.ToUpper().Replace(" ", "");
                    var igual = perfis_mapeaveis.Find(x => x.Perfil.ToUpper().Replace(" ", "") == perfil);

                    if (igual != null)
                    {
                        tirantes.Add(pc);
                        continue;
                    }


                    /*procura por marcas já definidas com o nome do perfil*/
                    igual = perfis_mapeaveis.Find(x => nome.StartsWith(x.Perfil.Replace("@", "")));

                    if (igual != null)
                    {
                        var ccmp = nome.Replace(igual.Perfil.Replace("@", ""), "").Double();

                        if (ccmp > 0)
                        {
                            pc.Perfil = igual.Perfil;
                            tirantes.Add(pc);
                            continue;
                        }
                    }
                    if (mapear_pecas)
                    {
                        outros.Add(pc);

                    }
                }
            }
            else
            {
                outros.AddRange(pecas);
            }

            outros = outros.OrderBy(x => x.Nome).ToList();

            List<PCQuantificar> final_tabela = new List<PCQuantificar>();

            final_tabela.AddRange(tirantes);
            final_tabela.AddRange(outros);

            if (tirantes.Count > 0)
            {
                if (perfis_mapeaveis.Count == 0)
                {
                    Conexoes.Utilz.Alerta($"Não foi possível carregar o arquivo de configuração de contraventos. Contacte suporte. \n{Cfg.Init.CAD_Arquivo_CTV}");

                    return seq;
                }

                var tipos = tirantes.GroupBy(x => x.Perfil).ToList().OrderBy(x => x.Key).ToList();
                //List<Autodesk.AutoCAD.DatabaseServices.BlockReference> excluir = new List<Autodesk.AutoCAD.DatabaseServices.BlockReference>();

                Core.Getw().SetProgresso(1, tipos.Count);

                List<BlockAttributes> final = new List<BlockAttributes>();
                List<BlockAttributes> desagrupado = new List<BlockAttributes>();
                foreach (var tipo_por_perfil in tipos)
                {
                    Core.Getw().somaProgresso();
                    string perfil = tipo_por_perfil.Key;
                    var igual = perfis_mapeaveis.Find(x => x.Perfil.ToUpper().Replace(" ", "") == perfil.ToUpper().Replace(" ", ""));
                    var comps = tipo_por_perfil.ToList().GroupBy(x => x.Comprimento.ArredondarMultiplo(arredon)).ToList().OrderBy(x => x.Key).ToList();
                    foreach (var comp in comps)
                    {
                        string numero = seq.String(2);
                        string familia = "TIRANTE " + igual.PecaLiberar.Replace("$C$", "");
                        string codigo = igual.PecaLiberar.Replace("$C$", comp.Key.String(0, igual.CaractComp));
                        var pedacos = comp.Key.Quebrar(6000, 600, 0);
                        string descricao = string.Join(" ", pedacos.GroupBy(x => x).ToList().Select(x => "(" + x.Key + " " + x.Count().ToString() + "x)"));


                        var atuais = comp.ToList();

                        List<BlockAttributes> bl_agrupados = new List<BlockAttributes>();

                        var blocos_atuais = atuais.SelectMany(x => x.Blocos).ToList();
                        desagrupado.AddRange(blocos_atuais);


                        if (agrupar_proximos)
                        {
                            foreach (var bloco in blocos_atuais)
                            {
                                bloco.Descricao = codigo;
                                var bl_ja_adicionados = bl_agrupados.SelectMany(x => x.Filhos).ToList();
                                bl_ja_adicionados.AddRange(bl_agrupados);

                                if (bl_ja_adicionados.Find(x => x.Block.Id == bloco.Block.Id) == null)
                                {
                                    var bl_a_adicionar = blocos_atuais.FindAll(x => bl_ja_adicionados.Find(y => y.Block.Id == x.Block.Id) == null).ToList();
                                    var iguais = bl_a_adicionar.FindAll(x => x.GetCoordenada().Distancia(bloco.GetCoordenada().Origem) <= escala * 5);
                                    bloco.Filhos = iguais.FindAll(x => x.Block.Id != bloco.Block.Id);
                                    bl_agrupados.Add(bloco);
                                }
                            }
                        }
                        else
                        {
                            bl_agrupados.AddRange(blocos_atuais);
                        }


                        final.AddRange(bl_agrupados);


                        foreach (var s in bl_agrupados)
                        {

                            var att = new db.Linha();
                            att.Add(Cfg.Init.CAD_ATT_N, numero);
                            att.Add(Cfg.Init.CAD_ATT_Familia, familia);
                            att.Add(Cfg.Init.CAD_ATT_Tipo, Cfg.Init.CAD_ATT_TECNOMETAL);
                            att.Add(Cfg.Init.CAD_ATT_Comprimento, comp.Key);
                            att.Add(Cfg.Init.CAD_ATT_Codigo, codigo);
                            att.Add(Cfg.Init.CAD_ATT_id, 0);
                            att.Add(Cfg.Init.CAD_ATT_Descricao, string.Join(" ", descricao));
                            att.Add(Cfg.Init.CAD_ATT_Destino, Cfg.Init.CAD_ATT_RME);
                            att.Add(Cfg.Init.CAD_ATT_Quantidade, 1 + s.Filhos.Count);
                            if (subs_bloco)
                            {
                                Blocos.Inserir(acDoc, arquivo_bloco, s.Block.Position.P3d(), escala, 0, att);
                            }
                            else
                            {
                                var angulo = s.GetAngulo();
                                Blocos.Inserir(acDoc, Cfg.Init.CAD_BL_INDICACAO_TXT, s.Block.Position.P3d(), escala, angulo, att);
                            }
                        }


                        using (var acTrans = acDoc.acTransST())
                        {
                            foreach (var s in atuais)
                            {
                                s.Nome = codigo;
                                s.Descricao = descricao;
                                s.Numero = numero;
                                s.Nome_Bloco = nome_bloco;
                                s.Familia = familia;
                                s.Destino = Cfg.Init.CAD_ATT_RME;
                                s.Perfil = perfil;
                                var att = new db.Linha();
                                //att.Add("LUN_PRO", comp);
                                att.Add("MARK", codigo);
                                att.Add(T_DBF1.MAR_PEZ.ToString(), codigo);
                                Atributos.Set(s.Blocos.Select(x => x.Block).ToList().ToList(), acTrans, att);
                                Atributos.Set(s.Filhos_Ignorar.SelectMany(x => x.Blocos).Select(x => x.Block).ToList().ToList(), acTrans, att);
                            }

                            acTrans.Commit();
                        }

                        seq++;
                    }

                }

                editor.Regen();




                Core.Getw().Close();

            }

            if (outros.Count > 0 && mapear_pecas)
            {
                var tot = outros.Sum(x => x.Blocos.Count);
                Core.Getw().SetProgresso(1, tot, $"Inserindo {tot} blocos de outras peças");
                foreach (var pc in outros)
                {
                    pc.Numero = seq.String(2);
                    seq++;
                    foreach (var s in pc.Blocos)
                    {
                        Core.Getw().somaProgresso();

                        var ht = new db.Linha();
                        ht.Add(Cfg.Init.CAD_ATT_N, pc.Numero);
                        ht.Add(Cfg.Init.CAD_ATT_Familia, pc.Familia);
                        ht.Add(Cfg.Init.CAD_ATT_Tipo, Cfg.Init.CAD_ATT_TECNOMETAL);
                        ht.Add(Cfg.Init.CAD_ATT_Comprimento, pc.Comprimento.String(0));
                        ht.Add(Cfg.Init.CAD_ATT_Codigo, pc.Nome);
                        ht.Add(Cfg.Init.CAD_ATT_id, 0);
                        ht.Add(Cfg.Init.CAD_ATT_Descricao, pc.Descricao);
                        ht.Add(Cfg.Init.CAD_ATT_Destino, pc.Destino);
                        ht.Add(Cfg.Init.CAD_ATT_Quantidade, 1 + s.Filhos.Count);

                        Blocos.Inserir(acDoc, Cfg.Init.BlocosIndicacao()[0].Endereco, s.Block.Position.P3d(), escala, 0, ht);
                    }

                }
                Core.Getw().Close();
            }

            if (Conexoes.Utilz.Pergunta("Mapeamento finalizado! Deseja inserir a tabela?"))
            {
                InserirTabelaQuantificacao(final_tabela);
            }

            CAD.editor.Regen();

            return seq;
        }
        public List<PCQuantificar> Quantificar(bool gerar_tabela = true, bool configurar = true, bool blocos = true, bool textos = true, bool tecnometal = true)
        {
            List<PCQuantificar> pecas = new List<PCQuantificar>();

            var sel = SelecionarObjetos(CAD_TYPE.INSERT, CAD_TYPE.TEXT, CAD_TYPE.MTEXT, CAD_TYPE.LEADER);
            if (sel.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK && this.Selecoes.Count > 0)
            {
                var opt = new ConfiguracaoQuantificar();
                opt.Blocos = blocos;
                opt.Textos = textos;
                opt.Pecas_TecnoMetal = tecnometal;
                if (configurar)
                {
                    configurar = opt.Propriedades();
                }
                else
                {
                    configurar = true;
                }

                if (configurar)
                {
                    if (opt.Blocos | opt.Pecas_TecnoMetal)
                    {
                        List<PCQuantificar> blocos_montagem_tecnometal = new List<PCQuantificar>();
                        foreach (var s in Selecoes.Filter<BlockReference>().FindAll(x => !
                         x.Name.Contains("*"))
                        .GroupBy(x => x.Name.ToUpper()
                        .Replace("SUPORTE_", "")
                        .Replace("SUPORTE ", "")
                        ))
                        {
                            var att = s.First().GetAttributes();

                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Bloco, s.Key.ToUpper(), "", s.Key.ToUpper(), s.ToList().Select(x => x.GetAttributes()).ToList());
                            if (npc.Nome.StartsWith(Cfg.Init.CAD_PC_Quantificar))
                            {
                                var blcs = npc.Agrupar(new List<string> { "CODIGO", Cfg.Init.CAD_ATT_N }, npc.Nome_Bloco);
                                foreach (var bl in blcs)
                                {
                                    bl.SetDescPorAtributo(Cfg.Init.CAD_ATT_Descricao);
                                    bl.SetNumeroPorAtributo(Cfg.Init.CAD_ATT_N);
                                    bl.SetDestinoPorAtributo("DESTINO");
                                    bl.SetQtdPorAtributo(Cfg.Init.CAD_ATT_Quantidade);
                                    bl.SetIdPorAtributo(Cfg.Init.CAD_ATT_id);
                                    bl.SetFamiliaPorAtributo(Cfg.Init.CAD_ATT_Familia);
                                }

                                pecas.AddRange(blcs);

                            }
                            else if (npc.Nome == Cfg.Init.CAD_Bloco_3D_Montagem_Info)
                            {
                                if (opt.Pecas_TecnoMetal)
                                {
                                    var blcs = npc.Agrupar(new List<string> { T_DBF1.MAR_PEZ.ToString() }, npc.Nome_Bloco);
                                    foreach (var bl in blcs)
                                    {
                                        bl.SetPerfilPorAtributo(T_DBF1.NOM_PRO.ToString());
                                        bl.SetCompPorAtributo(T_DBF1.LUN_PRO.ToString());
                                        bl.SetMaterialPorAtributo(T_DBF1.MAT_PRO.ToString());
                                        bl.SetDescPorAtributo(T_DBF1.NOM_PRO.ToString());
                                    }
                                    blocos_montagem_tecnometal.AddRange(blcs);
                                }


                            }
                            else if (npc.Nome == Cfg.Init.CAD_Bloco_3D_Montagem_Tecnometal)
                            {
                                if (opt.Pecas_TecnoMetal)
                                {
                                    var blcs = npc.Agrupar(new List<string> { "MARK" }, npc.Nome_Bloco);
                                    blocos_montagem_tecnometal.AddRange(blcs);
                                }
                            }
                            else if (opt.Blocos)
                            {
                                pecas.Add(npc);
                            }
                        }


                        var blk_tec = blocos_montagem_tecnometal.GroupBy(x => x.Nome);
                        foreach (var bl in blk_tec)
                        {
                            //nesse segmento, ignoro o bloco repetido que dá os dados da peça e crio 1 objeto novo, quantificando pela quantidade do bloco que contém somente a marca
                            var blocs = bl.ToList().FindAll(x => x.Nome_Bloco == Cfg.Init.CAD_Bloco_3D_Montagem_Tecnometal);
                            if (blocs.Count > 0)
                            {
                                var p = blocs[0];
                                var p_filhos_infos = bl.ToList().FindAll(x => x.Nome_Bloco == Cfg.Init.CAD_Bloco_3D_Montagem_Info);

                                var pf = new PCQuantificar(Tipo_Objeto.Bloco);

                                if (p_filhos_infos.Count > 0)
                                {
                                    pf = p_filhos_infos[0];
                                }
                                PCQuantificar pc = new PCQuantificar(Tipo_Objeto.Bloco, bl.Key, pf.Descricao, p.Nome_Bloco, blocs.SelectMany(x => x.Blocos).ToList(), "", pf.Perfil, Cfg.Init.CAD_ATT_TECNOMETAL, pf.Perfil, pf.Material, pf.Comprimento);
                                pc.Descricao = pf.Descricao;
                                /*essa propriedade guarda os blocos que tem as sub-informações dos blocos no tecnometal*/
                                pc.Filhos_Ignorar = p_filhos_infos;
                                pecas.Add(pc);
                            }

                        }

                    }

                    if (opt.Textos)
                    {
                        var txt00 = Selecoes.Filter<MText>().GroupBy(x => x.Text
                        .TrimStart()
                        .TrimEnd()
                        .Replace("*", " ")
                        .Replace("@", " ")
                        .Replace(",", " ")
                        .Replace("(", " ")
                        .Replace(")", " ")
                        .Replace("\r", " ")
                        .Replace("\t", " ")
                        .Replace("\n", " ")
                        .Split(' ')[0]).ToList().FindAll(x => x.Key.Length > 0);

                        foreach (var s in txt00)
                        {



                            bool nao_adicionar = false;
                            List<string> ignorar = Cfg.Init.Ignorar();
                            foreach (var ign in ignorar)
                            {
                                if (s.Key.Contains(ign))
                                {
                                    nao_adicionar = true;
                                    break;
                                }
                            }

                            if (nao_adicionar)
                            {
                                continue;
                            }


                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto, s.Key, s.First().Text, "", s.ToList().Select(x => new BlockAttributes(new List<db.Celula> { new db.Celula("VALOR", x.Text) })).ToList());
                            pecas.Add(npc);

                        }
                        var txtss = Selecoes.Filter<DBText>().GroupBy(x => x.TextString.Replace("*", "").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").TrimStart().TrimEnd().Split(' ')[0].Replace("(", " ").Replace(")", " ")).ToList().FindAll(x => x.Key.Length > 0);
                        foreach (var s in txtss)
                        {
                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto, s.Key, s.First().TextString, "", s.ToList().Select(x => new BlockAttributes(new List<db.Celula> { new db.Celula("VALOR", x.TextString) })).ToList());
                            pecas.Add(npc);

                        }
                    }

                    pecas = pecas.FindAll(x => x.Nome != "").OrderBy(x => x.ToString()).ToList().FindAll(
                        x =>
                        x.Nome != "DETALHE" &&
                        x.Nome != "PEÇA" &&
                        !x.Nome.Contains(".") &&
                        !x.Nome.Contains("@") &&
                        !x.Nome.Contains("$") &&
                        !x.Nome.Contains("+") &&
                        !x.Nome.Contains("?") &&
                        !x.Nome.Contains("%") &&
                        !x.Nome.Contains("*") &&
                        !x.Nome.StartsWith("_") &&
                        !x.Nome.Contains("3D_INFO") &&
                        !x.Nome.Contains("SELO") &&
                        !x.Nome.Contains("CORTE") &&
                        !x.Nome.Contains("EIXO") &&
                        !x.Nome.Contains("REVISÃO") &&
                        !x.Nome.Contains("NOTA") &&
                        !x.Nome.Contains("SOLUÇÃO") &&
                        !x.Nome.Contains("FACHADA") &&
                        !x.Nome.Contains("#")
                        );


                    if (pecas.Count > 0 && gerar_tabela)
                    {
                        InserirTabelaQuantificacao(pecas);

                    }

                }


            }

            return pecas.OrderBy(x => x.Numero + " - " + x.Nome).ToList();
        }

        private void InserirTabelaQuantificacao(List<PCQuantificar> pecas)
        {
            var pcs = pecas.ListaSelecionarVarios(false, "Determine quais peças deseja que apareçam na tabela");


            Menus.Quantificar_Menu_Configuracao mm = new Menus.Quantificar_Menu_Configuracao(pcs);

            mm.Show();

            mm.Closed += InserirTabelaQuantificacao;
        }
        private void InserirTabelaQuantificacao(object sender, EventArgs e)
        {
            Menus.Quantificar_Menu_Configuracao mm = sender as Menus.Quantificar_Menu_Configuracao;

            if (mm.confirmado)
            {
                List<PCQuantificar> pcs = mm.original;
                if (mm.confirmado)
                {
                    pcs = mm.filtro;
                }

                if (pcs.Count > 0)
                {
                    bool cancelado = false;
                    var pt = Ut.PedirPonto("Selecione a origem", out cancelado);
                    if (!cancelado)
                    {
                        var separar = Conexoes.Utilz.Pergunta("Separar as peças e gerar tabelas por família?");
                        Tabelas.Pecas(pcs, separar, pt, 0);
                    }
                }
            }


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

                acDoc.Comando("tec_stsetvar3d", selecao.ObjectId, secao, propriedade, valor);

            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }
        }
        public void GerarDBF3D()
        {
            if (!E_Tecnometal3D()) { return; }
            acDoc.Comando("TEC_ST3D2DBF", this.Nome, "t", Cfg.Init.CAD_ATT_N, Cfg.Init.CAD_ATT_N);
        }


        //public List<MarcaTecnoMetal> GetMarcas(ref List<Report> erros)
        //{
        //    return GetBlocosMarcas(acCurDb, ref erros, acDoc.Name).GetMarcas(ref erros);
        //}

        public List<MarcaTecnoMetal> GetMarcas(ref List<Report> erros)
        {
            //if (!this.E_Tecnometal())
            //{
            //    return new List<MarcaTecnoMetal>();
            //}
            var pcs = GetBlocosMarcas(acCurDb, ref erros, acDoc.Name);
            var tbl = Conexoes.Utilz.DBF.ConverterParaDBF(pcs, ref erros);
            return tbl.GetMarcas(ref erros);
        }

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


        public List<Conexoes.Arquivo> GerarDXFs(List<DLM.cam.ReadCAM> cams = null)
        {
            if (!E_Tecnometal()) { return new List<Conexoes.Arquivo>(); }


            if (cams == null)
            {
                cams = new List<cam.ReadCAM>();
                cams.AddRange(this.GetSubEtapa().GetPacote().GetCAMsNaoRM());
            }

            if (cams.Count > 0)
            {
                var dxfs = this.GetSubEtapa().GetPacote().GetDXFsPastaCAM();
                Core.Getw().SetProgresso(1, dxfs.Count, $"Apagando dxfs... da pasta {this.GetSubEtapa().PastaCAM_Pedido}");

                foreach (var arq in dxfs)
                {
                    arq.Delete();
                    Core.Getw().somaProgresso();
                }
                Core.Getw().Close();


                Conexoes.Utilz.TecnoPlotGerarDXF(cams.Select(x => new Conexoes.Arquivo(x.Arquivo)).ToList());

            }
            return this.GetSubEtapa().GetPacote().GetDXFsPastaCAM();
        }
        public void InserirTabela(P3d pt = null)
        {

            bool cancelado = false;
            if (pt == null)
            {
                pt = Ut.PedirPonto("Clique na origem", out cancelado);
            }

            if (!cancelado)
            {
                var erros = new List<Report>();
                var pcs = GetMarcas(ref erros);
                if (pcs.Count > 0)
                {
                    if (erros.Count == 0)
                    {
                        Tabelas.TecnoMetal(pcs, pt, -186.47);
                    }
                    else
                    {
                        erros.Show();
                    }
                }
            }
        }
        public void ApagarTabelaAuto()
        {
            acDoc.IrLayout();
            Ut.ZoomExtend();
            CleanTabela();
        }

        public void InserirTabelaAuto()
        {
            List<Report> erros = new List<Report>();
            InserirTabelaAuto(ref erros);
            erros.Show();
        }
        public void InserirTabelaAuto(ref List<Report> erros)
        {
            acDoc.IrLayout();
            Ut.ZoomExtend();

            DLM.db.Tabela marcas = new DLM.db.Tabela();
            DLM.db.Tabela posicoes = new DLM.db.Tabela();



            P3d pt = CleanTabela();

            if (pt != null)
            {
                InserirTabela(pt);
            }


            if (erros.Count > 0)
            {
                foreach (var erro in erros)
                {
                    AddMensagem($"\n{erro.ToString()}");
                }
            }

            editor.Regen();

        }

        private P3d CleanTabela()
        {
            P3d retorno = null;
            List<BlockReference> blocos = new List<BlockReference>();
            using (var acTrans = acCurDb.acTransST())
            {

                var ultima_edicao = this.Endereco.getEdicao();
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                List<Line> linhas = new List<Line>();
                List<Entity> apagar = new List<Entity>();
                List<Autodesk.AutoCAD.DatabaseServices.DBText> textos = new List<Autodesk.AutoCAD.DatabaseServices.DBText>();
                foreach (ObjectId objId in acBlkTblRec)
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
                string errosb = "";
                apagar.AddRange(blocos.Filter(new List<string> { "TECNOMETAL_TAB" }, out errosb, false));



                var selo = blocos.Filter(new List<string> { "SELO" }, out errosb, false);
                foreach (var s in selo)
                {
                    var offset = -7.01;
                    var pts = Ut.GetPontos(s);
                    if (pts.Count > 0)
                    {
                        retorno = new P3d(pts.Max(x => x.X) + offset, pts.Max(x => x.Y) + offset);
                    }
                    else
                    {
                        var ptsb = s.Bounds.Value.MaxPoint;
                        retorno = new P3d(ptsb.X + offset, ptsb.Y + offset);
                    }


                    break;
                }





                foreach (var s in apagar)
                {
                    s.Erase(true);
                }
                acTrans.Commit();


            }

            return retorno;
        }

        public void PreencheSelo(bool limpar = false)
        {
            //if (!E_Tecnometal()) { return; }
            acDoc.IrLayout();
            Ut.ZoomExtend();

            List<Report> erros = new List<Report>();

            var marcas = GetMarcas(ref erros);

            var nomes_PECAS = marcas.Select(x => x.Nome).Distinct().ToList();

            using (var docLock = acDoc.LockDocument())
            {

                using (var acTrans = acDoc.acTransST())
                {

                    var ultima_edicao = this.Endereco.getEdicao();

                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite);
                    List<BlockReference> blocos = new List<BlockReference>();
                    foreach (ObjectId objId in acBlkTblRec)
                    {
                        Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForWrite);
                        if (ent is BlockReference)
                        {
                            var s = ent as BlockReference;
                            blocos.Add(s);
                        }
                    }
                    string errosb = "";
                    List<BlockReference> tabela_tecno = blocos.Filter(new List<string> { "TECNOMETAL_TAB" }, out errosb, false);
                    List<BlockReference> selo = blocos.Filter(new List<string> { "SELO" }, out errosb, false);


                    if (errosb.Length > 0)
                    {
                        erros.Add(new Report("Erro ao tentar ler os blocos", errosb, TipoReport.Critico));
                    }

                    foreach (var s in selo)
                    {
                        var ht = new db.Linha();

                        if (limpar)
                        {
                            ht.Add("TIPO_DE_PROJETO", "");
                            ht.Add("TITULO_DA_PRANCHA", "");
                            ht.Add("TÍTULO_DA_PRANCHA", "");
                            ht.Add("OBRA", "");
                            ht.Add("PREDIO", "");
                            ht.Add("CLIENTE", "");
                            ht.Add("LOCAL", "");
                            ht.Add("PEDIDO", "");
                            ht.Add("ETAPA", "");
                            ht.Add("ESCALA", ""); ;
                            ht.Add("UNIDADE", "");
                            ht.Add("COORDENAÇÃO", "");
                            ht.Add("COORDENACAO", "");
                            ht.Add("PROJETO", "");
                            ht.Add("DESENHO", "");
                            ht.Add("RESPONSAVEL_TECNICO", "");
                            ht.Add("CREA", "");
                        }
                        else
                        {
                            ht.Add("TIPO_DE_PROJETO", this.Nome.Contains(Cfg.Init.DWG_FAB_FILTRO) ? "PROJETO DE FABRICAÇÃO" : "PROJETO DE MONTAGEM");
                            ht.Add("TITULO_DA_PRANCHA", $"DETALHAMENTO {string.Join(", ", marcas.Select(x => x.Nome.ToUpper()))}");
                            ht.Add("TÍTULO_DA_PRANCHA", $"DETALHAMENTO {string.Join(", ", marcas.Select(x => x.Nome.ToUpper()))}");
                            ht.Add("OBRA", this.GetObra().Descricao.ToUpper());
                            ht.Add("PREDIO", this.GetSubEtapa().Predio.ToUpper());
                            ht.Add("CLIENTE", this.GetObra().Cliente.ToUpper());
                            ht.Add("LOCAL", this.GetObra().Lugar.ToUpper());
                            ht.Add("PEDIDO", this.GetPedido().NomePedido.ToUpper());
                            ht.Add("ETAPA", this.GetSubEtapa().NomeEtapa.ToUpper());
                            ht.Add("ESCALA", "1/" + Math.Round(this.GetEscala(), 1));
                            ht.Add("UNIDADE", "MILÍMETROS");
                            ht.Add("COORDENAÇÃO", this.GetSubEtapa().Coordenador.ToUpper());
                            ht.Add("COORDENACAO", this.GetSubEtapa().Coordenador.ToUpper());
                            ht.Add("PROJETO", this.GetSubEtapa().Projetista.ToUpper());
                            ht.Add("DESENHO", Cfg.Init.GetUser().Nome.ToUpper());
                            ht.Add("RESPONSAVEL_TECNICO", this.GetSubEtapa().Calculista.ToUpper());
                            ht.Add("CREA", this.GetSubEtapa().CalculistaCREA.ToUpper());
                        }

                        Atributos.Set(s, acTrans, ht);

                    }

                    if (selo.Count > 0)
                    {
                        acTrans.Commit();
                        editor.Regen();
                    }
                    else
                    {
                        AddMensagem("\nNenhum Selo encontrado no desenho.");
                    }
                }
            }

        }
        public BlockAttributes GetBlocoTecnoMetal(BlockReference bloco, string arquivo, bool somente_visiveis = true, Database acCurDb = null)
        {
            var ultima_edicao = arquivo.getEdicao();
            var nome = arquivo.getNome();
            try
            {
                if (acCurDb == null)
                {
                    acCurDb = CAD.acCurDb;
                }



                var atributos = bloco.GetAttributes(somente_visiveis, acCurDb);
                atributos[Cfg.Init.CAD_ATT_ARQ].Valor = arquivo;

                try
                {
                    atributos[T_DBF1.NUM_COM.ToString()].Valor = this.GetPedido().NomePedido;
                    atributos[T_DBF1.DES_COM.ToString()].Valor = this.GetObra().Nome;
                    atributos[T_DBF1.LOT_COM.ToString()].Valor = this.GetSubEtapa().NomeEtapa;
                }
                catch (Exception ex)
                {
                    DLM.log.Log(ex);
                }

                atributos[T_DBF1.NUM_DIS.ToString()].Valor = nome;
                atributos[T_DBF1.FLG_DWG.ToString()].Valor = nome;
                atributos[T_DBF1.FLG_REC.ToString()].Valor = atributos[T_DBF1.POS_PEZ.ToString()].Valor == "" ? Cfg.Init.CAD_ATT_REC_MARCA : Cfg.Init.CAD_ATT_REC_POSICAO;
                atributos[T_DBF1.DAT_DIS.ToString()].Valor = ultima_edicao;
                atributos[Cfg.Init.CAD_ATT_BLK].Valor = bloco.Name.ToUpper();

                return atributos;
            }
            catch (Exception ex)
            {
                DLM.log.Log(ex);
                BlockAttributes att = new BlockAttributes(bloco, false);
                att[Cfg.Init.CAD_ATT_BLK].Valor = bloco.Name.ToUpper();
                att[T_DBF1.FLG_DWG.ToString()].Valor = nome;
                att[Cfg.Init.CAD_ATT_ERRO].Valor = ex.Message;
                att[Cfg.Init.CAD_ATT_ARQ].Valor = arquivo;

                return att;
            }
        }
        public db.Tabela GetPecasPranchas(ref List<Report> erros, List<Conexoes.Arquivo> pranchas = null)
        {

            db.Tabela marcas = new db.Tabela();
            try
            {
                List<FileInfo> arquivos = new List<FileInfo>();
                if (pranchas == null)
                {
                    pranchas = this.SelecionarDWGs();
                    if (pranchas.Count > 0)
                    {
                        arquivos = pranchas.Select(x => new FileInfo(x.Endereco)).ToList();
                    }
                }
                else
                {
                    pranchas = pranchas.FindAll(x => x.Extensao == Cfg.Init.EXT_DWG).ToList();
                    arquivos = pranchas.Select(x => new FileInfo(x.Endereco)).ToList();
                }



                if (arquivos.Count == 0)
                {
                    erros.Add(new Report("Erro", "Operação abortada - Nada Selecionado."));
                    return new db.Tabela();
                }

                Core.Getw().SetProgresso(1, arquivos.Count(), "Carregando...");
                foreach (FileInfo file in arquivos)
                {
                    Core.Getw().somaProgresso($"Mapeando peças: {file.Name}");

                    string arquivo = file.FullName;
                    try
                    {
                        using (Database acTmpDb = new Database(false, true))
                        {
                            acTmpDb.ReadDwgFile(arquivo, FileOpenMode.OpenForReadAndAllShare, false, null);
                            var marcas_prancha = GetBlocosMarcas(acTmpDb, ref erros, arquivo);
                            marcas.Linhas.AddRange(marcas_prancha.Linhas);
                            acTmpDb.CloseInput(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Getw().Close();
                        Conexoes.Utilz.Alerta(ex);
                        return new db.Tabela();
                    }

                }

            }
            catch (System.Exception ex)
            {
                Core.Getw().Close();
                Conexoes.Utilz.Alerta(ex);
                return new db.Tabela();
            }
            Core.Getw().Close();

            return Conexoes.Utilz.DBF.ConverterParaDBF(marcas, ref erros);
        }
        private db.Tabela GetBlocosMarcas(Database acCurdb, ref List<Report> erros, string arquivo)
        {
            var mars = new db.Tabela();
            var poss = new db.Tabela();
            using (var acTrans = acCurdb.acTrans())
            {
                var nome = arquivo.getNome();
                var ultima_edicao = arquivo.getEdicao();

                List<BlockReference> blocos = acCurdb.GetBlockReferences(acTrans);
                string errosm = "";
                string errosp = "";
                List<BlockReference> ms = blocos.Filter(Cfg.Init.GetBlocosTecnoMetalMarcas(), out errosm);
                List<BlockReference> pos = blocos.Filter(Cfg.Init.GetBlocosTecnoMetalPosicoes(), out errosp);

                if (errosm.Length > 0 | errosp.Length > 0)
                {
                    erros.Add(new Report($"{nome}", $"\nPrancha com problemas de blocos.\n{errosm}\n{errosp}\n\n", nome, TipoReport.Critico));
                }

                if (ms.Count == 0)
                {
                    erros.Add(new Report($"{nome}", $"Prancha não tem marcas", nome, DLM.vars.TipoReport.Critico));
                }

                foreach (var m in ms)
                {
                    mars.Add(GetBlocoTecnoMetal(m, arquivo, false, acCurdb));
                }

                foreach (var m in pos)
                {
                    poss.Add(GetBlocoTecnoMetal(m, arquivo, false, acCurdb));
                }

            }

            var retorno = new List<db.Tabela> { mars, poss }.Unir();



            return retorno;

        }





        public List<Report> AtualizarPesoChapa(List<BlockReference> blocos)
        {
            List<Report> erros = new List<Report>();

            // var pcs = GetMarcas(ref erros, blocos);

            using (var acTrans = acCurDb.acTransST())
            {

                if (erros.Count == 0 && blocos.Count > 0)
                {
                    var c = 0;
                    foreach (var blk in blocos)
                    {
                        var bkm = GetBlocoTecnoMetal(blk, acDoc.Name, true, acCurDb);
                        var bloco = new MarcaTecnoMetal(bkm);

                        if ((bloco.Tipo_Marca != Tipo_Marca.MarcaComposta) && (bloco.Tipo_Bloco == Tipo_Bloco.Arremate | bloco.Tipo_Bloco == Tipo_Bloco.Chapa | bloco.Tipo_Bloco == Tipo_Bloco.Elemento_M2))
                        {
                            var peso = bloco.CalcularPesoLinear();
                            var sup = bloco.CalcularSuperficieLinear();
                            if (peso > 0)
                            {
                                var att = new db.Linha();
                                att.Add(T_DBF1.PUN_LIS.ToString(), peso);
                                att.Add(T_DBF1.SUN_LIS.ToString(), sup);
                                att.Add(T_DBF1.ING_PEZ.ToString(), $"{bloco.Comprimento.String(0)}*{bloco.Espessura.String(2)}*{bloco.Largura.String(0)}");
                                att.Add(T_DBF1.SPE_PRO.ToString(), bloco.Espessura);

                                DLM.cad.Atributos.Set(blk, acTrans, att);
                                c++;
                            }
                            else
                            {
                                erros.Add(new Report("Bobina não encontrada", $"Marca/Pos: {bloco.Nome} => {bloco.Material} => {bloco.SAP}", DLM.vars.TipoReport.Critico));
                            }
                        }

                    }
                    acTrans.Commit();

                    if (c == 0)
                    {
                        erros.Add("Nenhuma posição atualizada.");
                    }
                }
            }
            erros.Show();
            return erros;
        }



        public List<Report> TrocarPerfilElementoMetroQuadrado(List<BlockReference> blocos, DLM.cam.Perfil novo_Perfil)
        {
            var erros = new List<Report>();

            if (novo_Perfil == null) { return new List<Report>().Add("Perfil inválido"); }
            if (blocos.Count == 0) { return new List<Report>().Add("Nenhum bloco de TecnoMetal encontrado na seleção."); }

            // var pcs = GetMarcas(ref erros, blocos);

            using (var acTrans = acCurDb.acTransST())
            {
                int c = 0;
                foreach (var blk in blocos)
                {
                    var bkm = GetBlocoTecnoMetal(blk, acDoc.Name, true, acCurDb);
                    var bloco = new MarcaTecnoMetal(bkm);
                    if (bloco.Tipo_Bloco == Tipo_Bloco.Elemento_M2)
                    {
                        c++;
                        var peso = bloco.CalcularPesoLinear(novo_Perfil);
                        var sup = bloco.CalcularSuperficieLinear();
                        if (peso > 0)
                        {
                            var att = new db.Linha();
                            att.Add(T_DBF1.NOM_PRO.ToString(), novo_Perfil.Descricao);
                            att.Add(T_DBF1.PUN_LIS.ToString(), peso);
                            att.Add(T_DBF1.SUN_LIS.ToString(), sup);
                            att.Add(T_DBF1.ING_PEZ.ToString(), $"{bloco.Comprimento.String(0)}*{bloco.Espessura.String(2)}*{bloco.Largura.String(0)}");
                            att.Add(T_DBF1.SPE_PRO.ToString(), bloco.Espessura);

                            DLM.cad.Atributos.Set(blk, acTrans, att);
                            erros.Add(new Report($"Atualizado", $"Marca/Pos: {bloco.Nome} => {bloco.Material} => {bloco.Perfil} => {bloco.SAP}"));
                        }
                        else
                        {
                            erros.Add(new Report("Cadastro não encontrado", $"Marca/Pos: {bloco.Nome} => {bloco.Material} => {bloco.Perfil} => {bloco.SAP}", DLM.vars.TipoReport.Critico));
                        }
                    }

                }

                acTrans.Commit();
                if (c == 0)
                {
                    erros.Add("Nenhuma posição do tipo Elemento m² encontrada.");
                }
            }
            erros.Show();
            return erros;
        }







        public Conexoes.Bobina PromptBobina(Conexoes.Bobina espessura = null)
        {
            var bobinas = new List<Conexoes.Bobina>();
            bobinas.AddRange(Conexoes.DBases.GetBancoRM().GetBobinas());
            if (espessura != null)
            {
                bobinas = bobinas.FindAll(x => x.Espessura == espessura.Espessura && x.Corte == espessura.Corte);
            }
            var sel = bobinas.ListaSelecionar();
            if (sel != null)
            {
                _Bobina_sel = sel;
            }

            return sel;
        }
        public Conexoes.Bobina PromptChapa(Tipo_Chapa tipo)
        {

            var chapas = new List<Conexoes.BobinaDummy>();
            chapas.AddRange(DBases.GetChapas());
            if (tipo == Tipo_Chapa.Fina)
            {
                chapas = chapas.FindAll(x => x.Chapa_Fina);
            }
            else if (tipo == Tipo_Chapa.Grossa)
            {
                chapas = chapas.FindAll(x => !x.Chapa_Fina);
            }

            var sel = chapas.ListaSelecionar();

            if (sel != null)
            {
                _Chapa_sel = sel;
            }
            return sel;
        }

        public string PromptMercadoria()
        {
            var sel = Conexoes.DBases.GetBancoRM().GetMercadorias().ListaSelecionar();
            if (sel != null)
            {
                _Mercadoria_sel = sel;
            }
            return sel;
        }


        public void CAM_de_Polilinha()
        {
            //if (!this.E_Tecnometal())
            //{
            //    return;
            //}
            var sel = SelecionarObjetos(CAD_TYPE.POLYLINE, CAD_TYPE.LINE, CAD_TYPE.LWPOLYLINE, CAD_TYPE.INSERT, CAD_TYPE.CIRCLE, CAD_TYPE.HATCH);
            var polylines = this.GetPolies().FindAll(x => x.SomenteLinhas);
            if (polylines.Count == 0)
            {
                var linhas_grp = this.GetLines().GroupBy(x => x.EntityColor).ToList().FindAll(x => x.ToList().GroupBy(y => y.Angle).Count() > 1);
                foreach (var lista_linha in linhas_grp)
                {
                    var lins = lista_linha.ToList().GetLinhasConectadas();
                    if (lins.Count > 2)
                    {
                        var pol = lins.CriarPolyLine();

                        polylines.Add(new PolyInfo(pol));
                        break;
                    }
                }

            }
            if (polylines.Count > 0)
            {
                var poly = polylines[0];

                var X0 = poly.Pontos.X0();
                bool status = false;


                if (poly.Comprimento > 0 && poly.Largura > 0)
                {
                    var marca = this.Nome.Prompt("Digite o nome da peça",25);
                    if (marca != null)
                    {
                        string material = null;
                        var selm = DBases.GetMateriais().ListaSelecionar();
                        if (selm != null)
                        {
                            material = selm.Nome;
                        }

                        var furos = this.GetFurosSelecao();

                        furos.CriarCirculosDosFuros();

                        if (material != null)
                        {
                            var esp = PromptChapa(Tipo_Chapa.Tudo);
                            if (esp != null)
                            {
                                var qtd = 1.Prompt("Digite a quantidade.");
                                if (qtd != null)
                                {
                                    var ficha = PromptFicha();
                                    if (ficha != null)
                                    {
                                        var coords_normalizadas = poly.Pontos.Normalizar();

                                        var p_marca = Ut.PedirPonto("Selecione a origem da marca.", out status);

                                        if (!status)
                                        {
                                            var face = coords_normalizadas.GetFace();
                                            var sub = this.GetSubEtapa();

                                            Blocos.MarcaChapa(p_marca, coords_normalizadas, esp.Espessura, qtd.Value, marca, material, ficha, this.GetEscala());
                                            var nCAM = new cam.Cam($"{this.PastaCAM}{marca}.{Cfg.Init.EXT_CAM}", face, esp.Espessura);
                                            nCAM.Cabecalho.Tratamento = ficha;
                                            nCAM.Cabecalho.Material = material;
                                            nCAM.Cabecalho.Quantidade = qtd.Value;

                                            nCAM.Cabecalho.Etapa = sub.NomeEtapa;

                                            if (sub.GetObra() != null)
                                            {
                                                nCAM.Cabecalho.Cliente = sub.GetObra().Cliente;
                                                nCAM.Cabecalho.Lugar = sub.GetObra().Lugar;
                                            }

                                            foreach (var furo in furos)
                                            {
                                                var fn = furo.Mover(X0.Inverter());
                                                if (fn.Origem.X > 0 && fn.Origem.X <= nCAM.Comprimento && fn.Origem.Y < 0 && fn.Origem.Y >= -nCAM.Perfil.Largura)
                                                {
                                                    nCAM.Formato.AddFuroLIV1(fn);
                                                }
                                            }
                                            nCAM.Gerar().Abrir();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    AddMensagem($"Polyline com geometria inválida");
                }
            }
        }



        public void PromptGeometria(out double comprimento, out double largura, out double area, out double perimetro)
        {
            comprimento = 0;
            largura = 0;
            area = 0;
            perimetro = 0;

            var opcao = Ut.PedirString("Selecione", new List<string> { "Digitar", "Polyline" });

            if (opcao == "Polyline")
            {
                var sel = SelecionarObjetos(Tipo_Selecao.Polyline);
                var pols = this.GetPolies();
                if (pols.Count > 0)
                {
                    var pl = pols[0];
                    if (!pl.Closed)
                    {
                        Conexoes.Utilz.Alerta("Polyline inválida. Somente polylines fechadas representando o contorno da chapa.");
                        return;
                    }

                    comprimento = pl.Comprimento.Round(0);
                    largura = pl.Largura.Round(0);
                    area = pl.Area.Round(0);
                    perimetro = pl.Perimetro.Round(0);
                }
            }
            else if (opcao == "Digitar")
            {
                bool cancelado = false;
                comprimento = Math.Abs(Ut.PedirDistancia("Defina o comprimento", out cancelado));
                if (!cancelado)
                {
                    largura = Math.Abs(Ut.PedirDistancia("Defina a largura", out cancelado));

                    if (!cancelado)
                    {
                        perimetro = 2 * largura + 2 * comprimento;
                        area = largura * comprimento;
                    }
                }
            }

        }
        public string PromptFicha()
        {
            return Cfg.Init.RM_SEM_PINTURA.Prompt("Digite a ficha de pintura", 20, false, true, "FICHA");
        }
        public string PromptMarca(string prefix = "ARR-", string nome = null)
        {

            var erros = new List<Report>();
            var marcas = this.GetMarcas(ref erros);
            var nnn = (marcas.FindAll(x => x.Nome.StartsWith(prefix)).Count + 1).String(2);
            if (nome != null)
            {
                nnn = nome;
            }
            nnn = $"{prefix}{nnn}";
        retentar:
            var nome_marca = nnn.Prompt("Digite o nome da Marca", 25, false, true, "NOME_MARCA").ToUpper().Replace(" ", "");

            if (nome_marca.Length == 0)
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
            var iguais = marcas.FindAll(x => x.Nome == nome_marca);
            if (iguais.Count > 0)
            {
                if (Conexoes.Utilz.Pergunta($"[{nome_marca}] Já existe uma marca com o mesmo nome. É necessário trocar. \nTentar Novamente?"))
                {
                    goto retentar;
                }
                else
                {
                    return "";
                }
            }
            if (nome_marca.CaracteresEspeciais())
            {
                if (Conexoes.Utilz.Pergunta($"[{nome_marca}] Nome não pode conter caracteres especiais. É necessário trocar. \nTentar Novamente?"))
                {
                    goto retentar;
                }
                else
                {
                    return "";
                }
            }
            return nome_marca;
        }



        public void InserirArremate(double escala = 0, string marca = "", string posicao = "", int quantidade = 1, string ficha = null, Conexoes.Bobina bobina = null, bool chapa_fina = true, string mercadoria = null)
        {
            if (escala == 0)
            {
                escala = this.GetEscala();
            }
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("ARR-");
            }
            if (marca == null | marca == "") { return; }


            SelecionarObjetos(Tipo_Selecao.Polyline);
            var pols = Selecoes.Filter<Polyline>();
            if (pols.Count > 0)
            {

                bool status = false;

                var pl = pols[0];

                if (pl.Closed)
                {
                    Conexoes.Utilz.Alerta("Polyline inválida. Somente polylines abertas representando o corte da chapa.");
                    return;
                }




                if (status)
                {
                    return;
                }


                var segmentos = new List<LineSegment3d>();
                var angulos = Angulo.GetAngulos(pl, out segmentos);

                foreach (var s in angulos)
                {
                    AddMensagem($"\nAngulo:" + s);
                }


                double corte = Math.Round(pl.Length);


                double comprimento = Ut.PedirDistancia("Defina o comprimento", out status);



                if (bobina == null)
                {

                    bobina = DBases.GetBobinaDummy(Cfg.Init.Material_RMU);
                    var espessura = PromptChapa(Tipo_Chapa.Fina);
                    if (espessura == null)
                    {
                        return;
                    }
                    chapa_fina = espessura.Chapa_Fina;
                    if (chapa_fina)
                    {

                        bobina = PromptBobina(espessura);
                    }
                    else
                    {
                        bobina.Espessura = espessura.Espessura;
                        bobina.Material = Cfg.Init.Material_Estrutura;
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

                ConfiguracaoChapa_Dobrada chapa_dobrada = new ConfiguracaoChapa_Dobrada(bobina, corte, comprimento, 0, angulos) { Marca = marca, GerarCam = chapa_fina ? Opcao.Nao : Opcao.Sim, DescontarDobras = !chapa_fina, Ficha = ficha, Quantidade = (int)quantidade, Mercadoria = mercadoria };
                //pa = Conexoes.Utilz.Propriedades(pa, out status);
                if (chapa_dobrada.Comprimento > 0 && chapa_dobrada.Espessura > 0 && chapa_dobrada.Marca.Replace(" ", "") != "" && chapa_dobrada.Quantidade > 0)
                {
                    bool cancelado = true;
                    var origem = Ut.PedirPonto("Selecione o ponto de inserção do bloco.", out cancelado);
                    if (!cancelado)
                    {
                        if (chapa_fina)
                        {
                            Blocos.MarcaChapa(origem, chapa_dobrada, Tipo_Bloco.Arremate, escala, posicao);
                        }
                        else
                        {
                            Blocos.MarcaChapa(origem, chapa_dobrada, Tipo_Bloco.Chapa, escala, posicao);
                        }

                        if (Conexoes.Utilz.Pergunta("Gerar CAM?"))
                        {
                            string destino = this.Pasta;
                            destino = destino.getUpdir().GetSubPasta(Cfg.Init.EXT_CAM);

                            if (Directory.Exists(destino))
                            {
                                var Perfil = DLM.cam.Perfil.Chapa(chapa_dobrada.Largura, chapa_dobrada.Espessura);

                                string arquivo = $"{destino}{chapa_dobrada.Marca}.{Cfg.Init.EXT_CAM}";

                                DLM.cam.Cam pcam = new DLM.cam.Cam(arquivo, Perfil, chapa_dobrada.Comprimento);
                                double x = 0;

                                for (int i = 0; i < angulos.Count; i++)
                                {
                                    var s = segmentos[i];
                                    x = x + s.Length - (chapa_fina ? 0 : chapa_dobrada.Espessura);
                                    var a = angulos[i];
                                    pcam.Formato.LIV1.Dobras.Add(new DLM.cam.Dobra(a, x, pcam, false));
                                }

                                pcam.Cabecalho.Tratamento = chapa_dobrada.Ficha;
                                pcam.Cabecalho.Quantidade = chapa_dobrada.Quantidade;
                                pcam.Cabecalho.Material = chapa_dobrada.Material;
                                pcam.Cabecalho.Marca = chapa_dobrada.Marca;
                                pcam.Nota_Custom = "PARA DOBRAS = SEGUIR DESENHO DA PRANCHA DE FABRICAÇÃO.";
                                pcam.Gerar();
                                arquivo.Abrir();

                            }
                        }
                    }

                }

            }
        }
        public void GerarCamsChapasRetas()
        {
            string destino = this.Pasta;
            destino = destino.getUpdir().GetSubPasta(Cfg.Init.EXT_CAM);

            var erros = new List<Report>();
            var marcas = this.GetMarcas(ref erros);



            var marcas_chapas = marcas.FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaSimples | x.Tipo_Marca == Tipo_Marca.Posicao).FindAll(x => x.Tipo_Bloco == Tipo_Bloco.Chapa | x.Tipo_Bloco == Tipo_Bloco.Arremate);

            if (marcas_chapas.Count > 0)
            {
                var selecoes = marcas_chapas.ListaSelecionarVarios(true);
                if (Directory.Exists(destino) && selecoes.Count > 0)
                {
                    if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja gerar os CAMs das Chapas selecionadas?\nAtenção: Serão gerados CAMs retos, sem furações e sem recortes."))
                    {
                        foreach (var chapa_dobrada in selecoes)
                        {
                            var Perfil = DLM.cam.Perfil.Chapa(chapa_dobrada.Largura, chapa_dobrada.Espessura);

                            string arquivo = $"{destino}{chapa_dobrada.Nome}.{Cfg.Init.EXT_CAM}";

                            DLM.cam.Cam pcam = new DLM.cam.Cam(arquivo, Perfil, chapa_dobrada.Comprimento);

                            pcam.Cabecalho.Tratamento = chapa_dobrada.Tratamento;
                            pcam.Cabecalho.Quantidade = chapa_dobrada.Quantidade.Int();
                            pcam.Cabecalho.Material = chapa_dobrada.Material;
                            pcam.Cabecalho.Marca = chapa_dobrada.Nome;
                            pcam.Gerar();
                        }
                    }


                    destino.Abrir();

                }
            }





        }


        public void InserirChapa(double escala = 0, string marca = "", string posicao = "", string material = null, int quantidade = 0, string ficha = null, Conexoes.Bobina espessura = null, string mercadoria = null, Conexoes.Bobina bobina = null)
        {
            if (escala == 0)
            {
                escala = this.GetEscala();
            }

            this.SetEscala(escala);
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


            if (comprimento > 0 && largura > 0)
            {
                bool status = false;
                if (espessura == null)
                {
                    espessura = PromptChapa(Tipo_Chapa.Tudo);
                }
                if (espessura != null)
                {
                    bool chapa_fina = espessura.Chapa_Fina;
                    if (bobina == null)
                    {
                        bobina = DBases.GetBobinaDummy(Cfg.Init.Material_Estrutura);

                        if (chapa_fina)
                        {

                            bobina = PromptBobina(espessura);
                            ficha = Cfg.Init.RM_SEM_PINTURA;
                        }
                        else
                        {
                            if (material == null)
                            {
                                var sel = DBases.GetMateriais().ListaSelecionar();
                                if (sel != null)
                                {
                                    material = sel.Nome;
                                }
                            }
                            bobina.Espessura = espessura.Espessura;
                            bobina.Material = material;
                            if (ficha == null)
                            {
                                ficha = PromptFicha();
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


                    if (bobina != null)
                    {
                        if (material == null)
                        {
                            material = bobina.Material;
                        }
                        ConfiguracaoChapa_Dobrada chapa_dobrada = new ConfiguracaoChapa_Dobrada(bobina, largura, comprimento, area, new List<double>()) { Marca = marca, Ficha = ficha, GerarCam = Opcao.Nao, Quantidade = quantidade, Mercadoria = mercadoria };

                        var origem = Ut.PedirPonto("Selecione a origem", out status);
                        if (!status)
                        {
                            if (chapa_fina)
                            {
                                Blocos.MarcaChapa(origem, chapa_dobrada, Tipo_Bloco.Arremate, escala, posicao);
                            }
                            else
                            {
                                Blocos.MarcaChapa(origem, chapa_dobrada, Tipo_Bloco.Chapa, escala, posicao);
                            }

                            if (chapa_dobrada.GerarCam == vars.Opcao.Sim)
                            {
                                string destino = this.Pasta;
                                destino = destino.getUpdir().GetSubPasta(Cfg.Init.EXT_CAM);

                                if (Directory.Exists(destino))
                                {
                                    var Perfil = DLM.cam.Perfil.Chapa(chapa_dobrada.Largura, chapa_dobrada.Espessura);

                                    string arquivo = $"{destino}{chapa_dobrada.Marca}.{Cfg.Init.EXT_CAM}";

                                    DLM.cam.Cam pcam = new DLM.cam.Cam(arquivo, Perfil, chapa_dobrada.Comprimento);

                                    pcam.Cabecalho.Tratamento = chapa_dobrada.Ficha;
                                    pcam.Cabecalho.Quantidade = chapa_dobrada.Quantidade;
                                    pcam.Cabecalho.Material = chapa_dobrada.Material;
                                    pcam.Cabecalho.Marca = chapa_dobrada.Marca;
                                    pcam.Gerar();
                                    destino.Abrir();

                                }
                            }
                        }

                    }

                }
            }
        }
        public void InserirElementoUnitario(double escala = 0, string marca = "", string posicao = "", double quantidade = 0, string mercadoria = null, Conexoes.RMA peca = null)
        {
            if (escala == 0)
            {
                escala = this.GetEscala();
            }
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("PC-");
            }
            if (marca == null | marca == "") { return; }

            if (peca == null)
            {
                peca = DBases.GetBancoRM().GetRMAs().ListaSelecionar("Selecione uma peça");
            }

            if (peca != null)
            {
                bool status = false;
            denovo:
                if (quantidade <= 0)
                {
                    var valor = peca.MULTIPLO.Prompt();

                    if (valor == null)
                    {
                        return;
                    }
                    quantidade = valor.Value;
                    if (quantidade <= 0 | !peca.MultiploOk(quantidade))
                    {
                        if (Conexoes.Utilz.Pergunta($"Valor setado [{quantidade} é inválido. Precisa ser maior que zero e múltiplo de {peca.MULTIPLO}\nTentar novamente?"))
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

                var origem = Ut.PedirPonto("Selecione a origem", out status);

                if (status)
                {
                    return;
                }

                Blocos.MarcaElemUnitario(origem, peca, quantidade, marca, escala, posicao, mercadoria);
            }
        }
        public void InserirElementoM2(double escala = 0, string marca = "", string posicao = "", string material = null, string ficha = null, int? valor = 0, DLM.cam.Perfil perfil = null, string mercadoria = null)
        {
            if (escala == 0)
            {
                escala = this.GetEscala();
            }
            this.SetEscala(escala);
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
                if (material == null)
                {
                    var sel = DBases.GetMateriais().ListaSelecionar();
                    if (sel != null)
                    {
                        material = sel.Nome;
                    }
                }

                if (material != null)
                {

                    if (valor == 0)
                    {
                        valor = (1).Prompt();
                    }

                    if (valor > 0)
                    {
                        if (perfil == null)
                        {

                            perfil = DBases.GetdbPerfil().GetPerfisTecnoMetal(DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez).ListaSelecionar("Selecione um perfil");
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
                            var ponto = Ut.PedirPonto("Selecione a origem do bloco", out status);

                            if (!status)
                            {
                                Blocos.MarcaElemM2(ponto, perfil, marca, valor.Value, comprimento, largura, area, perimetro, ficha, material, escala, posicao, mercadoria);
                            }
                        }
                    }
                }
            }

        }
        public void InserirPerfil(double escala, string marca = "", string posicao = "", string material = null, string ficha = null, int? valor = 0, DLM.cam.Perfil perfil = null, string mercadoria = null)
        {
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("PF-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.acTrans())
            {
                bool status;
                double comprimento = Ut.PedirDistancia("Defina o comprimento", out status);

                comprimento = Math.Round(comprimento, 0);




                if (comprimento > 0 && !status)
                {
                    if (material == null)
                    {
                        var sel = DBases.GetMateriais().ListaSelecionar();
                        if (sel != null)
                        {
                            material = sel.Nome;
                        }
                    }
                    if (material != null)
                    {
                        if (valor == 0)
                        {
                            valor = (1).Prompt();
                        }




                        if (valor > 0)
                        {
                            if (perfil == null)
                            {
                                perfil = DBases.GetdbPerfil().GetPerfisTecnoMetal(DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez).ListaSelecionar("Selecione um perfil");
                            }


                            if (mercadoria == null && posicao == "")
                            {
                                mercadoria = PromptMercadoria();
                            }

                            if (mercadoria == null && posicao == "")
                            {
                                return;
                            }


                            if (perfil != null)
                            {
                                if (ficha == null)
                                {
                                    ficha = PromptFicha();
                                }

                                var ponto = Ut.PedirPonto("Selecione a origem do bloco", out status);

                                if (!status)
                                {
                                    Blocos.MarcaPerfil(ponto, marca, comprimento, perfil, valor.Value, material, ficha, 0, 0, escala, posicao, mercadoria);
                                }
                            }
                        }
                    }

                }
            }
        }
        public void InserirMarcaComposta(double escala, string nome, string mercadoria, double quantidade, string ficha)
        {
            this.SetEscala(escala);
            bool cancelado = true;
            var origem = Ut.PedirPonto("Selecione a origem", out cancelado);

            if (cancelado)
            {
                return;
            }

            Blocos.MarcaComposta(origem, nome, quantidade, ficha, mercadoria, escala);

        }
        public MarcaTecnoMetal InserirMarcaComposta(double escala)
        {
            List<Report> erros = new List<Report>();
            this.SetEscala(escala);
            bool cancelado = true;
            var origem = Ut.PedirPonto("Selecione a origem", out cancelado);

            if (cancelado)
            {
                return null;
            }
            string nome = this.PromptMarca("P01");
            if (nome != null && nome != "")
            {
                var quantidade = (1.0).Prompt();
                if (quantidade != null)
                {
                    string ficha = this.PromptFicha();
                    if (ficha != null)
                    {
                        string mercadoria = this.PromptMercadoria();
                        if (mercadoria != null && mercadoria != "")
                        {
                            Blocos.MarcaComposta(origem, nome, quantidade.Value, ficha, mercadoria, escala);
                            return this.GetMarcas(ref erros).FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaComposta).Find(x => x.Nome == nome);
                        }
                    }
                }
            }
            return null;
        }



        public void Mercadorias()
        {
            using (var acTrans = acCurDb.acTransST())
            {
                var selecao = SelecionarObjetos(Tipo_Selecao.Blocos);
                string erros = "";
                var marcas = Selecoes.Filter<BlockReference>().Filter(Cfg.Init.GetBlocosTecnoMetalMarcas(), out erros);

                if (marcas.Count > 0)
                {
                    var mercadoria = PromptMercadoria();
                    if (mercadoria != null && mercadoria != "")
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, T_DBF1.DES_PEZ.ToString(), mercadoria);
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }

            }
        }
        public void Materiais()
        {
            using (var acTrans = acCurDb.acTransST())
            {
                var selecao = SelecionarObjetos(Tipo_Selecao.Blocos);
                string erros = "";
                var marcas = Selecoes.Filter<BlockReference>().Filter(Cfg.Init.GetBlocosTecnoMetalMarcas(), out erros);

                if (marcas.Count > 0)
                {
                    var sel = DBases.GetMateriais().ListaSelecionar();
                    if (sel != null)
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, T_DBF1.MAT_PRO.ToString(), sel.Nome);
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void Tratamentos()
        {
            using (var acTrans = acCurDb.acTransST())
            {
                var selecao = SelecionarObjetos(Tipo_Selecao.Blocos);
                string erros = "";
                var marcas = Selecoes.Filter<BlockReference>().Filter(Cfg.Init.GetBlocosTecnoMetalMarcas(), out erros);

                if (marcas.Count > 0)
                {
                    var mercadoria = this.PromptFicha();
                    if (mercadoria != null && mercadoria != "")
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, T_DBF1.TRA_PEZ.ToString(), mercadoria);
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public CADTecnoMetal()
        {
        }
    }
}
