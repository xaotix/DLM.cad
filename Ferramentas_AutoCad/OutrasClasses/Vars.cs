using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public static class Vars
    {
        public static string Raiz { get; set; } = @"R:\Blocos\SELO A2\";
        public static string Correntes_Titulo { get; set; } = Raiz + @"Listagem\CORRENTES_TITULO.dwg";
        public static string Correntes { get; set; } = Raiz + @"Listagem\CORRENTES.dwg";
        public static string Tercas_Titulo { get; set; } = Raiz + @"Listagem\TERCAS_TITULO.dwg";

        public static string Almox_Titulo { get; set; } = Raiz + @"Listagem\ALMOX_TITULO.dwg";
        public static string Almox { get; set; } = Raiz + @"Listagem\ALMOX.dwg";
        public static string Tirantes_Titulo { get; set; } = Raiz + @"Listagem\TIRANTES_TITULO.dwg";
        public static string Tirantes { get; set; } = Raiz + @"Listagem\TIRANTES.dwg";
        public static string Tercas_Indicacao { get; set; } = Raiz + @"Listagem\TERCA_INDICACAO.dwg";
        public static string Tirantes_Indicacao { get; set; } = Raiz + @"Listagem\TIRANTE_INDICACAO.dwg";
        public static string Correntes_Indicacao { get; set; } = Raiz + @"Listagem\CORRENTE_INDICACAO.dwg";
        public static string Tercas { get; set; } = Raiz + @"Listagem\TERCAS.dwg";
        public static string Texto { get; set; } = Raiz + @"Listagem\TEXTO.dwg";

        public static class Pecas
        {
            public static string RaizPcs { get; set; } = @"R:\Blocos\SELO A2\Listagem\Peças Mapeáveis\";
            public static string ESTICADOR { get; set; } = RaizPcs + @"ESTICADOR.dwg";
            public static string MANILHA { get; set; } = RaizPcs + @"MANILHA.dwg";
            public static string PASSARELA { get; set; } = RaizPcs + @"PASSARELA.dwg";
            public static string SFLH { get; set; } = RaizPcs + @"SFLH.dwg";
            public static string SFLI { get; set; } = RaizPcs + @"SFLI.dwg";
        }
    }
}
