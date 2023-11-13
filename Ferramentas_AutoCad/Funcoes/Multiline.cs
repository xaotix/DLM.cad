using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using static DLM.cad.CAD;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using DLM.vars;
using DLM.desenho;
using Conexoes;

namespace DLM.cad
{
    public static class Multiline
    {
        private static List<MlineStyle> _mlstyles { get; set; }

        public static MlineStyle GetMlStyle(ObjectId nome)
        {
            var s = GetMLStyles();
            if (_mlstyles != null)
            {
                var retorno = _mlstyles.Find(x => x.ObjectId == nome);
                return retorno;
            }
            return null;
        }
        public static MlineStyle GetMlStyle(string nome)
        {
            var s = GetMLStyles();
            if (_mlstyles != null)
            {
                var retorno = _mlstyles.Find(x => x.Name.ToUpper() == nome.ToUpper());
                return retorno;
            }
            return null;
        }
        public static List<MlineStyle> GetMlineStyles(this List<Mline> mlss)
        {
            var retorno = new List<MlineStyle>();
            Multiline.GetMLStyles();
            if (Multiline._mlstyles != null)
            {
                if (Multiline._mlstyles.Count > 0)
                {
                    var estilos = mlss.Select(x => x.Style).GroupBy(x => x).Select(x => x.First()).ToList();
                    foreach (var estilo in estilos)
                    {
                        var igual = Multiline._mlstyles.Find(x => x.ObjectId == estilo);
                        if (igual != null)
                        {
                            retorno.Add(igual);
                        }

                    }
                }

            }


            return retorno;
        }
        public static List<MlineStyle> GetMLStyles(bool update = false)
        {
            if (_mlstyles == null | update)
            {
                _mlstyles = new List<MlineStyle>();
                using (var acTrans = acCurDb.acTrans())
                {
                    DBDictionary acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.MLStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

                    foreach (var acObjId in acLyrTbl)
                    {
                        MlineStyle acLyrTblRec;
                        acLyrTblRec = acTrans.GetObject(acObjId.Value, OpenMode.ForRead) as MlineStyle;
                        _mlstyles.Add(acLyrTblRec);
                    }

                }
            }

            return _mlstyles;
        }
        public static void MudarPolyline()
        {
            var cadbase = new CADBase();
            var s = cadbase.SelecionarObjetos();
            if (s.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) { return; }

            var polylines = cadbase.Selecoes.Filter<Polyline>();

            if (polylines.Count == 0) { return; }

            var estilo = FuncoesCAD.GetArquivosMlStyles().GetEstilos().ListaSelecionar();
            if (estilo != null)
            {
                var ml = FuncoesCAD.GetArquivosMlStyles().GetEstilo(estilo);
                if (ml != null)
                {
                    var apagar = new List<Entity>();
                    foreach (var p in polylines)
                    {
                        FLayer.Set(p.Layer);
                        if (DesenharMLine(estilo, ml.Arquivo, new List<Point3d> { p.StartPoint, p.EndPoint }))
                        {
                            apagar.Add(p);
                        }
                    }

                    acDoc.Apagar(apagar);
                }


            }
        }
        public static void MudarMultiline()
        {
            CADBase pp = new CADBase();
            var s = pp.SelecionarObjetos();
            if (s.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) { return; }

            var multiline = pp.GetMultilines();

            if (multiline.Count == 0) { return; }

            var estilos = multiline.GetMlineStyles().Select(x => x.Name).OrderBy(x => x).ToList();

            if (estilos.Count == 0) { return; }
            var estilo_subst = estilos.ListaSelecionar();
            if (estilo_subst == null)
            {
                return;
            }

            var st = Multiline.GetMlStyle(estilo_subst);

            if (st == null)
            {
                return;
            }

            var estilo = FuncoesCAD.GetArquivosMlStyles().GetEstilos().ListaSelecionar(); 
            if (estilo != null)
            {
                var ml = FuncoesCAD.GetArquivosMlStyles().GetEstilo(estilo);

                if (ml != null)
                {
                    if (Conexoes.Utilz.Pergunta($"Tem certeza que deseja trocar a Multiline [{estilo_subst}] da seleção por [{estilo}]?"))
                    {
                        var mls = multiline.FindAll(x => x.Style == st.ObjectId);
                        List<Entity> apagar = new List<Entity>();
                        foreach (var p in mls)
                        {

                            var pts = Ut.GetPontos(p);
                            if (pts.Count > 1)
                            {
                                FLayer.Set(p.Layer);

                                if (DesenharMLine(estilo, ml.Arquivo, pts))
                                {
                                    apagar.Add(p);
                                }
                            }
                        }
                        acDoc.Apagar(apagar);
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
                    arquivo.Copiar(destino, false);
                    try
                    {
                        if (File.Exists(destino))
                        {
                            using (var acTrans = acCurDb.acTrans())
                            {
                                acCurDb.LoadMlineStyleFile(estilo, destino);
                                acTrans.Commit();
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
                        Conexoes.Utilz.Alerta(ex, $"Erro tentando carregar a MLStyle {estilo} \n do arquivo {arquivo}");
                        return false;
                    }
                }
                mlst = GetMLineStyles().Find(x => x.Name.ToUpper() == estilo.ToUpper());

            }


            try
            {

                if (mlst != null)
                {
                    using (var docLock = acDoc.LockDocument())
                    {
                        using (var acTrans = acDoc.acTransST())
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
                            BlockTableRecord acBlkTblRec = acTrans.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                            acBlkTblRec.AppendEntity(line);

                            acTrans.AddNewlyCreatedDBObject(line, true);

                            acTrans.Commit();

                        }
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

                Conexoes.Utilz.Alerta(ex);
                return false;
            }

            return true;


        }
        public static List<MlineStyle> GetMLineStyles()
        {
            List<MlineStyle> retorno = new List<MlineStyle>();
            using (var acTrans = acCurDb.acTrans())
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
            return retorno;
        }

        public static List<Mline> GetVerticais(this List<Mline> mls, double comp_min = 100)
        {
           var retorno = new List<Mline>();
            foreach (var s in mls)
            {
                var lista = Ut.GetPontos(s);
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(Math.Abs(new DLM.desenho.P3d(lista.Min(x => x.X), lista.Min(x => x.Y)).GetAngulo(new DLM.desenho.P3d(lista.Max(x => x.X), lista.Max(x => x.Y)))), 2);
                    var comp = Math.Abs(new P3d(lista.Max(x => x.X), lista.Max(x => x.Y)).Distancia(new P3d(lista.Min(x => x.X), lista.Min(x => x.Y))));
                    while (angulo >= 180)
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
        public static List<Mline> GetHorizontais(this List<Mline> LS, double comp_min = 100)
        {
            List<Mline> retorno = new List<Mline>();
            foreach (var s in LS)
            {
                List<Point3d> lista = Ut.GetPontos(s);
                if (lista.Count > 1)
                {
                    var angulo = Math.Round(new DLM.desenho.P3d(lista.Min(x => x.X), lista.Min(x => x.Y), 0).GetAngulo(new DLM.desenho.P3d(lista.Max(x => x.X), lista.Max(x => x.Y), 0)), 2);
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
        public static void GetOrigens(this Mline mline, out P3d p1, out P3d p2, out double largura)
        {
            var lista = Ut.GetPontos(mline).Select(x => x.P3d()).ToList();

            /*tem q ver como ele trata quando a purlin tem mais de 2 vertices*/
            var pts = new List<P3d>();
            pts.Add(mline.Bounds.Value.MinPoint.P3d());
            pts.Add(mline.Bounds.Value.MaxPoint.P3d());



            p1 = new P3d();
            p2 = new P3d();

            largura = 0;
            var isvertical = new List<Mline> { mline }.GetVerticais().Count > 0;

            if (isvertical)
            {
                largura = Math.Abs(pts.Max(x => x.X) - pts.Min(x => x.X));
            }
            else
            {
                largura = Math.Abs(pts.Max(x => x.Y) - pts.Min(x => x.Y));
            }

            if (lista.Count > 1)
            {
                var ys = new List<P3d>();
                ys.Add(mline.Bounds.Value.MaxPoint.P3d());
                ys.Add(mline.Bounds.Value.MinPoint.P3d());
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

        public static List<P3d> GetOrigens(this List<Mline> mlines)
        {
            var retorno = new List<P3d>();

            foreach(var mline in mlines)
            {
                P3d p1 = null;
                P3d p2 = null;
                double largura = 0;
                mline.GetOrigens(out p1, out p2, out largura);

                if(p1!=null)
                {
                    retorno.Add(p1);
                }
                if(p2!=null)
                {
                    retorno.Add(p2);
                }
            }
            retorno = retorno.GroupBy(x => x.ToString()).Select(x=>x.First()).ToList();

            return retorno;
        }
    }
}
