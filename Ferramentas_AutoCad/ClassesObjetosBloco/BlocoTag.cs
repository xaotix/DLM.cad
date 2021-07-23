using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
   public class BlocoTag: DB.Linha
    {
        private List<Point2d> _contorno { get; set; }

        public List<Point2d> GetContorno(Transaction tr)
        {
            if(_contorno==null)
            {
                _contorno = Ut.GetPontos(this.Bloco, tr).Select(x=>new Point2d(x.X,x.Y)).ToList();
            }
            return _contorno;
        }
        public BlockReference Bloco { get; set; }

        public List<BlocoTag> Filhos { get; set; } = new List<BlocoTag>();

        public double GetAngulo()
        {
            if(Bloco!=null)
            {
                Angulo.RadianosParaGraus(Bloco.Rotation);
            }
            return 0;
        }

        private Coordenada _coordenada { get; set; }
        public Coordenada GetCoordenada()
        {
            if(_coordenada==null)
            {
                if (Bloco == null)
                {
                    return new Coordenada();
                }
                else
                {
                    _coordenada = new Coordenada(this.Bloco.Position, -1, Tipo_Coordenada.Bloco);
                }
            }
            return _coordenada;

        }
        public BlocoTag()
        {

        }

        public BlocoTag(List<Celula> celulas) : base(celulas)
        {
            this.Celulas = celulas;
        }

       public BlocoTag(BlockReference bloco)
        {
            Carregar(bloco);
        }

        public void Carregar(BlockReference bloco)
        {
            this.Bloco = bloco;
            var bl = Atributos.GetBlocoTag(bloco);

            this.Celulas = bl.Celulas;
            this.Tabela = this.Bloco.BlockName;
        }
    }
}
