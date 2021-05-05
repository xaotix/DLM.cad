using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{

    public class PCQuantificar
    {
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
        public List<DB.Linha> Objetos { get; private set; } = new List<DB.Linha>();

        public List<string> GetAtributos()
        {
            var s = this.Objetos.SelectMany(x => x.Celulas).ToList().FindAll(x => x.Valor != "").Select(x => x.Coluna).Distinct().ToList().OrderBy(x => x).ToList();
            return s;
        }
        public DB.Linha Atributos
        {
            get
            {
                if(Objetos.Count>0)
                {
                    return Objetos[0];
                }
                return new DB.Linha();
            }
        }
        public string Nome { get; set; } = "";
        public double Quantidade { get; set; } = 1;
        public string Descricao { get; set; } = "";

        public List<PCQuantificar> Agrupar( string atributoNome, List<DB.Linha> blocos = null)
        {

            if(blocos==null)
            {
                blocos = this.Objetos;
            }
            List<PCQuantificar> retorno = new List<PCQuantificar>();

           if(atributoNome != "")
            {
                var ss = blocos.GroupBy(x => x.Get(atributoNome).ToString()).ToList();

                foreach (var s in ss.ToList())
                {
                    PCQuantificar nn = new PCQuantificar(Tipo_Objeto.Bloco, s.Key,"", s.ToList());
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

        public void SetDescPorAtributo(string tag)
        {
            var s = this.Atributos.Get(tag).ToString();
            if(s!="")
            {
                this.Descricao = s;
            }
        }
        public void SetQtdPorAtributo(string tag_qtd)
        {
            double tot = 0;
            foreach (var p in this.Objetos)
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

        public PCQuantificar(Tipo_Objeto Tipo, string nom, string desc,List<DB.Linha> objetos)
        {
            this.Tipo = Tipo;
            this.Nome = nom;
            if(desc.Length>0)
            {
                this.Descricao = desc.Replace(this.Nome, "").Replace("\r", " ").Replace("\t", " ").Replace("\n", " ").Replace("*", "").TrimStart();
                this.Descricao = Conexoes.Utilz.CortarString(this.Descricao, 25, false);
            }


            if(objetos!=null)
            {
                this.Objetos = objetos;
            }
            this.Quantidade = objetos.Count;

            if(Tipo== Tipo_Objeto.Texto)
            {
                List<string> atts = this.Objetos.Select(x => x.Get("VALOR").ToString()).ToList();

             

                double qtd = 0;

                foreach(var att in atts)
                {
                    double qtd_pc = 0;
                    var strn = att
                        .Replace(this.Nome, "")
                        .Replace("("," ")
                        .Replace(")"," ")
                        .Replace("X"," ")
                        .Replace("*"," ")
                        .TrimEnd().TrimStart()
                        .Split(' ').ToList().FindAll(x=>x!="");
                    foreach(var st in strn)
                    {
                        if(Conexoes.Utilz.ESoNumero(st))
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

        public PCQuantificar(List<PCQuantificar> pecas)
        {
            if(pecas.Count>0)
            {
                this.Tipo = pecas.First().Tipo;
                this.Nome = pecas.First().Nome;
                this.Descricao = pecas.First().Descricao;
                this.Quantidade = pecas.Sum(x => x.Quantidade);
                this.Objetos.AddRange(pecas.SelectMany(x => x.Objetos).ToList());
            }

          }
    }
}
