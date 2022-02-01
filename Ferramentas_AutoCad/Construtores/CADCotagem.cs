using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
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

namespace DLM.cad
{
    [Serializable]
    public class CADCotagem :CADBase
    {
     

        #region CAD

        public List<CADLine> Getlinhas_perfil()
        {
            return GetLinhas().FindAll(x => x.Layer == LayerLinhas && x.Comprimento >= distancia_minima_X);
        }

        public List<CADLine> Getlinhas_projecao()
        {
            return GetLinhas().FindAll(x => x.Layer == LayerProjecao && x.Comprimento >= distancia_minima_X && x.Comprimento >= tam_minimo_projecao);
        }

        public List<BlockReference> Getfuros_vista()
        {
            return GetBlocos().FindAll(x =>
                 x.Name.ToUpper() == "M8"
                | x.Name.ToUpper() == "M10"
                | x.Name.ToUpper() == "M12"
                | x.Name.ToUpper() == "M14"
                | x.Name.ToUpper() == "M14_"
                | x.Name.ToUpper() == "M16"
                | x.Name.ToUpper() == "M18"
                | x.Name.ToUpper() == "M20"
                | x.Name.ToUpper() == "M22"
                | x.Name.ToUpper() == "M24"
                | x.Name.ToUpper() == "M27"
                | x.Name.ToUpper() == "M30"
                | x.Name.ToUpper() == "M33"
                | x.Name.ToUpper() == "M36"
                | x.Name.ToUpper() == "M39"
                | x.Name.ToUpper() == "M42"
                | x.Name.ToUpper() == "M45"
                | x.Name.ToUpper() == "M48"
                | x.Name.ToUpper() == "M52"
                | x.Name.ToUpper() == "M56"
                | x.Name.ToUpper() == "M60"
                | x.Name.ToUpper() == "M64"
                | x.Name.ToUpper() == "M68"
                | x.Name.ToUpper() == "M72"
                | x.Name.ToUpper() == "M76"
                | x.Name.ToUpper() == "M80"
                | x.Name.ToUpper() == "3D_INFOHOLE1"
                | x.Name.ToUpper() == "MA"
                );
        }
        public List<BlockReference> GetBlocos_Marcas_Posicoes()
        {
            return GetBlocos().FindAll(x =>
                 Constantes.BlocosTecnoMetalMarcas.Find(y=> y.ToUpper() == x.Name.ToUpper())!=null |
                 Constantes.BlocosTecnoMetalPosicoes.Find(y=> y.ToUpper() == x.Name.ToUpper())!=null 
                                    );
        }
        public List<BlockReference> GetFuros_corte()
        {
            return GetBlocos().FindAll(x => x.Name.ToUpper() == "MS");
        }
        #endregion
        public void GetVars(CADCotagem c)
        {
            this.acumuladas_baixo = c.acumuladas_baixo;
            this.acumuladas_cima = c.acumuladas_cima;
            this.base_direita = c.base_direita;
            this.base_esquerda = c.base_esquerda;
            this.concavidade_contorno = c.concavidade_contorno;
            this.cotar_direita = c.cotar_direita;
            this.cotar_embaixo = c.cotar_embaixo;
            this.cotar_emcima = c.cotar_emcima;
            this.cotar_esquerda = c.cotar_esquerda;
            this.cotas_movidas_contagem = c.cotas_movidas_contagem;
            this.cota_anterior_movida = c.cota_anterior_movida;
            this.cota_horizontal_superior = c.cota_horizontal_superior;
            this.dist0 = c.dist0;
            this.dist1 = c.dist1;
            this.dist2 = c.dist2;
            this.distancia_minima_X = c.distancia_minima_X;
            this.distancia_minima_Y = c.distancia_minima_Y;
            //this.escala = c.escala;
            this.escala_contorno = c.escala_contorno;
            this.espessura_contorno = c.espessura_contorno;
            this.estilo_padrao = c.estilo_padrao;
            this.estilo_padrao_fonte = c.estilo_padrao_fonte;
            this.ForcarTamTexto = c.ForcarTamTexto;
            this.furos_corte_baixo = c.furos_corte_baixo;
            this.furos_corte_cima = c.furos_corte_cima;
            this.furos_vista_baixo = c.furos_vista_baixo;
            this.furos_vista_cima = c.furos_vista_cima;
            this.furos_vista_corte_cotar_direita = c.furos_vista_corte_cotar_direita;
            this.furos_vista_corte_cotar_esquerda = c.furos_vista_corte_cotar_esquerda;
            this.juntar_cotas = c.juntar_cotas;
            this.LayerLinhas = c.LayerLinhas;
            this.LayerProjecao = c.LayerProjecao;
            this.marcas_ajusta_escala = c.marcas_ajusta_escala;
            this.offset_centro_valor = c.offset_centro_valor;
            this.profundidade_direita = c.profundidade_direita;
            this.profundidade_esquerda = c.profundidade_esquerda;
            this.profundidade_furos_porcentagem = c.profundidade_furos_porcentagem;
            this.size = c.size;
            this.tam_minimo_projecao = c.tam_minimo_projecao;
            this.tipo_calculo_contorno = c.tipo_calculo_contorno;
            this.tipo_desenho = c.tipo_desenho;
            this.widthfactor = c.widthfactor;
            
        }

        #region Pontos
        [XmlIgnore]
        private Coordenada se { get; set; } = new Coordenada();
        [XmlIgnore]
        private Coordenada ie { get; set; } = new Coordenada();
        [XmlIgnore]
        private Coordenada sd { get; set; } = new Coordenada();
        [XmlIgnore]
        private Coordenada id { get; set; } = new Coordenada();
        [XmlIgnore]
        private Coordenada centro { get; set; } = new Coordenada();
        private double altura
        {
            get
            {
               return (se.Y > sd.Y ? se.Y : sd.Y) - (ie.Y < id.Y ? ie.Y : id.Y);
            }
        }
        private double largura
        {
            get
            {
                return (sd.X > id.X ? sd.X : id.X) - (se.X < ie.X ? se.X : ie.X);
            }
        }
        private double minY
        {
            get
            {
                return id.Y < ie.Y ? id.Y : ie.Y;
            }
        }
        private double maxY
        {
            get
            {
                return sd.Y > se.Y ? sd.Y : se.Y;
            }
        }

