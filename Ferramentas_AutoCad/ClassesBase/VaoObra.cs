using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class VaoObra
    {
        [Category("Purlin")]
        [DisplayName("id")]
        [ReadOnly(true)]
        public int id_terca { get; set; } = 1763;

        public List<ObjetoMultiline> GetMLPurlins(CADPurlin cADPurlin)
        {
            return Utilidades.MlinesPassando(Esquerda.Origem, Direita.Origem, cADPurlin.GetMultLinePurlins());
        }
        public List<ObjetoMultiline> GetMLCorrentes(CADPurlin cADPurlin)
        {
            return Utilidades.MlinesPassando(Esquerda.Origem, Direita.Origem, cADPurlin.GetMultLinesCorrentes(),true,true);
        }

        public List<PontoPurlin> Purlins { get; set; } = new List<PontoPurlin>();
        public int CalcularPurlins(CADPurlin CADPurlin, ref int c)
        {
            double ToleranciaPasse = 2;
            var CentroX = this.CentroX;

            this.Purlins.Clear();

            if (Vao >= CADPurlin.VaoMinimo && Vao <= CADPurlin.VaoMaximo)
            {

                var purlins = GetMLPurlins(CADPurlin);
                var correntes = GetMLCorrentes(CADPurlin);
                CADPurlin.AddMensagem("\n" + correntes.Count + " correntes encontradas");
                CADPurlin.AddMensagem("\n" + purlins.Count + " correntes encontradas");



                foreach (var purlin in purlins)
                {
                    CADPurlin.AddBarra();
                    CADPurlin.AddMensagem("\nMapeando Purlin...");
                    double TRE = Esquerda.Transpasse;
                    double TRD = Direita.Transpasse;
                    Point3d p_esq = purlin.Inicio.GetPoint();
                    Point3d p_dir = purlin.Fim.GetPoint();

                    if (p_esq.X >= Esquerda.Xmin && p_dir.X <= Direita.Xmin)
                    {
                        //purlin está dentro do eixo
                        CentroX = p_esq.X + ((p_dir.X - p_esq.X) / 2);
                        TRE = Math.Round(Esquerda.Xmin - p_esq.X);
                        TRD = Math.Round(p_dir.X - Direita.Xmin);
                    }

                    if (p_dir.X < Direita.Xmin + 1500)
                    {
                        //se a linha da purlin for menor que a soma do transpasse 
                        TRD = Math.Round(p_dir.X - Direita.Xmin);
                    }

                    if (p_esq.X > Esquerda.Xmin - 1500)
                    {
                        //se a linha da purlin for menor que a soma do transpasse
                        TRE = Math.Round(Esquerda.Xmin - p_esq.X);
                    }

                    //mapeia as correntes
                    Point3d origembloco = new Point3d(Math.Round(CentroX), Math.Round(p_esq.Y), 0);

                    Point3d centro = new Point3d(origembloco.X, origembloco.Y, 0);
                    CADPurlin.AddMensagem("\n" + origembloco + " centro do vão");

                    //desloca a origem do bloco
                    if (TRE < 0 | TRD < 0)
                    {
                        //desloca para a direita
                        var pe = new Point3d(Esquerda.Xmin, Esquerda.Ymin, Esquerda.Z);
                        var pd = new Point3d(Direita.Xmin, Direita.Ymin, Direita.Z);

                        if (TRE < 0)
                        {
                            pe = new Point3d(pe.X + Math.Abs(TRE), pe.Y, pe.Z);
                        }

                        if (TRD < 0)
                        {
                            pd = new Point3d(pd.X + TRD, pd.Y, pd.Z);
                        }

                        double cmp = pd.X - pe.X;

                        origembloco = new Point3d(pe.X + (cmp / 2), p_esq.Y, 0);

                    }



                    List<double> cre = new List<double>();
                    foreach (var corrente in correntes)
                    {
                        Point3d crp0 = corrente.Inicio.GetPoint();
                        Point3d crp1 = corrente.Fim.GetPoint();

                        crp0 = new Point3d(Math.Round(crp0.X), Math.Round(crp0.Y), 0);
                        crp1 = new Point3d(Math.Round(crp1.X), Math.Round(crp1.Y), 0);


                        //se está passando pela terça
                        if (crp0.Y + ToleranciaPasse + purlin.Largura >= centro.Y && crp1.Y - ToleranciaPasse - purlin.Largura <= centro.Y)
                        {
                            cre.Add(Math.Round(crp0.X - Esquerda.Xmin));
                        }

                    }
                    cre = cre.Distinct().ToList().OrderBy(x => x).ToList();

                    List<double> furos_m_esq = new List<double>();
                    if (CADPurlin.MapeiaFurosManuais)
                    {
                        var lista = Utilidades.LinhasPassando(Esquerda.Origem, Direita.Origem, CADPurlin.LinhasFuros(), true, true, true);
                        foreach (var ls in lista)
                        {
                            Point3d crp0 = new Point3d();
                            Point3d crp1 = new Point3d();
                            Point3d cc = new Point3d();
                            double angulo, comprimento, largura = 0;

                            Utilidades.GetCoordenadas(ls, out crp0, out crp1, out angulo, out comprimento, out cc, out largura);
                            crp0 = new Point3d(Math.Round(crp0.X), Math.Round(crp0.Y), 0);
                            crp1 = new Point3d(Math.Round(crp1.X), Math.Round(crp1.Y), 0);


                            //se está passando pela terça
                            if (crp0.Y + 5 >= centro.Y && crp1.Y - 5 <= centro.Y)
                            {
                                furos_m_esq.Add(Math.Round(crp0.X - Esquerda.Xmin));
                            }

                        }
                        furos_m_esq = furos_m_esq.Distinct().ToList().OrderBy(x => x).ToList();
                    }


                    var comp = Vao + TRE + TRD;
                    if (comp >= CADPurlin.PurlinCompMin && comp <= CADPurlin.PurlinCompMaximo)
                    {
                        //se a multiline é maior que o vão, se verifica se o comprimento em questão é maior que o balanço máximo.
                        if (comp > CADPurlin.PurlinBalancoMax)
                        {

                            PontoPurlin pp = new PontoPurlin(purlin, origembloco, c,this);
                            pp.Correntes.AddRange(cre);
                            pp.FurosManuais.AddRange(furos_m_esq);
                            pp.TRE = TRE;
                            pp.TRD = TRD;
                            c++;

                            this.Purlins.Add(pp);
                            //c = AddBlocoPurlin(c, id_terca, Vao, Esquerda.Transpasse, Direita.Transpasse, origembloco, cre, new List<double>(), furos_m_esq, new List<double>());
                        }
                    }

                    purlin.Mapeado = true;
                }

                if(this.Purlins.Count>0)
                {
                    var TREs = this.Purlins.Select(x => x.TRE).Distinct().ToList();
                    var TRDs = this.Purlins.Select(x => x.TRD).Distinct().ToList();
                    this.Esquerda.Transpasse = TREs.Max();
                    this.Direita.Transpasse = TRDs.Max();
                }

               
            }

            CADPurlin.AddBarra();

            return c;
        }

        public string PurlinPadrao
        {
            get
            {
                if(this.GetPecaPurlin()!=null)
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

        [Browsable(false)]
        public Eixo Esquerda { get; private set; } = new Eixo();
        [Browsable(false)]
        public Eixo Direita { get; private set; } = new Eixo();

        public double Vao
        {
            get
            {
                return Direita.Vao;
            }
        }

        public double CentroX
        {
            get
            {
                return Esquerda.Xmin + (Vao / 2);
            }
        }
        public VaoObra(Eixo esquerda, Eixo direita)
        {
            this.Esquerda = esquerda;
            this.Direita = direita;
        }
    }
}
