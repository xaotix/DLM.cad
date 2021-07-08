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
        public string GetInfo()
        {
            string retorno = $"" +
                $"\nComprimento >{this.Comprimento}" +
                $"\nDescrição >{this.Descricao}" +
                $"\nEspessura >{this.Espessura}" +
                $"\nLargura >{this.Largura}" +
                $"\nMaterial >{this.Material}" +
                $"\nMercadoria >{this.Mercadoria}" +
                $"\nBloco >{this.NomeBloco}" +
                $"\nPerfil >{this.Perfil}" +
                //$"\nPrancha >{this.Prancha}" +
                $"\nSAP >{this.SAP}" +
                $"\nTip. Blk >{this.Tipo_Bloco}" +
                $"\nTipo M >{this.Tipo_Marca}" +
                $"\nTrat >{this.Tratamento}";

            return retorno;
        }
        public MarcaTecnoMetal Pai { get; set; }
        public override string ToString()
        {
            return $"[{Marca}{(Tipo_Marca == Tipo_Marca.Posicao? $" - P = {Posicao}": $" - {Tipo_Bloco}")} ] - QTD.: {Quantidade}";
        }

        private double _Comprimento { get; set; } = 1;
        private double _Largura { get; set; } = 1;
        private double _PesoUnit { get;  set; } = 1;
        private double _Superficie { get; set; } = 1;
        private string _Perfil { get; set; } = "";
        private double _Espessura { get; set; } = 0;


        public bool TemCadastroDBF
        {
            get
            {
                if(Tipo_Bloco == Tipo_Bloco.DUMMY_Perfil | Tipo_Bloco == Tipo_Bloco.Elemento_M2 | Tipo_Bloco == Tipo_Bloco.Perfil)
                {
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
                else if(NomeBloco == "DUMMY")
                {
                    if(Pai!=null)
                    {
                        if(Pai.Tipo_Bloco ==  Tipo_Bloco.Elemento_M2 | Pai.Tipo_Bloco ==  Tipo_Bloco.Perfil)
                        {
                            return Tipo_Bloco.DUMMY_Perfil;
                        }
                    }
                    return Tipo_Bloco.DUMMY;
                }

                return Tipo_Bloco._;
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
                    return Tipo_Marca.Posicao;
                }

                return Tipo_Marca._;
            }
        }

        public string Marca { get; private set; } = "";
        public string SAP { get; private set; } = "";
        public string Tratamento { get; private set; } = "";
        public string Posicao { get; private set; } = "";
        public double Quantidade { get; private set; } = 1;
        public double Comprimento
        {
            get
            {
                if(this.SubItens.Count>0 && Tipo_Marca == Tipo_Marca.MarcaSimples)
                {
                    return this.SubItens[0].Comprimento;
                }
                return Math.Round(_Comprimento,1);
            }
            private set
            {
                _Comprimento = value;
            }
        }
        public string Mercadoria { get; private set; } = "";
        public double Largura
        {
            get
            {
                if (this.SubItens.Count > 0 && Tipo_Marca == Tipo_Marca.MarcaSimples)
                {
                    return Math.Round(this.SubItens[0].Largura,1);
                }
                return Math.Round(_Largura,1);
            }
            private set
            {
                _Largura = value;
            }
        }
        public double Espessura
        {
            get
            {
                if (this.SubItens.Count > 0 && Tipo_Marca == Tipo_Marca.MarcaSimples)
                {
                    return this.SubItens[0].Espessura;
                }
                return Math.Round(_Espessura,2);
            }
            private set
            {
                _Espessura = value;
            }
        }
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

        public Conexoes.TecnoMetal_Perfil GetPerfil()
        {
            return Conexoes.DBases.GetdbTecnoMetal().Get(this.Perfil);
        }
        public string Perfil
        {
            get
            {
                if(Tipo_Marca == Tipo_Marca.MarcaSimples)
                {
                    if(this.SubItens.Count>0)
                    {
                        return this.SubItens[0].Perfil;
                    }
                }
               
                return _Perfil;
            }
            private set
            {
                _Perfil = value;
            }
        }

        public string Descricao
        {
            get
            {
                string retorno = Perfil;
                 if (retorno == "")
                {
                    retorno =
                          "Ch. " + this.Linha.Get(Constantes.ATT_ESP).Double().ToString("N2").Replace(",", "") +
                          " x " + this.Linha.Get(Constantes.ATT_LRG).Double().ToString("N1").Replace(",", "") +
                          " x " + this.Linha.Get(Constantes.ATT_CMP).Double().ToString("N1").Replace(",", "");


                }

                return retorno + " x " + Math.Round(this.Comprimento);
            }
        }

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
            this.SAP = l.Get(Constantes.ATT_SAP).ToString();

            this._PesoUnit = l.Get(Constantes.ATT_PES).Double();
            this._Superficie = l.Get(Constantes.ATT_SUP).Double();
        }

        public List<MarcaTecnoMetal> Posicoes_Iguais { get; set; } = new List<MarcaTecnoMetal>();
        public MarcaTecnoMetal(List<MarcaTecnoMetal> posicoes_iguais)
        {
            var m = posicoes_iguais[0];
            this.Marca = string.Join(";", posicoes_iguais.Select(x => x.Marca));
            this.Comprimento = m.Comprimento;
            this.Espessura = m.Espessura;
            this.Largura = m.Largura;
            this.Linha = m.Linha;
            this.Mercadoria = m.Mercadoria;
            this.Material = m.Material;
            this.Tratamento = m.Tratamento;
            this.NomeBloco = m.NomeBloco;
            this.Perfil = m.Perfil;
            this.Posicao = m.Posicao;
            this.SAP = m.SAP;
            this._PesoUnit = m.PesoUnit;
            this.Prancha = m.Prancha;
            this.Quantidade = posicoes_iguais.Sum(x => x.Quantidade);
            this._Superficie = m.Superficie;
            this.Linha = m.Linha;
            this.Posicoes_Iguais = posicoes_iguais;
        }
    }

}
