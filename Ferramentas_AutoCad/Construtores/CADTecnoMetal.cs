using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoeditorInput;
using Conexoes;
using DLM.encoder;
using DLM.vars;
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

        public List<MarcaTecnoMetal> Getposicoes(ref List<Report> erros, bool update)
        {
            var marcas = GetMarcas(ref erros);
            var pos = marcas.SelectMany(x => x.GetPosicoes()).GroupBy(x => x.Posicao).Select(x => x.First()).ToList();
            return pos;
        }
        public List<Conexoes.Filete> InserirSoldaComposicao()
        {
            List<Conexoes.Filete> retorno = new List<Conexoes.Filete>();
            List<Report> erros = new List<Report>();
          
            var pos = Getposicoes(ref erros, true);
            var pos_soldados_desmembrados = pos.FindAll(y => !y.Posicao.Contains("_")).FindAll(y => y.GetPerfil().Familia == DLM.vars.CAM_FAMILIA.Soldado).ToList();

            var montar_desmembrado = pos.FindAll(y =>
            y.Posicao.Contains("_1") |
            y.Posicao.Contains("_2") |
            y.Posicao.Contains("_3") |
            y.Posicao.Contains("_4")
            );


            var marcas_desmembrados = montar_desmembrado.GroupBy(x => x.Posicao.Substring(0, x.Posicao.Length - 2));


            foreach(var m in marcas_desmembrados)
            {
                var almas = m.ToList().FindAll(x => x.Posicao.EndsWith("_1") | x.Posicao.EndsWith("_4") | x.Posicao.EndsWith("_7") | x.Posicao.EndsWith("_10") | x.Posicao.EndsWith("_13")).ToList();
                var resto = m.ToList().FindAll(x => almas.Find(y => y.Posicao == x.Posicao) == null).ToList();

                if(almas.Count>0 && resto.Count>0)
                {
                    var esp_m = resto.Max(x => x.Espessura);
                    var esp_alm = almas.Max(x => x.Espessura);
                    var altura = almas.Max(x => x.Largura) + (2 * esp_m);
                    var mesa = resto.Max(x => x.Largura);


                    var cmp = Conexoes.DBases.GetSoldaComposicao().Get(esp_m, esp_alm, altura, mesa, false).Clonar();
                    cmp.Perfil = new DLM.cam.PerfilTecnoMetal(altura, mesa, esp_alm, esp_m);
                    cmp.Nome_Pos = m.Key;
                    retorno.Add(cmp);
                }
                else
                {
                    erros.Add(new Report(m.Key, "Não foi possível montar o perfil. não há mesas / almas suficientes"));
                }
            }


            var perfis = pos_soldados_desmembrados.GroupBy(x => x.Perfil).ToList().FindAll(x=>x.Key.Replace(" ","")!="");
            foreach(var pf in perfis)
            {
                var pp = pf.First().GetPerfil();
                if(pp.Familia == DLM.vars.CAM_FAMILIA.Soldado)
                {
                
                    var cmp = Conexoes.DBases.GetSoldaComposicao().Get(pp.ESP_MESA, pp.ESP, pp.H, pp.ABA, false);
                    foreach(var p in pf.ToList())
                    {
                        var np = cmp.Clonar();
                        np.Perfil = pp;
                        np.Nome_Pos = p.Posicao;
                        retorno.Add(np);
                    }
                }
                
                else if(pp.Familia == DLM.vars.CAM_FAMILIA._Desconhecido)
                {
                    erros.Add(new Report($"{pf.Key} {pp.Nome}", "Perfil não encontrado."));
                }
            }





            Conexoes.Utilz.ShowReports(erros);

            if(retorno.Count>0)
            {
                Ut.IrLayout();
                Ut.ZoomExtend();
                Menus.SoldaComposicao mm = new Menus.SoldaComposicao();
                mm.lista.ItemsSource = retorno;

                mm.ShowDialog();
                if ((bool)mm.DialogResult)
                {
                    double largura = 63.69;
                    var filetes = retorno.GroupBy(x => x.ToString()).ToList();
                    bool cancelado = false;
                    var pt = Ut.PedirPonto3D("Selecione a origem", out cancelado);
                    var origem = new Coordenada(pt).Mover(-largura, 0, 0);
                    if(!cancelado)
                    {
                        foreach(var filete in filetes)
                        {
                            string nome = CADVars.BL_Solda_1;
                            if(filete.First().Filete_Duplo)
                            {
                                nome = CADVars.BL_Solda_2;
                            }
                            /*agrupa as posições de 1 em 1 para inserir o bloco*/
                            var pcs = Conexoes.Extensoes.Quebrar(filete.ToList().Select(x => x.Nome_Pos).ToList(),1).Select(x=> string.Join(",",x)).ToList();

                            foreach(var pc in pcs)
                            {
                                Hashtable ht = new Hashtable();
                                ht.Add("MBPERFIL", pc);
                                ht.Add("MBFILETE", filete.First().Filete_Minimo);
                                Blocos.Inserir(CAD.acDoc, nome, origem.GetPoint2d(), 1, 0, ht);

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
        public void GerarPDF(List<Conexoes.Arquivo> arqs =null, bool gerar_dxf = false)
        {
            List<Conexoes.Arquivo> arquivos = new List<Conexoes.Arquivo>();
            int pranchas_por_page_setup = 50;
            string config_layout = "PDF-A3-PAISAGEM";
            string config_model = "PDF_A3_PAISAGEM";
            string arquivo_dwg = DLM.vars.Cfg.Init.RaizBinarios + @"Lisps\Plot_pdf\CFG\Selo_2017.dwg";

            if(!File.Exists(arquivo_dwg))
            {
                Conexoes.Utilz.Alerta($"Abortado.\nArquivo de pagesetup não encontrado: {arquivo_dwg}");
                return;
            }



            if(arqs==null)
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
                Conexoes.Utilz.Alerta(ex.Message);
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
            var dsds = Conexoes.Utilz.GetArquivos(pasta_dsd, "*.dsd");
            foreach (var arq in dsds)
            {
                if (!Conexoes.Utilz.Apagar(arq))
                {
                    return;
                }
            }


            if (arquivos.Count == 0)
            {
                return;
            }
            var pacotes = arquivos.Quebrar(pranchas_por_page_setup);


            Core.Getw().Show();
            int c = 1;


            List<string> arquivos_dsd = new List<string>();
            Core.Getw().SetProgresso(1, arquivos.Count, $"Gerando PDF (Pacote) {c}/{pacotes.Count}");
            foreach (var pacote in pacotes)
            {
                string arquivo_dsd = pasta_dsd + $@"\Plotagem_{c}.dsd";
                try
                {
                    if (!Conexoes.Utilz.Apagar(arquivo_dsd))
                    {
                        return;
                    }

                    DsdData dsdData = new DsdData();

                    DsdEntryCollection collection = new DsdEntryCollection();
                    Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    /*todo = adicionar tela para configurar qual layout o usuário quer gerar*/
                    foreach (var arquivo in pacote)
                    {
                        string extensao = arquivo.Extensao;
                        try
                        {
                            var pdf = destino + @"\" + arquivo.Nome + ".PDF";
                            if (!Conexoes.Utilz.Apagar(pdf))
                            {
                                Core.Getw().Close();
                                return;
                            }
                            string nome = "";
                            if (extensao.ToUpper().EndsWith("DXF"))
                            {
                                nome = "Model";
                            }
                           else if (arquivo.Endereco.ToUpper() == this.Endereco.ToUpper())
                            {


                                var layouts = Ut.GetLayouts();
                                if (layouts.Count > 0)
                                {
                                    nome = layouts[0].LayoutName;
                                }
                            }
                            else
                            {
                                Database db = new Database(false, true);
                                db.ReadDwgFile(arquivo.Endereco, System.IO.FileShare.Read, true, "");
                                System.IO.FileInfo fi = new System.IO.FileInfo(arquivo.Endereco);


                                using (Transaction Tx = db.TransactionManager.StartTransaction())
                                {

                                    var layouts = Ut.getLayoutIds(db);
                                    if (layouts.Count > 0)
                                    {
                                        Layout layout = Tx.GetObject(layouts[0], OpenMode.ForRead) as Layout;
                                        nome = layout.LayoutName;
                                    }

                                    Tx.Commit();
                                }
                            }

                            if (nome != "")
                            {
                                collection.Add(GetEntradaDSD(arquivo.Endereco, nome));
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Conexoes.Utilz.Alerta($"Erro ao tentar ler o arquivo {arquivo} \n\n{ex.Message}\n{ex.StackTrace}");
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
                    Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
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
            Conexoes.Utilz.Alerta("Plotagem finalizada!","Finalizado", System.Windows.MessageBoxImage.Information);


        }

        private static DsdEntry GetEntradaDSD(string arquivo, string layout)
        {

            DsdEntry entry = new DsdEntry();
            entry.DwgName = arquivo;
            entry.Layout = layout;
            entry.Title = Conexoes.Utilz.getNome(arquivo);
            entry.NpsSourceDwg = "";
            entry.Nps = "";
            return entry;
        }
        public int MapearPCsTecnoMetal(int seq, int arredon, bool subs_bloco, List<CTV_de_para> perfis_mapeaveis, double escala, string arquivo_bloco, bool agrupar_proximos = true, bool contraventos = true, bool mapear_pecas = true)
        {
            this.SetEscala(escala);
            string nome_bloco = Conexoes.Utilz.getNome(arquivo_bloco);
    
            List<PCQuantificar> pecas = this.Quantificar(false,false,false,false,true);
            if(pecas.Count==0)
            {
                Conexoes.Utilz.Alerta($"Nenhuma peça mapeável encontrada na seleção.");
                return seq;
            }
            List<PCQuantificar> tirantes = new List<PCQuantificar>();
            List<PCQuantificar> outros = new List<PCQuantificar>();
            if(contraventos)
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
                        var ccmp = Conexoes.Utilz.Double(nome.Replace(igual.Perfil.Replace("@", ""), ""));

                        if (ccmp > 0)
                        {
                            pc.Perfil = igual.Perfil;
                            tirantes.Add(pc);
                            continue;
                        }
                    }
                    if(mapear_pecas)
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
                    Conexoes.Utilz.Alerta($"Não foi possível carregar o arquivo de configuração de contraventos. Contacte suporte. \n{CADVars.Arquivo_CTV}");

                    return seq;
                }

                var tipos = tirantes.GroupBy(x => x.Perfil).ToList().OrderBy(x=>x.Key).ToList();
                //List<Autodesk.AutoCAD.DatabaseServices.BlockReference> excluir = new List<Autodesk.AutoCAD.DatabaseServices.BlockReference>();

                Core.Getw().SetProgresso(1,tipos.Count);
                Core.Getw().Show();

                List<BlocoTag> final = new List<BlocoTag>();
                List<BlocoTag> desagrupado = new List<BlocoTag>();
                foreach (var tipo_por_perfil in tipos)
                {
                    Core.Getw().somaProgresso();
                    string perfil = tipo_por_perfil.Key;
                    var igual = perfis_mapeaveis.Find(x => x.Perfil.ToUpper().Replace(" ","") == perfil.ToUpper().Replace(" ",""));
                    var comps = tipo_por_perfil.ToList().GroupBy(x => Conexoes.Utilz.ArredondarMultiplo(x.Comprimento, arredon)).ToList().OrderBy(x=>x.Key).ToList();
                    foreach (var comp in comps)
                    {
                        string numero = seq.ToString().PadLeft(2, '0');
                        string familia = "TIRANTE " + igual.PecaLiberar.Replace("$C$", "");
                        string codigo = igual.PecaLiberar.Replace("$C$", Math.Round(comp.Key).ToString().PadLeft(igual.CaractComp, '0'));
                        var pedacos = Conexoes.Utilz.Quebrar(comp.Key, 6000, 600, 0);
                        string descricao = string.Join(" ",pedacos.GroupBy(x => x).ToList().Select(x => "(" + x.Key + " " + x.Count().ToString() + "x)"));


                        var atuais = comp.ToList();

                        List<BlocoTag> bl_agrupados = new List<BlocoTag>();

                        var blocos_atuais = atuais.SelectMany(x => x.Blocos).ToList();
                        desagrupado.AddRange(blocos_atuais);


                        if (agrupar_proximos)
                        {
                            foreach (var bl in blocos_atuais)
                            {
                                bl.Descricao = codigo;
                                var bl_ja_adicionados = bl_agrupados.SelectMany(x => x.Filhos).ToList();
                                bl_ja_adicionados.AddRange(bl_agrupados);

                                if (bl_ja_adicionados.Find(x => x.Bloco.Id == bl.Bloco.Id) == null)
                                {
                                    var bl_a_adicionar = blocos_atuais.FindAll(x => bl_ja_adicionados.Find(y => y.Bloco.Id == x.Bloco.Id) == null).ToList();
                                    var iguais = bl_a_adicionar.FindAll(x => x.GetCoordenada().Distancia(bl.GetCoordenada()) <= escala * 5);
                                    bl.Filhos = iguais.FindAll(x=>x.Bloco.Id!=bl.Bloco.Id);
                                    bl_agrupados.Add(bl);
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
                          
                            Hashtable att = new Hashtable();
                            att.Add(CADVars.ATT_N, numero);
                            att.Add(CADVars.ATT_Familia, familia);
                            att.Add(CADVars.ATT_Tipo, CADVars.ATT_TECNOMETAL);
                            att.Add(CADVars.ATT_Comprimento, comp.Key);
                            att.Add(CADVars.ATT_Codigo, codigo);
                            att.Add(CADVars.ATT_id, 0);
                            att.Add(CADVars.ATT_Descricao, string.Join(" ", descricao));
                            att.Add(CADVars.ATT_Destino, CADVars.ATT_RME);
                            att.Add(CADVars.ATT_Quantidade, 1 + s.Filhos.Count);
                            if(subs_bloco)
                            {
                            Blocos.Inserir(CAD.acDoc, arquivo_bloco, s.Bloco.Position, escala, 0, att);
                            }
                            else
                            {
                                var angulo = s.GetAngulo();
                                Blocos.Inserir(CAD.acDoc, CADVars.BL_INDICACAO_TXT, s.Bloco.Position, escala, angulo, att);
                            }
                        }


                        using (var acTrans = CAD.acCurDb.TransactionManager.StartOpenCloseTransaction())
                        {
                            foreach (var s in atuais)
                            {
                                s.Nome = codigo;
                                s.Descricao = descricao;
                                s.Numero = numero;
                                s.Nome_Bloco = nome_bloco;
                                s.Familia = familia;
                                s.Destino = CADVars.ATT_RME;
                                s.Perfil = perfil;
                                Hashtable att = new Hashtable();
                                //att.Add("LUN_PRO", comp);
                                att.Add("MARK", codigo);
                                att.Add(T_DBF1.MAR_PEZ.ToString(), codigo);
                                Atributos.Set(s.Blocos.Select(x => x.Bloco).ToList().ToList(), acTrans, att);
                                Atributos.Set(s.Filhos_Ignorar.SelectMany(x => x.Blocos).Select(x => x.Bloco).ToList().ToList(), acTrans, att);
                            }

                            acTrans.Commit();
                        }

                        seq++;
                    }

                }

                acDoc.Editor.Regen();




                Core.Getw().Close();

            }

            if(outros.Count>0 && mapear_pecas)
            {
                var tot = outros.Sum(x => x.Blocos.Count);
                Core.Getw().SetProgresso(1,tot,  $"Inserindo {tot} blocos de outras peças");
                Core.Getw().Show();
                foreach (var pc in outros)
                {
                    pc.Numero = seq.ToString().PadLeft(2, '0');
                    seq++;
                    foreach (var s in pc.Blocos)
                    {
                        Core.Getw().somaProgresso();

                        Hashtable ht = new Hashtable();
                        ht.Add(CADVars.ATT_N, pc.Numero);
                        ht.Add(CADVars.ATT_Familia, pc.Familia);
                        ht.Add(CADVars.ATT_Tipo, CADVars.ATT_TECNOMETAL);
                        ht.Add(CADVars.ATT_Comprimento, pc.Comprimento.ToString().Replace(",", ""));
                        ht.Add(CADVars.ATT_Codigo, pc.Nome);
                        ht.Add(CADVars.ATT_id, 0);
                        ht.Add(CADVars.ATT_Descricao, pc.Descricao);
                        ht.Add(CADVars.ATT_Destino, pc.Destino);
                        ht.Add(CADVars.ATT_Quantidade, 1 + s.Filhos.Count);

                        Blocos.Inserir(CAD.acDoc, CADVars.BlocosIndicacao()[0], s.Bloco.Position, escala, 0, ht);
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

            var sel = SelecionarObjetos( Tipo_Selecao.Blocos_Textos);
            if(sel.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK && this.selecoes.Count>0)
            {
                bool status = true;
                var opt = new ConfiguracaoQuantificar();
                opt.Blocos = blocos;
                opt.Textos = textos;
                opt.Pecas_TecnoMetal = tecnometal;
                if(configurar)
                {
                opt = Conexoes.Utilz.Propriedades(opt, out status);
                }

                if(status)
                {
                    if(opt.Blocos | opt.Pecas_TecnoMetal)
                    {
                        List<PCQuantificar> blocos_montagem_tecnometal = new List<PCQuantificar>();
                        foreach (var s in this.GetBlocos().FindAll(x => !
                         x.Name.Contains("*"))
                        .GroupBy(x => x.Name.ToUpper()
                        .Replace("SUPORTE_", "")
                        .Replace("SUPORTE ", "")
                        ))
                        {
                            var att = Atributos.GetBlocoTag(s.First());

                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Bloco, s.Key.ToUpper(), "", s.Key.ToUpper(), s.ToList().Select(x => DLM.cad.Atributos.GetBlocoTag(x)).ToList());
                            if (npc.Nome.StartsWith(CADVars.PC_Quantificar))
                            {
                                var blcs = npc.Agrupar(new List<string> { "CODIGO", CADVars.ATT_N }, npc.Nome_Bloco);
                                foreach (var bl in blcs)
                                {
                                    bl.SetDescPorAtributo(CADVars.ATT_Descricao);
                                    bl.SetNumeroPorAtributo(CADVars.ATT_N);
                                    bl.SetDestinoPorAtributo("DESTINO");
                                    bl.SetQtdPorAtributo(CADVars.ATT_Quantidade);
                                    bl.SetIdPorAtributo(CADVars.ATT_id);
                                    bl.SetFamiliaPorAtributo(CADVars.ATT_Familia);
                                }

                                pecas.AddRange(blcs);

                            }
                            else if (npc.Nome == CADVars.Bloco_3D_Montagem_Info)
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
                            else if (npc.Nome == CADVars.Bloco_3D_Montagem_Tecnometal)
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
                        foreach(var bl in blk_tec)
                        {
                            //nesse segmento, ignoro o bloco repetido que dá os dados da peça e crio 1 objeto novo, quantificando pela quantidade do bloco que contém somente a marca
                            var blocs = bl.ToList().FindAll(x => x.Nome_Bloco == CADVars.Bloco_3D_Montagem_Tecnometal);
                            if(blocs.Count>0)
                            {
                                var p = blocs[0];
                                var p_filhos_infos = bl.ToList().FindAll(x => x.Nome_Bloco == CADVars.Bloco_3D_Montagem_Info);

                                var pf = new PCQuantificar(Tipo_Objeto.Bloco);

                                if (p_filhos_infos.Count > 0)
                                {
                                    pf = p_filhos_infos[0];
                                }
                                PCQuantificar pc = new PCQuantificar(Tipo_Objeto.Bloco, bl.Key, pf.Descricao, p.Nome_Bloco, blocs.SelectMany(x=> x.Blocos).ToList(), "", pf.Perfil, CADVars.ATT_TECNOMETAL, pf.Perfil, pf.Material, pf.Comprimento);
                                pc.Descricao = pf.Descricao;
                               /*essa propriedade guarda os blocos que tem as sub-informações dos blocos no tecnometal*/
                                pc.Filhos_Ignorar = p_filhos_infos;
                                pecas.Add(pc);
                            }
           
                        }

                    }

                    if(opt.Textos)
                    {
                        var txt00 = this.GetMtexts().GroupBy(x => x.Text
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
                        .Split(' ')[0]).ToList().FindAll(x=>x.Key.Length>0);

                        foreach (var s in txt00)
                        {

      

                            bool nao_adicionar = false;
                            List<string> ignorar = CADVars.Ignorar();
                            foreach(var ign in ignorar)
                            {
                                if(s.Key.Contains(ign))
                                {
                                    nao_adicionar = true;
                                    break;
                                }
                            }

                            if(nao_adicionar)
                            {
                                continue;
                            }


                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto,s.Key,s.First().Text,"",s.ToList().Select(x=> new BlocoTag(new List<CelulaTag> { new CelulaTag("VALOR", x.Text,null) })).ToList());
                            pecas.Add(npc);

                        }
                        var txtss = this.GetTexts().GroupBy(x => x.TextString.Replace("*", "").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").TrimStart().TrimEnd().Split(' ')[0].Replace("(", " ").Replace(")", " ")).ToList().FindAll(x=>x.Key.Length>0);
                        foreach (var s in txtss)
                        {
                            PCQuantificar npc = new PCQuantificar(Tipo_Objeto.Texto,s.Key, s.First().TextString,"", s.ToList().Select(x => new BlocoTag(new List<CelulaTag> { new CelulaTag("VALOR", x.TextString,null) })).ToList());
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

                    
                    if(pecas.Count>0 && gerar_tabela)
                    {
                        InserirTabelaQuantificacao(pecas);

                    }

                }

                
            }

            return pecas.OrderBy(x=> x.Numero + " - " + x.Nome).ToList();
        }

        private void InserirTabelaQuantificacao(List<PCQuantificar> pecas)
        {
            List<PCQuantificar> pcs = Conexoes.Utilz.Selecao.SelecionarObjetos(new List<PCQuantificar> { }, pecas, "Determine quais peças deseja que apareçam na tabela");


            Menus.Quantificar_Menu_Configuracao mm = new Menus.Quantificar_Menu_Configuracao(pcs);

            mm.Show();

            mm.Closed += InserirTabelaQuantificacao;
        }

        private void InserirTabelaQuantificacao(object sender, EventArgs e)
        {
            Menus.Quantificar_Menu_Configuracao mm = sender as Menus.Quantificar_Menu_Configuracao;

            if(mm.confirmado)
            {
                List<PCQuantificar> pcs = mm.original;
                if (mm.confirmado)
                {
                    pcs = mm.filtro;
                }

                if (pcs.Count > 0)
                {
                    bool cancelado = false;
                    var pt = Ut.PedirPonto2D("Selecione a origem", out cancelado);
                    if (!cancelado)
                    {

                        var separar = Conexoes.Utilz.Pergunta("Separar as peças e gerar tabelas por família?");
                        Tabelas.Pecas(pcs, separar, pt, 0);
                    }
                }
            }
          

        }


        private List<DLM.cam.ReadCam> _cams { get; set; } = new List<DLM.cam.ReadCam>();
        public List<DLM.cam.ReadCam> GetCams(bool atualizar = false)
        {
            if(_cams.Count==0 && this.E_Tecnometal(false) | atualizar && this.E_Tecnometal(false))
            {
                var sub = this.GetSubEtapa();
                var cams = Conexoes.Utilz.GetArquivos(sub.PastaCAM, "*.CAM");
                Core.Getw().SetProgresso(1,cams.Count, "Carregando CAMs...");
                Core.Getw().Show();

                foreach(var CAM in cams)
                {
                    _cams.Add(new DLM.cam.ReadCam(CAM));
                    Core.Getw().somaProgresso();
                }
                Core.Getw().Close();
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
            var st = editor.Command("TEC_ST3D2DBF", this.Nome, "t", CADVars.ATT_N, CADVars.ATT_N);
        }
        public void RodarMacros(List<Conexoes.Arquivo> Arquivos = null)
        {
            if (!E_Tecnometal()) { return; }
            if (Arquivos == null)
            {
                Arquivos = SelecionarDWGs();
            }

            if (Arquivos.Count == 0)
            {
                Conexoes.Utilz.Alerta("Nenhuma prancha DWG selecionada.");
                return;
            }


            var arquivos_abertos = Arquivos.FindAll(x => x.EstaAberto).FindAll(x => x.Endereco.ToUpper() != this.Endereco.ToUpper());

            if(arquivos_abertos.Count>0)
            {
                Conexoes.Utilz.Alerta($"Há {arquivos_abertos.Count} abertos. Feche os arquivos para poder continuar: {string.Join(",",arquivos_abertos.Select(x=>x.Nome))}");
                return;
            }


            ConfiguracaoMacro cfg = new ConfiguracaoMacro();
            bool status = false;
            Conexoes.Utilz.Propriedades(cfg, out status);

            if (!status) { return; }

            List<Report> erros = new List<Report>();


            if (cfg.GerarDBF)
            {
                var s = GerarDBF(ref erros, cfg.AtualizarCams, null, Arquivos);
            }

            if(erros.Count>0)
            {
                Conexoes.Utilz.ShowReports(erros);
                return;
            }




            if (cfg.GerarTabela | cfg.AjustarMViews | cfg.AjustarLTS | cfg.PreencheSelos | cfg.LimparDesenhos)
            {
                Core.Getw().SetProgresso(1, Arquivos.Count);
                Core.Getw().Show();

                foreach (var drawing in Arquivos)
                {

                    Document docToWorkOn = CAD.acDoc;

                    if (drawing.Endereco.ToUpper() != this.Endereco.ToUpper())
                    {
                        docToWorkOn = CAD.documentManager.Open(drawing.Endereco, false);

                    }

                    Ut.IrLayout();

                    using (docToWorkOn.LockDocument())
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
                            Ut.SetLts();
                        }

                        if (cfg.PreencheSelos)
                        {
                            PreencheSelo();
                        }

                        if (cfg.LimparDesenhos)
                        {
                            Ut.LimparDesenho();
                        }
                    }

                    if (drawing.Endereco.ToUpper() == this.Endereco.ToUpper())
                    {
                        CAD.editor.Command("qsave", "");
                        //using (docToWorkOn.LockDocument())
                        //{
                        //    Autodesk.AutoCAD.DatabaseServices.DwgVersion tVersion = Autodesk.AutoCAD.DatabaseServices.DwgVersion.Current;
                        //    docToWorkOn.Database.SaveAs(drawing.Endereco, tVersion);
                        //}
                    }
                    else
                    {

                        docToWorkOn.CloseAndSave(drawing.Endereco);
                    }



                    Core.Getw().somaProgresso("1/3 - Rodando macros " + drawing.Nome);
                }
                Core.Getw().Close();

            }


            if (cfg.Gerar_PDFs)
            {
                this.GerarPDF(Arquivos, cfg.DXFs_de_CAMs);
            }
            else if(cfg.DXFs_de_CAMs)
            {
                this.GerarDXFs();
            }


            if (erros.Count > 0)
            {
                Conexoes.Utilz.ShowReports(erros);
                return;
            }

            Conexoes.Utilz.Alerta("Finalizado","", System.Windows.MessageBoxImage.Information);
        }

        public List<MarcaTecnoMetal> GetMarcas(ref List<Report> erros, TabelaBlocoTag pcs = null, List<BlockReference> blocos = null)
        {
            var _Marcas = new List<MarcaTecnoMetal>();

            List<MarcaTecnoMetal> mm = new List<MarcaTecnoMetal>();


            if (pcs == null)
            {
                TabelaBlocoTag marcas = new TabelaBlocoTag();
                TabelaBlocoTag posicoes = new TabelaBlocoTag();
                var ultima_edicao = System.IO.File.GetLastWriteTime(this.Endereco).ToString("dd/MM/yyyy");
                if (blocos == null)
                {
                    blocos = new List<BlockReference>();
                    using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        foreach (ObjectId objId in acBlkTblRec)
                        {
                            Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForRead);
                            if (ent is BlockReference)
                            {
                                var s = ent as BlockReference;
                                blocos.Add(s);
                            }
                        }
                    }
                }




                List<BlockReference> mss = Ut.Filtrar(blocos, CADVars.BlocosTecnoMetalMarcas);
                List<BlockReference> pos = Ut.Filtrar(blocos, CADVars.BlocosTecnoMetalPosicoes);


                foreach (var m in mss)
                {
                    marcas.Blocos.Add(GetDadosBloco(m, this.Endereco, this.Nome, ultima_edicao, false));
                }

                foreach (var m in pos)
                {
                    posicoes.Blocos.Add(GetDadosBloco(m, this.Endereco, this.Nome, ultima_edicao, false));
                }
                pcs = ConverterParaDBF(ref erros, marcas, posicoes);
            }

            foreach (var pc in pcs.Blocos)
            {
                mm.Add(new MarcaTecnoMetal(pc));
            }

            var ms = mm.Select(x => x.Marca).Distinct().ToList();

            foreach (var m in ms)
            {
                var iguais = mm.FindAll(x => x.Marca == m);

                var marcas = iguais.FindAll(x => x.Posicao == "");
                var posicoes = iguais.FindAll(x => x.Posicao != "");

                if (marcas.Count == 1)
                {
                    var marca = marcas[0];
                    marca.SubItens.AddRange(posicoes);
                    foreach (var pos in posicoes)
                    {
                        pos.Pai = marca;
                    }
                    _Marcas.Add(marca);
                }
                else if(marcas.Count>1)
                {
                    erros.Add(new Report("Marcas duplicadas", $" {marcas[0].Prancha} - M: {m}"));
                }
            }
            var posp = _Marcas.SelectMany(x => x.GetPosicoes()).GroupBy(x => x.Posicao);
            foreach(var p in posp)
            {
                var hashes = p.ToList().GroupBy(x => x.GetInfo()).ToList();
                if(hashes.Count>1)
                {
                    erros.Add(new Report($"{p.Key} => Posição com divergências", string.Join("\n",hashes.Select(x=>x.Key)), DLM.vars.TipoReport.Crítico));
                }
            }
            return _Marcas;
        }

        public List<MarcaTecnoMetal> GetMarcas()
        {
            List<Report> erros = new List<Report>();

            return GetMarcas(ref erros);
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


        public List<Conexoes.Arquivo> GerarDXFs(List<DLM.cam.ReadCam> camsext = null)
        {
            List<DLM.cam.ReadCam> cams = new List<DLM.cam.ReadCam>();


            if (!E_Tecnometal()) { return new List<Conexoes.Arquivo>(); }


            if(camsext == null)
            {
                var cms = this.GetSubEtapa().GetPacote().GetCAMsNaoRM();

                
                var subs = cams.SelectMany(x => x.GetFilhos()).ToList();
                cams.AddRange(cms);
                cams.AddRange(subs);
                
               //cams = Conexoes.Utilz.SelecionarObjetos(new List<DLM.cam.ReadCam>(), cams);
            }

            if(cams.Count>0)
            {
                var dxfs = this.GetSubEtapa().GetPacote().GetDXFsPastaCAM();
                Core.Getw().SetProgresso(1,dxfs.Count, $"Apagando dxfs... da pasta {this.GetSubEtapa().PastaCAM}");
                Core.Getw().Show();

                foreach(var s in dxfs)
                {
                    Conexoes.Utilz.Apagar(s.Endereco);
                    Core.Getw().somaProgresso();
                }
                Core.Getw().Close();


                Conexoes.Utilz.TecnoPlotGerarDXF(cams.Select(x => new Conexoes.Arquivo(x.Arquivo)).ToList());

            }
            return this.GetSubEtapa().GetPacote().GetDXFsPastaCAM();
        }
        public void InserirTabela(Point2d? pt =null)
        {

            bool cancelado = false;
            if(pt==null)
            {
                pt = Ut.PedirPonto2D("Clique na origem", out cancelado);
            }

            if (!cancelado)
            {
                List<Report> erros = new List<Report>();
                var pcs = GetMarcas(ref erros);
                if(pcs.Count > 0)
                {
                    if (erros.Count == 0)
                    {
                        Tabelas.TecnoMetal(pcs, (Point2d)pt, -186.47);
                    }
                    else
                    {
                        Conexoes.Utilz.ShowReports(erros);
                    }
                }
            }
        }
        public void ApagarTabelaAuto()
        {
            Ut.IrLayout();
            Ut.ZoomExtend();
            CleanTabela();
        }

        public void InserirTabelaAuto()
        {
            List<Report> erros = new List<Report>();
            InserirTabelaAuto(ref erros);
            Conexoes.Utilz.ShowReports(erros);
        }
        public void InserirTabelaAuto(ref List<Report> erros)
        {
            Ut.IrLayout();
            Ut.ZoomExtend();

            DLM.db.Tabela marcas = new DLM.db.Tabela();
            DLM.db.Tabela posicoes = new DLM.db.Tabela();



            Point2d? pt  = CleanTabela();

            if (pt!=null)
            {
                InserirTabela(pt);
            }


            if (erros.Count > 0)
            {
                foreach (var erro in erros)
                {
                    AddMensagem($"\n{ erro.ToString()}");
                }
            }

            editor.Regen();

        }

        private Point2d? CleanTabela()
        {
            Point2d? retorno = null;
            List<BlockReference> blocos = new List<BlockReference>();
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                var ultima_edicao = System.IO.File.GetLastWriteTime(this.Endereco).ToString("dd/MM/yyyy");
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
                apagar.AddRange(Ut.Filtrar(blocos, new List<string> { "TECNOMETAL_TAB" }, false));



                var selo = Ut.Filtrar(blocos, new List<string> { "SELO" }, false);
                foreach (var s in selo)
                {
                    var offset = -7.01;
                    var pts = Ut.GetPontos(s);
                    if (pts.Count > 0)
                    {
                        retorno = new Point2d(pts.Max(x => x.X) + offset, pts.Max(x => x.Y) + offset);
                    }
                    else
                    {
                        var ptsb = s.Bounds.Value.MaxPoint;
                        retorno = new Point2d(ptsb.X + offset, ptsb.Y + offset);
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
            if (!E_Tecnometal()) { return; }
            Ut.IrLayout();
            Ut.ZoomExtend();

            List<Report> erros = new List<Report>();

            var marcas = GetMarcas(ref erros);

            var nomes_PECAS = marcas.Select(x => x.Marca).Distinct().ToList();

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {

                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {

                    var ultima_edicao = System.IO.File.GetLastWriteTime(this.Endereco).ToString("dd/MM/yyyy");

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

                    List<BlockReference> tabela_tecno = Ut.Filtrar(blocos, new List<string> { "TECNOMETAL_TAB" }, false);
                    List<BlockReference> selo = Ut.Filtrar(blocos, new List<string> { "SELO" }, false);

      

                    foreach (var s in selo)
                    {
                        Hashtable ht = new Hashtable();

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
                            ht.Add("TIPO_DE_PROJETO", this.Nome.Contains("-FA-") ? "PROJETO DE FABRICAÇÃO" : "PROJETO DE MONTAGEM");
                            ht.Add("TITULO_DA_PRANCHA", $"DETALHAMENTO {string.Join(", ", marcas.Select(x=>x.Marca.ToUpper()))}");
                            ht.Add("TÍTULO_DA_PRANCHA", $"DETALHAMENTO {string.Join(", ", marcas.Select(x => x.Marca.ToUpper()))}");
                            ht.Add("OBRA", this.GetObra().Descrição.ToUpper());
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
                            ht.Add("DESENHO", Conexoes.DBases.GetUserAtual().nome.ToUpper());
                            ht.Add("RESPONSAVEL_TECNICO", this.GetSubEtapa().Calculista.ToUpper());
                            ht.Add("CREA", this.GetSubEtapa().CalculistaCREA.ToUpper());
                        }

                        Atributos.Set(s, acTrans, ht);

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

        }
        public BlocoTag GetDadosBloco(BlockReference bloco,  string arquivo, string nome, string ultima_edicao, bool somente_visiveis = true, Database acCurDb = null)
        {
            try
            {
                if(acCurDb==null)
                {
                    acCurDb = CAD.acCurDb;
                }
                var att = Atributos.GetBlocoTag(bloco,somente_visiveis, acCurDb);
                att.Set(CADVars.ATT_ARQ, arquivo);
                if (this.E_Tecnometal(false))
                {
                    try
                    {
                        att.Set(T_DBF1.NUM_COM.ToString(), this.GetPedido().NomePedido);
                        att.Set(T_DBF1.DES_COM.ToString(), this.GetObra().Nome);
                        att.Set(T_DBF1.LOT_COM.ToString(), this.GetSubEtapa().NomeEtapa);
                    }
                    catch (Exception)
                    {
                    }
                }

                att.Set(T_DBF1.NUM_DIS.ToString(), nome);
                att.Set(T_DBF1.FLG_DWG.ToString(), nome);
                att.Set(T_DBF1.FLG_REC.ToString(), att.Get(T_DBF1.POS_PEZ.ToString()).Valor == "" ? CADVars.ATT_REC_MARCA : CADVars.ATT_REC_POSICAO);
                att.Set(T_DBF1.DAT_DIS.ToString(), ultima_edicao);
                att.Set(CADVars.ATT_BLK, bloco.Name.ToUpper());

                return att;
            }
            catch (Exception ex)
            {
                BlocoTag att = new BlocoTag(bloco,false);
                att.Set(CADVars.ATT_BLK, bloco.Name.ToUpper());
                att.Set(T_DBF1.FLG_DWG.ToString(), nome);
                att.Set("ERRO", ex.Message);
                att.Set(CADVars.ATT_ARQ, arquivo);

                return att;
            }
        }



        public List<MarcaTecnoMetal> GetMarcasPranchas(ref List<Report> erros)
        {
            if(!this.E_Tecnometal())
            {
                return new List<MarcaTecnoMetal>();
            }
            var pcs = GetPecasPranchas(ref erros);
            return GetMarcas(ref erros,pcs);
        }
        public TabelaBlocoTag GetPecasPranchas(ref List<Report> erros, List<Conexoes.Arquivo> pranchas = null, bool filtrar = true, bool converter_padrao_dbf = true)
        {

            TabelaBlocoTag marcas = new TabelaBlocoTag();
            TabelaBlocoTag posicoes = new TabelaBlocoTag();
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
                    pranchas = pranchas.FindAll(x => x.Endereco.ToUpper().EndsWith(".DWG")).ToList();
                    arquivos = pranchas.Select(x => new FileInfo(x.Endereco)).ToList();
                }



                if (arquivos.Count == 0)
                {
                    erros.Add(new Report("Erro", "Operação abortada - Nada Selecionado."));
                    return new TabelaBlocoTag();
                }

                Core.Getw().SetProgresso(1,arquivos.Count(), "Carregando...");
                Core.Getw().Show();
                foreach (FileInfo file in arquivos)
                {
                    Core.Getw().somaProgresso($"Mapeando peças: {file.Name}");
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

                                //var nomes = blocos.Select(x => x.Name).Distinct().ToList();
                                List<BlockReference> ms = Ut.Filtrar(blocos, CADVars.BlocosTecnoMetalMarcas);
                                List<BlockReference> pos = Ut.Filtrar(blocos, CADVars.BlocosTecnoMetalPosicoes);

                                if(ms.Count==0)
                                {
                                    erros.Add(new Report("Prancha não tem marcas", $"{file.Name}", DLM.vars.TipoReport.Crítico));
                                }

                                foreach (var m in ms)
                                {
                                    marcas.Blocos.Add(GetDadosBloco(m, arquivo, nome_arq, ultima_edicao,false, acTmpDb));
                                }

                                foreach (var m in pos)
                                {
                                    posicoes.Blocos.Add(GetDadosBloco(m, arquivo, nome_arq, ultima_edicao, false, acTmpDb));
                                }

                            }
                            acTmpDb.CloseInput(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Getw().Close();
                        string msg = $"Erro ao tentar ler a prancha {file.Name}:\n {ex.Message}\n{ex.StackTrace}";
                        erros.Add(new Report("Erro fatal", msg, DLM.vars.TipoReport.Crítico));
                        Conexoes.Utilz.Alerta(msg, "Abortado - Erro fatal", System.Windows.MessageBoxImage.Error);
                        return new TabelaBlocoTag();
                    }

                }
              
            }
            catch (System.Exception ex)
            {
                Core.Getw().Close();
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace, "Abortado - Erro fatal", System.Windows.MessageBoxImage.Error);
                return new TabelaBlocoTag();
            }
            Core.Getw().Close();

            if (converter_padrao_dbf)
            {
                return ConverterParaDBF(ref erros, marcas, posicoes);
            }
            else
            {
                var lista = new TabelaBlocoTag(new List<TabelaBlocoTag> { marcas, posicoes });
                return lista;
            }

        }

        public List<Report> AtualizarPesoChapaFina(List<BlockReference> blocos =null)
        {
            List<Report> erros = new List<Report>();

            var pcs = GetMarcas(ref erros, null, blocos);

            using (var acTrans = CAD.acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                if (erros.Count == 0 && pcs.Count > 0)
                {
                    foreach (var m in pcs)
                    {
                        if (m.Tipo_Marca == Tipo_Marca.MarcaSimples && m.Tipo_Bloco == Tipo_Bloco.Arremate)
                        {
                            var bob = m.GetBobina();
                            if (bob != null)
                            {
                                var peso = m.CalcularPesoLinear();
                                var sup = m.CalcularSuperficieLinear();

                                Hashtable att = new Hashtable();
                                att.Add(T_DBF1.PUN_LIS.ToString(), peso.ToString("N4").Replace(",", "."));
                                att.Add(T_DBF1.SUN_LIS.ToString(), m.CalcularSuperficieLinear().ToString().Replace(",", "."));
                                att.Add(T_DBF1.ING_PEZ.ToString(), $"{m.Comprimento}*{m.Espessura}*{m.Largura}");
                                att.Add(T_DBF1.SPE_PRO.ToString(), m.Espessura.ToString("N2"));

                                DLM.cad.Atributos.Set(m.Bloco.Bloco, acTrans, att);
                            }
                            else
                            {
                                erros.Add(new Report("Bobina não existe ou está em branco", $"Marca: {m.Marca}", DLM.vars.TipoReport.Crítico));
                            }
                        }
                    }
                    acTrans.Commit();
                }
            }
            Conexoes.Utilz.ShowReports(erros);
            return erros;
        }

        private TabelaBlocoTag ConverterParaDBF(ref List<Report> erros, TabelaBlocoTag marcas, TabelaBlocoTag posicoes)
        {
            var lista = new TabelaBlocoTag(new List<TabelaBlocoTag> { marcas, posicoes });
            List<string> colunas = new List<string>();
            colunas.Add(T_DBF1.FLG_REC.ToString());
            colunas.Add(T_DBF1.NUM_COM.ToString());
            colunas.Add(T_DBF1.DES_COM.ToString());
            colunas.Add(T_DBF1.LOT_COM.ToString());
            colunas.Add(T_DBF1.DLO_COM.ToString());
            colunas.Add(T_DBF1.CLI_COM.ToString());
            colunas.Add(T_DBF1.IND_COM.ToString());
            colunas.Add(T_DBF1.DT1_COM.ToString());
            colunas.Add(T_DBF1.DT2_COM.ToString());
            colunas.Add(T_DBF1.NUM_DIS.ToString());
            colunas.Add(T_DBF1.DES_DIS.ToString());
            colunas.Add(T_DBF1.NOM_DIS.ToString());
            colunas.Add(T_DBF1.REV_DIS.ToString());
            colunas.Add(T_DBF1.DAT_DIS.ToString());
            colunas.Add(T_DBF1.TRA_PEZ.ToString());
            colunas.Add(T_DBF1.SBA_PEZ.ToString());
            colunas.Add(T_DBF1.TIP_PEZ.ToString());
            colunas.Add(T_DBF1.MAR_PEZ.ToString());
            colunas.Add(T_DBF1.MBU_PEZ.ToString());
            colunas.Add(T_DBF1.DES_PEZ.ToString());
            colunas.Add(T_DBF1.POS_PEZ.ToString());
            colunas.Add(T_DBF1.NOT_PEZ.ToString());
            colunas.Add(T_DBF1.ING_PEZ.ToString());
            colunas.Add(T_DBF1.QTA_PEZ.ToString());
            colunas.Add(T_DBF1.QT1_PEZ.ToString());
            colunas.Add(T_DBF1.MCL_PEZ.ToString());
            colunas.Add(T_DBF1.COD_PEZ.ToString());
            colunas.Add(T_DBF1.COS_PEZ.ToString());
            colunas.Add(T_DBF1.NOM_PRO.ToString());
            colunas.Add(T_DBF1.LUN_PRO.ToString());
            colunas.Add(T_DBF1.LAR_PRO.ToString());
            colunas.Add(T_DBF1.SPE_PRO.ToString());
            colunas.Add(T_DBF1.MAT_PRO.ToString());
            colunas.Add(T_DBF1.TIP_BUL.ToString());
            colunas.Add(T_DBF1.DIA_BUL.ToString());
            colunas.Add(T_DBF1.LUN_BUL.ToString());
            colunas.Add(T_DBF1.PRB_BUL.ToString());
            colunas.Add(T_DBF1.PUN_LIS.ToString());
            colunas.Add(T_DBF1.SUN_LIS.ToString());
            colunas.Add(T_DBF1.PRE_LIS.ToString());
            colunas.Add(T_DBF1.FLG_DWG.ToString());


            //propriedades que não vão para a DBF
            colunas.Add(CADVars.ATT_ARQ);
            colunas.Add(CADVars.ATT_BLK);


            TabelaBlocoTag tab_pecas = new TabelaBlocoTag();
            var grp_blocos = lista.Blocos.GroupBy(x => x.Get(T_DBF1.MAR_PEZ.ToString()).Valor).ToList().ToList();
            foreach (var s in lista.Blocos)
            {
                BlocoTag l = new BlocoTag(s.Bloco,false);
                foreach (var c in colunas)
                {
                    var igual = s.Get(c);
                    l.Atributos.Add(igual);
                }
                tab_pecas.Blocos.Add(l);
            }

            tab_pecas.Blocos = tab_pecas.Blocos.OrderBy(x => x.Descricao).ToList();

            List<BlocoTag> l_marcas = new List<BlocoTag>();
            var agrupado = tab_pecas.Blocos.GroupBy(x => x.Get(T_DBF1.MAR_PEZ.ToString()).Valor).Select(x => x.ToList()).ToList();


            foreach (var m in agrupado)
            {

                var blocos_marca = m.FindAll(x => x.Get(T_DBF1.POS_PEZ.ToString()).Valor == "");
                if (blocos_marca.Count > 1)
                {
                    string mm = blocos_marca[0].Get(T_DBF1.MAR_PEZ.ToString()).Valor;
                    erros.Add(new Report("Marca Duplicada",
                        $"\n{mm}" +
                        $"\nMarca duplicada ou se encontra em mais de uma prancha." +
                        $"\nOcorrências: {blocos_marca.Count} x\n" +
                        $"{string.Join("\n", blocos_marca.Select(x => x.Get("FLG_DWG")).Distinct().ToList())}",
                       DLM.vars.TipoReport.Crítico
                        ));
                }
                else if(blocos_marca.Count==1)
                {
                    var m_simples = blocos_marca[0].Clonar();
                    var marca = new MarcaTecnoMetal(m_simples);

                    l_marcas.Add(m_simples);

                    var posicoes_tbl = m.FindAll(x => x.Get(T_DBF1.POS_PEZ.ToString()).Valor != "").GroupBy(x => x.Get(T_DBF1.POS_PEZ.ToString()).Valor).Select(X => X.ToList());


                    //CRIA A LINHA DA MARCA SIMPLES
                    if (marca.Tipo_Marca == Tipo_Marca.MarcaSimples)
                    {
                        var p_simples = m_simples.Clonar();

                        m_simples.Set(T_DBF1.FLG_REC.ToString(), CADVars.ATT_REC_MARCA);
                        m_simples.Set(T_DBF1.POS_PEZ.ToString(), "");
                        m_simples.Set(T_DBF1.COD_PEZ.ToString(), "");
                        m_simples.Set(T_DBF1.NOM_PRO.ToString(), "");
                        m_simples.Set(T_DBF1.LUN_PRO.ToString(), "");
                        m_simples.Set(T_DBF1.LAR_PRO.ToString(), "");
                        m_simples.Set(T_DBF1.SPE_PRO.ToString(), "");
                        m_simples.Set(T_DBF1.MAT_PRO.ToString(), "");
                        m_simples.Set(T_DBF1.PUN_LIS.ToString(), "");
                        m_simples.Set(T_DBF1.SUN_LIS.ToString(), "");
                        m_simples.Set(T_DBF1.PRE_LIS.ToString(), "");

                        p_simples.Set(T_DBF1.QTA_PEZ.ToString(), "1");
                        p_simples.Set(T_DBF1.FLG_REC.ToString(), CADVars.ATT_REC_POSICAO);
                        p_simples.Set(T_DBF1.POS_PEZ.ToString(), p_simples.Get(T_DBF1.MAR_PEZ.ToString()).Valor);
                        p_simples.Set(CADVars.ATT_BLK, "DUMMY");

                        l_marcas.Add(p_simples);


                    }
                    else
                    {
                        //adiciona as posições
                        foreach (var pos in posicoes_tbl)
                        {
                            var lfim = pos[0].Clonar();
                            lfim.Set(T_DBF1.QTA_PEZ.ToString(), pos.Sum(x => x.Get(T_DBF1.QTA_PEZ.ToString()).Double()).ToString().Replace(",", "."));
                            l_marcas.Add(lfim);
                        }

                    }
                }
                else
                {
                    erros.Add(new Report("Marca Não existe",
                                           $"\n{string.Join("\n",m.Select(x=>x.Get(T_DBF1.MAR_PEZ.ToString()).Valor + @"/" + x.Get(T_DBF1.MAR_PEZ.ToString()).Valor))}" +
                                           $"\nMarca indicada nas posições não existe." +
                                           $"{string.Join("\n", blocos_marca.Select(x => x.Get("FLG_DWG")).Distinct().ToList())}",
                                          DLM.vars.TipoReport.Crítico
                                           ));
                }

            }
            foreach (var l in l_marcas)
            {
                var pos = l.Get(T_DBF1.POS_PEZ.ToString()).Valor;
                l.Descricao = l.Get(T_DBF1.MAR_PEZ.ToString()).Valor;
                if(pos!="")
                {
                    l.Descricao = l.Descricao + " - P = " + pos;
                }
            }

            //ordena as peças, colocando as marcas antes das posições
            l_marcas = l_marcas.OrderBy(x => x.Get("FLG_REC").Valor.PadLeft(2, '0') + x.Descricao).ToList();

            TabelaBlocoTag lista_convertida = new TabelaBlocoTag();
            lista_convertida.Blocos.AddRange(l_marcas);

            return lista_convertida;
        }

        public TabelaBlocoTag GerarDBF(ref List<Report> erros, bool atualizar_cams,string destino = null, List<Conexoes.Arquivo> pranchas = null)
        {
            if(!E_Tecnometal())
            {
                return new TabelaBlocoTag();
            }
            if (!this.Pasta.ToUpper().EndsWith(@".TEC\"))
            {
                erros.Add(new Report("Pasta Inválida", $"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                    $"\nPasta atual: {this.Pasta}", DLM.vars.TipoReport.Crítico));
                return new TabelaBlocoTag();
            }

            var etapa = this.GetSubEtapa();
            if (destino == null | destino == "")
            {
            setar_nome:
                var nome_dbf = Conexoes.Utilz.RemoverCaracteresEspeciais(Conexoes.Utilz.Prompt("Digine o nome do arquivo", "", $"{(etapa.Nome.Contains("B")?"T_":"")}{etapa.Nome}", false, "", false, 16)).ToUpper();

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
                        erros.Add(new Report("Cancelado", "Nome da DBF inválido", DLM.vars.TipoReport.Crítico));
                        return new TabelaBlocoTag();
                    }
                }
                destino = etapa.PastaDBF + nome_dbf + ".DBF";
            }






            TabelaBlocoTag lista_pecas = GetPecasPranchas(ref erros, pranchas);


            Core.Getw().SetProgresso(1,5, "Fazendo Verificações...");
            Core.Getw().Show();

            if (lista_pecas.Blocos.Count == 0)
            {
                erros.Add(new Report("Erro", "Nenhuma peça encontrada nas pranchas selecionadas", DLM.vars.TipoReport.Crítico));
                Core.Getw().Close();
                return new TabelaBlocoTag();
            }

            var marcas = GetMarcas(ref erros, lista_pecas);
            Core.Getw().somaProgresso();


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
           
            erros.AddRange(marcas_compostas.FindAll(x => x.SubItens.Count == 0).Select(x => new Report("Marca Composta sem posições", $"{x.Prancha} - {x.Marca}", DLM.vars.TipoReport.Crítico)));
                   

            erros.AddRange(marcas_elemento_unit.FindAll(x => !x.Marca.ToUpper().EndsWith("_A")).Select(x => new Report("Nome Inválido", $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}: Peças com elemento unitário devem ser marcadas com '_A' no fim.", DLM.vars.TipoReport.Crítico)));
            erros.AddRange(posicoes_elem_unit.FindAll(x => !x.Posicao.ToUpper().EndsWith("_A")).Select(x => new Report("Nome Inválido", $"{x.Prancha} - M: {x.Marca} - P: {x.Posicao}: Peças com elemento unitário devem ser marcadas com '_A' no fim.", DLM.vars.TipoReport.Crítico)));


            erros.AddRange(marcas.GroupBy(x => x.Marca).ToList().FindAll(x => x.Count() > 1).Select(x => new Report("Mesma marca em pranchas diferentes.", $"Marca: {x.Key} nas pranchas: {string.Join("\n", x.Select(y => y.Prancha))}", DLM.vars.TipoReport.Crítico)));
            erros.AddRange(marcas.FindAll(x=> Conexoes.Utilz.CaracteresEspeciais(x.Marca.Replace("-","").Replace("_","")) | x.Marca.Contains(" ")).Select(x => x.Marca).Distinct().ToList().Select(x => new Report("Nome de marca com caracteres inválidos.", $"Marca: {x}", DLM.vars.TipoReport.Crítico)));
            erros.AddRange(posicoes.FindAll(x=> Conexoes.Utilz.CaracteresEspeciais(x.Posicao.Replace("-","").Replace("_","")) | x.Marca.Contains(" ") | x.Marca.Replace(" ","").Length==0).Select(x=>"M: " + x.Marca + "Pos: " + x.Posicao).Distinct().ToList().Select(x => new Report("Nome de posição com caracteres inválidos ou em branco.", $"{x}", DLM.vars.TipoReport.Crítico)));


            var ppos = posicoes.GroupBy(x => x.Posicao);

            foreach (var p in ppos)
            {
                var pos1 = p.ToList();
                var pos_perfis = pos1.GroupBy(x => x.Perfil).ToList();
                var pos_larguras = pos1.GroupBy(x => x.Largura).ToList();
                var pos_materiais = pos1.GroupBy(x => x.Material).ToList();
                var pos_sap = pos1.GroupBy(x => x.SAP).ToList();

                if (pos_perfis.Count > 1)
                {
                    erros.AddRange(pos_perfis.Select(x => new Report("Posição com divergência", $"Posição: {p.Key} Nos locais: \n{string.Join("\n", x.Select(y => $"==> Perfil: {y.Perfil} => {y.Prancha} / {y.Marca} /  {y.Posicao} "))}", DLM.vars.TipoReport.Crítico)));
                }

                if (pos_larguras.Count > 1)
                {
                    erros.AddRange(pos_larguras.Select(x => new Report("Posição com divergência", $"Posição: {p.Key} Nos locais: \n{string.Join("\n", x.Select(y => $"==> Largura: {y.Perfil} => {y.Prancha} / {y.Marca} /  {y.Posicao} "))}", DLM.vars.TipoReport.Crítico)));
                }

                if (pos_materiais.Count > 1)
                {
                    erros.AddRange(pos_materiais.Select(x => new Report("Posição com divergência", $"Posição: {p.Key} Nos locais: \n{string.Join("\n", x.Select(y => $"==> Material: {y.Perfil} => {y.Prancha} / {y.Marca} /  {y.Posicao} "))}", DLM.vars.TipoReport.Crítico)));
                }


                if (pos_sap.Count > 1)
                {
                    erros.AddRange(pos_sap.Select(x => new Report("Posição com divergência", $"Posição: {p.Key} Nos locais: \n{string.Join("\n", x.Select(y => $"==> SAP: {y.Perfil} => {y.Prancha} / {y.Marca} /  {y.Posicao} "))}", DLM.vars.TipoReport.Crítico)));
                }
            }
        
            erros.AddRange(posicoes.FindAll(x => x.Posicao.ToUpper().EndsWith("_A") && x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).Select(x => new Report("Nome Inválido", $"Prancha: {x.Prancha} Marca: {x.Marca} ==> Posição {x.Posicao} termina com _A e não é um elemento unitário. Somente itens de almox podem terminar com _A", DLM.vars.TipoReport.Crítico)));
            erros.AddRange(marcas.FindAll(x => x.Marca.ToUpper().EndsWith("_A") && x.Tipo_Bloco != Tipo_Bloco.Elemento_Unitario).Select(x => new Report("Nome Inválido", $"Prancha: {x.Prancha} Marca: {x.Marca} ==> Posição {x.Posicao} termina com _A e não é um elemento unitário. Somente itens de almox podem terminar com _A", DLM.vars.TipoReport.Crítico)));


            Core.Getw().somaProgresso();
            if(atualizar_cams)
            {
                var cams = GetCams();

                foreach (var pos in posicoes_grp)
                {
                    var cam = cams.Find(x => x.Nome.ToUpper() == pos.Key.ToUpper());
                    var p0 = pos.First();



                    if (cam != null)
                    {


                        cam.Quantidade = (int)Math.Round(pos.Sum(x => x.Quantidade));
                        cam.Obra = this.GetObra().Descrição;
                        cam.Pedido = this.GetPedido().NomePedido;
                        cam.Etapa = this.GetSubEtapa().Nome;
                        cam.Material = p0.Material;
                        cam.Tratamento = p0.Tratamento;
                        cam.Prancha = cam.Etapa;
                        cam.Peso = p0.PesoUnit;

                        if(cam.Mercadoria=="")
                        {
                            cam.Mercadoria = "POSICAO";
                        }
                        cam.Peso = p0.PesoUnit;

                        foreach (var s in cam.GetSubCams())
                        {
                            var arq = this.GetSubEtapa().PastaCAM + s + ".CAM";
                            if (!File.Exists(arq))
                            {
                                erros.Add(new Report("Falta CAM Desmembrado", $"{s}.CAM \n {string.Join("\n", pos.Select(x => $"{x.Prancha} - M: {x.Marca}"))}", DLM.vars.TipoReport.Alerta));
                            }
                            else
                            {
                                DLM.cam.ReadCam sub = new DLM.cam.ReadCam(arq);
                                sub.Obra = this.GetObra().Descrição;
                                sub.Pedido = this.GetPedido().NomePedido;
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
                        if (p0.Espessura >= 1.55)
                        {
                            erros.Add(new Report("Falta CAM", $"{p0.Posicao}.CAM \n {string.Join("\n", pos.Select(x => $"{x.Prancha} - M: {x.Marca}"))}", DLM.vars.TipoReport.Alerta));
                        }
                    }
                }
            }


            Core.Getw().Close();

            if (destino != "" && destino != null && lista_pecas.Blocos.Count > 0)
            {
                if (!Conexoes.Utilz.DBF.Gerar(lista_pecas.GetTabela(), destino))
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
                _materiais = Conexoes.DBases.GetBancoRM().GetMateriais().Select(x=>x.nome).ToList();
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
            var sel = Conexoes.Utilz.Selecao.SelecionarObjeto(bobinas, null, "Selecione uma bobina");
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

            var sel = Conexoes.Utilz.Selecao.SelecionaCombo(chapas, chapa_sel, "Selecione uma espessura");

            if (sel != null)
            {
                chapa_sel = sel;
            }
            return sel;
        }
        public string PromptMaterial()
        {
            string sel = Conexoes.Utilz.Selecao.SelecionaCombo(GetMateriais(), material_sel, "Selecione o Material");
            if (sel != null)
            {
                material_sel = sel;
            }
            return sel;
        }
        public string PromptMercadoria()
        {
            var sel = Conexoes.Utilz.Selecao.SelecionaCombo(GetMercadorias(), mercadoria_sel, "Selecione a Mercadoria");
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
                var sel = SelecionarObjetos( Tipo_Selecao.Polyline);
                var pols = this.GetPolyLines();
                if (pols.Count > 0)
                {
                    var pl = pols[0];
                    if (!pl.Closed)
                    {
                        Conexoes.Utilz.Alerta("Polyline inválida. Somente polylines fechadas representando o contorno da chapa.");
                        return;
                    }
                    Ut.GetInfo(pl, out comprimento, out largura, out area, out perimetro);
                    comprimento = Math.Round(comprimento, 0);
                    largura = Math.Round(largura, 0);





                }
            }
            else if(opcao == "Digitar")
            {
                bool cancelado = false;
                comprimento = Math.Abs(Ut.PedirDistancia("Defina o comprimento", out cancelado));
                if(!cancelado)
                {
                    largura = Math.Abs(Ut.PedirDistancia("Defina a largura", out cancelado));

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



        public void InserirArremate(double escala,string marca = "", string posicao = "", int quantidade = 1, string ficha = null, Conexoes.Bobina bobina = null, bool chapa_fina = true, string mercadoria = null)
        {
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("ARR-");
            }
            if (marca == null | marca == "") { return; }


            SelecionarObjetos( Tipo_Selecao.Polyline);
            var pols = this.GetPolyLines();
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


                List<LineSegment3d> segmentos = new List<LineSegment3d>();
                var angulos = Angulo.GetAngulos(pl, out segmentos);

                foreach (var s in angulos)
                {
                    AddMensagem($"\nAngulo:" + s);
                }


                double corte = Math.Round(pl.Length);


                double comprimento = Ut.PedirDistancia("Defina o comprimento", out status);



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
                        bobina.Material = Cfg.Init.Material;

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

                ConfiguracaoChapa_Dobrada pa = new ConfiguracaoChapa_Dobrada(bobina, corte, comprimento, 0, angulos) { Marca = marca, GerarCam = chapa_fina ? Opcao.Nao : Opcao.Sim, DescontarDobras = !chapa_fina, Ficha = ficha, Quantidade = (int)quantidade ,Mercadoria = mercadoria };
                //pa = Conexoes.Utilz.Propriedades(pa, out status);
                if (pa.Comprimento > 0 && pa.Espessura > 0 && pa.Marca.Replace(" ", "") != "" && pa.Quantidade > 0)
                {
                    bool cancelado = true;
                    var origem = Ut.PedirPonto2D("Selecione o ponto de inserção do bloco.", out cancelado);
                    if (!cancelado)
                    {
                        if (chapa_fina)
                        {
                            Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Arremate, escala, posicao);
                        }
                        else
                        {
                            Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Chapa, escala, posicao);
                        }

                        if (Conexoes.Utilz.Pergunta("Gerar CAM?"))
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
                                DLM.cam.Chapa pp = new DLM.cam.Chapa(pa.Comprimento, pa.Largura, pa.Espessura);

                                string arquivo = destino + pa.Marca + ".CAM";

                                DLM.cam.Cam pcam = new DLM.cam.Cam(arquivo, pp);
                                double x = 0;

                                for (int i = 0; i < angulos.Count; i++)
                                {
                                    var s = segmentos[i];
                                    x = x + s.Length - (chapa_fina ? 0 : pa.Espessura);
                                    var a = angulos[i];
                                    pcam.Dobras.Liv1.Add(new DLM.cam.Estrutura.Dobra(a, x, pcam, false));
                                }

                                pcam.Cabecalho.TRA_PEZ = pa.Ficha;
                                pcam.Cabecalho.Quantidade = pa.Quantidade;
                                pcam.Cabecalho.Material = pa.Material;
                                pcam.Cabecalho.Marca = pa.Marca;
                                pcam.Nota = "PARA DOBRAS = SEGUIR DESENHO DA PRANCHA DE FABRICAÇÃO.";
                                pcam.Gerar();
                                Conexoes.Utilz.Abrir(arquivo);

                            }
                        }
                    }

                }

            }
        }
        public void InserirChapa(double escala, string marca = "", string posicao = "", string material =null, int quantidade = 0, string ficha = null, Conexoes.Chapa espessura = null, string mercadoria = null, Conexoes.Bobina bobina = null)
        {
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


         if(comprimento>0 && largura>0)
            {
                bool status = false;
                if (espessura == null)
                {
                    espessura = PromptChapa(Tipo_Chapa.Tudo);
                }
                if (espessura != null)
                {
                    bool chapa_fina = espessura.GetChapa_Fina();
                    if (bobina==null)
                    {
                        bobina = Conexoes.Extensoes.Clonar(Conexoes.DBases.GetBobinaDummy());
                 
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
                        ConfiguracaoChapa_Dobrada pa = new ConfiguracaoChapa_Dobrada(bobina, largura, comprimento,area, new List<double>()) { Marca = marca, Ficha = ficha, GerarCam = Opcao.Nao,  Quantidade = quantidade, Mercadoria = mercadoria };

                        var origem = Ut.PedirPonto2D("Selecione a origem", out status);
                        if (!status)
                        {
                            if (chapa_fina)
                            {
                                Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Arremate, escala, posicao);
                            }
                            else
                            {
                                Blocos.MarcaChapa(origem, pa, Tipo_Bloco.Chapa, escala, posicao);
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
                                    DLM.cam.Chapa pp = new DLM.cam.Chapa(pa.Comprimento, pa.Largura, pa.Espessura);

                                    string arquivo = destino + pa.Marca + ".CAM";

                                    DLM.cam.Cam pcam = new DLM.cam.Cam(arquivo, pp);

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
        public void InserirElementoUnitario(double escala, string marca = "", string posicao = "", double quantidade = 0, string mercadoria = null, Conexoes.RMA peca = null)
        {
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("PC-");
            }
            if (marca == null | marca == "") { return; }
            
            if(peca==null)
            {
            peca = Conexoes.Utilz.Selecao.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione uma peça");
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

                var origem = Ut.PedirPonto2D("Selecione a origem", out status);

                if (status)
                {
                    return;
                }

                Blocos.MarcaElemUnitario(origem, peca, quantidade, marca, escala, posicao,mercadoria);
            }
        }
        public void InserirElementoM2(double escala, string marca = "", string posicao = "", string material =null, string ficha = null, int quantidade = 0, DLM.cam.PerfilTecnoMetal perfil = null, string mercadoria = null)
        {
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
                            
                            perfil = Conexoes.Utilz.Selecao.SelecionarObjeto(Conexoes.DBases.GetdbTecnoMetal().GetPerfis(DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez), null, "Selecione um perfil");
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
                            var ponto = Ut.PedirPonto2D("Selecione a origem do bloco", out status);

                            if (!status)
                            {
                                Blocos.MarcaElemM2(ponto, perfil, marca, quantidade, comprimento, largura, area, perimetro, ficha, material, escala, posicao,mercadoria);
                            }
                        }
                    }
                }
            }

        }
        public void InserirPerfil(double escala, string marca = "", string posicao = "", string material = null, string ficha = null, int quantidade = 0, DLM.cam.PerfilTecnoMetal perfil = null, string mercadoria = null)
        {
            this.SetEscala(escala);
            if (marca == "")
            {
                marca = PromptMarca("PF-");
            }
            if (marca == null | marca == "") { return; }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                bool status;
                double comprimento = Ut.PedirDistancia("Defina o comprimento", out status);

                comprimento = Math.Round(comprimento,0);




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
                                perfil = Conexoes.Utilz.Selecao.SelecionarObjeto(Conexoes.DBases.GetdbTecnoMetal().GetPerfis(DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez), null, "Selecione um perfil");
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

                                var ponto = Ut.PedirPonto2D("Selecione a origem do bloco", out status);

                                if (!status)
                                {
                                    Blocos.MarcaPerfil(ponto, marca, comprimento, perfil, quantidade, material, ficha, 0, 0, escala, posicao,mercadoria);
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
            var origem = Ut.PedirPonto2D("Selecione a origem", out cancelado);

            if(cancelado)
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
            var origem = Ut.PedirPonto2D("Selecione a origem", out cancelado);

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
                            Blocos.MarcaComposta(origem, nome, quantidade, ficha, mercadoria, escala);
                         

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
                var selecao = SelecionarObjetos( Tipo_Selecao.Blocos);
                var marcas = Ut.Filtrar(this.GetBlocos(), CADVars.BlocosTecnoMetalMarcas);

                if(marcas.Count>0)
                {
                    var mercadoria =PromptMercadoria();
                    if(mercadoria!=null && mercadoria!="")
                    {
                        foreach(var bloco in marcas)
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
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var selecao = SelecionarObjetos( Tipo_Selecao.Blocos);
                var marcas = Ut.Filtrar(this.GetBlocos(), CADVars.BlocosTecnoMetalMarcas);

                if (marcas.Count > 0)
                {
                    var mercadoria = PromptMaterial();
                    if (mercadoria != null && mercadoria != "")
                    {
                        foreach (var bloco in marcas)
                        {
                            Atributos.Set(bloco, acTrans, T_DBF1.MAT_PRO.ToString(), mercadoria);
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


                var selecao = SelecionarObjetos( Tipo_Selecao.Blocos);

                var marcas = Ut.Filtrar(this.GetBlocos(), CADVars.BlocosTecnoMetalMarcas);

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
