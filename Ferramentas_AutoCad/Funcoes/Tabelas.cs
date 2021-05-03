using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.CAD;

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
                double escala = acDoc.Database.Dimscale;
               

                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE TERÇAS");
                    Blocos.Inserir(acDoc, Constantes.Tabela_Tercas_Titulo, p0, escala, 0, ht);
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
                        Blocos.Inserir(acDoc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
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
                double escala = acDoc.Database.Dimscale;

                if(mover_direita!=0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable ht = new Hashtable();
                ht.Add("TITULO", "LISTA DE TIRANTES");
                Blocos.Inserir(acDoc, Constantes.Tabela_Tirantes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("ORDEM", p.Sequencia.ToString().PadLeft(2, '0'));
                    hp.Add("PEÇA", p.Marca);
                    hp.Add("QUANT.", p.Qtd.ToString().PadLeft(3, '0'));
                    hp.Add("COMP", p.Comprimento.ToString().PadLeft(5, '0'));
                    Blocos.Inserir(acDoc, Constantes.Tabela_Tirantes, p0, escala, 0, hp);
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
                double escala = acDoc.Database.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new Point3d(p0.X + (mover_direita * escala), p0.Y, p0.Z);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable ht = new Hashtable();
                ht.Add("TITULO", "LISTA DE TIRANTES");
                Blocos.Inserir(acDoc, Constantes.Tabela_Correntes_Titulo, p0, escala, 0, ht);
                p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add("Nº", p.Sequencia);
                    hp.Add("PERFIL", p.Marca);
                    hp.Add("XXX", p.Qtd.ToString().PadLeft(3,'0'));
                    hp.Add("VÃO", p.Vao.ToString());
                    Blocos.Inserir(acDoc, Constantes.Tabela_Correntes, p0, escala, 0, hp);
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
                double escala = acDoc.Database.Dimscale;
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
                    Blocos.Inserir(acDoc, Constantes.Tabela_Tercas_Titulo, p0, escala, 0, ht);
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
                        Blocos.Inserir(acDoc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
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
                double escala = acDoc.Database.Dimscale;
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
                    Blocos.Inserir(acDoc, Constantes.Tabela_Almox_Titulo, p0, escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (escala * 12.86), p0.Z);
                    int seq = 1;
                    foreach (var p in RMAS)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add("QTD", p.Quantidade);
                        hp.Add("DESC", p.DESC);
                        hp.Add("UNID", p.UNIDADE);
                        hp.Add("SAP", p.SAP);

                        Blocos.Inserir(acDoc, Constantes.Tabela_Almox, p0, escala, 0, hp);
                        p0 = new Point3d(p0.X, p0.Y - (escala * 6.43), p0.Z);
                        seq++;
                    }
                }
            }
            return new Point3d(x0, y0, 0);

        }
        public static Point3d TecnoMetal(List<DB.Linha> pecas_tecnometal, Point3d p0, double mover_direita = 0, double escala = 1)
        {
            double x0 = 0;
            double y0 = 0;
            if (pecas_tecnometal.Count > 0)
            {
                //preguiça de ajustar os blocos da tabela, mantive um fator de escala
                double fator_escala = 1.25 * escala;
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

                    var pecas = pecas_tecnometal.GroupBy(x => x.Get(Constantes.ATT_MAR).valor).Select(X => X.ToList());
                    double total_superficie = 0;
                    double total_peso = 0;
                    foreach (var pc in pecas)
                    {
                        var marca = pc.FindAll(x => x.Get(Constantes.ATT_POS).valor == "");
                        var posics = pc.FindAll(x => x.Get(Constantes.ATT_POS).valor != "");
                        int qtd = marca[0].Get(Constantes.ATT_QTD).Int;
                        double peso_unit = posics.Sum(x => x.Get(Constantes.ATT_PES).Double() * x.Get(Constantes.ATT_QTD).Int);
                        double sup_unit = posics.Sum(x => x.Get(Constantes.ATT_SUP).Double() * x.Get(Constantes.ATT_QTD).Int);
                        marca[0].Set(Constantes.ATT_PES, peso_unit);
                        marca[0].Set(Constantes.ATT_SUP, sup_unit);
                        total_superficie += (sup_unit * qtd);
                        total_peso += (peso_unit * qtd);
                    }

                    total_peso = Math.Round(total_peso/1000, decimais);
                    total_superficie = Math.Round(total_superficie, decimais);

                    ht.Add("PESO_TOTAL", total_peso.ToString(dec_str).Replace(",", "") + " ton");
                    ht.Add("SUPERFICIE_TOTAL",total_superficie.ToString("N1").Replace(",", "") + " m²");
                    Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Titulo, p0, fator_escala, 0, ht);
                    p0 = new Point3d(p0.X, p0.Y - (fator_escala * 20.4), p0.Z);
                    int seq = 1;
                    foreach(var pc in pecas)
                    {


                        foreach (var pos in pc)
                        {
                            string descricao = pos.Get(Constantes.ATT_MER).valor;
                            if(pos.Get(Constantes.ATT_POS).valor != "")
                            {
                                descricao = pos.Get(Constantes.ATT_PER).valor;
                                if (descricao == "")
                                {
                                    descricao =
                                        "Ch. " + pos.Get(Constantes.ATT_ESP).Double().ToString("N2").Replace(",", "") +
                                        " x " + pos.Get(Constantes.ATT_LRG).Double().ToString("N1").Replace(",", "") +
                                        " x " + pos.Get(Constantes.ATT_CMP).Double().ToString("N1").Replace(",", "");
                                }
                            }

                            var tipo = pos.Get(Constantes.ATT_REC).ToString();
                            
                            Hashtable hp = new Hashtable();
                            hp.Add("MARCA", tipo == Constantes.ATT_REC_MARCA?pos.Get(Constantes.ATT_MAR): pos.Get(Constantes.ATT_POS));
                            hp.Add("QTD", pos.Get(Constantes.ATT_QTD));
                            hp.Add("DESC", descricao );
                            hp.Add("MATERIAL", pos.Get(Constantes.ATT_MAT));
                            hp.Add("SAP", pos.Get(Constantes.ATT_SAP));
                            hp.Add("PESO_UNIT", Math.Round(pos.Get(Constantes.ATT_PES).Double() /1000,decimais).ToString(dec_str).Replace(",",""));
                            hp.Add("PESO_TOT", Math.Round(pos.Get(Constantes.ATT_PES).Double() /1000 * pos.Get(Constantes.ATT_QTD).Int, decimais).ToString(dec_str).Replace(",", ""));
                            hp.Add("FICHA", pos.Get(Constantes.ATT_FIC));

                            Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal, p0, fator_escala, 0, hp);
                            p0 = new Point3d(p0.X, p0.Y - (fator_escala * 4.25), p0.Z);
                            seq++;
                        }
                        Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Vazia, p0, fator_escala, 0, new Hashtable());
                        p0 = new Point3d(p0.X, p0.Y - (fator_escala * 4.25), p0.Z);
                    }
                   
                }
            }
            return new Point3d(x0, y0, 0);

        }
    }
}
