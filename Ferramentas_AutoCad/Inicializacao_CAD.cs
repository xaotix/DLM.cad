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
        [CommandMethod("setarLTS")]
        public void setarLTS()
        {

            ClasseBase b = new ClasseBase();
            b.SetLts();
        }


        [CommandMethod("gerardbf")]
        public void gerardbf()
        {
              
            TecnoMetal mm = new TecnoMetal();
          var tbl = mm.GerarDBF();

           if(File.Exists(tbl.Banco))
            {
                Utilidades.Alerta("Arquivo gerado com sucesso!");
                Conexoes.Utilz.Abrir(Conexoes.Utilz.getPasta(tbl.Banco));
            }
        }
        [CommandMethod("abrepasta")]
        public void abrepasta()
        {
            ClasseBase pp = new ClasseBase();
            pp.AbrePasta();
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
            pp.InserirTabelaAuto();
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




        //ABANDONEI, SEM SUCCESSO TENTANDO PEGAR AS MARCAS NO 3D.

        //[CommandMethod("testelisp")]
        //public void testelisp()
        //{
        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Editor ed = doc.Editor;

       
        //    // create a result buffer containing a LISP list
        //    ResultBuffer input = new ResultBuffer(
        //        new TypedValue(System.Convert.ToInt32(LispDataType.ListBegin)), 
        //        new TypedValue(System.Convert.ToInt32(LispDataType.Int16), 12), 
        //        new TypedValue(System.Convert.ToInt32(LispDataType.Text), "toto"), 
        //        new TypedValue(System.Convert.ToInt32(LispDataType.T_atom)), 
        //        new TypedValue(System.Convert.ToInt32(LispDataType.ListEnd))
        //        );

        //    // bind the list to a 'lst1' LISP variable
        //    LispExtensions.SetLispSym("lst1", input);

        //    // call the 'foo' Lisp function which binds the reversed list to 'lst2'
        //    // (defun foo () (setq lst2 (reverse lst1))) (vl-acad-defun 'foo)
        //    LispExtensions.InvokeLisp(new ResultBuffer(new TypedValue(System.Convert.ToInt32(LispDataType.Text), "foo")));

        //    // get the 'lst2' variable value
        //    ResultBuffer output = LispExtensions.GetLispSym("vl-acad-defun");

        //    // print the value to the commande line
        //    foreach (TypedValue tv in output)
        //        ed.WriteMessage("Type: {0} Value: {1}", tv.TypeCode, tv.Value);
        //}

        //[CommandMethod("testemarcas")]
        //public void testemarcas()
        //{

        //    //estava tentando sem sucesso pegar as informações sobre os elementos do tecnometal


        //    Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        //    Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //    Database acCurDb = acDoc.Database;

        //    var selecao = ed.GetEntity("\nSelect object: ");
        //    if (selecao.Status != PromptStatus.OK)
        //        return;



        //    using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
        //    {
        //        Entity acEnt = acTrans.GetObject(selecao.ObjectId, OpenMode.ForRead) as Entity;





               
        //    }

        //    var ss = SelectionSet.FromObjectIds(new ObjectId[] { selecao.ObjectId });
        //    //////// //var st = ed.Command( "tec_stsetvar3d", ss, "PROFILEDATA", "MAR_PEZ", "asdadads");


        //    //ResultBuffer input = new ResultBuffer(
        //    //    new TypedValue((int)LispDataType.ListBegin),
        //    //    new TypedValue((int)LispDataType.Text, "C:tec_stGetvar3d"),
        //    //    new TypedValue((int)LispDataType.ObjectId, selecao.ObjectId),
        //    //    new TypedValue((int)LispDataType.Text, "profiledata"),
        //    //    new TypedValue((int)LispDataType.Text, "mar_pez"),
        //    //    new TypedValue((int)LispDataType.Text, ""),
        //    //    new TypedValue((int)LispDataType.ListEnd)
        //    //    );

        //    //LispExtensions.SetLispSym("lst1", input);


        //    //////// //' call the 'foo' Lisp function which binds the reversed list to 'lst2'
        //    //////// //' (defun foo () (setq lst2 (reverse lst1))) (vl-acad-defun 'foo)
        //    LispExtensions.InvokeLisp(new ResultBuffer(new TypedValue((int)(LispDataType.Text), "lst1")));
        //    ////////var et = LispExtensions.GetLispSym("lst2");


        //    var st2 = ed.Command("c:TM4D_MARKOFF", "_ALL", "");
        //    var st3 = ed.Command("c:TM4D_MARKON", ss, "");

        //    ResultBuffer output = LispExtensions.GetLispSym("MAR_PEZ");

        //    //TEC_STGETVAR3D
        //    //var s = ed.Command("tec_stsetvar3d", selecao.ObjectId, "profiledata", "mar_pez", "asdadads");
        //    //Tec_StResetIndexRecords

        //}

        //[LispFunction("DisplayFullName")]
        //public static void DisplayFullName(ResultBuffer rbArgs)
        //{
        //    if (rbArgs != null)
        //    {
        //        string strVal1 = "";
        //        string strVal2 = "";

        //        int nCnt = 0;
        //        foreach (TypedValue rb in rbArgs)
        //        {
        //            if (rb.TypeCode == (int)Autodesk.AutoCAD.Runtime.LispDataType.Text)
        //            {
        //                switch (nCnt)
        //                {
        //                    case 0:
        //                        strVal1 = rb.Value.ToString();
        //                        break;
        //                    case 1:
        //                        strVal2 = rb.Value.ToString();
        //                        break;
        //                }

        //                nCnt = nCnt + 1;
        //            }
        //        }

        //        Application.DocumentManager.MdiActiveDocument.Editor.
        //           WriteMessage("\nName: " + strVal1 + " " + strVal2);
        //    }
        //}
    }
}
