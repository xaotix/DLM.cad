using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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


        public List<UIElement> GetCanvas(System.Windows.Point p0, double escala ,double espessura, double raio, double tam_texto)
        {
            List<UIElement> retorno = new List<UIElement>();
            /*linha do eixo*/
            var p1 = new Point((this.Linha.StartPoint.X - p0.X) * escala, (this.Linha.StartPoint.Y - p0.Y) * escala);
            var p2 = new Point((this.Linha.EndPoint.X - p0.X) * escala, (this.Linha.EndPoint.Y - p0.Y) * escala);
            var l = Conexoes.FuncoesCanvas.Linha(p1, p2, Conexoes.FuncoesCanvas.Cores.Magenta, 0, Conexoes.FuncoesCanvas.TipoLinha.Traco_Ponto, espessura);
            retorno.Add(l);

            /*bolota do eixo*/
            var centro_circulo = new Point((Xmax - p0.X) *escala, (Ymax - p0.Y + raio) * escala);
            var c = Conexoes.FuncoesCanvas.Circulo(centro_circulo, raio, espessura, Conexoes.FuncoesCanvas.Cores.Red);
            retorno.Add(c);

            var ptexto = Conexoes.FuncoesCanvas.Label(this.Nome, centro_circulo, Conexoes.FuncoesCanvas.Cores.Cyan,  tam_texto );
            retorno.Add(ptexto);
            return retorno;
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
