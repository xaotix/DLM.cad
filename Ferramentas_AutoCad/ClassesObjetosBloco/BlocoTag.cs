using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
   public class BlocoTag
    {
        public override string ToString()
        {
            return this.Descricao;
        }
        public string Descricao { get; set; } = "";
        public BlockReference Bloco { get; private set; }
        public void Set(string coluna, string valor)
        {
            var t = this.Atributos.Find(x => x.Coluna.ToUpper() == coluna.ToUpper());
            if (t != null)
            {
                t.Set(valor);
            }
            else
            {
                this.Atributos.Add(new CelulaTag(coluna, valor,null));
            }

        }

        public CelulaTag Get(string Coluna)
        {
            var s = Atributos.FindAll(x => x != null).Find(x => x.Coluna.ToUpper() == Coluna.ToUpper());
            if (s != null)
            {
                return s;

            }
            else
            {
                return new CelulaTag(Coluna, "", null);
            }

        }
        public List<string> GetColunas()
        {
            return Atributos.Select(x => x.Coluna).ToList();
        }
        public List<CelulaTag> Atributos { get; set; } = new List<CelulaTag>();
        public BlocoTag Clonar()
        {
            BlocoTag retorno = new BlocoTag(this.Bloco,false);
            retorno.Descricao = this.Descricao;
            retorno.Atributos.Clear();
            foreach (var c in this.Atributos)
            {
                retorno.Atributos.Add(new CelulaTag(c.Coluna, c.Valor,c.Atributo));
            }
            return retorno;
        }
        private List<Point2d> _contorno { get; set; }

        public List<Point2d> GetPontos(Transaction tr)
        {
            if(_contorno==null)
            {
                _contorno = Ut.GetPontos(this.Bloco, tr).Select(x=>new Point2d(x.X,x.Y)).ToList();
            }
            return _contorno;
        }

        public List<BlocoTag> Filhos { get; set; } = new List<BlocoTag>();

        public double GetAngulo()
        {
            if(Bloco!=null)
            {
                Angulo.RadianosParaGraus(Bloco.Rotation);
            }
            return 0;
        }

        private P3dCAD _coordenada { get; set; }
        public P3dCAD GetCoordenada()
        {
            if(_coordenada==null)
            {
                if (Bloco == null)
                {
                    return new P3dCAD();
                }
                else
                {
                    _coordenada = new P3dCAD(this.Bloco.Position, -1, Tipo_Coordenada.Bloco);
                }
            }
            return _coordenada;
        }

        public BlocoTag(List<CelulaTag> atributos)
        {
            this.Atributos = atributos;
        }

        public BlocoTag(BlockReference bloco, bool carregar = true)
        {
            this.Bloco = bloco;


            if (carregar)
            {
                var bl = DLM.cad.Atributos.GetBlocoTag(bloco);

                this.Atributos =new List<CelulaTag>();
                this.Atributos.AddRange(bl.Atributos);
            }
        }

    }

    public class TabelaBlocoTag
    {

        public DLM.db.Tabela GetTabela()
        {
            DLM.db.Tabela tb = new DLM.db.Tabela(this.Nome);
            foreach(BlocoTag bl in this.Blocos)
            {
                tb.Linhas.Add(new DLM.db.Linha(bl.Atributos.Select(x=> new DLM.db.Celula(x.Coluna,x.Valor)).ToList()));
            }
            return tb;
        }
        public List<List<string>> GetLista(bool cabecalho = true)
        {
            List<List<string>> retorno = new List<List<string>>();
            List<string> colunas = this.GetColunas();

            if (cabecalho)
            {
                retorno.Add(colunas);
            }

            foreach (var l in this.Blocos)
            {
                List<string> ll = new List<string>();
                foreach (var c in colunas)
                {
                    ll.Add(l.Get(c).Valor);
                }
                retorno.Add(ll);
            }
            return retorno;
        }

        public string Nome { get; set; } = "";
        public string Banco { get; set; } = "";






        public override string ToString()
        {
            return "[" + Nome + "]" + "/L:" + Blocos.Count();
        }


        public List<string> GetColunas()
        {
            return Blocos.SelectMany(x => x.GetColunas()).Distinct().ToList();
        }
        public List<BlocoTag> Blocos { get; set; } = new List<BlocoTag>();
        public List<BlocoTag> Filtrar(string Chave, string Valor, bool exato = false)
        {
            List<BlocoTag> Retorno = new List<BlocoTag>();
            if (exato)
            {
                return Blocos.FindAll(x => x.Atributos.FindAll(y => y.Coluna == Chave && y.Valor == Valor).Count > 0);
            }
            else
            {
                return Blocos.FindAll(x => x.Atributos.FindAll(y => y.Coluna.ToLower().Replace(" ", "") == Chave.ToLower().Replace(" ", "") && y.Valor.ToLower().Replace(" ", "").Contains(Valor.ToLower().Replace(" ", ""))).Count > 0);
            }
        }
        public TabelaBlocoTag(List<BlocoTag> Linhas, string Nome)
        {
            this.Nome = Nome;
            this.Blocos = Linhas;
        }
        public TabelaBlocoTag Filtro(string Chave, string Valor, bool Exato)
        {
            return new TabelaBlocoTag(Filtrar(Chave, Valor, Exato), Nome);
        }
        public TabelaBlocoTag()
        {

        }
        public TabelaBlocoTag(List<TabelaBlocoTag> unir)
        {
            var s = unir.SelectMany(x => x.GetColunas()).Distinct().ToList();
            foreach (var tab in unir)
            {
                foreach (var l in tab.Blocos)
                {
                    BlocoTag nl = new BlocoTag(l.Bloco,false);
                    foreach (var c in s)
                    {
                        var igual = l.Get(c);
                        nl.Atributos.Add(new CelulaTag(c, igual.Valor,igual.Atributo));
                    }
                    this.Blocos.Add(nl);
                }
            }
        }
    }


    [Serializable]
    public class CelulaTag
    {
        public void Set(string valor)
        {
            this.Valor = valor;
        }
   
        public int Int
        {
            get
            {
                return Conexoes.Utilz.Int(this.Valor);
            }
        }
        public override string ToString()
        {
            return Valor;
        }
        public string Coluna { get; set; } = "";
        public string Valor { get; set; } = "";

        public AttributeReference Atributo { get; set; }

        public double Double(int decimais = 4)
        {
            return Conexoes.Utilz.Double(this.Valor,decimais);
        }

        public CelulaTag(string Coluna, string Valor, AttributeReference Atributo)
        {
            this.Valor = Valor;
            this.Coluna = Coluna;
            this.Atributo = Atributo;
        }

    }
}
