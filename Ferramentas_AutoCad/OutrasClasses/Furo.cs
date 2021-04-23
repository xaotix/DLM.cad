using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace Ferramentas_DLM.Classes
{
    public class Furo
    {
        public string GetNome()
        {
            return "Ø" + Diametro + (Oblongo > 0 ? ("x"+ (Oblongo + Diametro).ToString()) : "");
        }
        public Coordenada Origem { get; set; }
        public double Diametro { get; set; }
        public double Oblongo { get; set; }
        public List<List<string>> atributos { get; set; }
        public BlockReference bloco { get; set; }
        public Furo()
        {

        }
        public Furo(BlockReference vv)
        {
            this.bloco = vv;
            this.Origem = new Coordenada(bloco.Position);


            this.Diametro = 0;
            this.Oblongo = 0;

            this.atributos = Atributos.GetStr(this.bloco);

           

            if (bloco.Name.ToUpper().Contains("HOLE") | bloco.Name.ToUpper().Contains("MA"))
            {
                var dim = atributos.Find(x => x[0].ToUpper().Contains("DIAM"));
                var len = atributos.Find(x => x[0].ToUpper().Contains("LENGTH"));

                if (dim != null)
                {
                    this.Diametro = Conexoes.Utilz.Double(dim[1]);
                }

                if (len != null)
                {
                    this.Oblongo = Conexoes.Utilz.Double(len[1]);
                }
                else
                {
                    var x1 = atributos.Find(x => x[0].ToUpper().Contains("X1"));
                    var x2 = atributos.Find(x => x[0].ToUpper().Contains("X2"));

                    var y1 = atributos.Find(x => x[0].ToUpper().Contains("Y1"));
                    var y2 = atributos.Find(x => x[0].ToUpper().Contains("Y2"));

                    if (x1 != null && x2 != null && y1 != null && y2 != null)
                    {
                        var xobl = Math.Abs(Conexoes.Utilz.Double(x1[1])) + Math.Abs(Conexoes.Utilz.Double(x2[1]));
                        var yobl = Math.Abs(Conexoes.Utilz.Double(y1[1])) + Math.Abs(Conexoes.Utilz.Double(y2[1]));
                        if(xobl>0)
                        {
                            this.Oblongo = xobl;
                        }
                        else
                        {
                            this.Oblongo = yobl;
                        }
                    }
                }
            }
            else
            {
                var tolerancia = atributos.Find(x => x[0].ToUpper().StartsWith("TOLE"));

                double diametro = Conexoes.Utilz.Double(this.bloco.Name.Replace("M", "").Replace("N", ""));
                this.Diametro = diametro;
                if (tolerancia != null)
                {
                    this.Diametro = this.Diametro + Conexoes.Utilz.Double(tolerancia[1]);
                }
            }

        }
    }
}
