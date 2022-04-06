using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.cad
{
    internal static class CAD
    {
        public static DocumentCollection documentManager
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            }
        }
        public static Document acDoc
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            }
        }
        public static Editor editor
        {
            get
            {
                return acDoc.Editor;
            }
        }
        public static Database acCurDb
        {
            get
            {
                return acDoc.Database;
            }
        }
        public static dynamic acadApp
        {
            get
            {
                dynamic acadApp = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
                return acadApp;
            }
        }
    }
    public static class Constantes
    {
        private static List<CTV_de_para> _cts { get; set; }


        public static List<CTV_de_para> CTVs()
        {
            if (_cts == null)
            {
                _cts = new List<CTV_de_para>();
                var arq = DLM.vars.CADVars.Arquivo_CTV;
                var linhas = Conexoes.Utilz.Arquivo.Ler(arq);
                if (linhas.Count > 1)
                {
                    for (int i = 1; i < linhas.Count; i++)
                    {
                        var col = linhas[i].Split(';').ToList();
                        if (col.Count >= 5)
                        {
                            _cts.Add(
                                new CTV_de_para(
                                col[0],
                                col[2],
                                Conexoes.Utilz.Double(col[1]),
                                Conexoes.Utilz.Int(col[3]),
                                col[4]
                                )
                                );
                        }
                    }
                }
            }
            return _cts;
        }

        private static EstilosML _estilosML { get; set; }
        public static EstilosML GetArquivosMlStyles()
        {
            if (_estilosML == null)
            {
                _estilosML = new EstilosML();
            }
            return _estilosML;
        }
    }

}
