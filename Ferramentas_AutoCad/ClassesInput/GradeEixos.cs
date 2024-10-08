﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static DLM.cad.CAD;

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
            return _eixos.FindAll(x => x.Sentido == Sentido.Vertical);
        }
        public List<ObjetoPurlin> GetPurlinsSemVao()
        {
            if (_purlins_sem_vao == null)
            {

                _purlins_sem_vao = new List<ObjetoPurlin>();
                foreach (var s in Core.GetCADPurlin().GetMLPurlins().FindAll(x => !x.Mapeado && x.Comprimento >= Core.GetCADPurlin().PurlinCompMin))
                {
                    var vao = _vaos_verticais.Find(x => x.Esquerda.MinX >= s.Centro.X && x.Direita.MaxX <= s.Centro.X);

                    if (vao == null && _vaos_verticais.Count > 0)
                    {
                        vao = _vaos_verticais[0];
                    }

                    ObjetoPurlin pp = new ObjetoPurlin(s, this);
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
            if (_vaos_verticais == null | update)
            {
                _vaos_verticais = new List<VaoObra>();
                var verticais = GetEixosVerticais();
                if (verticais.Count > 1)
                {
                    for (int i = 1; i < verticais.Count; i++)
                    {
                        Tipo_Vao tipo = Tipo_Vao.Intermediario;
                        if (i == 1)
                        {
                            tipo = Tipo_Vao.Borda_Esquerdo;
                        }
                        else if (i == verticais.Count - 1)
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

                        if (i > 0)
                        {
                            _vaos_verticais[i].PurlinsDummy[a].PurlinEsquerda = _vaos_verticais[i - 1].PurlinsDummy.Find(x => x.Y == y);
                        }

                        if (i < _vaos_verticais.Count - 1)
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

        public void AddEixo(Sentido Sentido, double Vao, BlockAttributes bloco, CADLine line)
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


            double raio = Core.GetCADPurlin().Canvas_Tam_Texto * 2;
            double esc_y = Core.GetCADPurlin().Canvas_Altura / (GetAltura() + (2 * raio));

            double esc_x = Core.GetCADPurlin().Canvas_Largura / GetComprimento();

            Escala = esc_x > esc_y ? esc_x : esc_y;
            //escala = esc_x;

            double espessura = Core.GetCADPurlin().Canvas_Esp_Linha;

            var tam_txt_cotas = Core.GetCADPurlin().Canvas_Txt_Cotas * Core.GetCADPurlin().Canvas_Tam_Texto;

            P0 = new Point(GetXmin() - Core.GetCADPurlin().Canvas_Offset, GetYmin() - Core.GetCADPurlin().Canvas_Offset);

            List<UIElement> objetos = new List<UIElement>();

            // Start a transaction
            using (Transaction acTrans = CAD.acCurDb.acTrans())
            {
                var objs = Core.GetCADPurlin().GetObjetosNaoMapeados();
                foreach (var p in objs)
                {
                    objetos.AddRange(Ut.GetCanvas(p, P0, Escala, acTrans, 0.5));
                }

            }

            foreach (var obj in objetos)
            {
                obj.SetCor(System.Windows.Media.Brushes.DarkGray);
            }

            retorno.AddRange(objetos);

            var eixos = GetEixosVerticais();
            if (eixos.Count > 1)
            {
                /*linhas de eixo*/
                var y = this.GetYmax();
                var y2 = this.GetYmin();
                foreach (var eixo in this.GetEixosVerticais())
                {
                    var x = eixo.MinX;
                    /*linha do eixo*/
                    var p1 = new P3d((x - P0.X) * Escala, (y - P0.Y) * Escala);
                    var p2 = new P3d((x - P0.X) * Escala, (y2 - P0.Y) * Escala);
                    var l = p1.Linha(p2, System.Windows.Media.Brushes.Magenta, espessura, DLM.vars.TipoLinhaCanvas.Traco_Ponto);
                    retorno.Add(l);

                    /*bolota do eixo*/
                    var centro_circulo = new P3d((x - P0.X) * Escala, (y - P0.Y + raio) * Escala);
                    var c = centro_circulo.Circulo(raio, espessura, System.Windows.Media.Brushes.Red);
                    retorno.Add(c);

                    var ptexto = eixo.Nome.Label(centro_circulo, System.Windows.Media.Brushes.Cyan, Core.GetCADPurlin().Canvas_Tam_Texto);
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
                if (this._vaos_verticais.Count > 0)
                {
                    if (this._vaos_verticais.First().GetPurlins().Count > 0)
                    {
                        var pps = this._vaos_verticais.First().GetPurlins();
                        for (int i = 0; i < pps.Count; i++)
                        {
                            var pp = pps[i];
                            if (pp.DistBaixo > 0)
                            {
                                var pt = new System.Windows.Point((this.GetXmin() - P0.X - Core.GetCADPurlin().Canvas_Offset) * Escala, (pp.CentroBloco.Y - P0.Y - (pp.DistBaixo / 2)) * Escala);
                                var cota = pp.DistBaixo.String(0).Botao(pt, System.Windows.Media.Brushes.Cyan, tam_txt_cotas, 90);
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
                                var cota = pp.DistBaixo.String(0).Botao(pt, System.Windows.Media.Brushes.Cyan, tam_txt_cotas, 90);
                                cota.MouseEnter += evento_Botao_Sobre;
                                cota.MouseLeave += evento_Botao_Sai;
                                retorno.Add(cota);
                            }
                        }
                    }
                }
            }


            var niveis = Core.GetCADPurlin().GetAtributosNivel().OrderBy(x => x.GetCoordenada().Y).ToList();

            /*insere o nível*/
            if (niveis.Count > 0)
            {

                var linha = 
                    new P3d((this.GetXmin() - P0.X) * Escala, (GetNivel() - P0.Y) * Escala).Linha(
                    new P3d((this.GetXMax() - P0.X) * Escala, (GetNivel() - P0.Y) * Escala),
                    System.Windows.Media.Brushes.Blue);

                retorno.Add(linha);
            }







            foreach (var c in retorno)
            {
                Canvas.Children.Add(c);
            }


            Canvas.Width = Math.Round(this.GetComprimento() * Escala) + (Core.GetCADPurlin().Canvas_Offset * 2 * Escala);
            Canvas.Height = Math.Round(this.GetAltura() * Escala) + (Core.GetCADPurlin().Canvas_Offset * 2 * Escala);

            return retorno;
        }

        public double GetNivel()
        {
            var niveis = Core.GetCADPurlin().GetAtributosNivel().OrderBy(x => x.GetCoordenada().Y).ToList();
            if (niveis.Count > 0)
            {
                var nivel = niveis.Last().GetCoordenada().Origem.GetPoint3D();
                return nivel.Y;
            }

            return GetYmin();
        }

        private void evento_purlin_edita_TRD(object sender, RoutedEventArgs e)
        {
            var sd = sender as System.Windows.Controls.Button;
            var pp = sd.Tag as ObjetoPurlin;
            var ntranspasse = pp.TRD.Prompt("Digite o Transpasse Direito",0);
            if (ntranspasse!=null)
            {
                pp.TRD = ntranspasse.Value;
                var purlin_prox = pp.PurlinDireita;
                if (purlin_prox != null)
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
            var ntranspasse = pp.TRE.Prompt("Digite o Transpasse Esquerdo", 0);
            if (ntranspasse!=null)
            {
                pp.TRE = ntranspasse.Value;
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
            (sender as UIElement).SetCor(System.Windows.Media.Brushes.Cyan, System.Windows.Media.Brushes.Black);
        }

        public void evento_Botao_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
           (sender as UIElement).SetCor(System.Windows.Media.Brushes.Black, System.Windows.Media.Brushes.Cyan);
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
