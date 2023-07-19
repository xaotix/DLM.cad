
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
            foreach(BlockAttributes linha in this.BlockAttributes)
            {
                tb.Add(linha);
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
                foreach (var linha in tab.BlockAttributes)
                {
                    var nl = new BlockAttributes(linha.Block, false);
                    foreach (var coluna in colunas)
                    {
                        var igual = linha[coluna];
                        nl.Add(coluna, igual.Valor);
                    }
                    this.BlockAttributes.Add(nl);
                }
            }
        }
    }
}
