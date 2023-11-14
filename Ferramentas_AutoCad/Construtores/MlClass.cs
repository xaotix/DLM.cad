using DLM.vars.cad;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace DLM.cad
{
    public class MlClass
    {
        private static List<MlClass> _mlClasses { get; set; }
        public static List<MlClass> GetSetup()
        {
            if(_mlClasses==null)
            {
                _mlClasses = new List<MlClass>();
                _mlClasses.Add(new MlClass("10MM",Tipo_Multiline.Tirante));

                _mlClasses.Add(new MlClass("C50", Tipo_Multiline.Corrente));
                _mlClasses.Add(new MlClass("DIAG50X3", Tipo_Multiline.Corrente));
                _mlClasses.Add(new MlClass("L32X32X3MM", Tipo_Multiline.Corrente));
                _mlClasses.Add(new MlClass("L51X51X3MM", Tipo_Multiline.Corrente));

                _mlClasses.Add(new MlClass("MESA125", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("MESA150", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("MESA175", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("MESA200", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("MESA225", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("MESA250", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("W150X13LAT", Tipo_Multiline.Viga_Apoio));
                _mlClasses.Add(new MlClass("W360X39", Tipo_Multiline.Viga_Apoio));

                _mlClasses.Add(new MlClass("TERCA", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z165", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z185", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z185SUP", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z216", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z292", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z292", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z360", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z64", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z70", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("Z89", Tipo_Multiline.Purlin));
                _mlClasses.Add(new MlClass("ZZ360", Tipo_Multiline.Purlin));
            }
            return _mlClasses;
        }
        public override string ToString()
        {
            return $"{this.Nome} [{this.Mlines.Count}x]";
        }
        public string Nome { get; set; } = "";
        public Tipo_Multiline Tipo { get; set; } = Tipo_Multiline.Definir;
        [Browsable(false)]
        [XmlIgnore]
        public List<CADMline> Mlines { get; set; } = new List<CADMline>();
        public MlClass(string nome, List<CADMline> mlines)
        {
            this.Nome = nome;
            this.Mlines = mlines;
        }
        public MlClass(string nome, Tipo_Multiline tipo)
        {
            this.Nome = nome;
            this.Tipo = tipo;
        }
        public MlClass()
        {

        }
    }
}
