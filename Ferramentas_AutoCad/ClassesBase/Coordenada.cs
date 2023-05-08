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
            return p3DCADs.Select(x => (P3d)x).ToList().Area();
        }
        public static List<P3dCAD> P3dCAD(this List<P3d> lista)
        {
            return lista.Select(x => new P3dCAD(x.X,x.Y,x.Z)).ToList();
        }
        public static List<P3d> P3d(this List<P3dCAD> lista)
        {
            return lista.Select(x => new P3d(x.X, x.Y, x.Z)).ToList();
        }
        public static Point2d Centro(this Point2d p1, Point2d p2)
        {
            return new Point2d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        public static Point3d Centro(this Point3d p1, Point3d p2)
        {
            return new P3dCAD(p1).Centro(p2).GetPoint3dCad();
        }
        public static P3d P3d(this Point3d p)
        {
            return new P3d(p.X, p.Y,p.Z);
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
            return p.Select(x=> new P3d(x.X, x.Y, x.Z)).ToList();
        }


        public static Point3d GetPoint3dCad(this P3d p3d)
        {
            return new Point3d(p3d.X, p3d.Y, p3d.Z);
        }
        public static Point2d GetPoint2dCad(this P3d p3d)
        {
            return new Point2d(p3d.X, p3d.Y);
        }
        public static double DistanciaX(this P3d p3d,Point3d p2)
        {
            return p3d.DistanciaX(new P3dCAD(p2));
        }

        public static double Distancia(this P3d p1, Point3d p3d)
        {
            return p1.Distancia(new P3dCAD(p3d));
        }
        public static double GetAngulo(this P3d p1, Point3d p3d)
        {
            return p1.GetAngulo(new P3dCAD(p3d));
        }
        public static double DistanciaY(this P3d p1, Point3d v)
        {
            return p1.DistanciaY(new P3dCAD(v));
        }
    }

    public class P3dCAD:P3d
    {
        public override string ToString()
        {
            return "[" + Tipo.ToString().PadRight(10,' ') + "] [" + this.id.ToString().PadLeft(3, '0') + "] " + GetCid();
        }
        public Tipo_Coordenada Tipo { get; set; } = Tipo_Coordenada.Sem;
        public P3dCAD(Point3d pt, int id, Tipo_Coordenada tipo)
        {
            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
            this.id = id;
            this.Tipo = tipo;
        }
        public P3dCAD(P3d pt, int id, Tipo_Coordenada tipo)
        {
            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
            this.id = id;
            this.Tipo = tipo;
        }
        public P3dCAD(Point3d pt)
        {

            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = pt.Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public P3dCAD(double X, double Y, double Z)
        {

            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Tipo = Tipo_Coordenada.Ponto;
        }
        public P3dCAD(P3dCAD pt, int arredondar = 1)
        {
            this.X = Math.Round(pt.X, arredondar);
            this.Y = Math.Round(pt.Y, arredondar);
            this.Z = Math.Round(pt.Z, arredondar);

            this.Tipo = pt.Tipo;
        }

        public P3dCAD Centro(Point3d pt)
        {
            var centro = new P3d(this).Centro(new P3d(pt.X,pt.Y,pt.Z));
            return new P3dCAD(centro);
        }

        public P3d P3d()
        {
            return new P3d(this);
        }

        public P3dCAD(P3d p3D)
        {
            this.X = p3D.X;
            this.Y = p3D.Y;
            this.Z = p3D.Z;
        }
        public P3dCAD()
        {

        }
        public P3dCAD(Point2d pt)
        {
            this.X = pt.X;
            this.Y = pt.Y;
            this.Z = 0;
        }
    }
}
