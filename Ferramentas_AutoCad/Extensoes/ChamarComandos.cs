//using System;
//using System.Security;
//using System.Runtime.InteropServices;
//using System.Collections.Generic;

//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.DatabaseServices;
//using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
//using DLM.cad;
//using Autodesk.AutoCAD.ApplicationServices;

//namespace Ferramentas_DLM.ChamarComandos
//{
//    [SuppressUnmanagedCodeSecurity]
//    public static class CommandLineHelper
//    {
//        private const string ACAD_EXE = "acad.exe";

//        private const short RTSTR = 5005;

//        private const short RTNORM = 5100;

//        private const short RTNONE = 5000;

//        private const short RTREAL = 5001;

//        private const short RT3DPOINT = 5009;

//        private const short RTLONG = 5010;

//        private const short RTSHORT = 5003;

//        private const short RTENAME = 5006;

//        private const short RTPOINT = 5002; /*2D point X and Y only */

//        private static Dictionary<Type, short> resTypes = new Dictionary<Type, short>();



//        static CommandLineHelper()
//        {
//            resTypes[typeof(string)] = RTSTR;
//            resTypes[typeof(double)] = RTREAL;
//            resTypes[typeof(Point3d)] = RT3DPOINT;
//            resTypes[typeof(ObjectId)] = RTENAME;
//            resTypes[typeof(Int32)] = RTLONG;
//            resTypes[typeof(Int16)] = RTSHORT;
//            resTypes[typeof(Point2d)] = RTPOINT;
//        }

//        private static TypedValue TypedValueFromObject(Object val)
//        {
//            if (val == null) throw new ArgumentException("null not permitted as command argument");
//            short code = -1;

//            if (resTypes.TryGetValue(val.GetType(), out code) && code > 0)
//            {
//                return new TypedValue(code, val);
//            }
//            throw new InvalidOperationException("Unsupported type in Command() method");
//        }

//        public static int Command(params object[] args)
//        {
//           return Command(CAD.acDoc, args);
//            //if (AcadApp.DocumentManager.IsApplicationContext) throw new InvalidCastException("Invalid execution context => IsApplicationContext");
//            //int stat = 0;
//            //int cnt = 0;
//            //using (ResultBuffer buffer = new ResultBuffer())
//            //{
//            //    foreach (object o in args)
//            //    {
//            //        buffer.Add(TypedValueFromObject(o));
//            //        ++cnt;
//            //    }

//            //    if (cnt > 0)
//            //    {
//            //        stat = acedCmd2012(buffer.UnmanagedObject);
//            //        //#if acad2012
//            //        //                    stat = acedCmd2012(buffer.UnmanagedObject);
//            //        //#endif
//            //        //#if acad2013
//            //        //                    stat = acedCmd2013( buffer.UnmanagedObject );
//            //        //#endif

//            //    }
//            //}
//            //return stat;
//        }




//        [System.Security.SuppressUnmanagedCodeSecurity]
//        [DllImport("accore.dll", EntryPoint = "acedCmd", CallingConvention = CallingConvention.Cdecl,
//            CharSet = CharSet.Auto)]
//        private static extern int acedCmd2013(IntPtr resbuf);

//        [System.Security.SuppressUnmanagedCodeSecurity]
//        [DllImport("acad.exe", EntryPoint = "acedCmd", CallingConvention = CallingConvention.Cdecl,
//            CharSet = CharSet.Auto)]
//        private static extern int acedCmd2012(IntPtr resbuf);

//        //public static void ExecuteStringOverInvoke(string command)
//        //{
//        //    try
//        //    {
//        //        //#if acad2012
//        //        //                object activeDocument =
//        //        //                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.AcadDocument;
//        //        //#endif

//        //        //#if acad2013
//        //        //                object activeDocument = Autodesk.AutoCAD.ApplicationServices.DocumentExtension.GetAcadDocument(
//        //        //                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument);
//        //        //#endif
//        //        object[] data = { command };
//        //        CAD.acDoc.GetType().InvokeMember("SendCommand", System.Reflection.BindingFlags.InvokeMethod, null, CAD.acDoc, data);
//        //    }
//        //    catch (Autodesk.AutoCAD.Runtime.Exception ex)
//        //    {
//        //        Conexoes.Utilz.Alerta(ex);
//        //    }
//        //}
//        public static int Command(Document doc,params object[] args)
//        {
//            try
//            {
//                //#if acad2012
//                //                object activeDocument =
//                //                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.AcadDocument;
//                //#endif

//                //#if acad2013
//                //                object activeDocument = Autodesk.AutoCAD.ApplicationServices.DocumentExtension.GetAcadDocument(
//                //                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument);
//                //#endif
//                object[] data = { string.Join("\n", args) };
//                doc.AcadDocument.GetType().InvokeMember("SendCommand", System.Reflection.BindingFlags.InvokeMethod, null, doc.AcadDocument, data);
//            }
//            catch (Autodesk.AutoCAD.Runtime.Exception ex)
//            {
//                Conexoes.Utilz.Alerta(ex);
//            }
//            return 1;
//        }
//    }
//}
