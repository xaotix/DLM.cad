using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public class BlocoPecaTecnoMetal
    {
        public string GetInfo()
        {
            string retorno = $"" +
                $"\nComprimento >{this.Comprimento.String(0)}" +
                $"\nDescrição >{this.Descricao}" +
                $"\nEspessura >{this.Espessura.String(2)}" +
                $"\nLargura >{this.Largura.String(0)}" +
                $"\nMaterial >{this.Material}" +
                $"\nMercadoria >{this.Mercadoria}" +
                $"\nBloco >{this.NomeBloco}" +
                $"\nPerfil >{this.Perfil}" +
                $"\nSAP >{this.SAP}" +
                $"\nTip. Blk >{this.Tipo_Bloco}" +
                $"\nTipo M >{this.Tipo_Marca}" +
                $"\nTrat >{this.Tratamento}";

            return retorno;
        }
        public BlocoPecaTecnoMetal Pai { get; set; }
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
                if (NomeBloco == Cfg.Init.CAD_BL_M_CH | NomeBloco == Cfg.Init.CAD_BL_P_CH)
                {
                    return Tipo_Bloco.Chapa;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_M_PERF | NomeBloco == Cfg.Init.CAD_BL_P_PERF)
                {
                    return Tipo_Bloco.Perfil;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_M_ELEM2 | NomeBloco == Cfg.Init.CAD_BL_P_ELEM2)
                {
                    return Tipo_Bloco.Elemento_M2;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_M_ELUNIT | NomeBloco == Cfg.Init.CAD_BL_P_ELUNIT)
                {
                    return Tipo_Bloco.Elemento_Unitario;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_M_ARR)
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
                if (NomeBloco == Cfg.Init.CAD_BL_M_Composta)
                {
                    return Tipo_Marca.MarcaComposta;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_M_PERF | NomeBloco == Cfg.Init.CAD_BL_M_CH | NomeBloco == Cfg.Init.CAD_BL_M_ARR | NomeBloco == Cfg.Init.CAD_BL_M_ELEM2 | NomeBloco == Cfg.Init.CAD_BL_M_ELUNIT)
                {
                    return Tipo_Marca.MarcaSimples;
                }
                else if (NomeBloco == Cfg.Init.CAD_BL_P_PERF | NomeBloco == Cfg.Init.CAD_BL_P_CH | NomeBloco == Cfg.Init.CAD_BL_P_ELEM2 | NomeBloco == Cfg.Init.CAD_BL_P_ELUNIT)
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
                else if(_Superficie>0)
                {
                return _Superficie;
                }
                else
                {

                    return CalcularSuperficieLinear();
                }
            }
        }
        /// <summary>
        /// Retorna o peso da peça sem descontar recortes
        /// </summary>
        /// <returns></returns>
        public double CalcularSuperficieLinear()
        {
            double ret = (this.Comprimento * this.Largura * 2) + (this.Espessura * this.Comprimento * 2) + (this.Espessura * this.Largura * 2);
            double sup = Math.Round(ret / 1000 / 1000 / 1000, Cfg.Init.CAD_DECIMAIS_SUP);
            return sup;
        }
        public double CalcularPesoLinear()
        {
            if (this.Tipo_Bloco == Tipo_Bloco.Arremate | this.Tipo_Bloco == Tipo_Bloco.Chapa | this.Tipo_Bloco == Tipo_Bloco.Elemento_M2)
            {
                if (this.Tipo_Bloco == Tipo_Bloco.Arremate)
                {
                    var bob = this.GetBobina();
                    if (bob != null)
                    {
                        return Math.Round(this.Espessura * this.Area * bob.Peso_Especifico / 1000 / 1000, Cfg.Init.CAD_DECIMAIS);
                    }
                }
                else
                {
                    return Math.Round(this.Espessura * this.Area * 7.85 / 1000 / 1000, Cfg.Init.CAD_DECIMAIS);
                }
            }
            return 0;
        }
        public DLM.cam.Perfil GetPerfil()
        {
            return DBases.GetdbPerfil().GetPerfilTecnoMetal(this.Perfil);
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

        public double Area
        {
            get
            {
              return  this.Comprimento * this.Largura;
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
                          "Ch. " + this.Bloco.Get(TAB_DBF1.SPE_PRO.ToString()).Double().ToString("N2").Replace(",", "") +
                          " x " + this.Bloco.Get(TAB_DBF1.LAR_PRO.ToString()).Double().ToString("N1").Replace(",", "");
                          /*" x " + this.Linha.Get(T_DBF1.LUN_PRO.ToString()).Double().ToString("N1").Replace(",", "")*/;


                }

                return retorno + " x " + Math.Round(this.Comprimento);
            }
        }

        public string NomeBloco { get; private set; } = "";
        public string Prancha { get; private set; } = "";
        public string Material { get; private set; } = "";

        private List<BlocoPecaTecnoMetal> _Posicoes { get; set; }
        public List<BlocoPecaTecnoMetal> GetPosicoes(bool update = false)
        {
            if(this._Posicoes==null && this.SubItens.Count>0 | update)
            {
                var pcs = this.SubItens.GroupBy(x => x.Posicao).ToList();
                _Posicoes = new List<BlocoPecaTecnoMetal>();

                foreach (var pc in pcs)
                {
                    _Posicoes.Add(new BlocoPecaTecnoMetal(pc.ToList(), this));
                }

                return _Posicoes;
            }
            else if(this._Posicoes==null)
            {
                return new List<BlocoPecaTecnoMetal>();
            }
            return _Posicoes;
            
        }
        public List<BlocoPecaTecnoMetal> SubItens { get; private set; } = new List<BlocoPecaTecnoMetal>();
        public BlockAttributes Bloco { get; set; }


        private Conexoes.Bobina _bobina { get; set; }

        public Conexoes.Bobina GetBobina()
        {
            string sap = this.SAP;
            if(this.Tipo_Marca == Tipo_Marca.MarcaSimples)
            {
                sap = this.SubItens[0].SAP;
            }
            if(_bobina==null && this.SAP!= sap)
            {
                _bobina = DBases.GetBancoRM().GetBobina(sap);
            }
            
            return _bobina;
        }




        public List<BlocoPecaTecnoMetal> Posicoes_Iguais { get; set; } = new List<BlocoPecaTecnoMetal>();
        public BlocoPecaTecnoMetal(List<BlocoPecaTecnoMetal> posicoes_iguais, BlocoPecaTecnoMetal pai)
        {
            this.Pai = pai;
            var m = posicoes_iguais[0];
            this.Marca = string.Join(";", posicoes_iguais.Select(x => x.Marca));
            this.Comprimento = m.Comprimento;
            this.Espessura = m.Espessura;
            this.Largura = m.Largura;
            this.Bloco = m.Bloco;
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
            this.Bloco = m.Bloco;
            this.Posicoes_Iguais = posicoes_iguais;
        }
        public BlocoPecaTecnoMetal(BlockAttributes block)
        {
            this.Bloco = block;
            this.Marca =        block.Get(TAB_DBF1.MAR_PEZ.ToString()).Valor;
            this.Posicao =      block.Get(TAB_DBF1.POS_PEZ.ToString()).Valor;
            this.Quantidade =   block.Get(TAB_DBF1.QTA_PEZ.ToString()).Double();
            this.NomeBloco =    block.Get(Cfg.Init.CAD_ATT_BLK).Valor;
            this.Prancha =      block.Get(TAB_DBF1.FLG_DWG.ToString()).Valor;

            this.Comprimento =  block.Get(TAB_DBF1.LUN_PRO.ToString()).Double(0);
            this.Largura =      block.Get(TAB_DBF1.LAR_PRO.ToString()).Double(0);
            this.Espessura =    block.Get(TAB_DBF1.SPE_PRO.ToString()).Double(2);
            this.Perfil =       block.Get(TAB_DBF1.NOM_PRO.ToString()).Valor;

            this.Mercadoria =   block.Get(TAB_DBF1.DES_PEZ.ToString()).Valor;
            this.Material =     block.Get(TAB_DBF1.MAT_PRO.ToString()).Valor;
            this.Tratamento =   block.Get(TAB_DBF1.TRA_PEZ.ToString()).Valor;
            this.SAP =          block.Get(TAB_DBF1.COD_PEZ.ToString()).Valor;

            this._PesoUnit =    block.Get(TAB_DBF1.PUN_LIS.ToString()).Double(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS);
            this._Superficie =  block.Get(TAB_DBF1.SUN_LIS.ToString()).Double(Cfg.Init.DECIMAIS_Superficie);
        }
    }

}
