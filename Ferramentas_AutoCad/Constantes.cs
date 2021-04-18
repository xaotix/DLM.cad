using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    internal static class Constantes
    {
        public static string RaizArquivos { get; set; } = @"\\10.54.0.4\BancoDeDados\";
        public static string RaizBlocosA2 { get; set; } = RaizArquivos + @"Blocos\SELO A2\";
        public static string RaizPcs { get; set; } = RaizArquivos + @"Blocos\SELO A2\Listagem\Peças Mapeáveis\";
        public static string RaizTecnoMetalBlocos { get; set; } = RaizArquivos + @"Simbologias\usr\";


        public static string Tabela_Correntes_Titulo { get; set; } = RaizBlocosA2 + @"Listagem\CORRENTES_TITULO.dwg";
        public static string Tabela_Correntes { get; set; } = RaizBlocosA2 + @"Listagem\CORRENTES.dwg";
        public static string Tabela_Tercas_Titulo { get; set; } = RaizBlocosA2 + @"Listagem\TERCAS_TITULO.dwg";
        public static string Tabela_Almox_Titulo { get; set; } = RaizBlocosA2 + @"Listagem\ALMOX_TITULO.dwg";
        public static string Tabela_Almox { get; set; } = RaizBlocosA2 + @"Listagem\ALMOX.dwg";
        public static string Tabela_Tirantes_Titulo { get; set; } = RaizBlocosA2 + @"Listagem\TIRANTES_TITULO.dwg";
        public static string Tabela_Tirantes { get; set; } = RaizBlocosA2 + @"Listagem\TIRANTES.dwg";
        public static string Tabela_Tercas { get; set; } = RaizBlocosA2 + @"Listagem\TERCAS.dwg";
        public static string Tabela_TecnoMetal { get; set; } = RaizBlocosA2 + @"Listagem\TECNOMETAL_TAB_LIN.dwg";
        public static string Tabela_TecnoMetal_Titulo { get; set; } = RaizBlocosA2 + @"Listagem\TECNOMETAL_TAB_CAB.dwg";
        public static string Tabela_TecnoMetal_Vazia { get; set; } = RaizBlocosA2 + @"Listagem\TECNOMETAL_TAB_LIN_VAZIA.dwg";


        public static string Incicacao_Tercas { get; set; } = RaizBlocosA2 + @"Listagem\TERCA_INDICACAO.dwg";
        public static string Indicacao_Tirantes { get; set; } = RaizBlocosA2 + @"Listagem\TIRANTE_INDICACAO.dwg";
        public static string Indicacao_Correntes { get; set; } = RaizBlocosA2 + @"Listagem\CORRENTE_INDICACAO.dwg";
        
        public static string Texto { get; set; } = RaizBlocosA2 + @"Listagem\TEXTO.dwg";
        public static string Marca_Chapa { get; set; } = RaizTecnoMetalBlocos + @"\m8_lam.dwg";
        public static string Marca_Perfil { get; set; } = RaizTecnoMetalBlocos + @"\m8_pro.dwg";
        public static string Marca_Arremate { get; set; } = RaizArquivos + @"Blocos\SELO A2\Tecnometal\Arremates\m8_lam.dwg";
        public static string Peca_ESTICADOR { get; set; } = RaizPcs + @"ESTICADOR.dwg";
        public static string Peca_MANILHA { get; set; } = RaizPcs + @"MANILHA.dwg";
        public static string Peca_PASSARELA { get; set; } = RaizPcs + @"PASSARELA.dwg";
        public static string Peca_SFLH { get; set; } = RaizPcs + @"SFLH.dwg";
        public static string Peca_SFLI { get; set; } = RaizPcs + @"SFLI.dwg";

        public static List<string> BlocosTecnoMetalMarcas { get; set; } = new List<string>() { "M8_COM", "M8_ELE", "M8_ELU", "M8_LAM", "M8_PRO" };
        public static List<string> BlocosTecnoMetalPosicoes { get; set; } = new List<string>() { "P8_ELE", "P8_ELU", "P8_LAM", "P8_PRO", "P8_RIP" };

    }
}
