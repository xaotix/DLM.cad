using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
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
                        var ang = Utilidades.GetAngulos(pl);

                        var segs = Utilidades.GetSegmentos(pl);

                       

                        foreach (var s in ang)
                        {
                            AddMensagem($"\nAngulo:" + s);
                        }

                        var comprimento = this.PergundaDouble("Digite o comprimento", 1200);
                        if(comprimento>0)
                        {
                        var espessura = this.PergundaDouble("Digite a espessura", 1.25);
                            if (espessura > 0)
                            {
                                var nome = this.PerguntaString("Digite a Marca", new List<string>());

                                if (nome.Replace(" ", "") != "")
                                {
                                    bool cancelado = false;

                                    var origem = Utilidades.PedirPonto3D("Selecione a origem", out cancelado);
                                    if (!cancelado)
                                    {
                                        Utilidades.InserirBlocoArremate(origem, nome, comprimento, pl.Length, espessura, 1, "CIVIL 350", "SEM PINTURA", 2, 2);
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
