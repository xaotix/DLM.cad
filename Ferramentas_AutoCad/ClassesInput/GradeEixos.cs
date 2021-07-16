﻿using Autodesk.AutoCAD.DatabaseServices;
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
                        VaoObra pp = new VaoObra(verticais[i - 1], verticais[i], this.CADPurlin, tipo);
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


        public System.Windows.Shapes.Line GetLinha(Line Linha, System.Windows.Point p0, double escala, double espessura)
        {
            /*linha do eixo*/
            var p1 = new Point((Linha.StartPoint.X - p0.X) * escala, (Linha.StartPoint.Y - p0.Y) * escala);
            var p2 = new Point((Linha.EndPoint.X - p0.X) * escala, (Linha.EndPoint.Y - p0.Y) * escala);
          var cor =  Conexoes.Utilz.ColorToBrush(System.Drawing.Color.FromArgb(Linha.Color.Red, Linha.Color.Green, Linha.Color.Blue));
            cor.Opacity = 0.5;
            var l = Conexoes.FuncoesCanvas.Linha(p1, p2, cor, 0, Conexoes.FuncoesCanvas.TipoLinha.Continua, espessura);


            return l;
        }
        public List<UIElement> GetCanvasVertical(System.Windows.Controls.Canvas canvas)
        {
            canvas.Children.Clear();
            List<UIElement> retorno = new List<UIElement>();
            double escala = 1;
            double tam_texto = 10;

            double raio = tam_texto*2;
            double esc_y = 750 / (Altura + (2*raio));
            double esc_x = 1500 / Largura;

            escala = esc_x > esc_y ? esc_x : esc_y;
            //escala = esc_x;

            double espessura = 1;

            double offset = 10/escala;


            var ls = this.CADPurlin.GetEixos_Linhas();
            var lss = this.CADPurlin.Getlinhas().FindAll(x=> ls.Find(y=> y.Id == x.Id) == null);



            

            Point p0 = new Point(Xmin + offset, Ymin + offset);

            //foreach (var l in lss)
            //{
            //    var ln = GetLinha(l, p0, escala, espessura);
            //    retorno.Add(ln);
            //}

            var eixos = GetEixosVerticais();
            if(eixos.Count>1)
            {
                
                retorno.AddRange(this.GetEixosVerticais().SelectMany(x => x.GetCanvas(p0, escala, espessura,raio,tam_texto*1.5)));
                retorno.AddRange(this.CADPurlin.GetMultLinesCorrentes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Green)));
                retorno.AddRange(this.CADPurlin.GetMultLinesTirantes().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.White)));
                retorno.AddRange(this.CADPurlin.GetMultLinePurlins().Select(x => x.GetCanvas(p0, escala, espessura, Conexoes.FuncoesCanvas.Cores.Yellow)));
            }


            retorno.AddRange(this.GetVaosVerticais().SelectMany(x => x.GetCanvas(p0,escala,tam_texto)));

            foreach(var c in retorno)
            {
                canvas.Children.Add(c);
            }


            canvas.Width = Math.Round(this.Largura * escala) + (offset*2);
            canvas.Height = Math.Round(this.Altura * escala) + (offset*2);

            return retorno;
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
