using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Conexoes;
using Ferramentas_DLM.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ferramentas_DLM
{
    [Serializable]
    public class Telhas : ClasseBase
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
            return this.Getblocos().FindAll(x => x.Name == Conexoes.Utilz.getNome(Vars.Pecas.PASSARELA));
        }

        public List<Entity> Getcotaslinhadevida()
        {
            return this.Getcotas().FindAll(x => x.Layer == LayerLinhaDeVidaCotas | x.Layer == LayerLinhaDeVida);
        }

        public List<Entity> GetCotasPassarelas()
        {
            return this.Getcotas().FindAll(x => x.Layer == LayerPassarela | x.Layer == LayerLinhaDeVida);
        }

        public List<Polyline> Getcabos()
        {
            return this.Getpolylinhas().FindAll(x => x.Layer == LayerLinhaDeVida);
        }
        public void ApagarLinhaDeVida()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos(acTrans);
                foreach(var p in this.Getsflhs())
                {
                    p.Erase(true);
                }
                foreach (var p in this.Getsflis())
                {
                    p.Erase(true);
                }

                foreach(var p in this.Getcabos())
                {
                    p.Erase(true);
                }
                foreach(var p in this.Getblocostexto().FindAll(x=>x.Layer == LayerLinhaDeVida))
                {
                    p.Erase(true);
                }

                foreach (var p in this.Getcotas().FindAll(x => x.Layer == LayerLinhaDeVida | x.Layer == LayerLinhaDeVidaCotas))
                {
                    p.Erase(true);
                }


                acTrans.Commit();   
                acDoc.Editor.Regen();
            }
        }
        public void ApagarPassarelas()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos(acTrans);
                foreach (var p in this.Getpassarelas())
                {
                    p.Erase(true);
                }
                foreach (var p in this.Getcotas().FindAll(x => x.Layer == LayerPassarela | x.Layer == LayerPassarelaCotas))
                {
                    p.Erase(true);
                }
                acTrans.Commit();
                acDoc.Editor.Regen();
            }
        }

        public List<BlockReference> Getsflhs()
        {
            return this.Getblocos().FindAll(x => x.Name == Conexoes.Utilz.getNome(Vars.Pecas.SFLH));
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
            return this.Getblocos().FindAll(x => x.Name == Conexoes.Utilz.getNome(Vars.Pecas.SFLI));
        }

        public void AlinharLinhaDeVida()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos(acTrans);
               

                

                var xls = Getxlines();

                var sflis = Getsflis();
                var sflhs = Getsflhs();
                var textos = Getblocostexto();

                var cabos = Getcabos();
                var cabos_verticais = GetPolylinesVerticais(cabos);
                var cabos_horizontais = GetPolylinesHorizontais(cabos);

                var cotas = Getcotaslinhadevida();
                var cotas_verticais = Utilidades.CotasVerticais(cotas);
                var cotas_horizontais = Utilidades.CotasHorizontais(cotas);

                var etts = new List<Entity>();
                etts.AddRange(sflis);
                etts.AddRange(sflhs);
                etts.AddRange(cabos);

                Point3d ie, id, se, sd;


                Utilidades.GetBordas(etts, out se, out sd, out ie, out id);
                double maxY = (sd.Y > se.Y ? sd.Y : se.Y) + ToleranciaY;
                double minY = (sd.Y < se.Y ? sd.Y : se.Y) - ToleranciaY;

                //filtra as xls que estão dentro ou próximas dos objetos
                AddMensagem("\nXlines Encontradas:" + xls.Count);

                xls = Utilidades.XLinesHorizontais(xls);
                AddMensagem("\nXlines Horizontais Encontradas:" + xls.Count);




                var pcs = new List<BlockReference>();
                pcs.AddRange(sflhs);
                pcs.AddRange(sflis);
                Utils.SetUndoMark(true);
                foreach (var o in pcs)
                {
                    var p0 = new Coordenada(o.Position);
                    var xlss = Utilidades.GetXlinesProximas(o, xls, ToleranciaY);
                    var xl = Utilidades.GetXlineMaisProxima(o, xls, ToleranciaY);
                    if(xl == null) {
                        AddMensagem("\nNenhuma Xline encontrada. (SFLH) " + o.Position);
                        continue; }
                    var pos = new Point3d(o.Position.X, xl.BasePoint.Y, 0);
                    MoverBloco(o, pos, acTrans);

                    var polis = Utilidades.GetPolylinesProximas(cabos, p0.GetPoint(), 100);
                      
                    foreach(var p in polis)
                    {
                        var d1 = new Coordenada(p.StartPoint).Distancia(p0);
                        var d2 = new Coordenada(p.EndPoint).Distancia(p0);
                        Point3d pp1 = p.StartPoint;
                        Point3d pp2 = p.EndPoint;

                        var ang = Angulo(pp1, pp2);
                        if(ang<0)
                        {
                            ang = 360 - ang;
                        }

                        if(ang == 0 | ang == 180)
                        {
                            pp1 = new Point3d(pp1.X, pos.Y, 0);
                            pp2 = new Point3d(pp2.X, pos.Y, 0);
                        }
                        else if(ang == 90 | ang == 270)
                        {
                            pp1 = new Point3d(pos.X, pp1.Y, 0);
                            pp2 = new Point3d(pos.X, pp2.Y, 0);
                        }
                        
                        p.Erase(true);
                        AddPolyLine(new List<Coordenada> { new Coordenada(pp1), new Coordenada(pp2) }, this.EspessuraCabo, this.EspessuraCabo, System.Drawing.Color.Red);
                    }

                    var ccotas = Utilidades.GetCotasProximas(Getcotaslinhadevida().FindAll(x=>x is RotatedDimension).Select(x=> x as RotatedDimension).ToList(), p0.GetPoint(), 100);

                    foreach (var p in ccotas)
                    {
                        var d1 = new Coordenada(p.XLine1Point).Distancia(p0);
                        var d2 = new Coordenada(p.XLine2Point).Distancia(p0);
                        if (d1 < d2)
                        {
                            p.XLine1Point = pos;
                        }
                        else
                        {
                            p.XLine2Point = pos;
                        }
                    }


                    var p1 = new Coordenada(pos);
                    var angulo = p0.Angulo(p1);
                    var dist = p0.Distancia(p1);
                    var blocos_texto = Utilidades.GetBlocosProximos(textos, pos, Getescala() * 20);
                    foreach(var p in blocos_texto)
                    {
                        var p_texto = new Coordenada(p.Position);
                        var pxx = p_texto.Mover(angulo, dist);
                        MoverBloco(p,pxx.GetPoint(),acTrans);
                    }
                }
                Utils.SetUndoMark(false);

                acTrans.Commit();
                acDoc.Editor.Regen();
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

                Point3d p1 = new Point3d();
                if (selecionar)
                {
                    SelecionarObjetos(acTrans);
                    if(this.Getpassarelas().Count>0)
                    {
                        var p = this.Getpassarelas()[0];
                        p1 = p.Position;
                        p.Erase(true);

                    }
                    else
                    {
                        AddMensagem("Nenhuma Passarela selecionada.");
                        return;
                    }
                }
                else
                {
                    p1 = Utilidades.PedirPonto3D("Selecione o ponto inicial", out cancelado);
                }

                int sequencia = 0;
                if(!cancelado)
                {
                denovo:
                    Point3d p0 = new Point3d(p1.X, p1.Y, p1.Z);
                    Utilidades.SetOrtho(true);
                    var p2 = Utilidades.PedirPonto3D("Selecione o ponto final",p1, out cancelado);
                    if(!cancelado)
                    {

                        if (sequencia ==0 && !selecionar)
                        {
                            p1 = new Coordenada(p1).Mover(angulo, this.LarguraTelha/2).GetPoint();
                            angulo = new Coordenada(p1).Angulo(p2);
                            var tmpang = Utilidades.NormalizarAngulo(angulo);
                            if (tmpang == 90 | tmpang == 270)
                            {
                                p1 = new Coordenada(p1).Mover(tmpang, vert / 2).GetPoint();
                            }
                        }
                        angulo = new Coordenada(p1).Angulo(p2);

                        comp = new Coordenada(p1).Distancia(p2);

                        Ajustar(ref angulo, ref comp, p1, ref p2);
                        

                        if (Math.Abs(comp) > this.LarguraTelha)
                        {
                            Utilidades.CriarLayer(LayerPassarela, System.Drawing.Color.White);

                            var pcs = Conexoes.Utilz.ArredondarMultiplo(Math.Abs(comp), this.LarguraTelha);
                            int qtd = Conexoes.Utilz.Int(pcs / this.LarguraTelha);
                            Utils.SetUndoMark(true);
                            for (int i = 0; i < qtd; i++)
                            {
                                Hashtable t = new Hashtable();
                                t.Add("SAP", this.Codigo_Passarela);
                                Utilidades.InserirBloco(acDoc, Vars.Pecas.PASSARELA, p1, 1, 0, t);
                                if(angulo==90 | angulo == 270)
                                {
                                    mov = vert;
                                }
                                else
                                {
                                    mov = this.LarguraTelha;
                                }

                               var p3 = new Coordenada(p1).Mover(angulo, mov).GetPoint();


                                p1 = p3;
                                
                            }
                            if (AdicionarCotas)
                            {
                                Utilidades.CriarLayer(LayerPassarelaCotas, System.Drawing.Color.White);
                                var d1 = new Coordenada(p0);
                                if(sequencia>0)
                                {
                                    d1 = d1.Mover(angulo, -mov / 2);
                                }
                                else if(selecionar)
                                {
                                    d1 = d1.Mover(angulo, mov / 2);
                                    qtd = qtd - 1;
                                }

                                var d2 = new Coordenada(p1).Mover(angulo, -mov/2);
                                var ce = d1.GetCentro(d2);
                                var dist = Math.Round(d1.Distancia(d2));
                                if (angulo == 90 | angulo == 270)
                                {
                                    AddCotaVertical(d1, d2, dist + " (" + qtd + "x)", ce.Mover(angulo - 90, Getescala() * MultiplicadorEscala).GetPoint(), false, 0, false, false);
                                }
                                else
                                {
                                    AddCotaHorizontal(d1, d2, dist + " (" + qtd + "x)", ce.Mover(angulo - 90, Getescala() * MultiplicadorEscala).GetPoint(), false, 0, false, false);

                                }
                            }
                            sequencia++;
                            Utils.SetUndoMark(false);

                            
                        }
                        
                        goto denovo;
                    }
                    

                }
                acTrans.Commit();
                acDoc.Editor.Regen();
            }
        }


        public void InserirLinhaDeVida(bool selecionar = false)
        {

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var layer_atual = Utilidades.GetLayerAtual();

                Utilidades.CriarLayer(LayerLinhaDeVida, System.Drawing.Color.Red);

                //var selecao = SelecionarObjetos(acTrans);

   

                double angulo = 0;
                double comp = 0;

                double mov = this.LarguraTelha;


                bool cancelado = false;

                Point3d p1 = new Point3d();
                if (selecionar)
                {
                    SelecionarObjetos(acTrans);
                    if (this.Getsflhs().Count > 0)
                    {
                        var p = this.Getsflhs()[0];
                        p1 = p.Position;
                        p.Erase(true);

                    }
                    else
                    {
                        AddMensagem("Nenhum SFL-H selecionado.");
                        return;
                    }
                }
                else
                {
                    p1 = Utilidades.PedirPonto3D("Selecione o ponto inicial", out cancelado);
                }

                int sequencia = 0;
                if (!cancelado)
                {
                denovo:
                    Utilidades.SetOrtho(true);
                    var p2 = Utilidades.PedirPonto3D("Selecione o ponto final", p1, out cancelado);
                    if (!cancelado)
                    {
                        Utilidades.SetLayer(LayerLinhaDeVida);

                        List<Point3d> cotas = new List<Point3d>();
                        if (sequencia == 0 && !selecionar)
                        {
                            p1 = new Coordenada(p1).Mover(angulo, this.LarguraTelha / 2).GetPoint();
                            
                        }
                        cotas.Add(p1);
                        comp = new Coordenada(p1).Distancia(p2);
                        angulo = new Coordenada(p1).Angulo(p2);
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
                                p2 = new Coordenada(p1).Mover(angulo, compf).GetPoint();
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
                                AddPolyLine(new List<Coordenada> { new Coordenada(p1), new Coordenada(p2) }, EspessuraCabo, EspessuraCabo, System.Drawing.Color.Red);
                            }
                            else if(comps.Count>1)
                            {
                                Point3d pxa = new Point3d(p1.X, p1.Y, p1.Z);
                                foreach (var item in comps)
                                {
                                    Point3d pxb = new Coordenada(pxa).Mover(angulo, item).GetPoint();
                                    AddVaoSFLH(angulo,item, pxa, sequencia, pxb, ref cotas);
                                    sequencia++;
                                    cotas.Add(pxb);
                                    //adiciona o cabo
                                    AddPolyLine(new List<Coordenada> { new Coordenada(pxa), new Coordenada(pxb) }, EspessuraCabo, EspessuraCabo, System.Drawing.Color.Red);
                                    pxa = pxb;
                                }
                            }


                            sequencia++;
                            if(comps.Count==1)
                            {
                            cotas.Add(new Point3d(p2.X, p2.Y, p2.Z));
                            }

                            if (AdicionarCotas)
                            {
                                Utilidades.CriarLayer(LayerLinhaDeVidaCotas, System.Drawing.Color.White);
                                for (int i = 0; i < cotas.Count - 1; i++)
                                {
                                    if (angulo == 0 | angulo == 180)
                                    {
                                        AddCotaHorizontal(new Coordenada(cotas[i]), new Coordenada(cotas[i + 1]), "", Mover(Centro(cotas[i], cotas[i + 1]), angulo - 90, Getescala() * 10), false, 0, false, false);
                                    }
                                    else
                                    {
                                        AddCotaVertical(new Coordenada(cotas[i]), new Coordenada(cotas[i + 1]), "", Mover(Centro(cotas[i], cotas[i + 1]), angulo - 90, Getescala() * 10), false, 0, false, false);

                                    }
                                }
                                if (cotas.Count > 1)
                                {
                                    if (angulo == 0 | angulo == 180)
                                    {
                                        AddCotaHorizontal(new Coordenada(cotas.OrderBy(x => x.X).First()), new Coordenada(cotas.OrderBy(x => x.X).Last()), "", Mover(Centro(cotas.OrderBy(x => x.X).First(), cotas.OrderBy(x => x.X).Last()), angulo - 90, Getescala() * MultiplicadorEscala), false, 0, false, false);
                                    }
                                    else
                                    {
                                        AddCotaVertical(new Coordenada(cotas.OrderBy(x => x.Y).First()), new Coordenada(cotas.OrderBy(x => x.Y).Last()), "", Mover(Centro(cotas.OrderBy(x => x.Y).First(), cotas.OrderBy(x => x.Y).Last()), angulo - 90, Getescala() * MultiplicadorEscala), false, 0, false, false);

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
                        Utilidades.SetLayer(layer_atual);
                    }

                }
                acTrans.Commit();
                acDoc.Editor.Regen();
            }
        }
        public void InserirTabela()
        {
            string dest = Conexoes.Utilz.SalvarArquivo("RM");
            if(dest=="")
            {
                return;
            }
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                DBRM_Offline pp = new DBRM_Offline();
                SelecionarObjetos(acTrans);
                pp.RMA.AddRange(this.GetRMAsBlocos());
                if (pp.RMA.Count > 0)
                {
                    Tabelas.DBRM(pp);
                }

                pp.Salvar(dest);
            }
        }
        private void AddVaoSFLH(double angulo, double comp, Point3d p1, int sequencia, Point3d p2,  ref List<Point3d> cotas)
        {
           
            Hashtable sftlh = new Hashtable();
            sftlh.Add("SAP", Codigo_SFLH);
            Hashtable sftli = new Hashtable();
            sftli.Add("SAP", Codigo_SFLI);
            if (sequencia == 0)
            {
     
                Utilidades.InserirBloco(acDoc, Vars.Pecas.SFLH, p1, 1, 0, sftlh);
                AddBlocoTexto(angulo, p1, SFLH, Getescala() * 5, "");
                AddLeader(angulo, p1, "MANILHA\n ESTICADOR", MultiplicadorEscala*.8);

            }
            Utilidades.InserirBloco(acDoc, Vars.Pecas.SFLH, p2, 1, 0, sftlh);
            AddBlocoTexto(angulo, p2, SFLH, Getescala() * 5,"");
            AddLeader(angulo, p2, "MANILHA\n ESTICADOR", this.MultiplicadorEscala * .8);

            int qtd_sfli = Conexoes.Utilz.Int(comp / this.DistMaxSFLI);

            if (qtd_sfli > 0)
            {
                double espacos = Utilz.ArredondarMultiplo(Utilz.Double(comp / qtd_sfli), this.LarguraTelha);
                Point3d pp0 = new Coordenada(p1).Mover(angulo, espacos).GetPoint();
                for (int i = 0; i < qtd_sfli - 1; i++)
                {
                    Utilidades.InserirBloco(acDoc, Vars.Pecas.SFLI, pp0, 1, 0, sftli);

                    AddBlocoTexto(angulo, pp0, SFLI, Getescala() * 5,"");
                    cotas.Add(pp0);
                    pp0 = new Coordenada(pp0).Mover(angulo, espacos).GetPoint();

                }
            }
        }

        private void AddLeader(double angulo, Point3d pp0, string nome, double multiplicador = 7.5)
        {
            AddLeader(pp0, new Coordenada(pp0).Mover(angulo + 45, this.Getescala() * multiplicador).GetPoint(), nome, 2);
        }
        private void AddBlocoTexto(double angulo, Point3d pp0, string nome, double offset, string sap)
        {
            var p1 = new Coordenada(pp0).Mover(angulo + 90, offset).GetPoint();
            var ss = new Hashtable();
            ss.Add("TEXTO", nome);
            ss.Add("SAP", sap);
            if(angulo==90 | angulo == 270)
            {
                //move pro lado quando é vertical
                p1 = new Coordenada(pp0).Mover(angulo + 90, (Getescala() * 16)/2).GetPoint();
            }
            Utilidades.InserirBloco(acDoc, Vars.Texto, p1, Getescala(), 0, ss );

        }

        private void Ajustar(ref double angulo, ref double comp, Point3d p1, ref Point3d p2)
        {
            if (angulo < 0)
            {
                angulo = 360 + angulo;
            }

            //normaliza o ângulo e a coordenada, para evitar lançamentos inclinados
            if (angulo != 0 && angulo != 90 && angulo != 270 && angulo != 180)
            {
                angulo = Utilidades.NormalizarAngulo(angulo);
                comp = 0;
                if (angulo == 0 | angulo == 180)
                {
                    comp = new Coordenada(p1).DistanciaX(p2);
                }
                else
                {
                    comp = new Coordenada(p1).DistanciaY(p2);
                }
                p2 = new Coordenada(p1).Mover(angulo, comp).GetPoint();
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
            
            .Select(x => Utilidades.GetAtributo(x, "SAP").ToString()).Distinct().ToList();

            atributos = atributos.Distinct().ToList();
            if (blocos.Count>0)
            {
              foreach(var codigo in atributos)
                {
                    var pass = blocos.FindAll(x => Utilidades.GetAtributo(x, "SAP").ToString() == codigo).ToList();
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
            var pc = DBases.BancoRM.GetRMA(codigo);
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


        public Telhas()
        {

        }
    }
}
