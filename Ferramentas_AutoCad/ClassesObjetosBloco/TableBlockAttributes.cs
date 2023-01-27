
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{

    public class TableBlockAttributes
    {
        public override string ToString()
        {
            return "[" + Name + "]" + "/L:" + BlockAttributes.Count();
        }
        public string Name { get; set; } = "";
        public List<BlockAttributes> BlockAttributes { get; set; } = new List<BlockAttributes>();


        public List<string> GetColunas()
        {
            return BlockAttributes.SelectMany(x => x.GetColunas()).Distinct().ToList();
        }
        public DLM.db.Tabela GetTable()
        {
            DLM.db.Tabela tb = new DLM.db.Tabela(this.Name);
            foreach(BlockAttributes bl in this.BlockAttributes)
            {
                tb.Linhas.Add(new DLM.db.Linha(bl.Attributes.Select(x=> new DLM.db.Celula(x.Coluna,x.Valor)).ToList()));
            }
            return tb;
        }



        public TableBlockAttributes()
        {

        }
        public TableBlockAttributes(List<TableBlockAttributes> unir)
        {
            var colunas = unir.SelectMany(x => x.GetColunas()).Distinct().ToList();
            foreach (var tab in unir)
            {
                foreach (var l in tab.BlockAttributes)
                {
                    BlockAttributes nl = new BlockAttributes(l.Block, false);
                    foreach (var c in colunas)
                    {
                        var igual = l.Get(c);
                        nl.Attributes.Add(new db.Celula(c, igual.Valor));
                    }
                    this.BlockAttributes.Add(nl);
                }
            }
        }
    }
}
