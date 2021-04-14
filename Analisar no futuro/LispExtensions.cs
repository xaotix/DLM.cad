using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    //a ideia pra isso seria usar pra manipular lisps. preciso estudar melhor
    public class LispExtensions
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", EntryPoint = "acedInvoke",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        extern static private int acedInvoke(IntPtr args, out IntPtr result);

        /// <summary>
        /// Invoke a LISP function.
        /// The LISP function must be defined as an external subroutine using the c: prefix or invoking vl-acad-defun.
        /// This is no more mandatory since A2011 as the managed Application.Invoke() method wraps acedInvoke.
        /// </summary>
        /// <param name="args">The function name (string) following by the function arguments.</param>
        /// <returns>The LISP function return value or null if failed.</returns>
        public static ResultBuffer InvokeLisp(ResultBuffer args)
        {
            IntPtr ip = IntPtr.Zero;
            int status = acedInvoke(args.UnmanagedObject, out ip);
            if (status == (int)PromptStatus.OK && ip != IntPtr.Zero)
                return ResultBuffer.Create(ip, true);
            return null;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", EntryPoint = "acedPutSym",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        extern static private int acedPutSym(string args, IntPtr result);

        /// <summary>
        /// Set a LISP variable value.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="rb">The variable value</param>
        public static void SetLispSym(string name, ResultBuffer rb)
        {
            acedPutSym(name, rb.UnmanagedObject);
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", EntryPoint = "acedGetSym",
            CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        extern static private int acedGetSym(string args, out IntPtr result);

        /// <summary>
        /// Get a LISP variable value.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <returns>The variable value or null if failed.</returns>
        public static ResultBuffer GetLispSym(string name)
        {
            IntPtr ip = IntPtr.Zero;
            int status = acedGetSym(name, out ip);
            if (status == (int)PromptStatus.OK && ip != IntPtr.Zero)
            {
                return ResultBuffer.Create(ip, true);
            }
            return null;
        }
    }
}
