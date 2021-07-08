using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
   public static class Linha
    {
        public static List<Line> GetLinhas(List<Line> linhas, double DistMinEntreLinhas, Sentido Sentido)
        {
            List<Line> Retorno = new List<Line>();
          
            if (linhas.Count > 1)
            {

                for (int i = 0; i < linhas.Count; i++)
                {
                    if (i == 0)
                    {
                        Retorno.Add(linhas[i]);
                    }
                    else
                    {
                        double dist = 0;
                        if(Sentido == Sentido.Vertical)
                        {
                            dist = Math.Abs(linhas[i].StartPoint.X - Retorno.Last().StartPoint.X);
                        }
                        else
                        {
                            dist = Math.Abs(linhas[i].StartPoint.Y - Retorno.Last().StartPoint.Y);
                        }
                        if (dist >= DistMinEntreLinhas)
                        {
                            Retorno.Add(linhas[i]);
                        }
                        else
                        {

                        }
                    }
                }
            }
            return Retorno;
        }
    }
}
