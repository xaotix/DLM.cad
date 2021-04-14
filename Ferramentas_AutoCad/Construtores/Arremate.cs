using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DLMCam;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public class Arremate : ClasseBase
    {
        public Arremate()
        {

        }

        public void Mapear()
        {
            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {
                var sel = SelecionarObjetos(acTrans);
                if (sel.Status == PromptStatus.OK)
                {
                    var pols = this.Getpolylinhas();
                    if (pols.Count>0)
                    {

                      
                        var pl = pols[0];

                        List<LineSegment3d> segmentos = new List<LineSegment3d>();
                        var angulos = Utilidades.GetAngulos(pl, out segmentos);

                       


                        

                        foreach (var s in angulos)
                        {
                            AddMensagem($"\nAngulo:" + s);
                        }


                        double corte = Math.Round(pl.Length);



                        bool status = false;
                        Perfil_Arremate pa = Conexoes.Utilz.Propriedades(new Perfil_Arremate() { Dobras = angulos.Count },out status);
                        if(status)
                        {

                           

                            if (pa.Comprimento > 0 && pa.Espessura > 0 && pa.Marca.Replace(" ", "") != "" && pa.Quantidade>0)
                            {
                                for (int i = 0; i < angulos.Count; i++)
                                {
                                    var ang = angulos[i];
                                    corte = corte - (2 * pa.Espessura);
                                }
                                bool cancelado = true;
                                var origem = Utilidades.PedirPonto3D("Selecione o ponto de inserção do bloco.",out cancelado);
                                if (!cancelado)
                                {
                                    Utilidades.InserirBlocoArremate(origem,pa.Marca, pa.Comprimento, corte, pa.Espessura, pa.Quantidade, pa.Material, pa.Esquema, angulos.Count);

                                    if(pa.GerarCam == Opcao.Sim)
                                    {
                                        string destino = this.Pasta;
                                        if(this.Pasta.EndsWith(".TEC"))
                                        {
                                            destino = Conexoes.Utilz.CriarPasta(Conexoes.Utilz.getUpdir(destino), "CAM");
                                        }
                                        if(Directory.Exists(destino))
                                        {
                                            DLMCam.Chapa pp = new DLMCam.Chapa(pa.Comprimento, corte, pa.Espessura);

                                            string arquivo = destino + pa.Marca + ".CAM";

                                            DLMCam.Cam pcam = new DLMCam.Cam(arquivo, pp);
                                            double x = 0;

                                            for (int i = 0; i < angulos.Count; i++)
                                            {
                                                var s = segmentos[i];
                                                x = x + s.Length - pa.Espessura;   
                                                var a = angulos[i];
                                                pcam.Dobras.Liv1.Add(new DLMCam.Estrutura.Dobra(a,x,pcam,false));
                                            }

                                            pcam.Cabecalho.TRA_PEZ = pa.Esquema;
                                            pcam.Cabecalho.Quantidade = pa.Quantidade;
                                            pcam.Cabecalho.Material = pa.Material;
                                            pcam.Cabecalho.Marca = pa.Marca;
                                            pcam.Nota = "PARA DOBRAS = SEGUIR DESENHO DA PRANCHA DE FABRICAÇÃO.";
                                            pcam.GerareVisualizar();
                                            Conexoes.Utilz.Abrir(destino);

                                        }
                                    }
                                }

                            }
                        }

                    }
                }
            }
        }
    }

    public class Perfil_Arremate
    {
        [ReadOnly(true)]
        public int Dobras { get; set; } = 0;

        public int Quantidade { get; set; } = 1;
        public double Espessura { get; set; } = 1.25;
        public double Comprimento { get; set; } = 6000;
        public string Marca { get; set; } = "ARR-1";
        public string Material { get; set; } = "PP ZINC";
        public string Esquema { get; set; } = "SEM PINTURA";
        public Opcao GerarCam { get; set; } = Opcao.Sim;
        public Perfil_Arremate()
        {

        }
    }
    public enum Opcao
    {
        Não,
        Sim,
    }
}
