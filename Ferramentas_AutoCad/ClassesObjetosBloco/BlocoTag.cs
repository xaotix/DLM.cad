using Autodesk.AutoCAD.DatabaseServices;
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
