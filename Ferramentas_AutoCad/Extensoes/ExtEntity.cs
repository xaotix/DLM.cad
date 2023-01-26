using Autodesk.AutoCAD.DatabaseServices;
using Conexoes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cad
{
    public static class ExtEntity
    {

        public static void AjustarLayer(this Entity obj, OpenCloseTransaction acTrans)
        {
            try
            {
                var bylayer = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByLayer, 256);
                var item = (Entity)acTrans.GetObject(obj.ObjectId, OpenMode.ForWrite);

                var color = item.Color;
                var layer = item.GetLayer(acTrans);
                var linetype = item.GetLineType(acTrans);

                if (!item.Visible)
                {
                    return;
                }


                if (layer.Name.ToUpper() == "DEFPOINT" | layer.Name.ToUpper() == "DEFPOINTS" | layer.Name.ToUpper() == "MV")
                {
                    return;
                }

                if (item.IsDimmension())
                {
                    if (item.IsText())
                    {
                        item.Layer = "TEXTO";
                    }
                    else
                    {
                        item.Layer = "COTAS";
                    }
                }
                else if (item.Is<BlockReference>())
                {
                    item.Layer = "BLOCOS";
                }
                else if (item.Is<Viewport>())
                {
                    var vp = item.As<Viewport>();
                    vp.Locked = true;
                    item.Layer = "MV";
                }
                else
                {
                    if (color.ColorNameForDisplay.ToUpper() == "BYLAYER")
                    {
                        color = layer.Color;
                    }
                    if (linetype.Name.ToUpper() == "BYLAYER" | linetype.Name.ToUpper() == "BYBLOCK")
                    {
                        linetype = layer.GetLineType(acTrans);
                    }

                    var lname = linetype.Name.ToUpper();

                    if (lname.Contains("CONTINUOUS") | lname.Contains("HIDDEN") | lname.Contains("DASHED"))
                    {
                        var nlname = (lname.Contains("HIDDEN") | lname.Contains("DASHED")) ? "PROJECAO" : "CONTORNO";
                        if (color.Is(System.Drawing.Color.Yellow))
                        {
                            item.Layer = $"{nlname}1";
                        }
                        /*o windows não usa o verde total (0,255,0) no Green*/
                        else if (color.Is(System.Drawing.Color.Lime))
                        {
                            item.Layer = $"{nlname}2";
                        }
                        else if (color.Is(System.Drawing.Color.Red))
                        {
                            item.Layer = $"{nlname}3";
                        }
                        else if (color.Is(System.Drawing.Color.Blue))
                        {
                            item.Layer = $"{nlname}4";
                        }
                        else if (color.Is(System.Drawing.Color.Cyan))
                        {
                            item.Layer = $"{nlname}5";
                        }
                        else if (color.Is(System.Drawing.Color.Magenta))
                        {
                            item.Layer = $"{nlname}6";
                        }
                        else if (color.Is(System.Drawing.Color.White))
                        {
                            if (lname == "HIDDEN")
                            {
                                item.Layer = $"{nlname}2";
                            }
                            else
                            {
                                item.Layer = $"0";
                            }
                        }
                        else
                        {

                        }
                    }
                    else if (linetype.Name.ToUpper().Contains("HIDDEN"))
                    {
                        item.Layer = "PROJECAO";
                        item.Color = bylayer;
                    }
                    else if (linetype.Name.ToUpper().Contains("DASHDOT"))
                    {
                        item.Layer = "EIXOS";
                        item.Color = bylayer;
                    }
                    else
                    {

                    }
                }

                item.Color = bylayer;

            }
            catch (Exception ex)
            {

            }


        }
        public static LayerTableRecord GetLayer(this Entity entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LayerTableRecord layer = (LayerTableRecord)acTrans.GetObject(entity.LayerId, openMode);
            return layer;
        }
        public static LinetypeTableRecord GetLineType(this Entity entity, OpenCloseTransaction acTrans, OpenMode openMode = OpenMode.ForRead)
        {
            LinetypeTableRecord layer = (LinetypeTableRecord)acTrans.GetObject(entity.LinetypeId, openMode);
            return layer;
        }
        public static List<T> Filter<T>(this List<Entity> Items)
        {
            List<T> List = new List<T>();
            List.AddRange(Items.FindAll(x => x is T).Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList());
            return List;
        }
        public static bool IsDimmension(this Entity x)
        {
            return x is AlignedDimension
                                | x is ArcDimension
                                | x is Dimension
                                | x is DiametricDimension
                                | x is LineAngularDimension2
                                | x is Point3AngularDimension
                                | x is OrdinateDimension
                                | x is RadialDimension
                                | x is Leader
                                | x is MLeader
                                | x is MText
                                | x is DBText
                                ;
        }
        public static bool IsText(this Entity x)
        {
            return x is DBText
                                | x is MText;
        }
        public static List<Entity> GetDimmensions(this List<Entity> List)
        {
            return List.FindAll(x => x.IsDimmension());
        }
    }
}
