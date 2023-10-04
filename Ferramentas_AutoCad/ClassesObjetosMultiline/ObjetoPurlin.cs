using Autodesk.AutoCAD.Geometry;
using DLM.desenho;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DLM.cad
{
    public class ObjetoPurlin : ObjetoMultiLineBase
    {
        [Category("Eixo")]
        [DisplayName("Vão")]
        public double Vao
        {
            get
            {
                if (this.Objeto_Orfao)
                {
                    return Math.Round(this.Multiline.Comprimento);
                }
                else
                {
                    return this.VaoObra.Vao;
                }
            }
        }
     

        [Browsable(false)]
        public List<ObjetoCorrente> Correntes { get; set; } = new List<ObjetoCorrente>();
        [Category("Geometria")]
        [DisplayName("Comprimento FB. Esq.")]
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
                    var pc = Core.GetCADPurlin().GetFlangeBracePadrao();
                    if(pc!=null)
                    {
                        Conexoes.RMLite cc = pc.Get(value);

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
        [Category("Geometria")]
        [DisplayName("Comprimento FB. Dir.")]
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
                    var pc = Core.GetCADPurlin().GetFlangeBracePadrao();
                    if (pc != null)
                    {
                        Conexoes.RMLite cc = pc.Get(value);
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
        [Category("Geometria")]
        [DisplayName("Nome FB. Esq.")]
        public string FBE { get; private set; } = "";
        [Category("Geometria")]
        [DisplayName("Nome FB. Dir.")]
        public string FBD { get; private set; } = "";
        [Category("Geometria")]
        [DisplayName("Transpasse <- ESQ")]
        public double TRE { get; set; } = 0;
        [Category("Geometria")]
        [DisplayName("Transpasse DIR ->")]
        public double TRD { get; set; } = 0;
        [Browsable(false)]
        public List<double> FurosCorrentes { get; set; } = new List<double>();
        [Browsable(false)]
        public List<double> FurosManuais { get; set; } = new List<double>();
        public override string ToString()
        {
            return this.PurlinPadrao + " Vão: " + this.Vao + " Y: " + this.Y;
        }
        [Browsable(false)]
        public ObjetoPurlin PurlinEsquerda { get; set; }
        [Browsable(false)]
        public ObjetoPurlin PurlinDireita { get; set; }
        [Browsable(false)]
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
                    var desc = this.GetPeca().DESC.Replace("PERFIL PADRAO ", "");
                    return desc;
                }

                return "???";
            }
        }

        [Category("Geometria")]
        [DisplayName("Dist. Purlin Acima")]
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
        [Category("Geometria")]
        [DisplayName("Dist. Purlin Abaixo")]
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

        [Browsable(false)]
        public double X1 { get
            {
                return this.Origem_Esquerda.X - TRE;
            }
        }
        [Browsable(false)]
        public double X2
        {
            get
            {
                return this.Origem_Direita.X + TRD;
            }
        }





        public ObjetoPurlin(CADMline multiline,VaoObra vao)
        {
            this.Grade = vao.Grade;
  
            this.Multiline = multiline;

            this.VaoObra = vao;
            this.SetPeca(Core.GetCADPurlin().GetPurlinPadrao());


            var p1 = this.Multiline.GetInterSeccao(this.VaoObra.Esquerda.GetLinhaEixo(vao.Grade));
            var p2 = this.Multiline.GetInterSeccao(this.VaoObra.Direita.GetLinhaEixo(vao.Grade));
            if(p1.Count>0)
            {
                this.Origem_Esquerda = p1[0].P3d();
            }
            else
            {
                this.Origem_Esquerda = new P3d(this.VaoObra.Esquerda.Origem.X, this.CentroBloco.Y);
            }

            if (p2.Count>0)
            {
                this.Origem_Direita = p2[0].P3d();
            }
            else
            {
                this.Origem_Direita = new P3d(this.VaoObra.Direita.Origem.X, this.CentroBloco.Y);
            }
        }

        public ObjetoPurlin(CADMline multiline,  GradeEixos grade)
        {

            this.Multiline = multiline;
            this.Grade = grade;

            
            this.SetPeca(Core.GetCADPurlin().GetPurlinPadrao());



            this.Origem_Direita = new P3d(this.Multiline.Fim.X, multiline.Centro.Y);
            this.Origem_Esquerda = new P3d(this.Multiline.Inicio.X, multiline.Centro.Y);
        }

        public ObjetoPurlin(Point2d origem, VaoObra vao)
        {
            this.VaoObra = vao;
            this.id_peca = -1;
            this.Considerar = false;
        }
    }

}
