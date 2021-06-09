using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.Geometry;
using Ferramentas_DLM.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Ferramentas_DLM.CAD;
using Autodesk.AutoCAD.EditorInput;

namespace Ferramentas_DLM
{
    [Serializable]
    public class CADPurlin : ClasseBase
    {
        #region Opções para configuração
        [Category("Correntes")]
        [DisplayName("Tolerância Passe")]
        public double ToleranciaPasse { get; set; } = 2;

        [Category("Purlin")]
        [DisplayName("Rebater Furos")]
        public bool RebaterFuros { get; set; } = true;
        [Category("Purlin")]
        public bool SBR { get; set; } = false;
        [Category("Purlin")]
        [DisplayName("Transpasse Padrão")]
        public double TranspassePadrao { get; set; } = 337;
        [Category("Purlin")]
        [DisplayName("Vão Mínimo")]
        public double VaoMinimo { get; set; } = 1000;

        [Category("Purlin")]
        [DisplayName("Vão Máximo")]
        public double VaoMaximo { get; set; } = 19000;
        [Category("Purlin")]
        [DisplayName("Comp. Máximo")]
        public double PurlinCompMaximo { get; set; } = 19000;
        [Category("Tirantes")]
        [DisplayName("Tolerância")]
        public double TirantesTolerancia { get; set; } = 50;
        [Category("Tirantes")]
        [DisplayName("Comprimento Máximo")]
        public double TiranteMaxComp { get; set; } = 6000;
        [Category("Correntes")]
        [DisplayName("Comp. Mínimo")]
        public double CorrenteCompMin { get; set; } = 500;
        [Category("Purlin")]
        [DisplayName("Comp. Mín.")]
        public double PurlinCompMin { get; set; } = 400;
        [Category("Purlin")]
        [DisplayName("Balanço Máx.")]
        public double PurlinBalancoMax { get; set; } = 1600;
        [Category("Correntes")]
        [DisplayName("Descontar")]
        public double CorrenteDescontar { get; set; } = 20;
        [Category("Purlin")]
        [DisplayName("Mapear Fr. Man.")]
        public bool MapeiaFurosManuais { get; set; } = true;
        [Category("Purlin")]
        [DisplayName("Offset Apoio")]
        public double OffsetApoio { get; set; } = 0;
        [Category("Purlin")]
        [DisplayName("id")]
        [ReadOnly(true)]
        public int id_terca { get; set; } = 1763;
        [Category("Purlin")]
        [DisplayName("Tipo")]
        [ReadOnly(true)]
        public string tipo { get; set; } = "Z";
        [Category("Purlin")]
        [DisplayName("Seção")]
        [ReadOnly(true)]
        public string secao { get; set; } = "360";
        [Category("Purlin")]
        [DisplayName("Espessura")]
        [ReadOnly(true)]
        public string espessura { get; set; } = "1.55";
        [Category("Purlin")]
        [DisplayName("Ficha de Pintura")]
        [ReadOnly(true)]
        [Browsable(false)]
        public string FichaDePintura { get; set; } = "SEM PINTURA";
        [Category("Purlin")]
        [DisplayName("Layer Fr. Manuais")]
        public string MapeiaFurosManuaisLayer { get; set; } = "FUROS_MANUAIS";
        [Category("Correntes")]
        [DisplayName("Tipo")]
        public string CorrenteTipo { get; set; } = "DLDA$C$";
        [Category("Correntes")]
        [DisplayName("Fixador")]
        public string CorrenteFixador { get; set; } = "F156";
        [Category("Tirantes")]
        [DisplayName("MLStyle")]
        public List<string> TirantesMLStyles { get; set; } = new List<string> { "10MM" };
        [Category("Correntes")]
        [DisplayName("MLStyle")]
        public List<string> CorrenteMLStyles { get; set; } = new List<string> {"L32X32X3MM","DIAG50X3","CR"};

        [Category("Purlin")]
        [DisplayName("MLStyle")]
        public List<string> TercasMLStyles { get; set; } = new List<string> { "Z360", "Z185", "Z292", "Z216", "ZZ360","TERCA" };
        [Category("Tirantes")]
        [DisplayName("Mapear")]
        public bool MapearTirantes { get; set; } = true;
        [Category("Correntes")]
        [DisplayName("Mapear")]
        public bool MapearCorrentes { get; set; } = true;
        [Category("Purlin")]
        [DisplayName("Mapear")]
        public bool MapearTercas { get; set; } = true; 
        #endregion

        public List<BlockReference> Getblocos_tercas()
        {
            return this.Getblocos().FindAll(x => x.Name.ToUpper() == "TERCA_INDICACAO");
        }
        public List<BlockReference> Getblocos_tirantes()
        {
            return this.Getblocos().FindAll(x => x.Name.ToUpper() == "TIRANTE_INDICACAO");
        }
        public List<BlockReference> Getblocos_correntes()
        {
            return this.Getblocos().FindAll(x => x.Name.ToUpper() == "CORRENTE_INDICACAO");
        }
        public List<CCorrente> LinhasCorrentes()
        {
            List<CCorrente> retorno = new List<CCorrente>();
            var lista = Utilidades.MlinesVerticais(this.Getmultilines(), this.CorrenteCompMin);


            List<MlineStyle> estilos = new List<MlineStyle>();
            foreach (var s in this.CorrenteMLStyles)
            {
                var st = Utilidades.GetEstilo(s);
                if (st != null)
                {
                    estilos.Add(st);
                }
            }

            foreach (var l in lista)
            {
                if (estilos.Find(x => x.ObjectId == l.Style) != null)
                {
                    retorno.Add(new CCorrente(l));
                }
            }
            return retorno;
        }
        public List<CTirante> LinhasTirantes()
        {
            List<CTirante> retorno = new List<CTirante>();
            var lista = this.Getmultilines();


            List<MlineStyle> estilos = new List<MlineStyle>();
            foreach (var s in this.TirantesMLStyles)
            {
                var st = Utilidades.GetEstilo(s);
                if (st != null)
                {
                    estilos.Add(st);
                }
            }

            foreach (var l in lista)
            {
                if (estilos.Find(x => x.ObjectId == l.Style) != null)
                {
                    retorno.Add(new CTirante(l));
                }
            }
            return retorno;
        }

