using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ferramentas_DLM
{
    public class ObjetoPurlin : ObjetoBase
    {

        public Point3d Origem_Esquerda { get; private set; } = new Point3d();
        public Point3d Origem_Direita { get; private set; } = new Point3d();
        public List<ObjetoCorrente> Correntes { get; set; } = new List<ObjetoCorrente>();


        public double FBE_Comp
        {
            get
            {
                return _FBE_Comp;
            }
            set
            {
                _FBE_Comp = value;

                if(value>0)
                {
                    var pc = this.Vao.CADPurlin.GetFlangeBracePadrao();
                    if(pc!=null)
                    {
                        Conexoes.RME cc = new Conexoes.RME(pc);
                        cc.COMP = value;
                        FBE = cc.CODIGOFIM;
                    }
                    else
                    {
                        FBE = "???";
                    }
                }
                else
                {
                    FBE = "";
                }
            }
        }
        private double _FBE_Comp { get; set; } = 0;
        public double FBD_Comp
        {
            get
            {
                return _FBD_Comp;
            }
            set
            {
                _FBD_Comp = value;

                if (value > 0)
                {
                    var pc = this.Vao.CADPurlin.GetFlangeBracePadrao();
                    if (pc != null)
                    {
                        Conexoes.RME cc = new Conexoes.RME(pc);
                        cc.COMP = value;
                        FBD = cc.CODIGOFIM;
                    }
                    else
                    {
                        FBD = "???";
                    }
                }
                else
                {
                    FBD = "";
                }
            }
        }
        private double _FBD_Comp { get; set; } = 0;

        public string FBE { get; private set; } = "";
        public string FBD { get; private set; } = "";
        public double TRE { get; set; } = 0;
        public double TRD { get; set; } = 0;
        public List<double> FurosCorrentes { get; set; } = new List<double>();
        public List<double> FurosManuais { get; set; } = new List<double>();


        public override string ToString()
        {
            return this.PurlinPadrao + " Vão: " + this.Vao + " Y: " + this.Y;
        }

        public ObjetoPurlin PurlinEsquerda { get; set; }
        public ObjetoPurlin PurlinDireita { get; set; }
        public string PurlinPadrao
        {
            get
            {
                if(!this.Considerar)
                {
                    return "";
                }
                if (this.GetPeca() != null)
                {
                    var desc = Utilidades.GetDescricao(this.GetPeca());
                    return desc;
                }

                return "???";
            }
        }


        public double DistCima
        {
            get
            {
                if (PurlinEmCima != null)
                {
                    return Math.Abs(Math.Round(PurlinEmCima.Y - this.Y));
                }
                return 0;
            }
        }
        public double DistBaixo
        {
            get
            {
                if (PurlinEmBaixo != null)
                {
                    return Math.Abs(Math.Round(PurlinEmBaixo.Y - this.Y));
                }
                return 0;
            }
        }




        public double Comprimento
        {
            get
            {
                return this.TRE + this.TRD + this.Vao.Vao;
            }
        }

        public ObjetoPurlin(ObjetoMultiline multiline, Point3d origem,VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.Vao = vao;
            this.SetPeca(vao.CADPurlin.GetPurlinPadrao());

            this.SetLetra(this.PurlinPadrao);

            this.Origem_Direita = new Point3d(this.Vao.Direita.Origem.X, this.CentroBloco.Y, 0);
            this.Origem_Esquerda = new Point3d(this.Vao.Esquerda.Origem.X, this.CentroBloco.Y, 0);
        }

        public ObjetoPurlin(Point3d origem, VaoObra vao)
        {
            this.CentroBloco = origem;
            this.Vao = vao;
            this.id_peca = -1;
            this.Considerar = false;
        }
    }

    public class ObjetoCorrente :ObjetoBase
    {
        public override string ToString()
        {
            return this.Nome;
        }
        public string Nome
        {
            get
            {
                string nome = "";
                var pc = this.GetPeca();
                if(pc!=null)
                {
                    pc.COMP = this.Comprimento;
                    nome = pc.CODIGOFIM;
                }
                return nome;
            }
        }
        public string Suporte { get; set; } = "";
        public double Comprimento
        {
            get
            {
                var comp = EntrePurlin;
                if(comp>0)
                {
                    comp = comp - (2 * Math.Abs(Descontar));
                }
                return comp;
            }
        }
        public double Descontar { get; set; } = 20;

        public double EntrePurlin
        {
            get
            {
                if(PurlinEmCima!=null && PurlinEmBaixo!=null)
                {
                return Math.Abs(Math.Round(PurlinEmBaixo.CentroBloco.Y - PurlinEmCima.CentroBloco.Y));
                }
                else
                {
                    return 0;
                }
            }
        }

        public ObjetoCorrente(ObjetoMultiline multiline, Point3d centro,  VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = centro;

            this.Vao = vao;
            this.Descontar = vao.CADPurlin.CorrenteDescontar;
            this.id_peca = vao.CADPurlin.id_corrente;
            this.Suporte = vao.CADPurlin.CorrenteSuporte;



            this.SetPeca(vao.CADPurlin.GetCorrentePadrao());

            if(this.GetPeca()!=null)
            {
              
            }
        }
    }

    public class ObjetoTirante :ObjetoBase
    {
        public override string ToString()
        {
            return this.Nome;
        }
        public double Comprimento
        {
            get
            {
                return this.Multiline.comprimento +  2*Offset;
            }
        }



        public string Suporte { get; set; } = "";
        public double Offset { get; set; } = 0;

        public ObjetoTirante(ObjetoMultiline multiline, int numero, VaoObra vao)
        {
            this.id_peca = vao.CADPurlin.id_tirante;
            this.Multiline = multiline;
            this.CentroBloco = multiline.centro.GetPoint();
            this.Vao = vao;

            this.Offset = vao.CADPurlin.TirantesOffSet;

            this.Suporte = vao.CADPurlin.TirantesSuporte;
            this.SetPeca(vao.CADPurlin.GetTirantePadrao());
        }
    }

    public enum Tipo_ObjetoBase
    {
        Purlin,
        Corrente,
        Tirante,
        Base,
    }
    public class ObjetoBase
    {
        public override string ToString()
        {
            return this.Nome;
        }
        public double Espessura
        {
            get
            {
                if(this._pecaRME!=null)
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
        private System.Windows.Controls.Border txt { get; set; }
        public List<UIElement> GetCanvas(Point p0, double escala, double tam_texto)
        {
            List<UIElement> retorno = new List<UIElement>();
            var pt = new System.Windows.Point((this.CentroBloco.X - p0.X) * escala, (this.CentroBloco.Y - p0.Y) * escala);
            txt = Conexoes.FuncoesCanvas.Texto(this.Letra, pt, this.Cor.Clone(), tam_texto);
            txt.MouseMove += Txt_MouseMove;
            txt.MouseLeave += Txt_MouseLeave;
            txt.MouseRightButtonUp += Txt_MouseRightButtonUp;
            txt.MouseLeftButtonUp += Txt_MouseLeftButtonUp;
            txt.ToolTip = this;

            retorno.Add(txt);

            return retorno;
        }

        private void Txt_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           if(this is ObjetoCorrente)
            {

            }
           else if(this is ObjetoPurlin)
            {

            }
           else if(this is ObjetoTirante)
            {

            }
        }

        private void Txt_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Conexoes.Utilz.Propriedades(this);
        }

        private void Txt_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Normalizar();

        }

        private void Normalizar()
        {
            Conexoes.FuncoesCanvas.SetCor(txt, this.Cor, Conexoes.FuncoesCanvas.Cores.Black);
        }

        private void Txt_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Destacar();
        }

        private void Destacar()
        {
            Conexoes.FuncoesCanvas.SetCor(txt, Conexoes.FuncoesCanvas.Cores.Black, Conexoes.FuncoesCanvas.Cores.Yellow);
        }

        private string _Letra { get; set; } = "";

        public string Letra
        {
            get
            {
                if(this._Letra.Length>0)
                {
                    return _Letra;
                }
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
                    o.Opacity = .5;
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

        public void SetLetra(string Nome)
        {
            this._Letra = Nome;
        }
        public void SetPeca(Conexoes.RME rm)
        {
            this.id_peca = rm.id_db;

            this._pecaRME = rm;

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
        public bool Considerar { get; set; } =  true;
        public VaoObra Vao { get; set; }
        public ObjetoPurlin PurlinEmBaixo { get; internal set; }
        public ObjetoPurlin PurlinEmCima { get; internal set; }
        //public int Numero { get; set; } = 0;
        public ObjetoBase(ObjetoMultiline multiline, Point3d origem, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.Vao = vao;

        }
        public ObjetoBase(Point3d origem, VaoObra vao)
        {
            this.CentroBloco = origem;
            this.Vao = vao;
            this.Considerar = false;
        }

        public ObjetoBase()
        {
            this.id = ObjetoBase.id_cont;
            ObjetoBase.id_cont++;
        }


    }



}
