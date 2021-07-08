using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using static Ferramentas_DLM.CAD;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;

namespace Ferramentas_DLM
{
   public static class Multiline
    {
        public static void MudarPolyline()
        {
            ClasseBase pp = new ClasseBase();
            var s = pp.SelecionarObjetos();
            if (s.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) { return; }

            var polylines = pp.Getpolylinhas();

            if (polylines.Count == 0) { return; }

            var estilo = Conexoes.Utilz.SelecionaCombo(Constantes.GetArquivosMlStyles().GetEstilos(), null);
            if (estilo != null)
            {
                var ml = Constantes.GetArquivosMlStyles().GetEstilo(estilo);
                if(ml!=null)
                {
                    foreach (var p in polylines)
                    {
                        FLayer.Set(p.Layer);
                        if (DesenharMLine(estilo,ml.Arquivo, new List<Point3d> { p.StartPoint, p.EndPoint }))
                        {
                            pp.Apagar(p);
                        }
                    }
                }


            }
        }
        public static void MudarMultiline()
        {
            ClasseBase pp = new ClasseBase();
            var s = pp.SelecionarObjetos();
            if (s.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) { return; }

            var multiline = pp.Getmultilines();

            if (multiline.Count == 0) { return; }

            var estilos = pp.GetMLStyles();

            var estilo_subst = Conexoes.Utilz.SelecionaCombo(estilos, null);
            if (estilo_subst == null)
            {
                return;
            }

            var st = Utilidades.GetEstilo(estilo_subst);

            var estilo = Conexoes.Utilz.SelecionaCombo(Constantes.GetArquivosMlStyles().GetEstilos(), null);
            if (estilo != null)
            {
                var ml = Constantes.GetArquivosMlStyles().GetEstilo(estilo);

                if (ml!=null)
                {
                if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja trocar a Multiline [{estilo_subst}] da seleção por [{estilo}]?"))
                {
                    var mls = multiline.FindAll(x => x.Style == st.ObjectId);
                    foreach (var p in mls)
                    {

                        var pts = GetPontos(p);
                        if (pts.Count > 1)
                        {
                            FLayer.Set(p.Layer);

                            if (DesenharMLine(estilo,ml.Arquivo, pts))
                            {
                                pp.Apagar(p);
                            }
                        }
                    }
                }
                }
            }
        }

        public static bool DesenharMLine(string estilo, string arquivo, List<Point3d> pontos)
        {
            var mlst = GetMLineStyles().Find(x => x.Name.ToUpper() == estilo.ToUpper());
            if (mlst == null)
            {
                if (arquivo != null)
                {
                    /*tive que copiar o arquivo pra raiz do CAD, pq se não ele não aceita.*/
                    var destino = System.Environment.CurrentDirectory + $@"\{estilo}.mln";
                    Conexoes.Utilz.Copiar(arquivo, destino, false);
                    try
                    {
                        if (File.Exists(destino))
                        {
                            using (Transaction tr = acCurDb.TransactionManager.StartOpenCloseTransaction())
                            {
                                CAD.acCurDb.LoadMlineStyleFile(estilo, destino);

                                tr.Commit();
                            }
                        }
                        else
                        {
                            Conexoes.Utilz.Alerta($"Abortado\n Foi tentado copiar o arquivo {arquivo} para \n{destino} e não foi possível. \nContacte suporte.");
                            return false;
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Conexoes.Utilz.Alerta($"Erro tentando carregar a MLStyle {estilo} \n do arquivo {arquivo}\n\n{ex.Message}\n{ex.StackTrace}");
                        return false;
                    }
                }
                mlst = GetMLineStyles().Find(x => x.Name.ToUpper() == estilo.ToUpper());

            }


            try
            {

                if (mlst != null)
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        //get the mline style
                        Mline line = new Mline();
                        //get the current mline style
                        line.Style = mlst.ObjectId;
                        line.Normal = Vector3d.ZAxis;
                        foreach (var pt in pontos)
                        {
                            line.AppendSegment(pt);
                        }

                        //open modelpace
                        ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acCurDb);
                        BlockTableRecord acBlkTblRec = tr.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        acBlkTblRec.AppendEntity(line);

                        tr.AddNewlyCreatedDBObject(line, true);

                        tr.Commit();

                    }
                }
                else
                {
                    Conexoes.Utilz.Alerta($"Não foi possível carregar o arquivo de MLStyle: {estilo}");
                    return false;
                }

            }
            catch (System.Exception ex)
            {

                Conexoes.Utilz.Alerta($"Abortado.\n\nErro ao tentar inserir a MLine: \n{ex.Message}\n\n{ex.StackTrace}");
                return false;
            }

