using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public static class P3dCAD_Extensoes
    {
        public static List<Point3d> Point3d(this List<P3d> lista)
        {
            return lista.Select(x => new Autodesk.AutoCAD.Geometry.Point3d(x.X, x.Y, x.Z)).ToList();
        }

        public static List<P3dCAD> ArredondarJuntar(this List<P3dCAD> origem, int decimais_X = 0, int decimais_Y = 0)
        {
            try
            {
                return origem.Select(x => new P3dCAD(Math.Round(x.X, decimais_X), Math.Round(x.Y, decimais_Y), 0)).GroupBy(x => "X: " + x.X + " Y:" + x.Y).Select(x => x.First()).ToList();

            }
            catch (System.Exception)
            {


            }
            return new List<P3dCAD>();
        }
        public static List<P3dCAD> RemoverRepetidos(this List<P3dCAD> pts)
        {
            List<P3dCAD> lista = new List<P3dCAD>();
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                if (i > 0 && lista.Count > 0)
                {
                    var p0 = lista[lista.Count - 1];

                    if (p0.X == p.X && p0.Y == p.Y)
                    {

                    }
                    else
                    {
                        lista.Add(p);
                    }
                }
                else
                {
                    lista.Add(p);
                }
            }

            return lista;
        }
        public static System.Windows.Point Point(this Point2d pt)
        {
            return new System.Windows.Point(pt.X, pt.Y);
        }
        public static double Area(List<P3dCAD> p3DCADs)
        {
            return p3DCADs.Select(x => x.Origem).ToList().Area();
        }
        public static List<P3dCAD> P3dCAD(this List<P3d> lista)
        {
            return lista.Select(x => new P3dCAD(x.X, x.Y, x.Z)).ToList();
        }
        public static List<P3d> P3d(this List<P3dCAD> lista)
        {
            return lista.Select(x => new P3d(x.X, x.Y, x.Z)).ToList();
        }
        public static P3d P3d(this Point3d p)
        {
            return new P3d(p.X, p.Y, p.Z);
        }
        public static P3dCAD P3dCAD(this Point3d p)
        {
            return new P3dCAD(p.X, p.Y, p.Z);
        }
        public static P3d P3d(this Point2d p)
        {
            return new P3d(p.X, p.Y);
        }
        public static System.Windows.Point GetWinPoint(this P3d p3d)
        {
            return new System.Windows.Point(p3d.X, p3d.Y);
        }

        public static List<P3d> P3d(this List<Point3d> p)
        {
            return p.Select(x => new P3d(x.X, x.Y, x.Z)).ToList();
        }


        public static Point3d GetPoint3dCad(this P3d p3d)
        {
            return new Point3d(p3d.X, p3d.Y, p3d.Z);
        }
        public static Point2d GetPoint2dCad(this P3d p3d)
        {
            return new Point2d(p3d.X, p3d.Y);
        }
        public static double DistanciaX(this P3d p3d, Point3d p2)
        {
            return p3d.DistanciaX(new P3dCAD(p2).Origem);
        }

        public static double Distancia(this P3d p1, Point3d p3d)
        {
            return p1.Distancia(new P3dCAD(p3d).Origem);
        }
        public static double GetAngulo(this P3d p1, Point3d p3d)
        {
            return p1.GetAngulo(new P3dCAD(p3d).Origem);
        }
        public static double DistanciaY(this P3d p1, Point3d v)
        {
            return p1.DistanciaY(new P3dCAD(v).Origem);
        }
    }

    public class P3dCAD
    {
        public override string ToString()
        {
            return "[" + Tipo.ToString().PadRight(10, ' ') + "] [" + this.Origem.id.String(3) + "] " + this.Origem.GetCid();
        }

        public double Y
        {
            get
            {
                return Origem.Y;
            }
        }
        public double X
        {
            get
            {
                return Origem.X;
            }
        }
        public double Z
        {
            get
            {
                return Origem.Z;
            }
        }

        public P3d Origem { get; set; } = new P3d();

        public Tipo_Coordenada Tipo { get; set; } = Tipo_Coordenada.Sem;

        public P3dCAD(Point3d pt, int id, Tipo_Coordenada tipo)
        {
            this.Origem.X = pt.X;
            this.Origem.Y = pt.Y;
            this.Origem.Z = pt.Z;
            this.Origem.id = id;
            this.Tipo = tipo;
        }
        public P3dCAD(P3d pt, int id, Tipo_Coordenada tipo)
        {
            this.Origem.X = pt.X;
            this.Origem.Y = pt.Y;
            this.Origem.Z = pt.Z;
            this.Origem.id = id;
            this.Tipo = tipo;
        }
        public P3dCAD(Point3d pt)
        {
            this.Origem.X = pt.X;
            this.Origem.Y = pt.Y;
            this.Origem.Z = pt.Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public P3dCAD(double X, double Y, double Z)
        {

            this.Origem.X = X;
            this.Origem.Y = Y;
            this.Origem.Z = Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public P3dCAD(P3dCAD pt, int arredondar = 1)
        {
            this.Origem.X = Math.Round(pt.X, arredondar);
            this.Origem.Y = Math.Round(pt.Y, arredondar);
            this.Origem.Z = Math.Round(pt.Z, arredondar);

            this.Tipo = pt.Tipo;
        }

        public P3dCAD Round(int decimais)
        {
            return new P3dCAD(this.Origem.Round(decimais), this.Origem.id, this.Tipo);
        }

        public List<P3d> PegarIguaisX()
        {
            return Origem.PegarIguaisX();
            //if (_IguaisX == null)
            //{
            //    _IguaisX = new List<P3dCAD>();
            //    var proxima = this.Proximo;
            //    var atual = this;
            //    if (this.Proximo != null)
            //    {
            //        while (proxima != null && _IguaisX.Count < 10)
            //        {
            //            if (proxima.Getdist_proximaX() == this.Getdist_proximaX() && this.Getdist_proximaX() > 0)
            //            {
            //                if (_IguaisX
            //                    .Find(x => x.Origem.id == proxima.Origem.id) == null
            //                    && proxima.Origem.id == atual.Origem.id + 1
            //                    && proxima.X > atual.X
            //                    && proxima.Origem.GetCid() != this.Origem.GetCid()
            //                    && proxima.Origem.GetCid() != atual.Origem.GetCid())
            //                {
            //                    _IguaisX.Add(proxima);
            //                    atual = proxima;
            //                    proxima = proxima.Proximo;
            //                }
            //                else
            //                {
            //                    proxima = null;
            //                }
            //            }
            //            else
            //            {
            //                proxima = null;
            //            }
            //        }
            //    }
            //    _IguaisX = _IguaisX.FindAll(x => x != null);
            //}


            //return _IguaisX;
        }
        public List<P3d> PegarIguaisY()
        {
            return Origem.PegarIguaisY();
            //if (_IguaisY == null)
            //{
            //    _IguaisY = new List<P3dCAD>();

            //    var proxima = this.Proximo;
            //    var atual = this;
            //    if (this.Proximo != null)
            //    {
            //        while (proxima != null && _IguaisY.Count < 10)
            //        {
            //            if (proxima.GetDist_proximaY() == this.GetDist_proximaY() && this.GetDist_proximaY() > 0)
            //            {
            //                if (_IguaisY
            //                    .Find(x => x.Origem.id == proxima.Origem.id) == null
            //                    && proxima.Y > atual.Y
            //                    && proxima.Origem.id == atual.Origem.id + 1
            //                    && proxima.Origem.GetCid() != this.Origem.GetCid()
            //                    && proxima.Origem.GetCid() != atual.Origem.GetCid())
            //                {
            //                    _IguaisY.Add(proxima);
            //                    atual = proxima;
            //                    proxima = proxima.Proximo;
            //                }
            //                else
            //                {
            //                    proxima = null;
            //                }

            //            }
            //            else
            //            {
            //                proxima = null;
            //            }
            //        }
            //    }
            //    _IguaisY = _IguaisY.FindAll(x => x != null);

            //}


            //return _IguaisY;
        }

        public double Distancia(P3d ponto)
        {
            return this.Origem.Distancia(ponto);
        }

        public P3dCAD(P3d p3D)
        {
            this.Origem.X = p3D.X;
            this.Origem.Y = p3D.Y;
            this.Origem.Z = p3D.Z;
        }
        public P3dCAD()
        {

        }
    }
}