        private List<CTerca> _purlins { get; set; }
        private List<CTerca> GetCtercas(bool update = false)
        {
            if(_purlins==null | update)
            {
                _purlins = new List<CTerca>();
                var lista = Utilidades.MlinesHorizontais(this.Getmultilines(), this.PurlinCompMin);


                List<MlineStyle> estilos = new List<MlineStyle>();
                foreach (var s in this.TercasMLStyles)
                {
                    var st = Utilidades.GetEstilo(s);
                    estilos.Add(st);
                }

                foreach (var l in lista)
                {
                    if (estilos.Find(x => x.ObjectId == l.Style) != null)
                    {
                        _purlins.Add(new CTerca(l));
                    }
                }

            }

            return _purlins;

        }



        private List<Mline> mlines_verticais { get; set; } = new List<Mline>();
        private List<CCorrente> correntes { get; set; } = new List<CCorrente>();

        public void Mapear()
        {
            var sel = SelecionarObjetos();
            if (sel.Status == PromptStatus.OK)
            {

                List<BlockReference> blocos = new List<BlockReference>();
                if (MapearTercas)
                {
                    blocos.AddRange(this.Getblocos_tercas());
                }
                if (MapearTirantes)
                {
                    blocos.AddRange(this.Getblocos_tirantes());
                }

                if (MapearCorrentes)
                {
                    blocos.AddRange(this.Getblocos_correntes());
                }

                Apagar(blocos.Select(x => x as Entity).ToList());
                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {






                    this.mlines_verticais = Utilidades.MlinesVerticais(this.Getmultilines(), 100);

                    this.correntes = this.LinhasCorrentes().OrderBy(x => x.minx).ToList();

                    int c = 0;


                    FLayer.Set(LayerBlocos, true, true);

                       var eixos = this.GetEixos();
                    var verticais = eixos.GetEixosVerticais();
                    if (MapearTercas && verticais.Count>1)
                    {
                        for (int i = 1; i < verticais.Count; i++)
                        {
                            AddBarra();
                            var e0 = verticais[i - 1];
                            var e1 = verticais[i];
                            c = MapeiaPurlins(c, e0, e1);
                        }

 


                        foreach (var s in this.GetCtercas().FindAll(x => !x.Mapeado && x.comprimento >= this.PurlinCompMin))
                        {
                            //adiciona as purlins pequenas fora do vão.
                            //essa parte precisa emplementar melhor para mapear furos manuais e correntes.
                            c = AddPurlin(c, Math.Round(s.comprimento), 0, 0, s.centro.GetPoint(), new List<double>(), new List<double>(), new List<double>(), new List<double>());
                        }
                    }


                    if (MapearTirantes)
                    {
                        c = MapeiaTirantes(c);
                    }

                    if (MapearCorrentes)
                    {
                        c = MapeiaCorrentes(c);
                    }

                    AddBarra();
                    AddMensagem("\n" + blocos.Count.ToString() + " blocos encontrados excluídos");
                    AddMensagem("\n" + this.GetEixos_Linhas().Count.ToString() + " Linhas eixo encontradas");
                    AddMensagem("\n" + this.GetEixos_PolyLines().Count.ToString() + " PolyLinhas eixo encontradas");
                    AddMensagem("\n" + this.Getmultilines().Count.ToString() + " Multilines encontradas");
                    AddMensagem("\n" + this.mlines_verticais.Count.ToString() + " Mlines Verticais");
                    AddMensagem("\n" + this.LinhasFuros().Count.ToString() + " Linhas de furos manuais");
                    AddBarra();
                    AddMensagem("\n" + this.LinhasTirantes().Count + " Tirantes");
                    AddMensagem("\n" + this.GetCtercas().Count.ToString() + " Purlins");
                    AddMensagem("\n" + this.correntes.Count.ToString() + " Correntes");
                    AddBarra();
                    acTrans.Commit();
                }

            }
        }

        public void GetBoneco_Purlin()
        {
            List<double> retorno = new List<double>();

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {
                    foreach(var s in Getlinhas())
                    {
                        AddMensagem($"\nLinha: {s.StartPoint.ToString()} Comprimento: {s.Length} Angulo: {Math.Round(Conexoes.Utilz.RadianosParaGraus(s.Angle))}");
                    }
                    var linhas_horizon = Getlinhas_Horizontais();

                    var verticais = Getlinhas_Verticais();

                    AddMensagem($"\nLinhas verticais: {verticais.Count}");
                    AddMensagem($"\nLinhas horizontais: {linhas_horizon.Count}");

                    List<string> ret = new List<string>();
                    List<string> textos = new List<string>();

                    int c = 1;
                    foreach (var lhs in linhas_horizon)
                    {
                        var min_x = lhs.StartPoint.X < lhs.EndPoint.X ? lhs.StartPoint.X : lhs.EndPoint.X;
                        var max_x = lhs.StartPoint.X > lhs.EndPoint.X ? lhs.StartPoint.X : lhs.EndPoint.X;


                        var verts = verticais.FindAll(x =>x.StartPoint.X>min_x+1 && x.EndPoint.X<max_x-1);
                        List<Line> passa = new List<Line>();

                        foreach (var v in verts)
                        {
                           
                            var max_y = v.StartPoint.Y > v.EndPoint.Y ? v.StartPoint.Y : v.EndPoint.Y;
                            var min_y = v.StartPoint.Y < v.EndPoint.Y ? v.StartPoint.Y : v.EndPoint.Y;
                            if (max_y >= lhs.StartPoint.Y && min_y <= lhs.StartPoint.Y)
                            {
                                passa.Add(v);
                            }
                        }
                        AddMensagem($"Linhas verticais que passam: {passa.Count}");

                        double comprimento = Math.Round(Math.Abs(lhs.StartPoint.X - lhs.EndPoint.X));
                        List<double> furos = passa.Select(x => x.StartPoint.X - min_x).ToList().OrderBy(x=>x).ToList();
                        furos = furos.Distinct().ToList();
                        textos.Add($"@Linha {c}");
                        textos.Add($"Comprimento: {comprimento}");
                        textos.Add($"Origem:{lhs.StartPoint.ToString()}");

                        textos.Add($"Coordenadas:{furos.Count}");
                        textos.Add("$Inicio");
                        textos.AddRange(furos.Select(x => Math.Round(x).ToString()));
                        textos.Add("$Fim");


                        Utilidades.AddLeader(-45, lhs.StartPoint,this.Getescala(), "Linha:" + c, 15 * .8);
                        c++;
                    }
                 
                    var dest = Conexoes.Utilz.getPasta(acDoc.Name) + $@"\{Conexoes.Utilz.getNome(acDoc.Name)}_boneco.txt";
                   if(textos.Count>0)
                    {
                        Conexoes.Utilz.Arquivo.Gravar(dest, textos);
                        Conexoes.Utilz.Abrir(dest);
                    }
                   else
                    {
                        AddMensagem("\nNada encontrado.");
                    }

                    c++;

                }

            }
        }

