using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public class BlockAttributes
    {
        public override string ToString()
        {
            return this.Descricao;
        }
        public string Descricao { get; set; } = "";
        public BlockReference Block { get; private set; }
        public void Set(string coluna, string valor)
        {
            var t = this.Attributes.Find(x => x.Coluna.ToUpper() == coluna.ToUpper());
            if (t != null)
            {
                t.Set(valor);
            }
            else
            {
                this.Attributes.Add(new db.Celula(coluna, valor));
            }

        }

        public db.Celula Get(string Coluna)
        {
            var s = Attributes.FindAll(x => x != null).Find(x => x.Coluna.ToUpper() == Coluna.ToUpper());
            if (s != null)
            {
                return s;

            }
            else
            {
                return new db.Celula(Coluna, "");
            }

        }
        public List<string> GetColunas()
        {
            return Attributes.Select(x => x.Coluna).ToList();
        }
        public List<db.Celula> Attributes { get; set; } = new List<db.Celula>();
        public BlockAttributes Clonar()
        {
            BlockAttributes retorno = new BlockAttributes(this.Block, false);
            retorno.Descricao = this.Descricao;
            retorno.Attributes.Clear();
            foreach (var c in this.Attributes)
            {
                retorno.Attributes.Add(new db.Celula(c.Coluna, c.Valor));
            }
            return retorno;
        }
        private List<Point2d> _contorno { get; set; }

        public List<Point2d> GetPontos(Transaction acTrans)
        {
            if(_contorno==null)
            {
                _contorno = Ut.GetPontos(this.Block, acTrans).Select(x=>new Point2d(x.X,x.Y)).ToList();
            }
            return _contorno;
        }

        public List<BlockAttributes> Filhos { get; set; } = new List<BlockAttributes>();

        public double GetAngulo()
        {
            if(Block!=null)
            {
                Angulo.RadianosParaGraus(Block.Rotation);
            }
            return 0;
        }

        private P3dCAD _coordenada { get; set; }
        public P3dCAD GetCoordenada()
        {
            if(_coordenada==null)
            {
                if (Block == null)
                {
                    return new P3dCAD();
                }
                else
                {
                    _coordenada = new P3dCAD(this.Block.Position, -1, Tipo_Coordenada.Bloco);
                }
            }
            return _coordenada;
        }

        public db.Linha GetLinha()
        {
            return new DLM.db.Linha(this.Attributes.Select(x => new DLM.db.Celula(x.Coluna, x.Valor)).ToList());
        }

        public BlockAttributes(List<db.Celula> atributos)
        {
            this.Attributes = atributos;
        }

        public BlockAttributes(BlockReference bloco, bool carregar = true)
        {
            this.Block = bloco;


            if (carregar)
            {
                var bl = bloco.GetAttributes();

                this.Attributes =new List<db.Celula>();
                this.Attributes.AddRange(bl.Attributes);
            }
        }

    }
}
