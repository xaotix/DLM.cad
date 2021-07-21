using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace Ferramentas_DLM
{
    public class ObjetoPurlin : ObjetoBase
    {
        public double Vao
        {
            get
            {
                if (this.Objeto_Orfao)
                {
                    return this.Multiline.comprimento;
                }
                else
                {
                    return this.VaoObra.Vao;
                }
            }
        }
     
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
                    var pc = this.VaoObra.CADPurlin.GetFlangeBracePadrao();
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
                    var pc = this.VaoObra.CADPurlin.GetFlangeBracePadrao();
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


        public double X1 { get
            {
                return this.Origem_Esquerda.X - TRE;
            }
        }
        public double X2
        {
            get
            {
                return this.Origem_Direita.X + TRD;
            }
        }

        public double Comprimento
        {
            get
            {
                if(Objeto_Orfao)
                {
                    return this.Multiline.comprimento;
                }
                else
                {
                    return this.TRE + this.TRD + this.Vao;
                }
            }
        }

        public ObjetoPurlin(ObjetoMultiline multiline, Point3d origem,VaoObra vao)
        {
            this.CADPurlin = vao.CADPurlin;
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.VaoObra = vao;
            this.SetPeca(vao.CADPurlin.GetPurlinPadrao());



            this.Origem_Direita = new Point3d(this.VaoObra.Direita.Origem.X, this.CentroBloco.Y, 0);
            this.Origem_Esquerda = new Point3d(this.VaoObra.Esquerda.Origem.X, this.CentroBloco.Y, 0);
        }

        public ObjetoPurlin(ObjetoMultiline multiline, CADPurlin cADPurlin)
        {
            this.CADPurlin = cADPurlin;
            this.Multiline = multiline;
            this.CentroBloco = multiline.centro.GetPoint();
            
            this.SetPeca(cADPurlin.GetPurlinPadrao());



            this.Origem_Direita = new Point3d(this.Multiline.Fim.X, this.CentroBloco.Y, 0);
            this.Origem_Esquerda = new Point3d(this.Multiline.Inicio.X, this.CentroBloco.Y, 0);
        }

        public ObjetoPurlin(Point3d origem, VaoObra vao)
        {
            this.CADPurlin = vao.CADPurlin;
            this.CentroBloco = origem;
            this.VaoObra = vao;
            this.id_peca = -1;
            this.Considerar = false;
        }
    }

}