        private int MapeiaCorrentes(int c)
        {
            foreach (var cr in this.correntes)
            {
                var purlins = this.GetPurlinsPassando(cr);
                AddBarra();
                AddMensagem("\n Corrente " + cr.centro + " ");
                AddMensagem("\n" + purlins.Count.ToString() + " Purlins Passando");

                if (purlins.Count > 1)
                {
                    for (int i = 1; i < purlins.Count; i++)
                    {
                        var pur1 = purlins[i-1];
                        var pur2 = purlins[i];
                        double comp = Math.Abs(Math.Round(pur2.centro.Y - pur1.centro.Y));
                        var centro = pur1.centro.GetCentro(pur2.centro);
                        centro = new Coordenada(cr.minx, centro.Y, 0);
                        if (comp >= this.CorrenteCompMin)
                        {
                            AddCorrente(c, centro.GetPoint(), comp, this.CorrenteDescontar, this.CorrenteTipo, this.CorrenteFixador);
                            c++;
                        }
                    }
                }
            }

            return c;
        }

        public List<CTerca> GetPurlinsPassando(CCorrente corrente)
        {
            return this
                .GetCtercas()
                .FindAll(x => 
                x.centro.Y >= corrente.miny-5 
                && 
                x.centro.Y <= corrente.maxy+5
                )
                .FindAll(x => 
                x.minx <= corrente.minx 
                && 
                x.maxx >= corrente.minx
                
                ).OrderBy(x=>x.miny).ToList();
        }

        private int MapeiaTirantes(int c)
        {
            foreach (var p in this.LinhasTirantes())
            {

                if (p.comprimento <= this.TiranteMaxComp)
                {
                    AddTirante(c, p.centro.GetPoint(), Math.Round(p.comprimento));
                    c++;
                }
            }

            return c;
        }

