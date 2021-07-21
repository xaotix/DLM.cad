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
        public override string ToString()
        {
            return $"Grade Eixos {GetComprimento()}x{GetLargura()}";
        }
        public double GetComprimento()
        {
            if (this.GetEixosHorizontais().Count == 0) { return 0; }
            return this.GetEixosHorizontais().Sum(x => x.Vao);
        }
        public double GetLargura()
        {
            if (this.GetEixosVerticais().Count == 0) { return 0; }
            return this.GetEixosVerticais().Sum(x => x.Vao);
        }
        public Tipo_Vista Vista { get; private set; } = Tipo_Vista.Planta;
        private List<Eixo> _eixos { get; set; } = new List<Eixo>();
        public List<Eixo> GetEixosVerticais()
        {
            return _eixos.FindAll(x=>x.Sentido== Sentido.Vertical);
        }

        private List<VaoObra> _vaos_verticais { get; set; } = null;
       

        private List<ObjetoPurlin> _purlins_sem_vao { get; set; }

        public List<ObjetoPurlin> GetPurlinsSemVao()
        {
            if(_purlins_sem_vao==null)
            {
                _purlins_sem_vao = new List<ObjetoPurlin>();
                foreach (var s in this.CADPurlin.GetMultLinePurlins().FindAll(x => !x.Mapeado && x.comprimento >= this.CADPurlin.PurlinCompMin))
                {
                    var vao = GetVaosVerticais().Find(x => x.Esquerda.Xmin >= s.centro.GetPoint().X && x.Direita.Xmax <= s.centro.X);

                    if(vao==null &&  GetVaosVerticais().Count>0)
                    {
                        vao = GetVaosVerticais()[0];
                    }

                    ObjetoPurlin pp = new ObjetoPurlin(s, this.CADPurlin);
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
                        VaoObra pp = new VaoObra(this, verticais[i - 1], verticais[i], this.CADPurlin, tipo);
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
                            igual = new ObjetoPurlin(new Point3d(v.CentroX, alt, 0), v);
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
        public void Add(Sentido Sentido, string Nome, double Vao)
        {
            if(Nome.Replace(" ","") == "") { return; }
            if(_eixos.Find(x=>x.Nome.ToUpper().Replace(" ","") == Nome.ToUpper().Replace(" ","")) != null) { return; }
            if(Vao>0)
            {
                _eixos.Add(new Eixo(Sentido, Nome, Vao));
            }
        }
        public void Add(Sentido Sentido, double Vao, BlockReference bloco, Line line)
        {
            var neixo = new Eixo(Sentido, bloco, line, Vao);
            _eixos.Add(neixo);
        }

        public List<UIElement> GetCanvasVertical(System.Windows.Controls.Canvas canvas)
        {
            canvas.Children.Clear();
            List<UIElement> retorno = new List<UIElement>();
            double escala = 1;
            double tam_texto = this.CADPurlin.Canvas_Tam_Texto;

            double raio = tam_texto*2;
            double esc_y = this.CADPurlin.Canvas_Altura / (Altura + (2*raio));
      
            double esc_x = this.CADPurlin.Canvas_Largura / Largura;

            escala = esc_x > esc_y ? esc_x : esc_y;
            //escala = esc_x;

            double espessura = this.CADPurlin.Canvas_Esp_Linha;

            double offset = this.CADPurlin.Canvas_Offset/escala;


            var ls = this.CADPurlin.GetLinhas_Eixos();
            var lss = this.CADPurlin.GetLinhas().FindAll(x=> ls.Find(y=> y.Id == x.Id) == null);

            var tam_txt_cotas =  this.CADPurlin.Canvas_Txt_Cotas * tam_texto;

            

            Point p0 = new Point(Xmin + offset, Ymin + offset);

            var eixos = GetEixosVerticais();
            if(eixos.Count>1)
            {
                /*linhas de eixo*/
                retorno.AddRange(this.GetEixosVerticais().SelectMany(x => x.GetCanvas(p0, escala, espessura,raio,tam_texto*1.5)));

                /*correntes, tirantes e purlins (somente linhas*/
                retorno.AddRange(this.CADPurlin.GetMultLinesCorrentes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Green)));
                retorno.AddRange(this.CADPurlin.GetMultLinesTirantes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.White)));
                retorno.AddRange(this.CADPurlin.GetMultLinePurlins().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Yellow)));


                /*cotas horizontais 
                 + blocos onde tem os nomes das peças
                 */
                retorno.AddRange(this.GetVaosVerticais().SelectMany(x => x.GetCanvas(p0, escala, tam_texto)));


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
                                var pt = new System.Windows.Point((this.Xmin - p0.X) * escala, (pp.CentroBloco.Y- p0.Y - (pp.DistBaixo/2)) * escala);
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
                                var pt = new System.Windows.Point((this.XMax - p0.X + offset) * escala, (pp.CentroBloco.Y - p0.Y - (pp.DistBaixo / 2)) * escala);
                                var cota = Conexoes.FuncoesCanvas.Botao(pp.DistBaixo.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
            }


            /*botão edição transpasse*/
            for (int i = 0; i < this.GetVaosVerticais().Count; i++)
            {
                var vat = this.GetVaosVerticais()[i];

                foreach (var pp in vat.GetPurlins())
                {
                   
                 
                    if (i < this.GetVaosVerticais().Count)
                    {
                        var pt = new System.Windows.Point((vat.Direita.Xmax - p0.X + offset) * escala, (pp.CentroBloco.Y - p0.Y) * escala);
                        var cota1 = Conexoes.FuncoesCanvas.Botao(pp.TRD.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan, pp.TRD, 0);
                        cota1.Tag = pp;
                        cota1.MouseEnter += evento_Botao_Sobre;
                        cota1.MouseLeave += evento_Botao_Sai;
                        cota1.Click += evento_purlin_edita_TRD;
                        retorno.Add(cota1);

                        
                    }
                    if (i == 0)
                    {
                        var pt2 = new System.Windows.Point((vat.Esquerda.Xmin - p0.X + offset) * escala, (pp.CentroBloco.Y - p0.Y) * escala);
                        var cota2 = Conexoes.FuncoesCanvas.Botao(pp.TRE.ToString(), pt2, Conexoes.FuncoesCanvas.Cores.Cyan, pp.TRE, 0);
                        cota2.Tag = pp;
                        cota2.MouseEnter += evento_Botao_Sobre;
                        cota2.MouseLeave += evento_Botao_Sai;
                        cota2.Click += evento_purlin_edita_TRE;

                        retorno.Add(cota2);
                    }
                }
            }






            foreach (var c in retorno)
            {
                canvas.Children.Add(c);
            }


            canvas.Width = Math.Round(this.Largura * escala) + (offset*2);
            canvas.Height = Math.Round(this.Altura * escala) + (offset*2);

            return retorno;
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

        public double Largura
        {
            get
            {
                return Math.Round(Math.Abs(XMax - Xmin));
            }
        }
        public double Altura
        {
            get
            {
                return Math.Round(Math.Abs(Ymax - Ymin));
            }
        }
        public double Xmin
        {
            get
            {
                if(this._eixos.Count>0)
                {
                    return this._eixos.Min(x => x.Xmin);
                }
                return 0;
            }
        }
        public double Ymin
        {
            get
            {
                if (this._eixos.Count > 0)
                {
                    return this._eixos.Min(x => x.Ymin);
                }
                return 0;
            }
        }
        public double Ymax
        {
            get
            {
                if (this._eixos.Count > 0)
                {
                    return this._eixos.Max(x => x.Ymax);
                }
                return 0;
            }
        }
        public double XMax
        {
            get
            {
                if (this._eixos.Count > 0)
                {
                    return this._eixos.Max(x => x.Xmax);
                }
                return 0;
            }
        }


        public CADPurlin CADPurlin { get; private set; }
        public GradeEixos(CADPurlin cADPurlin)
        {
            this.CADPurlin = cADPurlin;
        }
    }
}
