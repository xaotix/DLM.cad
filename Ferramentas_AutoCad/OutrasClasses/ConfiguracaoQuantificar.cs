using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class ConfiguracaoQuantificar
    {
        [Category("Mapear")]
        [DisplayName("Textos")]
        public bool Textos { get; set; } = true;

        [Category("Mapear")]
        [DisplayName("Blocos")]
        public bool Blocos { get; set; } = true;

        [Category("Mapear")]
        [DisplayName("Blocos Montagem TecnoMetal")]
        public bool Pecas_TecnoMetal { get; set; } = true;


        public ConfiguracaoQuantificar()
        {

        }
    }
}