        private double minX
        {
            get
            {
              return  se.X < ie.X ? se.X : ie.X;
            }
        }
        private double maxX
        {
            get
            {
                return sd.X > id.X ? sd.X : id.X;
            }
        }
        private void GetPontosHorizontais(out List<Coordenada> pp, out double y, out Point3d origem, bool superior = true)
        {
            pp = new List<Coordenada>();
            pp = new List<Coordenada>();
            origem = new Point3d();
            y = 0;

            var ptss = GetContorno();
            if (ptss.Count == 0)
            {
                AddMensagem("\nNenhum contorno encontrado acima do centro");
              
                return;
            }
            //ptss = RemovePtsDistMin_X(ptss, distancia_minima_X);
            y = 0;
            origem = new Point3d();


            if (superior)
            {

                var sts = ptss.FindAll(x => x.Y >= centro.Y + offset_centro).ToList();

                pp.AddRange(sts);

                //encontra todos com Y maior que o meio
                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhum contorno encontrado acima do centro");
                    return;
                }

                y = pp.Max(x => x.Y);


                pp = pp.Select(x => new Point3d(Math.Round(x.X, 1), Math.Round(x.Y, 1), Math.Round(x.Z, 1))) //arredonda os valores
                .OrderBy(x => x.X).GroupBy(x => x.X) //agrupa por X
                .Select(x => x.ToList().OrderBy(z => z.Y)) //organiza por coordenada Y
                .Select(x => x.Last()).ToList().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Linha)).ToList();


