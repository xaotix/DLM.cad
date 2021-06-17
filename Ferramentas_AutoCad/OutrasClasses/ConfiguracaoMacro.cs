﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class ConfiguracaoMacro
    {
        [DisplayName("Gerar/Atualizar Tabelas")]
        public bool GerarTabela { get; set; } = false;
        [DisplayName("Preencher Selos")]
        public bool PreencheSelos { get; set; } = false;

        [DisplayName("Gerar DBF")]
        public bool GerarDBF { get; set; } = true;

        [DisplayName("Atualizar CAMs")]
        public bool AtualizarCams { get; set; } = true;
        [DisplayName("Ajustar Lts")]
        public bool AjustarLTS { get; set; } = true;

        [DisplayName("Ajustar MViews")]
        public bool AjustarMViews { get; set; } = true;

        [DisplayName("Gerar DXF de CAMs")]
        public bool DXFs_de_CAMs { get; set; } = false;

        [DisplayName("Gerar PDFs")]
        public bool Gerar_PDFs { get; set; } = false;
        public ConfiguracaoMacro()
        {

        }
    }
}
