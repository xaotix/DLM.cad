using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static Ferramentas_DLM.CAD;


namespace Ferramentas_DLM
{
   public static class Imagem
    {
        private static Dictionary<string, BitmapImage> imgs { get; set; } = new Dictionary<string, BitmapImage>();
        public static BitmapImage GetImagePreview(string arquivo)
        {

            var arq = imgs.ToList().Find(x => x.Key == arquivo.ToUpper());
            if (arq.Value!=null)
            {
                return arq.Value;
            }
           Document doc = null;

            try
            {
                using (Database acTmpDb = new Database(false, true))
                {
                    acTmpDb.ReadDwgFile(arquivo, FileOpenMode.OpenForReadAndAllShare, false, null);
                    var imagem = acTmpDb.ThumbnailBitmap;
                    var s = Conexoes.Utilz.BitmapParaImageSource(imagem);
                    imgs.Add(arquivo.ToUpper(), s);
                    acTmpDb.CloseInput(true);
                    return s;
                }

                        //doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(arquivo, false);
         
            
            }
            catch (Exception)
            {
                return null;
            }

            
         
        }
        public static void GenerateBlockPreviews()
        {
            Editor ed =Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptFileNameResult res =ed.GetFileNameForOpen("Select file for which to generate previews" );

            if (res.Status != PromptStatus.OK)

                return;



            Document doc = null;



            try

            {

                doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(res.StringResult, false);

            }

            catch

            {

                ed.WriteMessage("\nUnable to read drawing.");

                return;

            }



            Database db = doc.Database;



            string path = Path.GetDirectoryName(res.StringResult),

                   name = Path.GetFileName(res.StringResult),

                   iconPath = path + "\\" + name + " icons";



            int numIcons = 0;



            Transaction tr =

              doc.TransactionManager.StartTransaction();

            using (tr)

            {

                BlockTable table =

                  (BlockTable)tr.GetObject(

                    db.BlockTableId, OpenMode.ForRead

                  );



                foreach (ObjectId blkId in table)

                {

                    BlockTableRecord blk =

                      (BlockTableRecord)tr.GetObject(

                        blkId, OpenMode.ForRead

                      );



                    // Ignore layouts and anonymous blocks



                    if (blk.IsLayout || blk.IsAnonymous)

                        continue;



                    // Attempt to generate an icon, where one doesn't exist



                    if (blk.PreviewIcon == null)

                    {

                        object ActiveDocument = doc.AcadDocument;

                        object[] data = { "_.BLOCKICON " + blk.Name + "\n" };

                        ActiveDocument.GetType().InvokeMember(

                          "SendCommand",

                          System.Reflection.BindingFlags.InvokeMethod,

                          null, ActiveDocument, data

                        );

                    }



                    // Hopefully we now have an icon



                    if (blk.PreviewIcon != null)

                    {

                        // Create the output directory, if it isn't yet there



                        if (!Directory.Exists(iconPath))

                            Directory.CreateDirectory(iconPath);



                        // Save the icon to our out directory



                        blk.PreviewIcon.Save(

                           iconPath + "\\" + blk.Name + ".bmp"

                        );



                        // Increment our icon counter



                        numIcons++;

                    }

                }

                tr.Commit();

            }

            doc.CloseAndDiscard();


        }
    }
}
