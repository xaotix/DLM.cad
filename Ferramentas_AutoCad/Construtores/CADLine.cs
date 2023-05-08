using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DLM.cad
{
    public  class CADLine
    {
        public P3d Min
        {
            get
            {
                return new P3d(MinX, MinY);
            }
        }
        public P3d Max
        {
            get
            {
                return new P3d(MaxX, MaxY);
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
        public P3d StartPoint { get; private set; }
        public P3d EndPoint { get; private set; }
        public double Comprimento { get; private set; }
        public Line Line { get; private set; } 
        public CADLine(Line L)
        {
            this.Line = L;
            this.Comprimento = Math.Round(this.Line.Length);
            this.StartPoint = L.StartPoint.P3d();
            this.EndPoint  = L.EndPoint.P3d();
            this.Layer = L.Layer;
            this.Linetype = L.Linetype;
            this.Angulo = L.Angle.RadianosParaGraus();
            this.ObjectId = L.ObjectId;
        }
    }
}
