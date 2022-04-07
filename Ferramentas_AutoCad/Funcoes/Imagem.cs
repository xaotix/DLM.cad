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
using static DLM.cad.CAD;


namespace DLM.cad
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
            PromptFileNameResult res =CAD.editor.GetFileNameForOpen("Select file for which to generate previews" );
            if (res.Status != PromptStatus.OK)
                return;



            Document doc = null;
            try
            {
                doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Open(res.StringResult, false);
            }
            catch
            {
                editor.WriteMessage("\nUnable to read drawing.");
                return;
            }

            Database db = doc.Database;
            string path = Path.GetDirectoryName(res.StringResult),  name = Path.GetFileName(res.StringResult), iconPath = path + "\\" + name + " icons";
            int numIcons = 0;
            using (var acTrans = doc.TransactionManager.StartTransaction())
            {
                BlockTable table = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);

                foreach (ObjectId blkId in table)
                {
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject( blkId, OpenMode.ForRead);

                    // Ignore layouts and anonymous blocks
                    if (acBlkTblRec.IsLayout || acBlkTblRec.IsAnonymous)
                        continue;

                    // Attempt to generate an icon, where one doesn't exist
                    if (acBlkTblRec.PreviewIcon == null)
                    {
                        object ActiveDocument = doc.AcadDocument;
                        object[] data = { "_.BLOCKICON " + acBlkTblRec.Name + "\n" };
                        ActiveDocument.GetType().InvokeMember("SendCommand",System.Reflection.BindingFlags.InvokeMethod,null, ActiveDocument, data);
                    }

                    // Hopefully we now have an icon
                    if (acBlkTblRec.PreviewIcon != null)
                    {
                        // Create the output directory, if it isn't yet there
                        if (!Directory.Exists(iconPath))
                            Directory.CreateDirectory(iconPath);

                        // Save the icon to our out directory
                        acBlkTblRec.PreviewIcon.Save(iconPath + "\\" + acBlkTblRec.Name + ".bmp");

                        // Increment our icon counter
                        numIcons++;

                    }

                }

                acTrans.Commit();

            }
            doc.CloseAndDiscard();
        }
    }
}
