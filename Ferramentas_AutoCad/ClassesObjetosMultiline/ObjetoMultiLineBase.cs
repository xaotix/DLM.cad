using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DLM.cad
{
    public class ObjetoMultiLineBase : Notificar
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

        [Category("Geometria")]
        [DisplayName("Ângulo")]
        public double Angulo
        {
            get
            {
                return this.Origem_Esquerda.GetAngulo(this.Origem_Direita);
            }
        }

        [Browsable(false)]
        public P3d Origem_Esquerda { get; set; } = new P3d();
        [Browsable(false)]
        public P3d Origem_Direita { get;  set; } = new P3d();
        [Browsable(false)]
        public P3d P1
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
                            return this.Origem_Esquerda.Mover(this.Angulo, -p.TRE);
                        }
                    case Tipo_ObjetoBase.Corrente:
                        var c = this as ObjetoCorrente;
                        return this.Origem_Esquerda.Mover(this.Angulo, c.Descontar);
                    case Tipo_ObjetoBase.Tirante:
                        var t = this as ObjetoTirante;
                        return this.Origem_Esquerda.Mover(this.Angulo, -t.Offset);
                }
                return new P3d();
            }
        }
        [Browsable(false)]
        public P3d P2
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
                            return this.Origem_Direita.Mover(this.Angulo, p.TRD);
                        }
                    case Tipo_ObjetoBase.Corrente:
                        var c = this as ObjetoCorrente;
                        return this.Origem_Direita.Mover(this.Angulo, -c.Descontar);
                    case Tipo_ObjetoBase.Tirante:
                        var t = this as ObjetoTirante;
                        return this.Origem_Direita.Mover(this.Angulo, t.Offset);
                }
                return new P3d();
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
            var P1 = this.P1;
            var P2 = this.P2;

            P1 = new P3d((P1.X - this.Grade.P0.X) * this.Grade.Escala, (P1.Y - this.Grade.P0.Y) * this.Grade.Escala);
            P2 = new P3d((P2.X - this.Grade.P0.X) * this.Grade.Escala, (P2.Y - this.Grade.P0.Y) * this.Grade.Escala);
            _linha = P1.Linha(P2, this.GetCor().Clone(), Core.GetCADPurlin().Canvas_Espessura_Multiline);
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

            _botao = (this.Nome + $"\n#{this.Espessura.String(2)}").Botao(pt, this.GetCor().Clone(), Core.GetCADPurlin().Canvas_Tam_Texto, angulo, 1, System.Windows.Media.Brushes.Black);

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
            if(this.Propriedades())
            {
                Redraw();
            }
        }

        public void Redraw()
        {
            var pcs = this.GetCanvas();
            foreach (var pc in pcs)
            {
                Core.GetCADPurlin().GetGrade().Canvas.Children.Add(pc);
            }
        }

        public void Evento_Sair(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _linha.SetCor(this.GetCor(), System.Windows.Media.Brushes.Black);
            _botao.Visibility = Visibility.Collapsed;
            _linha.StrokeThickness = Core.GetCADPurlin().Canvas_Espessura_Multiline;
            _botao.SetCor(this.GetCor(), System.Windows.Media.Brushes.Black);
            _linha.TrazerPraFrente();
            _botao.TrazerPraFrente();
        }

        public void Evento_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = sender as UIElement;

            _botao.Visibility = Visibility.Visible;
            _linha.StrokeThickness = Core.GetCADPurlin().Canvas_Espessura_Multiline * 4;
            _linha.TrazerPraFrente();
            _botao.TrazerPraFrente();

        }

        public string Letra
        {
            get
            {
                return this.id.getLetra();
            }
        }


        private SolidColorBrush _Cor { get; set; }
        public SolidColorBrush GetCor()
        {

            if(_Cor==null)
            {
                if (this.Tipo == Tipo_ObjetoBase.Purlin)
                {
                    _Cor = System.Windows.Media.Brushes.Yellow.Clone();
                }
                else if (this.Tipo == Tipo_ObjetoBase.Corrente)
                {
                    _Cor = System.Windows.Media.Brushes.Red.Clone();
                }
                else if (this.Tipo == Tipo_ObjetoBase.Tirante)
                {
                    _Cor = System.Windows.Media.Brushes.White.Clone();
                }
                else
                {
                    return System.Windows.Media.Brushes.White.Clone();
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
            this.id_peca = rm.id_codigo;

            this._pecaRME = rm;

            this.NotifyPropertyChanged(nameof(Nome));

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
        public P3d CentroBloco
        {
            get
            {
                return this.P1.Centro(this.P2);
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
