using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace DLM.cad
{
    public class MlClass
    {
        public override string ToString()
        {
            return $"{this.Nome} [{this.Mlines.Count}x]";
        }
        public string Nome { get; set; } = "";
        public Tipo_Multiline Tipo { get; set; } = Tipo_Multiline.Desconhecido;
        [Browsable(false)]
        public List<CADMline> Mlines { get; set; } = new List<CADMline>();
        public MlClass(string nome, List<CADMline> mlines)
        {
            this.Nome = nome;
            this.Mlines = mlines;
        }
        public MlClass()
        {

        }
    }
    public class CADMline
    {
        private MlineStyle _mlstyle { get; set; }
        public MlineStyle GetStyle()
        {
            if (_mlstyle == null)
            {
                _mlstyle = Multiline.GetMlStyle(this.Mline.Style);
            }
            return _mlstyle;
        }
        public List<Point2d> GetInterSeccao(Entity line)
        {
            Point3dCollection pts = new Point3dCollection();
            this.GetPLineDummy().IntersectWith(line, Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendBoth, pts, new IntPtr(), new IntPtr());

            List<Point2d> ptss = new List<Point2d>();

            foreach (Point3d p in pts)
            {
                ptss.Add(new Point2d(p.X, p.Y));
            }
            return ptss;
        }

        public Tipo_Multiline Tipo { get; private set; } = Tipo_Multiline.Desconsiderar;
        public double Largura { get; private set; } = 0;
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return $"{Comprimento}X{Largura}X{Angulo}° St: {this.GetStyle().Name}";
        }
        public Mline Mline { get; private set; }
        public double Comprimento { get; private set; } = 0;
        public P3d Inicio { get; private set; }
        public P3d Fim { get; private set; }
        public P3d Centro { get; private set; }

        public Polyline GetPLineDummy()
        {
            Polyline ps = new Polyline();
            var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            for (int i = 0; i < this.Pontos.Count; i++)
            {

                ps.AddVertexAt(i, this.Pontos[i].Convert2d(plane), 0.0, 0.0, 0.0);
            }



            return ps;
        }

        public double MaxX { get; private set; } = 0;
        public double MaxY { get; private set; } = 0;
        public double MinY { get; private set; } = 0;
        public double MinX { get; private set; } = 0;
        public double Angulo { get; private set; } = 0;
        public Sentido Sentido { get; private set; } = Sentido.Inclinado;
        public List<Point3d> Pontos { get; set; } = new List<Point3d>();
        public CADMline()
        {

        }
        public CADMline(Mline objeto, Tipo_Multiline tipo)
        {
            this.Tipo = tipo;
            P3d p0, p1, centro;
            double comprimento, angulo, largura;
            Ut.GetCoordenadas(objeto, out p0, out p1, out angulo, out comprimento, out centro, out largura);
            this.Angulo = angulo;
            this.Centro = centro;
            this.Comprimento = comprimento.Round(0);
            this.Mline = objeto;
            this.Inicio = p0;
            this.Fim = p1;
            this.Largura = largura.Round(0);

            this.Pontos = Ut.GetPontos(this.Mline);

            this.MinX = Inicio.X < Fim.X ? Inicio.X : Fim.X;
            this.MinY = Inicio.Y < Fim.Y ? Inicio.Y : Fim.Y;

            this.MaxY = Inicio.Y > Fim.Y ? Inicio.Y : Fim.Y;
            this.MaxX = Inicio.X > Fim.X ? Inicio.X : Fim.X;

            this.Angulo = this.Angulo.Normalizar(360);

            if (this.Angulo >= 89 && this.Angulo <= 91)
            {
                this.Sentido = Sentido.Vertical;
            }
            else if ((this.Angulo >= -1 && this.Angulo <= 1) | (this.Angulo >= 179 && this.Angulo <= 181))
            {
                this.Sentido = Sentido.Horizontal;
            }
            else
            {
                this.Sentido = Sentido.Inclinado;
            }
        }
    }
}
