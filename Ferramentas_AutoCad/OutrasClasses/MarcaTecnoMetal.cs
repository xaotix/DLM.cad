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
        public Tipo_Bloco Tipo_Bloco
        {
            get
            {
                if (NomeBloco == BL_M_CH | NomeBloco == BL_P_CH)
                {
                    return Tipo_Bloco.Chapa;
                }
                else if (NomeBloco == BL_M_PERF | NomeBloco == BL_P_PERF)
                {
                    return Tipo_Bloco.Perfil;
                }
                else if (NomeBloco == BL_M_ELEM2 | NomeBloco == BL_P_ELEM2)
                {
                    return Tipo_Bloco.Elemento_M2;
                }
                else if (NomeBloco == BL_M_ELUNIT | NomeBloco == BL_P_ELUNIT)
                {
                    return Tipo_Bloco.Elemento_Unitario;
                }
                else if (NomeBloco == BL_M_ARR)
                {
                    return Tipo_Bloco.Arremate;
                }

                return Tipo_Bloco._;
            }
        }

        private Conexoes.TecnoMetal_Perfil _perfil { get; set; }

        public Conexoes.TecnoMetal_Perfil GetPerfil()
        {
            if(_perfil==null)
            {
                if(this.Tipo_Marca != Tipo_Marca.Posicao && (Tipo_Bloco == Tipo_Bloco.Elemento_M2 | Tipo_Bloco == Tipo_Bloco.Perfil))
                {
                        _perfil = Utilidades.GetdbTecnoMetal().Get(this.Perfil);
                }
                else
                {
                    int cat = 0;
                    if(Tipo_Bloco == Tipo_Bloco.Chapa | Tipo_Bloco == Tipo_Bloco.Arremate)
                    {
                        cat = -1;
                    }
                    _perfil = new Conexoes.TecnoMetal_Perfil() { CAT = cat };
                }
               
            }
            return _perfil;
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
                    return Tipo_Marca.Posicao;
                }

                return Tipo_Marca._;
            }
        }

        public string Marca { get; private set; } = "";
        public string Tratamento { get; private set; } = "";
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
                if (this.SubItens.Count > 0)
                {
                    return this.SubItens.Sum(x => x.PesoUnit * x.Quantidade);
                }
                return _PesoUnit;
            }
        }
        private double _PesoUnit { get;  set; } = 1;
        public double Superficie
        {
            get
            {
                if(this.SubItens.Count>0)
                {
                    return this.SubItens.Sum(x => x.Superficie * x.Quantidade);
                }
                return _Superficie;
            }
        }
        private double _Superficie { get; set; } = 1;
        public string Perfil { get; private set; } = "";
        public string NomeBloco { get; private set; } = "";
        public string Prancha { get; private set; } = "";
        public string Material { get; private set; } = "";

        public List<MarcaTecnoMetal> GetPosicoes()
        {
            var pcs = this.SubItens.GroupBy(x => x.Posicao).ToList();
            List<MarcaTecnoMetal> marcaTecnoMetals = new List<MarcaTecnoMetal>();

            foreach(var pc in pcs)
            {
                marcaTecnoMetals.Add(new MarcaTecnoMetal(pc.ToList()));
            }

            return marcaTecnoMetals;
        }

        public List<MarcaTecnoMetal> SubItens { get; private set; } = new List<MarcaTecnoMetal>();

        public DB.Linha Linha { get; set; } = new DB.Linha();
        public MarcaTecnoMetal(DB.Linha l)
        {
            this.Linha = l;
            this.Marca = l.Get(Constantes.ATT_MAR).ToString();
            this.Posicao = l.Get(Constantes.ATT_POS).ToString();
            this.Quantidade = l.Get(Constantes.ATT_QTD).Double();
            this.NomeBloco = l.Get(Constantes.ATT_BLK).ToString();
            this.Prancha = l.Get(Constantes.ATT_DWG).ToString();

            this.Comprimento = l.Get(Constantes.ATT_CMP).Double();
            this.Largura = l.Get(Constantes.ATT_LRG).Double();
            this.Espessura = l.Get(Constantes.ATT_ESP).Double(2);
            this.Perfil = l.Get(Constantes.ATT_PER).ToString();

            this.Mercadoria = l.Get(Constantes.ATT_MER).ToString();
            this.Material = l.Get(Constantes.ATT_MAT).ToString();
            this.Tratamento = l.Get(Constantes.ATT_FIC).ToString();

            this._PesoUnit = l.Get(Constantes.ATT_PES).Double();
            this._Superficie = l.Get(Constantes.ATT_SUP).Double();
        }
        public MarcaTecnoMetal(List<MarcaTecnoMetal> posicoes_iguais)
        {
            var m = posicoes_iguais[0];
            this.Marca = string.Join(";", posicoes_iguais.Select(x => x.Marca));
            this.Comprimento = m.Comprimento;
            this.Espessura = m.Espessura;
            this.Largura = m.Largura;
            this.Linha = m.Linha;
            this.Mercadoria = m.Mercadoria;
            this.NomeBloco = m.NomeBloco;
            this.Perfil = m.Perfil;
            this._PesoUnit = m.PesoUnit;
            this.Posicao = m.Posicao;
            this.Prancha = m.Prancha;
            this.Quantidade = posicoes_iguais.Sum(x => x.Quantidade);
            this._Superficie = m.Superficie;
            this.Linha = m.Linha;

        }
    }

}
