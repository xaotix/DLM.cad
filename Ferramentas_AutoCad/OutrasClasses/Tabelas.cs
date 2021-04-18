using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class Tabelas
    {
        public static void DBRM(Conexoes.DBRM_Offline dbase)
        {
            bool cancelado = false;
            Point3d p0 = Utilidades.PedirPonto3D("Selecione a origem para a tabela de peças.", out cancelado);

            if(cancelado)
            {
                return;
            }

            var purlins = dbase.RM_Macros.FindAll(x => x.Objeto is Conexoes.Macros.Purlin).Select(X => X.Objeto as Conexoes.Macros.Purlin).ToList();
            var tirantes = dbase.RM_Macros.FindAll(x => x.Objeto is Conexoes.Macros.Tirante).Select(X => X.Objeto as Conexoes.Macros.Tirante).ToList();

            Purlins(purlins,p0);
            double larg = 119.81;
           if (tirantes.Count>0)
            {
                Tirantes(tirantes, p0, larg);
                larg = larg * 2;
            }
          
           if(dbase.RME.Count>0)
            {
            RMES(dbase.RME,p0,larg);
                larg = larg + 119.81;
            }
           if(dbase.RMU.Count>0)
            {
            RMES(dbase.RMU.Select(x=>x as Conexoes.RME).ToList(), p0, larg);
                larg = larg + 119.81;
            }

            if (dbase.RMA.Count > 0)
            {
                RMAS(dbase.RMA.Select(x => x as Conexoes.RMA).ToList(), p0, larg);
            }

        }
        public static Point3d Purlins(List<Conexoes.Macros.Purlin> purlins, Point3d p0)
        {
            double x0 = 0;
            double y0 = 0;
            if (purlins.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = doc.Database.Dimscale;
               

                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE TERÇAS");
                    Utilidades.InserirBloco(doc, Constantes.Tabela_Tercas_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                    foreach (var p in purlins)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add("Nº", p.Sequencia.ToString().PadLeft(2, '0'));
                        hp.Add("PERFIL", p.Nome);
                        hp.Add("XX", p.Quantidade.ToString().PadLeft(3,'0'));
                        hp.Add("COMP", p.Comprimento.ToString().PadLeft(5,'0'));
                        hp.Add("ESP", p.Espessura.ToString("N2").PadLeft(5, '0'));
                        hp.Add("VP;RM;TM", "RME");
                        Utilidades.InserirBloco(doc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
                        p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                    }
                
            }
            return new Point3d(x0, y0, 0);

        }
        public static Point3d Tirantes(List<Conexoes.Macros.Tirante> trs, Point3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (trs.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = doc.Database.Dimscale;

                if(mover_direita!=0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable ht = new Hashtable();
                ht.Add("TITULO", "LISTA DE TIRANTES");
                Utilidades.InserirBloco(doc, Constantes.Tabela_Tirantes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("ORDEM", p.Sequencia.ToString().PadLeft(2, '0'));
                    hp.Add("PEÇA", p.Marca);
                    hp.Add("QUANT.", p.Qtd.ToString().PadLeft(3, '0'));
                    hp.Add("COMP", p.Comprimento.ToString().PadLeft(5, '0'));
                    Utilidades.InserirBloco(doc, Constantes.Tabela_Tirantes, p0, escala, 0, hp);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                }
            }
            return new Point3d(x0, y0, 0);

        }
        public static Point3d Correntes(List<Conexoes.Macros.Corrente> trs, Point3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (trs.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = doc.Database.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable ht = new Hashtable();
                ht.Add("TITULO", "LISTA DE TIRANTES");
                Utilidades.InserirBloco(doc, Constantes.Tabela_Correntes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("Nº", p.Sequencia);
                    hp.Add("PERFIL", p.Marca);
                    hp.Add("XXX", p.Qtd.ToString().PadLeft(3,'0'));
                    hp.Add("VÃO", p.Vao.ToString());
                    Utilidades.InserirBloco(doc, Constantes.Tabela_Correntes, p0, escala, 0, hp);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                }
            }
            return new Point3d(x0, y0, 0);

        }
        public static Point3d RMES(List<Conexoes.RME> RMES, Point3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMES.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = doc.Database.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE PEÇAS");
                    Utilidades.InserirBloco(doc, Constantes.Tabela_Tercas_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                    int seq = 1;
                    foreach (var p in RMES)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add("Nº", seq.ToString().PadLeft(2, '0'));
                        hp.Add("PERFIL", p.CODIGOFIM);
                        hp.Add("XX", p.Quantidade.ToString().PadLeft(3, '0'));
                        hp.Add("COMP", p.COMP.ToString().PadLeft(5, '0'));
                        hp.Add("ESP", p.ESP.ToString("N2").PadLeft(5, '0'));
                        hp.Add("VP;RM;TM", "RM");
                        Utilidades.InserirBloco(doc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
                        p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                        seq++;
                    }
                }
            }
            return new Point3d(x0, y0, 0);

        }
        public static Point3d RMAS(List<Conexoes.RMA> RMAS, Point3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMAS.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = doc.Database.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE PEÇAS");
                    Utilidades.InserirBloco(doc, Constantes.Tabela_Almox_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                    int seq = 1;
                    foreach (var p in RMAS)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add("QTD", p.Quantidade);
                        hp.Add("DESC", p.DESC);
                        hp.Add("UNID", p.UNIDADE);
                        hp.Add("SAP", p.SAP);

                        Utilidades.InserirBloco(doc, Constantes.Tabela_Almox, p0, escala, 0, hp);
                        p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                        seq++;
                    }
                }
            }
            return new Point3d(x0, y0, 0);

        }



        public static Point3d TecnoMetal(List<DB.Linha> pecas_tecnometal, Point3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (pecas_tecnometal.Count > 0)
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                double escala = 1.25;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new Point3d(p0.X + (mover_direita), p0.Y, p0.Z);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    int decimais = 4;
                    string dec_str = "N4";

                    var pecas = pecas_tecnometal.GroupBy(x => x.Get("MAR_PEZ").valor).Select(X => X.ToList());
                    double total_superficie = 0;
                    double total_peso = 0;
                    foreach (var pc in pecas)
                    {
                        var marca = pc.FindAll(x => x.Get("POS_PEZ").valor == "");
                        var posics = pc.FindAll(x => x.Get("POS_PEZ").valor != "");
                        int qtd = marca[0].Get("QTA_PEZ").Int;
                        double peso_unit = posics.Sum(x => x.Get("PUN_LIS").Double() * x.Get("QTA_PEZ").Int);
                        double sup_unit = posics.Sum(x => x.Get("SUN_LIS").Double() * x.Get("QTA_PEZ").Int);
                        marca[0].Set("PUN_LIS", peso_unit);
                        marca[0].Set("SUN_LIS", sup_unit);
                        total_superficie += (sup_unit * qtd);
                        total_peso += (peso_unit * qtd);
                    }

                    total_peso = Math.Round(total_peso/1000, decimais);
                    total_superficie = Math.Round(total_superficie, decimais);

                    ht.Add("PESO_TOTAL", total_peso.ToString(dec_str) + " ton");
                    ht.Add("SUPERFICIE_TOTAL",total_superficie.ToString("N1") + " m²");
                    Utilidades.InserirBloco(doc, Constantes.Tabela_TecnoMetal_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 20.4), p0.Z);
                    int seq = 1;
                    foreach(var pc in pecas)
                    {


                        foreach (var pos in pc)
                        {
                            string descricao = pos.Get("DES_PEZ").valor;
                            if(pos.Get("POS_PEZ").valor != "")
                            {
                                descricao = pos.Get("NOM_PRO").valor;
                                if (descricao == "")
                                {
                                    descricao =
                                        "Ch. " + pos.Get("SPE_PRO").Double().ToString("N2") +
                                        " x " + pos.Get("LAR_PRO").Double().ToString("N1") +
                                        " x " + pos.Get("LUN_PRO").Double().ToString("N1");
                                }
                            }

                            
                            Hashtable hp = new Hashtable();
                            hp.Add("MARCA", pos.Get("MAR_PEZ"));
                            hp.Add("QTD", pos.Get("QTA_PEZ"));
                            hp.Add("DESC", descricao );
                            hp.Add("MATERIAL", pos.Get("MAT_PRO"));
                            hp.Add("SAP", pos.Get("COD_PEZ"));
                            hp.Add("PESO_UNIT", Math.Round(pos.Get("PUN_LIS").Double() /1000,decimais).ToString(dec_str));
                            hp.Add("PESO_TOT", Math.Round(pos.Get("PUN_LIS").Double() /1000 * pos.Get("QTA_PEZ").Int, decimais).ToString(dec_str));
                            hp.Add("FICHA", pos.Get("TRA_PEZ"));

                            Utilidades.InserirBloco(doc, Constantes.Tabela_TecnoMetal, p0, escala, 0, hp);
                            p0 = new Point3d(p0.X, p0.Y - (escala * 4.25), p0.Z);
                            seq++;
                        }
                        Utilidades.InserirBloco(doc, Constantes.Tabela_TecnoMetal_Vazia, p0, escala, 0, new Hashtable());
                        p0 = new Point3d(p0.X, p0.Y - (escala * 4.25), p0.Z);
                    }
                   
                }
            }
            return new Point3d(x0, y0, 0);

        }
    }
}
