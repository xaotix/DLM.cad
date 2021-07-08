using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace Ferramentas_DLM
{
    public class ObjetoMultiline
    {
        public double TranspasseInicial { get; set; } = 0;
        public double TranspasseFinal { get; set; } = 0;
        public Tipo_Multiline Tipo { get; private set; } = Tipo_Multiline.Desconhecido;
        public double Largura { get; private set; } = 0;
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return "p0: " + Inicio + " / p1: " + Fim + " comprimento: " + comprimento + " angulo: " + angulo;
        }
        public Mline mline { get; set; }
        public double comprimento { get; set; } = 0;
        public Coordenada Inicio { get; private set; }
        public Coordenada Fim { get; private set; }
        public Coordenada centro { get; private set; }

        public double maxx
        {
            get
            {
                return Inicio.X > Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double maxy
        {
            get
            {
                return Inicio.Y > Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double miny
        {
            get
            {
                return Inicio.Y < Fim.Y ? Inicio.Y : Fim.Y;
            }
        }
        public double minx
        {
            get
            {
                return Inicio.X < Fim.X ? Inicio.X : Fim.X;
            }
        }
        public double angulo { get; set; }
        public ObjetoMultiline()
        {

        }
        public ObjetoMultiline(Mline l, Tipo_Multiline tipo)
        {
            this.Tipo = tipo;
            Point3d p0, p1, centro;
            double comprimento, angulo,largura;
            Utilidades.GetCoordenadas(l, out p0, out p1, out angulo, out comprimento, out centro, out largura);
            this.angulo = angulo;
            this.centro = new Coordenada(centro);
            this.comprimento = comprimento;
            this.mline = l;
            this.Inicio = new Coordenada(p0);
            this.Fim = new Coordenada(p1);
            this.Largura = largura;
        }
    }

    public class PontoPurlin
    {

        public string PurlinPadrao
        {
            get
            {
                if (this.GetPecaPurlin() != null)
                {
                    return this.GetPecaPurlin().PERFIL;
                }

                return "???";
            }
        }

        private Conexoes.RME _pecaRME { get; set; }
        public Conexoes.RME GetPecaPurlin()
        {
            if (_pecaRME == null)
            {
                _pecaRME = Conexoes.DBases.GetBancoRM().GetRME(this.id_terca);
            }
            return _pecaRME;
        }

        public void SetPurlin(int id)
        {
            this.id_terca = id;
            this._pecaRME = null;
        }
        public VaoObra Vao { get; set; }
        public List<double> Correntes { get; set; } = new List<double>();
        public List<double> FurosManuais { get; set; } = new List<double>();
        public int id_terca { get; set; } = 0;
        public double TRE { get; set; } = 0;
        public double TRD { get; set; } = 0;
        public int Numero { get; set; } = 0;

        public double Comprimento
        {
            get
            {
                return this.TRE + this.TRD + this.Vao.Vao;
            }
        }
        public Point3d CentroBloco { get; set; }
        public ObjetoMultiline Multiline { get; set; }
        public PontoPurlin(ObjetoMultiline multiline, Point3d origem, int numero, VaoObra vao)
        {
            this.Multiline = multiline;
            this.CentroBloco = origem;
            this.Numero = numero;
            this.Vao = vao;
            this.id_terca = vao.id_terca;
        }
    }
}
