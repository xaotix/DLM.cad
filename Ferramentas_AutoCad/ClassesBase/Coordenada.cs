using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{

    public class Coordenada
    {
        public System.Windows.Point GetPoint2d()
        {
            return new System.Windows.Point(this.X, this.Y);
        }
        public Coordenada(System.Windows.Point p)
        {
            this.X = p.X;
            this.X = p.Y;
        }
        public Coordenada Mover(double X, double Y = 0, double Z = 0)
        {
            var s= new Coordenada(new Point3d(this.X + X, this.Y + Y, this.Z + Z));
            s.Tipo = this.Tipo;
            return s;
        }
        public override string ToString()
        {
            return "[" + Tipo.ToString().PadRight(10,' ') + "] [" + this.id.ToString().PadLeft(3, '0') + "] " + chave;
        }
        public Point3d GetPoint()
        {
            return new Point3d(this.X, this.Y, 0);
        }
        //public string chave
        //{
        //    get
        //    {
        //        return "X: " + Math.Round(this.X,3).ToString().PadRight(10, ' ') + " | Y" + Math.Round(this.Y,3).ToString().PadRight(10, ' ');
        //    }
        //}

        public string chave
        {
            get
            {
                return "X: " + this.X.ToString().PadRight(10, ' ') + " | Y" + this.Y.ToString().PadRight(10, ' ');
            }
        }

        public double DistanciaX(Point3d p)
        {
            return DistanciaX(new Coordenada(p));
        }
        public double DistanciaX(Coordenada v)
        {
            if(v== null) { return - 1; }
            return this.X > v.X ? Math.Abs(this.X - v.X) : Math.Abs(v.X - this.X);
        }
        public double Distancia(Point3d p)
        {
            return Distancia(new Coordenada(p));
        }

        public double Angulo(Coordenada v)
        {
          return  Calculos.Trigonometria.Angulo(new Calculos.Ponto3D(this.X, this.Y, this.Z), new Calculos.Ponto3D(v.X, v.Y, v.Z));
        }
        public double Angulo(Point3d v)
        {
            return Angulo(new Coordenada(v));
        }

       
        public Coordenada Mover(double angulo, double distancia)
        {
            var s = Calculos.Trigonometria.Mover(new System.Windows.Point(this.X, this.Y), angulo, distancia);

            return new Coordenada(s.X, s.Y, 0);
        }
        public double Distancia(Coordenada v)
        {
            double xDelta = this.X - v.X;
            double yDelta = this.Y - v.Y;

            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2));
        }
        public double DistanciaY(Coordenada v)
        {
            if (v == null) { return -1; }
            return this.Y > v.Y ? Math.Abs(this.Y - v.Y) : Math.Abs(v.Y - this.Y);
        }
        public double DistanciaY(Point3d v)
        {
            return DistanciaY(new Coordenada(v));
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Tipo_Coordenada Tipo { get; set; }
        public int id { get; set; }
        public Coordenada anterior { get; set; } 
        public Coordenada proxima { get; set; }

        public double Getdist_proximaX(int decimais = 0)
        {
            return Math.Round(this.DistanciaX(proxima),decimais);
        }

        public double GetDist_proximaY(int decimais = 0)
        {
            return Math.Round(this.DistanciaY(proxima),decimais);
        }
        private List<Coordenada> IguaisX { get; set; }

        public List<Coordenada> PegarIguaisX()
        {
            if (IguaisX == null)
            {
                IguaisX = new List<Coordenada>();
                var proxima = this.proxima;
                var atual = this;
                if (this.proxima != null)
                {
                    while (proxima != null && IguaisX.Count < 10)
                    {
                        if (proxima.Getdist_proximaX() == this.Getdist_proximaX() && this.Getdist_proximaX() > 0)
                        {
                            if (IguaisX
                                .Find(x => x.id == proxima.id) == null 
                                && proxima.id == atual.id +1 
                                && proxima.X>atual.X
                                && proxima.chave!=this.chave 
                                && proxima.chave!=atual.chave)
                            {
                                IguaisX.Add(proxima);
                                atual = proxima;
                                proxima = proxima.proxima;
                            }
                            else
                            {
                                proxima = null;
                            }
                        }
                        else
                        {
                            proxima = null;
                        }
                    }
                }
                IguaisX = IguaisX.FindAll(x => x != null);
            }


            return IguaisX;
        }
        private List<Coordenada> IguaisY { get; set; }
        public List<Coordenada> PegarIguaisY()
        {
            if (IguaisY == null)
            {
                IguaisY = new List<Coordenada>();

                var proxima = this.proxima;
                var atual = this;
                if (this.proxima != null)
                {
                    while (proxima != null && IguaisY.Count < 10)
                    {
                        if (proxima.GetDist_proximaY() == this.GetDist_proximaY() && this.GetDist_proximaY() > 0)
                        {
                            if (IguaisY
                                .Find(x => x.id == proxima.id) == null
                                && proxima.Y > atual.Y
                                && proxima.id == atual.id +1
                                && proxima.chave != this.chave 
                                && proxima.chave != atual.chave)
                            {
                                IguaisY.Add(proxima);
                                atual = proxima;
                                proxima = proxima.proxima;
                            }
                            else
                            {
                                proxima = null;
                            }

                        }
                        else
                        {
                            proxima = null;
                        }
                    }
                }
                IguaisY = IguaisY.FindAll(x => x != null);

            }


            return IguaisY;
        }
        public Coordenada(Point3d pt, int id, Tipo_Coordenada tipo)
        {

            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
            this.id = id;
            this.Tipo = tipo;
        }
        public Coordenada(Point3d pt)
        {

            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public Coordenada(double X, double Y, double Z)
        {

            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public Coordenada(Coordenada pt, int arredondar = 1)
        {
            this.X = Math.Round(pt.X, arredondar);
            this.Y = Math.Round(pt.Y, arredondar);
            this.Z = Math.Round(pt.Z, arredondar);

            this.Tipo = pt.Tipo;
        }

        public Coordenada GetCentro(Point3d pt)
        {
            return GetCentro(new Coordenada(pt));
        }
        public Coordenada GetCentro(Coordenada ps)
        {
            return new Coordenada(
                (this.X + ps.X) / 2,
                (this.Y + ps.Y) / 2,
                (this.Z + ps.Z) / 2);
        }
        public Coordenada()
        {

        }
    }
}
