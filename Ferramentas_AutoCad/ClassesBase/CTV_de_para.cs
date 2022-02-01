using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
   public  class CTV_de_para
    {
        public override string ToString()
        {
            return this.Perfil + " => " + this.PecaLiberar;
        }
        public string Perfil { get; set; } = "";
        public string PecaLiberar { get; set; } = "";
        public string Destino { get; set; } = "";
        public double Diametro { get; set; } = 0;
        public int CaractComp { get; set; } = 5;

        public CTV_de_para(string perfil, string peca, double diam, int caract, string destino)
        {
            this.Perfil = perfil;
            this.PecaLiberar = peca;
            this.Diametro = diam;
            this.CaractComp = caract;
            this.Destino = destino;
        }
    }
}
