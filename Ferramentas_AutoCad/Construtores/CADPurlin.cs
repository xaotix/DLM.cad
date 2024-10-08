﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DLM.cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static DLM.cad.CAD;
using Autodesk.AutoCAD.EditorInput;
using DLM.vars;
using Conexoes;
using DLM.desenho;
using DLM.vars.cad;

namespace DLM.cad
{
    [Serializable]
    public class CADPurlin : CADBase
    {
        #region Opções para configuração
        [Category("Correntes")]
        [DisplayName("Tolerância Passe")]
        public double ToleranciaPasse { get; set; } = 2;
        [Category("Eixos")]
        [DisplayName("Tolerância Mapeamento")]
        public double Eixos_Tolerancia { get; set; } = 30;

        [Category("Canvas")]
        [DisplayName("Largura")]
        public double Canvas_Largura { get; set; } = 1500;

        [Category("Canvas")]
        [DisplayName("Altura")]
        public double Canvas_Altura { get; set; } = 750;

        [Category("Canvas")]
        [DisplayName("Texto")]
        public double Canvas_Tam_Texto { get; set; } = 20;
        [Category("Canvas")]
        [DisplayName("Espessura Multiline Mapeável")]
        public double Canvas_Espessura_Multiline { get; set; } = 2;
        [Category("Canvas")]
        [DisplayName("Esp. Linha")]
        public double Canvas_Esp_Linha { get; set; } = 1;
        [Category("Canvas")]
        [DisplayName("Txt. Cotas")]
        public double Canvas_Txt_Cotas { get; set; } = 1.25;

        [Category("Canvas")]
        [DisplayName("Offset")]
        public double Canvas_Offset { get; set; } = 1500;


        [Category("Purlin")]
        [DisplayName("Rebater Furos")]
        public bool RebaterFuros { get; set; } = true;
        [Category("Purlin")]
        public bool SBR { get; set; } = false;
        [Category("Purlin")]
        [DisplayName("Transpasse Padrão")]
        public double TranspassePadrao { get; set; } = 337;
        [Category("Tirantes")]
        [DisplayName("Tolerância X para mapeamento")]
        public double TirantesToleranciaXMapeamento { get; set; } = 100;
        [Category("Purlin")]
        [DisplayName("Tolerância X para purlins menores")]
        public double PurlinToleranciaXMapeamento { get; set; } = 1500;
        [Category("Purlin")]
        [DisplayName("Vão Mínimo")]
        public double VaoMinimo { get; set; } = 1000;

        [Category("Purlin")]
        [DisplayName("Vão Máximo")]
        public double VaoMaximo { get; set; } = 21000;

        [Category("Corrente")]
        [DisplayName("Fixadores")]
        public List<string> CorrenteFixadores { get; set; } = new List<string> { "F46", "F76", "F156" };


        [Category("MultiLines")]
        [DisplayName("Comp. Mín. Verticais")]
        public double MultiLinesVerticaisCompMin { get; set; } = 100;
        [Category("Purlin")]
        [DisplayName("Comp. Máximo")]
        public int PurlinCompMaximo { get; set; } = 19000;
        [Category("Tirantes")]
        [DisplayName("Tolerância")]
        public int TirantesTolerancia { get; set; } = 50;

        [Category("Tirantes")]
        [DisplayName("Offset")]
        public int TirantesOffSet { get; set; } = -72;

        [Category("Tirantes")]
        [DisplayName("Suporte")]
        public string TirantesSuporte { get; set; } = "SFT-01";

        [Category("Tirantes")]
        [DisplayName("Comprimento Máximo")]
        public int TiranteMaxComp { get; set; } = 6000;
        [Category("Correntes")]
        [DisplayName("Comp. Mínimo")]
        public int CorrenteCompMin { get; set; } = 500;
        [Category("Purlin")]
        [DisplayName("Comp. Mín.")]
        public int PurlinCompMin { get; set; } = 150;
        [Category("Purlin")]
        [DisplayName("Balanço Máx.")]
        public int PurlinBalancoMax { get; set; } = 1500;
        [Category("Correntes")]
        [DisplayName("Descontar")]
        public int CorrenteDescontar { get; set; } = 20;
        [Category("Purlin")]
        [DisplayName("Mapear Fr. Man.")]
        public bool MapeiaFurosManuais { get; set; } = true;


        [Category("Purlin")]
        [DisplayName("Offset Apoio")]
        public int OffsetApoio { get; set; } = 0;

        [Browsable(false)]
        public int id_flange_brace { get; set; } = 953;

        [Browsable(false)]
        public int id_purlin { get; set; } = 1763;

        [Browsable(false)]
        public int id_purlin_suporte { get; set; } = 881;
        [Browsable(false)]
        public int id_corrente { get; set; } = 2532;
        [Browsable(false)]
        public int id_corrente_suporte { get; set; } = 1386;
        [Browsable(false)]
        public int id_tirante { get; set; } = 1407;
        [Browsable(false)]
        public int id_tirante_suporte { get; set; } = 1722;


        [Category("Purlin")]
        [DisplayName("Ficha de Pintura")]
        [ReadOnly(true)]
        [Browsable(false)]
        public string FichaDePintura { get; set; } = Cfg.Init.RM_SEM_PINTURA;
        [Category("Purlin")]
        [DisplayName("Layer Fr. Manuais")]
        public string MapeiaFurosManuaisLayer { get; set; } = "FUROS_MANUAIS";

        [Category("Correntes")]
        [DisplayName("Fixador")]
        public string CorrenteSuporte { get; set; } = "F156";
        [Category("Purlin")]
        [DisplayName("Fixador")]
        public List<string> PurlinSuportes { get; set; } = new List<string> { "PG1", "PC3","PC5","PC9", "PG3"};






