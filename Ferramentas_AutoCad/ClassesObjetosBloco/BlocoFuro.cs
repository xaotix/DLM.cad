using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace DLM.cad
{
    public class BlocoFuro:BlocoTag
    {
        public override string ToString()
        {
            return this.GetChave();
        }
        public string GetChave()
        {
            return "Ø" + Diametro + (Oblongo > 0 ? ("x"+ (Oblongo + Diametro).ToString()) : "");
        }
        public double Diametro { get; private set; } = 0;
        public double Oblongo { get; private set; } = 0;
        public BlocoFuro(BlockReference vv):base(vv,true)
        {
            this.Diametro = 0;
            this.Oblongo = 0;



           

            if (Bloco.Name.ToUpper().Contains("HOLE") | Bloco.Name.ToUpper().Contains("MA"))
            {


                    this.Diametro = this.Get("DIAM").Double();
                    this.Oblongo = this.Get("LENGTH").Double();

                if(this.Oblongo==0)
                {
                    var x1 = this.Get("X1").Double();
                    var x2 = this.Get("X2").Double();

                    var y1 = this.Get("Y1").Double();
                    var y2 = this.Get("Y2").Double();

                    var xobl = Math.Abs(x1) + Math.Abs(x2);
                    var yobl = Math.Abs(y1) + Math.Abs(y2);

                    if (xobl > 0)
                    {
                        this.Oblongo = xobl;
                    }
                    else
                    {
                        this.Oblongo = yobl;
                    }
                }
               
            }
            else
            {
                var tolerancia = this.Get("TOLE").Double();

                double diametro = Conexoes.Utilz.Double(this.Bloco.Name.Replace("M", "").Replace(Constantes.ATT_N, ""));
                this.Diametro = diametro;
                if (tolerancia !=0)
                {
                    this.Diametro = this.Diametro + tolerancia;
                }
            }

        }
    }
}
