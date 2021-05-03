using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Autodesk.AutoeditorInput
{
    public static class Extensoes
    {
        static MethodInfo runCommand = typeof(Editor).GetMethod(
           "RunCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static PromptStatus Command(this Editor editor, params object[] args)
        {
            if (Application.DocumentManager.IsApplicationContext)
                throw new InvalidOperationException("Invalid execution context for Command()");
            if (editor.Document != Application.DocumentManager.MdiActiveDocument)
                throw new InvalidOperationException("Document is not active");
            return (PromptStatus)runCommand.Invoke(editor, new object[] { args });
        }
    }
}