                if (this.furos_vista_cima)
                {
                    //pega os furos da vista, organiza os mesmos pela menor coordenada em Y;
                    var ps = Getpts_furos_vista();

                    var furos_vista = ps
                         .GroupBy(x => x.X).ToList().Select(x => x.ToList().OrderBy(z => z.Y).Last()).ToList();
                    pp.AddRange(furos_vista.Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
                }

                if(this.furos_corte_cima)
                {
                    //adiciona os furos de corte;
                    var furos_corte = Getpts_furos_corte_verticais().FindAll(x => x.Y >= centro.Y + offset_centro);
                    pp.AddRange(furos_corte.Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
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
                origem = new Point3d(x0, y, 0);


            }
            else
            {
                var sts = ptss.FindAll(x => x.Y <= centro.Y - offset_centro);
                //encontra todos com Y menor que o meio
                if (sts.Count > 0)
                {
                    pp.AddRange(sts);
                }
                if (pp.Count == 0)
                {
                    AddMensagem("\nNenhum contorno encontrado abaixo do centro");
                    y = 0;
                    origem = new Point3d();

                    return;
                }

                y = pp.Min(x => x.Y);

                pp = pp.Select(x => new Point3d(Math.Round(x.X, 1), Math.Round(x.Y, 1), Math.Round(x.Z, 1))) //arredonda os valores
                .OrderBy(x => x.X).GroupBy(x => x.X) //agrupa por X
                .Select(x => x.ToList().OrderBy(z => z.Y)) //organiza por coordenada Y
                .Select(x => x.First()).ToList().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Linha)).ToList();

                if (furos_vista_baixo)
                {
                    var ps = Getpts_furos_vista();

                    //pega os furos da vista, organiza os mesmos pela menor coordenada em Y;
                    var furos_vista = ps
                         .GroupBy(x => x.X).Select(x => x.ToList().OrderBy(z => z.Y).First()).ToList();

                    pp.AddRange(furos_vista.Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
                }

                if(furos_corte_baixo)
                {
                    //adiciona os furos de corte;
                    var furos_corte = Getpts_furos_corte_verticais().FindAll(x => x.Y <= centro.Y - offset_centro);
                    pp.AddRange(furos_corte.Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
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
                origem = new Point3d(x0, y, 0);
            }

            AddBarra();
            AddMensagem("\nCoordenadas Horizontais:\n");
            pp = pp.OrderBy(x => x.X).ToList();
            pp = pp.GroupBy(x => x.X).Select(x => x.First()).ToList();
            Setids(pp);


            //se ativou pra cotar em cima e é uma vista inferior
            if(furos_vista_cima && !superior && !furos_vista_baixo)
            {
                pp = pp.FindAll(x => x.Tipo != Tipo_Coordenada.Furo_Vista);
            }
            else if (!furos_vista_cima && superior && furos_vista_baixo)
            {
                pp = pp.FindAll(x => x.Tipo != Tipo_Coordenada.Furo_Vista);
            }

            AddMensagem(string.Join("\n", pp.Select(x=>x.ToString())));
            AddBarra();

        }

        private void Setids(List<Coordenada> pp)
        {
            for (int i = 0; i < pp.Count; i++)
            {
                if(pp.Count ==1)
                {

                }
                else if(pp.Count==2 && i>0)
                {

                }
                else if (i == 0)
                {
                    pp[i].proxima = pp[i + 1];
                }
                
                else if (i > 0 && i < pp.Count - 2)
                {
                    pp[i].proxima = pp[i + 1];
                    pp[i].anterior = pp[i - 1];
                   
                }
                else
                {
                    pp[i].anterior = pp[i - 1];
                }
                pp[i].id = i;
            }
        }

        public List<Coordenada> GetPts_lado_esquerdo(bool agrupar = true)
        {
            List<Coordenada> lista = new List<Coordenada>();
            lista.AddRange(GetContorno());
            //lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
            double prof = profundidade_esquerda;
            if ((base_esquerda | (base_direita && base_esquerda)) && !tipo_desenho.StartsWith("C"))
            {
                //remove todas as coordenadas se tem placa base, deixando apenas os que tem xmax
                //isso vai funcionar bem em chapas retas, no entando em chapas inclinadas vai ficar uma bosta
                var pxmax = lista.Select(x => x.X).Min();
                lista = lista.FindAll(x => x.X <= pxmax + 1);

                lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Linha)).ToList());
            }
            else if(furos_vista_corte_cotar_esquerda | !tipo_desenho.StartsWith("C"))
            {
                lista.AddRange(Getpts_furos_vista().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
            }


            lista = lista.Select(x => new Coordenada(x, 1)).ToList().GroupBy(x => x.chave).Select(x => x.First()).ToList().OrderBy(x=>x.Y).ToList();

            if(lista.Count==0)
            {
                return new List<Coordenada>();
            }

            //forçando usar o maximo em X encontrado na lista ao invés do max X global
            double minX = lista.Min(x => x.X);

            if (!tipo_desenho.StartsWith("C"))
            {
                lista = lista.FindAll(x => x.X <= prof + this.minX).ToList();
            }
            else
            {
                lista = lista.FindAll(x => x.X <= this.centro.X).ToList();
            }

            lista = lista.OrderBy(x => x.Y).ToList();


            if (agrupar)
            {
            lista = lista.GroupBy(y => y.Y).Select(y => y.ToList().OrderBy(x=>x.X)).Select(x=>x.First()).ToList();
            }


            lista = RemovePtsDistMin_Y(lista, distancia_minima_Y);


            Setids(lista);

            return lista;
        }
        private List<Coordenada> GetPts_lado_direito(bool agrupar = true)
        {
            List<Coordenada> lista = new List<Coordenada>();
            lista.AddRange(GetContorno());
            double prof = profundidade_direita;
            if ((base_direita | (base_direita && base_esquerda))  && !tipo_desenho.StartsWith("C"))
            {
                //remove todas as coordenadas se tem placa base, deixando apenas os que tem xmax
                //isso vai funcionar bem em chapas retas, no entando em chapas inclinadas vai ficar uma bosta
                var pxmax = lista.Select(x => x.X).Max();
                lista = lista.FindAll(x => x.X >= pxmax - 1);

                lista.AddRange(Getpts_furos_corte_horizontais().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Corte)).ToList());
            }
            else if (furos_vista_corte_cotar_direita | !tipo_desenho.StartsWith("C"))
            {
                lista.AddRange(Getpts_furos_vista().Select(x => new Coordenada(x, 0, Tipo_Coordenada.Furo_Vista)).ToList());
            }
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Com furos");

            lista = lista.Select(x => new Coordenada(x, 1)).ToList().GroupBy(x=>x.chave).Select(x=>x.First()).ToList().OrderBy(x=>x.Y).ToList();
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Agrupadas");

            if (lista.Count == 0)
            {
                return new List<Coordenada>();
            }
            //forçando usar o maximo em X encontrado na lista ao invés do max X global
            double maxX = lista.Max(x => x.X);

            if (!tipo_desenho.StartsWith("C"))
            {
            lista = lista.FindAll(x => x.X >= maxX - prof);

            }
            else
            {
                lista = lista.FindAll(x => x.X >= this.centro.X);
            }
            lista = lista.OrderBy(x => x.Y).ToList();
            AddMensagem("\nCentro:\n" + centro + "\n");
            AddMensagemCotas(lista, "Cotas Verticais Lado Direito - Agrupadas");



            if (agrupar)
            {
                lista = lista.GroupBy(y => y.Y).Select(y => y.ToList().OrderBy(x => x.X)).Select(x => x.Last()).ToList();
            }


            lista = RemovePtsDistMin_Y(lista, distancia_minima_Y);

            Setids(lista);
            return lista;
        }

        private List<Coordenada> GetContorno()
        {
            var s = Getpts_linhas_perfil();
            var s2 = Getpts_linhas_projecao();
            List<Coordenada> retorno = new List<Coordenada>();
            if (s.Count == 0)
            {
                return new List<Coordenada>();
            }
            try
            {
                var pts = s.Select(x => new DLM.desenho.Contorno.Node(Math.Round(x.X, 1), Math.Round(x.Y,1), 0)).ToList();

                var contorno = new DLM.desenho.Contorno.ContornoPontos(pts);

                var contorno_perfil = contorno.Calcular(this.concavidade_contorno, escala_contorno).SelectMany(x => x.nodes).Select(x => new Point3d(x.x, x.y, 0)).ToList();

                retorno.AddRange(contorno_perfil.Select(x=> new Coordenada(x,0, Tipo_Coordenada.Linha)));

                retorno = RemoverRepetidos(retorno);
                //ordena as coordenadas em x e y
               // retorno = retorno.GroupBy(x => x.X).Select(x => x.ToList().OrderBy(y => y.Y)).ToList().SelectMany(x=>x).ToList();
            
                if (tipo_desenho.StartsWith("C") && s2.Count>0)
                {
                    var pts_projecao = s2.Select(x => new DLM.desenho.Contorno.Node(Math.Round(x.X, 1), Math.Round(x.Y, 1), 0)).ToList();
                    var contorno_pr = new DLM.desenho.Contorno.ContornoPontos(pts_projecao);


                    //var contorno_projecao = contorno_pr.Calcular(this.concavidade_contorno, escala_contorno).SelectMany(x => x.nodes).Select(x => new Point3d(x.x, x.y, 0)).ToList();

                    var conv = GetContornoConvexo(s2.Select(x=> new Coordenada(x)).ToList());
                    retorno.AddRange(conv);
                }
                Setids(retorno);
                return retorno;

            }
            catch (System.Exception)
            {

            }

            return new List<Coordenada>();
        }
        private List<Point3d> Getpts_furos_vista()
        {
            var s = Getfuros_vista();
            if (s.Count > 0)
            {
                return Getfuros_vista().Select(x => x.Position).Select(x => new Point3d(Math.Round(x.X), Math.Round(x.Y), Math.Round(x.Z))).ToList();

            }
            return new List<Point3d>();

        }
        private List<Point3d> Getpts_furos_corte_verticais()
        {
            var s = GetFuros_corte();
            if (s.Count == 0) { return new List<Point3d>(); }

            try
            {
                return s.FindAll(x =>
                (
                Angulo.RadianosParaGraus(x.Rotation) >= -8
                &&
                Angulo.RadianosParaGraus(x.Rotation) <= 8
                ) | (
                Angulo.RadianosParaGraus(x.Rotation) >= 172
                &&
                Angulo.RadianosParaGraus(x.Rotation) <= 188
                )

                ).Select(x => x.Position).Select(x => new Point3d(Math.Round(x.X, 2), Math.Round(x.Y, 2), 0)).ToList();
            }
            catch (System.Exception)
            {

            }
            return new List<Point3d>();
        }
        private List<Point3d> Getpts_furos_corte_horizontais()
        {
            var s = GetFuros_corte();
            if (s.Count == 0) { return new List<Point3d>(); }
            try
            {
                return s.FindAll(x =>
                Angulo.RadianosParaGraus(x.Rotation) == 90 |
                Angulo.RadianosParaGraus(x.Rotation) == 270

                ).Select(x => x.Position).Select(x => new Point3d(Math.Round(x.X,2), Math.Round(x.Y,2), 0)).ToList();

            }
            catch (System.Exception)
            {

            }
            return new List<Point3d>();
        }
        private List<Coordenada> Getpts_linhas_perfil()
        {
            List<Coordenada> pp = new List<Coordenada>();
            foreach (var s in Getlinhas_perfil())
            {
                pp.Add(new Coordenada(s.StartPoint));
                pp.Add(new Coordenada(s.EndPoint));
            }



            return pp;
        }
        private List<Point3d> Getpts_linhas_projecao()
        {
            List<Point3d> pp = new List<Point3d>();
            foreach (var s in Getlinhas_projecao())
            {
                pp.Add(s.StartPoint);
                pp.Add(s.EndPoint);
            }



            return pp;
        }
        private List<Coordenada> ArredondarJuntar(List<Coordenada> origem, int decimais_X = 0, int decimais_Y = 0)
        {
            try
            {
                return origem.Select(x => new Coordenada(Math.Round(x.X, decimais_X), Math.Round(x.Y, decimais_Y), 0)).GroupBy(x => "X: " + x.X + " Y:" + x.Y).Select(x => x.First()).ToList();

            }
            catch (System.Exception)
            {


            }
            return new List<Coordenada>();
        }
        #endregion

        #region Prompts e Mensagens
        public void AddMensagemCotas(List<Coordenada> pts, string titulo)
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
        public double profundidade_esquerda { get; set; } = 50;


        [Category("Mapeamento")]
        [DisplayName("Ignorar Linhas Proj. Menores que")]
        [Browsable(false)]
        public double tam_minimo_projecao { get; set; } = 25;

        [Category("Cantos Profundidade")]
        [DisplayName("Direita")]
        [Browsable(false)]
        public double profundidade_direita { get; set; } = 50;


        [Category("Cantos Profundidade")]
        [DisplayName("Furos %")]
        [Browsable(false)]
        public double profundidade_furos_porcentagem { get; set; } = 20;
        [XmlIgnore]
        private double profundidade_furos_porcentagem_valor
        {
            get
            {
                return (profundidade_furos_porcentagem / 100 * this.largura);
            }
        }


        #region Configurações



        [Category("Offset Cotas")]
        [DisplayName("Total")]
        public double dist0 { get; set; } = 20;
        [Category("Offset Cotas")]
        [DisplayName("Geral")]
        public double dist1 { get; set; } = 15;
        [Category("Offset Cotas")]
        [DisplayName("Acumuladas")]
        public double dist2 { get; set; } = 3;

        [Category("Cotas")]
        [DisplayName("Width Factor")]
        public double widthfactor { get; set; } = 0.8;


        [Category("Cotas")]
        [DisplayName("Juntar Cotas Iguais")]
        public bool juntar_cotas { get; set; } = true;
        [Category("Marcas e Posições")]
        [DisplayName("Ajustar Escala")]
        public bool marcas_ajusta_escala { get; set; } = false;
        [Category("Cotas")]
        [DisplayName("Estilo")]
        public string estilo_padrao { get; set; } = "ROMANS";
        [Category("Cotas")]
        [DisplayName("Fonte")]
        public string estilo_padrao_fonte { get; set; } = "romans.shx";
        [Category("Cotas")]
        [DisplayName("Size")]
        public double size { get; set; } = 1.8;
        [Category("Mapeamento")]
        public string LayerLinhas { get; set; } = "G";

        [Category("Mapeamento")]
        [DisplayName("Forçar Tamanho Texto")]
        public bool ForcarTamTexto { get; set; } = false;
        [Category("Mapeamento")]
        public string LayerProjecao { get; set; } = "TR";
        [Category("Mapeamento")]
        [DisplayName("Distância Mínima X")]
        public double distancia_minima_X { get; set; } = 2;
        [Category("Mapeamento")]
        [DisplayName("Distância Mínima Y")]
        public double distancia_minima_Y { get; set; } = 2;
        [Category("Cálculo")]
        [DisplayName("Contorno")]
        public Tipo_Calculo_Contorno tipo_calculo_contorno { get; set; } = Tipo_Calculo_Contorno.Bordas;

        [Category("Furos Vista")]
        [DisplayName("Em Baixo")]
        public bool furos_vista_baixo { get; set; } = false;
        [Category("Furos Vista")]
        [DisplayName("Em Cima")]
        public bool furos_vista_cima { get; set; } = true;

        [Category("Furos Projeção")]
        [DisplayName("Em Cima")]
        public bool furos_corte_cima { get; set; } = true;

        [Category("Furos Projeção")]
        [DisplayName("Em Baixo")]
        public bool furos_corte_baixo { get; set; } = true;


        [Category("Furos Corte")]
        [DisplayName("Esquerda")]
        public bool furos_vista_corte_cotar_esquerda { get; set; } = true;

        [Category("Furos Corte")]
        [DisplayName("Direita")]
        public bool furos_vista_corte_cotar_direita { get; set; } = false;


        [Category("Furos Vista")]
        [DisplayName("Adicionar Linha Projeção")]
        public bool linha_projecao { get; set; } = true;

        [Category("Furos Vista")]
        [DisplayName("Indicar Diâmetro")]
        public bool indicar_diametro { get; set; } = true;

        [Category("Furos Vista")]
        [DisplayName("Projeção - Dist. Mínima Entre Furos")]
        public double linha_projecao_dist_min { get; set; } = 25;

        public List<LinhaBlocoFuro> GetFurosPorDiam()
        {
            List<LinhaBlocoFuro> coordenadas = new List<LinhaBlocoFuro>();
            var furos = this.Getfuros_vista().Select(x => new BlocoFuro(x)).ToList();

            var ret = furos.GroupBy(x => x.GetChave()).Select(x => new LinhaBlocoFuro(x.ToList())).ToList();

            return ret;
        }

        public List<LinhaBlocoFuro> GetCoordenadasFurosProjecao()
        {
            List<LinhaBlocoFuro> coordenadas = new List<LinhaBlocoFuro>();
            var furos = this.Getfuros_vista();
            var ys = furos.Select(x => Math.Round(x.Position.Y)).Distinct().ToList().OrderBy(x => x).ToList();
            for (int i = 0; i < ys.Count; i++)
            {
                coordenadas.Add(new LinhaBlocoFuro(furos.FindAll(x => Math.Round(x.Position.Y) == ys[i] | Math.Round(x.Position.Y)+1 == ys[i] | Math.Round(x.Position.Y)-1 == ys[i]).ToList(), ys[i]));
            }

            return coordenadas;
        }

        [Category("Cálculo")]
        [DisplayName("Offset Centro %")]
        public double offset_centro_valor { get; set; } = 10;
        private double offset_centro
        {
            get
            {
                if (offset_centro_valor > 0)
                {
                    return altura * offset_centro_valor / 100;
                }

                return 0;
            }
        }

        [Category("Mapeamento")]
        [DisplayName("Concavidade")]
        public double concavidade_contorno { get; set; } = 1;
        [Category("Mapeamento")]
        [DisplayName("Contorno")]
        public int escala_contorno { get; set; } = 10;
        [Category("Mapeamento")]
        [DisplayName("Espessura")]
        public double espessura_contorno { get; set; } = 2;
        [XmlIgnore]
        private double offset0
        {
            get
            {
                return GetEscala() * dist0;
            }
        }
        [XmlIgnore]
        private double offset1
        {
            get
            {
                return GetEscala() * dist1;
            }
        }
        [XmlIgnore]
        private double offset2
        {
            get
            {
                return GetEscala() * dist2;
            }
        }

        #endregion
        private void Calcular_Cantos()
        {
            


            if (tipo_calculo_contorno == Tipo_Calculo_Contorno.Bordas)
            {
                List<Coordenada> c = GetContornoConvexo();

                var ctr = c.Min(x=>x.X) + (c.Max(x => x.X) - c.Min(x=>x.X));

                var le = c.FindAll(x => x.X <= ctr);
                var ld = c.FindAll(x => x.X >= ctr);

                if (le.Count > 0 && ld.Count > 0)
                {

                    se = new Coordenada(le.Min(x => x.X), le.Max(x => x.Y), 0);
                    ie = new Coordenada(le.Min(x => x.X), le.Min(x => x.Y), 0);

                    sd = new Coordenada(ld.Max(x => x.X), ld.Max(x => x.Y), 0);
                    id = new Coordenada(ld.Max(x => x.X), ld.Min(x => x.Y), 0);
                }

                var cc = GetCentro(
                    new List<Coordenada>{
                    new Coordenada(se.X,se.Y,0),
                    new Coordenada(sd.X,sd.Y,0),
                    new Coordenada(id.X,id.Y,0),
                    new Coordenada(ie.X,ie.Y,0),
                    new Coordenada(se.X,se.Y,0)

                }
                );

                centro = new Coordenada(cc.X, cc.Y, 0);


            }
            else
            {
                var s = ArredondarJuntar(Getpts_linhas_perfil().Select(x=>new Coordenada(x)).ToList());
                se = new Coordenada(s.Min(x => x.X), s.Max(x => x.Y), 0);
                ie = new Coordenada(s.Min(x => x.X), s.Min(x => x.Y), 0);

                sd = new Coordenada(s.Max(x => x.X), s.Max(x => x.Y), 0);
                id = new Coordenada(s.Max(x => x.X), s.Min(x => x.Y), 0);

                var cc = GetCentro(Getpts_linhas_perfil());

                centro = new Coordenada(cc.X, cc.Y, 0);
            }

           
        }

        public Coordenada GetCentro(List<Coordenada> ps)
        {
            if (ps.Count < 2)
            {
                return new Coordenada();
            }

          var centros =  GetCentroSimple(ps);
            AddMensagem("\nCentro S: " + centros + "\n");
            if(ps.Count==4)
            {
                AddMensagem("\nUtilizando Centro S: apenas 4 pontos\n");

                return centros;
            }

            // Add the first point at the end of the array.
            int num_points = ps.Count;
            Coordenada[] pts = new Coordenada[num_points + 1];
            ps.CopyTo(pts, 0);
            pts[num_points] = ps[0];

            // Find the centroid.
            double X = 0;
            double Y = 0;
            double second_factor;
            for (int i = 0; i < num_points; i++)
            {
                second_factor =
                    pts[i].X * pts[i + 1].Y -
                    pts[i + 1].X * pts[i].Y;
                X += (pts[i].X + pts[i + 1].X) * second_factor;
                Y += (pts[i].Y + pts[i + 1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            double polygon_area = Area(ps);
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            //if (X < 0)
            //{
            //    X = -X;
            //    Y = -Y;
            //}
            if((centros.X>0 && X<0)| (centros.X < 0 && X > 0))
            {
                X = -X;
            }

            if((centros.Y>0 && Y<0) | (centros.Y < 0 && Y > 0))
            {
                Y = -Y;
            }
            var c = new Coordenada(X, Y, 0);
            AddMensagem("\nCentro C: " + c + "\n");

            return c;
        }

        public Coordenada GetCentroSimple(List<Coordenada> ps)
        {
            var se = new Coordenada(ps.Min(x => x.X), ps.Max(x => x.Y), 0);
            var sd = new Coordenada(ps.Max(x => x.X), ps.Max(x => x.Y), 0);

            var ie = new Coordenada(ps.Min(x => x.X), ps.Min(x => x.Y), 0);
            var id = new Coordenada(ps.Max(x => x.X), ps.Min(x => x.Y), 0);

            var seXid = se.GetCentro(id);
            var sdXie = sd.GetCentro(ie);
            var cc = seXid.GetCentro(sdXie);

            return cc;
        }

        public static double Area(List<Coordenada> Points)
        {
            // Add the first point to the end.
            int num_points = Points.Count;
            Coordenada[] pts = new Coordenada[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
        }
        private List<Coordenada> GetContornoConvexo(List<Coordenada> sss = null)
        {
            if(sss==null)
            {
            sss = ArredondarJuntar(Getpts_linhas_perfil().Select(x=> new Coordenada(x)).ToList());
            }
            var c = DLM.desenho.Contorno.GrahamScan.convexHull(sss.Select(x => new DLM.desenho.Contorno.Node(x.X, x.Y, 0)).ToList()).Select(x => new Point3d(x.x, x.y, 0)).ToList();
            return c.Select(x=> new Coordenada(x)).ToList();
        }

        public void Contornar(bool calculo = true)
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos();

                if (calculo)
                {
                    AddPolyLine(GetContorno(), espessura_contorno,0, System.Drawing.Color.Red);

                }
                else
                {
                    Calcular_Cantos();
                    AddPolyLine(new List<Coordenada> { se, sd, id, ie, se }, 2,0, System.Drawing.Color.Blue);
                }



                AddMensagem("\nFinalizado.");
                acTrans.Commit();
            }

        }
        public void ContornarConvexo()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SelecionarObjetos();

                var s = GetContornoConvexo();
                AddBarra();
                AddMensagem(string.Join("\n", s));
                AddBarra();
                AddPolyLine(s, espessura_contorno,0, System.Drawing.Color.Red);
                acTrans.Commit();
            };
        }

        public List<Coordenada> RemovePtsDistMin_X(List<Coordenada> pp, double xmin)
        {
            pp = pp.OrderBy(x => x.X).ToList();
            List<Coordenada> pps = new List<Coordenada>();
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
                    if(dist>=xmin)
                    {
                        pps.Add(pp[i]);
                    }
                }

            }
            return pps;
        }

        public List<Coordenada> RemovePtsDistMin_Y(List<Coordenada> pp, double min)
        {
            pp = pp.OrderBy(x => x.Y).ToList();
            List<Coordenada> pps = new List<Coordenada>();
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

        [Browsable(false)]
        public string tipo_desenho { get; set; } = "";

        [Browsable(false)]
        public bool cotar_embaixo { get; set; } = false;
        [Browsable(false)]
        public bool acumuladas_cima { get; set; } = true;
        [Browsable(false)]
        public bool acumuladas_baixo { get; set; } = false;
        [Browsable(false)]
        public bool cotar_emcima { get; set; } = true;
        [Browsable(false)]
        public bool cotar_direita { get; set; } = false;
        [Browsable(false)]
        public bool cotar_esquerda { get; set; } = true;
        [Browsable(false)]
        public bool base_esquerda { get; set; } = false;
        [Browsable(false)]
        public bool base_direita { get; set; } = false;

        public void Configurar()
        {
            var opt = PerguntaString("Selecione o tipo de configuração", new List<string> {"Contorno", "Dimensoes" });
 
            if(opt.StartsWith("C"))
            {
                ConfigurarContorno();
            }
            else if(opt.StartsWith("D"))
            {
                ConfigurarDesenho();
            }


        }
        private void ConfigurarDesenho()
        {
            dist0 = PergundaDouble("Digite a dist0", dist0);
            dist1 = PergundaDouble("Digite a dist1", dist1);
            dist2 = PergundaDouble("Digite a espessura", dist2);
            profundidade_esquerda = PergundaDouble("Digite a profundidade_corte", profundidade_esquerda);
            profundidade_direita = PergundaDouble("Digite a profundidade_corte", profundidade_direita);
        }
        public void ConfigurarContorno()
        {
            concavidade_contorno = PergundaDouble("Digite a concavidade", concavidade_contorno);
            escala_contorno = PerguntaInteger("Digite a escala", escala_contorno);
            espessura_contorno = PergundaDouble("Digite a espessura", espessura_contorno);
        }
        public void SetWidthFactor(double valor)
        {


            Comando(
                    "_-style" ,
                    estilo_padrao ,
                    estilo_padrao_fonte,
                    "0" ,
                    widthfactor ,
                    "0" ,
                    "No" ,
                    "No" ,
                    "No"
                );
            //using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            //{
            //    // Open the current text style for write
            //    TextStyleTableRecord acTextStyleTblRec;
                
            //    acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle,
            //                                          OpenMode.ForWrite) as TextStyleTableRecord;

                
            //    var s = acTextStyleTblRec.Name;
            //    if(s.ToUpper()=="ROMANS")
            //    {
            //        // Save the changes and dispose of the transaction
            //        acTextStyleTblRec.XScale = valor;

            //        acTrans.Commit();
            //        acDoc.Editor.Regen();
            //    }
            //}
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
            AddMensagem("\nDist0: " + dist0);
            AddMensagem("\nDist1: " + dist1);
            AddMensagem("\nDist2: " + dist2);
            AddMensagem("\nFuros na Vista: " + Getpts_furos_vista().Count);
            AddMensagem("\nFuros em corte: " + GetFuros_corte().Count);
            AddMensagem("\nFuros em corte sentido horizontal: " + Getpts_furos_corte_verticais().Count);
            AddMensagem("\nFuros em corte sentido vertical: " + Getpts_furos_corte_horizontais().Count);
            AddMensagem("\nCoordenadas de Bordas:");
            AddMensagem("\nSE: " + se);
            AddMensagem("\nSD: " + sd);
            AddMensagem("\nIE: " + ie);
            AddMensagem("\nID: " + id);
            AddMensagem("\nCENTRO: " + centro);
            AddBarra();
            AddMensagem("\nOffset Centro: " + offset_centro);
            AddMensagem("\nAltura: " + altura);
            AddMensagem("\nLargura: " + largura);
            AddBarra();
            AddMensagem("\nLinhas:" + GetLinhas().Count);
            AddMensagem("\nLinhas de perfil:" + Getlinhas_perfil().Count);
            AddMensagem("\nProjeções:" + Getlinhas_projecao().Count);
            AddBarra();
        }

        [Browsable(false)]
        private double DistMinTexto_X
        {
            get
            {
                return this.GetEscala() * 4.2 * this.widthfactor ;
            }
        }

        [XmlIgnore]
        private bool cota_horizontal_superior { get; set; } = true;
        [XmlIgnore]
        private bool cota_anterior_movida { get; set; } = false;
        [XmlIgnore]
        private bool ultima_cota { get; set; } = false;
        [XmlIgnore]
        private int sequencia { get; set; } = 0;
        [XmlIgnore]
        private int max_sequencia { get; set; } = 0;
        [XmlIgnore]
        private int cotas_movidas_contagem { get; set; } = 0;
        private void AddCotasHorizontais(List<Coordenada> pp, double y, bool superior = true)
        {
            if(pp.Count<2)
            {
                AddMensagem("Cotagem Horizontal abortada - Lista contém apenas " + pp.Count + " coordenadas");
                return;
            }
            sequencia = 0;
            cotas_movidas_contagem = 0;
            cota_anterior_movida = false;
            ultima_cota = false;
            this.cota_horizontal_superior = superior;
            max_sequencia = pp.Count-2;
            if (superior)
            {
               
                for (int i = 0; i < pp.Count-1; i++)
                {


                    sequencia = i;
                    var dist = Math.Abs(pp[i + 1].X - pp[i].X);
                    if(pp[i].PegarIguaisX().Count>0 && juntar_cotas)
                    {
                        AddCotaHorizontal(pp[i], pp[i + pp[i].PegarIguaisX().Count+1], offset1 + y);
                        i = i + pp[i].PegarIguaisX().Count;
                    }
                    else if (i == 0)
                    {
                        //cota da esquerda pra direita, ajustanto automaticamente a primeira cota, se a mesma for pequena
                        AddCotaHorizontal(pp[i + 1], pp[i], offset1 + y);
                    }
                    else
                    {
                        AddCotaHorizontal(pp[i], pp[i + 1], offset1 + y);
                       
                    }
                }
                //cota de ponta a ponta
                ultima_cota = true;
                AddCotaHorizontal(pp[0], pp[pp.Count - 1], offset0 + y + (cotas_movidas_contagem>0?(offset0-offset1):0));
            }
            else
            {
                //cotar em baixo
                for (int i = 0; i < pp.Count-1; i++)
                {

                    sequencia = i;
                    if (pp[i].PegarIguaisX().Count > 0 && juntar_cotas)
                    {
                        AddCotaHorizontal(pp[i], pp[i + pp[i].PegarIguaisX().Count+1], -offset1 + y);
                        i = i + pp[i].PegarIguaisX().Count;
                    }
                    else if(i == 0)
                    {
                        //cota da esquerda pra direita, ajustanto automaticamente a primeira cota, se a mesma for pequena
                        AddCotaHorizontal(pp[i +1], pp[i], -offset1 + y);
                    }
                    else
                    {
                        AddCotaHorizontal(pp[i], pp[i+1], -offset1 + y);
                    }
                }
                //cota de ponta a ponta
                ultima_cota = true;
                AddCotaHorizontal(pp[0], pp[pp.Count - 1], -offset0 + y - (cotas_movidas_contagem > 0 ? (offset0 - offset1) : 0));
            }

        }
        private void AddCotasAcumuladas(List<Coordenada> pp, double y, Point3d origem, bool superior = true)
        {
            cotas_movidas_contagem = 0;
            sequencia = 0;
            cota_anterior_movida = false;
            ultima_cota = false;
            max_sequencia = pp.Count-2;
            ordinate_anterior = origem.X;

            var coordy2 = offset2 + y;
            if(!superior)
            {
                coordy2 = -offset2 + y;
            }
            var coordy1 = offset1 + y;
            if (!superior)
            {
                coordy1 = -offset1 + y;
            }

            //0 - Sem chapa
            if (!base_esquerda && !base_direita && pp.Count > 2)
            {
                for (int i = 0; i < pp.Count; i++)
                {
                    sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new Point3d(pp[i].X, coordy2, 0));

                }
            }

            //1 - Só na esquerda
            else if (base_esquerda && !base_direita && pp.Count > 2)
            {
                origem = new Point3d(pp[1].X, y, 0);
                for (int i = 1; i < pp.Count; i++)
                {
                    sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new Point3d(pp[i].X, coordy2, 0));
                }
            }

            //2 - Só na direita
            else if (!base_esquerda && base_direita && pp.Count > 3)
            {
                for (int i = 0; i < pp.Count - 1; i++)
                {
                    sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new Point3d(pp[i].X, coordy2, 0));
                }
            }

            //3 - Ambos os lados
            else if (base_direita && base_esquerda && pp.Count > 3)
            {
                origem = new Point3d(pp[1].X, y, 0);
                for (int i = 1; i < pp.Count - 1; i++)
                {
                    sequencia = i;
                    AddCotaOrdinate(origem, pp[i], new Point3d(pp[i].X, coordy2, 0));
                }
            }


        }
        private void AddCotasVerticais(List<Coordenada> pp, double xmin, double xmin2)
        {
            cotas_movidas_contagem = 0;
            sequencia = 0;
            cota_anterior_movida = false;
            ultima_cota = false;
            max_sequencia = pp.Count - 2;
            if (pp.Count > 1)
            {
                for (int i = 0; i < pp.Count-1; i++)
                {

                    sequencia = i;
                    var distancia = pp[i+1].Y - pp[i].Y;
                    if (pp[i].PegarIguaisY().Count > 0 && juntar_cotas)
                    {
                        AddBarra();
                        AddMensagem("\n" + pp[i] + "\n");
                        AddMensagem("\nCotas Iguais:\n");
                        AddMensagem(string.Join("\n", pp[i].PegarIguaisY()));
                        AddCotaVertical(pp[i + pp[i].PegarIguaisY().Count+1], pp[i], xmin);
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
                ultima_cota = true;
                AddCotaVertical(pp[0], pp[pp.Count - 1], xmin2);

            }
        }

        public void AddMLeader(Point3d origem, Point3d pt2, string texto)
        {
          

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForWrite) as BlockTableRecord;



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
            if(cotar_esquerda)
            {
               
                try
                {
                    var pts = GetPts_lado_esquerdo().OrderBy(y => y.Y).ToList();
                    AddMensagem("\nCoordenadas lado esquerdo:\n");
                    AddMensagem(string.Join("\n", pts.Select(x=>x.ToString())));
                    if (pts.Count > 0)
                    {
                        var xmin = pts.Min(y => y.X) - offset1;
                        var xmin2 = pts.Min(y => y.X) - offset0;
               
                        AddCotasVerticais(pts, xmin, xmin2);
                    }
                    AddBarra();

                }
                catch (System.Exception ex)
                {
                    AddBarra();
                    AddMensagem("\n Erro");
                    AddBarra();
                    AddMensagem("\n" + ex.Message);
                    AddMensagem("\n" + ex.StackTrace);
                    AddBarra();
                }

            }

            if(cotar_direita)
            {
                try
                {
                    var pts = GetPts_lado_direito().OrderBy(y => y.Y).ToList();
                    AddMensagem("\nCoordenadas lado direito:\n");
                    AddMensagem(string.Join("\n", pts));
                    if (pts.Count > 0)
                    {
                        var xmin = pts.Max(y => y.X) + offset1;
                        var xmin2 = pts.Max(y => y.X) + offset0;
                        
                        AddCotasVerticais(pts, xmin, xmin2);
                    }
                }
                catch (System.Exception ex)
                {
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

        public void AddCotaVertical(Coordenada inicio, Coordenada fim, double x, string texto = "")
        {
            RotatedDimension acRotDim;
            // AddMensagem("\nCota vertical: p1:" + inicio + " p2:" + fim);
            double dist = Math.Abs(fim.Y - inicio.Y);
            //if (dist == 0)
            //{
            //    return;
            //}
            double y = (inicio.Y > fim.Y ? inicio.Y : fim.Y) - (dist / 2);
            Point3d posicao = new Point3d(x, y, 0);


            bool dimtix = false;

            if (sequencia == 0 | sequencia == max_sequencia)
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
                tam = size;
            }

            acRotDim = AddCotaVertical(inicio, fim, texto, posicao, dimtix, tam,juntar_cotas,ultima_cota);
        }
        public void AddCotaHorizontal(Coordenada inicio, Coordenada fim, double y, string texto = "")
        {
            RotatedDimension acRotDim;
            //AddMensagem("\nCota horizontal: p1:" + inicio + " p2:" + fim);
            double dist = Math.Abs(fim.X - inicio.X);
            //if (dist == 0)
            //{
            //    return null;
            //}
            var posicao = new Point3d((inicio.X > fim.X ? inicio.X : fim.X) - (dist / 2), y, 0);
            bool movida = false;
            if (dist < DistMinTexto_X && sequencia > 0 && sequencia != max_sequencia)
            {
                if (cota_anterior_movida)
                {
                    cota_anterior_movida = false;
                }
                else
                {
                    cotas_movidas_contagem++;
                    movida = true;
                    posicao = new Point3d(posicao.X, posicao.Y + (this.cota_horizontal_superior ? (offset0 - offset1) : -(offset0 - offset1)), posicao.Z);
                }
            }

            bool dimtix = false;
            if (sequencia == 0 | sequencia == max_sequencia | movida)
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
                tam = size;

            }

            acRotDim = AddCotaHorizontal(inicio, fim, texto, posicao, dimtix, tam,juntar_cotas,ultima_cota);
        }
        public void AddCotaOrdinate(Point3d pontozero, Coordenada ponto, Point3d posicao)
        {
            double tam = 0;

            //tam_texto
            if (ForcarTamTexto)
            {
                tam = size;

            }
            double dist = Math.Abs(ponto.X - ordinate_anterior);


            if (sequencia > 0 && dist < (GetEscala() * 2.1) && sequencia != max_sequencia)
            {
                if (cota_anterior_movida)
                {
                    cota_anterior_movida = false;
                }
                else
                {
                    posicao = new Point3d(posicao.X + (2.2 * GetEscala()), posicao.Y, posicao.Z);
                    cota_anterior_movida = true;
                }
            }

            AddCotaOrdinate(pontozero, ponto, posicao, tam);
            ordinate_anterior = ponto.X;
        }


        public TextStyleTableRecord GetStyle(string nome)
        {
            TextStyleTableRecord ret = null;
            Database acCurDb = HostApplicationServices.WorkingDatabase;
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                SymbolTable symTable = (SymbolTable)acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForRead);
                foreach (ObjectId id in symTable)
                {
                    TextStyleTableRecord symbol = (TextStyleTableRecord)acTrans.GetObject(id, OpenMode.ForRead);

                  if(symbol.Name.ToUpper() == nome)
                    {
                        ret = symbol;
                    }
                    
                }

                acTrans.Commit();
            }
            return ret;
        }

        private double ordinate_anterior { get; set; } = 0;


        /// <summary>
        /// Esse cara habilita a opção das cotas não serem jogadas pro lado quando a distância for muito pequena
        /// </summary>
        public void SetDimtix(bool valor, double size)
        {
            // Get the current database
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;



                DimStyleTableRecord acDimStyleTbl = acTrans.GetObject(acCurDb.Dimstyle, OpenMode.ForWrite) as DimStyleTableRecord;
                if(acDimStyleTbl!=null) 
                {
                    acDimStyleTbl.Dimtix = valor;
                    //tam_texto
                    if(ForcarTamTexto)
                    {
                    acDimStyleTbl.Dimtxt = size;

                    }
                    acTrans.Commit();
                    acDoc.Editor.Regen();
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
            if(selecao.Status != PromptStatus.OK)
            {
                return "";
            }

            //limpa as cotas atuais
            Ut.Apagar(this.GetCotas().FindAll(x=> !(x is Leader) && !(x is MLeader) && !(x is DBText) && !(x is MText)));

                if (GetLinhas().Count == 0 | selecao.Status != PromptStatus.OK)
                {
                    AddMensagem("\nNenhuma linha encontrada na seleção.\nÉ necessário selecionar uma peça de TecnoMetal.\nExploda a seleção antes de tentar cotar.");
                    return "";
                }


                //escala = acCurDb.Dimscale;
                size = acCurDb.Dimtxt;
                acCurDb.Dimtfill = 1;
                if (ForcarTamTexto)
                {
                    acCurDb.Textsize = size;

                }
                acCurDb.Dimtix = true;





                MapeiaObjetos();

                if (marcas_ajusta_escala)
                {
                    Blocos.SetEscala(this.GetBlocos_Marcas_Posicoes(),this.GetEscala());
                }


                if (GetContorno().Count == 0)
                {
                    AddMensagem("Região selecionada não contém objetos cotáveis do TecnoMetal. \nExploda as vistas para poder usar a ferramenta.");
                    return "Nada";
                }
       

                List<Coordenada> pp = new List<Coordenada>();

                Point3d origem;
                double y;
                //cotas horizotais superiores

                GetPontosHorizontais(out pp, out y, out origem, true);


                if (cotar_emcima)
                {
                    AddCotasHorizontais(pp, y);
                }

                //cotas acumuladas
                if (acumuladas_cima && !tipo_desenho.StartsWith("C"))
                {
                    AddCotasAcumuladas(pp, y, origem);
                }

                //cotas horizotais inferiores
                GetPontosHorizontais(out pp, out y, out origem, false);

                if (cotar_embaixo)
                {
                    AddCotasHorizontais(pp, y, false);
                }

                if (acumuladas_baixo && !tipo_desenho.StartsWith("C"))
                {
                    AddCotasAcumuladas(pp, y, origem, false);
                }

                //cotas dos cantos
                AddCotasVerticaisCantos();

                if (linha_projecao && !tipo_desenho.StartsWith("C"))
                {
                    foreach (var s in this.GetCoordenadasFurosProjecao())
                    {
                        AddLinha(s.Origem(), s.Fim(), "DASHDOT", System.Drawing.Color.Yellow);
                    }
                }

                if (indicar_diametro && !tipo_desenho.StartsWith("C"))
                {
                    foreach (var s in this.GetFurosPorDiam())
                    {
                        Ut.AddLeader(s.Origem().GetPoint2d(), s.Origem().Mover(offset1, -offset1 / 2).GetPoint2d(), s.Nome, size * this.GetEscala());
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
            if (tipo_desenho == "") { return; }




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
                    var pp = this.PerguntaString("Deseja mudar as opções e continuar?", new List<string> { "Sim", "Não" });
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

                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
    }



}
