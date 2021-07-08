using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Ferramentas_DLM.Classes
{
    public class LinhaFuros
    {
        public string Nome
        {
            get
            {
                if(Furos!=null)
                {
                    if(Furos.Count>0)
                    {
                        var s = Furos.Select(x => x.GetNome()).Distinct().ToList();
                        if(s.Count==1)
                        {
                            return s[0];
                        }
                        else
                        {
                            return string.Join(" ", s);
                        }

                    }
                }
                return "";

            }
        }
        public List<Furo> Furos { get; set; }
        public double Y { get; set; }
        public Coordenada Origem()
        {
            return new Coordenada(new Point3d(GetXmin(), Y, 0));
        }
        public Coordenada Fim()
        {
            return new Coordenada(new Point3d(GetXmax(), Y, 0));
        }
        public double GetXmin()
        {
            if(Furos!=null)
            {
                if(Furos.Count>0)
                {
                    return Furos.Min(x => x.Origem.X);
                }
            }
            return 0;
        }
        public double GetXmax()
        {
            if (Furos != null)
            {
                if (Furos.Count > 0)
                {
                    return Furos.Max(x => x.Origem.X);
                }
            }
            return 0;
        }
        public LinhaFuros(List<BlockReference> blocos, double Y)
        {
            this.Furos = new List<Furo>();
            this.Y = Y;
            foreach(var s in blocos)
            {
                this.Furos.Add(new Furo(s));
            }
        }
        public LinhaFuros(List<Furo> furos)
        {
            this.Y = 0;
            this.Furos = furos;
            if(this.Furos.Count>0)
            {
                this.Y = this.Furos.Max(x => x.Origem.Y);
            }
        }
    }
}
