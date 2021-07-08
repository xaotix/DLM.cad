using Autodesk.AutoCAD.DatabaseServices;
using DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
   public class BlocoTags: DB.Linha
    {
        public BlockReference Bloco { get; set; }
        public BlocoTags()
        {

        }

        public List<BlocoTags> Filhos { get; set; } = new List<BlocoTags>();

        public double GetAngulo()
        {
            if(Bloco!=null)
            {
                Angulo.RadianosParaGraus(Bloco.Rotation);
            }
            return 0;
        }
        public Coordenada GetCoordenada()
        {
            if(Bloco==null)
            {
                return new Coordenada();
            }
            else
            {
                return new Coordenada(this.Bloco.Position, -1, Tipo_Coordenada.Bloco);
            }
        }

        public BlocoTags(List<Celula> celulas) : base(celulas)
        {
            this.Celulas = celulas;
        }
    }
}
