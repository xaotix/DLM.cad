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
        /// <summary>
        /// Representa o arquivo que está aberto
        /// </summary>
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


}
