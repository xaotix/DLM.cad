using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DLM.cad
{
    public partial class CriarMarcas : Form
    {
        public CriarMarcas()
        {
            InitializeComponent();



        }

        private void button1_Click(object sender, EventArgs e)
        {
            P3d p0 = new P3d();
            double escala = 10;
            double offset = 67;

            foreach (DataGridViewRow s in this.myDataGridView.Rows)
            {
                var perfil = s.Cells[3].Value = "";
            }
            int c = 0;
            foreach (DataGridViewRow s in this.myDataGridView.Rows)
            {
                try
                {
                    if (s.Cells.Count < 4)
                    {
                        continue;
                    }
                    var marca = s.Cells[0].Value.ToString();
                    var perfil = s.Cells[1].Value.ToString();
                    var comprimento = s.Cells[2].Value.ToString().Double();
                    var pf = DBases.GetdbPerfil().GetPerfilTecnoMetal(perfil);
                    if (marca == "")
                    {
                        s.Cells[3].Value = "Marca em Branco.";
                        continue;
                    }
                    if (perfil == "")
                    {
                        s.Cells[3].Value = "Perfil em Branco.";
                        continue;
                    }
                    if (comprimento <= 0)
                    {
                        s.Cells[3].Value = "Comprimento inválido.";
                        continue;
                    }
                    if (pf.Descricao != "")
                    {
                        Blocos.MarcaPerfil(p0, marca, comprimento, pf,1, Cfg.Init.Material_Estrutura, Cfg.Init.RM_SEM_PINTURA);

                        if (c == 5)
                        {
                            //sobe uma linha, criando as marcas acima
                            p0 = new P3d(0, p0.Y + (escala * offset / 2));
                            c = 0;
                        }

                        p0 = new P3d(
                            p0.X + (escala * offset),
                            p0.Y/* + (escala * offset)*/
                            );
                    }
                    else
                    {
                        s.Cells[3].Value = "Perfil não encontrado.";

                    }
                }
                catch (Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }
                c++;

            }

            Conexoes.Utilz.Alerta("Finalizado.","", System.Windows.MessageBoxImage.Information);
        }

        

        private void CriarCAMs_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Conexoes.Utilz.Excel.ColarExcel(this.myDataGridView);
        }
    }
}
