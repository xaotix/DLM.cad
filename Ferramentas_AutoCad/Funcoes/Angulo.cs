using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using System;
using System.Collections.Generic;

namespace DLM.cad
{
    public static class Angulo
    {
        public static double Get(Xline s)
        {
            return Math.Abs(new DLM.desenho.P3d(0, 0, 0).GetAngulo(new DLM.desenho.P3d(s.UnitDir.X, s.UnitDir.Y, 0)));
        }

        public static bool E_Horizontal(double Radianos)
        {
            if (Radianos == 0 | Radianos == 180)
            {
                return true;
            }
            else if (Utilz.RadianosParaGraus(Radianos) == 0 | Utilz.RadianosParaGraus(Radianos) == 180)
            {
                return true;
            }
            return false;
        }
        public static bool E_Vertical(double Angulo)
        {
            if (Angulo == 90 | Angulo == 270)
            {
                return true;
            }
            else if (Utilz.RadianosParaGraus(Angulo) == 90 | Utilz.RadianosParaGraus(Angulo) == 270)
            {
                return true;
            }
            return false;
        }
        public static double GetSegmentos(LineSegment3d l1, LineSegment3d l2, Vector3d normal)
        {
            Vector3d v1 = l1.EndPoint - l1.StartPoint;
            Vector3d v2 = l2.EndPoint - l2.StartPoint;



            var rad_angulo = v1.GetAngleTo(v2, normal);

            if (rad_angulo > Math.PI)
            {
                rad_angulo = (rad_angulo - Math.PI * 2.0);
            }



            var angulo = Utilz.RadianosParaGraus(rad_angulo, 0);
            angulo = 180 + angulo;
            return angulo;
        }
        public static List<double> GetAngulos(Polyline pl, out List<LineSegment3d> segmentos3d)
        {

            List<double> retorno = new List<double>();
            segmentos3d = Ut.GetSegmentos3D(pl);

            if (segmentos3d.Count > 1)
            {

                for (int i = 1; i < segmentos3d.Count; i++)
                {
                    retorno.Add(GetSegmentos(segmentos3d[i], segmentos3d[i - 1], pl.Normal));
                }
            }



            return retorno;
        }
        public static double Normalizar(double angulo)
        {
            if (angulo < 0)
            {
                angulo = 360 + angulo;
            }
            if (angulo >= 0 && angulo < 45)
            {
                return 0;
            }
            else if (angulo >= 45 && angulo <= 90)
            {
                return 90;
            }
            else if (angulo > 90 && angulo < 135)
            {
                return 90;
            }
            else if (angulo >= 135 && angulo <= 180)
            {
                return 180;
            }
            else if (angulo > 180 && angulo < 225)
            {
                return 180;
            }
            else if (angulo >= 225 && angulo < 270)
            {
                return 270;
            }
            else if (angulo >= 270)
            {
                return 0;
            }
            return 0;

        }
    }
}