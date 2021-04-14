using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using MIConvexHull;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: CommandClass(typeof(Ferramentas_DLM.Ferramentas))]

namespace Ferramentas_DLM
{
    public class Ferramentas
    {
        public static Monitoramento monitoramento { get; set; }
        public static Cotagem Cotas { get; set; } = new Cotagem();

        [CommandMethod("listarcomandos")]
        public static void listarcomandos()
        {
            DocumentCollection dm = Application.DocumentManager;
            Editor ed = dm.MdiActiveDocument.Editor;
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] cmds = Utilidades.listarcomandos(asm, false);
            foreach (string cmd in cmds)
            {
                ed.WriteMessage(cmd + "\n");
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

        [CommandMethod("arremates")]
        public static void arremates()
        {

            Arremate pp = new Arremate();
            pp.Mapear();


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
        [CommandMethod("interseccao")]
        public void interseccao()
        {
            Utilidades.InterSectionPoint();
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
        [CommandMethod("criarmarcasexcel")]
        public void criarmarcasexcel()
        {




            CriarMarcas mm = new CriarMarcas();
            mm.Show();

        }
        [CommandMethod("criarmarcas")]
        public void criarmarcas()
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
                        Utilidades.InserirMarcaSimplesCam(cam, p0);

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
        [CommandMethod("importarRM")]
        public void importarRM()
        {
            string arquivo = Conexoes.Utilz.Abrir_String("RM", "Selecione");
            if (File.Exists(arquivo))
            {
                Conexoes.DBRM_Offline pp = new Conexoes.DBRM_Offline();

                var s = pp.Ler(arquivo);


                Tabelas.DBRM(s);
            }

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
        [CommandMethod("tabelatelhas")]
        public void tabelatelhas()
        {
            Telhas pp = new Telhas();
            pp.InserirTabela();
        }

        [CommandMethod("testectv")]
        public void testectv()
        {
            TecnoMetal pp = new TecnoMetal();
            pp.GetInfos();
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
        [CommandMethod("teste")]
        public void teste()
        {
         
            //estava tentando sem sucesso pegar as informações sobre os elementos do tecnometal


            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            var selecao = ed.GetEntity("\nSelect object: ");
            if (selecao.Status != PromptStatus.OK)
                return;



            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                Entity acEnt = acTrans.GetObject(selecao.ObjectId, OpenMode.ForRead) as Entity;





                //ResultBuffer args = new ResultBuffer(
                //    new TypedValue((int)LispDataType.ListBegin),
                //    new TypedValue((int)LispDataType.ObjectId, Autodesk.AutoCAD.Internal.Utils.EntLast()),
                //    new TypedValue((int)LispDataType.Text, "profiledata"),
                //    new TypedValue((int)LispDataType.Text, "mar_pez"),
                //    new TypedValue((int)LispDataType.Text, "sssdaas"),
                //    new TypedValue((int)LispDataType.ListEnd)
                //    );

                //LispExtensions.SetLispSym("tec_stsetvar3d", args);
            }
            var st = ed.Command("TEC_STGETVAR3D", selecao.ObjectId, "profiledata", "mar_pez", "asdadads");





            //TEC_STGETVAR3D
            //var s = ed.Command("tec_stsetvar3d", selecao.ObjectId, "profiledata", "mar_pez", "asdadads");


        }

        [CommandMethod("extrair")]
        public void extrair()
        {
            string destino = Conexoes.Utilz.SalvarArquivo("dbf");
            if(destino=="" | destino ==null)
            {
                return;
            }
              
            TecnoMetal mm = new TecnoMetal();
           mm.GerarDBF(destino);

            Conexoes.Utilz.Abrir(Conexoes.Utilz.getPasta(destino));
        }

    }
}
