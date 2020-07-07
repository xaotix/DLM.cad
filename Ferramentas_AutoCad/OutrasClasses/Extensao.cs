using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

namespace Autodesk.AutoCAD.EditorInput
{
    public static class Extensoes
    {
        static MethodInfo runCommand = typeof(Editor).GetMethod(
           "RunCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static PromptStatus Command(this Editor ed, params object[] args)
        {
            if (Application.DocumentManager.IsApplicationContext)
                throw new InvalidOperationException("Invalid execution context for Command()");
            if (ed.Document != Application.DocumentManager.MdiActiveDocument)
                throw new InvalidOperationException("Document is not active");
            return (PromptStatus)runCommand.Invoke(ed, new object[] { args });
        }
    }
}