        [Category("Peças Montagem")]
        [DisplayName("Mapear")]
        public bool Mapear_Pecas_Montagem { get; set; } = true;

        private RMLite _fb_padrao { get; set; }
        private RMLite _purlin_padrao { get; set; }
        private RMLite _purlin_padrao_suporte { get; set; }
        private RMLite _corrente_padrao { get; set; }
        private RMLite _corrente_padrao_suporte { get; set; }
        private RMLite _tirante_padrao { get; set; }
        private RMLite _tirante_padrao_suporte { get; set; }
        #endregion

        public RMLite GetPurlinPadrao()
        {
            if (_purlin_padrao == null)
            {
                _purlin_padrao = Ut.GetPURLINS().Get(this.id_purlin);
            }
            return _purlin_padrao;
        }

        public RMLite GetPurlinSuportePadrao()
        {
            if (_purlin_padrao_suporte == null)
            {
                _purlin_padrao_suporte = Ut.GetPURLINS().Get(this.id_purlin_suporte);
            }
            return _purlin_padrao_suporte;
        }
        public RMLite GetFlangeBracePadrao()
        {
            if (_fb_padrao == null)
            {
                _fb_padrao = Ut.GetFBs().Get(this.id_flange_brace);
            }
            return _fb_padrao;
        }
        public RMLite GetCorrentePadrao()
        {
            if (_corrente_padrao == null)
            {
                _corrente_padrao = Ut.GetCORRENTES().Get(this.id_corrente);
            }
            return _corrente_padrao;
        }

        public RMLite GetCorrentePadraoSuporte()
        {
            if (_corrente_padrao_suporte == null)
            {
                _corrente_padrao_suporte = Ut.GetSUPORTES_CORRENTES().Get(this.id_corrente_suporte);
            }
            return _corrente_padrao_suporte;
        }
        public RMLite GetTirantePadrao()
        {
            if (_tirante_padrao == null)
            {
                _tirante_padrao = Ut.GetTIRANTES().Get(this.id_tirante);
            }
            return _tirante_padrao;
        }
        public RMLite GetTirantePadraoSuporte()
        {
            if (_tirante_padrao_suporte == null)
            {
                _tirante_padrao_suporte = Ut.GetSUPORTES_TIRANTE().Get(this.id_tirante_suporte);
            }
            return _tirante_padrao_suporte;
        }
        public void SetTerca(int id)
        {
            this.id_purlin = id;
            this._purlin_padrao = null;
        }
        public void SetCorrente(int id)
        {
            this.id_corrente = id;
            this._corrente_padrao = null;
        }
        public void SetTirante(int id)
        {
            this.id_tirante = id;
            this._tirante_padrao = null;
        }
        public List<BlockReference> GetBlocosSecundariasIndicacao()
        {
            List<BlockReference> retorno = new List<BlockReference>();
            retorno.AddRange(this.Getblocos_correntes());
            retorno.AddRange(this.Getblocos_tercas());
            retorno.AddRange(this.Getblocos_tirantes());

            return retorno;
        }

        public List<CADMline> GetMLinesSecundarias()
        {
            List<CADMline> retorno = new List<CADMline>();
            retorno.AddRange(this.GetMLTirantes());
            retorno.AddRange(this.GetMLCorrentes());
            retorno.AddRange(this.GetMLPurlins());

            return retorno;
        }



        public void Purlin()
        {
            var sel = SelecionarObjetos();
            if (sel.Status == PromptStatus.OK)
            {


                try
                {
                    FLayer.Set(LayerBlocos, true, true);

                    var eixos = this.GetGrade();

                    if (eixos.GetEixosVerticais().Count < 2)
                    {
                        Conexoes.Utilz.Alerta("Não foi encontrado pelo menos 2 eixos verticais.", "Abortado.");
                        return;
                    }

                    var pp = new Menus.Purlin();
                    pp.eixos_mapeados.ItemsSource = Core.GetCADPurlin().GetGrade().GetEixosVerticais();
                    pp.vaos_mapeados.ItemsSource = Core.GetCADPurlin().GetGrade().GetVaosVerticais();


                    Ut.GetPURLINS();
                    Ut.GetTIRANTES();
                    Ut.GetCORRENTES();

                    pp.Lista.ItemsSource = Core.GetCADPurlin().GetMultiLines();



                    pp.Show();
                }
                catch (Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }


            }
        }

        public bool Mapear()
        {
            var eixos = this.GetGrade();

            if (eixos.GetEixosVerticais().Count < 2)
            {
                Conexoes.Utilz.Alerta("Não foi encontrado nenhum eixo vertical.", "Abortado.");
                return false;
            }

            var verticais = eixos.GetVaosVerticais();
            if (verticais.Count > 1)
            {

                Menus.MenuConfigurarVaos mm = new Menus.MenuConfigurarVaos(eixos);
                mm.Show();
            }
            return true;
        }

