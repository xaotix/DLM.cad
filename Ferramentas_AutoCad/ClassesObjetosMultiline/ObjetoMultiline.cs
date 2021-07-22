﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Windows.Media;

namespace Ferramentas_DLM
{
    public class ObjetoMultiline
    {
        
        public System.Windows.Shapes.Line GetCanvas(System.Windows.Point p0, double escala, double espessura, SolidColorBrush cor)
        {
            var p1 = new System.Windows.Point((this.Inicio.X - p0.X) * escala, (this.Inicio.Y - p0.Y) * escala);
            var p2 = new System.Windows.Point((this.Fim.X - p0.X) * escala, (this.Fim.Y - p0.Y) * escala);
            var l = Conexoes.FuncoesCanvas.Linha(p1, p2, cor, 0, Conexoes.FuncoesCanvas.TipoLinha.Continua, espessura);

            return l;
        }
        public Tipo_Multiline Tipo { get; private set; } = Tipo_Multiline.Desconhecido;
        public double Largura { get; private set; } = 0;
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return "p0: " + Inicio + " / p1: " + Fim + " comprimento: " + Comprimento + " angulo: " + Angulo;
        }
        public Mline Mline { get; private set; }
        public double Comprimento { get; private set; } = 0;
        public Coordenada Inicio { get; private set; }
        public Coordenada Fim { get; private set; }
        public Coordenada Centro { get; private set; }

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
        public double Angulo { get; set; } = 0;
        public ObjetoMultiline()
        {

        }
        public ObjetoMultiline(Mline l, Tipo_Multiline tipo)
        {
            this.Tipo = tipo;
            Point3d p0, p1, centro;
            double comprimento, angulo,largura;
            Utilidades.GetCoordenadas(l, out p0, out p1, out angulo, out comprimento, out centro, out largura);
            this.Angulo = angulo;
            this.Centro = new Coordenada(centro);
            this.Comprimento = Math.Round(comprimento);
            this.Mline = l;
            this.Inicio = new Coordenada(p0);
            this.Fim = new Coordenada(p1);
            this.Largura = largura;
        }
    }
}
