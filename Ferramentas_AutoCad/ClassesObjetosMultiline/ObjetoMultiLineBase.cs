using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Ferramentas_DLM
{
    public class ObjetoMultiLineBase : INotifyPropertyChanged
    {

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
                return Ferramentas_DLM.Angulo.Get(
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
                        return Ut.Mover(this.Origem_Esquerda, this.Angulo, t.Offset);
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
                        return Ut.Mover(this.Origem_Direita, this.Angulo, -t.Offset);
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
        public string Nome
        {
            get
            {
                string retorno = "";
                if (Tipo == Tipo_ObjetoBase.Corrente)
                {

                    var s = this as ObjetoCorrente;
                    var pc = this.GetPeca();
                    if (pc != null)
                    {
                        pc.COMP = s.Comprimento;
                        retorno = pc.CODIGOFIM;
                    }
                }
                else if (Tipo == Tipo_ObjetoBase.Tirante)
                {
                    var s = this as ObjetoTirante;
                    var pc = this.GetPeca();
                    if (pc != null)
                    {
                        pc.COMP = s.Comprimento;
                        retorno = pc.CODIGOFIM;
                    }
                }
                else if (Tipo == Tipo_ObjetoBase.Purlin)
                {
                    var s = this as ObjetoPurlin;
                    retorno = s.PurlinPadrao;
                    return retorno;
                }
                else retorno = "Base";

                retorno = retorno + " #" + this.Espessura.ToString("N2");



                return retorno;
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
        private Conexoes.RME _pecaRME { get; set; }


        public List<UIElement> GetCanvas(bool inserir_botao = false)
        {

            List<UIElement> retorno = new List<UIElement>();

            UpdateLinha();
            retorno.Add(_linha);


           if(inserir_botao)
            {
                var pt = new System.Windows.Point((this.CentroBloco.X - this.Grade.p0.X) * this.Grade.escala, (this.CentroBloco.Y - this.Grade.p0.Y) * this.Grade.escala);

                double angulo = 0;
                if (this is ObjetoCorrente)
                {
                    angulo = 90;
                }

                _botao = Conexoes.FuncoesCanvas.Botao(this.Letra, pt, this.Cor.Clone(), this.Grade.CADPurlin.Canvas_Tam_Texto, angulo, 1, Conexoes.FuncoesCanvas.Cores.Black);

                _botao.MouseMove += Evento_Sobre;
                _botao.MouseLeave += Evento_Sair;
                _botao.MouseRightButtonUp += Botao_Direito;

                _botao.ToolTip = this;

                _botao.Click += Evento_Clicar;


                retorno.Add(_botao);
            }

            return retorno;
        }

        public void UpdateLinha()
        {
            if(this.Grade.canvas!=null && this._linha!=null)
            {
                this.Grade.canvas.Children.Remove(this._linha);
            }
            var p1 = Ut.GetWPoint(this.P1);
            var p2 = Ut.GetWPoint(this.P2);

            p1 = new Point((p1.X - this.Grade.p0.X)*this.Grade.escala, (p1.Y - this.Grade.p0.Y)* this.Grade.escala);
            p2 = new Point((p2.X - this.Grade.p0.X)* this.Grade.escala, (p2.Y - this.Grade.p0.Y)* this.Grade.escala);
            _linha = Conexoes.FuncoesCanvas.Linha(p1,p2, this.Cor.Clone(),this.VaoObra.CADPurlin.Canvas_Espessura_Multiline);
            _linha.MouseMove += Evento_Sobre;
            _linha.MouseLeave += Evento_Sair;
            _linha.MouseRightButtonUp += Botao_Direito;
            _linha.ToolTip = this;
        }
        public Conexoes.RME GetPeca()
        {
            if (_pecaRME == null)
            {
                if (!this.Considerar | this.id_peca < 1)
                {
                    return null;
                }
                _pecaRME = Conexoes.DBases.GetBancoRM().GetRME(this.id_peca);
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
                }
            }
            else if (this is ObjetoPurlin)
            {
                var sel = Ut.SelecionarPurlin(this.GetPeca());
                if (sel != null)
                {
                    SetPeca(sel);
                }

            }
            else if (this is ObjetoTirante)
            {
                var sel = Ut.SelecionarTirante();
                if (sel != null)
                {
                    SetPeca(sel);
                }
            }

            Conexoes.FuncoesCanvas.SetCor(_botao, this.Cor, Conexoes.FuncoesCanvas.Cores.Black);


        }

        private void Botao_Direito(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool status = false;
            Conexoes.Utilz.Propriedades(this,out status);
            if(status)
            {
                UpdateLinha();
                this.Grade.canvas.Children.Add(this._linha);
            }
        }

        public void Evento_Sair(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, this.Cor, Conexoes.FuncoesCanvas.Cores.Black);
        }

        public void Evento_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = sender as UIElement;
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, Conexoes.FuncoesCanvas.Cores.White, this.Cor);
            Conexoes.FuncoesCanvas.TrazerPraFrente(p);
        }

        public string Letra
        {
            get
            {
                return Conexoes.Utilz.getLetra(this.id);
            }
        }

        [Browsable(false)]
        public SolidColorBrush Cor
        {
            get
            {
                if (this.GetPeca() != null)
                {
                    var o = this.GetPeca().GetCorEsp();
                    o = o.Clone();
                    o.Opacity = 1;
                    return o;
                }

                SolidColorBrush pp = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                return pp;
            }
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



        public void SetPeca(Conexoes.RME rm)
        {
            this.id_peca = rm.id_db;

            this._pecaRME = rm;

            this.NotifyPropertyChanged("Cor");
            this.NotifyPropertyChanged("id");
            this.NotifyPropertyChanged("Nome");

            if (this._botao != null)
            {
                this._botao.Content = this.Letra;
            }

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
                return Ut.Centro(this.Origem_Esquerda, this.Origem_Direita);
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
        public CADPurlin CADPurlin { get; internal set; }
        [Browsable(false)]
        public GradeEixos Grade { get; set; }
        public ObjetoMultiLineBase()
        {
            this.id = ObjetoMultiLineBase.id_cont;
            ObjetoMultiLineBase.id_cont++;
        }
    }
}
