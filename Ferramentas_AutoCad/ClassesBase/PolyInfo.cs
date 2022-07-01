using Autodesk.AutoCAD.DatabaseServices;
using DLM.desenho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class PolyInfo
    {
        public Polyline Polyline { get; private set; }
        public double Comprimento { get; private set; } = 0;
        public double Largura { get; private set; } = 0;

        public List<P3d> Pontos { get; set; } = new List<P3d>();



        public PolyInfo(Polyline polyline)
        {
            this.Polyline = polyline;

        }
    }
}
