using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.Constantes;

namespace Ferramentas_DLM
{
    public class MarcaTecnoMetal
    {
        public override string ToString()
        {
            return $"[{Marca}{(Tipo_Marca == Tipo_Marca.Posicao? $" - P = {Posicao}": $" - {Mercadoria}")} ] - QTD.: {Quantidade}";
        }
        public Tipo_Perfil Tipo_Perfil
        {
            get
            {
                if (NomeBloco == BL_M_CH | NomeBloco == BL_P_CH)
                {
                    return Tipo_Perfil.Chapa;
                }
                else if (NomeBloco == BL_M_PERF | NomeBloco == BL_P_PERF)
                {
                    return Tipo_Perfil.Perfil;
                }
                else if (NomeBloco == BL_M_ELEM2 | NomeBloco == BL_P_ELEM2)
                {
                    return Tipo_Perfil.Elemento_M2;
                }
                else if (NomeBloco == BL_M_ELUNIT | NomeBloco == BL_P_ELUNIT)
                {
                    return Tipo_Perfil.Elemento_Unitario;
                }
                else if (NomeBloco == BL_M_ARR)
                {
                    return Tipo_Perfil.Arremate;
                }

                return Tipo_Perfil._;
            }
        }
        public Tipo_Marca Tipo_Marca
        {
            get
            {
                if (NomeBloco == BL_M_Composta)
                {
                    return Tipo_Marca.MarcaComposta;
                }
                else if (NomeBloco == BL_M_PERF | NomeBloco == BL_M_CH | NomeBloco == BL_M_ARR | NomeBloco == BL_M_ELEM2 | NomeBloco == BL_M_ELUNIT)
                {
                    return Tipo_Marca.MarcaSimples;
                }
                else if (NomeBloco == BL_P_PERF | NomeBloco == BL_P_CH | NomeBloco == BL_P_ELEM2 | NomeBloco == BL_P_ELUNIT)
                {
                    return Tipo_Marca.MarcaSimples;
                }

                return Tipo_Marca._;
            }
        }

        public string Marca { get; private set; } = "";
        public string Posicao { get; private set; } = "";
        public double Quantidade { get; private set; } = 1;
        public double Comprimento { get; private set; } = 1;
        public string Mercadoria { get; private set; } = "";
        public double Largura { get; private set; } = 1;
        public double Espessura { get; private set; } = 1;

        public double PesoUnit
        {
            get
            {
                if (this.Posicoes.Count > 0)
                {
                    return this.Posicoes.Sum(x => x.PesoUnit * x.Quantidade);
                }
                return _PesoUnit;
            }
        }
        private double _PesoUnit { get;  set; } = 1;
        public double Superficie
        {
            get
            {
                if(this.Posicoes.Count>0)
                {
                    return this.Posicoes.Sum(x => x.Superficie * x.Quantidade);
                }
                return _Superficie;
            }
        }
        private double _Superficie { get; set; } = 1;
        public string Perfil { get; private set; } = "";
        public string NomeBloco { get; private set; } = "";

        public List<MarcaTecnoMetal> Posicoes { get; set; } = new List<MarcaTecnoMetal>();

        public DB.Linha Linha { get; set; } = new DB.Linha();
        public MarcaTecnoMetal(DB.Linha l)
        {
            this.Linha = l;
            this.Marca = l.Get(Constantes.ATT_MAR).ToString();
            this.Posicao = l.Get(Constantes.ATT_POS).ToString();
            this.Quantidade = l.Get(Constantes.ATT_QTD).Double();
            this.NomeBloco = l.Get(Constantes.ATT_BLK).ToString();

            this.Comprimento = l.Get(Constantes.ATT_CMP).Double();
            this.Largura = l.Get(Constantes.ATT_LRG).Double();
            this.Espessura = l.Get(Constantes.ATT_ESP).Double();
            this.Perfil = l.Get(Constantes.ATT_PER).ToString();

            this.Mercadoria = l.Get(Constantes.ATT_MER).ToString();

            this._PesoUnit = l.Get(Constantes.ATT_PES).Double();
            this._Superficie = l.Get(Constantes.ATT_SUP).Double();
        }
    }

}
