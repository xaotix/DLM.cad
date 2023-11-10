﻿using Autodesk.AutoCAD.ApplicationServices;
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
using DLM.desenho;
using DLM.vars.cad;

[assembly: CommandClass(typeof(DLM.cad.Core))]

namespace DLM.cad
{
    public class Core
    {
        private static Conexoes.ControleWait _w { get; set; }
        private static MenuMarcas _MenuMarcas { get; set; }
        private static CADCotagem _Cotas { get; set; }
        private static CADPurlin _cadPurlin { get; set; }
        private static CADTecnoMetal _TecnMetal { get; set; }

        private static Menus.Menu_Bloco_Peca _menu_bloco { get; set; }


        public static CADMonitoramento monitoramento { get; set; }

        public static Conexoes.ControleWait Getw()
        {
            if (_w == null)
            {
                _w = Conexoes.Utilz.Wait(100, "");
            }
            return _w;
        }

        public static MenuMarcas GetMenuMarcas()
        {
            if (_MenuMarcas == null)
            {
                _MenuMarcas = new MenuMarcas(GetTecnoMetal());
            }
            return _MenuMarcas;
        }

        public static CADCotagem GetCotas()
        {
            if (_Cotas == null)
            {
                _Cotas = new CADCotagem();
            }
            return _Cotas;
        }

        public static CADTecnoMetal GetTecnoMetal()
        {
            if (_TecnMetal == null)
            {
                _TecnMetal = new CADTecnoMetal();
            }
            return _TecnMetal;
        }

        public static CADPurlin GetCADPurlin()
        {
            if (_cadPurlin == null)
            {
                _cadPurlin = new CADPurlin();
            }
            return _cadPurlin;
        }




        [CommandMethod(nameof(GetMLStylesNames))]
        public static void GetMLStylesNames()
        {
            GetCADPurlin().SelecionarObjetos(Tipo_Selecao.MultiLines);

            var mls = GetCADPurlin().GetMultilines().GetMlineStyles().Select(x => x.Name).Distinct().ToList();
            foreach (var ml in mls)
            {
                Ut.AddMensagem($"\n{ml}");
            }
        }

