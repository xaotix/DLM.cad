using Conexoes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    public class DrawPurlinCFG :Notificar
    {

        public double TR1 { get; set; } = 337;
        public double TR2 { get; set; } = 337;

        

        public bool RebaterFuros { get; set; } = false;
        public bool InserirXlines { get; set; } = false;

        [DisplayName("FB. Esquerda")]
        public List<double> TR1FBS { get; set; } = new List<double>();
        [DisplayName("FB. Direta")]
        public List<double> TR2FBS { get; set; } = new List<double>();



        [Browsable(false)]
        public double X1 { get; set; } = 0;
        [Browsable(false)]
        public double X2 { get; set; } = 0;

        [Browsable(false)]
        public double X01 => X1 - TR1;
        [Browsable(false)]
        public double X02 => X2 + TR2;
        [Browsable(false)]
        public double Comprimento => (X02 - X01).Round(0).Abs();


        [Browsable(false)]
        public List<MlClass> MultiLines { get; set; } = new List<MlClass>();
        public DrawPurlinCFG()
        {
            this.TR1FBS.Add(305);
            this.TR1FBS.Add(610);
            this.TR1FBS.Add(915);

            this.TR2FBS.Add(305);
            this.TR2FBS.Add(610);
            this.TR2FBS.Add(915);
        }
    }
}
