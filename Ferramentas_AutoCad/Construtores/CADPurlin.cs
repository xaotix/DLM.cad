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
        public double PurlinBalancoMax { get; set; } = 1500;
        [Category("Correntes")]
        [DisplayName("Descontar")]
        public double CorrenteDescontar { get; set; } = 20;
        [Category("Purlin")]
        [DisplayName("Mapear Fr. Man.")]
        public bool MapeiaFurosManuais { get; set; } = true;
        [Category("Purlin")]
        [DisplayName("Offset Apoio")]
        public double OffsetApoio { get; set; } = 0;

        public void SetTerca(int id)
        {
            this.id_terca = id;
            this._pecaRME = null;
        }

        [Category("Purlin")]
        [DisplayName("id")]
        [ReadOnly(true)]
        public int id_terca { get; set; } = 1763;

        private Conexoes.RME _pecaRME { get; set; }
        public Conexoes.RME GetPeca()
        {
            if(_pecaRME==null)
            {
                _pecaRME = Conexoes.DBases.GetBancoRM().GetRME(this.id_terca);
            }
            return _pecaRME;
        }


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

        private List<ObjetoMultiline> _correntes { get; set; } = null;
        public List<ObjetoMultiline> GetMultLinesCorrentes( bool reset = false)
        {
            if (_correntes == null | reset)
            {
                _correntes = new List<ObjetoMultiline>();
                var lista = Multiline.GetVerticais(this.Getmultilines(), this.CorrenteCompMin);


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
                        _correntes.Add(new ObjetoMultiline(l, Tipo_Multiline.Corrente));
                    }
                }
            }
            
            return _correntes.OrderBy(x => x.minx).ToList(); 
        }

        private List<ObjetoMultiline> _tirantes { get; set; } = null;
        public List<ObjetoMultiline> LinhasTirantes( bool reset = false)
        {
           if(_tirantes==null | reset)
            {
                _tirantes = new List<ObjetoMultiline>();
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
                        _tirantes.Add(new ObjetoMultiline(l, Tipo_Multiline.Tirante));
                    }
                }
            }
            return _tirantes;
        }

        private List<ObjetoMultiline> _purlins { get; set; }
        public List<ObjetoMultiline> GetMultLinePurlins(bool update = false)
        {
            if(_purlins==null | update)
            {
                _purlins = new List<ObjetoMultiline>();
                var lista = Multiline.GetHorizontais(this.Getmultilines(), this.PurlinCompMin);


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
                        _purlins.Add(new ObjetoMultiline(l, Tipo_Multiline.Purlin));
                    }
                }

            }

            return _purlins;

        }


        private List<Mline> mlines_verticais { get; set; } = new List<Mline>();


        public void Mapear()
        {
            var sel = SelecionarObjetos();
            if (sel.Status == PromptStatus.OK)
            {
                List<BlockReference> blocos_excluir = new List<BlockReference>();

                //using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                //{


                  
                //    acTrans.Commit();
                //}

                this.mlines_verticais = Multiline.GetVerticais(this.Getmultilines(), 100);


                int c = 1;



                FLayer.Set(LayerBlocos, true, true);

                var eixos = this.GetEixos();

                if (eixos.GetEixosVerticais().Count < 2)
                {
                    Conexoes.Utilz.Alerta("Não foi encontrado nenhum eixo vertical.", "Abortado.");
                    return;
                }

                var verticais = eixos.GetVaosObraVerticais();
                if (MapearTercas && verticais.Count > 1)
                {

                    foreach (var v in verticais)
                    {
                        v.CalcularPurlins(this, ref c);
                    }

                    Menus.MenuConfigurarVaos mm = new Menus.MenuConfigurarVaos(verticais);
                    mm.ShowDialog();

                    if (!mm.confirmado)
                    {
                        return;
                    }


                    if (MapearTercas)
                    {
                        blocos_excluir.AddRange(this.Getblocos_tercas());
                    }
                    if (MapearTirantes)
                    {
                        blocos_excluir.AddRange(this.Getblocos_tirantes());
                    }

                    if (MapearCorrentes)
                    {
                        blocos_excluir.AddRange(this.Getblocos_correntes());
                    }

                    Apagar(blocos_excluir.Select(x => x as Entity).ToList());



                    for (int i = 0; i < verticais.Count; i++)
                    {
                        InserirBlocosPurlin(verticais[i]);
                        //c = InserirBlocosVao(c, verticais[i]);
                    }



                    foreach (var s in this.GetMultLinePurlins().FindAll(x => !x.Mapeado && x.comprimento >= this.PurlinCompMin))
                    {
                        //adiciona as purlins pequenas fora do vão.
                        //essa parte precisa emplementar melhor para mapear furos manuais e correntes.
                        c = AddBlocoPurlin(c, this.id_terca, Math.Round(s.comprimento), 0, 0, s.centro.GetPoint(), new List<double>(), new List<double>(), new List<double>(), new List<double>());
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
                AddMensagem("\n" + blocos_excluir.Count.ToString() + " blocos encontrados excluídos");
                AddMensagem("\n" + this.GetEixos_Linhas().Count.ToString() + " Linhas eixo encontradas");
                AddMensagem("\n" + this.GetEixos_PolyLines().Count.ToString() + " PolyLinhas eixo encontradas");
                AddMensagem("\n" + this.Getmultilines().Count.ToString() + " Multilines encontradas");
                AddMensagem("\n" + this.mlines_verticais.Count.ToString() + " Mlines Verticais");
                AddMensagem("\n" + this.LinhasFuros().Count.ToString() + " Linhas de furos manuais");
                AddBarra();
                AddMensagem("\n" + this.LinhasTirantes().Count + " Tirantes");
                AddMensagem("\n" + this.GetMultLinePurlins().Count.ToString() + " Purlins");
                AddMensagem("\n" + this.GetMultLinesCorrentes().Count.ToString() + " Correntes");
                AddBarra();

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
            foreach (var cr in this.GetMultLinesCorrentes())
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

        public List<ObjetoMultiline> GetPurlinsPassando(ObjetoMultiline corrente)
        {
            /*melhorar a parte de quando tem inclinado*/
            return this
                .GetMultLinePurlins()
                .FindAll(x => 
                x.centro.Y >= corrente.miny-x.Largura 
                && 
                x.centro.Y <= corrente.maxy+x.Largura
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

        private void InserirBlocosPurlin(VaoObra vao)
        {

           foreach(var p in vao.Purlins)
            {
                if (p.Comprimento > this.PurlinBalancoMax)
                {
                    AddBlocoPurlin(p.Numero, p.id_terca, vao.Vao, p.TRE, p.TRD, p.CentroBloco, p.Correntes, new List<double>(),p.FurosManuais, new List<double>());
                }
            }
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
        public int AddBlocoPurlin(int c, int id_purlin, double VAO, double TRE, double TRD, Point3d origembloco, List<double> cre, List<double> crd, List<double> fe, List<double> fd)
        {
            Conexoes.RME pc = Conexoes.DBases.GetBancoRM().GetRME(id_purlin);
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
            ht.Add("ID_PECA", id_purlin);
            ht.Add("TIPO", Utilidades.Gettipo(pc));
            ht.Add("SECAO", Utilidades.Getsecao(pc));
            ht.Add("ESP", Utilidades.Getespessura(pc));

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
        public List<Entity> LinhasFuros()
        {
            return this.selecoes.FindAll(x => x.Layer.ToUpper().Replace(" ", "") == this.MapeiaFurosManuaisLayer.ToUpper().Replace(" ", "") && (x is Line | x is Polyline));
        }

        public void SetPerfil()
        {
            var perfil = Utilidades.SelecionarPurlin();
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
            var perfil = Utilidades.SelecionarPurlin();
            if (perfil == null)
            {
                return;
            }

            this.SetTerca(perfil.id_db);
   

            double comprimento = Math.Round(Math.Abs(pt1.X - pt2.X));
            Coordenada p = new Coordenada(pt1);
           

            if(comprimento> this.PurlinCompMin)
            {
                AddBlocoPurlin(0,this.id_terca, comprimento, 0, 0, p.GetCentro(pt2).GetPoint(), new List<double>(), new List<double>(), new List<double>(), new List<double>());
            }
            else
            {
                Conexoes.Utilz.Alerta("Comprimento [" + comprimento + "] inferior ao mínimo possível [" + this.PurlinCompMin + "]");
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

        public void GerarCroquis()
        {
           if(SelecionarObjetos(Tipo_Selecao.Blocos).Status!= PromptStatus.OK)
            {
                return;
            }
            bool cancelado = false;
            double dist = 2500;

            var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
            var grupos = this.Getblocos_tercas().GroupBy(x => Math.Round(x.Position.X)).OrderBy(x=>x.Key).ToList();
            bool baixo = false;
            if(cancelado)
            {
                return;
            }

            foreach (var pc in grupos)
            {
                var pecas = pc.ToList().Select(x => GetPurlin(x)).ToList();
                var purlins = pecas.GroupBy(x => x.GetChave()).ToList();
                if (purlins.Count > 0)
                {
                    double offset = baixo ? dist : 0;
                    foreach (var p in purlins)
                    {
                        var purlin = p.ToList()[0];
                        Point3d p0 = new Point3d(purlin.Origem.X - purlin.Vao / 2, origem.Y - offset, 0);
                        GerarCroqui(p.ToList()[0], p0);
                        offset = offset + dist;
                    }
                }
                baixo = !baixo;
            }
        }


        public void GerarCroqui(Conexoes.Macros.Purlin purlin, Point3d pt)
        {
            double fonte = 30;
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
            nome.TextString = $"{purlin.IndicacaoMontagem} / {purlin.Nome}";
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
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        private List<Conexoes.Macros.Purlin> JuntarERenomearPurlinsIguais(IEnumerable<Conexoes.Macros.Purlin> purlins, OpenCloseTransaction acTrans)
        {
            int c = 1;
            List<Conexoes.Macros.Purlin> ss = new List<Conexoes.Macros.Purlin>();
            purlins = purlins.OrderBy(x => x.ToString()).ToList();
            foreach(var p in purlins)
            {
                p.IndicacaoMontagem = "";
            }
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

            p.IndicacaoMontagem = N;

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
}
