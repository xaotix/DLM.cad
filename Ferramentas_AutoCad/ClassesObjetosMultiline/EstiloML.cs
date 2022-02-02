using DLM.vars;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
   public class EstiloML
    {
        public override string ToString()
        {
            return Conexoes.Utilz.getNome(this.Arquivo);
        }
        public List<string> Estilos { get; set; } = new List<string>();
        public string Arquivo { get; set; } 
        public EstiloML(string arquivo)
        {
            this.Arquivo = arquivo;


            if (File.Exists(arquivo))
            {
                var linhas = Conexoes.Utilz.Arquivo.Ler(arquivo);

                for (int i = 0; i < linhas.Count; i++)
                {
                    if (linhas[i] == "2" && i < linhas.Count - 2)
                    {
                        Estilos.Add(linhas[i + 1].TrimStart().TrimEnd());
                    }
                }
            }
        }


    }
    public class EstilosML
    {
        public List<EstiloML> Arquivos { get; set; } = new List<EstiloML>();

        public List<string> GetEstilos()
        {
            return Arquivos.SelectMany(x => x.Estilos).Distinct().ToList().OrderBy(x => x).ToList();
        }

        public EstiloML GetEstilo(string nome)
        {
            var s = Arquivos.Find(x => x.Estilos.Find(y => y == nome) != null);
            if (s != null)
            {
                return s;
            }
            return new EstiloML("");
        }
        public EstilosML()
        {
            this.Arquivos = Conexoes.Utilz.GetArquivos(CADVars.Raiz_MlStyles, "*.mln").Select(x => new EstiloML(x)).ToList();
        }
    }
}
