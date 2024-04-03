using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DLM.cad.CAD;

namespace DLM.cad
{
    public class Tabelas
    {
        public static void InserirTabela(DBRM_User dbase)
        {


            var macros = dbase.TabelaMacros.Select(x => x.GetObjeto()).ToList();

            var purlins = macros.Get<Conexoes.Macros.Purlin>();
            var tirantes = macros.Get<DLM.macros.Tirante>();
            var correntes = macros.Get<DLM.macros.Corrente>();
            var contraventos = macros.Get<DLM.macros.CTV2>();

            var opcoes = new List<string>();
            if (purlins.Count > 0)
            {
                opcoes.Add("Purlins");
            }
            if (tirantes.Count > 0)
            {
                opcoes.Add("Tirantes");
            }
            if (correntes.Count > 0)
            {
                opcoes.Add("Correntes");
            }
            if (contraventos.Count > 0)
            {
                opcoes.Add("Contraventos");
            }
            if (dbase.GetRMAFim().Count > 0)
            {
                opcoes.Add("RMAs");
            }
            if (dbase.GetRMEFim().Count > 0)
            {
                opcoes.Add("RMEs");
            }
            if (dbase.GetRMUFim().Count > 0)
            {
                opcoes.Add("RMUs");
            }
            if (dbase.GetRMTFim().Count > 0)
            {
                opcoes.Add("RMTs");
            }

            var selecao = opcoes.ListaSelecionarVarios(false, "Selecione quais peças deseja inserir");

            List<object> items = new List<object>();

            if (selecao.Find(x => x == "Purlins") != null)
            {
                items.AddRange(purlins);
            }

            if (selecao.Find(x => x == "Tirantes") != null)
            {
                items.AddRange(tirantes);
            }
            if (selecao.Find(x => x == "Correntes") != null)
            {
                items.AddRange(correntes);
            }
            if (selecao.Find(x => x == "Contraventos") != null)
            {
                items.AddRange(contraventos);
            }
            if (selecao.Find(x => x == "RMAs") != null)
            {
                items.AddRange(dbase.GetRMAFim());
            }
            if (selecao.Find(x => x == "RMEs") != null)
            {
                items.AddRange(dbase.GetRMEFim());
            }
            if (selecao.Find(x => x == "RMUs") != null)
            {
                items.AddRange(dbase.GetRMUFim());
            }
            if (selecao.Find(x => x == "RMTs") != null)
            {
                items.AddRange(dbase.GetRMTFim());
            }

            if (items.Count > 0)
            {
                InserirTabela(items);
            }
        }

