using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public class ConfiguracaoMacro
    {
        [DisplayName("Gerar/Atualizar Tabelas")]
        public bool GerarTabela { get; set; } = false;
        [DisplayName("Preencher Selos")]
        public bool PreencheSelos { get; set; } = false;

        [DisplayName("Gerar DBF")]
        public bool GerarDBF { get; set; } = false;

        [DisplayName("Atualizar CAMs")]
        public bool AtualizarCams { get; set; } = false;
        [DisplayName("Ajustar Lts")]
        public bool AjustarLTS { get; set; } = false;

        [DisplayName("Limpar desenhos")]
        public bool LimparDesenhos { get; set; } = false;
        [DisplayName("Ajustar Layers (somente montagens)")]
        [Browsable(false)]
        public bool Ajustar_Layers { get; set; } = false;

        [DisplayName("Ajustar MViews")]
        public bool AjustarMViews { get; set; } = false;

        [DisplayName("Gerar DXF de CAMs")]
        public bool DXFs_de_CAMs { get; set; } = false;

        [DisplayName("Gerar PDFs")]
        public bool Gerar_PDFs { get; set; } = false;
        public ConfiguracaoMacro()
        {

        }
    }
}
