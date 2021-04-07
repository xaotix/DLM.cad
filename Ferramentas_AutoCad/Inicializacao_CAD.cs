using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using MIConvexHull;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: CommandClass(typeof(Ferramentas_DLM.Ferramentas))]

namespace Ferramentas_DLM
{
    public class Ferramentas
    {
        public static Cotagem pp = new Cotagem();
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
                    LimparCotas(acTrans, acSSet);


                    // Save the new object to the database
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                    acDoc.Editor.WriteMessage("Finalizado.");
                }
            }
        }

        public static void LimparCotas(OpenCloseTransaction acTrans, SelectionSet acSSet)
        {
            if (acTrans == null | acSSet == null)
            {
                return;
            }
            // Step through the objects in the selection set
            foreach (SelectedObject acSSObj in acSSet)
            {
                //System.Windows.Forms.MessageBox.Show(acSSObj.ToString());
                // Check to make sure a valid SelectedObject object was returned
                if (acSSObj != null)
                {
                    // Open the selected object for write
                    Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                                                        OpenMode.ForWrite) as Entity;

                    if (acEnt != null)
                    {
                        if (acEnt is AlignedDimension)
                        {
                            var s = acEnt as AlignedDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is OrdinateDimension)
                        {
                            var s = acEnt as OrdinateDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is RadialDimension)
                        {
                            var s = acEnt as RadialDimension;
                            s.Erase(true);
                        }
                        else if (acEnt is RotatedDimension)
                        {
                            var s = acEnt as RotatedDimension;
                            s.Erase(true);
                        }
                        //else if (acEnt is Leader)
                        //{
                        //    var s = acEnt as Leader;
                        //    s.Erase(true);
                        //}
                        else if (acEnt is MLeader)
                        {
                            var s = acEnt as MLeader;
                            s.Erase(true);
                        }
                        else if (acEnt is MText)
                        {
                            var s = acEnt as MText;
                            s.Erase(true);
                        }

                        else if (acEnt is Dimension)
                        {
                            var s = acEnt as Dimension;
                            s.Erase(true);
                        }
                    }
                }
            }
        }

        [CommandMethod("cotar")]
        public static void Cotar()
        {
            pp = new Cotagem();
            pp.Cotar();

        }
        [CommandMethod("cconfigurar")]
        public static void configurar()
        {

            pp.Configurar();
        }
        [CommandMethod("contornar")]
        public static void contornar()
        {


            pp.Contornar();


        }
        [CommandMethod("cco")]
        public static void cco()
        {

            pp.ConfigurarContorno();
            pp.Contornar();


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


            pp.Contornar(false);


        }
        [CommandMethod("ccv")]
        public static void ccv()
        {


            pp.ContornarConvexo();


        }
        [CommandMethod("purlin")]
        public void purlin()
        {
            CADPurlin p = new CADPurlin();

            TercasMenu mm = new TercasMenu();
            mm.propertyGrid1.SelectedObject = p;
            mm.ShowDialog();
            p.TranspassePadrao = (double)mm.transpasse_padrao.Value;
            p.OffsetApoio = (double)mm.ofsset_apoio.Value;
            p.FichaDePintura = mm.ficha_de_pintura.Text;
            p.MapeiaFurosManuais = mm.mapeia_linhas_verticais.Checked;
            p.MapeiaFurosManuaisLayer = mm.furos_manuais_layer.Text;
            p.TirantesMLStyle = mm.tirantes_mlstyle.Text;
            p.MapearTirantes = mm.mapeia_tirantes.Checked;
            if(mm.id_terca!=1763)
            {
                var pc = Conexoes.DBases.GetBancoRM().GetRME(p.id_terca);
                if(pc!=null)
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
            else if(mm.acao == "mapear")
            {
                p.MapearBlocos();
            }
            else if(mm.acao == "transpasse")
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
            else if(mm.acao == "purlin_edicao_completa")
            {
                p.EdicaoCompleta();
            }
            else if (mm.acao == "boneco")
            {
                p.GetBoneco_Purlin();
            }
        }
        [CommandMethod("teste")]
        public void teste()
        {
            System.Windows.Forms.MessageBox.Show(Utilidades.GetEstilo("10MM").Name);
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
            P.MapearBlocos();

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
            pp = new Cotagem();
            var offset = pp.Getescala() * 70;
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

                        TecnoUtilz.ReadCam cam = new TecnoUtilz.ReadCam(s);
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

    }
}