        public static void InserirTabela(List<object> objetos)
        {
            var rmas = objetos.Get<RMA>();
            var rmes = objetos.GetRMEs();
            var rmus = objetos.GetRMUs();
            var rmts = objetos.Get<RMT>();
            var purlins = objetos.Get<Conexoes.Macros.Purlin>();
            var tirantes = objetos.Get<DLM.macros.Tirante>();
            var correntes = objetos.Get<DLM.macros.Corrente>();
            var contraventos = objetos.Get<DLM.macros.CTV2>();


            bool cancelado = false;
            var p0 = Ut.PedirPonto("Selecione a origem para a tabela de peças.", out cancelado);

            if (cancelado)
            {
                return;
            }
            double larg = Cfg.Init.CAD_TABLE_WIDTH;

            if (purlins.Count > 0)
            {
                InserirTabela(purlins, p0);
            }


            if (contraventos.Count > 0)
            {
                InserirTabela(contraventos, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }

            if (correntes.Count > 0)
            {
                InserirTabela(correntes, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }

            if (tirantes.Count > 0)
            {
                InserirTabela(tirantes, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }

            if (rmes.Count > 0)
            {
                InserirTabela(rmes, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }
            if (rmus.Count > 0)
            {
                InserirTabela(rmus, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }
            if (rmts.Count > 0)
            {
                InserirTabela(rmts, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }

            if (rmas.Count > 0)
            {
                InserirTabela(rmas, p0, larg);
                larg += Cfg.Init.CAD_TABLE_WIDTH;
            }



        }
        public static void InserirTabela(Conexoes.DBRM_Offline dbase)
        {
            bool cancelado = false;
            var p0 = Ut.PedirPonto("Selecione a origem para a tabela de peças.", out cancelado);

            if (cancelado)
            {
                return;
            }

            var macros = dbase.RM_Macros.Select(x => x.GetObjeto()).ToList();

            var purlins = macros.Get<Conexoes.Macros.Purlin>();
            var tirantes = macros.Get<DLM.macros.Tirante>();
            var correntes = macros.Get<DLM.macros.Corrente>();
            var contraventos = macros.Get<DLM.macros.CTV2>();


            var opcoes = new List<string>();
            if (purlins.Count > 0)
            {
                opcoes.Add("Purlins");
            }
            if (tirantes.Count > 0)
            {
                opcoes.Add("Tirantes");
            }
            if (correntes.Count > 0)
            {
                opcoes.Add("Correntes");
            }
            if (contraventos.Count > 0)
            {
                opcoes.Add("Contraventos");
            }
            if (dbase.RMA.Count > 0)
            {
                opcoes.Add("RMAs");
            }
            if (dbase.RME.Count > 0)
            {
                opcoes.Add("RMEs");
            }
            if (dbase.RMU.Count > 0)
            {
                opcoes.Add("RMUs");
            }
            if (dbase.RMT.Count > 0)
            {
                opcoes.Add("RMTs");
            }

            var selecao = opcoes.ListaSelecionarVarios(false, "Selecione quais peças deseja inserir");

            List<object> items = new List<object>();

            if (selecao.Find(x => x == "Purlins") != null)
            {
                items.AddRange(purlins);
            }

            if (selecao.Find(x => x == "Tirantes") != null)
            {
                items.AddRange(tirantes);
            }
            if (selecao.Find(x => x == "Correntes") != null)
            {
                items.AddRange(correntes);
            }
            if (selecao.Find(x => x == "Contraventos") != null)
            {
                items.AddRange(contraventos);
            }
            if (selecao.Find(x => x == "RMAs") != null)
            {
                items.AddRange(dbase.RMA);
            }
            if (selecao.Find(x => x == "RMEs") != null)
            {
                items.AddRange(dbase.RME);
            }
            if (selecao.Find(x => x == "RMUs") != null)
            {
                items.AddRange(dbase.RMU);
            }
            if (selecao.Find(x => x == "RMTs") != null)
            {
                items.AddRange(dbase.RMT);
            }

            if (items.Count > 0)
            {
                InserirTabela(items);
            }

        }
        public static P3d InserirTabela(List<Conexoes.Macros.Purlin> purlins, P3d p0)
        {
            double x0 = 0;
            double y0 = 0;
            if (purlins.Count > 0)
            {
                double escala = acCurDb.Dimscale;


                x0 = p0.X;
                y0 = p0.Y;
                var ht = new db.Linha();
                ht.Add("TITULO", "LISTA DE PURLINS");
                Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas_Titulo, p0, escala, 0, ht);
                p0 = new P3d(p0.X, p0.Y - (escala * 12.86));
                foreach (var obj in purlins)
                {
                    var hp = new db.Linha();
                    hp.Add(Cfg.Init.CAD_ATT_N, obj.Sequencia);
                    hp.Add(Cfg.Init.CAD_ATT_Perfil, obj.Nome);
                    hp.Add(Cfg.Init.CAD_ATT_Quantidade, obj.Quantidade.String(3));
                    hp.Add(Cfg.Init.CAD_ATT_Comprimento, obj.Comprimento.String(0, 5));
                    hp.Add(Cfg.Init.CAD_ATT_Espessura, obj.Espessura.String(2, 5));
                    hp.Add(Cfg.Init.CAD_ATT_Destino, Cfg.Init.CAD_ATT_RME);
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas, p0, escala, 0, hp);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                }

            }
            return new P3d(x0, y0);

        }
        public static P3d InserirTabela(List<DLM.macros.CTV2> contravento, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (contravento.Count > 0)
            {
                double escala = acCurDb.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                x0 = p0.X;
                y0 = p0.Y;
                var htt = new db.Linha();
                htt.Add("TITULO", "LISTA DE CONTRAVENTOS");
                Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tirantes_Titulo, p0, escala, 0, htt);
                p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                var offsetY = (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2);

                foreach (var obj in contravento)
                {
                    obj.Calcular();
                    var ht = new db.Linha();
                    ht.Add("ORDEM", obj.Sequencia);
                    ht.Add(Cfg.Init.CAD_ATT_Peca, obj.Marca);
                    ht.Add(Cfg.Init.CAD_ATT_Quantidade, obj.Quantidade.String(3));
                    ht.Add(Cfg.Init.CAD_ATT_Comprimento, obj.Comprimento.String(0, 5));
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tirantes, p0, escala, 0, ht);
                    p0 = p0.MoverY(-offsetY);
                    foreach (var pc in obj.Pecas)
                    {
                        var ht2 = new db.Linha();
                        ht2.Add(Cfg.Init.CAD_ATT_Peca, pc.Nome);
                        ht2.Add(Cfg.Init.CAD_ATT_Quantidade, pc.Quantidade.String(3));
                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tirantes_SUB, p0, escala, 0, ht2);
                        p0 = p0.MoverY(-offsetY);
                    }
                }
            }
            return new P3d(x0, y0);
        }
        public static P3d InserirTabela(List<DLM.macros.Tirante> tirante, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (tirante.Count > 0)
            {
                double escala = acCurDb.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                x0 = p0.X;
                y0 = p0.Y;
                var htt = new db.Linha();
                htt.Add("TITULO", "LISTA DE TIRANTES");
                Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tirantes_Titulo, p0, escala, 0, htt);
                p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                foreach (var p in tirante)
                {
                    var ht = new db.Linha();
                    ht.Add("ORDEM", p.Sequencia);
                    ht.Add(Cfg.Init.CAD_ATT_Peca, p.Marca);
                    ht.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade.String(3));
                    ht.Add(Cfg.Init.CAD_ATT_Comprimento, p.Comprimento.String(0, 5));
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tirantes, p0, escala, 0, ht);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                }
            }
            return new P3d(x0, y0);
        }
        public static P3d InserirTabela(List<DLM.macros.Corrente> trs, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (trs.Count > 0)
            {
                double escala = acCurDb.Dimscale;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                x0 = p0.X;
                y0 = p0.Y;
                var ht = new db.Linha();
                ht.Add("TITULO", "LISTA DE CORRENTES");
                Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Correntes_Titulo, p0, escala, 0, ht);
                p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                foreach (var p in trs)
                {
                    var hp = new db.Linha();
                    hp.Add(Cfg.Init.CAD_ATT_N, p.Sequencia);
                    hp.Add(Cfg.Init.CAD_ATT_Perfil, p.Marca);
                    hp.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade.String(3));
                    hp.Add(Cfg.Init.CAD_ATT_Vao, p.Vao.String(0));
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Correntes, p0, escala, 0, hp);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                }
            }
            return new P3d(x0, y0);
        }


        public static P3d InserirTabela(List<Conexoes.RME> RMES, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMES.Count > 0)
            {
                double escala = acCurDb.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    var ht = new db.Linha();
                    ht.Add("TITULO", "LISTA DE PEÇAS");
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas_Titulo, p0, escala, 0, ht);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                    int seq = 1;
                    foreach (var p in RMES)
                    {
                        var hp = new db.Linha();
                        hp.Add(Cfg.Init.CAD_ATT_N, seq.String(2));
                        hp.Add(Cfg.Init.CAD_ATT_Perfil, p.CODIGOFIM);
                        hp.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade.String(3));
                        hp.Add(Cfg.Init.CAD_ATT_Comprimento, p.COMP_USER.String(0, 5));
                        hp.Add(Cfg.Init.CAD_ATT_Espessura, p.ESP.String(2, 5));
                        hp.Add(Cfg.Init.CAD_ATT_Destino, Cfg.Init.EXT_RM);
                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas, p0, escala, 0, hp);
                        p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                        seq++;
                    }
                }
            }
            return new P3d(x0, y0);

        }
        public static P3d InserirTabela(List<Conexoes.RMT> rms, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (rms.Count > 0)
            {
                double escala = acCurDb.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    var ht = new db.Linha();
                    ht.Add("TITULO", "LISTA DE RMT");
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas_Titulo, p0, escala, 0, ht);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                    int seq = 1;
                    foreach (var p in rms)
                    {
                        var hp = new db.Linha();
                        hp.Add(Cfg.Init.CAD_ATT_N, seq.String(2));
                        hp.Add(Cfg.Init.CAD_ATT_Perfil, p.CODIGOFIM);
                        hp.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade.String(3));
                        hp.Add(Cfg.Init.CAD_ATT_Comprimento, p.COMP.String(0, 5));
                        hp.Add(Cfg.Init.CAD_ATT_Espessura, p.Bobina.Espessura.String(2, 5));
                        hp.Add(Cfg.Init.CAD_ATT_Destino, "RMT");
                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Tercas, p0, escala, 0, hp);
                        p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                        seq++;
                    }
                }
            }
            return new P3d(x0, y0);

        }
        public static P3d InserirTabela(List<Conexoes.RMA> RMAS, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;
            if (RMAS.Count > 0)
            {
                double escala = acCurDb.Dimscale;
                bool cancelado = false;

                if (mover_direita != 0)
                {
                    p0 = new P3d(p0.X + (mover_direita * escala), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;
                    var ht = new db.Linha();
                    ht.Add("TITULO", "LISTA DE RMA");
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Almox_Titulo, p0, escala, 0, ht);
                    p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                    int seq = 1;
                    foreach (var p in RMAS)
                    {
                        var htt = new db.Linha();
                        htt.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade);
                        htt.Add(Cfg.Init.CAD_ATT_Descricao, p.DESCRICAO);
                        htt.Add("UNID", p.UNIDADE);
                        htt.Add(Cfg.Init.CAD_ATT_Cod_SAP, p.SAP);

                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Almox, p0, escala, 0, htt);
                        p0 = new P3d(p0.X, p0.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                        seq++;
                    }
                }
            }
            return new P3d(x0, y0);

        }


        public static P3d Pecas(List<PCQuantificar> pcs, bool separar, P3d p0, double mover_direita = 0)
        {
            double x0 = 0;
            double y0 = 0;



            if (pcs.Count > 0)
            {
                double escala = acCurDb.Dimscale;

                P3d p1 = new P3d(p0.X, p0.Y);
                if (mover_direita != 0)
                {
                    p1 = new P3d(p1.X + (mover_direita * escala), p1.Y);
                }

                P3d p0a = new P3d(p1.X, p1.Y);

                List<List<PCQuantificar>> pacotes = new List<List<PCQuantificar>>();
                if (separar)
                {
                    pacotes = pcs.GroupBy(x => x.Familia).Select(X => X.ToList()).ToList();
                }
                else
                {
                    pacotes = new List<List<PCQuantificar>> { pcs };
                }

                foreach (var pacote in pacotes)
                {
                    x0 = p1.X;
                    y0 = p1.Y;
                    var ht = new db.Linha();
                    ht.Add("TITULO", "LISTA " + pacote[0].Familia.ToUpper());
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Pecas_Titulo, p1, escala, 0, ht);
                    p1 = new P3d(p1.X, p1.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE));
                    int seq = 1;
                    var linhas = pacote.OrderBy(x => x.Numero + "|" + x.Nome).ToList();
                    foreach (var p in linhas)
                    {
                        var hp = new db.Linha();
                        hp.Add(Cfg.Init.CAD_ATT_Marca, p.Nome);
                        if (p.Nome_Bloco.StartsWith(Cfg.Init.CAD_PC_Quantificar) && !p.Nome_Bloco.Contains(Cfg.Init.CAD_ATT_Texto))
                        {
                            P3d pcentro = new P3d(p1.X + (escala * 6.9894), p1.Y + (escala * -3.2152));
                            var bl = new db.Linha();
                            //foreach (var obj in p.Atributos.Celulas)
                            //{
                            //    bl.Add(obj.Coluna, obj.Valor);
                            //}

                            bl.Add(Cfg.Init.CAD_ATT_N, p.Atributos[Cfg.Init.CAD_ATT_N].Valor);
                            bl.Add(Cfg.Init.CAD_ATT_Familia, p.Familia);
                            bl.Add(Cfg.Init.CAD_ATT_Tipo, p.Tipo);
                            bl.Add(Cfg.Init.CAD_ATT_Comprimento, p.Comprimento);
                            bl.Add(Cfg.Init.CAD_ATT_Descricao, p.Descricao);
                            bl.Add(Cfg.Init.CAD_ATT_Destino, p.Destino);
                            bl.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade);

                            Blocos.Inserir(acDoc, p.Nome_Bloco, pcentro, escala * .8, 0, bl);

                        }
                        else
                        {
                            hp.Add(Cfg.Init.CAD_ATT_N, p.Numero);
                        }
                        hp.Add(Cfg.Init.CAD_ATT_Quantidade, p.Quantidade);
                        hp.Add(Cfg.Init.CAD_ATT_Destino, p.Destino);
                        hp.Add("DESCRICAO", p.Descricao);

                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_Pecas_Linha, p1, escala, 0, hp);
                        p1 = new P3d(p1.X, p1.Y - (escala * Cfg.Init.CAD_TABLE_HEADER_SCALE / 2));
                        seq++;
                    }

                    //p0a = new Point3d(p0a.X + (x_tabela * escala), p0a.Y, p0a.Z);
                    //p1 = new Point3d(p0a.X, p0a.Y, p0a.Z);
                }


            }
            return new P3d(x0, y0);

        }



        public static P3d TecnoMetal(List<MarcaTecnoMetal> pecas_tecnometal, P3d p0, double mover_direita = 0, double escala = 1)
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
                    p0 = new P3d(p0.X + (mover_direita), p0.Y);
                }

                if (!cancelado)
                {
                    x0 = p0.X;
                    y0 = p0.Y;



                    double total_superficie = pecas_tecnometal.Sum(x => x.Superficie * x.Quantidade);
                    double total_peso = pecas_tecnometal.Sum(x => x.PesoUnit * x.Quantidade);


                    total_peso = (total_peso / 1000);


                    var ht = new db.Linha();
                    ht.Add("PESO_TOTAL", total_peso.Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS) + " ton");
                    ht.Add("SUPERFICIE_TOTAL", total_superficie.String(1).Replace(",", "") + " m²");
                    Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_TecnoMetal_Titulo, p0, fator_escala, 0, ht);
                    p0 = new P3d(p0.X, p0.Y - (fator_escala * 20.4));
                    foreach (var Marca in pecas_tecnometal)
                    {
                        var m_pes_uni = (Marca.PesoUnit / 1000);
                        if (m_pes_uni < Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON)
                        {
                            m_pes_uni = Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON;
                        }

                        var m_pes_tot = (Marca.PesoUnit / 1000) * Marca.Quantidade;
                        if (m_pes_tot < Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON)
                        {
                            m_pes_tot = Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON;
                        }
                        var mp = new db.Linha();
                        mp.Add(Cfg.Init.CAD_ATT_Marca, Marca.Nome);
                        mp.Add(Cfg.Init.CAD_ATT_Quantidade, Marca.Quantidade.Round(2));
                        mp.Add(Cfg.Init.CAD_ATT_Descricao, Marca.Mercadoria);
                        mp.Add(Cfg.Init.CAD_ATT_Material, Marca.Material);
                        mp.Add(Cfg.Init.CAD_ATT_Cod_SAP, Marca.SAP);
                        mp.Add(Cfg.Init.CAD_ATT_Peso_Unit, m_pes_uni.String(Cfg.Init.TEC_DECIMAIS_PESO_TABELA));
                        mp.Add(Cfg.Init.CAD_ATT_Peso_Tot, m_pes_tot.String(Cfg.Init.TEC_DECIMAIS_PESO_TABELA));
                        mp.Add(Cfg.Init.CAD_ATT_Ficha_Pintura, Marca.Tratamento);

                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_TecnoMetal, p0, fator_escala, 0, mp);
                        p0 = new P3d(p0.X, p0.Y - (fator_escala * 4.25));
                        foreach (var Pos in Marca.GetPosicoes())
                        {
                            var p_pes_uni = (Pos.PesoUnit / 1000);
                            if (p_pes_uni < Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON)
                            {
                                p_pes_uni = Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON;
                            }

                            var p_pes_tot = (Pos.PesoUnit / 1000) * Pos.Quantidade * (Marca.Tipo_Marca == Tipo_Marca.MarcaSimples ? 1 : Marca.Quantidade);
                            if (p_pes_tot < Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON)
                            {
                                p_pes_tot = Cfg.Init.TEC_DECIMAIS_PESO_MINIMO_TON;
                            }

                            var hp = new db.Linha();
                            hp.Add(Cfg.Init.CAD_ATT_Marca, Pos.Nome_Posicao);
                            hp.Add(Cfg.Init.CAD_ATT_Quantidade, (Pos.Quantidade * Marca.Quantidade).Round(2));
                            hp.Add(Cfg.Init.CAD_ATT_Descricao, $"{Pos.Perfil} x {Pos.Comprimento.Round(0)}".Esquerda(34, false));
                            hp.Add(Cfg.Init.CAD_ATT_Material, Pos.Material);
                            hp.Add(Cfg.Init.CAD_ATT_Cod_SAP, Pos.SAP);
                            hp.Add(Cfg.Init.CAD_ATT_Peso_Unit, p_pes_uni.String(Cfg.Init.TEC_DECIMAIS_PESO_TABELA));
                            hp.Add(Cfg.Init.CAD_ATT_Peso_Tot, p_pes_tot.String(Cfg.Init.TEC_DECIMAIS_PESO_TABELA));
                            hp.Add(Cfg.Init.CAD_ATT_Ficha_Pintura, Pos.Tratamento);

                            Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_TecnoMetal, p0, fator_escala, 0, hp);
                            p0 = new P3d(p0.X, p0.Y - (fator_escala * 4.25));
                        }
                        Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_TAB_TecnoMetal_Vazia, p0, fator_escala, 0, new db.Linha());
                        p0 = new P3d(p0.X, p0.Y - (fator_escala * 4.25));
                    }

                }
            }
            return new P3d(x0, y0);

        }
    }
}
