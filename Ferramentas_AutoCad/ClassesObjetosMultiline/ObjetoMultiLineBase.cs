using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DLM.cad
{
    public class ObjetoMultiLineBase : INotifyPropertyChanged
    {
        [Category("Geometria")]
        public double Comprimento
        {
            get
            {
                if (this.Tipo == Tipo_ObjetoBase.Corrente)
                {
                    var tt = this as ObjetoCorrente;
                    var comp = tt.EntrePurlin;
                    if (comp > 0)
                    {
                        comp = comp - (2 * Math.Abs(tt.Descontar));
                    }
                    return comp;
                }
                else if (this.Tipo == Tipo_ObjetoBase.Purlin)
                {
                    var tt = this as ObjetoPurlin;

                    if (Objeto_Orfao)
                    {
                        return tt.Multiline.Comprimento;
                    }
                    else
                    {
                        return tt.TRE + tt.TRD + tt.Vao;
                    }
                }
                else if (this.Tipo == Tipo_ObjetoBase.Tirante)
                {
                    var tt = this as ObjetoTirante;
                    return this.Multiline.Comprimento + 2 * tt.Offset;
                }
                else if (Objeto_Orfao)
                {
                    return Multiline.Comprimento;
                }
                else return 0;

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        [Category("Geometria")]
        [DisplayName("Ângulo")]
        public double Angulo
        {
            get
            {
                return DLM.cad.Angulo.Get(
                    this.Origem_Esquerda,
                    this.Origem_Direita
                    );
            }
        }

        [Browsable(false)]
        public Point2d Origem_Esquerda { get; set; } = new Point2d();
        [Browsable(false)]
        public Point2d Origem_Direita { get;  set; } = new Point2d();
        [Browsable(false)]
        public Point2d P1
        {
            get
            {
                switch (this.Tipo)
                {
                    case Tipo_ObjetoBase.Purlin:
                        var p = this as ObjetoPurlin;
                        if(p.Objeto_Orfao)
                        {
                            return this.Origem_Esquerda;
                        }
                        else
                        {
                            return Ut.Mover(this.Origem_Esquerda, this.Angulo, -p.TRE);
                        }
                    case Tipo_ObjetoBase.Corrente:
                        var c = this as ObjetoCorrente;
                        return Ut.Mover(this.Origem_Esquerda, this.Angulo, c.Descontar);
                    case Tipo_ObjetoBase.Tirante:
                        var t = this as ObjetoTirante;
                        return Ut.Mover(this.Origem_Esquerda, this.Angulo, -t.Offset);
                }
                return new Point2d();
            }
        }
        [Browsable(false)]
        public Point2d P2
        {
            get
            {
                switch (this.Tipo)
                {
                    case Tipo_ObjetoBase.Purlin:
                        var p = this as ObjetoPurlin;
                        if (p.Objeto_Orfao)
                        {
                            return this.Multiline.Fim;
                        }
                        else
                        {
                            return Ut.Mover(this.Origem_Direita, this.Angulo, p.TRD);
                        }
                    case Tipo_ObjetoBase.Corrente:
                        var c = this as ObjetoCorrente;
                        return Ut.Mover(this.Origem_Direita, this.Angulo, -c.Descontar);
                    case Tipo_ObjetoBase.Tirante:
                        var t = this as ObjetoTirante;
                        return Ut.Mover(this.Origem_Direita, this.Angulo, t.Offset);
                }
                return new Point2d();
            }
        }
        [Browsable(false)]
        public bool Objeto_Orfao
        {
            get
            {
                return this.VaoObra == null;
            }
        }
        [ReadOnly(true)]
        [Category("Fixação")]

        public string Suporte { get;  set; } = "";
        public override string ToString()
        {
            return this.Nome;
        }
        [Category("Geometria")]
        public double Espessura
        {
            get
            {
                if (this._pecaRME != null)
                {
                    return this._pecaRME.ESP;
                }
                return 0;
            }
        }
        private string _Nome { get; set; }
        public string Nome
        {
            get
            {
                _Nome = "";
                if (Tipo == Tipo_ObjetoBase.Corrente)
                {

                    var s = this as ObjetoCorrente;
                    var pc = this.GetPeca();
                    if (pc != null)
                    {
                        _Nome = pc.CODIGOFIM;
                    }
                }
                else if (Tipo == Tipo_ObjetoBase.Tirante)
                {
                    var s = this as ObjetoTirante;
                    var pc = this.GetPeca();
                    if (pc != null)
                    {
                        _Nome = pc.CODIGOFIM;
                    }
                }
                else if (Tipo == Tipo_ObjetoBase.Purlin)
                {
                    var s = this as ObjetoPurlin;
                    _Nome = s.PurlinPadrao;
                    return _Nome;
                }
                else _Nome = "Base";

                _Nome = _Nome;



                return _Nome;
            }
        }
        [Category("Geometria")]
        public Tipo_ObjetoBase Tipo
        {
            get
            {
                if (this is ObjetoPurlin)
                {
                    return Tipo_ObjetoBase.Purlin;
                }
                else if (this is ObjetoTirante)
                {
                    return Tipo_ObjetoBase.Tirante;
                }
                else if (this is ObjetoCorrente)
                {
                    return Tipo_ObjetoBase.Corrente;
                }
                else
                {
                    return Tipo_ObjetoBase.Base;
                }
            }
        }



        private System.Windows.Controls.Button _botao { get; set; }
        private System.Windows.Shapes.Line _linha { get; set; }
        private Conexoes.RMLite _pecaRME { get; set; }


        public List<UIElement> GetCanvas()
        {

            List<UIElement> retorno = new List<UIElement>();

            if (this.Grade.Canvas != null && this._linha != null)
            {
                this.Grade.Canvas.Children.Remove(this._linha);
            }
            if (this.Grade.Canvas != null && this._botao != null)
            {
                this.Grade.Canvas.Children.Remove(this._botao);
            }
            var p1 = Ut.GetWPoint(this.P1);
            var p2 = Ut.GetWPoint(this.P2);

            p1 = new Point((p1.X - this.Grade.P0.X) * this.Grade.Escala, (p1.Y - this.Grade.P0.Y) * this.Grade.Escala);
            p2 = new Point((p2.X - this.Grade.P0.X) * this.Grade.Escala, (p2.Y - this.Grade.P0.Y) * this.Grade.Escala);
            _linha = DLM.desenho.FuncoesCanvas.Linha(p1, p2, this.GetCor().Clone(), Core.CADPurlin.Canvas_Espessura_Multiline);
            _linha.MouseMove += Evento_Sobre;
            _linha.MouseLeave += Evento_Sair;
            _linha.MouseRightButtonUp += Botao_Direito;
            _linha.ToolTip = this;
            

            var pt = new System.Windows.Point((this.CentroBloco.X - this.Grade.P0.X) * this.Grade.Escala, (this.CentroBloco.Y - this.Grade.P0.Y) * this.Grade.Escala);

            double angulo = 0;
            if (this is ObjetoCorrente)
            {
                angulo = 90;
            }

            _botao = DLM.desenho.FuncoesCanvas.Botao(this.Nome + $"\n#{this.Espessura.ToString("N2")}", pt, this.GetCor().Clone(), Core.CADPurlin.Canvas_Tam_Texto, angulo, 1, DLM.desenho.FuncoesCanvas.Cores.Black);

            _botao.MouseRightButtonUp += Botao_Direito;
            _botao.MouseMove += Evento_Sobre;
            _botao.MouseLeave += Evento_Sair;
            _botao.ToolTip = this;
            _botao.Visibility = Visibility.Collapsed;
            _botao.Click += Evento_Clicar;


            retorno.Add(_linha);
            retorno.Add(_botao);

            return retorno;
        }


        public Conexoes.RMLite GetPeca()
        {
            if(_pecaRME==null)
            {
                return null;
            }
            if(this.Comprimento!= _pecaRME.COMP)
            {
                _pecaRME = _pecaRME.Get(this.Comprimento);
            }
            return _pecaRME;
        }

        private void Evento_Clicar(object sender, RoutedEventArgs e)
        {
            if (this is ObjetoCorrente)
            {
                var sel = Ut.SelecionarCorrente();
                if (sel != null)
                {
                    SetPeca(sel);

                    Redraw();
                }
            }
            else if (this is ObjetoPurlin)
            {
                var sel = Ut.SelecionarPurlin(this.GetPeca());
                if (sel != null)
                {
                    SetPeca(sel);

                    Redraw();
                }

            }
            else if (this is ObjetoTirante)
            {
                var sel = Ut.SelecionarTirante();
                if (sel != null)
                {
                    SetPeca(sel);

                    Redraw();
                }
            }

            

        }

        private void Botao_Direito(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool status = false;
            Conexoes.Utilz.Propriedades(this,out status);
            if(status)
            {
                Redraw();
            }
        }

        public void Redraw()
        {
            var pcs = this.GetCanvas();
            foreach (var pc in pcs)
            {
                Core.CADPurlin.GetGrade().Canvas.Children.Add(pc);
            }
        }

        public void Evento_Sair(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DLM.desenho.FuncoesCanvas.SetCor(_linha, this.GetCor(), DLM.desenho.FuncoesCanvas.Cores.Black);
            _botao.Visibility = Visibility.Collapsed;
            _linha.StrokeThickness = Core.CADPurlin.Canvas_Espessura_Multiline;
            DLM.desenho.FuncoesCanvas.SetCor(_botao, this.GetCor(), DLM.desenho.FuncoesCanvas.Cores.Black);
            DLM.desenho.FuncoesCanvas.TrazerPraFrente(_linha);
            DLM.desenho.FuncoesCanvas.TrazerPraFrente(_botao);
        }

        public void Evento_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = sender as UIElement;

            _botao.Visibility = Visibility.Visible;
            _linha.StrokeThickness = Core.CADPurlin.Canvas_Espessura_Multiline * 4;
            DLM.desenho.FuncoesCanvas.TrazerPraFrente(_linha);
            DLM.desenho.FuncoesCanvas.TrazerPraFrente(_botao);

        }

        public string Letra
        {
            get
            {
                return Conexoes.Utilz.getLetra(this.id);
            }
        }


        private SolidColorBrush _Cor { get; set; }
        public SolidColorBrush GetCor()
        {

            if(_Cor==null)
            {
                if (this.Tipo == Tipo_ObjetoBase.Purlin)
                {
                    _Cor = DLM.desenho.FuncoesCanvas.Cores.Yellow.Clone();
                }
                else if (this.Tipo == Tipo_ObjetoBase.Corrente)
                {
                    _Cor = DLM.desenho.FuncoesCanvas.Cores.Red.Clone();
                }
                else if (this.Tipo == Tipo_ObjetoBase.Tirante)
                {
                    _Cor = DLM.desenho.FuncoesCanvas.Cores.White.Clone();
                }
                else
                {
                    return DLM.desenho.FuncoesCanvas.Cores.White.Clone();
                }
            }
            return _Cor;
        }
        [Browsable(false)]
        public static int id_cont { get; set; } = 1;
        [Browsable(false)]
        public int id { get; private set; } = 0;
        [Browsable(false)]
        public double Y
        {
            get
            {
                return Math.Round(this.CentroBloco.Y);
            }
        }



        public void SetPeca(Conexoes.RMLite rm)
        {
            if(rm==null)
            {
                return;
            }
            this.id_peca = rm.id_db;

            this._pecaRME = rm;

            this.NotifyPropertyChanged("Cor");
            this.NotifyPropertyChanged("id");
            this.NotifyPropertyChanged("Nome");

            if (this._botao != null)
            {
                this._botao.Content = this.Letra;
            }

            this._Nome = null;

        }
        [Browsable(false)]
        public int id_peca { get; internal set; } = -1;
        [Browsable(false)]
        public Visibility visivel
        {
            get
            {
                if (Considerar)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }
        [Browsable(false)]
        public Point2d CentroBloco
        {
            get
            {
                return Ut.Centro(this.P1, this.P2);
            }
        }
        [Browsable(false)]
        public CADMline Multiline { get; internal set; }
        [Browsable(false)]
        public bool Considerar { get; internal set; } = true;
        [Browsable(false)]
        public VaoObra VaoObra { get; internal set; }
        [Browsable(false)]
        public ObjetoPurlin PurlinEmBaixo { get; internal set; }
        [Browsable(false)]
        public ObjetoPurlin PurlinEmCima { get; internal set; }

        [Browsable(false)]
        public GradeEixos Grade { get; set; }
        public ObjetoMultiLineBase()
        {
            this.id = ObjetoMultiLineBase.id_cont;
            ObjetoMultiLineBase.id_cont++;
        }
    }
}
