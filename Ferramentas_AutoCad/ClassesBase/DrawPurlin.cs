using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public class DrawPurlin :Notificar
    {
        [Browsable(false)]
        public double Y { get; set; } = 0;
        [Browsable(false)]
        public double X1 { get; set; } = 0;
        [Browsable(false)]
        public double X2 { get; set; } = 0;
        public double TR1 { get; set; } = 337;
        public double TR2 { get; set; } = 337;

        

        public bool RebaterFuros { get; set; } = false;
        public bool InserirXlines { get; set; } = false;

        [Browsable(false)]
        public double X01 => X1 - TR1;
        public double X02 => X2 + TR2;
        public double Comprimento => (X2 - X1).Round(0).Abs();
        public DrawPurlin()
        {

        }
    }
}
