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
                    Utilidades.InserirBloco(doc, Constantes.Tercas_Titulo, p0, escala, 0, ht);
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
                        Utilidades.InserirBloco(doc, Constantes.Indicacao_Tercas, p0, escala, 0, hp);
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
                Utilidades.InserirBloco(doc, Constantes.Tirantes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("ORDEM", p.Sequencia.ToString().PadLeft(2, '0'));
                    hp.Add("PEÇA", p.Marca);
                    hp.Add("QUANT.", p.Qtd.ToString().PadLeft(3, '0'));
                    hp.Add("COMP", p.Comprimento.ToString().PadLeft(5, '0'));
                    Utilidades.InserirBloco(doc, Constantes.Tirantes, p0, escala, 0, hp);
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
                Utilidades.InserirBloco(doc, Constantes.Correntes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("Nº", p.Sequencia);
                    hp.Add("PERFIL", p.Marca);
                    hp.Add("XXX", p.Qtd.ToString().PadLeft(3,'0'));
                    hp.Add("VÃO", p.Vao.ToString());
                    Utilidades.InserirBloco(doc, Constantes.Correntes, p0, escala, 0, hp);
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
                    Utilidades.InserirBloco(doc, Constantes.Tercas_Titulo, p0, escala, 0, ht);
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
                        Utilidades.InserirBloco(doc, Constantes.Indicacao_Tercas, p0, escala, 0, hp);
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
                    Utilidades.InserirBloco(doc, Constantes.Almox_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                    int seq = 1;
                    foreach (var p in RMAS)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add("QTD", p.Quantidade);
                        hp.Add("DESC", p.DESC);
                        hp.Add("UNID", p.UNIDADE);
                        hp.Add("SAP", p.SAP);

                        Utilidades.InserirBloco(doc, Constantes.Almox, p0, escala, 0, hp);
                        p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                        seq++;
                    }
                }
            }
            return new Point3d(x0, y0, 0);

        }
    }
}
