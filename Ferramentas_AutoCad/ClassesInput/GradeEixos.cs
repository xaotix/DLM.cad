using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DLM.cad
{
    public class GradeEixos
    {
        private List<VaoObra> _vaos_verticais { get; set; }
        private List<ObjetoPurlin> _purlins_sem_vao { get; set; }
        private List<Eixo> _eixos { get; set; } = new List<Eixo>();

        public Tipo_Vista Vista { get; private set; } = Tipo_Vista.Planta;
        public Point P0 { get; private set; } = new Point();
        public double Escala { get; private set; } = 1;
        public System.Windows.Controls.Canvas Canvas { get; private set; }

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
                    var vao = _vaos_verticais.Find(x => x.Esquerda.MinX >= s.Centro.X && x.Direita.MaxX <= s.Centro.X);

                    if(vao==null && _vaos_verticais.Count>0)
                    {
                        vao = _vaos_verticais[0];
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

        public void AddEixo(Sentido Sentido, double Vao, BlocoTag bloco, CADLine line)
        {
            var neixo = new Eixo(Sentido, bloco, line, Vao);
            _eixos.Add(neixo);
        }



        public List<UIElement> GetCanvasVertical(System.Windows.Controls.Canvas cc)
        {
            this.GetVaosVerticais();

            if (this._vaos_verticais == null) { return new List<UIElement>(); }
            this.Canvas = cc;
            Canvas.Children.Clear();
            List<UIElement> retorno = new List<UIElement>();
            Escala = 1;


            double raio = Core.CADPurlin.Canvas_Tam_Texto * 2;
            double esc_y = Core.CADPurlin.Canvas_Altura / (GetAltura() + (2*raio));
      
            double esc_x = Core.CADPurlin.Canvas_Largura / GetComprimento();

            Escala = esc_x > esc_y ? esc_x : esc_y;
            //escala = esc_x;

            double espessura = Core.CADPurlin.Canvas_Esp_Linha;

            var tam_txt_cotas =  Core.CADPurlin.Canvas_Txt_Cotas * Core.CADPurlin.Canvas_Tam_Texto;

            P0 = new Point(GetXmin() - Core.CADPurlin.Canvas_Offset, GetYmin() - Core.CADPurlin.Canvas_Offset);

            List<UIElement> objetos = new List<UIElement>();
            using (DocumentLock docLock = CAD.acDoc.LockDocument())
            {
                // Start a transaction
                using (Transaction acTrans = CAD.acCurDb.TransactionManager.StartTransaction())
                {
                    var objs = Core.CADPurlin.GetObjetosNaoMapeados();
                    foreach (var p in objs)
                    {
                        objetos.AddRange(Ut.GetCanvas(p, P0, Escala,acTrans,0.5));
                    }

                }
            }

           foreach(var obj in objetos)
            {
                DLM.desenho.FuncoesCanvas.SetCor(obj, DLM.desenho.FuncoesCanvas.Cores.DarkGray);
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
                    var p1 = new Point((x - P0.X) * Escala, (y - P0.Y) * Escala);
                    var p2 = new Point((x - P0.X) * Escala, (y2 - P0.Y) * Escala);
                    var l = DLM.desenho.FuncoesCanvas.Linha(p1, p2, DLM.desenho.FuncoesCanvas.Cores.Magenta,espessura, DLM.vars.TipoLinhaCanvas.Traco_Ponto);
                    retorno.Add(l);

                    /*bolota do eixo*/
                    var centro_circulo = new Point((x - P0.X) * Escala, (y - P0.Y + raio) * Escala);
                    var c = DLM.desenho.FuncoesCanvas.Circulo(centro_circulo, raio, espessura, DLM.desenho.FuncoesCanvas.Cores.Red);
                    retorno.Add(c);

                    var ptexto = DLM.desenho.FuncoesCanvas.Label(eixo.Nome, centro_circulo, DLM.desenho.FuncoesCanvas.Cores.Cyan, Core.CADPurlin.Canvas_Tam_Texto);
                    retorno.Add(ptexto);
                }

                /*correntes, tirantes e purlins (somente linhas*/
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinesCorrentes().Select(x => x.GetCanvas(p0, escala, espessura, DLM.desenho.FuncoesCanvas.Cores.Green)));
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinesTirantes().Select(x => x.GetCanvas(p0, escala, espessura, DLM.desenho.FuncoesCanvas.Cores.White)));
                //retorno.AddRange(Comandos.CADPurlin.GetMultLinePurlins().Select(x => x.GetCanvas(p0, escala, espessura, DLM.desenho.FuncoesCanvas.Cores.Yellow)));


                /*cotas horizontais 
                 + blocos onde tem os nomes das peças
                 */
                retorno.AddRange(this._vaos_verticais.SelectMany(x => x.GetCanvas()));


                /*cotas verticais*/
                if(this._vaos_verticais.Count>0)
                {
                    if(this._vaos_verticais.First().GetPurlins().Count>0)
                    {
                        var pps = this._vaos_verticais.First().GetPurlins();
                        for (int i = 0; i < pps.Count; i++)
                        {
                            var pp = pps[i];
                            if(pp.DistBaixo>0)
                            {
                                var pt = new System.Windows.Point((this.GetXmin() - P0.X - Core.CADPurlin.Canvas_Offset) * Escala, (pp.CentroBloco.Y- P0.Y - (pp.DistBaixo/2)) * Escala);
                                var cota = DLM.desenho.FuncoesCanvas.Botao(pp.DistBaixo.String(0), pt, DLM.desenho.FuncoesCanvas.Cores.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                cota.ToolTip = pp;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
                if (this._vaos_verticais.Count > 1)
                {
                    if (this._vaos_verticais.Last().GetPurlins().Count > 0)
                    {
                        var pps = this._vaos_verticais.Last().GetPurlins();
                        for (int i = 0; i < pps.Count; i++)
                        {
                            var pp = pps[i];
                            if (pp.DistBaixo > 0)
                            {
                                var pt = new System.Windows.Point((this.GetXMax() - P0.X) * Escala, (pp.CentroBloco.Y - P0.Y - (pp.DistBaixo / 2)) * Escala);
                                var cota = DLM.desenho.FuncoesCanvas.Botao(pp.DistBaixo.String(), pt, DLM.desenho.FuncoesCanvas.Cores.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
            }


            var niveis = Core.CADPurlin.GetBlocos_Nivel().OrderBy(x=>x.GetCoordenada().Y).ToList();

            /*insere o nível*/
            if(niveis.Count>0)
            {
                
                var linha = DLM.desenho.FuncoesCanvas.Linha(
                    new Point((this.GetXmin() - P0.X) * Escala, (GetNivel() - P0.Y) * Escala),
                    new Point((this.GetXMax() - P0.X) * Escala, (GetNivel() - P0.Y) * Escala), 
                    DLM.desenho.FuncoesCanvas.Cores.Blue);

                retorno.Add(linha);
            }




             


            foreach (var c in retorno)
            {
                Canvas.Children.Add(c);
            }


            Canvas.Width = Math.Round(this.GetComprimento() * Escala) + (Core.CADPurlin.Canvas_Offset * 2*Escala);
            Canvas.Height = Math.Round(this.GetAltura() * Escala) + (Core.CADPurlin.Canvas_Offset * 2 * Escala);

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
            DLM.desenho.FuncoesCanvas.SetCor(sender as UIElement, DLM.desenho.FuncoesCanvas.Cores.Cyan, DLM.desenho.FuncoesCanvas.Cores.Black);
        }

        public void evento_Botao_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DLM.desenho.FuncoesCanvas.SetCor(sender as UIElement, DLM.desenho.FuncoesCanvas.Cores.Black, DLM.desenho.FuncoesCanvas.Cores.Cyan);
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
