using Autodesk.AutoCAD.DatabaseServices;
using Conexoes;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public class PCQuantificar
    {
        public int id { get; set; } = 0;
        public List<PCQuantificar> Filhos_Ignorar { get; set; } = new List<PCQuantificar>();
        public string Nome_Bloco { get; set; } = "";
        public bool isbloco
        {
            get
            {
                
                return this.Tipo == Tipo_Objeto.Bloco;
            }
        }
        public override string ToString()
        {
            return $"[{this.Tipo}] { this.Nome} - {this.Quantidade}x";
        }
        public List<BlockAttributes> Blocos { get; private set; } = new List<BlockAttributes>();

        public List<string> GetAtributos()
        {
            var s = this.Blocos.SelectMany(x => x.Attributes).ToList().FindAll(x => x.Valor != "").Select(x => x.Coluna).Distinct().ToList().OrderBy(x => x).ToList();
            return s;
        }
        public BlockAttributes Atributos
        {
            get
            {
                if(Blocos.Count>0)
                {
                    return Blocos[0];
                }
                return new BlockAttributes(null);
            }
        }
        public string Nome { get; set; } = "";
        public double Quantidade { get; set; } = 1;
        public string Descricao { get; set; } = "";
        public string Numero { get; set; } = "";
        public string Destino { get; set; } = "";
        public string Perfil { get; set; } = "";
        public double Comprimento { get; set; } = 0;
        public string Material { get; set; } = "";

        public List<PCQuantificar> Agrupar( List<string>  atributoNome, string novo_nome_bloco)
        {

            var blocos = this.Blocos;
            List<PCQuantificar> retorno = new List<PCQuantificar>();

           if(atributoNome.Count>0)
            {
                var ss = blocos.GroupBy(x => string.Join("|",(atributoNome.Select(y=> x.Get(y).Valor).Distinct().ToList()))).ToList();

                foreach (var s in ss.ToList())
                {
                    var blks = s.ToList();
                    PCQuantificar nn = new PCQuantificar(Tipo_Objeto.Bloco, s.Key.Split('|')[0],"", novo_nome_bloco, blks);
                    retorno.Add(nn);
                }
            }


            if (retorno.Count > 0)
            {
                return retorno;
            }
            return new List<PCQuantificar> { this };
        }

        public Tipo_Objeto Tipo { get; private set; } = Tipo_Objeto.Texto;

        public string Familia { get; set; } = "PEÇAS";

        public void SetDescPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if(s!="")
            {
                this.Descricao = s;
            }
        }
        public void SetNumeroPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Numero = s;
            }
        }
        public void SetDestinoPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Destino = s;
            }
        }
        public void SetCompPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Comprimento = Conexoes.Utilz.Double(s);
            }
        }
        public void SetIdPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.id = Conexoes.Utilz.Int(s);
            }
        }
        public void SetPerfilPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Perfil = s;
            }
        }

        public void SetMaterialPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Material = s;
            }
        }
        public void SetQtdPorAtributo(string tag_qtd)
        {
            double tot = 0;
            foreach (var p in this.Blocos)
            {
                var qtd = p.Get(tag_qtd).Double();
                if (qtd > 0)
                {
                    tot = tot + qtd;
                }
                else
                {
                    tot = tot + 1;
                }

            }
            this.Quantidade = tot;
        }

        public void SetFamiliaPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).Valor;
            if (s != "")
            {
                this.Familia = s;
            }
        }

        public PCQuantificar(Tipo_Objeto Tipo, string nom, string desc, string nome_bloco,List<BlockAttributes> objetos)
        {
            this.Tipo = Tipo;
            this.Nome = nom;
            this.Nome_Bloco = nome_bloco;
            if(desc.Length>0)
            {
                if(this.Nome.Length>0)
                {
                    desc = desc.Replace(this.Nome, "");
                }
                this.Descricao = desc.Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").Replace("*", "").TrimStart();
                this.Descricao = this.Descricao.CortarString(25, false);
            }


            this.Blocos = objetos;

            this.Quantidade = objetos.Count;

            if(Tipo== Tipo_Objeto.Texto)
            {
                List<string> atts = this.Blocos.Select(x => x.Get("VALOR").Valor).ToList();

             

                double qtd = 0;

                foreach (var att in atts)
                {
                    double qtd_pc = 1;
                    string strn = att.ToUpper();

                    if (this.Nome.Length>0)
                    {
                        strn = att.ToUpper().Replace(this.Nome.ToUpper(), "");
                    }

                     var  str2 = strn
                        .Replace("("," ")
                        .Replace(")"," ")
                        .Replace("X"," ")
                        .Replace("*"," ")
                        .TrimEnd().TrimStart()
                        .Split(' ').ToList().FindAll(x=>x!="");

                    foreach(var st in str2)
                    {
                        if(st.ESoNumero())
                        {
                            qtd_pc  = Conexoes.Utilz.Double(st);
                            break;
                        }
                        else if(st == "A.L.")
                        {
                            qtd_pc = 2;
                            break;
                        }
                    }
                    if(qtd_pc==0)
                    {
                        qtd_pc = 1;
                    }

                    qtd = qtd + qtd_pc;
                }
                this.Quantidade = qtd;
            }
        }
        public PCQuantificar(Tipo_Objeto tipo, string nome, string desc, string nome_bloco, List<BlockAttributes> blocos, string numero,  string familia = "", string destino = "",  string perfil = "", string material = "", double comprimento = 0 )
        {
            this.Comprimento = comprimento;
            this.Descricao = desc;
            this.Destino = destino;
            this.Familia = familia;
            this.Material = material;
            this.Nome = nome;
            this.Nome_Bloco = nome_bloco;
            this.Numero = numero;
            this.Perfil = perfil;
            this.Quantidade = blocos.Count;
            this.Tipo = tipo;
            this.Blocos = blocos;

        }
        public PCQuantificar(Tipo_Objeto Tipo)
        {
            this.Tipo = Tipo;
        }
        public PCQuantificar(List<PCQuantificar> pecas)
        {
            if(pecas.Count>0)
            {
                this.Blocos.AddRange(pecas.SelectMany(x => x.Blocos).ToList());
                this.Tipo = pecas.First().Tipo;

                this.Familia = pecas.First().Familia;
                this.Nome_Bloco = pecas.First().Nome_Bloco;
                this.Perfil = pecas.First().Perfil;
                this.Comprimento = pecas.First().Comprimento;
                this.Material = pecas.First().Material;
                this.Nome = pecas.First().Nome;
                this.Quantidade = pecas.Sum(x => x.Quantidade);
                this.Descricao = pecas.First().Descricao;
                this.Numero = pecas.First().Numero;
                this.Destino = pecas.First().Destino;

            }
        }


    }
}
