using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Ferramentas_DLM.Lisp;
using MIConvexHull;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using static Ferramentas_DLM.CAD;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.EditorInput;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Internal.Reactors;

[assembly: CommandClass(typeof(Ferramentas_DLM.Comandos))]

namespace Ferramentas_DLM
{
    public class Comandos
    {
        private static Menus.Menu_Bloco_Peca menu_bloco { get; set; }
        private static Cotagem _Cotas { get; set; }
        public static Monitoramento monitoramento { get; set; }
        private static MenuMarcas _MenuMarcas { get; set; }
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
        public static Cotagem Cotas
        {
            get
            {
                if(_Cotas ==null)
                {
                    _Cotas = new Cotagem();
                }
                return _Cotas;
            }
        }
        private static TecnoMetal _TecnMetal { get; set; }
        public static TecnoMetal TecnoMetal
        {
            get
            {
                if (_TecnMetal == null)
                {
                    _TecnMetal = new TecnoMetal();
                }
                return _TecnMetal;
            }
        }

        [CommandMethod("listarcomandos")]
        public static void listarcomandos()
        {

            Assembly asm = Assembly.GetExecutingAssembly();
            List<string> comandos = Utilidades.listarcomandos(asm, false).ToList().OrderBy(x=>x).ToList();

            editor.WriteMessage("=== Lista de comandos ===\n");
            foreach (var s in comandos)
            {
                editor.WriteMessage($"---> {s.ToUpper()}\n");
            }
        }

        [CommandMethod("lcotas")]
        public static void LimparCotas()
        {
            Cotagem pp = new Cotagem();
            pp.ApagarCotas();
        }
        [CommandMethod("cotar")]
        public static void Cotar()
        {
            Cotas.Cotar();

        }
        [CommandMethod("cconfigurar")]
        public static void configurar()
        {

            Cotas.Configurar();
        }
        [CommandMethod("contornar")]
        public static void contornar()
        {


            Cotas.Contornar();


        }
        [CommandMethod("cco")]
        public static void cco()
        {

            Cotas.ConfigurarContorno();
            Cotas.Contornar();
        }


        [CommandMethod("ccb")]
        public static void ccb()
        {


            Cotas.Contornar(false);


        }
        [CommandMethod("ccv")]
        public static void ccv()
        {


            Cotas.ContornarConvexo();


        }


        [CommandMethod("desenharmline")]
        public static void desenharmline()
        {
            var estilo = Conexoes.Utilz.SelecionaCombo(Constantes.GetArquivosMlStyles().GetEstilos(), null);
            if(estilo!=null)
            {
                var ml = Constantes.GetArquivosMlStyles().GetEstilo(estilo);
                if (ml!=null)
                {
                    var pts = Utilidades.PedirPontos3D();
                    if (pts.Count > 0)
                    {
                        Multiline.DesenharMLine(estilo, ml.Arquivo, pts);
                    }
                }

            }

        }


        [CommandMethod("substituirpolylinepormultiline")]
        public static void substituirpolylinepormultiline()
        {
            Multiline.MudarPolyline();

        }
        [CommandMethod("mudarmultiline")]
        public static void mudarmultiline()
        {
            Multiline.MudarMultiline();
        }