        private int MapeiaPurlins(int c, Eixo e0, Eixo e1)
        {
            var VAO = Math.Round(Math.Abs(e1.Xmin - e0.Xmin));
            AddMensagem("\nVÃO " + VAO + " - " + c.ToString().PadLeft(2,'0'));

            if (VAO >= this.VaoMinimo && VAO<=this.VaoMaximo)
            {

                var purlins = Utilidades.MlinesPassando(e0.Origem, e1.Origem, this.GetCtercas());
                var correntes = Utilidades.MlinesPassando(e0.Origem, e1.Origem, this.correntes,true,true);
                AddMensagem("\n" + correntes.Count + " correntes encontradas");
                var centroxx = e0.Xmin + (VAO / 2);


                foreach (var p in purlins)
                {
                    AddBarra();
                    AddMensagem("\nMapeando Purlin...");

                    Point3d p0 = p.p0.GetPoint();
                    Point3d p1 = p.p1.GetPoint();
                    //AddMensagem("purlin " + p0 + " - " + p1);

                    double TRE = this.TranspassePadrao;
                    double TRD = this.TranspassePadrao;


                    if (p0.X >= e0.Xmin && p1.X <= e1.Xmin)
                    {
                        //purlin está dentro do eixo
                        centroxx = p0.X + ((p1.X - p0.X) / 2);
                        TRE = Math.Round(e0.Xmin - p0.X);
                        TRD = Math.Round(p1.X - e1.Xmin);
                    }

                    if (p1.X < e1.Xmin + 1500)
                    {
                        //se a linha da purlin for menor que a soma do transpasse 
                        TRD = Math.Round(p1.X - e1.Xmin);
                    }

                    if (p0.X > e0.Xmin - 1500)
                    {
                        //se a linha da purlin for menor que a soma do transpasse
                        TRE = Math.Round(e0.Xmin - p0.X);
                    }

                    //mapeia as correntes
                    Point3d origembloco = new Point3d(Math.Round(centroxx), Math.Round(p0.Y), 0);

                    Point3d centro = new Point3d(origembloco.X, origembloco.Y, 0);
                    AddMensagem("\n" + origembloco + " centro do vão");

                    //desloca a origem do bloco
                    if (TRE < 0 | TRD < 0)
                    {
                        //desloca para a direita
                        var pe = new Point3d(e0.Xmin, e0.Ymin, e0.Z);
                        var pd = new Point3d(e1.Xmin, e1.Ymin, e1.Z);

                        if (TRE < 0)
                        {
                            pe = new Point3d(pe.X + Math.Abs(TRE), pe.Y, pe.Z);
                        }

                        if (TRD < 0)
                        {
                            pd = new Point3d(pd.X + TRD, pd.Y, pd.Z);
                        }

                        double cmp = pd.X - pe.X;

                        origembloco = new Point3d(pe.X + (cmp / 2), p0.Y, 0);

                    }

                    AddMensagem("\n" + origembloco + " centro do bloco");

                    List<double> cre = new List<double>();
                    List<double> crd = new List<double>();
                    foreach (var corrente in correntes)
                    {
                        Point3d crp0 = corrente.p0.GetPoint();
                        Point3d crp1 = corrente.p1.GetPoint();
        
                        crp0 = new Point3d(Math.Round(crp0.X), Math.Round(crp0.Y), 0);
                        crp1 = new Point3d(Math.Round(crp1.X), Math.Round(crp1.Y), 0);


                        //se está passando pela terça
                        if (crp0.Y + ToleranciaPasse >= centro.Y && crp1.Y - ToleranciaPasse <= centro.Y)
                        {
                            if ((crp0.X < centroxx | crp1.X < centroxx))
                            {
                                cre.Add(Math.Round(crp0.X - e0.Xmin));
                            }
                            else
                            {
                                crd.Add(Math.Round(e1.Xmin - crp0.X));
                            }
                        }

                    }
                    cre = cre.Distinct().ToList().OrderBy(x => x).ToList();
                    crd = crd.Distinct().ToList().OrderBy(x => x).ToList();



                    List<double> fe = new List<double>();
                    List<double> fd = new List<double>();
                    if (MapeiaFurosManuais)
                    {
                        var lista = Utilidades.LinhasPassando(e0.Origem, e1.Origem, LinhasFuros(), true, true, true);
                        foreach (var ls in lista)
                        {
                            Point3d crp0 = new Point3d();
                            Point3d crp1 = new Point3d();
                            Point3d cc = new Point3d();
                            double angulo = 0;
                            double comprimento = 0;
                            Utilidades.GetCoordenadas(ls, out crp0, out crp1, out angulo, out comprimento,out cc);
                            crp0 = new Point3d(Math.Round(crp0.X), Math.Round(crp0.Y), 0);
                            crp1 = new Point3d(Math.Round(crp1.X), Math.Round(crp1.Y), 0);


                            //se está passando pela terça
                            if (crp0.Y + 5 >= centro.Y && crp1.Y - 5 <= centro.Y)
                            {
                                if ((crp0.X < centroxx | crp1.X < centroxx))
                                {
                                    fe.Add(Math.Round(crp0.X - e0.Xmin));
                                }
                                else
                                {
                                    fd.Add(Math.Round(e1.Xmin - crp0.X));
                                }
                            }

                        }
                        fe = fe.Distinct().ToList().OrderBy(x => x).ToList();
                        fd = fd.Distinct().ToList().OrderBy(x => x).ToList();
                    }

                   

                    var comp = VAO + TRE + TRD;
                    if(comp>=this.PurlinCompMin && comp<=this.PurlinCompMaximo)
                    {
                        //se a multiline é maior que o vão, se verifica se o comprimento em questão é maior que o balanço máximo.
                        if(comp> this.PurlinBalancoMax)
                        {
                            c = AddPurlin(c, VAO, TRE, TRD, origembloco, cre, crd, fe, fd);
                        }
                    }

                    p.Mapeado = true;

                }
            }

            AddBarra();
            return c;
        }
        public Hashtable GetHashtable(Conexoes.Macros.Purlin p)
        {
            Hashtable ht = new Hashtable();

            ht.Add("N", p.Sequencia.ToString().PadLeft(3,'0'));
            ht.Add("CRD", string.Join(";", p.Correntes_Direita));
            ht.Add("CRE", string.Join(";",p.Correntes_Esquerda));
            ht.Add("AD", this.OffsetApoio.ToString());
            ht.Add("AE", this.OffsetApoio.ToString());
            ht.Add("FBD", "");
            ht.Add("FBE", "");
            ht.Add("REB", p.Rebater_Furos ? "Sim" : "Não");
            ht.Add("SBR", p.Corrente_SBR ? "Sim" : "Não");
            ht.Add("FD", string.Join(";", p.Direita.Furos_Manuais));
            ht.Add("FE", string.Join(";", p.Esquerda.Furos_Manuais));
            ht.Add("VAO", p.Vao);
            ht.Add("NOME", "");
            ht.Add("TRD", p.Direita.Comprimento);
            ht.Add("TRE", p.Esquerda.Comprimento);
            ht.Add("ID_DB", "");
            ht.Add("PINTURA", p.Pintura);
            ht.Add("ID_PECA", p.id_peca.ToString());
            ht.Add("TIPO", p.Perfil.Contains("C")?"C":"Z");
            ht.Add("SECAO", p.Secao);
            ht.Add("ESP", p.Espessura);

            return ht;
        }
        public int AddPurlin(int c, double VAO, double TRE, double TRD, Point3d origembloco, List<double> cre, List<double> crd, List<double> fe, List<double> fd)
        {
            //AddMensagem("Origem: " + centro + "\n");
            Hashtable ht = new Hashtable();

            AddMensagem("\n" + cre.Count + " correntes esquerdas");
            AddMensagem("\n" + crd.Count + " correntes direitas");

            ht.Add("N", Conexoes.Utilz.getLetra(c));
            ht.Add("CRD", string.Join(";", crd));
            ht.Add("CRE", string.Join(";", cre));
            ht.Add("AD", this.OffsetApoio.ToString());
            ht.Add("AE", this.OffsetApoio.ToString());
            ht.Add("FBD", "");
            ht.Add("FBE", "");
            ht.Add("REB", this.RebaterFuros ? "Sim" : "Não");
            ht.Add("SBR", this.SBR ? "Sim" : "Não");
            ht.Add("FD", string.Join(";", fd));
            ht.Add("FE", string.Join(";", fe));
            ht.Add("VAO", VAO);
            ht.Add("NOME", "");
            ht.Add("TRD", TRD);
            ht.Add("TRE", TRE);
            ht.Add("ID_DB", "");
            ht.Add("PINTURA", this.FichaDePintura);
            ht.Add("ID_PECA", this.id_terca.ToString());
            ht.Add("TIPO", this.tipo);
            ht.Add("SECAO", this.secao);
            ht.Add("ESP", this.espessura);

            //quando a purlin está deslocada.


            double comp_sem_transpasse = VAO + (TRE < 0 ? TRE : 0) + (TRD < 0 ? TRD : 0);
            //verifica se a purlin é maior que 150
            if (comp_sem_transpasse > 150)
            {
                Blocos.Inserir(CAD.acDoc, Constantes.Incicacao_Tercas, origembloco, this.Getescala(), 0, ht);
                c++;
            }

            return c;
        }
        public int AddTirante(int c,  Point3d origembloco, double Comp, double offset1 = -72, double offset2 = -72,string TIP = "03TR", string sfta = "STF-01", string sftb = "STF-02")
        {
            //AddMensagem("Origem: " + centro + "\n");
            Hashtable ht = new Hashtable();
            ht.Add("N", Conexoes.Utilz.getLetra(c));

            ht.Add("COMP", Comp.ToString());
            ht.Add("OFFSET1", offset1.ToString());
            ht.Add("OFFSET2", offset2.ToString());

            ht.Add("TIP", TIP.ToString());
            ht.Add("SFTA", sfta.ToString());
            ht.Add("SFTB", sftb.ToString());

            Blocos.Inserir(CAD.acDoc, Constantes.Indicacao_Tirantes, origembloco, this.Getescala(), 0, ht);
            c++;

            return c;
        }