        public void InserirBlocos(GradeEixos grade)
        {
            var blocos_excluir = LimparBlocos();

            var verticais = grade.GetVaosVerticais();
            var fb = this.GetFlangeBracePadrao();
            for (int i = 0; i < verticais.Count; i++)
            {
                var vao = verticais[i];


                foreach (var p in vao.GetPurlins())
                {
                    if (p.Comprimento > this.PurlinBalancoMax)
                    {
                        AddBlocoPurlin(p.Letra, p.id_peca, p.Vao, p.TRE, p.TRD, p.CentroBloco, p.FurosCorrentes, p.FurosManuais);

                        if (p.FBD_Comp > 0 && fb != null)
                        {
                            Blocos.IndicacaoPeca(Cfg.Init.CAD_Bloco_PECA_INDICACAO_ESQ, p.FBD, p.FBD_Comp, this.id_flange_brace, p.Origem_Direita, this.DescFB, this.GetEscala());
                        }

                        if (p.FBE_Comp > 0 && fb != null)
                        {
                            Blocos.IndicacaoPeca(Cfg.Init.CAD_Bloco_PECA_INDICACAO_DIR, p.FBE, p.FBE_Comp, this.id_flange_brace, p.Origem_Esquerda, this.DescFB, this.GetEscala());
                        }
                    }
                }

                foreach (var p in vao.GetCorrentes())
                {
                    AddBlocoCorrente(p.Letra, p.CentroBloco, p.EntrePurlin, p.Descontar, p.GetPeca().COD_DB, p.Suporte);
                }

                foreach (var p in vao.GetTirantes())
                {
                    AddBlocoTirante(p.Letra, p.CentroBloco, Math.Round(p.Multiline.Comprimento), p.Offset, p.Offset, p.GetPeca().COD_DB, p.Suporte, p.Suporte);
                }
            }

            foreach (var p in this.GetGrade().GetPurlinsSemVao())
            {
                AddBlocoPurlin(p.Letra, p.id_peca, p.Vao, p.TRE, p.TRD, p.CentroBloco, p.FurosCorrentes, p.FurosManuais);
            }

            AddBarra();
            AddMensagem("\n" + blocos_excluir.Count.ToString() + " blocos encontrados excluídos");
            AddMensagem("\n" + this.GetMls().Count.ToString() + " Multilines encontradas");
            AddMensagem("\n" + this.LinhasFuros().Count.ToString() + " Linhas de furos manuais");
            AddBarra();
            AddMensagem("\n" + this.GetMLTirantes().Count + " Tirantes");
            AddMensagem("\n" + this.GetMLPurlins().Count.ToString() + " Purlins");
            AddMensagem("\n" + this.GetMLCorrentes().Count.ToString() + " Correntes");
            AddBarra();
        }

        public List<BlockReference> LimparBlocos()
        {
            var blocos_excluir = new List<BlockReference>();
            blocos_excluir.AddRange(this.Getblocos_tercas());
            blocos_excluir.AddRange(this.Getblocos_tirantes());
            blocos_excluir.AddRange(this.Getblocos_correntes());


            acDoc.Apagar(blocos_excluir.Select(x => x as Entity).ToList());
            return blocos_excluir;
        }







        public List<Entity> GetObjetosNaoMapeados(bool ignorar_multilines = true)
        {
            var blocos = new List<Entity>();
            blocos.AddRange(this.Selecoes);
            blocos = blocos.FindAll(x=> Selecoes.Filter<BlockReference>().FindAll(w => w.Name.ToUpper().StartsWith(Cfg.Init.CAD_PC_Quantificar)).Find(y=>y.ObjectId == x.ObjectId)==null);
            blocos = blocos.FindAll(x => this.GetAtributosEixos().Find(y => y.Block.ObjectId == x.ObjectId) == null);
            blocos = blocos.FindAll(x => this.GetLinhas_Eixos().Find(y => y.ObjectId == x.ObjectId) == null);
            blocos = blocos.FindAll(x => this.GetBlocosSecundariasIndicacao().Find(y => y.ObjectId == x.ObjectId) == null);
            blocos = blocos.FindAll(x => this.GetAtributosNivel().Select(y => y.Block).ToList().Find(y => y.ObjectId == x.ObjectId) == null);
            if(!ignorar_multilines)
            {
                blocos = blocos.FindAll(x => this.GetMLPurlins().Select(y => y.Mline).ToList().Find(y => y.ObjectId == x.ObjectId) == null);
                blocos = blocos.FindAll(x => this.GetMLCorrentes().Select(y => y.Mline).ToList().Find(y => y.ObjectId == x.ObjectId) == null);
                blocos = blocos.FindAll(x => this.GetMLTirantes().Select(y => y.Mline).ToList().Find(y => y.ObjectId == x.ObjectId) == null);
            }


            return blocos;
        }

        public List<CADMline> GetMLPurlins()
        {
            return this.GetMultiLines().FindAll(x => x.Tipo == Tipo_Multiline.Purlin).SelectMany(x => x.Mlines).ToList();
        }
        public List<CADMline> GetMLCorrentes()
        {
            return this.GetMultiLines().FindAll(x => x.Tipo == Tipo_Multiline.Corrente).SelectMany(x => x.Mlines).ToList();
        }
        public List<CADMline> GetMLTirantes()
        {
            return this.GetMultiLines().FindAll(x => x.Tipo == Tipo_Multiline.Tirante).SelectMany(x => x.Mlines).ToList();
        }

