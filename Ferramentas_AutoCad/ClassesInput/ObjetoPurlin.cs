using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ferramentas_DLM
{
    public class ObjetoPurlin : ObjetoBase
    {

        public List<ObjetoCorrente> Correntes
        {
            get
            {
                return this.Vao.GetCorrentes().FindAll(x => PurlinEmCima == this);
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

        public double FBE { get; set; } = 0;
        public double FBD { get; set; } = 0;
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

        public ObjetoPurlin(ObjetoMultiline multiline, Point3d origem, int numero, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.Numero = numero;
            this.Vao = vao;
            this.id_peca = vao.id_terca;
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

        public ObjetoCorrente(ObjetoMultiline multiline, Point3d centro, int numero, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = centro;
            this.Numero = numero;
            this.Vao = vao;
            this.Descontar = vao.CADPurlin.CorrenteDescontar;
            this.id_peca = vao.CADPurlin.id_corrente;
            this.Suporte = vao.CADPurlin.CorrenteSuporte;
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

        public string Nome
        {
            get
            {
                string nome = "";
                var pc = this.GetPeca();
                if (pc != null)
                {
                    pc.COMP = this.Comprimento;
                    nome = pc.CODIGOFIM;
                }
                return nome;
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
        }
    }

    public class ObjetoBase
    {
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
        public void SetPeca(int id)
        {
            this.id_peca = id;
            this._pecaRME = null;
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
        public int Numero { get; set; } = 0;
        public ObjetoBase(ObjetoMultiline multiline, Point3d origem, int numero, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.Numero = numero;
        }
        public ObjetoBase(Point3d origem, VaoObra vao)
        {
            this.CentroBloco = origem;
            this.Vao = vao;
            this.Considerar = false;
        }

        public ObjetoBase()
        {

        }


    }



}
