using Autodesk.AutoCAD.Geometry;
using System;

namespace Ferramentas_DLM
{
    public class ObjetoCorrente :ObjetoBase
    {
        public override string ToString()
        {
            return this.Nome;
        }
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
            this.CADPurlin = vao.CADPurlin;
            this.Multiline = multiline;
            this.CentroBloco = centro;

            this.VaoObra = vao;
            this.Descontar = vao.CADPurlin.CorrenteDescontar;
            this.id_peca = vao.CADPurlin.id_corrente;
            this.Suporte = vao.CADPurlin.CorrenteSuporte;



            this.SetPeca(vao.CADPurlin.GetCorrentePadrao());

            if(this.GetPeca()!=null)
            {
              
            }
        }
    }



}