            return true;


        }
        public static List<MlineStyle> GetMLineStyles()
        {
            List<MlineStyle> retorno = new List<MlineStyle>();
            using (DocumentLock docLock = acDoc.LockDocument())
            {
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    DBDictionary lays = acTrans.GetObject(acCurDb.MLStyleDictionaryId, OpenMode.ForWrite) as DBDictionary;

                    foreach (DBDictionaryEntry item in lays)
                    {
                        MlineStyle acLyrTblRec;
                        acLyrTblRec = acTrans.GetObject(item.Value, OpenMode.ForRead) as MlineStyle;

                        if (acLyrTblRec != null)
                        {
                            retorno.Add(acLyrTblRec);
                        }
                    }
                    acTrans.Abort();
                }
            }
            return retorno;
        }

        public static List<Mline> GetVerticais(List<Mline> LS, double comp_min = 100)
        {
            List<Mline> retorno = new List<Mline>();
            foreach (var s in LS)
            {
                List<Point3d> lista = Multiline.GetPontos(s);
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(lista.Min(x => x.X), lista.Min(x => x.Y), 0), new Calculos.Ponto3D(lista.Max(x => x.X), lista.Max(x => x.Y), 0))), 2);
                    var comp = Math.Abs(Calculos.Trigonometria.Distancia(lista.Max(x => x.X), lista.Max(x => x.Y), lista.Min(x => x.X), lista.Min(x => x.Y)));
                    if (angulo >= 180)
                    {
                        angulo = angulo - 180;
                    }

                    if ((angulo >= 85 && angulo <= 95) && comp >= comp_min)
                    {
                        retorno.Add(s);
                    }
                }

            }
            return retorno;
        }
        public static List<Mline> GetHorizontais(List<Mline> LS, double comp_min = 100)
        {
            List<Mline> retorno = new List<Mline>();
            foreach (var s in LS)
            {
                List<Point3d> lista = Multiline.GetPontos(s);
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(Math.Abs(Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(lista.Min(x => x.X), lista.Min(x => x.Y), 0), new Calculos.Ponto3D(lista.Max(x => x.X), lista.Max(x => x.Y), 0))), 2);
                    var comp = lista.Max(x => x.X) - lista.Min(x => x.X);
                    if (angulo >= 180)
                    {
                        angulo = angulo - 180;
                    }

                    if ((angulo >= 175 | angulo <= 5) && comp >= comp_min)
                    {
                        retorno.Add(s);
                    }
                }

            }
            return retorno;
        }
        public static void GetOrigens(Mline s, out Point3d p1, out Point3d p2, out double largura)
        {
            List<Point3d> lista = Multiline.GetPontos(s);

            /*tem q ver como ele trata quando a purlin tem mais de 2 vertices*/
            var pts = new List<Point3d>();
            pts.Add(s.Bounds.Value.MinPoint);
            pts.Add(s.Bounds.Value.MaxPoint);


            p1 = new Point3d();
            p2 = new Point3d();

            largura = 0;
            var isvertical = GetVerticais(new List<Mline> { s }).Count > 0;

            if(isvertical)
            {
                largura = Math.Abs(pts.Max(x=>x.X) - pts.Min(x => x.X));
            }
            else
            {
                largura = Math.Abs(pts.Max(x => x.Y) - pts.Min(x => x.Y));
            }

            if (lista.Count > 1)
            {
                var ys = new List<Point3d>();
                ys.Add(s.Bounds.Value.MaxPoint);
                ys.Add(s.Bounds.Value.MinPoint);
                p1 = lista.OrderBy(x => x.X).First();
                p2 = lista.OrderBy(x => x.X).Last();

                if (lista.Select(x => Math.Round(x.X)).Distinct().ToList().Count == 1)
                {
                    //maior Y está mais pra cima
                    p1 = lista.OrderBy(x => x.Y).Last();
                    p2 = lista.OrderBy(x => x.Y).First();
                }
            }
            else if (lista.Count == 1)
            {
                p1 = lista[0];
                p2 = lista[0];
            }
        }
        public static List<Point3d> GetPontos(Mline ml)
        {
            List<Point3d> retorno = new List<Point3d>();

            for (int i = 0; i < ml.NumberOfVertices; i++)
            {
                var t = ml.VertexAt(i);
                retorno.Add(t);
            }

            return retorno;
        }
    }
}
