using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public static class Vars
    {
        public static string RaizBlocos { get; set; } = @"R:\Blocos\SELO A2\";
        public static string Correntes_Titulo { get; set; } = RaizBlocos + @"Listagem\CORRENTES_TITULO.dwg";
        public static string Correntes { get; set; } = RaizBlocos + @"Listagem\CORRENTES.dwg";
        public static string Tercas_Titulo { get; set; } = RaizBlocos + @"Listagem\TERCAS_TITULO.dwg";

        public static string Almox_Titulo { get; set; } = RaizBlocos + @"Listagem\ALMOX_TITULO.dwg";
        public static string Almox { get; set; } = RaizBlocos + @"Listagem\ALMOX.dwg";
        public static string Tirantes_Titulo { get; set; } = RaizBlocos + @"Listagem\TIRANTES_TITULO.dwg";
        public static string Tirantes { get; set; } = RaizBlocos + @"Listagem\TIRANTES.dwg";
        public static string Tercas_Indicacao { get; set; } = RaizBlocos + @"Listagem\TERCA_INDICACAO.dwg";
        public static string Tirantes_Indicacao { get; set; } = RaizBlocos + @"Listagem\TIRANTE_INDICACAO.dwg";
        public static string Correntes_Indicacao { get; set; } = RaizBlocos + @"Listagem\CORRENTE_INDICACAO.dwg";
        public static string Tercas { get; set; } = RaizBlocos + @"Listagem\TERCAS.dwg";
        public static string Texto { get; set; } = RaizBlocos + @"Listagem\TEXTO.dwg";

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
