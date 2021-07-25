using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public enum Tipo_ObjetoBase
    {
        Purlin,
        Corrente,
        Tirante,
        Base,
    }
    public enum Tipo_Vao
    {
        Intermediario,
        Borda_Esquerdo,
        Borda_Direito,
    }
    public enum Sentido
    {
        Horizontal,
        Vertical,
        Inclinado,
    }
    public enum Tipo_Vista
    {
        Planta,
        Corte,
        Fachada,
    }
    public enum Tipo_Selecao
    {
        Tudo,
        Blocos,
        Textos,
        Blocos_Textos,
        Dimensoes,
        Polyline,
    }
    public enum Tipo_Objeto
    {
        Texto,
        Bloco,
    }
    public enum Tipo_Calculo_Contorno
    {
        Maximo,
        Bordas
    }
    public enum Opcao
    {
        Nao = 0,
        Sim = 1,
    }
    public enum Tipo_Coordenada
    {
        Furo_Vista,
        Furo_Corte,
        Linha,
        Projecao,
        Sem,
        Ponto,
        Bloco,
    }

    public enum Tipo_Bloco
    {
        Chapa,
        Perfil,
        Elemento_M2,
        Elemento_Unitario,
        Arremate,
        DUMMY,
        DUMMY_Perfil,
        _ = -1
    }

    public enum Tipo_Marca
    {
        MarcaComposta,
        MarcaSimples,
        Posicao,
        _ = -1
    }
    public enum Tipo_Chapa
    {
        Fina,
        Grossa,
        Tudo,
    }
    public enum Tipo_Multiline
    {
        Purlin,
        Corrente,
        Tirante,
        Desconhecido,
    }
}
