using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DLM.cad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class CADCotagem : CADBase
    {
        #region CAD
        public List<CADLine> Getlinhas_perfil()
        {
            return GetCADLines().FindAll(x => x.Layer == LayerLinhas && x.Comprimento >= D_Min_X);
        }
        public List<CADLine> Getlinhas_projecao()
        {
            return GetCADLines().FindAll(x => x.Layer == LayerProjecao && x.Comprimento >= D_Min_X && x.Comprimento >= Tam_Minimo_Projecao);
        }
        public List<BlockReference> GetBlocos_Marcas_Posicoes()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x =>
                 Cfg.Init.GetBlocosTecnoMetalMarcas().Find(y => y.ToUpper() == x.Name.ToUpper()) != null |
                 Cfg.Init.GetBlocosTecnoMetalPosicoes().Find(y => y.ToUpper() == x.Name.ToUpper()) != null
                                    );
        }
        public List<BlockReference> GetFuros_corte()
        {
            return Selecoes.Filter<BlockReference>().FindAll(x => x.Name.ToUpper() == "MS");
        }
        #endregion
        public void GetVars(CADCotagem c)
        {
            this.Acumuladas_Baixo = c.Acumuladas_Baixo;
            this.Acumuladas_Cima = c.Acumuladas_Cima;
            this.Base_Direita = c.Base_Direita;
            this.Base_Esquerda = c.Base_Esquerda;
            this.Concavidade_Contorno = c.Concavidade_Contorno;
            this.Cotar_Direita = c.Cotar_Direita;
            this.Cotar_Embaixo = c.Cotar_Embaixo;
            this.Cotar_Em_Cima = c.Cotar_Em_Cima;
            this.Cotar_Esquerda = c.Cotar_Esquerda;
            this.Cotas_Movidas_Contagem = c.Cotas_Movidas_Contagem;
            this.Cota_Anterior_Movida = c.Cota_Anterior_Movida;
            this.Cota_Horizontal_Superior = c.Cota_Horizontal_Superior;
            this.Dist0 = c.Dist0;
            this.Dist1 = c.Dist1;
            this.Dist2 = c.Dist2;
            this.D_Min_X = c.D_Min_X;
            this.D_Min_Y = c.D_Min_Y;
            //this.escala = c.escala;
            this.Escala_Contorno = c.Escala_Contorno;
            this.Espessura_Contorno = c.Espessura_Contorno;
            this.Estilo_Padrao = c.Estilo_Padrao;
            this.Estilo_Padrao_Fonte = c.Estilo_Padrao_Fonte;
            this.ForcarTamTexto = c.ForcarTamTexto;
            this.Furos_Corte_Baixo = c.Furos_Corte_Baixo;
            this.Furos_Corte_Cima = c.Furos_Corte_Cima;
            this.Furos_Vista_Baixo = c.Furos_Vista_Baixo;
            this.Furos_Vista_Cima = c.Furos_Vista_Cima;
            this.Furos_Vista_Corte_Cotar_direita = c.Furos_Vista_Corte_Cotar_direita;
            this.Furos_Vista_Corte_Cotar_Esquerda = c.Furos_Vista_Corte_Cotar_Esquerda;
            this.Juntar_Cotas = c.Juntar_Cotas;
            this.LayerLinhas = c.LayerLinhas;
            this.LayerProjecao = c.LayerProjecao;
            this.Marcas_Ajusta_Escala = c.Marcas_Ajusta_Escala;
            this.Offset_Centro_Valor = c.Offset_Centro_Valor;
            this.Profundidade_Direita = c.Profundidade_Direita;
            this.Profundidade_Esquerda = c.Profundidade_Esquerda;
            this.Profundidade_Furos_Porcentagem = c.Profundidade_Furos_Porcentagem;
            this.Size = c.Size;
            this.Tam_Minimo_Projecao = c.Tam_Minimo_Projecao;
            this.Tipo_Calculo_Contorno = c.Tipo_Calculo_Contorno;
            this.Tipo_Desenho = c.Tipo_Desenho;
            this.Widthfactor = c.Widthfactor;

        }

        #region Pontos
        [XmlIgnore]
        private P3d Superior_Esquerdo { get; set; } = new P3d();
        [XmlIgnore]
        private P3d Inferior_Esquerdo { get; set; } = new P3d();
        [XmlIgnore]
        private P3d Superior_Direito { get; set; } = new P3d();
        [XmlIgnore]
        private P3d Inferior_Direito { get; set; } = new P3d();
        [XmlIgnore]
        private P3d Centro { get; set; } = new P3d();
        private double Altura
        {
            get
            {
                return (Superior_Esquerdo.Y > Superior_Direito.Y ? Superior_Esquerdo.Y : Superior_Direito.Y) - (Inferior_Esquerdo.Y < Inferior_Direito.Y ? Inferior_Esquerdo.Y : Inferior_Direito.Y);
            }
        }
        private double Largura
        {
            get
            {
                return (Superior_Direito.X > Inferior_Direito.X ? Superior_Direito.X : Inferior_Direito.X) - (Superior_Esquerdo.X < Inferior_Esquerdo.X ? Superior_Esquerdo.X : Inferior_Esquerdo.X);
            }
        }
        private double MinY
        {
            get
            {
                return Inferior_Direito.Y < Inferior_Esquerdo.Y ? Inferior_Direito.Y : Inferior_Esquerdo.Y;
            }
        }
        private double MaxY
        {
            get
            {
                return Superior_Direito.Y > Superior_Esquerdo.Y ? Superior_Direito.Y : Superior_Esquerdo.Y;
            }
        }

        private double MinX
        {
            get
            {
                return Superior_Esquerdo.X < Inferior_Esquerdo.X ? Superior_Esquerdo.X : Inferior_Esquerdo.X;
            }
        }
        private double MaxX
        {
            get
            {
                return Superior_Direito.X > Inferior_Direito.X ? Superior_Direito.X : Inferior_Direito.X;
            }
        }
        private void GetPontosHorizontais(out List<P3d> retorno, out double y, out P3d origem, bool superior = true)
        {
            retorno = new List<P3d>();
          var  pp = new List<P3dCAD>();
            origem = new P3d();
            y = 0;

            var ptss = GetContorno();
            if (ptss.Count == 0)
            {
                AddMensagem("\nNenhum contorno encontrado acima do centro");

                return;
            }
            //ptss = RemovePtsDistMin_X(ptss, distancia_minima_X);
            y = 0;
            origem = new P3d();


            if (superior)
            {

                var superiores = ptss.FindAll(x => x.Y >= Centro.Y + Offset_Centro).ToList();

                pp.AddRange(superiores);

                //encontra todos com Y maior que o meio
                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhum contorno encontrado acima do centro");
                    return;
                }

                y = pp.Max(x => x.Y);


                pp = pp.Select(x => x.Round(1)) //arredonda os valores
                .OrderBy(x => x.X).GroupBy(x => x.X) //agrupa por X
                .Select(x => x.ToList().OrderBy(z => z.Y)) //organiza por coordenada Y
                .Select(x => x.Last()).ToList();


                if (this.Furos_Vista_Cima)
                {
                    //pega os furos da vista, organiza os mesmos pela menor coordenada em Y;
                    var ps = Getpts_furos_vista();

                    var furos_vista = ps
                         .GroupBy(x => x.X).ToList().Select(x => x.ToList().OrderBy(z => z.Y).Last()).ToList();
                    pp.AddRange(furos_vista.Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
                }

                if (this.Furos_Corte_Cima)
                {
                    //adiciona os furos de corte;
                    var furos_corte = Getpts_furos_corte_verticais().FindAll(x => x.Y >= Centro.Y + Offset_Centro);
                    pp.AddRange(furos_corte.Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
                }




                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhuma coordenada encontrada abaixo do centro");

                    return;
                }


                //agrupa as coordenadas e pega a que tem maior y. Isso faz com que quando tem mais de um ponto com a mesma coordenada X, 
                //ele pegue o maior valor em Y
                pp = pp.GroupBy(z => z.X).Select(z => z.ToList().OrderBy(x => x.Y)).Select(x => x.First()).ToList();


                var x0 = pp.Min(x => x.X);
                origem = new P3d(x0, y, 0);


            }
            else
            {
                var inferiores = ptss.FindAll(x => x.Y <= Centro.Y - Offset_Centro);
                //encontra todos com Y menor que o meio
                if (inferiores.Count > 0)
                {
                    pp.AddRange(inferiores);
                }
                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhum contorno encontrado abaixo do centro");
                    y = 0;
                    origem = new P3d();

                    return;
                }

                y = pp.Min(x => x.Y);

                pp = pp.Select(x => x.Round(1)) //arredonda os valores
                .OrderBy(x => x.X).GroupBy(x => x.X) //agrupa por X
                .Select(x => x.ToList().OrderBy(z => z.Y)) //organiza por coordenada Y
                .Select(x => x.First()).ToList().Select(x => new P3dCAD(x.Origem, 0, Tipo_Coordenada.Linha)).ToList();

                if (Furos_Vista_Baixo)
                {
                    var ps = Getpts_furos_vista();

                    //pega os furos da vista, organiza os mesmos pela menor coordenada em Y;
                    var furos_vista = ps
                         .GroupBy(x => x.X).Select(x => x.ToList().OrderBy(z => z.Y).First()).ToList();

                    pp.AddRange(furos_vista.Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
                }

                if (Furos_Corte_Baixo)
                {
                    //adiciona os furos de corte;
                    var furos_corte = Getpts_furos_corte_verticais().FindAll(x => x.Y <= Centro.Y - Offset_Centro);
                    pp.AddRange(furos_corte.Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
                }




                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhuma coordenada encontrada abaixo do centro");
                    return;
                }

                //agrupa as coordenadas e pega a que tem menor y. Isso faz com que quando tem mais de um ponto com a mesma coordenada X, 
                //ele pegue o menor valor em Y
                pp = pp.GroupBy(z => z.X).Select(z => z.ToList().OrderBy(x => x.Y)).Select(x => x.Last()).ToList();

                var x0 = pp.Min(x => x.X);
                origem = new P3d(x0, y, 0);
            }

            AddBarra();
            AddMensagem("\nCoordenadas Horizontais:\n");
            pp = pp.OrderBy(x => x.X).ToList();
            pp = pp.GroupBy(x => x.X).Select(x => x.First()).ToList();
            Aninhar(pp);


            //se ativou pra cotar em cima e é uma vista inferior
            if (Furos_Vista_Cima && !superior && !Furos_Vista_Baixo)
            {
                pp = pp.FindAll(x => x.Tipo != Tipo_Coordenada.Furo_Vista);
            }
            else if (!Furos_Vista_Cima && superior && Furos_Vista_Baixo)
            {
                pp = pp.FindAll(x => x.Tipo != Tipo_Coordenada.Furo_Vista);
            }

            //AddMensagem(string.Join("\n", pp.Select(x => x.ToString())));
            //AddBarra();

            retorno = pp.Select(x => x.Origem).ToList();
        }

        private void Aninhar(List<P3dCAD> pp)
        {
            for (int i = 0; i < pp.Count; i++)
            {
                if (pp.Count == 1)
                {

                }
                else if (pp.Count == 2 && i > 0)
                {

                }
                else if (i == 0)
                {
                    pp[i].Origem.Proximo = pp[i + 1].Origem;
                }

                else if (i > 0 && i < pp.Count - 2)
                {
                    pp[i].Origem.Proximo = pp[i + 1].Origem;
                    pp[i].Origem.Anterior = pp[i - 1].Origem;

                }
                else
                {
                    pp[i].Origem.Anterior = pp[i - 1].Origem;
                }
                pp[i].Origem.id = i;
            }
        }

        public List<P3dCAD> GetPts_lado_esquerdo(bool agrupar = true)
        {
            List<P3dCAD> lista = new List<P3dCAD>();
            lista.AddRange(GetContorno());
            //lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
            double prof = Profundidade_Esquerda;
            if ((Base_Esquerda | (Base_Direita && Base_Esquerda)) && !Tipo_Desenho.StartsWith("C"))
            {
                //remove todas as coordenadas se tem placa base, deixando apenas os que tem xmax
                //isso vai funcionar bem em chapas retas, no entando em chapas inclinadas vai ficar uma bosta
                var pxmax = lista.Select(x => x.X).Min();
                lista = lista.FindAll(x => x.X <= pxmax + 1);

                lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Linha)).ToList());
            }
            else if (Furos_Vista_Corte_Cotar_Esquerda | !Tipo_Desenho.StartsWith("C"))
            {
                lista.AddRange(Getpts_furos_vista().Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
            }


            lista = lista.Select(x => new P3dCAD(x, 1)).ToList().GroupBy(x => x.Origem.GetCid()).Select(x => x.First()).ToList().OrderBy(x => x.Y).ToList();

            if (lista.Count == 0)
            {
                return new List<P3dCAD>();
            }

            //forçando usar o maximo em X encontrado na lista ao invés do max X global
            double minX = lista.Min(x => x.X);

            if (!Tipo_Desenho.StartsWith("C"))
            {
                lista = lista.FindAll(x => x.X <= prof + this.MinX).ToList();
            }
            else
            {
                lista = lista.FindAll(x => x.X <= this.Centro.X).ToList();
            }

            lista = lista.OrderBy(x => x.Y).ToList();


            if (agrupar)
            {
                lista = lista.GroupBy(y => y.Y).Select(y => y.ToList().OrderBy(x => x.X)).Select(x => x.First()).ToList();
            }


            lista = RemovePtsDistMin_Y(lista, D_Min_Y);


            Aninhar(lista);

            return lista;
        }
        private List<P3dCAD> GetPts_lado_direito(bool agrupar = true)
        {
            List<P3dCAD> lista = new List<P3dCAD>();
            lista.AddRange(GetContorno());
            double prof = Profundidade_Direita;
            if ((Base_Direita | (Base_Direita && Base_Esquerda)) && !Tipo_Desenho.StartsWith("C"))
            {
                //remove todas as coordenadas se tem placa base, deixando apenas os que tem xmax
                //isso vai funcionar bem em chapas retas, no entando em chapas inclinadas vai ficar uma bosta
                var pxmax = lista.Select(x => x.X).Max();
                lista = lista.FindAll(x => x.X >= pxmax - 1);

                lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
            }
            else if (Furos_Vista_Corte_Cotar_direita | !Tipo_Desenho.StartsWith("C"))
            {
                lista.AddRange(Getpts_furos_vista().Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
            }
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Com furos");

            lista = lista.Select(x => new P3dCAD(x, 1)).ToList().GroupBy(x => x.Origem.GetCid()).Select(x => x.First()).ToList().OrderBy(x => x.Y).ToList();
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Agrupadas");

            if (lista.Count == 0)
            {
                return new List<P3dCAD>();
            }
            //forçando usar o maximo em X encontrado na lista ao invés do max X global
            double maxX = lista.Max(x => x.X);

            if (!Tipo_Desenho.StartsWith("C"))
            {
                lista = lista.FindAll(x => x.X >= maxX - prof);

            }
            else
            {
                lista = lista.FindAll(x => x.X >= this.Centro.X);
            }
            lista = lista.OrderBy(x => x.Y).ToList();
            AddMensagem("\nCentro:\n" + Centro + "\n");
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Agrupadas");



            if (agrupar)
            {
                lista = lista.GroupBy(y => y.Y).Select(y => y.ToList().OrderBy(x => x.X)).Select(x => x.Last()).ToList();
            }


            lista = RemovePtsDistMin_Y(lista, D_Min_Y);

            Aninhar(lista);
            return lista;
        }



        private List<P3dCAD> GetContorno()
        {
            var linhas = Getpts_linhas_perfil();
            var projecoes = Getpts_linhas_projecao();
            var retorno = new List<P3dCAD>();
            if (linhas.Count == 0)
            {
                return new List<P3dCAD>();
            }
            try
            {

                var contorno = new DLM.desenho.Contorno.Hull(linhas);
                var contorno_perfil = contorno.GetPontos(this.Concavidade_Contorno, Escala_Contorno).Select(x => new Point3d(x.X, x.Y, 0)).ToList();
                retorno.AddRange(contorno_perfil.Select(x => new P3dCAD(x, 0, Tipo_Coordenada.Linha)));

                retorno = retorno.RemoverRepetidos();

                var uniao = retorno.Select(x => x.Origem).ToList().GetPath().GetPathsD().Union(Clipper2Lib.FillRule.Positive).GetFaces();

                if (Tipo_Desenho.StartsWith("C") && projecoes.Count > 0)
                {
                    var conv = GetContornoConvexo(projecoes.P3dCAD()).ToList();
                    retorno.AddRange(conv);
                }
                //Setids(retorno);
                return retorno;

            }
            catch (System.Exception)
            {

            }

            return new List<P3dCAD>();
        }
        private List<P3d> Getpts_furos_vista()
        {
            var s = GetBlocos_Furos_Vista();
            if (s.Count > 0)
            {
                return GetBlocos_Furos_Vista().Select(x => x.Position).Select(x => new P3d(Math.Round(x.X), Math.Round(x.Y), Math.Round(x.Z))).ToList();

            }
            return new List<P3d>();
        }
        private List<P3d> Getpts_furos_corte_verticais()
        {
            var s = GetFuros_corte();
            if (s.Count == 0) { return new List<P3d>(); }

            try
            {
                return s.FindAll(x =>
                (
                x.Rotation.RadianosParaGraus() >= -8
                &&
                x.Rotation.RadianosParaGraus() <= 8
                ) | (
                x.Rotation.RadianosParaGraus() >= 172
                &&
                x.Rotation.RadianosParaGraus() <= 188
                )

                ).Select(x => x.Position).Select(x => new P3d(Math.Round(x.X, 2), Math.Round(x.Y, 2), 0)).ToList();
            }
            catch (System.Exception)
            {

            }
            return new List<P3d>();
        }
        private List<P3d> Getpts_furos_corte_horizontais()
        {
            var s = GetFuros_corte();
            if (s.Count == 0) { return new List<P3d>(); }
            try
            {
                return s.FindAll(x =>
                x.Rotation.RadianosParaGraus() == 90 |
                x.Rotation.RadianosParaGraus() == 270

                ).Select(x => x.Position).Select(x => new P3d(Math.Round(x.X, 2), Math.Round(x.Y, 2), 0)).ToList();

            }
            catch (System.Exception)
            {

            }
            return new List<P3d>();
        }
        private List<P3d> Getpts_linhas_perfil()
        {
            List<P3d> pp = new List<P3d>();
            foreach (var s in Getlinhas_perfil())
            {
                pp.Add(s.P1);
                pp.Add(s.P2);
            }

            return pp;
        }
        private List<P3d> Getpts_linhas_projecao()
        {
            List<P3d> pp = new List<P3d>();
            foreach (var s in Getlinhas_projecao())
            {
                pp.Add(s.P1);
                pp.Add(s.P2);
            }



            return pp;
        }

        #endregion

        #region Prompts e Mensagens
        public void AddMensagemCotas(List<P3dCAD> pts, string titulo)
        {
            AddBarra();
            AddMensagem("\n" + titulo + "\n");
            AddMensagem(string.Join("\n", pts.Select(x => x.ToString())));
            AddBarra();
        }

        #endregion

        [Category("Cantos Profundidade")]
        [DisplayName("Esquerda")]
        [Browsable(false)]
        public double Profundidade_Esquerda { get; set; } = 50;

        [Category("Mapeamento")]
        [DisplayName("Ignorar Linhas Proj. Menores que")]
        [Browsable(false)]
        public double Tam_Minimo_Projecao { get; set; } = 25;

        [Category("Cantos Profundidade")]
        [DisplayName("Direita")]
        [Browsable(false)]
        public double Profundidade_Direita { get; set; } = 50;

        [Category("Cantos Profundidade")]
        [DisplayName("Furos %")]
        [Browsable(false)]
        public double Profundidade_Furos_Porcentagem { get; set; } = 20;
        [XmlIgnore]
        private double Profundidade_Furos_Porcentagem_valor
        {
            get
            {
                return (Profundidade_Furos_Porcentagem / 100 * this.Largura);
            }
        }

        #region Configurações
        [Browsable(false)]
        public string Tipo_Desenho { get; set; } = "";
        private double Ordinate_Anterior { get; set; } = 0;
        [Browsable(false)]
        public bool Cotar_Embaixo { get; set; } = false;
        [Browsable(false)]
        public bool Acumuladas_Cima { get; set; } = true;
        [Browsable(false)]
        public bool Acumuladas_Baixo { get; set; } = false;
        [Browsable(false)]
        public bool Cotar_Em_Cima { get; set; } = true;
        [Browsable(false)]
        public bool Cotar_Direita { get; set; } = false;
        [Browsable(false)]
        public bool Cotar_Esquerda { get; set; } = true;
        [Browsable(false)]
        public bool Base_Esquerda { get; set; } = false;
        [Browsable(false)]
        public bool Base_Direita { get; set; } = false;

        [Category("Offset Cotas")]
        [DisplayName("Total")]
        public double Dist0 { get; set; } = 20;
        [Category("Offset Cotas")]
        [DisplayName("Geral")]
        public double Dist1 { get; set; } = 15;
        [Category("Offset Cotas")]
        [DisplayName("Acumuladas")]
        public double Dist2 { get; set; } = 3;
        [Category("Cotas")]
        [DisplayName("Width Factor")]
        public double Widthfactor { get; set; } = 0.8;
        [Category("Cotas")]
        [DisplayName("Juntar Cotas Iguais")]
        public bool Juntar_Cotas { get; set; } = true;
        [Category("Marcas e Posições")]
        [DisplayName("Ajustar Escala")]
        public bool Marcas_Ajusta_Escala { get; set; } = false;
        [Category("Cotas")]
        [DisplayName("Estilo")]
        public string Estilo_Padrao { get; set; } = "ROMANS";
        [Category("Cotas")]
        [DisplayName("Fonte")]
        public string Estilo_Padrao_Fonte { get; set; } = "romans.shx";
        [Category("Cotas")]
        [DisplayName("Size")]
        public double Size { get; set; } = 1.8;
        [Category("Mapeamento")]
        public string LayerLinhas { get; set; } = "G";
        [Category("Mapeamento")]
        [DisplayName("Forçar Tamanho Texto")]
        public bool ForcarTamTexto { get; set; } = false;
        [Category("Mapeamento")]
        public string LayerProjecao { get; set; } = "TR";
        [Category("Mapeamento")]
        [DisplayName("Distância Mínima X")]
        public double D_Min_X { get; set; } = 2;
        [Category("Mapeamento")]
        [DisplayName("Distância Mínima Y")]
        public double D_Min_Y { get; set; } = 2;
        [Category("Cálculo")]
        [DisplayName("Contorno")]
        public Tipo_Calculo_Contorno Tipo_Calculo_Contorno { get; set; } = Tipo_Calculo_Contorno.Bordas;
        [Category("Furos Vista")]
        [DisplayName("Em Baixo")]
        public bool Furos_Vista_Baixo { get; set; } = false;
        [Category("Furos Vista")]
        [DisplayName("Em Cima")]
        public bool Furos_Vista_Cima { get; set; } = true;
        [Category("Furos Projeção")]
        [DisplayName("Em Cima")]
        public bool Furos_Corte_Cima { get; set; } = true;
        [Category("Furos Projeção")]
        [DisplayName("Em Baixo")]
        public bool Furos_Corte_Baixo { get; set; } = true;
        [Category("Furos Corte")]
        [DisplayName("Esquerda")]
        public bool Furos_Vista_Corte_Cotar_Esquerda { get; set; } = true;
        [Category("Furos Corte")]
        [DisplayName("Direita")]
        public bool Furos_Vista_Corte_Cotar_direita { get; set; } = false;
        [Category("Furos Vista")]
        [DisplayName("Adicionar Linha Projeção")]
        public bool Linha_Projecao { get; set; } = true;
        [Category("Furos Vista")]
        [DisplayName("Indicar Diâmetro")]
        public bool Indicar_Diametro { get; set; } = true;
        [Category("Furos Vista")]
        [DisplayName("Projeção - Dist. Mínima Entre Furos")]
        public double Linha_Projecao_Dist_Min { get; set; } = 25;
        [Category("Cálculo")]
        [DisplayName("Offset Centro %")]
        public double Offset_Centro_Valor { get; set; } = 10;
        private double Offset_Centro
        {
            get
            {
                if (Offset_Centro_Valor > 0)
                {
                    return Altura * Offset_Centro_Valor / 100;
                }

                return 0;
            }
        }
        [Category("Mapeamento")]
        [DisplayName("Concavidade")]
        public double Concavidade_Contorno { get; set; } = -1;
        [Category("Mapeamento")]
        [DisplayName("Contorno")]
        public int Escala_Contorno { get; set; } = 5;
        [Category("Mapeamento")]
        [DisplayName("Espessura")]
        public double Espessura_Contorno { get; set; } = 2;
        [XmlIgnore]
        private double Offset0
        {
            get
            {
                return GetEscala() * Dist0;
            }
        }
        [XmlIgnore]
        private double Offset1
        {
            get
            {
                return GetEscala() * Dist1;
            }
        }
        [XmlIgnore]
        private double Offset2
        {
            get
            {
                return GetEscala() * Dist2;
            }
        }
        #endregion

        private void Calcular_Cantos()
        {
            if (Tipo_Calculo_Contorno == Tipo_Calculo_Contorno.Bordas)
            {
                List<P3d> c = GetContornoConvexo().P3d();

                var ctr = c.Min(x => x.X) + (c.Max(x => x.X) - c.Min(x => x.X));

                var le = c.FindAll(x => x.X <= ctr);
                var ld = c.FindAll(x => x.X >= ctr);

                if (le.Count > 0 && ld.Count > 0)
                {

                    Superior_Esquerdo = new P3d(le.Min(x => x.X), le.Max(x => x.Y), 0);
                    Inferior_Esquerdo = new P3d(le.Min(x => x.X), le.Min(x => x.Y), 0);

                    Superior_Direito = new P3d(ld.Max(x => x.X), ld.Max(x => x.Y), 0);
                    Inferior_Direito = new P3d(ld.Max(x => x.X), ld.Min(x => x.Y), 0);
                }

                var cc = new List<P3d>{
                    new P3d(Superior_Esquerdo.X,Superior_Esquerdo.Y,0),
                    new P3d(Superior_Direito.X,Superior_Direito.Y,0),
                    new P3d(Inferior_Direito.X,Inferior_Direito.Y,0),
                    new P3d(Inferior_Esquerdo.X,Inferior_Esquerdo.Y,0),
                    new P3d(Superior_Esquerdo.X,Superior_Esquerdo.Y,0)

                }.Centro();

                Centro = new P3d(cc.X, cc.Y, 0);


            }
            else
            {
                var s = Getpts_linhas_perfil().ArredondarJuntar();
                Superior_Esquerdo = new P3d(s.Min(x => x.X), s.Max(x => x.Y), 0);
                Inferior_Esquerdo = new P3d(s.Min(x => x.X), s.Min(x => x.Y), 0);

                Superior_Direito = new P3d(s.Max(x => x.X), s.Max(x => x.Y), 0);
                Inferior_Direito = new P3d(s.Max(x => x.X), s.Min(x => x.Y), 0);


                Centro = Getpts_linhas_perfil().Centro();
            }


        }
        public List<LinhaBlocoFuro> GetFurosPorDiam()
        {
            List<LinhaBlocoFuro> coordenadas = new List<LinhaBlocoFuro>();
            List<BlocoFuro> furos = GetFurosVista();

            var ret = furos.GroupBy(x => x.GetChave()).Select(x => new LinhaBlocoFuro(x.ToList())).ToList();

            return ret;
        }



        public List<LinhaBlocoFuro> GetCoordenadasFurosProjecao()
        {
            List<LinhaBlocoFuro> coordenadas = new List<LinhaBlocoFuro>();
            var furos = this.GetBlocos_Furos_Vista();
            var ys = furos.Select(x => Math.Round(x.Position.Y)).Distinct().ToList().OrderBy(x => x).ToList();
            for (int i = 0; i < ys.Count; i++)
            {
                coordenadas.Add(new LinhaBlocoFuro(furos.FindAll(x => Math.Round(x.Position.Y) == ys[i] | Math.Round(x.Position.Y) + 1 == ys[i] | Math.Round(x.Position.Y) - 1 == ys[i]).ToList(), ys[i]));
            }

            return coordenadas;
        }
        private List<P3dCAD> GetContornoConvexo(List<P3dCAD> sss = null)
        {
            double escala = 1000;
            if (sss == null)
            {
                sss = Getpts_linhas_perfil().P3dCAD().ArredondarJuntar();
            }
            if (sss.Count > 0)
            {
                var juntar = DLM.desenho.Contorno.GrahamScan.convexHull(sss.Select(x => new DLM.desenho.Contorno.Node(x.X * escala, x.Y * escala, 0)).ToList());
                var fim = juntar.Select(x => new P3dCAD(x.x / escala, x.y / escala, 0)).ToList();

                return fim;
            }
            return new List<P3dCAD>();
        }
        public void Contornar(bool calculo = true)
        {
            using (var acTrans = acCurDb.acTransST())
            {
                SelecionarObjetos();

                if (calculo)
                {
                    AddPolyLine(GetContorno().P3d(), Espessura_Contorno, 0, System.Drawing.Color.Red);
                }
                else
                {
                    Calcular_Cantos();
                    AddPolyLine(new List<P3d> { Superior_Esquerdo, Superior_Direito, Inferior_Direito, Inferior_Esquerdo, Superior_Esquerdo }, 2, 0, System.Drawing.Color.Blue);
                }



                AddMensagem("\nFinalizado.");
                acTrans.Commit();
            }

        }
        public void ContornarConvexo()
        {
            using (var acTrans = acCurDb.acTransST())
            {
                SelecionarObjetos();

                var pontos = GetContornoConvexo();
                AddBarra();
                AddMensagem(string.Join("\n", pontos));
                AddBarra();
                if (pontos.Count > 1)
                {
                    var first = pontos.First();
                    pontos.Add(first);
                    AddPolyLine(pontos.P3d(), Espessura_Contorno, 0, System.Drawing.Color.Red);
                }


                acTrans.Commit();
            };
        }
        public List<P3dCAD> RemovePtsDistMin_X(List<P3dCAD> pp, double xmin)
        {
            pp = pp.OrderBy(x => x.X).ToList();
            List<P3dCAD> pps = new List<P3dCAD>();
            for (int i = 0; i < pp.Count; i++)
            {
                if (i == 0)
                {
                    pps.Add(pp[i]);
                }
                else
                {
                    var p0 = pp[i];
                    var p1 = pps.Last();
                    var dist = Math.Abs(p0.X - p1.X);
                    if (dist >= xmin)
                    {
                        pps.Add(pp[i]);
                    }
                }

            }
            return pps;
        }
        public List<P3dCAD> RemovePtsDistMin_Y(List<P3dCAD> pp, double min)
        {
            pp = pp.OrderBy(x => x.Y).ToList();
            List<P3dCAD> pps = new List<P3dCAD>();
            for (int i = 0; i < pp.Count; i++)
            {
                if (i == 0)
                {
                    pps.Add(pp[i]);
                }
                else
                {
                    var p0 = pp[i];
                    var p1 = pps.Last();
                    var dist = Math.Abs(p0.Y - p1.Y);
                    if (dist >= min)
                    {
                        pps.Add(pp[i]);
                    }
                }

            }
            return pps;
        }
        public void Configurar()
        {
            var opt = Ut.PedirString("Selecione o tipo de configuração", new List<string> { "Contorno", "Dimensoes" });

            if (opt.StartsWith("C"))
            {
                ConfigurarContorno();
            }
            else if (opt.StartsWith("D"))
            {
                ConfigurarDesenho();
            }


        }
        private void ConfigurarDesenho()
        {
            Dist0 = Ut.PedirDouble("Digite a dist0", Dist0);
            Dist1 = Ut.PedirDouble("Digite a dist1", Dist1);
            Dist2 = Ut.PedirDouble("Digite a espessura", Dist2);
            Profundidade_Esquerda = Ut.PedirDouble("Digite a profundidade_corte", Profundidade_Esquerda);
            Profundidade_Direita = Ut.PedirDouble("Digite a profundidade_corte", Profundidade_Direita);
        }
        public void ConfigurarContorno()
        {
            Concavidade_Contorno = Ut.PedirDouble("Digite a concavidade", Concavidade_Contorno);
            Escala_Contorno = Ut.PedirInteger("Digite a escala", Escala_Contorno);
            Espessura_Contorno = Ut.PedirDouble("Digite a espessura", Espessura_Contorno);
        }
        public void SetWidthFactor(double valor)
        {


            acDoc.Comando(
                    "_-style",
                    Estilo_Padrao,
                    Estilo_Padrao_Fonte,
                    "0",
                    Widthfactor,
                    "0",
                    "No",
                    "No",
                    "No"
                );
        }
        public bool OpcoesComMenu()
        {
            Tela mm = new Tela(this);
            var dialog = mm.ShowDialog();
            //pp.Opcoes();
            if (dialog == System.Windows.Forms.DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void MapeiaObjetos()
        {
            Calcular_Cantos();
            AddBarra();
            AddMensagem("\nEscala: " + GetEscala());
            AddMensagem("\nDist0: " + Dist0);
            AddMensagem("\nDist1: " + Dist1);
            AddMensagem("\nDist2: " + Dist2);
            AddMensagem("\nFuros na Vista: " + Getpts_furos_vista().Count);
            AddMensagem("\nFuros em corte: " + GetFuros_corte().Count);
            AddMensagem("\nFuros em corte sentido horizontal: " + Getpts_furos_corte_verticais().Count);
            AddMensagem("\nFuros em corte sentido vertical: " + Getpts_furos_corte_horizontais().Count);
            AddMensagem("\nCoordenadas de Bordas:");
            AddMensagem("\nSE: " + Superior_Esquerdo);
            AddMensagem("\nSD: " + Superior_Direito);
            AddMensagem("\nIE: " + Inferior_Esquerdo);
            AddMensagem("\nID: " + Inferior_Direito);
            AddMensagem("\nCENTRO: " + Centro);
            AddBarra();
            AddMensagem("\nOffset Centro: " + Offset_Centro);
            AddMensagem("\nAltura: " + Altura);
            AddMensagem("\nLargura: " + Largura);
            AddBarra();
            AddMensagem("\nLinhas:" + GetCADLines().Count);
            AddMensagem("\nLinhas de perfil:" + Getlinhas_perfil().Count);
            AddMensagem("\nProjeções:" + Getlinhas_projecao().Count);
            AddBarra();
        }

        [Browsable(false)]
        private double DistMinTexto_X
        {
            get
            {
                return this.GetEscala() * 4.2 * this.Widthfactor;
            }
        }
        [XmlIgnore]
        private bool Cota_Horizontal_Superior { get; set; } = true;
        [XmlIgnore]
        private bool Cota_Anterior_Movida { get; set; } = false;
        [XmlIgnore]
        private bool Ultima_Cota { get; set; } = false;
        [XmlIgnore]
        private int Sequencia { get; set; } = 0;
        [XmlIgnore]
        private int Max_Sequencia { get; set; } = 0;
        [XmlIgnore]
        private int Cotas_Movidas_Contagem { get; set; } = 0;
        private void AddCotasHorizontais(List<P3d> coords, double y, bool superior = true)
        {
            if (coords.Count < 2)
            {
                AddMensagem("Cotagem Horizontal abortada - Lista contém apenas " + coords.Count + " coordenadas");
                return;
            }
            Sequencia = 0;
            Cotas_Movidas_Contagem = 0;
            Cota_Anterior_Movida = false;
            Ultima_Cota = false;
            this.Cota_Horizontal_Superior = superior;
            Max_Sequencia = coords.Count - 2;
            if (superior)
            {

                for (int i = 0; i < coords.Count - 1; i++)
                {


                    Sequencia = i;
                    var dist = Math.Abs(coords[i + 1].X - coords[i].X);
                    if (coords[i].PegarIguaisX().Count > 0 && Juntar_Cotas)
                    {
                        AddCotaHorizontal(coords[i], coords[i + coords[i].PegarIguaisX().Count + 1], Offset1 + y);
                        i = i + coords[i].PegarIguaisX().Count;
                    }
                    else if (i == 0)
                    {
                        //cota da esquerda pra direita, ajustanto automaticamente a primeira cota, se a mesma for pequena
                        AddCotaHorizontal(coords[i + 1], coords[i], Offset1 + y);
                    }
                    else
                    {
                        AddCotaHorizontal(coords[i], coords[i + 1], Offset1 + y);
                    }
                }
                //cota de ponta a ponta
                Ultima_Cota = true;
                AddCotaHorizontal(coords[0], coords[coords.Count - 1], Offset0 + y + (Cotas_Movidas_Contagem > 0 ? (Offset0 - Offset1) : 0));
            }
            else
            {
                //cotar em baixo
                for (int i = 0; i < coords.Count - 1; i++)
                {
                    Sequencia = i;
                    if (coords[i].PegarIguaisX().Count > 0 && Juntar_Cotas)
                    {
                        AddCotaHorizontal(coords[i], coords[i + coords[i].PegarIguaisX().Count + 1], -Offset1 + y);
                        i = i + coords[i].PegarIguaisX().Count;
                    }
                    else if (i == 0)
                    {
                        //cota da esquerda pra direita, ajustanto automaticamente a primeira cota, se a mesma for pequena
                        AddCotaHorizontal(coords[i + 1], coords[i], -Offset1 + y);
                    }
                    else
                    {
                        AddCotaHorizontal(coords[i], coords[i + 1], -Offset1 + y);
                    }
                }
                //cota de ponta a ponta
                Ultima_Cota = true;
                AddCotaHorizontal(coords[0], coords[coords.Count - 1], -Offset0 + y - (Cotas_Movidas_Contagem > 0 ? (Offset0 - Offset1) : 0));
            }

        }
        private void AddCotasAcumuladas(List<P3d> pp, double y, P3d origem, bool superior = true)
        {
            Cotas_Movidas_Contagem = 0;
            Sequencia = 0;
            Cota_Anterior_Movida = false;
            Ultima_Cota = false;
            Max_Sequencia = pp.Count - 2;
            Ordinate_Anterior = origem.X;

            var coordy2 = Offset2 + y;
            if (!superior)
            {
                coordy2 = -Offset2 + y;
            }
            var coordy1 = Offset1 + y;
            if (!superior)
            {
                coordy1 = -Offset1 + y;
            }

            //0 - Sem chapa
            if (!Base_Esquerda && !Base_Direita && pp.Count > 2)
            {
                for (int i = 0; i < pp.Count; i++)
                {
                    Sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new P3d(pp[i].X, coordy2, 0));

                }
            }

            //1 - Só na esquerda
            else if (Base_Esquerda && !Base_Direita && pp.Count > 2)
            {
                origem = new P3d(pp[1].X, y, 0);
                for (int i = 1; i < pp.Count; i++)
                {
                    Sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new P3d(pp[i].X, coordy2, 0));
                }
            }

            //2 - Só na direita
            else if (!Base_Esquerda && Base_Direita && pp.Count > 3)
            {
                for (int i = 0; i < pp.Count - 1; i++)
                {
                    Sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new P3d(pp[i].X, coordy2, 0));
                }
            }

            //3 - Ambos os lados
            else if (Base_Direita && Base_Esquerda && pp.Count > 3)
            {
                origem = new P3d(pp[1].X, y, 0);
                for (int i = 1; i < pp.Count - 1; i++)
                {
                    Sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new P3d(pp[i].X, coordy2, 0));
                }
            }


        }
        private void AddCotasVerticais(List<P3d> pp, double xmin, double xmin2)
        {
            Cotas_Movidas_Contagem = 0;
            Sequencia = 0;
            Cota_Anterior_Movida = false;
            Ultima_Cota = false;
            Max_Sequencia = pp.Count - 2;
            if (pp.Count > 1)
            {
                for (int i = 0; i < pp.Count - 1; i++)
                {
                    Sequencia = i;
                    var distancia = pp[i + 1].Y - pp[i].Y;
                    if (pp[i].PegarIguaisY().Count > 0 && Juntar_Cotas)
                    {
                        AddBarra();
                        AddMensagem("\n" + pp[i] + "\n");
                        AddMensagem("\nCotas Iguais:\n");
                        AddMensagem(string.Join("\n", pp[i].PegarIguaisY()));
                        AddCotaVertical(pp[i + pp[i].PegarIguaisY().Count + 1], pp[i], xmin);
                        i = i + pp[i].PegarIguaisY().Count;
                    }
                    else if (i == pp.Count - 2)
                    {
                        //inverte a cotagem para por a cota pequena de borda (se tiver)
                        AddCotaVertical(pp[i], pp[i + 1], xmin);
                    }
                    else
                    {
                        AddCotaVertical(pp[i + 1], pp[i], xmin);
                    }

                }
                //cota final ponta a ponta
                Ultima_Cota = true;
                AddCotaVertical(pp[0], pp[pp.Count - 1], xmin2);
            }
        }
        public void AddMLeader(Point3d origem, Point3d pt2, string texto)
        {


            using (var acTrans = acCurDb.acTransST())
            {
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;



                MLeader leader = new MLeader();
                //leader.SetDatabaseDefaults();
                leader.ContentType = ContentType.MTextContent;
                leader.MLeaderStyle = acCurDb.MLeaderstyle;

                MText mText = new MText();
                mText.TextStyleId = acCurDb.Textstyle;
                //mText.SetDatabaseDefaults();

                //mText.Width = 100;
                //mText.Height = 50;
                mText.SetContentsRtf(texto);
                mText.Location = pt2;

                mText.UseBackgroundColor = true;
                mText.BackgroundFill = true;
                mText.BackgroundFillColor = Color.FromColorIndex(ColorMethod.ByAci, 1);

                leader.MText = mText;



                int idx = leader.AddLeaderLine(origem);
                leader.AddFirstVertex(idx, origem);

                acBlkTblRec.AppendEntity(leader);
                acTrans.AddNewlyCreatedDBObject(leader, true);

                acTrans.Commit();

            }
        }
        private void AddCotasVerticaisCantos()
        {
            if (Cotar_Esquerda)
            {

                try
                {
                    var pts = GetPts_lado_esquerdo().OrderBy(y => y.Y).ToList().Select(x=>x.Origem).ToList();
                    AddMensagem("\nCoordenadas lado esquerdo:\n");
                    AddMensagem(string.Join("\n", pts.Select(x => x.ToString())));
                    if (pts.Count > 0)
                    {
                        var xmin = pts.Min(y => y.X) - Offset1;
                        var xmin2 = pts.Min(y => y.X) - Offset0;

                        AddCotasVerticais(pts, xmin, xmin2);
                    }
                    AddBarra();

                }
                catch (System.Exception ex)
                {
                    DLM.log.Log(ex);
                    AddBarra();
                    AddMensagem("\n Erro");
                    AddBarra();
                    AddMensagem("\n" + ex.Message);
                    AddMensagem("\n" + ex.StackTrace);
                    AddBarra();
                }

            }

            if (Cotar_Direita)
            {
                try
                {
                    var pts = GetPts_lado_direito().OrderBy(y => y.Y).ToList().Select(x=>x.Origem).ToList();
                    AddMensagem("\nCoordenadas lado direito:\n");
                    AddMensagem(string.Join("\n", pts));
                    if (pts.Count > 0)
                    {
                        var xmin = pts.Max(y => y.X) + Offset1;
                        var xmin2 = pts.Max(y => y.X) + Offset0;

                        AddCotasVerticais(pts, xmin, xmin2);
                    }
                }
                catch (System.Exception ex)
                {
                    DLM.log.Log(ex);
                    AddBarra();
                    AddMensagem("\n Erro");
                    AddBarra();
                    AddMensagem("\n" + ex.Message);
                    AddMensagem("\n" + ex.StackTrace);
                    AddBarra();

                }
                AddBarra();
            }

        }
        public void AddCotaVertical(P3d inicio, P3d fim, double x, string texto = "")
        {
            RotatedDimension acRotDim;
            // AddMensagem("\nCota vertical: p1:" + inicio + " p2:" + fim);
            double dist = Math.Abs(fim.Y - inicio.Y);
            //if (dist == 0)
            //{
            //    return;
            //}
            double y = (inicio.Y > fim.Y ? inicio.Y : fim.Y) - (dist / 2);
            P3d posicao = new P3d(x, y, 0);


            bool dimtix = false;

            if (Sequencia == 0 | Sequencia == Max_Sequencia)
            {
                dimtix = false;
            }
            else
            {
                dimtix = true;
            }


            double tam = 0;
            if (ForcarTamTexto)
            {
                tam = Size;
            }

            acRotDim = AddCotaVertical(inicio, fim, texto, posicao, dimtix, tam, Juntar_Cotas, Ultima_Cota);
        }
        public void AddCotaHorizontal(P3d inicio, P3d fim, double y, string texto = "")
        {
            RotatedDimension acRotDim;
            //AddMensagem("\nCota horizontal: p1:" + inicio + " p2:" + fim);
            double dist = Math.Abs(fim.X - inicio.X);
            //if (dist == 0)
            //{
            //    return null;
            //}
            var posicao = new P3d((inicio.X > fim.X ? inicio.X : fim.X) - (dist / 2), y, 0);
            bool movida = false;
            if (dist < DistMinTexto_X && Sequencia > 0 && Sequencia != Max_Sequencia)
            {
                if (Cota_Anterior_Movida)
                {
                    Cota_Anterior_Movida = false;
                }
                else
                {
                    Cotas_Movidas_Contagem++;
                    movida = true;
                    posicao = new P3d(posicao.X, posicao.Y + (this.Cota_Horizontal_Superior ? (Offset0 - Offset1) : -(Offset0 - Offset1)), posicao.Z);
                }
            }

            bool dimtix = false;
            if (Sequencia == 0 | Sequencia == Max_Sequencia | movida)
            {
                dimtix = false;
            }
            else
            {
                dimtix = true;
            }
            double tam = 0;
            //tam_texto
            if (ForcarTamTexto)
            {
                tam = Size;

            }

            acRotDim = AddCotaHorizontal(inicio, fim, texto, posicao, dimtix, tam, Juntar_Cotas, Ultima_Cota);
        }
        public void AddCotaOrdinate(P3d pontozero, P3d ponto, P3d posicao)
        {
            double tam = 0;

            //tam_texto
            if (ForcarTamTexto)
            {
                tam = Size;

            }
            double dist = Math.Abs(ponto.X - Ordinate_Anterior);


            if (Sequencia > 0 && dist < (GetEscala() * 2.1) && Sequencia != Max_Sequencia)
            {
                if (Cota_Anterior_Movida)
                {
                    Cota_Anterior_Movida = false;
                }
                else
                {
                    posicao = new P3d(posicao.X + (2.2 * GetEscala()), posicao.Y, posicao.Z);
                    Cota_Anterior_Movida = true;
                }
            }

            AddCotaOrdinate(pontozero, ponto, posicao, tam);
            Ordinate_Anterior = ponto.X;
        }
        public TextStyleTableRecord GetStyle(string nome)
        {
            TextStyleTableRecord ret = null;
            Database acCurDb = HostApplicationServices.WorkingDatabase;
            using (var acTrans = acCurDb.acTransST())
            {
                SymbolTable symTable = (SymbolTable)acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForRead);
                foreach (ObjectId id in symTable)
                {
                    TextStyleTableRecord symbol = (TextStyleTableRecord)acTrans.GetObject(id, OpenMode.ForRead);

                    if (symbol.Name.ToUpper() == nome)
                    {
                        ret = symbol;
                    }

                }

                acTrans.Commit();
            }
            return ret;
        }
        /// <summary>
        /// Esse cara habilita a opção das cotas não serem jogadas pro lado quando a distância for muito pequena
        /// </summary>
        public void SetDimtix(bool valor, double size)
        {
            // Get the current database
            // Start a transaction
            using (var acTrans = acCurDb.acTransST())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;



                DimStyleTableRecord acDimStyleTbl = acTrans.GetObject(acCurDb.Dimstyle, OpenMode.ForWrite) as DimStyleTableRecord;
                if (acDimStyleTbl != null)
                {
                    acDimStyleTbl.Dimtix = valor;
                    //tam_texto
                    if (ForcarTamTexto)
                    {
                        acDimStyleTbl.Dimtxt = size;

                    }
                    acTrans.Commit();
                    editor.Regen();
                }
                else
                {
                    AddMensagem("Não achei a cota para poder mexer");
                }
            };
        }
        private string CotarPeca()
        {
            var selecao = SelecionarObjetos();
            if (selecao.Status != PromptStatus.OK)
            {
                return "";
            }

            //limpa as cotas atuais
            acDoc.Apagar(Selecoes.GetDimmensions().FindAll(x => !(x is Leader) && !(x is MLeader) && !(x is DBText) && !(x is MText)));

            if (GetCADLines().Count == 0 | selecao.Status != PromptStatus.OK)
            {
                AddMensagem("\nNenhuma linha encontrada na seleção.\nÉ necessário selecionar uma peça de TecnoMetal.\nExploda a seleção antes de tentar cotar.");
                return "";
            }


            //escala = acCurDb.Dimscale;
            Size = acCurDb.Dimtxt;
            acCurDb.Dimtfill = 1;
            if (ForcarTamTexto)
            {
                acCurDb.Textsize = Size;

            }
            acCurDb.Dimtix = true;





            MapeiaObjetos();

            if (Marcas_Ajusta_Escala)
            {
                Blocos.SetEscala(this.GetBlocos_Marcas_Posicoes(), this.GetEscala());
            }


            if (GetContorno().Count == 0)
            {
                AddMensagem("Região selecionada não contém objetos cotáveis do TecnoMetal. \nExploda as vistas para poder usar a ferramenta.");
                return "Nada";
            }


            var pp = new List<P3d>();

            P3d origem;
            double y;
            //cotas horizotais superiores

            GetPontosHorizontais(out pp, out y, out origem, true);


            if (Cotar_Em_Cima)
            {
                AddCotasHorizontais(pp, y);
            }

            //cotas acumuladas
            if (Acumuladas_Cima && !Tipo_Desenho.StartsWith("C"))
            {
                AddCotasAcumuladas(pp, y, origem);
            }

            //cotas horizotais inferiores
            GetPontosHorizontais(out pp, out y, out origem, false);

            if (Cotar_Embaixo)
            {
                AddCotasHorizontais(pp, y, false);
            }

            if (Acumuladas_Baixo && !Tipo_Desenho.StartsWith("C"))
            {
                AddCotasAcumuladas(pp, y, origem, false);
            }

            //cotas dos cantos
            AddCotasVerticaisCantos();

            if (Linha_Projecao && !Tipo_Desenho.StartsWith("C"))
            {
                foreach (var s in this.GetCoordenadasFurosProjecao())
                {
                    AddLinha(s.Origem().Origem, s.Fim().Origem, "DASHDOT", System.Drawing.Color.Yellow);
                }
            }

            if (Indicar_Diametro && !Tipo_Desenho.StartsWith("C"))
            {
                foreach (var s in this.GetFurosPorDiam())
                {
                    Ut.AddLeader(s.Origem().Origem, s.Origem().Origem.Mover(Offset1, -Offset1 / 2), s.Nome, Size * this.GetEscala());
                }
            }

            AddMensagem("\nFinalizado.");
            AddBarra();
            return "Finalizado";



        }
        public void Cotar()
        {
        retentar:
            var st = OpcoesComMenu();
            if (st == false)
            {
                return;
            }
            if (Tipo_Desenho == "") { return; }




        denovo:
            try
            {
                var s = CotarPeca();
                if (s != "")
                {
                    goto denovo;
                }
                else
                {
                    var pp = Ut.PedirString("Deseja mudar as opções e continuar?", new List<string> { "Sim", "Não" });
                    if (pp.StartsWith("S"))
                    {
                        //Opcoes();
                        goto retentar;
                    }
                    return;
                }

            }
            catch (System.Exception ex)
            {

                Conexoes.Utilz.Alerta(ex);
            }

        }
    }



}
