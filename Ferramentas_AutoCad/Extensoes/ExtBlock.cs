using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Windows;

namespace DLM.cad
{
    public static class ExtBlock
    {
        public static List<BlockReference> Filtrar(this List<BlockReference> blocos, List<string> nomes, bool exato = true)
        {
            List<BlockReference> marcas = new List<BlockReference>();

            foreach (var b in blocos)
            {
                try
                {
                    var nome = b.Name.ToUpper();
                    foreach (var s in nomes)
                    {
                        if (exato)
                        {
                            if (nome.ToUpper() == s.ToUpper())
                            {
                                marcas.Add(b);
                                break;
                            }
                        }
                        else
                        {
                            if (nome.ToUpper().Contains(s.ToUpper()))
                            {
                                marcas.Add(b);
                                break;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex, $"Erro ao tentar ler um bloco.");
                }

            }

            return marcas;
        }
    }
}
