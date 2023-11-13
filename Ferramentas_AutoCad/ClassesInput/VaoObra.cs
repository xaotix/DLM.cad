using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DLM.cad
{
    public class VaoObra
    {
        private System.Windows.Controls.Button cota { get; set; }
        public List<UIElement> GetCanvas()
        {
            List<UIElement> retorno = new List<UIElement>();

            var p0 = this.Grade.P0;
            var escala = this.Grade.Escala;

            /*Cotas*/
            var pt = new System.Windows.Point((this.CentroX - p0.X) * escala, (this.Ymax - p0.Y) * escala);
            cota = this.Vao.String(0).Botao(pt, System.Windows.Media.Brushes.Cyan, Core.GetCADPurlin().Canvas_Tam_Texto);
            cota.MouseEnter += Grade.evento_Botao_Sobre;
            cota.MouseLeave += Grade.evento_Botao_Sai;
            cota.ToolTip = this;
            retorno.Add(cota);


            /*blocos*/
            retorno.AddRange(this.GetTirantes().SelectMany(x => x.GetCanvas()));
            retorno.AddRange(this.GetCorrentes().SelectMany(x => x.GetCanvas()));
            retorno.AddRange(this.GetPurlins().SelectMany(x => x.GetCanvas()));

            return retorno;
        }
        private List<ObjetoPurlin> _purlins { get; set; } = new List<ObjetoPurlin>();
        private List<ObjetoPurlin> _purlinsDummy { get; set; } = new List<ObjetoPurlin>();


        public Tipo_Vao Tipo { get; private set; } = Tipo_Vao.Intermediario;
        public override string ToString()
        {
            return $"[{this.Esquerda.Nome}] - {this.Vao } - [{this.Direita.Nome}]";
        }


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

            if (Vao >= Core.GetCADPurlin().VaoMinimo && Vao <= Core.GetCADPurlin().VaoMaximo)
            {

                var purlins = Ut.MlinesPassando(Esquerda.Origem, Direita.Origem, Core.GetCADPurlin().GetMLPurlins());
                var correntes = Ut.MlinesPassando(Esquerda.Origem, Direita.Origem, Core.GetCADPurlin().GetMLCorrentes(), true);



                foreach (var purlin in purlins)
                {

                    double TRE = Core.GetCADPurlin().TranspassePadrao;
                    double TRD = Core.GetCADPurlin().TranspassePadrao;
                    var p_esq = purlin.Inicio;
                    var p_dir = purlin.Fim;

                    if (p_esq.X >= Esquerda.MinX && p_dir.X <= Direita.MinX)
                    {
                        //purlin está dentro do eixo
                        CentroX = p_esq.X + ((p_dir.X - p_esq.X) / 2);
                        TRE = Math.Round(Esquerda.MinX - p_esq.X);
                        TRD = Math.Round(p_dir.X - Direita.MinX);
                    }

                    if (p_dir.X < Direita.MinX + Core.GetCADPurlin().PurlinToleranciaXMapeamento)
                    {
                        //se a linha da purlin for menor que a soma do transpasse 
                        TRD = Math.Round(p_dir.X - Direita.MinX);
                    }

                    if (p_esq.X > Esquerda.MinX - Core.GetCADPurlin().PurlinToleranciaXMapeamento)
                    {
                        //se a linha da purlin for menor que a soma do transpasse
                        TRE = Math.Round(Esquerda.MinX - p_esq.X);
                    }

                    //mapeia as correntes
                    P3d origembloco = new P3d(Math.Round(CentroX), Math.Round(p_esq.Y));

                    P3d centro = new P3d(origembloco.X, origembloco.Y);


                    //desloca a origem do bloco
                    if (TRE < 0 | TRD < 0)
                    {
                        //desloca para a direita
                        var pe = new P3d(Esquerda.MinX, Esquerda.MinY, Esquerda.Z);
                        var pd = new P3d(Direita.MinX, Direita.MinY, Direita.Z);

                        if (TRE < 0)
                        {
                            pe = new P3d(pe.X + Math.Abs(TRE), pe.Y, pe.Z);
                        }

                        if (TRD < 0)
                        {
                            pd = new P3d(pd.X + TRD, pd.Y, pd.Z);
                        }

                        double cmp = pd.X - pe.X;

                        origembloco = new P3d(pe.X + (cmp / 2), p_esq.Y);

                    }



                    List<double> cre = new List<double>();
                    foreach (var corrente in correntes)
                    {
                        var crp0 = corrente.Inicio;
                        var crp1 = corrente.Fim;

                        crp0 = new P3d(Math.Round(crp0.X), Math.Round(crp0.Y));
                        crp1 = new P3d(Math.Round(crp1.X), Math.Round(crp1.Y));

                        //se está passando pela terça
                        if (crp0.Y + ToleranciaPasse + purlin.Largura >= centro.Y && crp1.Y - ToleranciaPasse - purlin.Largura <= centro.Y)
                        {
                            cre.Add(Math.Round(crp0.X - Esquerda.MinX));
                        }
                    }
                    cre = cre.Distinct().ToList().OrderBy(x => x).ToList();

                    List<double> furos_m_esq = new List<double>();
                    if (Core.GetCADPurlin().MapeiaFurosManuais)
                    {
                        var lista = Ut.LinhasPassando(Esquerda.Origem, Direita.Origem, Core.GetCADPurlin().LinhasFuros(), true, true, true);
                        foreach (var ls in lista)
                        {
                            P3d crp0 = new P3d();
                            P3d crp1 = new P3d();
                            P3d cc = new P3d();
                            double angulo, comprimento, largura = 0;

                            Ut.GetCoordenadas(ls, out crp0, out crp1, out angulo, out comprimento, out cc, out largura);
                            crp0 = new P3d(Math.Round(crp0.X), Math.Round(crp0.Y), 0);
                            crp1 = new P3d(Math.Round(crp1.X), Math.Round(crp1.Y), 0);

                            //se está passando pela terça
                            if (crp0.Y + 5 >= centro.Y && crp1.Y - 5 <= centro.Y)
                            {
                                furos_m_esq.Add(Math.Round(crp0.X - Esquerda.MinX));
                            }

                        }
                        furos_m_esq = furos_m_esq.Distinct().ToList().OrderBy(x => x).ToList();
                    }


                    var comp = Vao + TRE + TRD;
                    if (comp >= Core.GetCADPurlin().PurlinCompMin && comp <= Core.GetCADPurlin().PurlinCompMaximo)
                    {
                        //se a multiline é maior que o vão, se verifica se o comprimento em questão é maior que o balanço máximo.
                        if (comp > Core.GetCADPurlin().PurlinBalancoMax)
                        {

                            ObjetoPurlin pp = new ObjetoPurlin(purlin, this);
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

            Core.GetCADPurlin().AddBarra();

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

                var tirantes = Ut.MlinesPassando(Esquerda.Origem, Direita.Origem, Core.GetCADPurlin().GetMLTirantes(), true, Core.GetCADPurlin().TirantesToleranciaXMapeamento);

                foreach (var ml in tirantes)
                {

                    ObjetoTirante pp = new ObjetoTirante(ml, this);
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
                var correntes = Ut.MlinesPassando(pe, pd, Core.GetCADPurlin().GetMLCorrentes(), true);
                var purlins = this.GetPurlins();
                foreach (var corrente in correntes)
                {



                    if (purlins.Count > 1)
                    {
                        for (int i = 1; i < purlins.Count; i++)
                        {
                            var pur1 = purlins[i - 1];
                            var pur2 = purlins[i];
                            double comp = Math.Abs(Math.Round(pur2.Multiline.Centro.Y - pur1.Multiline.Centro.Y));
                            var centro = pur1.Multiline.Centro.Centro(pur2.Multiline.Centro);
                            centro = new P3d(corrente.MinX, centro.Y);

                            /*verifica se a corrente tem um comp min ok e se está dentro de 2 purlin*/
                            if (comp >= Core.GetCADPurlin().CorrenteCompMin && centro.X >= pur1.X1 && centro.X <= pur1.X2 && centro.X > pur2.X1 && centro.X <= pur2.X2)
                            {

                                ObjetoCorrente pp = new ObjetoCorrente(corrente, centro,this, pur1, pur2);
                                pur1.Correntes.Add(pp);
                
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
                return Esquerda.MinX + (Vao / 2);
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
                return Esquerda.MaxY > Direita.MaxY ? Esquerda.MaxY : Direita.MaxY;
            }
        }
        public GradeEixos Grade { get; private set; }
        public VaoObra(GradeEixos Grade ,Eixo esquerda, Eixo direita, Tipo_Vao tipo)
        {
            this.Grade = Grade;
            this.Esquerda = esquerda;
            this.Direita = direita;

            this.Tipo = tipo;

            MapearPurlins();

            GetCorrentes();
            GetTirantes();
        }
    }
}