        public int AddCorrente(int c, Point3d origembloco, double Comp, double desc = 18, string tip = "DLDA", string fix = "F156")
        {
            //AddMensagem("Origem: " + centro + "\n");
            Hashtable ht = new Hashtable();
            ht.Add("N", Conexoes.Utilz.getLetra(c));
            ht.Add("TIP", tip);
            ht.Add("DESC", desc.ToString());
            ht.Add("COMP", Comp.ToString());
            ht.Add("FIX", fix);



            Blocos.Inserir(CAD.acDoc, Constantes.Indicacao_Correntes, origembloco, this.Getescala(), 0, ht);
            c++;

            return c;
        }
        private List<Entity> LinhasFuros()
        {
            return this.selecoes.FindAll(x => x.Layer.ToUpper().Replace(" ", "") == this.MapeiaFurosManuaisLayer.ToUpper().Replace(" ", "") && (x is Line | x is Polyline));
        }

        public void SetPerfil()
        {
            var perfil = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetTercas(),null,"Selecione");
            if(perfil==null)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                   foreach(var s in this.Getblocos_tercas())
                    {
                        Hashtable tt = new Hashtable();
                        tt.Add("ID_PECA", perfil.id_db.ToString());
                        tt.Add("ESP", perfil.ESP.ToString());
                        tt.Add("SECAO", perfil.GetCadastroRME().SECAO.ToString());
                        tt.Add("TIPO", perfil.TIPO.Contains("C")?"C":"Z");

                        Atributos.Set(s, acTrans, tt);
                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void EdicaoCompleta()
        {


            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    var s = this.Getblocos_tercas().Select(x => GetPurlin(x)).ToList();
                    if(s.Count>0)
                    {
                        var st = Conexoes.Utilz.Editar(s);
                        if(Conexoes.Utilz.Pergunta("Salvar edições?"))
                        {
                            foreach(var p in st)
                            {
                                var ll = p.Objeto as BlockReference;
                                Atributos.Set(ll, acTrans, GetHashtable(p));
                            }
                        }
                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void PurlinManual()
        {
            bool cancelado = false;
            var pt1 = Utilidades.PedirPonto3D("Selecione o ponto de origem", out cancelado);
            if(cancelado)
            {
                return;
            }
            var pt2 = Utilidades.PedirPonto3D("Selecione o ponto final", out cancelado);
            if(cancelado)
            {
                return;
            }
            var perfil = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetTercas(), null, "Selecione o perfil");
            if (perfil == null)
            {
                return;
            }

            this.id_terca = perfil.id_db;
            this.secao = perfil.GetCadastroRME().SECAO.ToString();
            this.tipo = perfil.TIPO.Contains("C") ? "C" : "Z";
            this.espessura = perfil.ESP.ToString("N2");

            double comprimento = Math.Round(Math.Abs(pt1.X - pt2.X));
            Coordenada p = new Coordenada(pt1);
           

            if(comprimento> this.PurlinCompMin)
            {
                AddPurlin(0, comprimento, 0, 0, p.GetCentro(pt2).GetPoint(), new List<double>(), new List<double>(), new List<double>(), new List<double>());
            }
            else
            {
                Utilidades.Alerta("Comprimento [" + comprimento + "] inferior ao mínimo possível [" + this.PurlinCompMin + "]");
                return;
            }

            
        }
        public void ExcluirBlocosMarcas()
        {
            var sel = SelecionarObjetos(Tipo_Selecao.Blocos);
            if (sel.Status == PromptStatus.OK)
            {
                List<BlockReference> blocos = new List<BlockReference>();
                blocos.AddRange(this.Getblocos_tercas());
                blocos.AddRange(this.Getblocos_correntes());
                blocos.AddRange(this.Getblocos_tirantes());

                Apagar(blocos.Select(x => x as Entity).ToList());
            }
        }
        public void SetCorrente()
        {
            var perfil = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMEs().FindAll(x=>x.TIPO == "DLD" && x.VARIAVEL).ToList(), null, "Selecione");
            if (perfil == null)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_correntes())
                    {
                        Atributos.Set(s, acTrans, "TIP", perfil.CODIGOFIM.ToString());

                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }

        public void SetCorrenteDescontar()
        {
            var valor = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite","","20"));
            if (valor <0)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_correntes())
                    {
                        Atributos.Set(s, acTrans, "DESC", valor.ToString());

                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void SetCorrenteFixador()
        {
            var valor = Conexoes.Utilz.SelecionarObjeto(new List<string> { "F46", "F76", "F156" },null,"Selecione");
            if (valor ==null)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_correntes())
                    {
                        Atributos.Set(s, acTrans, "FIX", valor.ToString());

                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void Editar(bool editar = false)
        {


            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {
                    if (this.Getblocos_tercas().Count > 0)
                    {
                        Conexoes.Utilz.Editar(GetPurlin(this.Getblocos_tercas()[0]),editar);


                    }
                  
                }
            }
        }

        public void InserirCroquis_Purlin()
        {
           if(SelecionarObjetos(Tipo_Selecao.Blocos).Status!= PromptStatus.OK)
            {
                return;
            }

            var pecas = this.Getblocos_tercas().Select(x => GetPurlin(x)).ToList();
                
                
                
                
             var purlins = pecas.GroupBy(x=>x.GetChave()).ToList();

            if (purlins.Count>0)
            {
                bool cancelado = false;
                double dist = this.Getescala() * 0.25;
                var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                if(!cancelado)
                {

                    double offset = dist;
                    foreach(var p in purlins)
                    {
                        var purlin = p.ToList()[0];
                        Point3d p0 = new Point3d(purlin.Origem.X - purlin.Vao/2, origem.Y - offset, 0);
                        InserirCroqui_Purlin(p.ToList()[0], p0);
                        offset = offset + dist;
                    }
                }
            }

        }


        public void InserirCroqui_Purlin(Conexoes.Macros.Purlin purlin, Point3d pt)
        {
            double fonte = 0.25 * this.Getescala();
            List<Entity> linhas = new List<Entity>();
            Line pp = new Line();
            pp.StartPoint = new Point3d(- purlin.Esquerda.Comprimento,0,0);
            pp.EndPoint = new Point3d(purlin.Vao + purlin.Direita.Comprimento, 0, 0);
            pp.Color = Autodesk.AutoCAD.Colors.Color.FromColor(System.Drawing.Color.Yellow);
            linhas.Add(pp);
            double off_y = 100;
                var xx = - purlin.Esquerda.Comprimento;
            foreach (var s in purlin.GetFurosVista(false))
            {
                Line fr = new Line();


                fr.StartPoint = new Point3d(xx + s.X, - off_y, 0);
                fr.EndPoint = new Point3d(xx + s.X, off_y, 0);
                fr.Color = Autodesk.AutoCAD.Colors.Color.FromColor(purlin.GetCor(s.Posicao).ToColor());

                DBText txt = new DBText();
                txt.Color = (Autodesk.AutoCAD.Colors.Color)fr.Color.Clone();
                txt.TextString = $"{s.Posicao.ToString()} - {s.X}";
                txt.Position = new Point3d(fr.StartPoint.X, fr.StartPoint.Y - 5, 0);
                txt.Rotation = Conexoes.Utilz.GrausParaRadianos(-90);
                txt.Height = fonte;

                linhas.Add(txt);
                linhas.Add(fr);
            }
            DBText nome = new DBText();
            nome.TextString = $"{purlin.Nome}";
            nome.Position = new Point3d(pp.StartPoint.X + (purlin.Vao/2), pp.StartPoint.Y + 125, 0);
            nome.Height = fonte;
            linhas.Add(nome);

            Blocos.Criar("PURLIN_" + purlin.Nome, linhas, pt);
        }

        public void Exportar(bool tabela = true, bool exportar = true)
        {
            try
            {
                string dest = "";
                if (exportar)
                {
                    dest = Conexoes.Utilz.SalvarArquivo("RM");
                }
                if (dest == "" && exportar)
                {
                    return;
                }

                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    var sel = SelecionarObjetos();
                    if (sel.Status == PromptStatus.OK)
                    {
                        Conexoes.DBRM_Offline mm = new Conexoes.DBRM_Offline();
                        var purlins = this.Getblocos_tercas().Select(x => GetPurlin(x));
                        List<Conexoes.Macros.Purlin> ss = JuntarERenomearPurlinsIguais(purlins, acTrans);
                        Point3d p = new Point3d();
                        if (tabela)
                        {
                            bool cancelado = false;
                            var PS = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                            if (!cancelado)
                            {
                                p = Tabelas.Purlins(ss, PS);
                            }
                        }

                        if (MapearTirantes)
                        {
                            var tirantes = this.Getblocos_tirantes().Select(x => GetTirante(x));
                            List<Conexoes.Macros.Tirante> pcs = JuntarTirantesIguais(tirantes, acTrans);
                            if (tabela)
                            {
                                p = Tabelas.Tirantes(pcs, new Point3d(p.X + (119.81 * Getescala()), p.Y, p.Z));
                            }
                            mm.RM_Macros.AddRange(pcs.Select(x => new Conexoes.RME_Macro(x)));
                        }

                        if (MapearCorrentes)
                        {
                            var correntes = this.Getblocos_correntes().Select(x => GetCorrente(x));
                            List<Conexoes.Macros.Corrente> pcs = JuntarCorrentesIguais(correntes, acTrans);
                            if (tabela)
                            {
                                p = Tabelas.Correntes(pcs, new Point3d(p.X + (86.77 * Getescala()), p.Y, p.Z));
                            }
                            mm.RM_Macros.AddRange(pcs.Select(x => new Conexoes.RME_Macro(x)));
                        }

                        mm.RM_Macros.AddRange(ss.Select(x => new Conexoes.RME_Macro(x)));
                        if (exportar)
                        {
                            mm.Salvar(dest);
                        }

                        acTrans.Commit();
                        acDoc.Editor.Regen();
                    }
                }
            }
            catch (Exception ex)
            {
                Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        private List<Conexoes.Macros.Purlin> JuntarERenomearPurlinsIguais(IEnumerable<Conexoes.Macros.Purlin> purlins, OpenCloseTransaction acTrans)
        {
            int c = 1;
            List<Conexoes.Macros.Purlin> ss = new List<Conexoes.Macros.Purlin>();
            purlins = purlins.OrderBy(x => x.ToString()).ToList();
            foreach (var p in purlins.GroupBy(x => x.GetChave()).OrderByDescending(X => X.Count()))
            {
                var pps = p.ToList();
                var nova = pps[0].Clonar();
                nova.Quantidade = pps.Count();
                nova.Sequencia = c;
                foreach (var s in pps.Select(x=>x.Objeto as BlockReference))
                {
                    Atributos.Set(s, acTrans, "N", c.ToString().PadLeft(3, '0'));
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }
        private List<Conexoes.Macros.Tirante> JuntarTirantesIguais(IEnumerable<Conexoes.Macros.Tirante> tirantes, OpenCloseTransaction acTrans)
        {
            int c = 1;
            List<Conexoes.Macros.Tirante> ss = new List<Conexoes.Macros.Tirante>();
            tirantes = tirantes.OrderBy(x => x.ToString()).ToList();
            foreach (var p in tirantes.GroupBy(x => x.Tipo + Conexoes.Utilz.ArredondarMultiplo(x.Comprimento, this.TirantesTolerancia).ToString() + " - " + x.SFTA + "/" + x.SFTB + "/" + x.Tratamento).OrderByDescending(X => X.Count()))
            {
                var pps = p.ToList();
                var nova = pps[0].Clonar();
                nova.Qtd = pps.Count();
                var comp = nova.Comprimento;
                nova.Offset1 = 0;
                nova.Offset2 = 0;
                nova.CompUser = Conexoes.Utilz.ArredondarMultiplo(comp, this.TirantesTolerancia);
                nova.Sequencia = c.ToString().PadLeft(2,'0');
               
                foreach (var s in pps.Select(x => x.Objeto as BlockReference))
                {
                    Atributos.Set(s, acTrans, "Nº", c.ToString().PadLeft(2, '0'));
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }
        private List<Conexoes.Macros.Corrente> JuntarCorrentesIguais(IEnumerable<Conexoes.Macros.Corrente> tirantes, OpenCloseTransaction acTrans)
        {
            int c = 0;
            List<Conexoes.Macros.Corrente> ss = new List<Conexoes.Macros.Corrente>();
            tirantes = tirantes.OrderBy(x => x.ToString()).ToList();
            foreach (var p in tirantes.GroupBy(x => x.ToString()).OrderByDescending(X=>X.Count()))
            {
                
                var pps = p.ToList();
                var nova = pps[0].Clonar();
                nova.Qtd = pps.Count();
                var comp = nova.Comprimento;

                nova.Sequencia = Conexoes.Utilz.getLetra(c); 

                foreach (var s in pps.Select(x => x.Objeto as BlockReference))
                {
                    Atributos.Set(s, acTrans, "Nº", Conexoes.Utilz.getLetra(c));
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }
        public Conexoes.Macros.Purlin GetPurlin(BlockReference bloco)
        {
            var atributos = Atributos.GetLinha(bloco);
            var N = atributos.Get("N").ToString();
            var ESP = atributos.Get("ESP").Double();
            var SECAO = atributos.Get("SECAO").Double();
            var TIPO = atributos.Get("TIPO").ToString();
            var ID_PECA = atributos.Get("ID_PECA").Int;
            var PINTURA = atributos.Get("PINTURA").ToString();
            var ID_DB = atributos.Get("ID_DB").Int;
            var VAO = atributos.Get("VAO").Double();
            var TRE = atributos.Get("TRE").Double();
            var TRD = atributos.Get("TRD").Double();
            var AD = atributos.Get("AD").Double();
            var AE = atributos.Get("AE").Double();
            var REB = atributos.Get("REB").ToString().ToUpper() == "SIM";
            var SBR = atributos.Get("SBR").ToString().ToUpper() == "SIM";

            var NOME = atributos.Get("NOME").ToString();
            var FE = atributos.Get("FE").ToString();
            var FD = atributos.Get("FD").ToString();
            var FBE = atributos.Get("FBE").ToString();
            var FBD = atributos.Get("FBD").ToString();

            var CRE = atributos.Get("CRE").ToString();
            var CRD = atributos.Get("CRD").ToString();

            Conexoes.Macros.Purlin p = new Conexoes.Macros.Purlin();
            p.id_peca = ID_PECA;
            //p.SetPeca(Conexoes.DBases.GetBancoRM().GetTercas().Find(x => x.id_db == ID_PECA));
            p.Objeto = bloco;
            p.Tipo_Corrente = Conexoes.Tipo_Corrente_Purlin.Manual;
            p.Rebater_Furos = REB;
            p.Corrente_SBR = SBR;

            p.Origem = new System.Windows.Media.Media3D.Point3D(bloco.Position.X, bloco.Position.Y, 0);
        
            //CORRENTES RÍGIDAS
            foreach(var s in CRE.Split(';').Select(x=> Conexoes.Utilz.Double(x)).OrderBy(x=>x).ToList().Distinct().ToList())
            {
                p.Correntes_Esquerda.Add(s);
            }
            foreach (var s in CRD.Split(';').Select(x => Conexoes.Utilz.Double(x)).OrderBy(x => x).ToList().Distinct().ToList())
            {
                p.Correntes_Direita.Add(s);
            }
            p.Tipo_Corrente = Conexoes.Tipo_Corrente_Purlin.Manual;

            //FURO MANUAL
            foreach (var s in FE.Split(';').Select(x => Conexoes.Utilz.Double(x)).OrderBy(x => x).ToList().Distinct().ToList())
            {
                p.Esquerda.Furos_Manuais.Add(s);
            }
            foreach (var s in FD.Split(';').Select(x => Conexoes.Utilz.Double(x)).OrderBy(x => x).ToList().Distinct().ToList())
            {
                p.Direita.Furos_Manuais.Add(s);
            }

            p.Esquerda.Tipo_Furo_FB = Conexoes.Tipo_Furo_FB.Manual;
            p.Direita.Tipo_Furo_FB = Conexoes.Tipo_Furo_FB.Manual;


            //FLANGE BRACES
            foreach (var s in FBE.Split(';').Select(x => Conexoes.Utilz.Double(x)).OrderBy(x => x).ToList().Distinct().ToList())
            {
                p.Esquerda.Flange_Braces.Add(s);
            }
            foreach (var s in FD.Split(';').Select(x => Conexoes.Utilz.Double(x)).OrderBy(x => x).ToList().Distinct().ToList())
            {
                p.Direita.Flange_Braces.Add(s);
            }


            if (AE>0)
            {
                p.Esquerda.Tipo_Furo_Apoio = Conexoes.Tipo_Furo_Purlin.Espelhado;
                p.Esquerda.Furo_Apoio_Offset = AE;
            }

            if (AD > 0)
            {
                p.Esquerda.Tipo_Furo_Apoio = Conexoes.Tipo_Furo_Purlin.Espelhado;
                p.Esquerda.Furo_Apoio_Offset = AD;
            }
            /*falta ajustar pra buscar pela espessura e pela secao*/
            p.Vao = VAO;
            p.Esquerda.Comprimento = TRE;
            p.Direita.Comprimento = TRD;
            return p;
        }
        public Conexoes.Macros.Tirante GetTirante(BlockReference bloco)
        {
            var atributos = Atributos.GetLinha(bloco);
            var SFTA = atributos.Get("SFTA").ToString();
            var SFTB = atributos.Get("SFTB").ToString();
            var TIP = atributos.Get("TIP").ToString();
            var OFF1 = atributos.Get("OFF1").Double();
            var OFF2 = atributos.Get("OFF2").Double();
            var COMP = atributos.Get("COMP").Double();

            Conexoes.Macros.Tirante p = new Conexoes.Macros.Tirante();
            p.SFTA = SFTA;
            p.SFTB = SFTB;
            p.Tipo = TIP;
            p.Offset1 = OFF1;
            p.Offset2 = OFF2;
            p.CompUser = COMP;
            p.Objeto = bloco;
            return p;
        }
        public Conexoes.Macros.Corrente GetCorrente(BlockReference bloco)
        {
            var atributos = Atributos.GetLinha(bloco);

            var TIP = atributos.Get("TIP").ToString();
            var DESC = atributos.Get("DESC").Double();
            var COMP = atributos.Get("COMP").Double();
            var FIX = atributos.Get("FIX").ToString();

            Conexoes.Macros.Corrente p = new Conexoes.Macros.Corrente();
            p.Vao = COMP;
            p.Descontar = DESC;
            p.Objeto = bloco;
            p.SetFixacao(FIX);
            var diagonal = Conexoes.DBases.GetBancoRM().GetRME(TIP);
            if(diagonal!=null)
            {
                p.SetDiagonal(diagonal);
            }
            return p;
        }
        public void SetFicha()
        {
            var valor = Conexoes.Utilz.SelecionarObjeto(new Conexoes.its.TipoPintura().GetValues().Select(x => x.Value.ToString()).ToList(),null,"Selecione a ficha");
            if (valor == null)
            {
                return;
            }

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        Atributos.Set(s, acTrans, "PINTURA", valor);
                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void SetTranspasse()
        {
            var trs = Conexoes.Utilz.SelecionarObjeto(new List<string> { "Esquerda", "Direita", "Ambos" }, null, "Selecione");
            if (trs == null)
            {
                return;
            }

            var valor = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite o valor", "", "337"));


            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        if(trs == "Esquerda" | trs == "Ambos")
                        {
                            Atributos.Set(s, acTrans, "TRE", valor.ToString());
                        }

                        if (trs == "Direita" | trs == "Ambos")
                        {
                            Atributos.Set(s, acTrans, "TRD", valor.ToString());
                        }
                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }
        public void SetSuporte()
        {
            var trs = Conexoes.Utilz.SelecionarObjeto(new List<string> { "Esquerda", "Direita", "Ambos" }, null, "Selecione");
            if (trs == null)
            {
                return;
            }

            var tip = Conexoes.Utilz.SelecionarObjeto(new List<string> { "Centralizado", "Offset" }, null, "Selecione");
            if (tip == null)
            {
                return;
            }
            double valor = 0;
            if(tip == "Offset")
            {
                valor = Conexoes.Utilz.Double(Conexoes.Utilz.Prompt("Digite o valor", "", "38"));
            }



            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        if (tip == "Centralizado" )
                        {
                            if(trs == "Ambos")
                            {
                                Atributos.Set(s, acTrans, "AD", "0");
                                Atributos.Set(s, acTrans, "AE", "0");
                            }
                            else if(trs == "Esquerda")
                            {
                                Atributos.Set(s, acTrans, "AE", "0");
                            }
                            else if(trs =="Direita")
                            {
                                Atributos.Set(s, acTrans, "AD", "0");
                            }
                        }
                        else if (tip == "Offset")
                        {
                            if (trs == "Ambos")
                            {
                                Atributos.Set(s, acTrans, "AD", valor.ToString());
                                Atributos.Set(s, acTrans, "AE", "0");
                            }
                            else if (trs == "Esquerda")
                            {
                                Atributos.Set(s, acTrans, "AE", valor.ToString());
                            }
                            else if (trs == "Direita")
                            {
                                Atributos.Set(s, acTrans, "AD", valor.ToString());
                            }
                        }
                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
                }
            }
        }

        public CADPurlin()
        {

        }
    }

    public class CCorrente
    {
        public bool Mapeado { get; set; } = false;
        public override string ToString()
        {
            return "p0: " + p0 + " / p1: " + p1 + " comprimento: " + comprimento + " angulo: " + angulo;
        }
        public Mline mline { get; set; }
        public double comprimento { get; set; } = 0;
        public Coordenada p0 { get; set; }
        public Coordenada p1 { get; set; }
        public Coordenada centro { get; set; }

        public double maxx
        {
            get
            {
                return p0.X > p1.X ? p0.X : p1.X;
            }
        }
        public double maxy
        {
            get
            {
                return p0.Y > p1.Y ? p0.Y : p1.Y;
            }
        }
        public double miny
        {
            get
            {
                return p0.Y < p1.Y ? p0.Y : p1.Y;
            }
        }
        public double minx
        {
            get
            {
                return p0.X < p1.X ? p0.X : p1.X;
            }
        }
        public double angulo { get; set; }
        public CCorrente()
        {

        }
        public CCorrente(Mline l)
        {
            Point3d p0, p1, centro;
            double comprimento, angulo;
            Utilidades.GetCoordenadas(l, out p0, out p1, out angulo, out comprimento, out centro);
            this.angulo = angulo;
            this.centro = new Coordenada(centro);
            this.comprimento = comprimento;
            this.mline = l;
            this.p0 = new Coordenada(p0);
            this.p1 = new Coordenada(p1);
           
        }
    }
    public class CTerca:CCorrente
    {
        public CTerca()
        {

        }

        public CTerca(Mline l) : base(l)
        {
        }
    }

    public class CTirante:CCorrente
    {
        public CTirante()
        {

        }

        public CTirante(Mline l) : base(l)
        {
        }
    }
}
