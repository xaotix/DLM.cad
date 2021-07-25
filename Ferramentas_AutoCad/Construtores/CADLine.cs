using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ferramentas_DLM
{
    public  class CADLine
    {
        public Point2d Min
        {
            get
            {
                return new Point2d(MinX, MinY);
            }
        }
        public Point2d Max
        {
            get
            {
                return new Point2d(MaxX, MaxY);
            }
        }
        public double MinY
        {
            get
            {
                return StartPoint.Y < EndPoint .Y ? StartPoint.Y : EndPoint .Y;
            }
        }
        public double MaxY
        {
            get
            {
                return StartPoint.Y > EndPoint .Y ? StartPoint.Y : EndPoint .Y;
            }
        }
        public double MinX
        {
            get
            {
                return StartPoint.X < EndPoint .X ? StartPoint.X : EndPoint .X;
            }
        }
        public double MaxX
        {
            get
            {
                return StartPoint.X > EndPoint .X ? StartPoint.X : EndPoint .X;
            }
        }

        public override string ToString()
        {
            return $"[{Layer}] - {Comprimento} [{Math.Round(Angulo)}°]";
        }


        public Sentido Sentido
        {
            get
            {
                var ang = Math.Round(Math.Abs(this.Angulo));
                if (ang == 0 | ang ==180 | ang == 360)
                {
                    return Sentido.Horizontal;
                }
                else if(ang == 90 | ang == 270)
                {
                    return Sentido.Vertical;
                }
                return Sentido.Inclinado;
            }
        }


        public double Angulo { get; private set; }
        public ObjectId ObjectId { get; private set; }
        public string Layer { get; private set; }
        public string Linetype { get; private set; }
        public Point3d StartPoint { get; private set; }
        public Point3d EndPoint { get; private set; }
        public double Comprimento { get; private set; }
        public Line Line { get; private set; } 
        public CADLine(Line L)
        {
            this.Line = L;
            this.Comprimento = Math.Round(this.Line.Length);
            this.StartPoint = L.StartPoint;
            this.EndPoint  = L.EndPoint;
            this.Layer = L.Layer;
            this.Linetype = L.Linetype;
            this.Angulo = Ferramentas_DLM.Angulo.RadianosParaGraus(L.Angle);
            this.ObjectId = L.ObjectId;
        }
    }
}
