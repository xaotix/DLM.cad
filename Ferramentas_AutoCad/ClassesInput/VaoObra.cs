using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ferramentas_DLM
{
    public class VaoObra
    {
        public System.Windows.Controls.Button cota { get; set; }
        public List<UIElement> GetCanvas(Point p0, double escala, double tam_texto)
        {
            List<UIElement> retorno = new List<UIElement>();


            /*Cotas*/
            var pt = new System.Windows.Point((this.CentroX - p0.X) * escala, (this.Ymax - p0.Y) * escala);
            cota = Conexoes.FuncoesCanvas.Botao(this.Vao.ToString(), pt, Conexoes.FuncoesCanvas.Cores.Cyan,  tam_texto);
            cota.MouseEnter += Grade.evento_Botao_Sobre;
            cota.MouseLeave += Grade.evento_Botao_Sai;
            cota.ToolTip = this;
            retorno.Add(cota);
            

            /*blocos*/
            retorno.AddRange(this.GetTirantes().SelectMany(x => x.GetCanvas(p0,escala,tam_texto)));
            retorno.AddRange(this.GetCorrentes().SelectMany(x => x.GetCanvas(p0,escala,tam_texto)));
            retorno.AddRange(this.GetPurlins().SelectMany(x => x.GetCanvas(p0,escala,tam_texto)));

            return retorno;
        }
        private List<ObjetoPurlin> _purlins { get; set; } = new List<ObjetoPurlin>();
        private List<ObjetoPurlin> _purlinsDummy { get; set; } = new List<ObjetoPurlin>();

        public Visibility LE_Visivel
        {
            get
            {
                return Visibility.Visible;
            }
        }
        public Visibility LD_Visivel
        {
            get
            {
                if (this.Tipo == Tipo_Vao.Borda_Direito)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        public Tipo_Vao Tipo { get; private set; } = Tipo_Vao.Intermediario;
        public override string ToString()
        {
            return $"[{this.Esquerda.Nome}] - {this.Vao } - [{this.Direita.Nome}]";
        }
        public CADPurlin CADPurlin { get; set; }

        public List<ObjetoPurlin> GetPurlins()
        {
            return _purlins;
        }

        private void SetPurlins(List<ObjetoPurlin> value)
        {
            _purlins = IndexarLista(value);
        }
        public List<ObjetoPurlin> PurlinsDummy
        {
            get
            {
                return _purlinsDummy;
            }
        }

        public void SetPurlinsDummy(List<ObjetoPurlin> value)
        {
            _purlinsDummy = IndexarLista(value);
        }

        public int MapearPurlins()
        {
            int c = 1;
            double ToleranciaPasse = 2;
            var CentroX = this.CentroX;

            this._purlins.Clear();

            if (Vao >= CADPurlin.VaoMinimo && Vao <= CADPurlin.VaoMaximo)
            {

                var purlins = Utilidades.MlinesPassando(Esquerda.Origem, Direita.Origem, CADPurlin.GetMultLinePurlins());
                var correntes = Utilidades.MlinesPassando(Esquerda.Origem, Direita.Origem, CADPurlin.GetMultLinesCorrentes(), true);

                CADPurlin.AddMensagem("\n" + correntes.Count + " correntes encontradas");
                CADPurlin.AddMensagem("\n" + purlins.Count + " correntes encontradas");

                foreach (var purlin in purlins)
                {
                    CADPurlin.AddBarra();
                    CADPurlin.AddMensagem("\nMapeando Purlin...");
                    double TRE = CADPurlin.TranspassePadrao;
                    double TRD = CADPurlin.TranspassePadrao;
                    Point3d p_esq = purlin.Inicio.GetPoint();
                    Point3d p_dir = purlin.Fim.GetPoint();

                    if (p_esq.X >= Esquerda.Xmin && p_dir.X <= Direita.Xmin)
                    {
                        //purlin está dentro do eixo
                        CentroX = p_esq.X + ((p_dir.X - p_esq.X) / 2);
                        TRE = Math.Round(Esquerda.Xmin - p_esq.X);
                        TRD = Math.Round(p_dir.X - Direita.Xmin);
                    }

                    if (p_dir.X < Direita.Xmin + this.CADPurlin.PurlinToleranciaXMapeamento)
                    {
                        //se a linha da purlin for menor que a soma do transpasse 
                        TRD = Math.Round(p_dir.X - Direita.Xmin);
                    }

                    if (p_esq.X > Esquerda.Xmin - this.CADPurlin.PurlinToleranciaXMapeamento)
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

                            ObjetoPurlin pp = new ObjetoPurlin(purlin, origembloco, this);
                            pp.FurosCorrentes.AddRange(cre);
                            pp.FurosManuais.AddRange(furos_m_esq);
                            pp.TRE = TRE;
                            pp.TRD = TRD;
                            c++;

                            this._purlins.Add(pp);
                        }
                    }

                    purlin.Mapeado = true;
                }



                this.SetPurlins(IndexarLista(_purlins));

            }

            CADPurlin.AddBarra();

            return c;
        }

        private List<ObjetoPurlin> IndexarLista(List<ObjetoPurlin> pts)
        {
            var Retorno = pts.OrderByDescending(x => x.Y).ToList();

            for (int i = 0; i < Retorno.Count; i++)
            {
                if (i < Retorno.Count - 1)
                {
                    Retorno[i].PurlinEmBaixo = Retorno[i + 1];
                }

                if (i > 0)
                {
                    Retorno[i].PurlinEmCima = Retorno[i - 1];
                }
            }

            return Retorno;
        }


        private List<ObjetoTirante> _tirantes { get; set; }
        public List<ObjetoTirante> GetTirantes(bool update = false)
        {
           
            if(_tirantes==null| update)
            {
                int c = 1;
                _tirantes = new List<ObjetoTirante>();

                var tirantes = Utilidades.MlinesPassando(Esquerda.Origem, Direita.Origem, CADPurlin.GetMultLinesTirantes(), true,this.CADPurlin.TirantesToleranciaXMapeamento);

                foreach (var ml in tirantes)
                {

                    ObjetoTirante pp = new ObjetoTirante(ml, c, this);
                    if(pp.Comprimento>0)
                    {
                    _tirantes.Add(pp);
                    }
                    c++;
                }
            }



            return _tirantes;
        }

        private List<ObjetoCorrente> _correntes { get; set; }
        public List<ObjetoCorrente> GetCorrentes()
        {
            if(_correntes==null)
            {
                int c = 1;
                _correntes = new List<ObjetoCorrente>();
                var pe = Esquerda.Origem;
                var pd = Direita.Origem;
                var correntes = Utilidades.MlinesPassando(pe, pd, CADPurlin.GetMultLinesCorrentes(), true);
                var purlins = this.GetPurlins();
                foreach (var cr in correntes)
                {



                    if (purlins.Count > 1)
                    {
                        for (int i = 1; i < purlins.Count; i++)
                        {
                            var pur1 = purlins[i - 1];
                            var pur2 = purlins[i];
                            double comp = Math.Abs(Math.Round(pur2.Multiline.centro.Y - pur1.Multiline.centro.Y));
                            var centro = pur1.Multiline.centro.GetCentro(pur2.Multiline.centro);
                            centro = new Coordenada(cr.minx, centro.Y, 0);

                            /*verifica se a corrente tem um comp min ok e se está dentro de 2 purlin*/
                            if (comp >= this.CADPurlin.CorrenteCompMin && centro.X >= pur1.X1 && centro.X <= pur1.X2 && centro.X > pur2.X1 && centro.X <= pur2.X2)
                            {

                                ObjetoCorrente pp = new ObjetoCorrente(cr, centro.GetPoint(),this);
                                pp.PurlinEmCima = pur1;
                                pur1.Correntes.Add(pp);
                                pp.PurlinEmBaixo = pur2;
                                c++;

                                _correntes.Add(pp);

                            }
                        }
                    }
                }
            }
            return _correntes;
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
        public double Ymax
        {
            get
            {
                if(Esquerda==null| Direita==null)
                {
                    return 0;
                }
                return Esquerda.Ymax > Direita.Ymax ? Esquerda.Ymax : Direita.Ymax;
            }
        }
        public GradeEixos Grade { get; private set; }
        public VaoObra(GradeEixos Grade ,Eixo esquerda, Eixo direita, CADPurlin cADPurlin, Tipo_Vao tipo)
        {
            this.Grade = Grade;
            this.Esquerda = esquerda;
            this.Direita = direita;
            this.CADPurlin = cADPurlin;
            this.Tipo = tipo;

            MapearPurlins();

            GetCorrentes();
            GetTirantes();
        }
    }
}
