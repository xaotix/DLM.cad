﻿using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DLM.cad
{
    public class ConfiguracaoChapa_Dobrada
    {
        private double _Peso_Unitario { get; set; } = 0;
        private double _Area { get; set; } = 0;
        private double _Superficie { get; set; } = 0;
        private string _Volume { get; set; } = "";
        private double _Corte { get; set; } = 0;



        [ReadOnly(true)]
        [Display(Order = 5, Name = "Esp.", GroupName = "Geometria")]
        public double Espessura { get; set; } = 1.25;

        [Display(Order = 5, Name = "Dobras", GroupName = "Geometria")]
        public int Dobras { get; set; } = 0;

        [ReadOnly(true)]
        [Display(Order = 5, Name = "Larg.", GroupName = "Geometria")]
        public double Largura
        {
            get
            {
                if(DescontarDobras)
                {
                    double corte = this._Corte;
                    for (int i = 0; i <Dobras; i++)
                    {
                 
                        corte = corte - (2 * Espessura);
                    }
                    corte = Math.Round(corte);

                    return corte;
                }
                else
                {
                    return _Corte;
                }
            }
        }

        [ReadOnly(true)]
        [Display(Order = 5, Name = "Peso Esp.", GroupName = "Geometria")]
        public double Peso_Especifico { get; set; } = 7.85;

        [Display(Order = 5, Name = "Peso Unit.", GroupName = "Geometria")]
        public double Peso_Unitario
        {
            get
            {
                if(_Peso_Unitario>0)
                {
                    return _Peso_Unitario;
                }
                return Math.Round(this.Espessura * this.Area * this.Peso_Especifico /1000/1000,Cfg.Init.CAD_DECIMAIS);
            }
        }

        public double Area
        {
            get
            {
                if(_Area>0)
                {
                    return _Area;
                }
                return Comprimento * Largura;

            }
        }

        [Display(Order = 5, Name = "Superfície", GroupName = "Geometria")]
        public double Superficie
        {
            get
            {
                if(_Superficie>0)
                {
                    return _Superficie;
                }

                double ret = (Area * 2) + (this.Espessura * Comprimento * 2) + (this.Espessura * Largura * 2);
                double sup = Math.Round(ret / 1000 / 1000 / 1000, Cfg.Init.CAD_DECIMAIS_SUP);
                return sup;
            }
        }
        [ReadOnly(true)]
        [Display(Order = 5, Name = "Comp.", GroupName = "Geometria")]
        public double Comprimento { get; set; } = 6000;
        [Display(Order = 5, Name = "Volume", GroupName = "Geometria")]
        public string Volume
        {
            get
            {
                if(_Volume!="")
                {
                    return _Volume;
                }
                return $"{this.Comprimento.ToString("N0").Replace(",", "")}*{this.Espessura.String(2).Replace(",", "")}*{this.Largura.ToString("N0").Replace(",", "")}";
            }
        }
        [Display(Order = 5, Name = "Descontar Dobras", GroupName = "Geometria")]
        public bool DescontarDobras { get; set; } = false;

        [Display(Order = -1, Name = "Qtd", GroupName = "Peça")]
        public int Quantidade { get; set; } = 1;

        [Display(Order = -1, Name = "Descrição", GroupName = "Peça")]
        public string Descricao { get; set; } = "";

        [Display(Order = -1, Name = "Mercadoria", GroupName = "Peça")]
        [ReadOnly(true)]
        public string Mercadoria { get; set; } = "ARREMATE";

        [Display(Order =-1, Name ="Marca",GroupName ="Peça")]
        [ReadOnly(true)]
        public string Marca { get; set; } = "ARR-1";

        [Display(Order = -1, Name = "Gerar CAM", GroupName = "Peça")]
        public Opcao GerarCam { get; set; } = Opcao.Sim;



        [Display(Order = 10, Name = "Material", GroupName = "Bobina")]
        [ReadOnly(true)]
        public string Material { get; set; } = "PP ZINC";


        [Display(Order = 10, Name = "Ficha", GroupName = "Bobina")]
        public string Ficha { get; set; } = Cfg.Init.RM_SEM_PINTURA;

        [Display(Order = 10, Name = "Cor", GroupName = "Bobina")]
        [ReadOnly(true)]
        public string Cor_1 { get; set; } = "";

        [Display(Order = 10, Name = "Cor", GroupName = "Bobina")]
        [ReadOnly(true)]
        public string Cor_2 { get; set; } = "";

        [Display(Order = 10, Name = "SAP", GroupName = "Bobina")]
        [ReadOnly(true)]
        public string SAP { get; set; } = "";

        public ConfiguracaoChapa_Dobrada(Conexoes.Bobina bobina, double corte, double comprimento, double area,List<double> dobras, string Descricao = "")
        {
            this._Corte = Math.Round(corte);
            this.Espessura = bobina.Espessura;
            this.Material = bobina.Material;
            this.Comprimento = comprimento;
            this.Cor_1 = bobina.Cor1Str;
            this.Cor_2 = bobina.Cor2Str;
            this.SAP = bobina.SAP;
            this.Dobras = dobras.Count;
            this.Peso_Especifico = bobina.Coeficiente_Aco;
            this._Area = area;
            this.Descricao = Descricao;
            
        }
        public ConfiguracaoChapa_Dobrada(Conexoes.Bobina bobina, double comprimento, double largura)
        {
            this._Corte = largura;
            this.Comprimento = comprimento;
            this.Espessura = bobina.Espessura;
            this.Material = bobina.Material;
            this.Cor_1 = bobina.Cor2Str;
            this.Cor_2 = bobina.Cor2Str;
            this.SAP = bobina.SAP;
            this.Peso_Especifico = bobina.Coeficiente_Aco;
           
            this.Comprimento = comprimento;
            this.Descricao = $"Ch #{this.Espessura.String(2)}x{this.Largura.ToString("N0")}x{this.Comprimento.ToString("N0")}";
        }
        public ConfiguracaoChapa_Dobrada(DLM.cam.ReadCAM CAM)
        {
            this.Comprimento = CAM.Comprimento;
            this.Espessura = CAM.Espessura;
            this.Marca = CAM.Marca;
            this.Material = CAM.Material;
            this.Mercadoria = CAM.Descricao;
            this.Quantidade = CAM.Quantidade;
            this.Ficha = CAM.Tratamento;
            this._Corte = CAM.Largura;
            this._Peso_Unitario = CAM.Peso;
            this._Superficie = CAM.Superficie;
            this._Area = CAM.Formato.LIV1.GetArea();
            this.Descricao = CAM.Descricao;
        }
       
    }
}
