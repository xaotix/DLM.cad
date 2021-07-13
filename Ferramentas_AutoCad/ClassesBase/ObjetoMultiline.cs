using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Ferramentas_DLM
{
    public class ObjetoMultiline
    {
        public Tipo_Multiline Tipo { get; private set; } = Tipo_Multiline.Desconhecido;
        public double Largura { get; private set; } = 0;
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return "p0: " + Inicio + " / p1: " + Fim + " comprimento: " + comprimento + " angulo: " + angulo;
        }
        public Mline mline { get; private set; }
        public double comprimento { get; private set; } = 0;
        public Coordenada Inicio { get; private set; }
        public Coordenada Fim { get; private set; }
        public Coordenada centro { get; private set; }

        public double maxx
        {
            get
            {
                return Inicio.X > Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double maxy
        {
            get
            {
                return Inicio.Y > Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double miny
        {
            get
            {
                return Inicio.Y < Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double minx
        {
            get
            {
                return Inicio.X < Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double angulo { get; set; }
        public ObjetoMultiline()
        {

        }
        public ObjetoMultiline(Mline l, Tipo_Multiline tipo)
        {
            this.Tipo = tipo;
            Point3d p0, p1, centro;
            double comprimento, angulo,largura;
            Utilidades.GetCoordenadas(l, out p0, out p1, out angulo, out comprimento, out centro, out largura);
            this.angulo = angulo;
            this.centro = new Coordenada(centro);
            this.comprimento = comprimento;
            this.mline = l;
            this.Inicio = new Coordenada(p0);
            this.Fim = new Coordenada(p1);
            this.Largura = largura;
        }
    }
}
