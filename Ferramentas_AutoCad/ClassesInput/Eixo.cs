using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DLM.desenho;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DLM.cad
{
    public class Eixo
    {
        public P3d Origem
        {
            get
            {
                return new P3d(MinX, MinY);
            }
        }
        public double MinX
        {
            get
            {
                if (Linha == null)
                {
                    return 0;
                }
                return Linha.StartPoint.X < Linha.EndPoint.X ? Linha.StartPoint.X : Linha.EndPoint.X;
            }
        }
        public double MaxX
        {
            get
            {
                if (Linha == null)
                {
                    return 0;
                }
                return Linha.StartPoint.X > Linha.EndPoint.X ? Linha.StartPoint.X : Linha.EndPoint.X;
            }
        }
        public double MinY
        {
            get
            {
                if (Linha == null)
                {
                    return 0;
                }
                return Linha.StartPoint.Y < Linha.EndPoint.Y ? Linha.StartPoint.Y : Linha.EndPoint.Y;
            }
        }
        public double MaxY
        {
            get
            {
                if (Linha == null)
                {
                    return 0;
                }
                return Linha.StartPoint.Y>Linha.EndPoint.Y?Linha.StartPoint.Y:Linha.EndPoint.Y;
            }
        }
        public double Z
        {
            get
            {
                return 0;
            }
        }




        public Line GetLinhaEixo(GradeEixos grade)
        {
            Line p = new Line();
            p.StartPoint = new Point3d(this.MinX, grade.GetYmax(), 0);
            p.EndPoint = new Point3d(this.MinX, grade.GetYmin(), 0);
            return p;
        }

        public BlockAttributes Bloco { get; private set; }
        public CADLine Linha { get; private set; }
        public override string ToString()
        {
            return $"[{Nome} - {Sentido}]";
        }
        public Sentido Sentido { get; private set; } = Sentido.Horizontal;
        public string Nome { get; private set; } = "A";
        public double Vao { get; private set; } = 0;
        public Eixo()
        {

        }
        public Eixo(Sentido sentido,string Nome, double Vao)
        {
            this.Sentido = sentido;
            this.Nome = Nome;
            this.Vao = Vao;
        }
        public Eixo(Sentido sentido, BlockAttributes bloco, CADLine linha, double Vao)
        {
            this.Sentido = sentido;
            this.Vao = Vao;
            this.Bloco = bloco;
            this.Linha = linha;
           

            Nome = "";
            if (bloco != null)
            {

                var nomes = this.Bloco.Celulas.FindAll(x => x.Coluna.ToUpper().Contains("EIXO")).Select(x => x.Valor).Distinct().ToList().FindAll(x => x.Replace(" ", "") != "").ToList();

                if (nomes.Count > 0)
                {
                    Nome = nomes[0];
                }
                if (Nome == "") { Nome = this.Bloco.Get("Nome").Valor; };
                var preenchidos = this.Bloco.Celulas.FindAll(x => x.Valor.Replace(" ", "") != "");
                if (Nome == "" && preenchidos.Count > 0)
                {
                    Nome = preenchidos[0].Valor;
                }
            }

            if (Nome == "")
            {
                Nome = "???";
            }



        }
    }
}
