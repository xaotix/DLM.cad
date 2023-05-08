using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.cad;
using DLM.desenho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public class PolyInfo
    {
        public override string ToString()
        {
            return $"L={this.Perimetro} [{X0.ToString()}]";
        }
        public Polyline Polyline { get; private set; }
        public double Perimetro { get; private set; } = 0;
        public double Comprimento { get; private set; } = 0;
        public double Largura { get; private set; } = 0;
        public bool Closed { get; private set; } = false;
        public double Area { get; private set; } = 0;

        public P3d X0
        {
            get
            {
                return this.Pontos.X0();
            }
        }

        public bool SomenteLinhas { get; private set; } = false;
        public string Layer { get; private set; } = "";

        public List<P3d> Pontos { get; set; } = new List<P3d>();
        public List<object> Objetos { get; set; } = new List<object>();

        public List<CircularArc3d> Arcos => Objetos.FindAll(x => x is CircularArc3d).Select(x => x as CircularArc3d).ToList();
        public List<LineSegment3d> Linhas => Objetos.FindAll(x => x is LineSegment3d).Select(x => x as LineSegment3d).ToList();

        public PolyInfo(Polyline polyline)
        {
            this.Polyline = polyline;
            this.Pontos = Ut.GetPontos(polyline).P3d();
            this.Comprimento = this.Pontos.Comprimento();
            this.Largura = this.Pontos.Largura();
            this.Area = this.Polyline.Area;
            this.Perimetro = this.Polyline.Length;
            this.Closed = this.Polyline.Closed;

            this.SomenteLinhas = this.Polyline.IsOnlyLines;
            this.Layer = this.Polyline.Layer;

            for (int i = 0; i < this.Polyline.NumberOfVertices; i++)
            {
                var tipo = this.Polyline.GetSegmentType(i);
                switch (tipo)
                {
                    case SegmentType.Empty:
                        break;
                    case SegmentType.Point:
                        this.Objetos.Add(this.Polyline.GetPoint3dAt(i));
                        break;
                    case SegmentType.Coincident:
                        break;
                    case SegmentType.Arc:
                        this.Objetos.Add(this.Polyline.GetArcSegmentAt(i));
                        break;
                    case SegmentType.Line:
                        this.Objetos.Add(this.Polyline.GetLineSegmentAt(i));
                        break;
                }

            }
        }
    }
}