        [CommandMethod(nameof(listarcomandos))]
        public static void listarcomandos()
        {

            Assembly asm = Assembly.GetExecutingAssembly();
            List<string> comandos = Ut.listarcomandos(asm, false).ToList().OrderBy(x => x).ToList();

            Ut.AddMensagem("=== Lista de comandos ===\n");
            foreach (var s in comandos)
            {
                Ut.AddMensagem($"---> {s.ToUpper()}\n");
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
            GetCotas().Cotar();

        }
        [CommandMethod(nameof(cconfigurar))]
        public static void cconfigurar()
        {

            GetCotas().Configurar();
        }
        [CommandMethod(nameof(contornar))]
        public static void contornar()
        {


            GetCotas().Contornar();


        }
        [CommandMethod(nameof(cco))]
        public static void cco()
        {

            GetCotas().ConfigurarContorno();
            GetCotas().Contornar();
        }

        [CommandMethod(nameof(getContorno_polilinhas))]
        public static void getContorno_polilinhas()
        {
            Contorno.GetContornoPolyLines();
        }

        [CommandMethod(nameof(getcontorno))]
        public static void getcontorno()
        {
            GetCotas().Contornar();
        }
        [CommandMethod(nameof(getcontorno_convexo))]
        public static void getcontorno_convexo()
        {
            GetCotas().ContornarConvexo();
        }
        [CommandMethod(nameof(getcontorno_linhas))]
        public static void getcontorno_linhas()
        {
            var sel = GetCotas().SelecionarObjetos(CAD_TYPE.LINE, CAD_TYPE.LWPOLYLINE, CAD_TYPE.POLYLINE);
            if (sel.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK && GetCotas().Selecoes.Count > 0)
            {
                var pts = Ut.GetPontos(GetCotas().Selecoes);
                var p3ds = pts.Select(x => new P3d(x.X, x.Y, x.Z)).ToList();
                var contorno = p3ds.GetContornoHull(Ut.PedirInteger("Digite a escala", 1), Ut.PedirDouble("Digite a concavidade", 0.5));
                GetCotas().AddPolyLine(contorno, 0, 10, System.Drawing.Color.Red);
            }
        }


        [CommandMethod(nameof(desenharmline))]
        public static void desenharmline()
        {
            var estilo = FuncoesCAD.GetArquivosMlStyles().GetEstilos().ListaSelecionar();
            if (estilo != null)
            {
                var ml = FuncoesCAD.GetArquivosMlStyles().GetEstilo(estilo);
                if (ml != null)
                {
                    var pts = Ut.PedirPontos3D();
                    if (pts.Count > 0)
                    {
                        Multiline.DesenharMLine(estilo, ml.Arquivo, pts.Point3d());
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




        [CommandMethod(nameof(renomeiablocos))]
        public static void renomeiablocos()
        {
            GetCADPurlin().RenomeiaBlocos();
        }






        [CommandMethod(nameof(boneco))]
        public static void boneco()
        {
            GetCADPurlin().GetBoneco_Purlin();
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
            using (var acTrans = acCurDb.acTrans())
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
                        DLM.log.Log(ex);
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
            GetCotas().AbrePasta();
        }


        [CommandMethod(nameof(ExportaRMA))]
        public static void ExportaRMA()
        {
            var pp = new CADTelhas();
            pp.ExportarRMAdeTabela();
        }
        [CommandMethod(nameof(ImportaRMOffline))]
        public static void ImportaRMOffline()
        {
            string arquivo = Conexoes.Utilz.Abrir_String(Cfg.Init.EXT_RM, "Selecione");
            if (File.Exists(arquivo))
            {
                var pp = new Conexoes.DBRM_Offline();
                var s = pp.Ler(arquivo);
                Tabelas.InserirTabela(s);
            }

        }


        [CommandMethod(nameof(ImportaRM))]
        public static void ImportaRM()
        {
            var etapa = GetTecnoMetal().GetSubEtapa();
            if (etapa != null)
            {
                var dbrm = new DBRM_User(etapa.GetObra().id_plm_tercas, etapa.GetPedido().id_plm_tercas, etapa.id_plm_tercas, etapa.PEP, Acessos_Criterio.ENG);
                dbrm.Update(RM_Tipo.Tudo);
                Tabelas.InserirTabela(dbrm);
            }
        }



        [CommandMethod(nameof(TabelaTecnometal))]
        public static void TabelaTecnometal()
        {
            GetTecnoMetal().InserirTabela();
        }

        [CommandMethod(nameof(CAMsChapasRetas))]
        public static void CAMsChapasRetas()
        {
            GetTecnoMetal().GerarCamsChapasRetas();
        }

        [CommandMethod(nameof(AtualizarPesoChapas))]
        public static void AtualizarPesoChapas()
        {
            var cad = new CADTecnoMetal();
            var sel = cad.SelecionarObjetos();
            if (sel.Status != PromptStatus.OK)
            {
                return;
            }


            var blks = cad.Selecoes.Filter<BlockReference>();
            if (blks.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Nada Selecionado. Selecionar tudo?"))
                {
                    cad.SelecionarTudo();
                    blks = cad.Selecoes.Filter<BlockReference>();
                }

            }
            if (blks.Count == 0)
            {
                return;
            }

            var err = cad.AtualizarPesoChapa(blks);
            if (err.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Pesos Atualizados! Deseja gerar/atualizar a tabela?}"))
                {
                    cad.InserirTabelaAuto();
                }
            }
        }

        [CommandMethod(nameof(TrocarPerfilElementoMetroQuadrado))]
        public static void TrocarPerfilElementoMetroQuadrado()
        {
            var cad = new CADTecnoMetal();
            var sel = cad.SelecionarObjetos();
            if (sel.Status != PromptStatus.OK)
            {
                return;
            }


            var blks = cad.Selecoes.Filter<BlockReference>();
            if (blks.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Nada Selecionado. Selecionar tudo?"))
                {
                    cad.SelecionarTudo();
                    blks = cad.Selecoes.Filter<BlockReference>();
                }

            }
            if (blks.Count == 0)
            {
                return;
            }
            var novo_Perfil = DBases.GetdbPerfil().GetPerfisTecnoMetal().FindAll(x => x.Tipo == DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez).ListaSelecionar("Selecione o novo perfil");
            if (novo_Perfil == null)
            {
                return;
            }
            if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja trocar o material dos itens selecionados para {novo_Perfil}"))
            {
                var err = cad.TrocarPerfilElementoMetroQuadrado(blks, novo_Perfil);

                if (err.FindAll(x => x.Tipo == TipoReport.Critico).Count == 0)
                {
                    if (Conexoes.Utilz.Pergunta("Materiais atualizados! Deseja gerar/atualizar a tabela?"))
                    {
                        cad.InserirTabelaAuto();
                    }
                }
            }

        }

        [CommandMethod(nameof(TabelaTecnometalAuto))]
        public static void TabelaTecnometalAuto()
        {
            var cad = new CADTecnoMetal();
            var erros = new List<Report>();
            cad.InserirTabelaAuto(ref erros);
            erros.Show();
        }




        [CommandMethod(nameof(SeloPreenche))]
        public static void SeloPreenche()
        {
            GetTecnoMetal().PreencheSelo();
        }
        [CommandMethod(nameof(SeloLimpar))]
        public static void SeloLimpar()
        {
            GetTecnoMetal().PreencheSelo(true);
        }


        [CommandMethod(nameof(RodarMacros), CommandFlags.Session | CommandFlags.Modal)]
        public static void RodarMacros()
        {
            Conexoes.Utilz.Alerta("Descontinuado por muitos problemas de compatibilidade.");
            //CADTecnoMetal pp = new CADTecnoMetal();
            //pp.RodarMacros();
        }


        [CommandMethod(nameof(GerarDBF3d))]
        public static void GerarDBF3d()
        {
            GetTecnoMetal().GerarDBF3D();
        }
        [CommandMethod(nameof(GerarDBF))]
        public static void GerarDBF()
        {


            List<Report> erros = new List<Report>();

            if (!GetTecnoMetal().Pasta.ToUpper().EndsWith($@".{Cfg.Init.EXT_Etapa}\"))
            {
                erros.Add(new Report("Pasta Inválida", $"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                    $"\nPasta atual: {GetTecnoMetal().Pasta}", DLM.vars.TipoReport.Critico));

                erros.Show();
                return;
            }

            var lista_pecas = GetTecnoMetal().GetPecasPranchas(ref erros);
            var etapa = GetTecnoMetal().GetSubEtapa();

            Conexoes.Utilz.DBF.Gerar(etapa, lista_pecas, ref erros);
        }


        [CommandMethod(nameof(CAMPolyLine))]
        public static void CAMPolyLine()
        {
            GetTecnoMetal().CAM_de_Polilinha();
        }


        [CommandMethod(nameof(InserirArremate))]
        public static void InserirArremate()
        {
            GetTecnoMetal().InserirArremate();
        }
        [CommandMethod(nameof(InserirChapa))]
        public static void InserirChapa()
        {
            GetTecnoMetal().InserirChapa();
        }

        [CommandMethod(nameof(InserirUnitario))]
        public static void InserirUnitario()
        {
            GetTecnoMetal().InserirElementoUnitario();
        }
        [CommandMethod(nameof(InserirElem2))]
        public static void InserirElem2()
        {
            GetTecnoMetal().InserirElementoM2();
        }

        [CommandMethod(nameof(CriarmarcasdeExcel))]
        public static void CriarmarcasdeExcel()
        {




            CriarMarcas mm = new CriarMarcas();
            mm.Show();

        }
        [CommandMethod(nameof(CriarMarcasdeCAM))]
        public static void CriarMarcasdeCAM()
        {

            var cams = Conexoes.Utilz.Abrir_Strings(Cfg.Init.EXT_CAM);
            var offset = GetCotas().GetEscala() * 70;
            if (cams.Count > 0)
            {
                bool cancelado = false;
                var p0 = Ut.PedirPonto("\nSelecione a origem", out cancelado);
                var x0 = p0.X;
                var y0 = p0.Y;
                int c = 1;

                if (!cancelado)
                {
                    foreach (var s in cams)
                    {

                        var cam = new DLM.cam.ReadCAM(s);
                        Blocos.CamToMarcaSimples(cam, p0, GetCotas().GetEscala());

                        p0 = new P3d(p0.X + offset, p0.Y);

                        if (c == 10)
                        {
                            p0 = new P3d(x0, p0.Y + (offset / 2));
                            c = 1;
                        }
                        c++;
                    }
                }
            }

        }


        [CommandMethod(nameof(Bloqueiamviews))]
        public static void Bloqueiamviews()
        {
            GetCotas().SetViewport(true);
        }

        [CommandMethod(nameof(Desbloqueiamviews))]
        public static void Desbloqueiamviews()
        {
            GetCotas().SetViewport(false);
        }

        [CommandMethod(nameof(AjustarLayers))]
        public static void AjustarLayers()
        {
            GetCotas().AjustarLayers();
            GetCotas().AddMensagem("Finalizado!");
        }




        [CommandMethod(nameof(marcar))]
        public static void marcar()
        {
            GetMenuMarcas().Iniciar();
        }

        [CommandMethod(nameof(ConverterDXF))]
        public static void ConverterDXF()
        {
            List<Report> erros = new List<Report>();
            var pasta = Conexoes.Utilz.Selecao.SelecionarPasta();
            if (pasta.Exists())
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
                                nome_fim.Delete();
                                //Conexoes.Utilz.Alerta(ex);
                            }
                            doc.CloseAndDiscard();
                        }

                        w.somaProgresso();
                    }
                    w.Close();
                    erros.Show();
                }

            }
        }
        [CommandMethod(nameof(ConverterDWG))]
        public static void ConverterDWG()
        {
            var pasta = Conexoes.Utilz.Selecao.SelecionarPasta();
            if (pasta.Exists())
            {
                var destino = Conexoes.Utilz.Selecao.SelecionarPasta("Selecione o destino", pasta);
                if (destino.Exists())
                {
                    var arquivos = pasta.GetArquivos("*.DXF").ListaSelecionarVarios();
                    if (arquivos.Count > 0)
                    {
                        List<Report> erros = new List<Report>();
                        var w = Conexoes.Utilz.Wait(arquivos.Count, $"Aguarde... Convertendo [{arquivos.Count}] itens");
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
                                if (nome_fim.Exists())
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
                        erros.Show();
                    }
                }

            }
        }


        [CommandMethod(nameof(Medabil))]
        public static void Medabil()
        {

            GetMenuMarcas().Iniciar();
        }

        [CommandMethod(nameof(Quantificar))]
        public static void Quantificar()
        {
            GetTecnoMetal().Quantificar(true, false, true, true, false);
        }

        [CommandMethod(nameof(MarcarMontagem))]
        public static void MarcarMontagem()
        {
            if (_menu_bloco == null)
            {
                _menu_bloco = new Menus.Menu_Bloco_Peca(GetTecnoMetal());
                _menu_bloco.Show();
            }
            else
            {
                _menu_bloco.txt_escala.Text = GetTecnoMetal().GetEscala().String(0);
                _menu_bloco.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [CommandMethod(nameof(DesligarLayersMarcas))]
        public static void DesligarLayersMarcas()
        {
            FLayer.Desligar(Cfg.Init.GetLayersMarcasDesligar());
        }

        [CommandMethod(nameof(mercadorias21))]
        public static void mercadorias21()
        {
            GetTecnoMetal().Mercadorias();
        }
        [CommandMethod(nameof(materiais21))]
        public static void materiais21()
        {
            GetTecnoMetal().Materiais();
        }
        [CommandMethod(nameof(tratamentos21))]
        public static void tratamentos21()
        {
            GetTecnoMetal().Tratamentos();
        }

        [CommandMethod(nameof(CriarlayersPadrao))]
        public static void CriarlayersPadrao()
        {
            GetCotas().CriarLayersPadrao();
        }

        [CommandMethod(nameof(gerardxf))]
        public static void gerardxf()
        {
            GetTecnoMetal().GerarDXFs();
        }

        [CommandMethod(nameof(testeinterseccao))]
        public static void testeinterseccao()
        {
            Ut.InterSectionPoint();
        }

        [CommandMethod(nameof(PreencheSelo))]
        public static void PreencheSelo()
        {
            acDoc.IrLayout();
            acDoc.SetLts(10);
            Ut.ZoomExtend();
            GetTecnoMetal().InserirTabelaAuto();
            GetTecnoMetal().PreencheSelo();

        }
        /*Esse cara força o CAD rodar sincrono, CommandFlags.Session*/
        [CommandMethod(nameof(TabelaLimpar))]
        public static void TabelaLimpar()
        {
            acDoc.IrLayout();
            acDoc.SetLts(10);
            Ut.ZoomExtend();
            List<Report> erros = new List<Report>();
            GetTecnoMetal().ApagarTabelaAuto();
            GetTecnoMetal().PreencheSelo(true);

            erros.Show();
        }


        [CommandMethod(nameof(GerarPDFEtapa))]
        public static void GerarPDFEtapa()
        {
            GetTecnoMetal().GerarPDF();
        }

        [CommandMethod(nameof(GerarPDFEtapacarrega))]
        public static void GerarPDFEtapacarrega()
        {
            var arquivos = Conexoes.Utilz.Arquivo.Ler(GetTecnoMetal().Pasta + @"DAT\plotar.txt").Select(x => new Conexoes.Arquivo(x)).ToList();
            arquivos = arquivos.FindAll(x => x.Exists());
            GetTecnoMetal().GerarPDF(arquivos);
        }

        [CommandMethod(nameof(Composicao))]
        public static void Composicao()
        {
            GetTecnoMetal().InserirSoldaComposicao();
        }


        [CommandMethod(nameof(LimparDesenho), CommandFlags.Session | CommandFlags.Modal)]
        public static void LimparDesenho(Document doc)
        {

            if (doc == null) { doc = CAD.acDoc; }



            doc.Comando(
            "REMOVEAEC",
            "_tilemode", "0",/*vai pro layout*/
            //"_layout", "r", "", "Layout",/*renomeia o layout para "Layout"*/
            "_zoom", "e",
            "-SCALELISTEDIT", "R", "Y", "e",
            "-SCALELISTEDIT", "d", "*", "e",
            "-overkill", "all ",
            "_tilemode", "1",/*vai pro model*/
            "-purge", "A", "*", "N",
            "-overkill", "all", "", "", "d", "",
            //"-layer", "set", "0","",
            //"-layer", "off", "mv","",
            "AUDIT", "Y",
            "_tilemode", "0", ""
            );/*vai pro layout*/
        }


        [CommandMethod(nameof(ListarQuantidadeBlocos))]
        public static void ListarQuantidadeBlocos()
        {
            using (var acTrans = acCurDb.acTransST())
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
                    Ut.AddMensagem($"\n{group.Key}: {group.Count()}");
                }
                acTrans.Commit();
            }
        }



        [CommandMethod(nameof(xpurlinMudarPerfil))]
        public static void xpurlinMudarPerfil()
        {
            GetCADPurlin().SetPurlin();
        }

        [CommandMethod(nameof(xpurlinApagarBlocos))]
        public static void xpurlinApagarBlocos()
        {
            var sel = GetCADPurlin().SelecionarObjetos(Tipo_Selecao.Blocos);
            if (sel.Status == PromptStatus.OK)
            {
                GetCADPurlin().LimparBlocos();
            }
        }


        [CommandMethod(nameof(xpurlinvista))]
        public static void xpurlinvista()
        {
            GetCADPurlin().Purlin();
        }

        public static DrawPurlin PurlinOpt { get; set; } = new DrawPurlin();
        [CommandMethod(nameof(purlinXLines))]
        public static void purlinXLines()
        {
            var eixos = new List<P3d>();
            var origens = GetCADPurlin().GetOrigensVerticaisVistaPurlin(out eixos);

            

            if(eixos.Count>0)
            {
                bool cancelado = true;
                var pt = Ut.PedirPonto("Selecione a origem do boneco.",out cancelado);
                if(!cancelado)
                {


                    PurlinOpt.Y = pt.Y;
                    PurlinOpt.X1 = eixos.Min(x => x.X);
                    PurlinOpt.X2 = eixos.Max(x => x.X);

                 
                    if(!PurlinOpt.Propriedades())
                    { return; }

                    var p1 = new P3d(PurlinOpt.X01, PurlinOpt.Y);
                    var p2 = new P3d(PurlinOpt.X02, PurlinOpt.Y);
                    GetCADPurlin().AddLinha(p1, p2, "CONTINUOUS", System.Drawing.Color.Yellow);



                    var pl1 = new P3d(PurlinOpt.X1, PurlinOpt.Y);
                    var pl2 = new P3d(PurlinOpt.X2, PurlinOpt.Y);
                    GetCADPurlin().AddLinha(pl1.MoverY(150), pl1.MoverY(-150), "CONTINUOUS", System.Drawing.Color.Blue);
                    GetCADPurlin().AddLinha(pl2.MoverY(150), pl2.MoverY(150), "CONTINUOUS", System.Drawing.Color.Blue);

                    foreach(var p in origens)
                    {
                        GetCADPurlin().AddLinha(p.MoverY(100), p.MoverY(-100), "CONTINUOUS", System.Drawing.Color.Yellow);
                    }


                    if (Ut.PedirString("Desenhar Xlines?",new List<string> { "Sim","Não"}) == "Sim")
                    {
                        for (int i = 0; i < origens.Count; i++)
                        {
                            if (i == 0)
                            {
                                GetCADPurlin().AddXline(origens[i]);
                            }
                            else if (origens[i].Distancia(origens[i - 1]) > 20)
                            {
                                GetCADPurlin().AddXline(origens[i]);
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }


            
        }
    }
}
