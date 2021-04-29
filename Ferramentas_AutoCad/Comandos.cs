using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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

[assembly: CommandClass(typeof(Ferramentas_DLM.Comandos))]

namespace Ferramentas_DLM
{
    public class Comandos
    {
        public static Monitoramento monitoramento { get; set; }
        public static Cotagem Cotas { get; set; } = new Cotagem();

        [CommandMethod("listarcomandos")]
        public static void listarcomandos()
        {
            DocumentCollection dm = Application.DocumentManager;
            Editor ed = dm.MdiActiveDocument.Editor;
            Assembly asm = Assembly.GetExecutingAssembly();
            List<string> comandos = Utilidades.listarcomandos(asm, false).ToList().OrderBy(x=>x).ToList();

            ed.WriteMessage("=== Lista de comandos ===\n");
            foreach (var s in comandos)
            {
                ed.WriteMessage($"---> {s.ToUpper()}\n");
            }
        }

        [CommandMethod("lcotas")]
        public static void LimparCotas()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;


            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                ed.WriteMessage("Selecione os objetos");
                PromptSelectionOptions sel = new PromptSelectionOptions();

                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    Utilidades.LimparCotas(acTrans, acSSet);


                    // Save the new object to the database
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                    acDoc.Editor.WriteMessage("Finalizado.");
                }
            }
        }
        [CommandMethod("cotar")]
        public static void Cotar()
        {
            Cotas = new Cotagem();
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






        [CommandMethod("purlin")]
        public void purlin()
        {
            CADPurlin p = new CADPurlin();

            var estilos = p.GetMLStyles();
            var layers = p.GetLayers();
            TercasMenu mm = new TercasMenu();
            mm.tirantes_mlstyle.Items.AddRange(estilos.ToArray());
            mm.correntes_mlstyle.Items.AddRange(estilos.ToArray());
            mm.furos_manuais_layer.Items.AddRange(layers.ToArray());
            if (estilos.Find(x => x == p.CorrenteMLStyle) != null)
            {
                mm.correntes_mlstyle.Text = p.CorrenteMLStyle;
            }
            if (estilos.Find(x => x == p.TirantesMLStyle) != null)
            {
                mm.tirantes_mlstyle.Text = p.TirantesMLStyle;
            }

            if (layers.Find(x => x == p.MapeiaFurosManuaisLayer) != null)
            {
                mm.furos_manuais_layer.Text = p.MapeiaFurosManuaisLayer;
            }

            mm.tercas_mlstyles.Items.AddRange(p.TercasMLStyles.FindAll(x => estilos.Find(y => y == x) != null).ToArray());

            mm.propertyGrid1.SelectedObject = p;
            mm.ShowDialog();

            p.TranspassePadrao = (double)mm.transpasse_padrao.Value;
            p.OffsetApoio = (double)mm.ofsset_apoio.Value;
            p.FichaDePintura = mm.ficha_de_pintura.Text;
            p.MapeiaFurosManuais = mm.mapeia_furos_manuais.Checked;
            p.MapeiaFurosManuaisLayer = mm.furos_manuais_layer.Text;
            p.TirantesMLStyle = mm.tirantes_mlstyle.Text;
            p.MapearTirantes = mm.mapeia_tirantes.Checked;

            p.CorrenteMLStyle = mm.correntes_mlstyle.Text;
            p.MapearCorrentes = mm.mapeia_correntes.Checked;
            p.MapearTercas = mm.mapeia_tercas.Checked;
            p.TercasMLStyles = mm.tercas_mlstyles.Items.Cast<string>().ToList();


            if (mm.id_terca != 1763)
            {
                var pc = Conexoes.DBases.GetBancoRM().GetRME(p.id_terca);
                if (pc != null)
                {
                    p.id_terca = mm.id_terca;
                    p.secao = pc.GetCadastroRME().SECAO.ToString();
                    p.tipo = pc.TIPO.Contains("Z") ? "Z" : "C";
                    p.espessura = pc.ESP.ToString("N2").Replace(",", ".");
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
                p.Excluir();
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
        }
      
        [CommandMethod("mapeiapurlins")]
        public void mapeiapurlins()
        {
            CADPurlin P = new CADPurlin();
            P.Mapear();

        }
        [CommandMethod("boneco")]
        public void boneco()
        {
            CADPurlin P = new CADPurlin();
            P.GetBoneco_Purlin();
        }
        [CommandMethod("mudaperfiltercas")]
        public void mudaperfiltercas()
        {
            CADPurlin P = new CADPurlin();
            P.SetPerfil();

        }


        
        
        [CommandMethod("passarela")]
        public void passarela()
        {
            Telhas pp = new Telhas();
            pp.InserirPassarela();
        }
        [CommandMethod("apagapassarela")]
        public void apagapassarela()
        {
            Telhas pp = new Telhas();
            pp.ApagarPassarelas();
        }
        [CommandMethod("linhadevida")]
        public void linhadevida()
        {
            Telhas pp = new Telhas();
            pp.InserirLinhaDeVida();
        }
        [CommandMethod("rlinhadevida")]
        public void rlinhadevida()
        {
            Telhas pp = new Telhas();
            pp.InserirLinhaDeVida(true);
        }
        [CommandMethod("apagalinhadevida")]
        public void apagalinhadevida()
        {
            Telhas pp = new Telhas();
            pp.ApagarLinhaDeVida();
        }
        [CommandMethod("rpassarela")]
        public void rpassarela()
        {
            Telhas pp = new Telhas();
            pp.InserirPassarela(true);
        }
        [CommandMethod("alinharlinhadevida")]
        public void alinharlinhadevida()
        {
            Telhas pp = new Telhas();
            pp.AlinharLinhaDeVida();
        }
       
        
        




        [CommandMethod("monitorar")]
        public void monitorar()
        {
            monitoramento = new Monitoramento();

        }

        [CommandMethod("salvarlog")]
        public void salvarlog()
        {
            if (monitoramento != null)
            {
                monitoramento.SalvarLog();
            }
        }
        
        
        
        [CommandMethod("setarLTS")]
        public void setarLTS()
        {

            ClasseBase b = new ClasseBase();
            b.SetLts();
        }



        [CommandMethod("abrepasta")]
        public void abrepasta()
        {
            ClasseBase pp = new ClasseBase();
            pp.AbrePasta();
        }


        [CommandMethod("tabelatelhas")]
        public void tabelatelhas()
        {
            Telhas pp = new Telhas();
            pp.InserirTabela();
        }
        [CommandMethod("tabelaRM")]
        public void tabelaRM()
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
        public void tabelatecnometal()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirTabela();
        }
        [CommandMethod("tabelatecnometalauto")]
        public void tabelatecnometalauto()
        {
            TecnoMetal pp = new TecnoMetal();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            pp.InserirTabelaAuto(ref erros);

            Conexoes.Utilz.ShowReports(erros);
        }

        
        
        
        [CommandMethod("selopreenche")]
        public void selopreenche()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.PreencheSelo();
        }
        [CommandMethod("selolimpar")]
        public void selolimpar()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.PreencheSelo(true);
        }

        
        [CommandMethod("rodarmacros")]
        public void rodarmacros()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.RodarMacros();
        }


        [CommandMethod("gerardbf3d")]
        public void gerardbf3d()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.GerarDBF3D();
        }
        [CommandMethod("gerardbf")]
        public void gerardbf()
        {

            TecnoMetal mm = new TecnoMetal();
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            var tbl = mm.GerarDBF(ref erros,Conexoes.Utilz.Pergunta("Atualizar CAMs?\nAo ativar essa opção também será verificado CAM x Projeto"));

            if (File.Exists(tbl.Banco) && erros.Count==0)
            {
                Utilidades.Alerta("Arquivo gerado com sucesso!\nCAMs Atualizados!");
                Conexoes.Utilz.Abrir(Conexoes.Utilz.getPasta(tbl.Banco));
            }

            Conexoes.Utilz.ShowReports(erros);
        }




        [CommandMethod("arremate")]
        public static void arremate()
        {

            TecnoMetal pp = new TecnoMetal();
            pp.InserirArremate();
        }
        [CommandMethod("chapa")]
        public static void chapa()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirChapa();

        }

        [CommandMethod("unitario")]
        public static void unitario()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirElementoUnitario();
        }
        [CommandMethod("elem2")]
        public static void elem2()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.InserirElementoM2();
        }

        [CommandMethod("criarmarcasdeexcel")]
        public void criarmarcasdeexcel()
        {




            CriarMarcas mm = new CriarMarcas();
            mm.Show();

        }
        [CommandMethod("criarmarcasdecam")]
        public void criarmarcasdecam()
        {

            var arqs = Conexoes.Utilz.AbrirArquivos("Selecione os arquivos", new List<string> { "CAM" });
            Cotas = new Cotagem();
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
        public void bloqueiamviews()
        {
            ClasseBase b = new ClasseBase();
            b.SetViewport(true);
        }

        [CommandMethod("desbloqueiamviews")]
        public void desbloqueiamviews()
        {
            ClasseBase b = new ClasseBase();
            b.SetViewport(false);
        }



        [CommandMethod("teste")]
        public void teste()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.GetMarcas();
        }

        [CommandMethod("marcar")]
        public void marcar()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.Marcar();
        }



        [CommandMethod("mercadorias")]
        public void mercadorias()
        {
            TecnoMetal p = new TecnoMetal();
            p.Mercadorias();
        }
        [CommandMethod("materiais")]
        public void materiais()
        {
            TecnoMetal p = new TecnoMetal();
            p.Materiais();
        }
        [CommandMethod("tratamentos")]
        public void tratamentos()
        {
            TecnoMetal p = new TecnoMetal();
            p.Tratamentos();
        }


        ////[CommandMethod("3dmercadorias")]
        ////public void mercadorias3d()
        ////{
        ////    TecnoMetal p = new TecnoMetal();
        ////    p.Mercadorias3d();
        ////}



        [CommandMethod("testeinterseccao")]
        public void testeinterseccao()
        {
            Utilidades.InterSectionPoint();
        }


    }
}
