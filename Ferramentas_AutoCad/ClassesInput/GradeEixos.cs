using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Ferramentas_DLM
{
    public class GradeEixos
    {
        private List<VaoObra> _vaos_verticais { get; set; }
        private List<ObjetoPurlin> _purlins_sem_vao { get; set; }
        private List<Eixo> _eixos { get; set; } = new List<Eixo>();

        public Tipo_Vista Vista { get; private set; } = Tipo_Vista.Planta;
        public override string ToString()
        {
            return $"Grade Eixos";
        }


        public List<Eixo> GetEixosVerticais()
        {
            return _eixos.FindAll(x=>x.Sentido== Sentido.Vertical);
        }

        public List<ObjetoPurlin> GetPurlinsSemVao()
        {
            if(_purlins_sem_vao==null)
            {
                _purlins_sem_vao = new List<ObjetoPurlin>();
                foreach (var s in Core.CADPurlin.GetMLPurlins().FindAll(x => !x.Mapeado && x.Comprimento >= Core.CADPurlin.PurlinCompMin))
                {
                    var vao = GetVaosVerticais().Find(x => x.Esquerda.MinX >= s.Centro.X && x.Direita.MaxX <= s.Centro.X);

                    if(vao==null &&  GetVaosVerticais().Count>0)
                    {
                        vao = GetVaosVerticais()[0];
                    }

                    ObjetoPurlin pp = new ObjetoPurlin(s,this);
                    _purlins_sem_vao.Add(pp);
                    //adiciona as purlins pequenas fora do vão.
                    //essa parte precisa emplementar melhor para mapear furos manuais e correntes.
                    //AddBlocoPurlin("", this.id_terca, Math.Round(s.comprimento), 0, 0, s.centro.GetPoint(), new List<double>(), new List<double>());
                }
            }
            return _purlins_sem_vao;
        }

        public List<VaoObra> GetVaosVerticais(bool update = false)
        {
            if(_vaos_verticais == null | update)
            {
                _vaos_verticais = new List<VaoObra>();
                var verticais = GetEixosVerticais();
                if (verticais.Count > 1)
                {
                    for (int i = 1; i < verticais.Count; i++)
                    {
                        Tipo_Vao tipo = Tipo_Vao.Intermediario;
                        if(i ==1)
                        {
                            tipo = Tipo_Vao.Borda_Esquerdo;
                        }
                        else if(i == verticais.Count-1)
                        {
                            tipo = Tipo_Vao.Borda_Direito;
                        }
                        VaoObra pp = new VaoObra(this, verticais[i - 1], verticais[i], tipo);
                        _vaos_verticais.Add(pp);
                    }
                }
                int c = 0;
                var alturas = this._vaos_verticais.SelectMany(x => x.GetPurlins().Select(z => z.Y)).Distinct().ToList();

               
                foreach (var v in _vaos_verticais)
                {
                    List<ObjetoPurlin> pts = new List<ObjetoPurlin>();
                    foreach (var alt in alturas)
                    {
                        var igual = v.GetPurlins().Find(x => x.Y == alt);
                        if (igual == null)
                        {
                            igual = new ObjetoPurlin(new Point2d(v.CentroX, alt), v);
                        }
                        pts.Add(igual);
                    }

                    v.SetPurlinsDummy(pts);
                }


                /*vincula as purlins procurando pontos nos vãos ao lado com a mesma coordenada Y*/
                for (int i = 0; i < _vaos_verticais.Count; i++)
                {
                    for (int a = 0; a < _vaos_verticais[i].PurlinsDummy.Count; a++)
                    {
                        var y = _vaos_verticais[i].PurlinsDummy[a].Y;

                        if(i>0)
                        {
                            _vaos_verticais[i].PurlinsDummy[a].PurlinEsquerda = _vaos_verticais[i - 1].PurlinsDummy.Find(x => x.Y == y);
                        }

                        if(i<_vaos_verticais.Count-1)
                        {
                            _vaos_verticais[i].PurlinsDummy[a].PurlinDireita = _vaos_verticais[i + 1].PurlinsDummy.Find(x => x.Y == y);
                        }
                    }
                }
                /*mapeia os tirantes e as correntes*/
                for (int i = 0; i < _vaos_verticais.Count; i++)
                {
                    _vaos_verticais[i].GetTirantes();
                    _vaos_verticais[i].GetCorrentes();
                }

    

            }

          

            return _vaos_verticais;
        }
        public List<Eixo> GetEixosHorizontais()
        {
            return _eixos.FindAll(x => x.Sentido == Sentido.Horizontal);
        }

        public void Add(Sentido Sentido, double Vao, BlocoTag bloco, Line line)
        {
            var neixo = new Eixo(Sentido, bloco, line, Vao);
            _eixos.Add(neixo);
        }

        public Point p0 { get; private set; } = new Point();
        public double escala { get; private set; } = 1;

        public System.Windows.Controls.Canvas canvas { get; private set; }

        public List<UIElement> GetCanvasVertical(System.Windows.Controls.Canvas cc)
        {
            this.canvas = cc;
            canvas.Children.Clear();
            List<UIElement> retorno = new List<UIElement>();
            escala = 1;


            double raio = Core.CADPurlin.Canvas_Tam_Texto * 2;
            double esc_y = Core.CADPurlin.Canvas_Altura / (GetAltura() + (2*raio));
      
            double esc_x = Core.CADPurlin.Canvas_Largura / GetComprimento();

            escala = esc_x > esc_y ? esc_x : esc_y;
            //escala = esc_x;

            double espessura = Core.CADPurlin.Canvas_Esp_Linha;

 



            var tam_txt_cotas =  Core.CADPurlin.Canvas_Txt_Cotas * Core.CADPurlin.Canvas_Tam_Texto;

            

            p0 = new Point(GetXmin() + Core.CADPurlin.Canvas_Offset, GetYmin() + Core.CADPurlin.Canvas_Offset);


            List<UIElement> objetos = new List<UIElement>();
            using (DocumentLock docLock = CAD.acDoc.LockDocument())
            {
                // Start a transaction
                using (Transaction acTrans = CAD.acCurDb.TransactionManager.StartTransaction())
                {
                    var objs = Core.CADPurlin.GetObjetosNaoMapeados();
                    foreach (var p in objs)
                    {
                        objetos.AddRange(Ut.GetCanvas(p, p0, escala,acTrans,0.5));
                    }

                }
            }

           foreach(var obj in objetos)
            {
                Conexoes.FuncoesCanvas.SetCor(obj, Conexoes.FuncoesCanvas.Cores.DarkGray);
            }

            retorno.AddRange(objetos);

            var eixos = GetEixosVerticais();
            if(eixos.Count>1)
            {
                /*linhas de eixo*/
                var y = this.GetYmax();
                var y2 = this.GetYmin();
                foreach (var eixo in this.GetEixosVerticais())
                {
                    var x = eixo.MinX;
                    /*linha do eixo*/
                    var p1 = new Point((x - p0.X) * escala, (y - p0.Y) * escala);
                    var p2 = new Point((x - p0.X) * escala, (y2 - p0.Y) * escala);
                    var l = Conexoes.FuncoesCanvas.Linha(p1, p2, Conexoes.FuncoesCanvas.Cores.Magenta,espessura, Conexoes.FuncoesCanvas.TipoLinha.Traco_Ponto);
                    retorno.Add(l);

                    /*bolota do eixo*/
                    var centro_circulo = new Point((x - p0.X) * escala, (y - p0.Y + raio) * escala);
                    var c = Conexoes.FuncoesCanvas.Circulo(centro_circulo, raio, espessura, Conexoes.FuncoesCanvas.Cores.Red);
                    retorno.Add(c);

                    var ptexto = Conexoes.FuncoesCanvas.Label(eixo.Nome, centro_circulo, Conexoes.FuncoesCanvas.Cores.Cyan, Core.CADPurlin.Canvas_Tam_Texto);
                    retorno.Add(ptexto);
                }

                /*correntes, tirantes e purlins (somente linhas*/
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinesCorrentes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Green)));
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinesTirantes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.White)));
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinePurlins().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Yellow)));


                /*cotas horizontais 
                 + blocos onde tem os nomes das peças
                 */
                retorno.AddRange(this.GetVaosVerticais().SelectMany(x => x.GetCanvas()));


                /*cotas verticais*/
                if(this.GetVaosVerticais().Count>0)
                {
                    if(this.GetVaosVerticais().First().GetPurlins().Count>0)
                    {
                        var pps = this.GetVaosVerticais().First().GetPurlins();
                        for (int i = 0; i < pps.Count; i++)
                        {
                            var pp = pps[i];
                            if(pp.DistBaixo>0)
                            {
                                var pt = new System.Windows.Point((this.GetXmin() - p0.X) * escala, (pp.CentroBloco.Y- p0.Y - (pp.DistBaixo/2)) * escala);
                                var cota = Conexoes.FuncoesCanvas.Botao(pp.DistBaixo.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                cota.ToolTip = pp;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
                if (this.GetVaosVerticais().Count > 1)
                {
                    if (this.GetVaosVerticais().Last().GetPurlins().Count > 0)
                    {
                        var pps = this.GetVaosVerticais().Last().GetPurlins();
                        for (int i = 0; i < pps.Count; i++)
                        {
                            var pp = pps[i];
                            if (pp.DistBaixo > 0)
                            {
                                var pt = new System.Windows.Point((this.GetXMax() - p0.X + Core.CADPurlin.Canvas_Offset) * escala, (pp.CentroBloco.Y - p0.Y - (pp.DistBaixo / 2)) * escala);
                                var cota = Conexoes.FuncoesCanvas.Botao(pp.DistBaixo.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
            }


            ///*botão edição transpasse*/
            //for (int i = 0; i < this.GetVaosVerticais().Count; i++)
            //{
            //    var vat = this.GetVaosVerticais()[i];

            //    foreach (var pp in vat.GetPurlins())
            //    {

            //        var pt = new System.Windows.Point((vat.Direita.MaxX - p0.X) * escala, (pp.CentroBloco.Y - p0.Y) * escala);
            //        var cota1 = Conexoes.FuncoesCanvas.Botao(pp.TRD.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan, pp.TRD, 0);
            //        cota1.Tag = pp;
            //        cota1.MouseEnter += evento_Botao_Sobre;
            //        cota1.MouseLeave += evento_Botao_Sai;
            //        cota1.Click += evento_purlin_edita_TRD;
            //        retorno.Add(cota1);

            //        var pt2 = new System.Windows.Point((vat.Esquerda.MinX - p0.X) * escala, (pp.CentroBloco.Y - p0.Y) * escala);
            //        var cota2 = Conexoes.FuncoesCanvas.Botao(pp.TRE.ToString(), pt2, Conexoes.FuncoesCanvas.Cores.Cyan, pp.TRE, 0);
            //        cota2.Tag = pp;
            //        cota2.MouseEnter += evento_Botao_Sobre;
            //        cota2.MouseLeave += evento_Botao_Sai;
            //        cota2.Click += evento_purlin_edita_TRE;

            //        retorno.Add(cota2);
            //    }
            //}


            var niveis = Core.CADPurlin.GetBlocos_Nivel().OrderBy(x=>x.GetCoordenada().Y).ToList();

            /*insere o nível*/
            if(niveis.Count>0)
            {
                
                var linha = Conexoes.FuncoesCanvas.Linha(
                    new Point((this.GetXmin() - p0.X) * escala, (GetNivel() - p0.Y) * escala),
                    new Point((this.GetXMax() - p0.X) * escala, (GetNivel() - p0.Y) * escala), 
                    Conexoes.FuncoesCanvas.Cores.Blue);

                retorno.Add(linha);
            }




             


            foreach (var c in retorno)
            {
                canvas.Children.Add(c);
            }


            canvas.Width = Math.Round(this.GetComprimento() * escala) + (Core.CADPurlin.Canvas_Offset * 2*escala);
            canvas.Height = Math.Round(this.GetAltura() * escala) + (Core.CADPurlin.Canvas_Offset * 2 * escala);

            return retorno;
        }

        public double GetNivel()
        {
            var niveis = Core.CADPurlin.GetBlocos_Nivel().OrderBy(x => x.GetCoordenada().Y).ToList();
            if (niveis.Count > 0)
            {
                var nivel = niveis.Last().GetCoordenada().GetPoint3D();
                return nivel.Y;
            }

            return GetYmin();
        }

        private void evento_purlin_edita_TRD(object sender, RoutedEventArgs e)
        {
            var sd = sender as System.Windows.Controls.Button;
            var pp = sd.Tag as ObjetoPurlin;
            bool confirmado = false;
            double ntranspasse = Conexoes.Utilz.Prompt(pp.TRD, out confirmado, 0);
            if(confirmado)
            {
                pp.TRD = ntranspasse;
                var purlin_prox = pp.PurlinDireita;
                if(purlin_prox!=null)
                {
                    purlin_prox.TRE = pp.TRD;
                }
                sd.Content = ntranspasse;
            }
        }
        private void evento_purlin_edita_TRE(object sender, RoutedEventArgs e)
        {
            var sd = sender as System.Windows.Controls.Button;
            var pp = sd.Tag as ObjetoPurlin;
            bool confirmado = false;
            double ntranspasse = Conexoes.Utilz.Prompt(pp.TRE, out confirmado, 0);
            if (confirmado)
            {
                pp.TRE = ntranspasse;
                var purlin_prox = pp.PurlinEsquerda;
                if (purlin_prox != null)
                {
                    purlin_prox.TRD = pp.TRE;
                }
                sd.Content = ntranspasse;
            }
        }

        public void evento_Botao_Sai(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, Conexoes.FuncoesCanvas.Cores.Cyan, Conexoes.FuncoesCanvas.Cores.Black);
        }

        public void evento_Botao_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, Conexoes.FuncoesCanvas.Cores.Black, Conexoes.FuncoesCanvas.Cores.Cyan);
        }

        public double GetComprimento()
        {
            return Math.Round(Math.Abs(GetXMax() - GetXmin()));
        }

        public double GetAltura()
        {
            return Math.Round(Math.Abs(GetYmax() - GetYmin()));
        }

        public double GetXmin()
        {
            if (this._eixos.Count > 0)
            {
                return this._eixos.Min(x => x.MinX);
            }
            return 0;
        }

        public double GetYmin()
        {
            if (this._eixos.Count > 0)
            {
                return this._eixos.Min(x => x.MinY);
            }
            return 0;
        }

        public double GetYmax()
        {
            if (this._eixos.Count > 0)
            {
                return this._eixos.Max(x => x.MaxY);
            }
            return 0;
        }

        public double GetXMax()
        {
            if (this._eixos.Count > 0)
            {
                return this._eixos.Max(x => x.MaxX);
            }
            return 0;
        }



        public GradeEixos()
        {

        }
    }
}
