using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DLM.cad.Lisp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using static DLM.cad.CAD;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.EditorInput;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Internal.Reactors;
using DLM.encoder;
using DLM.vars;
using Conexoes;

[assembly: CommandClass(typeof(DLM.cad.Core))]

namespace DLM.cad
{
    public class Core
    {
        private static Conexoes.ControleWait _w { get; set; }

        public static Conexoes.ControleWait Getw()
        {
            if (_w == null)
            {
                _w = Conexoes.Utilz.Wait(100, "");
            }
            return _w;
        }
        private static MenuMarcas _MenuMarcas { get; set; }
        private static CADCotagem _Cotas { get; set; }
        private static CADTecnoMetal _TecnMetal { get; set; }
        private static Menus.Menu_Bloco_Peca menu_bloco { get; set; }
        public static CADMonitoramento monitoramento { get; set; }

        public static MenuMarcas MenuMarcas
        {
            get
            {
                if(_MenuMarcas==null)
                {
                    _MenuMarcas = new MenuMarcas(TecnoMetal);
                }
                return _MenuMarcas;
            }
        }
        public static CADCotagem Cotas
        {
            get
            {
                if(_Cotas ==null)
                {
                    _Cotas = new CADCotagem();
                }
                return _Cotas;
            }
        }

        public static CADTecnoMetal TecnoMetal
        {
            get
            {
                if (_TecnMetal == null)
                {
                    _TecnMetal = new CADTecnoMetal();
                }
                return _TecnMetal;
            }
        }

        public static CADPurlin CADPurlin { get; private set; }

        [CommandMethod(nameof(listarcomandos))]
        public static void listarcomandos()
        {

            Assembly asm = Assembly.GetExecutingAssembly();
            List<string> comandos = Ut.listarcomandos(asm, false).ToList().OrderBy(x=>x).ToList();

            editor.WriteMessage("=== Lista de comandos ===\n");
            foreach (var s in comandos)
            {
                editor.WriteMessage($"---> {s.ToUpper()}\n");
            }
        }

        [CommandMethod(nameof(LCotas))]
        public static void LCotas()
        {
            CADCotagem pp = new CADCotagem();
            pp.ApagarCotas();
        }
        [CommandMethod(nameof(Cotar))]
        public static void Cotar()
        {
            Cotas.Cotar();

        }
        [CommandMethod(nameof(cconfigurar))]
        public static void cconfigurar()
        {

            Cotas.Configurar();
        }
        [CommandMethod(nameof(contornar))]
        public static void contornar()
        {


            Cotas.Contornar();


        }
        [CommandMethod(nameof(cco))]
        public static void cco()
        {

            Cotas.ConfigurarContorno();
            Cotas.Contornar();
        }


        [CommandMethod(nameof(ccb))]
        public static void ccb()
        {


            Cotas.Contornar(false);


        }
        [CommandMethod(nameof(ccv))]
        public static void ccv()
        {


            Cotas.ContornarConvexo();


        }


        [CommandMethod(nameof(desenharmline))]
        public static void desenharmline()
        {
            var estilo = Conexoes.Utilz.Selecao.SelecionaCombo(FuncoesCAD.GetArquivosMlStyles().GetEstilos(), null);
            if(estilo!=null)
            {
                var ml = FuncoesCAD.GetArquivosMlStyles().GetEstilo(estilo);
                if (ml!=null)
                {
                    var pts = Ut.PedirPontos3D();
                    if (pts.Count > 0)
                    {
                        Multiline.DesenharMLine(estilo, ml.Arquivo, pts);
                    }
                }

            }

        }


        [CommandMethod(nameof(substituirpolylinepormultiline))]
        public static void substituirpolylinepormultiline()
        {
            Multiline.MudarPolyline();

        }
        [CommandMethod(nameof(mudarmultiline))]
        public static void mudarmultiline()
        {
            Multiline.MudarMultiline();
        }




