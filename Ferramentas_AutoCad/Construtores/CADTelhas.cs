using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static DLM.cad.CAD;

namespace DLM.cad
{
    [Serializable]
    public class CADTelhas : CADBase
    {
        public double MultiplicadorEscala { get; set; } = 15;
        public double SomarCaboDobraSFLH { get; set; } = 300;
        public string LayerLinhaDeVida { get; set; } = "LINHA_DE_VIDA";
        public string LayerLinhaDeVidaCotas { get; set; } = "COTAS_LINHA_DE_VIDA";
        public string LayerPassarela { get; set; } = "PASSARELA";
        public string LayerPassarelaCotas { get; set; } = "COTAS_PASSARELA";

        public int Qtd_Sapatilha { get; set; } = 2;
        public int Qtd_Clip_Pesado { get; set; } = 3;
        public string Codigo_SFLH { get; set; } = "310001444242";
        public string SFLH { get; set; } = "SFL-H";
        public string SFLI { get; set; } = "SFL-I";
        public string Codigo_SFLI { get; set; } = "310001444243";
        public string Codigo_Cabo { get; set; } = "1001305";
        public string Codigo_Passarela { get; set; } = "1004277";
        public string Codigo_Esticador { get; set; } = "1001304";
        public string Codigo_Manilha { get; set; } = "1001303";

        public string Codigo_Sapatilha { get; set; } = "1001306";

        public string Codigo_Clip_Pesado { get; set; } = "1001307";

        public double QtdFita_Por_Emenda { get; set; } = 100;
        public double Fita_Rendimento { get; set; } = 10000;
        public string Codigo_Fita_Isolante { get; set; } = "1009095";
        public double EspessuraCabo { get; set; } = 10;
        public double ToleranciaY { get; set; } = 3000;
        public double DistMaxSFLI { get; set; } = 13400;
        public double DistMaxSFLH { get; set; } = 80000;
        public double LarguraTelha { get; set; } = 610;

        public bool AdicionarCotas { get; set; } = true;

