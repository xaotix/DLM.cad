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
        public bool Objeto_Orfao
        {
            get
            {
                return this.VaoObra == null;
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
        public string Suporte { get; set; } = "";
        public override string ToString()
        {
            return this.Nome;
        }
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
        private System.Windows.Controls.Button botao { get; set; }
        public List<UIElement> GetCanvas(Point p0, double escala, double tam_texto)
        {


            List<UIElement> retorno = new List<UIElement>();

            var pt = new System.Windows.Point((this.CentroBloco.X - p0.X) * escala, (this.CentroBloco.Y - p0.Y) * escala);

            double angulo = 0;
            if (this is ObjetoCorrente)
            {
                angulo = 90;
            }

            botao = Conexoes.FuncoesCanvas.Botao(this.Letra, pt, this.Cor.Clone(), tam_texto, angulo, 1, Conexoes.FuncoesCanvas.Cores.Black);

            botao.MouseMove += Botao_Sobre;
            botao.MouseLeave += Botao_Sai;
            botao.MouseRightButtonUp += Botao_Direito;

            botao.ToolTip = this;

            botao.Click += Botao_Click;


            retorno.Add(botao);

            return retorno;
        }

        private void Botao_Click(object sender, RoutedEventArgs e)
        {
            if (this is ObjetoCorrente)
            {
                var sel = Utilidades.SelecionarCorrente();
                if (sel != null)
                {
                    SetPeca(sel);
                }
            }
            else if (this is ObjetoPurlin)
            {
                var sel = Utilidades.SelecionarPurlin(this.GetPeca());
                if (sel != null)
                {
                    SetPeca(sel);
                }

            }
            else if (this is ObjetoTirante)
            {
                var sel = Utilidades.SelecionarTirante();
                if (sel != null)
                {
                    SetPeca(sel);
                }
            }

            Conexoes.FuncoesCanvas.SetCor(botao, this.Cor, Conexoes.FuncoesCanvas.Cores.Black);


        }

        private void Botao_Direito(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Conexoes.Utilz.Propriedades(this);
        }

        public void Botao_Sai(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, this.Cor, Conexoes.FuncoesCanvas.Cores.Black);
        }

        public void Botao_Sobre(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var p = sender as UIElement;
            Conexoes.FuncoesCanvas.SetCor(sender as UIElement, Conexoes.FuncoesCanvas.Cores.Black, this.Cor);
            Conexoes.FuncoesCanvas.TrazerPraFrente(p);
        }

        public string Letra
        {
            get
            {
                return Conexoes.Utilz.getLetra(this.id);
            }
        }

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
        public static int id_cont { get; set; } = 1;
        public int id { get; private set; } = 0;
        public double Y
        {
            get
            {
                return Math.Round(this.CentroBloco.Y);
            }
        }

        private Conexoes.RME _pecaRME { get; set; }
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


        public void SetPeca(Conexoes.RME rm)
        {
            this.id_peca = rm.id_db;

            this._pecaRME = rm;

            this.NotifyPropertyChanged("Cor");
            this.NotifyPropertyChanged("id");
            this.NotifyPropertyChanged("Nome");

            if (this.botao != null)
            {
                this.botao.Content = this.Letra;
            }

        }
        public int id_peca { get; internal set; } = -1;
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
        public Point3d CentroBloco { get; internal set; }
        public ObjetoMultiline Multiline { get; internal set; }
        public bool Considerar { get; internal set; } = true;
        public VaoObra VaoObra { get; internal set; }
        public ObjetoPurlin PurlinEmBaixo { get; internal set; }
        public ObjetoPurlin PurlinEmCima { get; internal set; }
        public CADPurlin CADPurlin { get; internal set; }
        public ObjetoMultiLineBase(ObjetoMultiline multiline, Point3d origem, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.VaoObra = vao;

        }
        public ObjetoMultiLineBase(Point3d origem, VaoObra vao)
        {
            this.CentroBloco = origem;
            this.VaoObra = vao;
            this.Considerar = false;
        }
        public ObjetoMultiLineBase()
        {
            this.id = ObjetoMultiLineBase.id_cont;
            ObjetoMultiLineBase.id_cont++;
        }
    }
}
