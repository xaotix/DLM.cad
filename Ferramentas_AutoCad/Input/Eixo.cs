using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
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
       

        public BlockReference Bloco { get; set; }
        public Line Linha { get; set; }
        public override string ToString()
        {
            return $"Eixo {Nome} - [{Sentido}] - Vão: {Vao}";
        }
        public Sentido Sentido { get; set; } = Sentido.Horizontal;
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
    }
    public class GradeEixos
    {
        

        public override string ToString()
        {
            return $"Grade Eixos {GetComprimento()}x{GetLargura()}";
        }
        public double Nivel { get; set; } = 0;
        public double GetComprimento()
        {
            if (this.GetEixosHorizontais().Count == 0) { return 0; }
            return this.GetEixosHorizontais().Sum(x => x.Vao);
        }
        public double GetLargura()
        {
            if (this.GetEixosVerticais().Count == 0) { return 0; }
            return this.GetEixosVerticais().Sum(x => x.Vao);
        }
        public TipoVista Vista { get; private set; } = TipoVista.Planta;
        private List<Eixo> _eixos { get; set; } = new List<Eixo>();
        public List<Eixo> GetEixosVerticais()
        {
            return _eixos.FindAll(x=>x.Sentido== Sentido.Vertical);
        }
        public List<Eixo> GetEixosHorizontais()
        {
            return _eixos.FindAll(x => x.Sentido == Sentido.Horizontal);
        }
        public void Add(Sentido Sentido, string Nome, double Vao)
        {
            if(Nome.Replace(" ","") == "") { return; }
            if(_eixos.Find(x=>x.Nome.ToUpper().Replace(" ","") == Nome.ToUpper().Replace(" ","")) != null) { return; }
            if(Vao>0)
            {
                _eixos.Add(new Eixo(Sentido, Nome, Vao));
            }
        }
        public void Add(Sentido Sentido, double Vao, BlockReference bloco, Line line)
        {

            string Nome = "?";
            if(bloco!=null)
            {
                var atributos = Atributos.GetLinha(bloco);
                Nome = atributos.Get("Eixo").valor;
                if (Nome == "") { Nome = atributos.Get("Nome").valor; };
                var preenchidos = atributos.Celulas.FindAll(x => x.Valor.Replace(" ", "") != "");
                if (Nome == "" && preenchidos.Count > 0)
                {
                    Nome = preenchidos[0].Valor;
                }
            }
       
            var neixo = new Eixo(Sentido, Nome, Vao);

            neixo.Bloco = bloco;
            neixo.Linha = line;
            
            _eixos.Add(neixo);
        }
        public GradeEixos()
        {

        }
    }
}