        [CommandMethod(nameof(purlin))]
        public static void purlin()
        {
            CADPurlin = new CADPurlin();

            CADPurlin.Purlin();
        }

        [CommandMethod(nameof(renomeiablocos))]
        public static void renomeiablocos()
        {
            CADPurlin = new CADPurlin();
            CADPurlin.RenomeiaBlocos();
        }



        [CommandMethod(nameof(ApagarBlocosPurlin))]
        public static void ApagarBlocosPurlin()
        {
            CADPurlin = new CADPurlin();
            CADPurlin.ApagarBlocosPurlin();

        }
        [CommandMethod(nameof(apagarpurlins))]
        public static void apagarpurlins()
        {
            CADPurlin = new CADPurlin();
            CADPurlin.ApagarPurlins();

        }

        [CommandMethod(nameof(boneco))]
        public static void boneco()
        {
            CADPurlin = new CADPurlin();
            CADPurlin.GetBoneco_Purlin();
        }
        [CommandMethod(nameof(mudaperfiltercas))]
        public static void mudaperfiltercas()
        {
            CADPurlin = new CADPurlin();
            CADPurlin.SetPurlin();

        }


        
        
        [CommandMethod(nameof(passarela))]
        public static void passarela()
        {
            CADTelhas pp = new CADTelhas();
            pp.InserirPassarela();
        }
        [CommandMethod(nameof(apagapassarela))]
        public static void apagapassarela()
        {
            CADTelhas pp = new CADTelhas();
            pp.ApagarPassarelas();
        }
        [CommandMethod(nameof(linhadevida))]
        public static void linhadevida()
        {
            CADTelhas pp = new CADTelhas();
            pp.InserirLinhaDeVida();
        }
        [CommandMethod(nameof(rlinhadevida))]
        public static void rlinhadevida()
        {
            CADTelhas pp = new CADTelhas();
            pp.InserirLinhaDeVida(true);
        }
        [CommandMethod(nameof(apagalinhadevida))]
        public static void apagalinhadevida()
        {
            CADTelhas pp = new CADTelhas();
            pp.ApagarLinhaDeVida();
        }
        [CommandMethod(nameof(rpassarela))]
        public static void rpassarela()
        {
            CADTelhas pp = new CADTelhas();
            pp.InserirPassarela(true);
        }
        [CommandMethod(nameof(alinharlinhadevida))]
        public static void alinharlinhadevida()
        {
            CADTelhas pp = new CADTelhas();
            pp.AlinharLinhaDeVida();
        }


        [CommandMethod(nameof(GetInfos))]
        public static void GetInfos()
        {
            string msg = "";
            var selecao = editor.GetEntity("\nSelecione: ");
            if (selecao.Status != PromptStatus.OK)
                return;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                Entity obj = acTrans.GetObject(selecao.ObjectId, OpenMode.ForRead) as Entity;

                msg = string.Format("Propriedades de {0}:\n", selecao.GetType().Name);



                msg += "\n\nPropriedades custom\n\n";

                msg += Ut.RetornaCustomProperties(obj.ObjectId);

                var props = Ut.GetOPMProperties(obj.ObjectId);

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
                        msg += string.Format("\t{0}: {1}\n", pi.Name, "Erro ao tentar ler: " + ex.Message);
                    }

                    msg += string.Format("\t{0}: {1}\n", pi.Name, value);
                }