        public List<BlockReference> Getpassarelas()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name == Conexoes.Utilz.getNome(Cfg.Init.CAD_Peca_PASSARELA));
        }

        public List<Entity> Getcotaslinhadevida()
        {
            return Selecoes.GetDimmensions().FindAll(x => x.Layer == LayerLinhaDeVidaCotas | x.Layer == LayerLinhaDeVida);
        }

        public List<Entity> GetCotasPassarelas()
        {
            return Selecoes.GetDimmensions().FindAll(x => x.Layer == LayerPassarela | x.Layer == LayerLinhaDeVida);
        }

        public List<Polyline> Getcabos()
        {
            return Selecoes.Filter<Polyline>().FindAll(x => x.Layer == LayerLinhaDeVida);
        }
        public List<BlockReference> GetblocostextoLinhaDeVida()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name == Conexoes.Utilz.getNome(Cfg.Init.CAD_BL_Texto));
        }
        public void ApagarLinhaDeVida()
        {
            SelecionarObjetos();
            List<Entity> list_apagar = new List<Entity>();
            list_apagar.AddRange(this.Getsflhs());
            list_apagar.AddRange(this.Getsflis());
            list_apagar.AddRange(this.Getcabos());
            list_apagar.AddRange(this.GetblocostextoLinhaDeVida().FindAll(x => x.Layer == LayerLinhaDeVida));
            list_apagar.AddRange(Selecoes.GetDimmensions().FindAll(x => x.Layer == LayerLinhaDeVida | x.Layer == LayerLinhaDeVidaCotas));


            acDoc.Apagar(list_apagar);
        }
        public void ApagarPassarelas()
        {

            SelecionarObjetos();
            List<Entity> list_apagar = new List<Entity>();
            list_apagar.AddRange(this.Getpassarelas());
            list_apagar.AddRange(Selecoes.GetDimmensions().FindAll(x => x.Layer == LayerPassarela | x.Layer == LayerPassarelaCotas));


            acDoc.Apagar(list_apagar);

        }

        public List<BlockReference> Getsflhs()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name == Conexoes.Utilz.getNome(Cfg.Init.CAD_Peca_SFLH));
        }
        public List<BlockReference> GetLinhasDeVida()
        {
            List<BlockReference> retorno = new List<BlockReference>();
            retorno.AddRange(Getsflhs());
            retorno.AddRange(Getsflis());
            return retorno;
        }
        public List<BlockReference> Getsflis()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name == Conexoes.Utilz.getNome(Cfg.Init.CAD_Peca_SFLI));
        }

        public void AlinharLinhaDeVida()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos();




                var xls = Selecoes.Filter<Xline>();

                var sflis = Getsflis();
                var sflhs = Getsflhs();
                var textos = GetblocostextoLinhaDeVida();

                var cabos = Getcabos();
                var cabos_verticais = Ut.PolylinesVerticais(cabos);
                var cabos_horizontais = Ut.PolylinesHorizontais(cabos);

                var cotas = Getcotaslinhadevida();
                var cotas_verticais = Ut.CotasVerticais(cotas);
                var cotas_horizontais = Ut.CotasHorizontais(cotas);

                var etts = new List<Entity>();
                etts.AddRange(sflis);
                etts.AddRange(sflhs);
                etts.AddRange(cabos);

                Point3d ie, id, se, sd;


                Ut.GetBordas(etts, out se, out sd, out ie, out id);
                double maxY = (sd.Y > se.Y ? sd.Y : se.Y) + ToleranciaY;
                double minY = (sd.Y < se.Y ? sd.Y : se.Y) - ToleranciaY;

                //filtra as xls que estão dentro ou próximas dos objetos
                AddMensagem("\nXlines Encontradas:" + xls.Count);

                xls = Ut.XLinesHorizontais(xls);
                AddMensagem("\nXlines Horizontais Encontradas:" + xls.Count);




                var pcs = new List<BlockReference>();
                pcs.AddRange(sflhs);
                pcs.AddRange(sflis);
                Utils.SetUndoMark(true);
                foreach (var o in pcs)
                {
                    var p0 = new P3dCAD(o.Position);
                    var xlss = Ut.GetXlinesProximas(o, xls, ToleranciaY);
                    var xl = Ut.GetXlineMaisProxima(o, xls, ToleranciaY);
                    if(xl == null) {
                        AddMensagem("\nNenhuma Xline encontrada. (SFLH) " + o.Position);
                        continue; }
                    var pos = new P3d(o.Position.X, xl.BasePoint.Y);
                    Blocos.Mover(o, pos);

                    var polis = Ut.GetPolylinesProximas(cabos, p0, 100);
                      
                    foreach(var p in polis)
                    {
                        var d1 = p.StartPoint.P3d().Distancia(p0);
                        var d2 = p.EndPoint.P3d().Distancia(p0);
                        P3d pp1 = p.StartPoint.P3d();
                        P3d pp2 = p.EndPoint.P3d();

                        var ang = pp1.GetAngulo(pp2);
                        if (ang<0)
                        {
                            ang = 360 - ang;
                        }

                        if(ang == 0 | ang == 180)
                        {
                            pp1 = new P3d(pp1.X, pos.Y, 0);
                            pp2 = new P3d(pp2.X, pos.Y, 0);
                        }
                        else if(ang == 90 | ang == 270)
                        {
                            pp1 = new P3d(pos.X, pp1.Y, 0);
                            pp2 = new P3d(pos.X, pp2.Y, 0);
                        }
                        
                        p.Erase(true);
                        AddPolyLine(new List<P3d> { pp1, pp2 }, this.EspessuraCabo, this.EspessuraCabo, System.Drawing.Color.Red);
                    }

                    var ccotas = Ut.GetCotasProximas(Getcotaslinhadevida().FindAll(x=>x is RotatedDimension).Select(x=> x as RotatedDimension).ToList(), p0, 100);

                    foreach (var p in ccotas)
                    {
                        var d1 = new P3dCAD(p.XLine1Point).Distancia(p0);
                        var d2 = new P3dCAD(p.XLine2Point).Distancia(p0);
                        var ptt = new Point3d(pos.X, pos.Y, 0);
                        if (d1 < d2)
                        {
                            p.XLine1Point = ptt;
                        }
                        else
                        {
                            p.XLine2Point = ptt;
                        }
                    }


                    var angulo = p0.GetAngulo(pos);
                    var dist = p0.Distancia(pos);
                    var blocos_texto = Ut.GetBlocosProximos(textos, pos, GetEscala() * 20);
                    foreach(var p in blocos_texto)
                    {
                        var p_texto = new P3dCAD(p.Position);
                        var pxx = p_texto.Mover(angulo, dist);
                        Blocos.Mover(p,pxx);
                    }
                }
                Utils.SetUndoMark(false);

                acTrans.Commit();
                editor.Regen();
            }

        }
        public void InserirPassarela(bool selecionar = false)
        {

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                //var selecao = SelecionarObjetos(acTrans);

            
                double vert = 660;
                double angulo = 0;
                double comp = 0;

                double mov = this.LarguraTelha;

                //var sentido_telha = PerguntaString("Qual o Sentido da Telha?", new List<string> { "Vertical", "Horizontal" });
                //if(sentido_telha == "Horizontal")
                //{

                //}
                //if(sentido_telha!="Vertical" && sentido_telha!="Horizontal")
                //{
                //    return;
                //}
                bool cancelado = false;

                P3d p1 = new P3d();
                if (selecionar)
                {
                    SelecionarObjetos();
                    if (this.Getpassarelas().Count>0)
                    {
                        var p = this.Getpassarelas()[0];
                        p1 = p.Position.P3d();
                        p.Erase(true);
                    }
                    else
                    {
                        AddMensagem("\nNenhuma Passarela selecionada.");
                        return;
                    }
                }
                else
                {
                    p1 = Ut.PedirPonto("Selecione o ponto inicial", out cancelado);
                }

                int sequencia = 0;
                if(!cancelado)
                {
                denovo:
                    Point3d p0 = new Point3d(p1.X, p1.Y, p1.Z);
                    Ut.SetOrtho(true);
                    var p2 = Ut.PedirPonto("Selecione o ponto final",p1, out cancelado);
                    if(!cancelado)
                    {

                        if (sequencia ==0 && !selecionar)
                        {
                            p1 = p1.Mover(angulo, this.LarguraTelha/2);
                            angulo = p1.GetAngulo(p2);
                            var tmpang = Angulo.Normalizar(angulo);
                            if (tmpang == 90 | tmpang == 270)
                            {
                                p1 = p1.Mover(tmpang, vert / 2);
                            }
                        }
                        angulo = p1.GetAngulo(p2);

                        comp = p1.Distancia(p2);

                        Ajustar(ref angulo, ref comp, p1, ref p2);
                        

                        if (Math.Abs(comp) > this.LarguraTelha)
                        {
                            FLayer.Criar(LayerPassarela, System.Drawing.Color.White);

                            var pcs = Conexoes.Utilz.ArredondarMultiplo(Math.Abs(comp), this.LarguraTelha);
                            int qtd = Conexoes.Utilz.Int(pcs / this.LarguraTelha);
                            Utils.SetUndoMark(true);
                            for (int i = 0; i < qtd; i++)
                            {
                                Hashtable tt = new Hashtable();
                                tt.Add(Cfg.Init.CAD_ATT_Cod_SAP, this.Codigo_Passarela);
                                Blocos.Inserir(CAD.acDoc, Cfg.Init.CAD_Peca_PASSARELA, p1, 1, 0, tt);
                                if(angulo==90 | angulo == 270)
                                {
                                    mov = vert;
                                }
                                else
                                {
                                    mov = this.LarguraTelha;
                                }

                               var p3 = p1.Mover(angulo, mov);


                                p1 = p3;
                                
                            }
                            if (AdicionarCotas)
                            {
                                FLayer.Criar(LayerPassarelaCotas, System.Drawing.Color.White);
                                var d1 = new P3dCAD(p0);
                                if(sequencia>0)
                                {
                                    d1 = (P3dCAD)d1.Mover(angulo, -mov / 2);
                                }
                                else if(selecionar)
                                {
                                    d1 = (P3dCAD)d1.Mover(angulo, mov / 2);
                                    qtd = qtd - 1;
                                }

                                var d2 = p1.Mover(angulo, -mov/2);
                                var ce = d1.Centro(d2);
                                var dist = Math.Round(d1.Distancia(d2));
                                if (angulo == 90 | angulo == 270)
                                {
                                    AddCotaVertical(d1, d2, dist + " (" + qtd + "x)", ce.Mover(angulo - 90, GetEscala() * MultiplicadorEscala), false, 0, false, false);
                                }
                                else
                                {
                                    AddCotaHorizontal(d1, d2, dist + " (" + qtd + "x)", ce.Mover(angulo - 90, GetEscala() * MultiplicadorEscala), false, 0, false, false);

                                }
                            }
                            sequencia++;
                            Utils.SetUndoMark(false);

                            
                        }
                        
                        goto denovo;
                    }
                    

                }
                acTrans.Commit();
                editor.Regen();
            }
        }


        public void InserirLinhaDeVida(bool selecionar = false)
        {

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var layer_atual = FLayer.GetAtual();

                FLayer.Criar(LayerLinhaDeVida, System.Drawing.Color.Red);

                //var selecao = SelecionarObjetos(acTrans);

   

                double angulo = 0;
                double comp = 0;

                double mov = this.LarguraTelha;


                bool cancelado = false;

                P3d p1 = new P3d();
                if (selecionar)
                {
                    SelecionarObjetos();
                    if (this.Getsflhs().Count > 0)
                    {
                        var p = this.Getsflhs()[0];
                        p1 = p.Position.P3d();
                        p.Erase(true);
                    }
                    else
                    {
                        AddMensagem("\nNenhum SFL-H selecionado.");
                        return;
                    }
                }
                else
                {
                    p1 = Ut.PedirPonto("Selecione o ponto inicial", out cancelado);
                }

                int sequencia = 0;
                if (!cancelado)
                {
                denovo:
                    Ut.SetOrtho(true);
                    var p2 = Ut.PedirPonto("Selecione o ponto final", p1, out cancelado);
                    if (!cancelado)
                    {
                        FLayer.Set(LayerLinhaDeVida);

                        List<P3d> cotas = new List<P3d>();
                        if (sequencia == 0 && !selecionar)
                        {
                            p1 = p1.Mover(angulo, this.LarguraTelha / 2);
                            
                        }
                        cotas.Add(p1);
                        comp = p1.Distancia(p2);
                        angulo = p1.GetAngulo(p2);
                        AddMensagem("\nÂngulo: " + angulo);

                   

                        Ajustar(ref angulo, ref comp, p1, ref p2);
                        AddMensagem("\nÂngulo ajustado: " + angulo);


                        if (Math.Abs(comp) > this.LarguraTelha)
                        {
                            Utils.SetUndoMark(true);

                            int qtd_telhas = Utilz.Int(comp / this.LarguraTelha);
                            //se é horizontal, alinha com a telha.
                            if (angulo == 0 | angulo == 180)
                            {
                                var compf = (this.LarguraTelha * qtd_telhas) - (this.LarguraTelha);
                                p2 = p1.Mover(angulo, compf);
                            }

                            var qtd_sflh = Conexoes.Utilz.Int(comp/ this.DistMaxSFLH);
                            var pcs = Conexoes.Utilz.ArredondarMultiplo(Math.Abs(comp), this.LarguraTelha);
                            int qtd = Conexoes.Utilz.Int(pcs / this.LarguraTelha);
                        

                            List<double> comps = new List<double>();
                            comps.Add(comp);
                            var comp_mult = Conexoes.Utilz.ArredondarMultiplo(comp / qtd_sflh, this.LarguraTelha);

                            if (qtd_sflh>1)
                            {
                                comps.Clear();
                                for (int i = 0; i < qtd_sflh; i++)
                                {
                                    if(i==qtd_sflh-1)
                                    {
                                        var sobra = comp - comps.Sum();
                                        if(angulo == 0 | angulo == 180)
                                        {
                                            sobra = sobra - (this.LarguraTelha / 2);
                                        }
                                        sobra = Conexoes.Utilz.ArredondarMultiplo(sobra, this.LarguraTelha);
                                        comps.Add(sobra);
                                    }
                                    else
                                    {
                                        comps.Add(comp_mult);
                                    }
                                }
                            }
                            //Alerta("\n Coordenadas:" + comps.Count + "\n" + string.Join("\n", comps));

                            if(comps.Count==1)
                            {
                            AddVaoSFLH(angulo, comp, p1, sequencia, p2, ref cotas);
                                //adiciona o cabo
                                AddPolyLine(new List<P3d> { new P3d(p1), new P3d(p2) }, EspessuraCabo, EspessuraCabo, System.Drawing.Color.Red);
                            }
                            else if(comps.Count>1)
                            {
                                P3d pxa = new P3d(p1.X, p1.Y, p1.Z);
                                foreach (var item in comps)
                                {
                                    var pxb = pxa.Mover(angulo, item);
                                    AddVaoSFLH(angulo,item, pxa, sequencia, pxb, ref cotas);
                                    sequencia++;
                                    cotas.Add(pxb);
                                    //adiciona o cabo
                                    AddPolyLine(new List<P3d> { pxa, pxb }, EspessuraCabo, EspessuraCabo, System.Drawing.Color.Red);
                                    pxa = pxb;
                                }
                            }


                            sequencia++;
                            if (comps.Count == 1)
                            {
                                cotas.Add(p2);
                            }

                            if (AdicionarCotas)
                            {
                                FLayer.Criar(LayerLinhaDeVidaCotas, System.Drawing.Color.White);
                                for (int i = 0; i < cotas.Count - 1; i++)
                                {
                                    if (angulo == 0 | angulo == 180)
                                    {
                                        AddCotaHorizontal(cotas[i], cotas[i + 1], "", cotas[i].Centro(cotas[i + 1]).Mover(angulo - 90, GetEscala() * 10), false, 0, false, false);
                                    }
                                    else
                                    {
                                        AddCotaVertical(cotas[i], cotas[i + 1], "", cotas[i].Centro(cotas[i + 1]).Mover(angulo - 90, GetEscala() * 10), false, 0, false, false);

                                    }
                                }
                                if (cotas.Count > 1)
                                {
                                    if (angulo == 0 | angulo == 180)
                                    {
                                        AddCotaHorizontal(cotas.OrderBy(x => x.X).First(), cotas.OrderBy(x => x.X).Last(), "", cotas.OrderBy(x => x.X).First().Centro(cotas.OrderBy(x => x.X).Last()).Mover(angulo - 90, GetEscala() * MultiplicadorEscala), false, 0, false, false);
                                    }
                                    else
                                    {
                                        AddCotaVertical(cotas.OrderBy(x => x.Y).First(), cotas.OrderBy(x => x.Y).Last(), "", cotas.OrderBy(x => x.Y).First().Centro(cotas.OrderBy(x => x.Y).Last()).Mover(angulo - 90, GetEscala() * MultiplicadorEscala), false, 0, false, false);

                                    }
                                }
                            }

                            p1 = p2;
                            Utils.SetUndoMark(false);

                        }

                        goto denovo;
                    }
                    else
                    {
                        FLayer.Set(layer_atual);
                    }

                }
                acTrans.Commit();
                editor.Regen();
            }
        }
        public void ExportarRMAdeTabela()
        {
            string destino = Conexoes.Utilz.SalvarArquivo(Cfg.Init.EXT_RM);
            if(destino==null)
            {
                return;
            }
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                DBRM_Offline pp = new DBRM_Offline();
                SelecionarObjetos();
                pp.RMA.AddRange(this.GetRMAsBlocos());
                if (pp.RMA.Count > 0)
                {
                    Tabelas.DBRM(pp);
                }

                pp.Salvar(destino);
            }
        }
        private void AddVaoSFLH(double angulo, double comp, P3d p1, int sequencia, P3d p2,  ref List<P3d> cotas)
        {
           
            Hashtable sftlh = new Hashtable();
            sftlh.Add(Cfg.Init.CAD_ATT_Cod_SAP, Codigo_SFLH);
            Hashtable sftli = new Hashtable();
            sftli.Add(Cfg.Init.CAD_ATT_Cod_SAP, Codigo_SFLI);
            if (sequencia == 0)
            {
     
                Blocos.Inserir(CAD.acDoc, Cfg.Init.CAD_Peca_SFLH, p1, 1, 0, sftlh);
                AddBlocoTexto(angulo, p1, SFLH, GetEscala() * 5, "");
                Ut.AddLeader(angulo, p1,this.GetEscala(), "MANILHA\n ESTICADOR", MultiplicadorEscala*.8);

            }
            Blocos.Inserir(CAD.acDoc, Cfg.Init.CAD_Peca_SFLH, p2, 1, 0, sftlh);
            AddBlocoTexto(angulo, p2, SFLH, GetEscala() * 5,"");
            Ut.AddLeader(angulo, p2, this.GetEscala(), "MANILHA\n ESTICADOR", this.MultiplicadorEscala * .8);

            int qtd_sfli = Conexoes.Utilz.Int(comp / this.DistMaxSFLI);

            if (qtd_sfli > 0)
            {
                double espacos = Utilz.ArredondarMultiplo(Utilz.Double(comp / qtd_sfli), this.LarguraTelha);
                var pp0 = p1.Mover(angulo, espacos);
                for (int i = 0; i < qtd_sfli - 1; i++)
                {
                    Blocos.Inserir(CAD.acDoc, Cfg.Init.CAD_Peca_SFLI, pp0, 1, 0, sftli);

                    AddBlocoTexto(angulo, pp0, SFLI, GetEscala() * 5,"");
                    cotas.Add(pp0);
                    pp0 = pp0.Mover(angulo, espacos);

                }
            }
        }
        private void AddBlocoTexto(double angulo, P3d pp0, string nome, double offset, string sap)
        {
            var p1 = pp0.Mover(angulo + 90, offset);
            var ht = new Hashtable();
            ht.Add(Cfg.Init.CAD_ATT_Texto, nome);
            ht.Add(Cfg.Init.CAD_ATT_Cod_SAP, sap);
            if(angulo==90 | angulo == 270)
            {
                //move pro lado quando é vertical
                p1 = pp0.Mover(angulo + 90, (GetEscala() * 16)/2);
            }
            Blocos.Inserir(CAD.acDoc, Cfg.Init.CAD_BL_Texto, p1, GetEscala(), 0, ht );

        }
        private void Ajustar(ref double angulo, ref double comp, P3d p1, ref P3d p2)
        {
            if (angulo < 0)
            {
                angulo = 360 + angulo;
            }

            //normaliza o ângulo e a coordenada, para evitar lançamentos inclinados
            if (angulo != 0 && angulo != 90 && angulo != 270 && angulo != 180)
            {
                angulo = Angulo.Normalizar(angulo);
                comp = 0;
                if (angulo == 0 | angulo == 180)
                {
                    comp = p1.DistanciaX(p2);
                }
                else
                {
                    comp = p1.DistanciaY(p2);
                }

                comp = comp.Abs();
                p2 = p1.Mover(angulo, comp);
                AddMensagem("\nÂngulo ajustado:" + angulo);

            }
            AddMensagem("\nComprimento:" + comp );
        }
        public List<RMA> GetRMAsBlocos()
        {
            List<RMA> retorno = new List<RMA>();
            List<BlockReference> blocos = new List<BlockReference>();
            blocos.AddRange(this.Getpassarelas());
            blocos.AddRange(this.GetLinhasDeVida());
            blocos = blocos.GroupBy(x =>
            Math.Round(x.Position.X) + ";" +
            Math.Round(x.Position.Y) + ";" +
            Math.Round(x.Position.Z))
            .Select(x => x.First()).ToList();
            var atributos = blocos
            
            .Select(x => Atributos.GetValor(x, Cfg.Init.CAD_ATT_Cod_SAP).ToString()).Distinct().ToList();

            atributos = atributos.Distinct().ToList();
            if (blocos.Count>0)
            {
              foreach(var codigo in atributos)
                {
                    var pass = blocos.FindAll(x => Atributos.GetValor(x, Cfg.Init.CAD_ATT_Cod_SAP).ToString() == codigo).ToList();
                    retorno.Add(GetRMA(codigo, (double)pass.Count));
                    AddMensagem("\n " + codigo + " - " +  pass.Count + " x");
                }

            }




            var fixacoes = Getcabos().Sum(x=>x.NumberOfVertices);
            AddMensagem("\n Fixações: " + fixacoes);
            if (Getcabos().Count > 0)
            {
                string codigo = this.Codigo_Cabo;
                double qtd = Math.Round(Getcabos().Sum(x => x.Length)/1000);
                qtd = qtd + Math.Round(fixacoes * SomarCaboDobraSFLH/1000);
                retorno.Add(GetRMA(codigo, qtd));

                AddMensagem("\n Total Cabos: " + qtd + " m");
            }

            if (fixacoes>0)
            {
                var metade = Utilz.Double(fixacoes / 2, 0);
                retorno.Add(GetRMA(this.Codigo_Esticador, metade));
                retorno.Add(GetRMA(this.Codigo_Manilha, metade));
                retorno.Add(GetRMA(this.Codigo_Sapatilha, fixacoes));
                retorno.Add(GetRMA(this.Codigo_Clip_Pesado, fixacoes * this.Qtd_Clip_Pesado));
                double total_fita = Math.Ceiling(fixacoes * this.QtdFita_Por_Emenda / this.Fita_Rendimento);
                retorno.Add(GetRMA(this.Codigo_Fita_Isolante, total_fita));
                AddMensagem("\n Total emendas: " + fixacoes );

            }

            return retorno;
        }
        private RMA GetRMA(string codigo, double qtd)
        {
            var pc = DBases.GetBancoRM().GetRMA(codigo);
            if (pc == null)
            {
                pc = new RMA();
                pc.DESC = codigo != "" ? "Código Não encontrado." : "Código em branco";
                pc.SAP = codigo;
            }
            if (pc != null)
            {
                pc = new RMA(pc);
                pc.Quantidade = qtd;
               
            }
            else
            {
                pc = new RMA();
                pc.DESC = codigo != "" ? "Código Não encontrado." : "Código em branco";
                pc.SAP = codigo;
                pc.Quantidade = qtd;
            }
            AddMensagem("\n" + codigo + " - " + pc.DESC + " Qtd>" + qtd);
            return pc;
        }
        public CADTelhas()
        {

        }
    }
}