        [CommandMethod("purlin")]
        public static void purlin()
        {
            CADPurlin p = new CADPurlin();

            var estilos = p.GetMLStyles();
            var layers = FLayer.Get();
            TercasMenu mm = new TercasMenu();
            mm.furos_manuais_layer.Items.AddRange(layers.ToArray());
            mm.correntes_mlstyles.Items.AddRange(p.CorrenteMLStyles.FindAll(x => estilos.Find(y => y == x) != null).ToArray());
            mm.tirantes_mlstyles.Items.AddRange(p.TirantesMLStyles.FindAll(x => estilos.Find(y => y == x) != null).ToArray());
            mm.tercas_mlstyles.Items.AddRange(p.TercasMLStyles.FindAll(x => estilos.Find(y => y == x) != null).ToArray());



            if (layers.Find(x => x == p.MapeiaFurosManuaisLayer) != null)
            {
                mm.furos_manuais_layer.Text = p.MapeiaFurosManuaisLayer;
            }
            else if(mm.furos_manuais_layer.Items.Count>0)
            {
                mm.furos_manuais_layer.SelectedIndex = 0;
            }

            if(mm.ficha_de_pintura.Items.Count>0)
            {
                mm.ficha_de_pintura.Text = "FICHA 01";
            }


            mm.propertyGrid1.SelectedObject = p;
            mm.ShowDialog();

            p.TranspassePadrao = (double)mm.transpasse_padrao.Value;
            p.OffsetApoio = (double)mm.ofsset_apoio.Value;
            p.FichaDePintura = mm.ficha_de_pintura.Text;
            p.MapeiaFurosManuais = mm.mapeia_furos_manuais.Checked;
            p.MapeiaFurosManuaisLayer = mm.furos_manuais_layer.Text;
            p.MapearTirantes = mm.mapeia_tirantes.Checked;
            p.MapearCorrentes = mm.mapeia_correntes.Checked;
            p.MapearTercas = mm.mapeia_tercas.Checked;
            
            p.TercasMLStyles = mm.tercas_mlstyles.Items.Cast<string>().ToList();
            p.TirantesMLStyles = mm.tirantes_mlstyles.Items.Cast<string>().ToList();
            p.CorrenteMLStyles = mm.correntes_mlstyles.Items.Cast<string>().ToList();


            if (mm.id_terca != 1763)
            {
                var pc = Conexoes.DBases.GetBancoRM().GetRME(p.id_terca);
                if (pc != null)
                {
                    p.SetTerca(mm.id_terca);
                }
            }

            if (mm.acao == "perfil")
            {
                p.SetPerfil();
            }
            else if (mm.acao == "mapear")
            {
                p.Mapear();
            }
            else if (mm.acao == "transpasse")
            {
                p.SetTranspasse();
            }
            else if (mm.acao == "ficha")
            {
                p.SetFicha();
            }
            else if (mm.acao == "tabela")
            {
                p.Exportar(true, false);
            }
            else if (mm.acao == "exportar")
            {
                p.Exportar();
            }
            else if (mm.acao == "fixacao")
            {
                p.SetSuporte();
            }
            else if (mm.acao == "ver")
            {
                p.Editar(true);
            }
            else if (mm.acao == "troca_corrente")
            {
                p.SetCorrente();
            }
            else if (mm.acao == "descontar_corrente")
            {
                p.SetCorrenteDescontar();
            }
            else if (mm.acao == "fixador_corrente")
            {
                p.SetCorrenteFixador();
            }
            else if (mm.acao == "excluir")
            {
                p.ExcluirBlocosMarcas();
            }
            else if (mm.acao == "marcacao_purlin")
            {
                p.PurlinManual();
            }
            else if (mm.acao == "purlin_edicao_completa")
            {
                p.EdicaoCompleta();
            }
            else if (mm.acao == "boneco")
            {
                p.GetBoneco_Purlin();
            }
            else if (mm.acao == "gerarcroqui")
            {
                p.GerarCroquis();
            }
        }
      
        [CommandMethod("mapeiapurlins")]
        public static void mapeiapurlins()
        {
            CADPurlin P = new CADPurlin();
            P.Mapear();

        }
        [CommandMethod("boneco")]
        public static void boneco()
        {
            CADPurlin P = new CADPurlin();
            P.GetBoneco_Purlin();
        }
        [CommandMethod("mudaperfiltercas")]
        public static void mudaperfiltercas()
        {
            CADPurlin P = new CADPurlin();
            P.SetPerfil();

        }


        
        
        [CommandMethod("passarela")]
        public static void passarela()
        {
            Telhas pp = new Telhas();
            pp.InserirPassarela();
        }
        [CommandMethod("apagapassarela")]
        public static void apagapassarela()
        {
            Telhas pp = new Telhas();
            pp.ApagarPassarelas();
        }
        [CommandMethod("linhadevida")]
        public static void linhadevida()
        {
            Telhas pp = new Telhas();
            pp.InserirLinhaDeVida();
        }
        [CommandMethod("rlinhadevida")]
        public static void rlinhadevida()
        {
            Telhas pp = new Telhas();
            pp.InserirLinhaDeVida(true);
        }
        [CommandMethod("apagalinhadevida")]
        public static void apagalinhadevida()
        {
            Telhas pp = new Telhas();
            pp.ApagarLinhaDeVida();
        }
        [CommandMethod("rpassarela")]
        public static void rpassarela()
        {
            Telhas pp = new Telhas();
            pp.InserirPassarela(true);
        }
        [CommandMethod("alinharlinhadevida")]
        public static void alinharlinhadevida()
        {
            Telhas pp = new Telhas();
            pp.AlinharLinhaDeVida();
        }
       
        
        




