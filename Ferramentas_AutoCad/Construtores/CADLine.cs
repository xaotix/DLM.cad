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
        public override string ToString()
        {
            return $"[{Layer}] - {Comprimento} [{Angulo.Round(1)}°]";
        }

        public P3d Min { get; set; } = new P3d();
        public P3d Max { get; set; } = new P3d();
        public Sentido Sentido { get; private set; } = Sentido.Inclinado;
        public double Angulo { get; private set; }
        public ObjectId ObjectId { get; private set; }
        public string Layer { get; private set; }
        public string Linetype { get; private set; }
        public P3d P1 { get; private set; }
        public P3d P2 { get; private set; }
        public double Comprimento { get; private set; }
        public Line Line { get; private set; }

        public CADLine(Line linha)
        {
            this.Line = linha;
            this.Comprimento = Math.Round(this.Line.Length);
            this.P1 = linha.StartPoint.P3d();
            this.P2  = linha.EndPoint.P3d();
            this.Layer = linha.Layer;
            this.Linetype = linha.Linetype;
            this.Angulo = linha.Angle.RadianosParaGraus();
            this.ObjectId = linha.ObjectId;


            this.Max.X = P1.X > P2.X ? P1.X : P2.X;
            this.Max.Y = P1.Y > P2.Y ? P1.Y : P2.Y;

            this.Min.X = P1.X < P2.X ? P1.X : P2.X;
            this.Min.Y = P1.Y < P2.Y ? P1.Y : P2.Y;

            var ang = this.Angulo.Normalizar(360);
            if (ang == 0 | ang == 180 | ang == 360)
            {
                this.Sentido = Sentido.Horizontal;
            }
            else if (ang == 90 | ang == 270)
            {
                this.Sentido = Sentido.Vertical;
            }
            else
            {
                this.Sentido = Sentido.Inclinado;
            }
        }
    }
}
