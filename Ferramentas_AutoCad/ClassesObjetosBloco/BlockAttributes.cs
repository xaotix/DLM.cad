using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public class BlockAttributes : db.Linha
    {
        public override string ToString()
        {
            return this.Descricao;
        }
        public BlockReference Block { get; private set; }

        public new  BlockAttributes Clonar()
        {
            BlockAttributes retorno = new BlockAttributes(this.Block, false);
           
            retorno.Descricao = this.Descricao;
            retorno.Celulas.Clear();
            foreach (var c in this.Celulas)
            {
                retorno.Celulas.Add(new db.Celula(c.Coluna, c.Valor));
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

        public BlockAttributes(List<db.Celula> atributos)
        {
            this.Celulas = atributos;
        }

        public BlockAttributes(BlockReference bloco, bool carregar = true)
        {
            this.Block = bloco;

            
            if (carregar)
            {
                var bl = bloco.GetAttributes();

                this.Celulas =new List<db.Celula>();
                this.Celulas.AddRange(bl.Celulas);
            }
        }

    }
}