        [CommandMethod("monitorar")]
        public static void monitorar()
        {
            monitoramento = new Monitoramento();

        }

        [CommandMethod("salvarlog")]
        public static void salvarlog()
        {
            if (monitoramento != null)
            {
                monitoramento.SalvarLog();
            }
        }
        
        
        
        [CommandMethod("setarLTS")]
        public static void setarLTS()
        {

            ClasseBase b = new ClasseBase();
            b.SetLts();
        }



        [CommandMethod("abrepasta")]
        public static void abrepasta()
        {
            ClasseBase pp = new ClasseBase();
            pp.AbrePasta();
        }


        [CommandMethod("exportarma")]
        public static void exportarma()
        {
            Telhas pp = new Telhas();
            pp.ExportarRMAdeTabela();
        }
        [CommandMethod("importarm")]
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


        [CommandMethod("tabelatecnometal")]
        public static void tabelatecnometal()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirTabela();
        }

        [CommandMethod("tabelatecnometalauto")]
        public static void tabelatecnometalauto()
        {
            TecnoMetal pp = new TecnoMetal();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            pp.InserirTabelaAuto(ref erros);

            Conexoes.Utilz.ShowReports(erros);
        }

        
        
        
        [CommandMethod("selopreenche")]
        public static void selopreenche()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.PreencheSelo();
        }
        [CommandMethod("selolimpar")]
        public static void selolimpar()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.PreencheSelo(true);
        }

        
        [CommandMethod("rodarmacros")]
        public static void rodarmacros()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.RodarMacros();
        }


        [CommandMethod("gerardbf3d")]
        public static void gerardbf3d()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.GerarDBF3D();
        }
        [CommandMethod("gerardbf")]
        public static void gerardbf()
        {


            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            var tbl = TecnoMetal.GerarDBF(ref erros,Conexoes.Utilz.Pergunta("Atualizar CAMs?\nAo ativar essa opção também será verificado CAM x Projeto"));

            if (File.Exists(tbl.Banco))
            {
          
                if(erros.FindAll(x=>x.Tipo == Conexoes.TipoReport.Crítico).Count>0)
                {
                    Conexoes.Utilz.ShowReports(erros);
                }
                else
                {
                    Conexoes.Utilz.Alerta($"Arquivo {tbl.Banco} gerado!", "", System.Windows.MessageBoxImage.Information);
                    Conexoes.Utilz.Abrir(Conexoes.Utilz.getPasta(tbl.Banco));
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

        [CommandMethod("listarquantidadeblocos")]
        public static void listarquantidadeblocos()
        {
            Utilidades.ListarQuantidadeBlocos();
        }


        [CommandMethod("arremate")]
        public static void arremate()
        {

            TecnoMetal pp = new TecnoMetal();
            pp.InserirArremate(pp.Getescala());
        }
        [CommandMethod("chapa")]
        public  static void chapa()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirChapa(pp.Getescala());

        }

        [CommandMethod("unitario")]
        public static void unitario()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirElementoUnitario(pp.Getescala());
        }
        [CommandMethod("elem2")]
        public static void elem2()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirElementoM2(pp.Getescala());
        }

        [CommandMethod("criarmarcasdeexcel")]
        public static void criarmarcasdeexcel()
        {




            CriarMarcas mm = new CriarMarcas();
            mm.Show();

        }
        [CommandMethod("criarmarcasdecam")]
        public static void criarmarcasdecam()
        {

            var arqs = Conexoes.Utilz.AbrirArquivos("Selecione os arquivos", new List<string> { "CAM" });
            var offset = Cotas.Getescala() * 70;
            if (arqs.Count > 0)
            {
                bool cancelado = false;
                var p0 = Utilidades.PedirPonto3D("\nSelecione a origem", out cancelado);
                var x0 = p0.X;
                var y0 = p0.Y;
                int c = 1;

                if (!cancelado)
                {
                    foreach (var s in arqs)
                    {

                        DLMCam.ReadCam cam = new DLMCam.ReadCam(s);
                        Blocos.CamToMarcaSimples(cam, p0, Cotas.Getescala());

                        p0 = new Point3d(p0.X + offset, p0.Y, p0.Z);

                        if (c == 10)
                        {
                            p0 = new Point3d(x0, p0.Y + (offset / 2), p0.Z);
                            c = 1;
                        }
                        c++;
                    }
                }
            }

        }


        [CommandMethod("bloqueiamviews")]
        public static void bloqueiamviews()
        {
            ClasseBase b = new ClasseBase();
            b.SetViewport(true);
        }

        [CommandMethod("desbloqueiamviews")]
        public static void desbloqueiamviews()
        {
            ClasseBase b = new ClasseBase();
            b.SetViewport(false);
        }



        [CommandMethod("teste")]
        public static void teste()
        {
            TecnoMetal pp = new TecnoMetal();
        }

        [CommandMethod("marcar")]
        public static void marcar()
        {
            MenuMarcas.Iniciar();
        }

        [CommandMethod("medabil")]
        public static void medabil()
        {
            MenuMarcas.Iniciar();
        }

        [CommandMethod("quantificar")]
        public static void quantificar()
        {
            TecnoMetal.Quantificar(true,false,true,true,false);
        }

        [CommandMethod("marcarmontagem")]
        public static void marcarmontagem()
        {
         if(menu_bloco==null)
            {
                menu_bloco = new Menus.Menu_Bloco_Peca(TecnoMetal);
                menu_bloco.Show();
            }
         else
            {
                menu_bloco.txt_escala.Text = TecnoMetal.Getescala().ToString();
                menu_bloco.Visibility = System.Windows.Visibility.Visible;
            }
        }

        [CommandMethod("offtec")]
        public static void offtec()
        {
            FLayer.Desligar(Constantes.LayersMarcasDesligar);
        }



        [CommandMethod("mercadorias21")]
        public  static void mercadorias()
        {
            TecnoMetal p = new TecnoMetal();
            p.Mercadorias();
        }
        [CommandMethod("materiais21")]
        public static void materiais()
        {
            TecnoMetal p = new TecnoMetal();
            p.Materiais();
        }
        [CommandMethod("tratamentos21")]
        public static void tratamentos()
        {
            TecnoMetal p = new TecnoMetal();
            p.Tratamentos();
        }


        [CommandMethod("criarlayersPadrao")]
        public static void criarlayersPadrao()
        {
            ClasseBase p = new ClasseBase();
            p.CriarLayersPadrao();
        }



        [CommandMethod("gerardxf")]
        public static void gerardxf()
        {
            TecnoMetal.GerarDXFs();
        }

        [CommandMethod("testeinterseccao")]
        public static void testeinterseccao()
        {
            Utilidades.InterSectionPoint();
        }


        [CommandMethod("testeeixos")]
        public static void testeeixos()
        {
            ClasseBase p = new ClasseBase();
            p.SelecionarObjetos();
            var eixos = p.GetEixos();
        }


        [CommandMethod("preenche")]

        static public void preenche()
        {
            TecnoMetal.IrLayout();
            TecnoMetal.SetLts(10);
            TecnoMetal.ZoomExtend();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            TecnoMetal.InserirTabelaAuto(ref erros);
            TecnoMetal.PreencheSelo();

            Conexoes.Utilz.ShowReports(erros);
        }

        [CommandMethod("limpa")]

        static public void limpa()
        {
            TecnoMetal.IrLayout();
            TecnoMetal.SetLts(10);
            TecnoMetal.ZoomExtend();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            TecnoMetal.ApagarTabelaAuto();
            TecnoMetal.PreencheSelo(true);

            Conexoes.Utilz.ShowReports(erros);
        }


        [CommandMethod("gerarPDFEtapa")]

        static public void gerarPDFEtapa()
        {
            TecnoMetal.GerarPDF();
        }

        [CommandMethod("gerarPDFEtapacarrega")]

        static public void gerarPDFEtapacarrega()
        {
            var arquivos = Conexoes.Utilz.Arquivo.Ler(TecnoMetal.Pasta + @"DAT\plotar.txt").Select(x=> new Conexoes.Arquivo(x)).ToList();
            arquivos = arquivos.FindAll(x => x.Existe);
            TecnoMetal.GerarPDF(arquivos);
        }

        [CommandMethod("composicao")]

        static public void composicao()
        {
            TecnoMetal.InserirSoldaComposicao();
        }


        [CommandMethod("limpardesenho")]
        public static void limpardesenho()
        {
            Utilidades.LimparDesenho();
            
        }


    }
}
