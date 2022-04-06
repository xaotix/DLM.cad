//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoeditorInput;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Interop;
//using System.Threading.Tasks;
//using System.Linq.Expressions;
//using DLM.cad;

//namespace Autodesk.AutoeditorInput
//{
//    public static class Extensoes
//    {
//        //static MethodInfo runCommand = typeof(Editor).GetMethod(
//        //   "RunCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

//        //public static PromptStatus Command(this Editor editor, params object[] args)
//        //{
//        //    if (Application.DocumentManager.IsApplicationContext)
//        //        throw new InvalidOperationException("Invalid execution context for Command()");
//        //    if (editor.Document != Application.DocumentManager.MdiActiveDocument)
//        //        throw new InvalidOperationException("Document is not active");

//        //    var s = (PromptStatus)runCommand.Invoke(editor, new object[] { args });

//        //    return s;
//        //}




//        //public static PromptStatus Command(this Editor editor, params object[] args)
//        //{
//        //    if (editor == null)
//        //        throw new ArgumentNullException("editor");
//        //    return runCommand(editor, args);
//        //}

//        //static Func<Editor, object[], PromptStatus> runCommand = GenerateRunCommand();

//        //static Func<Editor, object[], PromptStatus> GenerateRunCommand()
//        //{
//        //    MethodInfo method = typeof(Editor).GetMethod("RunCommand",
//        //       BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
//        //    ParameterExpression instance = Expression.Parameter(typeof(Editor), "editor");
//        //    ParameterExpression args = Expression.Parameter(typeof(object[]), "args");
//        //    return Expression.Lambda<Func<Editor, object[], PromptStatus>>(
//        //       Expression.Call(instance, method, args), instance, args)
//        //          .Compile();
//        //}


//        [CommandMethod("ChamarComando", CommandFlags.Session)]
//        public static void Command(this Editor editor, params object[] args)
//        {
//            Ferramentas_DLM.ChamarComandos.CommandLineHelper.Command(args);
//            //AcadApplication acadApp = (AcadApplication)Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
//            //var comando = string.Join("\n", args);
//            //editor.Document.SendStringToExecute(comando, true,false,true);

//            //acadApp.ActiveDocument.SendCommand(comando);
//        }
//    }
//}
