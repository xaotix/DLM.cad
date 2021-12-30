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
            Point2d p0 = Ut.PedirPonto2D("Selecione a origem para a tabela de peças.", out cancelado);

            if(cancelado)
            {
                return;
            }

            var purlins = dbase.RM_Macros.FindAll(x => x.GetObjeto() is Conexoes.Macros.Purlin).Select(X => X.GetObjeto() as Conexoes.Macros.Purlin).ToList();
            var tirantes = dbase.RM_Macros.FindAll(x => x.GetObjeto() is Conexoes.Macros.Tirante).Select(X => X.GetObjeto() as Conexoes.Macros.Tirante).ToList();

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
        public static Point2d Purlins(List<Conexoes.Macros.Purlin> purlins, Point2d p0)
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
                    p0 = new Point2d(p0.X, p0.Y - (escala * 12.86));
                    foreach (var p in purlins)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add(Constantes.ATT_N, p.Sequencia.ToString().PadLeft(2, '0'));
                        hp.Add(Constantes.ATT_Perfil, p.Nome);
                        hp.Add(Constantes.ATT_Quantidade, p.Quantidade.ToString().PadLeft(3,'0'));
                        hp.Add(Constantes.ATT_Comprimento, p.Comprimento.ToString().PadLeft(5,'0'));
                        hp.Add(Constantes.ATT_Espessura, p.Espessura.ToString("N2").PadLeft(5, '0'));
                        hp.Add(Constantes.ATT_Destino, Constantes.ATT_RME);
                        Blocos.Inserir(acDoc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
                        p0 = new Point2d(p0.X, p0.Y - (escala * 6.43));
                    }
                
            }
            return new Point2d(x0, y0);

        }
        public static Point2d Tirantes(List<Conexoes.Macros.Tirante> trs, Point2d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (trs.Count > 0)
            {
                double escala = acDoc.Database.Dimscale;

                if(mover_direita!=0)
                {
                    p0 = new Point2d(p0.X + (mover_direita * escala), p0.Y);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable htt = new Hashtable();
                htt.Add("TITULO", "LISTA DE TIRANTES");
                Blocos.Inserir(acDoc, Constantes.Tabela_Tirantes_Titulo, p0, escala, 0, htt);
                p0 = new Point2d(p0.X, p0.Y - (escala * 12.86));
                foreach (var p in trs)
                {
                    Hashtable ht = new Hashtable();
                    ht.Add("ORDEM", p.Sequencia.ToString().PadLeft(2, '0'));
                    ht.Add(Constantes.ATT_Peca, p.Marca);
                    ht.Add(Constantes.ATT_Quantidade, p.Qtd.ToString().PadLeft(3, '0'));
                    ht.Add(Constantes.ATT_Comprimento, p.Comprimento.ToString().PadLeft(5, '0'));
                    Blocos.Inserir(acDoc, Constantes.Tabela_Tirantes, p0, escala, 0, ht);
                    p0 = new Point2d(p0.X, p0.Y - (escala * 6.43));
                }
            }
            return new Point2d(x0, y0);

        }
        public static Point2d Correntes(List<Conexoes.Macros.Corrente> trs, Point2d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (trs.Count > 0)
            {
                double escala = acDoc.Database.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new Point2d(p0.X + (mover_direita * escala), p0.Y);
                }

                x0 = p0.X;
                y0 = p0.Y;
                Hashtable ht = new Hashtable();
                ht.Add("TITULO", "LISTA DE TIRANTES");
                Blocos.Inserir(acDoc, Constantes.Tabela_Correntes_Titulo, p0, escala, 0, ht);
                p0 = new Point2d(p0.X, p0.Y - (escala * 12.86));
                foreach (var p in trs)
                {
                    Hashtable hp = new Hashtable();
                    hp.Add(Constantes.ATT_N, p.Sequencia);
                    hp.Add(Constantes.ATT_Perfil, p.Marca);
                    hp.Add(Constantes.ATT_Quantidade, p.Qtd.ToString().PadLeft(3,'0'));
                    hp.Add(Constantes.ATT_Vao, p.Vao.ToString());
                    Blocos.Inserir(acDoc, Constantes.Tabela_Correntes, p0, escala, 0, hp);
                    p0 = new Point2d(p0.X, p0.Y - (escala * 6.43));
                }
            }
            return new Point2d(x0, y0);

        }
        public static Point2d RMES(List<Conexoes.RME> RMES, Point2d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMES.Count > 0)
            {
                double escala = acDoc.Database.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new Point2d(p0.X + (mover_direita * escala), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE PEÇAS");
                    Blocos.Inserir(acDoc, Constantes.Tabela_Tercas_Titulo, p0, escala, 0, ht);
                    p0 = new Point2d(p0.X, p0.Y - (escala * 12.86));
                    int seq = 1;
                    foreach (var p in RMES)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add(Constantes.ATT_N, seq.ToString().PadLeft(2, '0'));
                        hp.Add(Constantes.ATT_Perfil, p.CODIGOFIM);
                        hp.Add(Constantes.ATT_Quantidade, p.Quantidade.ToString().PadLeft(3, '0'));
                        hp.Add(Constantes.ATT_Comprimento, p.COMP.ToString().PadLeft(5, '0'));
                        hp.Add(Constantes.ATT_Espessura, p.ESP.ToString("N2").PadLeft(5, '0'));
                        hp.Add(Constantes.ATT_Destino, "RM");
                        Blocos.Inserir(acDoc, Constantes.Tabela_Tercas, p0, escala, 0, hp);
                        p0 = new Point2d(p0.X, p0.Y - (escala * 6.43));
                        seq++;
                    }
                }
            }
            return new Point2d(x0, y0);

        }
        public static Point2d RMAS(List<Conexoes.RMA> RMAS, Point2d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMAS.Count > 0)
            {
                double escala = acDoc.Database.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new Point2d(p0.X + (mover_direita * escala), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA DE PEÇAS");
                    Blocos.Inserir(acDoc, Constantes.Tabela_Almox_Titulo, p0, escala, 0, ht);
                    p0 = new Point2d(p0.X, p0.Y - (escala * 12.86));
                    int seq = 1;
                    foreach (var p in RMAS)
                    {
                        Hashtable htt = new Hashtable();
                        htt.Add(Constantes.ATT_Quantidade, p.Quantidade);
                        htt.Add(Constantes.ATT_Descricao, p.DESC);
                        htt.Add("UNID", p.UNIDADE);
                        htt.Add(Constantes.ATT_Cod_SAP, p.SAP);

                        Blocos.Inserir(acDoc, Constantes.Tabela_Almox, p0, escala, 0, htt);
                        p0 = new Point2d(p0.X, p0.Y - (escala * 6.43));
                        seq++;
                    }
                }
            }
            return new Point2d(x0, y0);

        }


        public static Point2d Pecas(List<PCQuantificar> pcs, bool separar, Point2d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;

            double x_tabela = 125;


            if (pcs.Count > 0)
            {
                double escala = acDoc.Database.Dimscale;

                Point2d p1 = new Point2d(p0.X, p0.Y);
                if (mover_direita != 0)
                {
                    p1 = new Point2d(p1.X + (mover_direita * escala), p1.Y);
                }

                Point2d p0a = new Point2d(p1.X, p1.Y);

                List<List<PCQuantificar>> pacotes = new List<List<PCQuantificar>>();
                if(separar)
                {
                    pacotes = pcs.GroupBy(x => x.Familia).Select(X => X.ToList()).ToList();
                }
                else
                {
                    pacotes = new List<List<PCQuantificar>> { pcs };
                }

                foreach(var pacote in pacotes)
                {
                    x0 = p1.X;
                    y0 = p1.Y;
                    Hashtable ht = new Hashtable();
                    ht.Add("TITULO", "LISTA " + pacote[0].Familia.ToUpper());
                    Blocos.Inserir(acDoc, Constantes.Tabela_Pecas_Titulo, p1, escala, 0, ht);
                    p1 = new Point2d(p1.X, p1.Y - (escala * 12.86));
                    int seq = 1;
                    var linhas = pacote.OrderBy(x => x.Numero + "|" + x.Nome).ToList();
                    foreach (var p in linhas)
                    {
                        Hashtable hp = new Hashtable();
                        hp.Add(Constantes.ATT_Marca, p.Nome);
                        if (p.Nome_Bloco.StartsWith(Constantes.PC_Quantificar) && !p.Nome_Bloco.Contains(Constantes.ATT_Texto))
                        {
                            Point2d pcentro = new Point2d(p1.X + (escala * 6.9894), p1.Y + (escala * -3.2152));
                            Hashtable bl = new Hashtable();
                            //foreach (var obj in p.Atributos.Celulas)
                            //{
                            //    bl.Add(obj.Coluna, obj.Valor);
                            //}
                           
                            bl.Add(Constantes.ATT_N, p.Atributos.Get(Constantes.ATT_N).Valor);
                            bl.Add(Constantes.ATT_Familia, p.Familia);
                            bl.Add(Constantes.ATT_Tipo, p.Tipo);
                            bl.Add(Constantes.ATT_Comprimento, p.Comprimento);
                            bl.Add(Constantes.ATT_Descricao, p.Descricao);
                            bl.Add(Constantes.ATT_Destino, p.Destino);
                            bl.Add(Constantes.ATT_Quantidade, p.Quantidade);

                            Blocos.Inserir(acDoc, p.Nome_Bloco, pcentro, escala * .8, 0, bl);

                        }
                        else
                        {
                            hp.Add(Constantes.ATT_N, p.Numero);
                        }
                        hp.Add(Constantes.ATT_Quantidade, p.Quantidade);
                        hp.Add(Constantes.ATT_Destino, p.Destino);
                        hp.Add("DESCRICAO", p.Descricao);

                        Blocos.Inserir(acDoc, Constantes.Tabela_Pecas_Linha, p1, escala, 0, hp);
                        p1 = new Point2d(p1.X, p1.Y - (escala * 6.43));
                        seq++;
                    }

                    //p0a = new Point3d(p0a.X + (x_tabela * escala), p0a.Y, p0a.Z);
                    //p1 = new Point3d(p0a.X, p0a.Y, p0a.Z);
                }
              

            }
            return new Point2d(x0, y0);

        }
        public static Point2d TecnoMetal(List<DB.Linha> pecas_tecnometal, Point2d p0, double mover_direita = 0, double escala = 1)
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
                    p0 = new Point2d(p0.X + (mover_direita), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    Hashtable htt = new Hashtable();
                    int decimais = 4;
                    string dec_str = "N4";

                    var pecas = pecas_tecnometal.GroupBy(x => x.Get(Constantes.ATT_MAR).Valor).Select(X => X.ToList());
                    double total_superficie = 0;
                    double total_peso = 0;
                    foreach (var pc in pecas)
                    {
                        var marca = pc.FindAll(x => x.Get(Constantes.ATT_POS).Valor == "");
                        var posics = pc.FindAll(x => x.Get(Constantes.ATT_POS).Valor != "");
                        int qtd = marca[0].Get(Constantes.ATT_QTD).Int();
                        double peso_unit = posics.Sum(x => x.Get(Constantes.ATT_PES).Double() * x.Get(Constantes.ATT_QTD).Int());
                        double sup_unit = posics.Sum(x => x.Get(Constantes.ATT_SUP).Double() * x.Get(Constantes.ATT_QTD).Int());
                        marca[0].Set(Constantes.ATT_PES, peso_unit);
                        marca[0].Set(Constantes.ATT_SUP, sup_unit);
                        total_superficie += (sup_unit * qtd);
                        total_peso += (peso_unit * qtd);
                    }

                    total_peso = Math.Round(total_peso/1000, decimais);
                    total_superficie = Math.Round(total_superficie, decimais);

                    htt.Add("PESO_TOTAL", total_peso.ToString(dec_str).Replace(",", "") + " ton");
                    htt.Add("SUPERFICIE_TOTAL",total_superficie.ToString("N1").Replace(",", "") + " m²");
                    Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Titulo, p0, fator_escala, 0, htt);
                    p0 = new Point2d(p0.X, p0.Y - (fator_escala * 20.4));
                    int seq = 1;
                    foreach(var Marca in pecas)
                    {


                        foreach (var Pos in Marca)
                        {
                            string descricao = Pos.Get(Constantes.ATT_MER).Valor;
                            if(Pos.Get(Constantes.ATT_POS).Valor != "")
                            {
                                descricao = Pos.Get(Constantes.ATT_PER).Valor;
                                if (descricao == "")
                                {
                                    descricao =
                                        "Ch. " + Pos.Get(Constantes.ATT_ESP).Double().ToString("N2").Replace(",", "") +
                                        " x " + Pos.Get(Constantes.ATT_LRG).Double().ToString("N1").Replace(",", "") +
                                        " x " + Pos.Get(Constantes.ATT_CMP).Double().ToString("N1").Replace(",", "");
                                }
                            }

                            var tipo = Pos.Get(Constantes.ATT_REC).ToString();
                            
                            Hashtable ht = new Hashtable();
                            ht.Add(Constantes.ATT_Marca, tipo == Constantes.ATT_REC_MARCA?Pos.Get(Constantes.ATT_MAR): Pos.Get(Constantes.ATT_POS));
                            ht.Add(Constantes.ATT_Quantidade, Pos.Get(Constantes.ATT_QTD));
                            ht.Add(Constantes.ATT_Descricao, descricao );
                            ht.Add(Constantes.ATT_Material, Pos.Get(Constantes.ATT_MAT));
                            ht.Add(Constantes.ATT_Cod_SAP, Pos.Get(Constantes.ATT_SAP));
                            ht.Add("PESO_UNIT", Math.Round(Pos.Get(Constantes.ATT_PES).Double() /1000,decimais).ToString(dec_str).Replace(",",""));
                            ht.Add("PESO_TOT", Math.Round(Pos.Get(Constantes.ATT_PES).Double() /1000 * Pos.Get(Constantes.ATT_QTD).Int(), decimais).ToString(dec_str).Replace(",", ""));
                            ht.Add(Constantes.ATT_Ficha_Pintura, Pos.Get(Constantes.ATT_FIC));

                            Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal, p0, fator_escala, 0, ht);
                            p0 = new Point2d(p0.X, p0.Y - (fator_escala * 4.25));
                            seq++;
                        }
                        Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Vazia, p0, fator_escala, 0, new Hashtable());
                        p0 = new Point2d(p0.X, p0.Y - (fator_escala * 4.25));
                    }
                   
                }
            }
            return new Point2d(x0, y0);

        }


        public static Point2d TecnoMetal(List<MarcaTecnoMetal> pecas_tecnometal, Point2d p0, double mover_direita = 0, double escala = 1)
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
                    p0 = new Point2d(p0.X + (mover_direita), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    int decimais = 4;
                    string dec_str = "N4";


                    double total_superficie = pecas_tecnometal.Sum(x=>x.Superficie  * x.Quantidade);
                    double total_peso = pecas_tecnometal.Sum(x=>x.PesoUnit * x.Quantidade);


                    total_peso = Math.Round(total_peso / 1000, decimais);
                    total_superficie = Math.Round(total_superficie, decimais);

                    Hashtable ht = new Hashtable();
                    ht.Add("PESO_TOTAL", total_peso.ToString(dec_str).Replace(",", "") + " ton");
                    ht.Add("SUPERFICIE_TOTAL", total_superficie.ToString("N1").Replace(",", "") + " m²");
                    Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Titulo, p0, fator_escala, 0, ht);
                    p0 = new Point2d(p0.X, p0.Y - (fator_escala * 20.4));
                    foreach (var Marca in pecas_tecnometal)
                    {

                        Hashtable mp = new Hashtable();
                        mp.Add(Constantes.ATT_Marca, Marca.Marca);
                        mp.Add(Constantes.ATT_Quantidade, Marca.Quantidade);
                        mp.Add(Constantes.ATT_Descricao, Marca.Mercadoria);
                        mp.Add(Constantes.ATT_Material, Marca.Material);
                        mp.Add(Constantes.ATT_Cod_SAP, Marca.SAP);
                        mp.Add("PESO_UNIT", Math.Round(Marca.PesoUnit / 1000, decimais).ToString(dec_str).Replace(",", ""));
                        mp.Add("PESO_TOT", Math.Round(Marca.PesoUnit * Marca.Quantidade / 1000 , decimais).ToString(dec_str).Replace(",", ""));
                        mp.Add(Constantes.ATT_Ficha_Pintura, Marca.Tratamento);

                        Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal, p0, fator_escala, 0, mp);
                        p0 = new Point2d(p0.X, p0.Y - (fator_escala * 4.25));
                        foreach (var Pos in Marca.GetPosicoes())
                        {
                            Hashtable hp = new Hashtable();
                            hp.Add(Constantes.ATT_Marca, Pos.Posicao);
                            hp.Add(Constantes.ATT_Quantidade, Math.Round(Pos.Quantidade * Marca.Quantidade,decimais).ToString().Replace(",","."));
                            hp.Add(Constantes.ATT_Descricao, Pos.Descricao);
                            hp.Add(Constantes.ATT_Material, Pos.Material);
                            hp.Add(Constantes.ATT_Cod_SAP, Pos.SAP);
                            hp.Add("PESO_UNIT", Math.Round(Pos.PesoUnit / 1000, decimais).ToString(dec_str).Replace(",", ""));
                            hp.Add("PESO_TOT", Math.Round(Pos.PesoUnit/1000 * Pos.Quantidade, decimais).ToString(dec_str).Replace(",", ""));
                            hp.Add(Constantes.ATT_Ficha_Pintura, Pos.Tratamento);

                            Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal, p0, fator_escala, 0, hp);
                            p0 = new Point2d(p0.X, p0.Y - (fator_escala * 4.25));
                        }
                        Blocos.Inserir(acDoc, Constantes.Tabela_TecnoMetal_Vazia, p0, fator_escala, 0, new Hashtable());
                        p0 = new Point2d(p0.X, p0.Y - (fator_escala * 4.25));
                    }

                }
            }
            return new Point2d(x0, y0);

        }
    }
}