                //AddMensagem("\n" + msg);
            }



            Conexoes.Utilz.JanelaTexto(msg, "Propriedades");
        }




        [CommandMethod(nameof(monitorar))]
        public static void monitorar()
        {
            monitoramento = new CADMonitoramento();

        }

        [CommandMethod(nameof(salvarlog))]
        public static void salvarlog()
        {
            if (monitoramento != null)
            {
                monitoramento.SalvarLog();
            }
        }
        
        
        
        [CommandMethod(nameof(setarLTS))]
        public static void setarLTS()
        {
            acDoc.SetLts(10);
        }



        [CommandMethod(nameof(abrepasta))]
        public static void abrepasta()
        {
            CADBase pp = new CADBase();
            pp.AbrePasta();
        }


        [CommandMethod(nameof(exportarma))]
        public static void exportarma()
        {
            CADTelhas pp = new CADTelhas();
            pp.ExportarRMAdeTabela();
        }
        [CommandMethod(nameof(importarm))]
        public static void importarm()
        {
            string arquivo = Conexoes.Utilz.Abrir_String("RM", "Selecione");
            if (File.Exists(arquivo))
            {
                Conexoes.DBRM_Offline pp = new Conexoes.DBRM_Offline();
                var s = pp.Ler(arquivo);
                Tabelas.DBRM(s);
            }

        }


        [CommandMethod(nameof(tabelatecnometal))]
        public static void tabelatecnometal()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.InserirTabela();
        }

        [CommandMethod(nameof(AtualizarPesoChapaFina))]
        public static void AtualizarPesoChapaFina()
        {
            CADTecnoMetal pp = new CADTecnoMetal();

            List<BlockReference> blks = null;
            if (!Conexoes.Utilz.Pergunta("Selecionar tudo?"))
            {
                if (pp.SelecionarObjetos(Tipo_Selecao.Blocos).Status == PromptStatus.OK)
                {
                    blks = pp.GetBlocos();
                    if(blks.Count==0)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            var err = pp.AtualizarPesoChapaFina(blks);
            if (err.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Pesos Atualizados! Deseja gerar/atualizar a tabela?}"))
                {
                    pp.InserirTabelaAuto();
                }
            }
        }

        [CommandMethod(nameof(tabelatecnometalauto))]
        public static void tabelatecnometalauto()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            List<Report> erros = new List<Report>();
            pp.InserirTabelaAuto(ref erros);

            Conexoes.Utilz.ShowReports(erros);
        }

        
        
        
        [CommandMethod(nameof(selopreenche))]
        public static void selopreenche()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.PreencheSelo();
        }
        [CommandMethod(nameof(selolimpar))]
        public static void selolimpar()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.PreencheSelo(true);
        }

        
        [CommandMethod(nameof(rodarmacros), CommandFlags.Session | CommandFlags.Modal)]
        public static void rodarmacros()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.RodarMacros();
        }


        [CommandMethod(nameof(gerardbf3d))]
        public static void gerardbf3d()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.GerarDBF3D();
        }
        [CommandMethod(nameof(gerardbf))]
        public static void gerardbf()
        {


            List<Report> erros = new List<Report>();
            var tbl = TecnoMetal.GerarDBF(ref erros,Conexoes.Utilz.Pergunta("Atualizar CAMs?\nAo ativar essa opção também será verificado CAM x Projeto"));

            if (File.Exists(tbl.Banco))
            {
          
                if(erros.FindAll(x=>x.Tipo == DLM.vars.TipoReport.Crítico).Count>0)
                {
                    Conexoes.Utilz.ShowReports(erros);
                }
                else
                {
                    Conexoes.Utilz.Alerta($"Arquivo {tbl.Banco} gerado!", "", System.Windows.MessageBoxImage.Information);
                    Conexoes.Utilz.getPasta(tbl.Banco).Abrir();
                    //if(Conexoes.Utilz.Pergunta($"Arquivo {tbl.Banco} gerado! Deseja fazer um testlist?"))
                    //{
                    //    var etapa = TecnoMetal.GetSubEtapa();
                    //    if(etapa!=null)
                    //    {
                    //        var pacote = etapa.GetPacote();
                    //        if(pacote!=null)
                    //        {
                    //            pacote.Selecionar(Conexoes.Tipo_Pacote.DBF);
                    //            var testlist = pacote.GetTestList();
                    //            Conexoes.Utilz.ShowReports(testlist.Verificar());
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Conexoes.Utilz.Abrir(Conexoes.Utilz.getPasta(tbl.Banco));

                    //}
                }
            }

        }

 



        [CommandMethod(nameof(arremate))]
        public static void arremate()
        {

            CADTecnoMetal pp = new CADTecnoMetal();
            pp.InserirArremate(pp.GetEscala());
        }
        [CommandMethod(nameof(chapa))]
        public  static void chapa()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.InserirChapa(pp.GetEscala());

        }

        [CommandMethod(nameof(unitario))]
        public static void unitario()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.InserirElementoUnitario(pp.GetEscala());
        }
        [CommandMethod(nameof(elem2))]
        public static void elem2()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
            pp.InserirElementoM2(pp.GetEscala());
        }

        [CommandMethod(nameof(criarmarcasdeexcel))]
        public static void criarmarcasdeexcel()
        {




            CriarMarcas mm = new CriarMarcas();
            mm.Show();

        }
        [CommandMethod(nameof(criarmarcasdecam))]
        public static void criarmarcasdecam()
        {

            var arqs = Conexoes.Utilz.AbrirArquivos("Selecione os arquivos", new List<string> { "CAM" });
            var offset = Cotas.GetEscala() * 70;
            if (arqs.Count > 0)
            {
                bool cancelado = false;
                var p0 = Ut.PedirPonto2D("\nSelecione a origem", out cancelado);
                var x0 = p0.X;
                var y0 = p0.Y;
                int c = 1;

                if (!cancelado)
                {
                    foreach (var s in arqs)
                    {

                        DLM.cam.ReadCAM cam = new DLM.cam.ReadCAM(s);
                        Blocos.CamToMarcaSimples(cam, p0, Cotas.GetEscala());

                        p0 = new Point2d(p0.X + offset, p0.Y);

                        if (c == 10)
                        {
                            p0 = new Point2d(x0, p0.Y + (offset / 2));
                            c = 1;
                        }
                        c++;
                    }
                }
            }

        }


        [CommandMethod(nameof(bloqueiamviews))]
        public static void bloqueiamviews()
        {
            CADBase b = new CADBase();
            b.SetViewport(true);
        }

        [CommandMethod(nameof(desbloqueiamviews))]
        public static void desbloqueiamviews()
        {
            CADBase b = new CADBase();
            b.SetViewport(false);
        }



        [CommandMethod(nameof(teste))]
        public static void teste()
        {
            CADTecnoMetal pp = new CADTecnoMetal();
        }

        [CommandMethod(nameof(marcar))]
        public static void marcar()
        {
            MenuMarcas.Iniciar();
        }

        [CommandMethod(nameof(converterDXF))]
        public static void converterDXF()
        {
            List<Report> erros = new List<Report>();
            var pasta = Conexoes.Utilz.Selecao.SelecionarPasta();
            if(pasta.Existe())
            {
                var arquivos = pasta.GetArquivos("*.DWG").ListaSelecionarVarios();
                if (arquivos.Count > 0)
                {
                    var w = Conexoes.Utilz.Wait(arquivos.Count, $"Aguarde... Convertendo [{arquivos.Count}] itens");
                    foreach (var arq in arquivos)
                    {
                        var nome_fim = $@"{arq.Pasta}\{arq.Nome}.dxf";
                        using (var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(arq.Endereco, true))
                        {
                            try
                            {

                                Core.LimparDesenho(doc);
                                //Core.LimparDesenho();
                                Database db = doc.Database;
                                /*
                                 MC0.0 - DWG Release 1.1
                                 AC1.2 - DWG Release 1.2
                                 AC1.4 - DWG Release 1.4
                                 AC1.50 - DWG Release 2.0
                                 AC2.10 - DWG Release 2.10
                                 AC1002 - DWG Release 2.5
                                 AC1003 - DWG Release 2.6
                                 AC1004 - DWG Release 9
                                 AC1006 - DWG Release 10
                                 AC1009 - DWG Release 11/12 (LT R1/R2)
                                 AC1012 - DWG Release 13 (LT95)
                                 AC1014 - DWG Release 14, 14.01 (LT97/LT98)
                                 AC1015 - DWG AutoCAD 2000/2000i/2002
                                 AC1018 - DWG AutoCAD 2004/2005/2006
                                 AC1021 - DWG AutoCAD 2007/2008/2009
                                 AC1024 - DWG AutoCAD 2010/2011/2012
                                 AC1027 - DWG AutoCAD 2013/2014/2015/2016/2017
                                 AC1032 - DWG AutoCAD 2018/2019/2020
                                 */


                                db.DxfOut(nome_fim, 16, DwgVersion.AC1024);
                            }
                            catch (System.Exception ex)
                            {
                                erros.Add(new Report(ex, arq.Nome + ".DWG"));
                                Conexoes.Utilz.Apagar(nome_fim);
                                //Conexoes.Utilz.Alerta(ex);
                            }
                            doc.CloseAndDiscard();
                        }

                        w.somaProgresso();
                    }
                    w.Close();
                    Conexoes.Utilz.ShowReports(erros);
                }

            }
        }
        [CommandMethod(nameof(converterDWG))]
        public static void converterDWG()
        {
            var pasta = Conexoes.Utilz.Selecao.SelecionarPasta();
            if (pasta.Existe())
            {
                var destino = Conexoes.Utilz.Selecao.SelecionarPasta("Selecione o destino", pasta);
                if (destino.Existe())
                {
                    var arquivos = pasta.GetArquivos("*.DXF").ListaSelecionarVarios();
                    if (arquivos.Count > 0)
                    {
                        List<Report> erros = new List<Report>();
                      var w=  Conexoes.Utilz.Wait(arquivos.Count, $"Aguarde... Convertendo [{arquivos.Count}] itens");
                        List<string> arquivos_dwg = new List<string>();
                        foreach (var arq in arquivos)
                        {
                            var nome_fim = $@"{destino}\{arq.Nome}.dwg";
                            arquivos_dwg.Add(nome_fim);

                            try
                            {

                                using (Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(arq.Endereco, false))
                                {
                                  
                                    doc.Database.SaveAs(nome_fim, DwgVersion.AC1021);
                                    doc.CloseAndDiscard();
                                }
                                if(nome_fim.Existe())
                                {
                                    using (Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(nome_fim, false))
                                    {
                                        Core.LimparDesenho(doc);
                                        doc.CloseAndSave(nome_fim);
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                erros.Add(new Report(ex, arq.Endereco));
                            }

                            w.somaProgresso();
                        }

                        w.Close();
                        Conexoes.Utilz.ShowReports(erros);
                    }
                }

            }
        }


        [CommandMethod(nameof(medabil))]
        public static void medabil()
        {
            
            MenuMarcas.Iniciar();
        }

        [CommandMethod(nameof(quantificar))]
        public static void quantificar()
        {
            TecnoMetal.Quantificar(true,false,true,true,false);
        }

        [CommandMethod(nameof(marcarmontagem))]
        public static void marcarmontagem()
        {
         if(menu_bloco==null)
            {
                menu_bloco = new Menus.Menu_Bloco_Peca(TecnoMetal);
                menu_bloco.Show();
            }
         else
            {
                menu_bloco.txt_escala.Text = TecnoMetal.GetEscala().ToString();
                menu_bloco.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [CommandMethod(nameof(offtec))]
        public static void offtec()
        {
            FLayer.Desligar(CADVars.LayersMarcasDesligar);
        }

        [CommandMethod(nameof(mercadorias21))]
        public  static void mercadorias21()
        {
            CADTecnoMetal p = new CADTecnoMetal();
            p.Mercadorias();
        }
        [CommandMethod(nameof(materiais21))]
        public static void materiais21()
        {
            CADTecnoMetal p = new CADTecnoMetal();
            p.Materiais();
        }
        [CommandMethod(nameof(tratamentos21))]
        public static void tratamentos21()
        {
            CADTecnoMetal p = new CADTecnoMetal();
            p.Tratamentos();
        }

        [CommandMethod(nameof(criarlayersPadrao))]
        public static void criarlayersPadrao()
        {
            CADBase p = new CADBase();
            p.CriarLayersPadrao();
        }

        [CommandMethod(nameof(gerardxf))]
        public static void gerardxf()
        {
            TecnoMetal.GerarDXFs();
        }

        [CommandMethod(nameof(testeinterseccao))]
        public static void testeinterseccao()
        {
            Ut.InterSectionPoint();
        }

        [CommandMethod(nameof(preencheSelo))]
        static public void preencheSelo()
        {
            acDoc.IrLayout();
            acDoc.SetLts(10);
            Ut.ZoomExtend();
            TecnoMetal.InserirTabelaAuto();
            TecnoMetal.PreencheSelo();

        }
        /*Esse cara força o CAD rodar sincrono, CommandFlags.Session*/
        [CommandMethod(nameof(tabela_limpa))]

        static public void tabela_limpa()
        {
            acDoc.IrLayout();
            acDoc.SetLts(10);
            Ut.ZoomExtend();
            List<Report> erros = new List<Report>();
            TecnoMetal.ApagarTabelaAuto();
            TecnoMetal.PreencheSelo(true);

            Conexoes.Utilz.ShowReports(erros);
        }


        [CommandMethod(nameof(gerarPDFEtapa))]

        static public void gerarPDFEtapa()
        {
            TecnoMetal.GerarPDF();
        }

        [CommandMethod(nameof(gerarPDFEtapacarrega))]

        static public void gerarPDFEtapacarrega()
        {
            var arquivos = Conexoes.Utilz.Arquivo.Ler(TecnoMetal.Pasta + @"DAT\plotar.txt").Select(x=> new Conexoes.Arquivo(x)).ToList();
            arquivos = arquivos.FindAll(x => x.Existe());
            TecnoMetal.GerarPDF(arquivos);
        }

        [CommandMethod(nameof(composicao))]
        static public void composicao()
        {
            TecnoMetal.InserirSoldaComposicao();
        }


        [CommandMethod(nameof(LimparDesenho), CommandFlags.Session | CommandFlags.Modal)]
        public static void LimparDesenho(Document doc)
        {
            if(doc == null) { doc = CAD.acDoc; }
            doc.Comando(
                "_tilemode", "0",/*vai pro layout*/
            "_layout", "r", "", "Layout",/*renomeia o layout para "Layout"*/
            "_zoom", "e",
            "-SCALELISTEDIT", "R", "Y", "e",
            "-SCALELISTEDIT", "d", "*", "e",
            "-overkill", "all",
            "_tilemode", "1",/*vai pro model*/
            "-purge", "A", "*", "N",
            "-overkill", "all", "", "", "d",
            "-layer", "set", "0","",
            "-layer", "off", "mv","",
            "AUDIT", "Y",
            "_tilemode", "0",""
            );/*vai pro layout*/
        }


        [CommandMethod(nameof(ListarQuantidadeBlocos))]
        public static void ListarQuantidadeBlocos()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(acCurDb), OpenMode.ForRead);

                var brclass = RXObject.GetClass(typeof(BlockReference));

                var blocks = acBlkTblRec
                    .Cast<ObjectId>()
                    .Where(id => id.ObjectClass == brclass)
                    .Select(id => (BlockReference)acTrans.GetObject(id, OpenMode.ForRead))
                    .GroupBy(br => ((BlockTableRecord)acTrans.GetObject(
                        br.DynamicBlockTableRecord, OpenMode.ForRead)).Name);

                foreach (var group in blocks.OrderBy(x => x.Key))
                {
                    editor.WriteMessage($"\n{group.Key}: {group.Count()}");
                }
                acTrans.Commit();
            }
        }
    }
}
