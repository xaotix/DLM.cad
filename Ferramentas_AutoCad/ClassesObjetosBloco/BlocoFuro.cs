using Autodesk.AutoCAD.DatabaseServices;
using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public class BlocoFuro:BlockAttributes
    {
        public override string ToString()
        {
            return this.GetChave();
        }
        public string GetChave()
        {
            return "Ø" + Diametro + (Oblongo > 0 ? ("x"+ (Oblongo + Diametro).String(0)) : "");
        }
        public double Diametro { get; private set; } = 0;
        public double Oblongo { get; private set; } = 0;
        public BlocoFuro(BlockReference vv):base(vv,true)
        {
            this.Diametro = 0;
            this.Oblongo = 0;

            if (Block.Name.ToUpper().Contains("HOLE") | Block.Name.ToUpper().Contains("MA"))
            {


                    this.Diametro = this["DIAM"].Double();
                    this.Oblongo =  this["LENGTH"].Double();

                if(this.Oblongo==0)
                {
                    var x1 = this["X1"].Double();
                    var x2 = this["X2"].Double();
                    var y1 = this["Y1"].Double();
                    var y2 = this["Y2"].Double();

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
                var tolerancia = this["TOLE"].Double();

                double diametro = this.Block.Name.Replace("M", "").Replace(Cfg.Init.CAD_ATT_N, "").Double();
                this.Diametro = diametro;
                if (tolerancia != 0)
                {
                    this.Diametro = this.Diametro + tolerancia;
                }
            }

        }
    }
}
