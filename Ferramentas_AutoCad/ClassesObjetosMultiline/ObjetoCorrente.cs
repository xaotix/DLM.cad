﻿using Autodesk.AutoCAD.Geometry;
using Conexoes;
using DLM.desenho;
using System;
using System.ComponentModel;

namespace DLM.cad
{
    public class ObjetoCorrente :ObjetoMultiLineBase
    {
        public override string ToString()
        {
            return this.Nome;
        }


        [Category("Geometria")]
        public double Descontar { get; set; } = 20;
        [Category("Geometria")]
        public double EntrePurlin
        {
            get
            {
                if(PurlinEmCima!=null && PurlinEmBaixo!=null)
                {
                    return Math.Abs(Math.Round(this.Origem_Direita.Distancia(Origem_Esquerda)));
                }
                else
                {
                    return 0;
                }
            }
        }

        public ObjetoCorrente(CADMline multiline, P3d centro,  VaoObra vao, ObjetoPurlin purlin_cima, ObjetoPurlin purlin_baixo)
        {
            this.Grade = vao.Grade;

            this.Multiline = multiline;


            this.VaoObra = vao;
            this.Descontar = Core.GetCADPurlin().CorrenteDescontar;
            this.id_peca = Core.GetCADPurlin().id_corrente;
            this.Suporte = Core.GetCADPurlin().CorrenteSuporte;

            this.PurlinEmCima = purlin_cima;
            this.PurlinEmBaixo = purlin_baixo;


            var p1 = this.PurlinEmCima.Multiline.GetInterSeccao(this.Multiline.GetPLineDummy());
            var p2 = this.PurlinEmBaixo.Multiline.GetInterSeccao(this.Multiline.GetPLineDummy());

            if(p1.Count>0)
            {
                this.Origem_Direita = p1[0].P3d();
            }
            else
            {
                this.Origem_Direita = new P3d(centro.X, PurlinEmCima.Y);
            }
            if (p2.Count > 0)
            {
                this.Origem_Esquerda = p2[0].P3d();
            }
            else
            {
                this.Origem_Esquerda = new P3d(centro.X, PurlinEmBaixo.Y);
            }

            this.SetPeca(Core.GetCADPurlin().GetCorrentePadrao());


        }
    }



}
