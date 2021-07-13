using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class Eixo
    {
        public Point3d Origem
        {
            get
            {
                return new Point3d(Xmin, Ymin, 0);
            }
        }
        public double Xmin
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
        public double Xmax
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
        public double Ymin
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
        public double Ymax
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





        public BlockReference Bloco { get; private set; }
        public Line Linha { get; private set; }
        public override string ToString()
        {
            return $"Eixo {Nome} - [{Sentido}] - Vão: {Vao}";
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
        public Eixo(Sentido sentido, BlockReference bloco, Line linha, double Vao)
        {
            this.Sentido = sentido;
            this.Vao = Vao;
            this.Bloco = bloco;
            this.Linha = linha;
           

            Nome = "";
            if (bloco != null)
            {
                var atributos = Atributos.GetLinha(this.Bloco);
                var nomes = atributos.Celulas.FindAll(x => x.Coluna.ToUpper().Contains("EIXO")).Select(x => x.Valor).Distinct().ToList().FindAll(x => x.Replace(" ", "") != "").ToList();

                if (nomes.Count > 0)
                {
                    Nome = nomes[0];
                }
                if (Nome == "") { Nome = atributos.Get("Nome").valor; };
                var preenchidos = atributos.Celulas.FindAll(x => x.Valor.Replace(" ", "") != "");
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