        public List<BlockReference> Getblocos_tercas()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name.ToUpper() == "TERCA_INDICACAO");
        }
        public List<BlockReference> Getblocos_tirantes()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name.ToUpper() == "TIRANTE_INDICACAO");
        }
        public List<BlockReference> Getblocos_correntes()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name.ToUpper() == "CORRENTE_INDICACAO");
        }



       

        

        private GradeEixos _grade { get; set; }
        public GradeEixos GetGrade(bool update = false)
        {

            if(_grade==null | update)
            {
                _grade = new GradeEixos();


                var blocos = GetAtributosEixos(true);


                var horiz = GetLinhas_Horizontais().FindAll(x=> x.Comprimento >= this.LayerEixosCompMin);
                var vertz = GetLinhas_Verticais().FindAll(x => x.Comprimento >= this.LayerEixosCompMin);
                /*considera apenas linhas que estão em layers de eixo e que sejam Dashdot*/
                var HORIS1 = horiz.FindAll(x => x.Layer.ToUpper().Contains(this.LayerEixos));
                var VERTS1 = vertz.FindAll(x => x.Layer.ToUpper().Contains(this.LayerEixos));

                var HORIS = HORIS1.GroupBy(x=>x.Min.Y).Select(x=>x.First()).ToList().OrderBy(x=>x.Min.Y).ToList();
                var VERTS = VERTS1.GroupBy(x => x.Min.X).Select(x => x.First()).ToList().OrderBy(x => x.Min.X).ToList();

                /*organiza por comprimento e pega as linhas maiores*/


                if (HORIS.Count > 1)
                {
                    for (int i = 0; i < HORIS.Count; i++)
                    {
                        var linha = HORIS[i];
                        double dist = 0;
                        if (_grade.GetEixosHorizontais().Count > 0)
                        {
                            dist = Math.Round(Math.Abs(HORIS[(int)i].P1.Y - _grade.GetEixosHorizontais().Last().Linha.P1.Y));
                        }

                        if(dist>=DistanciaMinimaEixos | _grade.GetEixosHorizontais().Count ==0)
                        {
                            var blks = Blocos.GetAtributosProximos(blocos, linha.Min, linha.Max, this.Eixos_Tolerancia);


                            if (blks.Count >= 1)
                            {
                                _grade.AddEixo(Sentido.Horizontal, dist, blks[0], linha);
                            }
                            else
                            {
                                _grade.AddEixo(Sentido.Horizontal, dist, null, linha);
                            }
                        }
                    }
                }


                if (VERTS.Count > 1)
                {
                    for (int i = 0; i < VERTS.Count; i++)
                    {
                        var linha = VERTS[i];
                        double dist = 0;
                        if (_grade.GetEixosVerticais().Count > 0)
                        {
                            dist = Math.Round(Math.Abs(VERTS[i].P1.X - _grade.GetEixosVerticais().Last().Linha.P1.X));
                        }

                        if(dist >= DistanciaMinimaEixos | _grade.GetEixosVerticais().Count == 0)
                        {
                            var blks = Blocos.GetAtributosProximos(blocos, linha.Min, linha.Max, this.Eixos_Tolerancia);

                            if (blks.Count >= 1)
                            {
                                _grade.AddEixo(Sentido.Vertical, dist, blks[0], linha);
                            }
                            else
                            {
                                _grade.AddEixo(Sentido.Vertical, dist,null, linha);
                            }
                        }
                    }
                }
            }


            return _grade;
        }
        public void GetBoneco_Purlin()
        {
            List<double> retorno = new List<double>();

            using (var acTrans = acCurDb.acTrans())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {
                    foreach(var s in GetCADLines())
                    {
                        AddMensagem($"\nLinha: {s.P1.ToString()} Comprimento: {s.Comprimento} Angulo: {s.Angulo}");
                    }
                    var _horizon = GetLinhas_Horizontais();

                    var _verts = GetLinhas_Verticais();

                    

                    AddMensagem($"\nLinhas verticais: {_verts.Count}");
                    AddMensagem($"\nLinhas horizontais: {_horizon.Count}");

                    var ret = new List<string>();
                    var textos = new List<string>();

                    int c = 1;
                    foreach (var lhs in _horizon)
                    {



                        var verts = _verts.FindAll(x =>x.Min.X>lhs.Min.X+1 && x.Max.X<lhs.Max.X-1);
                        var passa = new List<CADLine>();

                        foreach (var v in verts)
                        {
                           
                            var max_y = v.P1.Y > v.P2.Y ? v.P1.Y : v.P2.Y;
                            var min_y = v.P1.Y < v.P2.Y ? v.P1.Y : v.P2.Y;
                            if (max_y >= lhs.P1.Y && min_y <= lhs.P1.Y)
                            {
                                passa.Add(v);
                            }
                        }
                        AddMensagem($"Linhas verticais que passam: {passa.Count}");

                        double comprimento = Math.Round(Math.Abs(lhs.P1.X - lhs.P2.X));
                        var furos = passa.Select(x => x.P1.X - lhs.Min.X).ToList().OrderBy(x=>x).ToList();
                        furos = furos.Distinct().ToList();
                        textos.Add($"@Linha {c}");
                        textos.Add($"Comprimento: {comprimento}");
                        textos.Add($"Origem:{lhs.P1.ToString()}");

                        textos.Add($"Coordenadas:{furos.Count}");
                        textos.Add("$Inicio");
                        textos.AddRange(furos.Select(x => x.String(0)));
                        textos.Add("$Fim");


                        Ut.AddLeader(-45, lhs.P1,this.GetEscala(), $"Linha:{c}", 15 * .8);
                        c++;
                    }
                 
                    var arquivo = $"{acDoc.Name.getPasta()}{acDoc.Name.getNome()}_boneco.txt";
                   if(textos.Count>0)
                    {
                        Conexoes.Utilz.Arquivo.Gravar(arquivo, textos);
                        arquivo.Abrir();
                    }
                   else
                    {
                        AddMensagem("\nNada encontrado.");
                    }

                    c++;

                }

            }
        }

        public db.Linha GetHashtable(Conexoes.Macros.Purlin p)
        {
            var ht = new db.Linha();

            ht.Add(Cfg.Init.CAD_ATT_N, p.Sequencia);
            ht.Add("CRD", string.Join(";", p.Correntes_Direita));
            ht.Add("CRE", string.Join(";",p.Correntes_Esquerda));
            ht.Add("AD", this.OffsetApoio);
            ht.Add("AE", this.OffsetApoio);
            ht.Add("FBD", "");
            ht.Add("FBE", "");
            ht.Add("REB", p.Rebater_Furos ? "Sim" : "Não");
            ht.Add("SBR", p.Corrente_SBR ? "Sim" : "Não");
            ht.Add("FD", string.Join(";", p.Direita.Furos_Manuais));
            ht.Add("FE", string.Join(";", p.Esquerda.Furos_Manuais));
            ht.Add(Cfg.Init.CAD_ATT_Vao, p.Vao);
            ht.Add("NOME", "");
            ht.Add(Cfg.Init.CAD_ATT_Transp_Dir, p.Direita.Comprimento);
            ht.Add(Cfg.Init.CAD_ATT_Transp_Esq, p.Esquerda.Comprimento);
            ht.Add("ID_DB", "");
            ht.Add("PINTURA", p.Pintura);
            ht.Add("ID_PECA", p.id_peca);
            ht.Add(Cfg.Init.CAD_ATT_Tipo, p.Perfil.Contains("C")?"C":"Z");
            ht.Add("SECAO", p.Secao);
            ht.Add(Cfg.Init.CAD_ATT_Espessura, p.Espessura);

            return ht;
        }


        public void AddBlocoPurlin(string letra, int id_purlin, double VAO, double TRE, double TRD, P3d origembloco, List<double> Correntes_Esq, List<double> Furos_Manuais_Esq)
        {
            RMLite pc = Ut.GetPURLINS().Get(id_purlin);
            //AddMensagem("Origem: " + centro + "\n");
            var ht = new db.Linha();    

            ht.Add(Cfg.Init.CAD_ATT_N, letra);
            ht.Add("CRD", "");
            ht.Add("CRE", string.Join(";", Correntes_Esq));
            ht.Add("AD", this.OffsetApoio);
            ht.Add("AE", this.OffsetApoio);
            ht.Add("FBD", "");
            ht.Add("FBE", "");
            ht.Add("REB", this.RebaterFuros ? "Sim" : "Não");
            ht.Add("SBR", this.SBR ? "Sim" : "Não");
            ht.Add("FD", "");
            ht.Add("FE", string.Join(";", Furos_Manuais_Esq));
            ht.Add(Cfg.Init.CAD_ATT_Vao, VAO);
            ht.Add("NOME", "");
            ht.Add(Cfg.Init.CAD_ATT_Transp_Dir, TRD);
            ht.Add(Cfg.Init.CAD_ATT_Transp_Esq, TRE);
            ht.Add("ID_DB", id_purlin);
            ht.Add("PINTURA", this.FichaDePintura);
            ht.Add("ID_PECA", id_purlin);
            ht.Add(Cfg.Init.CAD_ATT_Tipo, pc.GRUPO);
            ht.Add("SECAO", pc.SECAO);
            ht.Add(Cfg.Init.CAD_ATT_Espessura, pc.ESP);

            //quando a purlin está deslocada.
            double comp_sem_transpasse = VAO + (TRE < 0 ? TRE : 0) + (TRD < 0 ? TRD : 0);
            //verifica se a purlin é maior que 150
            if (comp_sem_transpasse > 150)
            {
                Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_Incicacao_Tercas, origembloco, this.GetEscala(), 0, ht);
            }
        }
        public void AddBlocoTirante(string letra,  P3d origembloco, double Comp, double offset1 = -72, double offset2 = -72,string TIP = "03TR", string sfta = "STF-01", string sftb = "STF-01")
        {
            //AddMensagem("Origem: " + centro + "\n");
            var ht = new db.Linha();
            ht.Add(Cfg.Init.CAD_ATT_N, letra);

            ht.Add(Cfg.Init.CAD_ATT_Comprimento, Comp.String(0));
            ht.Add("OFFSET1", offset1.String(0));
            ht.Add("OFFSET2", offset2.String(0));

            ht.Add("TIP", TIP);
            ht.Add("SFTA", sfta);
            ht.Add("SFTB", sftb);

            Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_Indicacao_Tirantes, origembloco, this.GetEscala(), 0, ht);
        }
        public void AddBlocoCorrente(string letra, P3d origembloco, double Comp, double desc = 18, string tip = "DLDA", string fix = "F156")
        {
            var ht = new db.Linha();
            ht.Add(Cfg.Init.CAD_ATT_N, letra);
            ht.Add("TIP", tip);
            ht.Add(Cfg.Init.CAD_ATT_Descricao, desc.String(3));
            ht.Add(Cfg.Init.CAD_ATT_Comprimento, Comp.String(0));
            ht.Add(Cfg.Init.CAD_ATT_Corrente_Fixador, fix);

            Blocos.Inserir(acDoc, Cfg.Init.CAD_BLK_Indicacao_Correntes, origembloco, this.GetEscala(), 0, ht);
        }
        public List<Entity> LinhasFuros()
        {
            return this.Selecoes.FindAll(x => x.Layer.ToUpper().Replace(" ", "") == this.MapeiaFurosManuaisLayer.ToUpper().Replace(" ", "") && (x is Line | x is Polyline));
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

                acDoc.Apagar(blocos.Select(x => x as Entity).ToList());
            }
        }
        public void EdicaoCompleta()
        {


            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    var selecao = this.Getblocos_tercas().Select(x => GetPurlin(x)).ToList();
                    if(selecao.Count>0)
                    {
                        var purlins = Conexoes.Utilz.Editar(selecao);
                        if(Conexoes.Utilz.Pergunta("Salvar edições?"))
                        {
                            foreach(var purlin in purlins)
                            {
                                var ll = purlin.Objeto as BlockReference;
                                Atributos.Set(ll, acTrans, GetHashtable(purlin));
                            }
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void PurlinManual()
        {
            bool cancelado = false;
            var pt1 = Ut.PedirPonto("Selecione o ponto de origem", out cancelado);
            if(cancelado)
            {
                return;
            }
            var pt2 = Ut.PedirPonto("Selecione o ponto final",pt1, out cancelado);
            if(cancelado)
            {
                return;
            }
            var perfil = Ut.SelecionarPurlin(null);
            if (perfil == null)
            {
                return;
            }

            this.SetTerca(perfil.id_codigo);
   

            double comprimento = Math.Round(Math.Abs(pt1.X - pt2.X));

           

            if(comprimento> this.PurlinCompMin)
            {
                AddBlocoPurlin("",this.id_purlin, comprimento, 0, 0, pt1.Centro(pt2), new List<double>(), new List<double>());
            }
            else
            {
                Conexoes.Utilz.Alerta("Comprimento [" + comprimento + "] inferior ao mínimo possível [" + this.PurlinCompMin + "]");
                return;
            }

            
        }

        public void Editar(bool editar = false)
        {


            using (var acTrans = acCurDb.acTrans())
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

            var origem = Ut.PedirPonto("Selecione a origem", out cancelado);
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
                        P3d p0 = new P3d(purlin.Origem.X - purlin.Vao / 2, origem.Y - offset, 0);
                        GerarCroqui(p.ToList()[0], p0);
                        offset = offset + dist;
                    }
                }
                baixo = !baixo;
            }
        }
        public void GerarCroqui(Conexoes.Macros.Purlin purlin, P3d pt)
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
                txt.TextString = $"{s.Posicao.ToString()} - {s.X} [{s.Origem}]";
                txt.Position = new Point3d(fr.StartPoint.X, fr.StartPoint.Y - 5, 0);
                txt.Rotation = (-90.0).GrausParaRadianos();
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
                string destino = "";
                if (exportar)
                {
                    destino = Cfg.Init.EXT_RM.SalvarArquivo();
                }
                if (destino ==null && exportar)
                {
                    return;
                }

                using (var acTrans = acCurDb.acTrans())
                {
                    var sel = SelecionarObjetos();
                    if (sel.Status == PromptStatus.OK)
                    {
                        Conexoes.DBRM_Offline mm = new Conexoes.DBRM_Offline();
                        var purlins = this.Getblocos_tercas().Select(x => GetPurlin(x));
                        List<Conexoes.Macros.Purlin> ss = JuntarERenomearPurlinsIguais(purlins, acTrans);
                        P3d p = new P3d();
                        if (tabela)
                        {
                            bool cancelado = false;
                            var PS = Ut.PedirPonto("Selecione a origem", out cancelado);
                            if (!cancelado)
                            {
                                p = Tabelas.InserirTabela(ss, PS);
                            }
                        }

                        var tirantes = this.Getblocos_tirantes().Select(x => GetTirante(x));
                        var pcsTirantes = JuntarTirantesIguais(tirantes, acTrans);
                        if (tabela)
                        {
                            p = Tabelas.InserirTabela(pcsTirantes, new P3d(p.X + (119.81 * GetEscala()), p.Y));
                        }
                        mm.RM_Macros.AddRange(pcsTirantes.Select(x => new Conexoes.RME_Macro(x)));


                        var correntes = this.Getblocos_correntes().Select(x => GetCorrente(x));
                        List<DLM.macros.Corrente> pcsCorrentes = JuntarCorrentesIguais(correntes, acTrans);
                        if (tabela)
                        {
                            p = Tabelas.InserirTabela(pcsCorrentes, new P3d(p.X + (86.77 * GetEscala()), p.Y));
                        }
                        mm.RM_Macros.AddRange(pcsCorrentes.Select(x => new Conexoes.RME_Macro(x)));


                        if (Mapear_Pecas_Montagem)
                        {
                            var pcs = this.GetBlocos_IndicacaoPecas();

                            if(pcs.Count>0)
                            {
                               p = Tabelas.Pecas(pcs, true, new P3d(p.X + (86.77 * GetEscala()), p.Y));
                            }
                        }

                        mm.RM_Macros.AddRange(ss.Select(x => new Conexoes.RME_Macro(x)));
                        if (exportar)
                        {
                            mm.Salvar(destino);
                        }

                        acTrans.Commit();
                        editor.Regen();
                    }
                }
            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

        }
        private List<Conexoes.Macros.Purlin> JuntarERenomearPurlinsIguais(IEnumerable<Conexoes.Macros.Purlin> purlins, OpenCloseTransaction acTrans)
        {
            int c = 1;
            var ss = new List<Conexoes.Macros.Purlin>();
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
                nova.Sequencia = c.ToString();
                foreach (var s in pps.Select(x=>x.Objeto as BlockReference))
                {
                    Atributos.Set(s, acTrans, Cfg.Init.CAD_ATT_N, c.String(3));
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }
        private List<DLM.macros.Tirante> JuntarTirantesIguais(IEnumerable<DLM.macros.Tirante> tirantes, OpenCloseTransaction acTrans)
        {
            int c = 1;
            List<DLM.macros.Tirante> ss = new List<DLM.macros.Tirante>();
            tirantes = tirantes.OrderBy(x => x.ToString()).ToList();
            foreach (var tirante in tirantes.GroupBy(x => x.NomePadronizado + x.Comprimento.ArredondarMultiplo(this.TirantesTolerancia).String(0) + " - " + x.Fixacao_1 + "/" + x.Fixacao_2 + "/" + x.Tratamento).OrderByDescending(X => X.Count()))
            {
                var pps = tirante.ToList();
                var nova = pps[0].Clonar();
                nova.Quantidade = pps.Count();
                var comp = nova.Comprimento;
                nova.Offset_1 = 0;
                nova.Offset_2 = 0;
                nova.CompUser = comp.ArredondarMultiplo(this.TirantesTolerancia);
                nova.Sequencia = c.String(2);
               
                foreach (var s in pps.FindAll(x=> x.Bloco is BlockReference).Select(x => x.Bloco as BlockReference))
                {
                    Atributos.Set(s, acTrans, Cfg.Init.CAD_ATT_N, c.String(2));
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }
        private List<DLM.macros.Corrente> JuntarCorrentesIguais(IEnumerable<DLM.macros.Corrente> tirantes, OpenCloseTransaction acTrans)
        {
            int c = 0;
            List<DLM.macros.Corrente> ss = new List<DLM.macros.Corrente>();
            tirantes = tirantes.OrderBy(x => x.ToString()).ToList();
            foreach (var tirante in tirantes.GroupBy(x => x.ToString()).OrderByDescending(X=>X.Count()))
            {
                
                var pps = tirante.ToList();
                var nova = pps[0].Clonar();
                nova.RME_Macro = pps[0].RME_Macro;
                nova.Quantidade = pps.Count();
                var comp = nova.CompCorrente;

                nova.Sequencia = c.getLetra(); 

                foreach (var s in pps.FindAll(x=>x.Bloco is BlockReference).Select(x => x.Bloco as BlockReference))
                {
                    Atributos.Set(s, acTrans, Cfg.Init.CAD_ATT_N, c.getLetra());
                }
                ss.Add(nova);
                c++;
            }

            return ss;
        }


        public Conexoes.Macros.Purlin GetPurlin(BlockReference bloco)
        {
            var linha = bloco.GetAttributes();
            var N = linha[Cfg.Init.CAD_ATT_N].Valor;
            var ESP = linha[Cfg.Init.CAD_ATT_Espessura].Double();
            var SECAO = linha["SECAO"].Double();
            var TIPO = linha[Cfg.Init.CAD_ATT_Tipo].Valor;
            var ID_PECA = linha["ID_PECA"].Int();
            var PINTURA = linha["PINTURA"].Valor;
            var ID_DB = linha["ID_DB"].Int();
            var VAO = linha[Cfg.Init.CAD_ATT_Vao].Double();
            var TRE = linha[Cfg.Init.CAD_ATT_Transp_Esq].Double();
            var TRD = linha[Cfg.Init.CAD_ATT_Transp_Dir].Double();
            var AD = linha["AD"].Double();
            var AE = linha["AE"].Double();
            var REB = linha["REB"].Valor.ToUpper() == "SIM";
            var SBR = linha["SBR"].Valor.ToUpper() == "SIM";

            var NOME = linha["NOME"].Valor;
            var FE = linha["FE"].Valor;
            var FD = linha["FD"].Valor;
            var FBE = linha["FBE"].Valor;
            var FBD = linha["FBD"].Valor;

            var CRE = linha["CRE"].Valor;
            var CRD = linha["CRD"].Valor;

            Conexoes.Macros.Purlin p = new Conexoes.Macros.Purlin();

            p.IndicacaoMontagem = N;

            p.id_peca = ID_PECA;
            //p.SetPeca(DBases.GetBancoRM().GetTercas().Find(x => x.id_db == ID_PECA));
            p.Objeto = bloco;
            p.Tipo_Corrente = Tipo_Corrente_Purlin.Manual;
            p.Rebater_Furos = REB;
            p.Corrente_SBR = SBR;

            p.Origem = new P3d(bloco.Position.X, bloco.Position.Y, 0);

            //CORRENTES RÍGIDAS
            if (CRE.Length > 0)
            {
                foreach (var s in CRE.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Correntes_Esquerda.Add(s);
                }
            }
            if (CRD.Length > 0)
            {
                foreach (var s in CRD.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Correntes_Direita.Add(s);
                }
            }
            p.Tipo_Corrente = Tipo_Corrente_Purlin.Manual;

            //FURO MANUAL
            if (FE.Length > 0)
            {
                foreach (var s in FE.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Esquerda.Furos_Manuais.Add(s);
                }
            }
            if (FD.Length > 0)
            {
                foreach (var s in FD.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Direita.Furos_Manuais.Add(s);
                }
            }

            p.Esquerda.Tipo_Furo_FB = Tipo_Furo_FB.Manual;
            p.Direita.Tipo_Furo_FB = Tipo_Furo_FB.Manual;


            //FLANGE BRACES
            if(FBE.Length>0)
            {
                foreach (var s in FBE.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Esquerda.Flange_Braces.Add(s);
                }
            }

            if(FBD.Length>0)
            {
                foreach (var s in FD.Split(';').Select(x => x.Double()).OrderBy(x => x).ToList().Distinct().ToList())
                {
                    if (s > 0)
                        p.Direita.Flange_Braces.Add(s);
                }
            }



            if (AE>0)
            {
                p.Esquerda.Tipo_Furo_Apoio = Tipo_Furo_Purlin.Espelhado;
                p.Esquerda.Furo_Apoio_Offset = AE;
            }

            if (AD > 0)
            {
                p.Esquerda.Tipo_Furo_Apoio = Tipo_Furo_Purlin.Espelhado;
                p.Esquerda.Furo_Apoio_Offset = AD;
            }
            /*falta ajustar pra buscar pela espessura e pela secao*/
            p.Vao = VAO;
            p.Esquerda.Comprimento = TRE;
            p.Direita.Comprimento = TRD;
            return p;
        }
        public DLM.macros.Tirante GetTirante(BlockReference bloco)
        {
            var linha = bloco.GetAttributes();
            var SFTA = linha["SFTA"].Valor;
            var SFTB = linha["SFTB"].Valor;
            var TIP =  linha["TIP"].Valor;
            var OFF1 = linha["OFF1"].Double();
            var OFF2 = linha["OFF2"].Double();
            var COMP = linha[Cfg.Init.CAD_ATT_Comprimento].Double();

            var p = new DLM.macros.Tirante();
            //p.Fixacao_1 = SFTA;
            //p.Fixacao_2 = SFTB;
            p.NomePadronizado = TIP;
            p.Offset_1 = OFF1;
            p.Offset_2 = OFF2;
            p.CompUser = COMP;
            p.Bloco = bloco;
            return p;
        }
        public DLM.macros.Corrente GetCorrente(BlockReference bloco)
        {
            var atributos = bloco.GetAttributes();

            var TIP =   atributos["TIP"].Valor;
            var DESC =  atributos[Cfg.Init.CAD_ATT_Descricao].Double();
            var COMP =  atributos[Cfg.Init.CAD_ATT_Comprimento].Double();
            var FIX =   atributos["FIX"].Valor;

            var p = new DLM.macros.Corrente();
            p.Vao = COMP;
            p.Descontar = DESC;
            p.Bloco = bloco;
            p.SetFixacao(FIX);
            var diagonal = Ut.GetCORRENTES().Get(TIP);
            if(diagonal!=null)
            {
                p.SetDiagonal(diagonal);
            }
            return p;
        }

        public void SetFicha()
        {
            var valor = DBases.GetBancoRM().Get_Pinturas().Selecao<string>();
            if (valor == null)
            {
                return;
            }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        Atributos.Set(s, acTrans, "PINTURA", valor);
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void SetTranspasse()
        {
            var trs = new List<string> { "Esquerda", "Direita", "Ambos" }.ListaSelecionar();
            if (trs == null)
            {
                return;
            }

            var valor = this.TranspassePadrao.Prompt();

            if (valor == null) { return; }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        if(trs == "Esquerda" | trs == "Ambos")
                        {
                            Atributos.Set(s, acTrans, Cfg.Init.CAD_ATT_Transp_Esq, valor.Value.String(0));
                        }

                        if (trs == "Direita" | trs == "Ambos")
                        {
                            Atributos.Set(s, acTrans, Cfg.Init.CAD_ATT_Transp_Dir, valor.Value.String(0));
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void SetSuporte()
        {
            var trs = new List<string> { "Esquerda", "Direita", "Ambos" }.ListaSelecionar();
            if (trs == null)
            {
                return;
            }

            var tip = new List<string> { "Centralizado", "Offset" }.ListaSelecionar();
            if (tip == null)
            {
                return;
            }
            double? valor = 0;
            if(tip == "Offset")
            {
                valor = this.OffsetApoio.Prompt();
            }



            using (var acTrans = acCurDb.acTransST())
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
                                Atributos.Set(s, acTrans, "AD", valor.Value.String(0));
                                Atributos.Set(s, acTrans, "AE", "0");
                            }
                            else if (trs == "Esquerda")
                            {
                                Atributos.Set(s, acTrans, "AE", valor.Value.String(0));
                            }
                            else if (trs == "Direita")
                            {
                                Atributos.Set(s, acTrans, "AD", valor.Value.String(0));
                            }
                        }
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }




        public void SetPurlinSuporte(int id)
        {
            this.id_purlin_suporte = id;
            this._purlin_padrao_suporte = null;
        }
        public void SetTiranteSuporte(int id)
        {
            this.id_tirante_suporte = id;
            this._tirante_padrao = null;
        }
        public void SetCorrenteSuporte(int id)
        {
            this.id_corrente_suporte = id;
            this._corrente_padrao_suporte = null;
        }
        public void SetPurlin(int id)
        {
            this.id_purlin = id;
            this._purlin_padrao = null;


        }

        public void SetPurlin()
        {
            var perfil = Ut.SelecionarPurlin(this.GetPurlinPadrao());
            if (perfil == null)
            {
                return;
            }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_tercas())
                    {
                        var ht = new db.Linha();
                        ht.Add("ID_PECA", perfil.id_codigo);
                        ht.Add(Cfg.Init.CAD_ATT_Espessura, perfil.ESP.String(2));
                        ht.Add("SECAO", perfil.SECAO.String(0));
                        ht.Add(Cfg.Init.CAD_ATT_Tipo, perfil.GRUPO.Contains("C") ? "C" : "Z");

                        Atributos.Set(s, acTrans, ht);
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void SetCorrente()
        {
            var perfil = Ut.SelecionarCorrente();
            if (perfil == null)
            {
                return;
            }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_correntes())
                    {
                        Atributos.Set(s, acTrans, "TIP", perfil.COD_DB.ToString());
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void SetCorrenteDescontar()
        {
            var valor = this.CorrenteDescontar.Prompt();
            if (valor < 0)
            {
                return;
            }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {
                    foreach (var bloco in this.Getblocos_correntes())
                    {
                        Atributos.Set(bloco, acTrans, Cfg.Init.CAD_ATT_Descricao, valor.ToString());
                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }
        public void SetCorrenteSuporte()
        {
            var valor = CorrenteFixadores.ListaSelecionar();
            if (valor == null)
            {
                return;
            }

            using (var acTrans = acCurDb.acTransST())
            {
                var sel = SelecionarObjetos();
                if (sel.Status == PromptStatus.OK)
                {

                    foreach (var s in this.Getblocos_correntes())
                    {
                        Atributos.Set(s, acTrans, "FIX", valor.ToString());

                    }
                    acTrans.Commit();
                    editor.Regen();
                }
            }
        }

        public CADPurlin()
        {
            Multiline.GetMLStyles(true);
        }
    }
}
