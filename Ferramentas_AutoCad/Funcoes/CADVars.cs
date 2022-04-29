using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DLM.cad
{
    internal static class FuncoesCAD
    {
        private static List<CTV_de_para> _cts { get; set; }
        public static List<CTV_de_para> CTVs()
        {
            if (_cts == null)
            {
                _cts = new List<CTV_de_para>();
                var arq = DLM.vars.Cfg.Init.CAD_Arquivo_CTV;
                var linhas = Conexoes.Utilz.Arquivo.Ler(arq);
                if (linhas.Count > 1)
                {
                    for (int i = 1; i < linhas.Count; i++)
                    {
                        var col = linhas[i].Split(';').ToList();
                        if (col.Count >= 5)
                        {
                            _cts.Add(
                                new CTV_de_para(
                                col[0],
                                col[2],
                                Conexoes.Utilz.Double(col[1]),
                                Conexoes.Utilz.Int(col[3]),
                                col[4]
                                )
                                );
                        }
                    }
                }
            }
            return _cts;
        }

        private static EstilosML _estilosML { get; set; }
        public static EstilosML GetArquivosMlStyles()
        {
            if(_estilosML==null)
            {
                _estilosML = new EstilosML();
            }
            return _estilosML;
        }

    }
}
