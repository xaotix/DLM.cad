using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DLM.cad.CAD;
using Autodesk.AutoCAD.EditorInput;
using System.Runtime.InteropServices;
using System.Windows;
using DLM.vars;

namespace DLM.cad
{
    public static class Blocos
    {
        ///// <summary>
        /////Pega o contorno de blocos já escalonando. 
        /////Suporta somente Polyline, Line e Circle
        ///// </summary>
        ///// <param name="s"></param>
        ///// <param name="tr"></param>
        ///// <returns></returns>
        
        //public static List<Point3d> GetContorno(BlockReference s, Transaction tr)
        //{
        //    List<Point3d> pts = new List<Point3d>();
        //    BlockTableRecord acBlkTblRec = (BlockTableRecord)tr.GetObject(s.BlockTableRecord, OpenMode.ForRead);
        //    foreach (ObjectId id in acBlkTblRec)
        //    {

        //        var obj = tr.GetObject(id, OpenMode.ForRead);
        //        if (obj is Line)
        //        {
        //            var tt = obj as Line;
        //            Point3d p1 = tt.StartPoint.TransformBy(s.BlockTransform);
        //            Point3d p2 = tt.EndPoint.TransformBy(s.BlockTransform);
        //            pts.Add(p1);
        //            pts.Add(p2);
        //        }
        //        else if (obj is Polyline)
        //        {
        //            var tt = obj as Polyline;
        //            int vn = tt.NumberOfVertices;
        //            for (int i = 0; i < vn; i++)
        //            {
        //                Point3d pt = tt.GetPoint3dAt(i).TransformBy(s.BlockTransform);
        //                pts.Add(pt);
        //            }
        //        }
        //        else if(obj is Circle)
        //        {
        //            var tt = obj as Circle;
        //            var center = tt.Center.TransformBy(s.BlockTransform);

        //            var raioy = tt.Radius * s.ScaleFactors.Y;
        //            var raiox = tt.Radius * s.ScaleFactors.X;


        //            /*centro*/
        //            pts.Add(new Point3d(center.X, center.Y, 0));

        //            /*bordas horizontais*/
        //            pts.Add(new Point3d(center.X + raiox, center.Y, 0));
        //            pts.Add(new Point3d(center.X - raiox, center.Y, 0));

        //            /*bordas verticais*/
        //            pts.Add(new Point3d(center.X , center.Y + raioy, 0));
        //            pts.Add(new Point3d(center.X , center.Y - raioy, 0));

        //        }
        //        else if(obj is Arc)
        //        {
        //            var tt = obj as Arc;
        //            var center = tt.Center.TransformBy(s.BlockTransform);
        //            var p1 = tt.StartPoint.TransformBy(s.BlockTransform);
        //            var p2 = tt.EndPoint.TransformBy(s.BlockTransform);
        //            pts.Add(p1);
        //            pts.Add(center);
        //            pts.Add(p2);

        //        }
        //    }
        //    return pts;
        //}


        public static void Inserir(Document acDoc, string nome, Point3d origem, double escala, double rotacao, Hashtable atributos)
        {
            Inserir(acDoc, nome, new Point2d(origem.X, origem.Y), escala, rotacao, atributos);
        }


        public static void Renomear(string nome_antigo, string novo_nome, bool auto_cont = true)
        {


            using (Transaction tr = CAD.acCurDb.TransactionManager.StartTransaction())
            {
                string nome = novo_nome;
                var bt = (BlockTable)tr.GetObject(CAD.acCurDb.BlockTableId, OpenMode.ForRead);
                if (bt.Has(nome_antigo))
                {
                    if (auto_cont)
                    {
                        int c = 1;
                        while (bt.Has(nome))
                        {
                            nome = novo_nome + "_" + c;
                            c++;
                        }
                    }


                    if (!bt.Has(nome))
                    {
                        var btr = (BlockTableRecord)tr.GetObject(bt[nome_antigo], OpenMode.ForWrite);
                        btr.Name = nome;
                    }
                    else
                    {
                        Conexoes.Utilz.Alerta($"Já Existe um bloco com o nome {nome}.");
                    }

                }

                tr.Commit();
            }
        }


        public static void IndicacaoPeca(string Bloco, string CODIGO,double COMP, int ID,  Point2d origem,string DESC = "", double escala = 1, double rotacao = 0, string QTD = "1",  string DESTINO = "RME",  string N = "", string FAMILIA = "PECA", string TIPO = "PECA")
        {
            Hashtable ht = new Hashtable();
            ht.Add(CADVars.ATT_N, N);
            ht.Add(CADVars.ATT_Familia, FAMILIA);
            ht.Add(CADVars.ATT_Tipo, TIPO);
            ht.Add(CADVars.ATT_Comprimento, COMP);
            ht.Add("CODIGO", CODIGO);
            ht.Add(CADVars.ATT_id, ID);
            ht.Add(CADVars.ATT_Descricao, DESC);
            ht.Add(CADVars.ATT_Destino, DESTINO);
            ht.Add(CADVars.ATT_Quantidade, QTD);

            Inserir(CAD.acDoc, Bloco, origem, escala, rotacao, ht);
        }
        public static void Criar(string nome, List<Entity> Objetos, Point3d origem)
        {
            string nome_fim = nome;
            using (var tr = CAD.acCurDb.TransactionManager.StartTransaction())
            {
                // Get the block table from the drawing
                BlockTable bt = (BlockTable)tr.GetObject(CAD.acCurDb.BlockTableId,OpenMode.ForRead);

                int c = 1;
                while (bt.Has(nome_fim))
                {
                    nome_fim = nome + "_" + c;
                    c++;
                }
                
                //cria o bloco
                BlockTableRecord btr = new BlockTableRecord();
                btr.Name = nome_fim;
                bt.UpgradeOpen();

                ObjectId btrId = bt.Add(btr);
                tr.AddNewlyCreatedDBObject(btr, true);

                foreach (Entity ent in Objetos)
                {
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                }
                // Insere o bloco
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                BlockReference br = new BlockReference(origem, btrId);

                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);
                tr.Commit();
            }

        }
        public static void MarcaComposta(Point2d p0, string marca, double quantidade, string ficha, string mercadoria, double escala = 10)
        {
            try
            {

                Hashtable ht = new Hashtable();


                ht.Add(TAB_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(TAB_DBF1.QTA_PEZ.ToString(), quantidade);
                ht.Add(TAB_DBF1.TRA_PEZ.ToString(), ficha);
                ht.Add(TAB_DBF1.DES_PEZ.ToString(), mercadoria);

                Inserir(acDoc, CADVars.Marca_Composta, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public static void Inserir(Document acDoc, string nome, Point2d origem, double escala, double rotacao, Hashtable atributos)
        {
            string endereco = "";
            if (File.Exists(nome))
            {
                endereco = nome;
            }
            else
            {
                if (nome.Contains(@"\"))
                {
                    nome = Conexoes.Utilz.getNome(nome);
                }
                var s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos_TecnoMetal_Marcacao, nome + ".dwg");

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos_TecnoMetal_Simbologias, nome + ".dwg"); ;
                }

                if(s.Count==0)
                {
                s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos_Indicacao, nome + ".dwg");

                }

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos_Pcs, nome + ".dwg"); ;
                }

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos_A2, nome + ".dwg", SearchOption.AllDirectories);
                }


                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(CADVars.Raiz_Blocos, nome + ".dwg", SearchOption.TopDirectoryOnly);
                }
                if (s.Count == 0)
                {
                    Conexoes.Utilz.Alerta($"Bloco não encontrado:{nome}", "Operação abortada");
                    return;
                }
                else
                {
                    endereco = s[0];

                }
            }

            if (!File.Exists(endereco))
            {
                Conexoes.Utilz.Alerta($"Bloco não encontrado\n! {endereco}");
                return;
            }


            try
            {
                string nomeBloco = Conexoes.Utilz.getNome(endereco);




                ObjectId blkid = ObjectId.Null;
                using (var acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (acBlkTbl.Has(nomeBloco))
                    {
                        using (DocumentLock acLckDoc = acDoc.LockDocument())
                        {
                            //ed.WriteMessage("\nBloco já existe, adicionando atual...\n");

                            blkid = acBlkTbl[nomeBloco];
                            BlockReference bref = new BlockReference(new Point3d(origem.X, origem.Y,0), blkid);
                            BlockTableRecord btr2 = (BlockTableRecord)acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite);
                            using (BlockTableRecord bdef = (BlockTableRecord)acTrans.GetObject(bref.BlockTableRecord, OpenMode.ForWrite))
                            {
                                bref.ScaleFactors = new Scale3d(escala, escala, escala);
                                bref.Rotation = Conexoes.Utilz.GrausParaRadianos(rotacao);
                                bref.TransformBy(editor.CurrentUserCoordinateSystem);
                                bref.RecordGraphicsModified(true);
                                if (bdef.Annotative == AnnotativeStates.True)
                                {
                                    ObjectContextCollection contextCollection = acCurDb.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                                    Autodesk.AutoCAD.Internal.ObjectContexts.AddContext(bref, contextCollection.GetContext("1:1"));
                                }
                                btr2.AppendEntity(bref);
                                acTrans.AddNewlyCreatedDBObject(bref, true);

                                foreach (ObjectId eid in bdef)
                                {
                                    DBObject obj = (DBObject)acTrans.GetObject(eid, OpenMode.ForRead);
                                    if (obj is AttributeDefinition)
                                    {
                                        AttributeDefinition atdef = (AttributeDefinition)obj;
                                        AttributeReference atref = new AttributeReference();
                                        if (atdef != null)
                                        {
                                            atref = new AttributeReference();
                                            atref.SetAttributeFromBlock(atdef, bref.BlockTransform);
                                            //atref.Position = atdef.Position + bref.Position.GetAsVector();
                                            atref.Position = atdef.Position.TransformBy(bref.BlockTransform);
                                            if (atributos.ContainsKey(atdef.Tag.ToUpper()))
                                            {
                                                atref.TextString = atributos[atdef.Tag.ToUpper()].ToString();
                                            }
                                        }
                                        bref.AttributeCollection.AppendAttribute(atref);
                                        acTrans.AddNewlyCreatedDBObject(atref, true);
                                    }
                                }
                                bref.DowngradeOpen();
                            }

                            acTrans.TransactionManager.QueueForGraphicsFlush();
                            acDoc.TransactionManager.FlushGraphics();
                            acTrans.Commit();
                            //doc.Editor.Regen();

                            return;
                        }
                    }
                }

                using (DocumentLock loc = acDoc.LockDocument())
                {

                    using (Database AcTmpDB = new Database(false, true))
                    {
                        using (var acTrans = acDoc.TransactionManager.StartTransaction())
                        {
                            BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                            acBlkTbl.UpgradeOpen();

                            //se nao tem, ai ele vai tentar abrir e inserir
                            AcTmpDB.ReadDwgFile(endereco, System.IO.FileShare.Read, true, "");
                            blkid = acCurDb.Insert(endereco, AcTmpDB, true);

                            BlockTableRecord btrec = (BlockTableRecord)blkid.GetObject(OpenMode.ForRead);
                            btrec.UpgradeOpen();
                            btrec.Name = nomeBloco;
                            btrec.DowngradeOpen();



                            using (BlockTableRecord btr = (BlockTableRecord)acCurDb.CurrentSpaceId.GetObject(OpenMode.ForWrite))
                            {
                                using (BlockReference bref = new BlockReference(new Point3d(origem.X, origem.Y,0), blkid))
                                {
                                    Matrix3d mat = Matrix3d.Identity;
                                    bref.TransformBy(mat);
                                    bref.ScaleFactors = new Scale3d(escala, escala, escala);
                                    bref.Rotation = Conexoes.Utilz.GrausParaRadianos(rotacao);
                                    bref.Position = new Point3d(origem.X, origem.Y, 0);
                                    btr.AppendEntity(bref);
                                    acTrans.AddNewlyCreatedDBObject(bref, true);

                                    using (BlockTableRecord acBlkTblRec = (BlockTableRecord)bref.BlockTableRecord.GetObject(OpenMode.ForRead))
                                    {
                                        Autodesk.AutoCAD.DatabaseServices.AttributeCollection atcoll = bref.AttributeCollection;

                                        foreach (ObjectId subid in acBlkTblRec)
                                        {
                                            Entity ent = (Entity)subid.GetObject(OpenMode.ForRead);
                                            AttributeDefinition attDef = ent as AttributeDefinition;

                                            if (attDef != null)
                                            {
                                                // ed.WriteMessage("\nValue: " + attDef.TextString);
                                                AttributeReference attRef = new AttributeReference();
                                                attRef.SetPropertiesFrom(attDef);
                                                attRef.Visible = attDef.Visible;
                                                attRef.SetAttributeFromBlock(attDef, bref.BlockTransform);
                                                attRef.HorizontalMode = attDef.HorizontalMode;
                                                attRef.VerticalMode = attDef.VerticalMode;
                                                attRef.Rotation = attDef.Rotation;
                                                attRef.TextStyleId = attDef.TextStyleId;

                                                attRef.Position = attDef.Position.TransformBy(bref.BlockTransform);


                                                //attRef.Position = attDef.Position + origem.GetAsVector();
                                                attRef.Tag = attDef.Tag;
                                                attRef.FieldLength = attDef.FieldLength;
                                                attRef.TextString = attDef.TextString;

                                                attRef.AdjustAlignment(acCurDb);//?

                                                if (atributos.ContainsKey(attRef.Tag.ToUpper()))
                                                {
                                                    attRef.TextString = atributos[attRef.Tag.ToUpper()].ToString();
                                                }

                                                atcoll.AppendAttribute(attRef);

                                                acTrans.AddNewlyCreatedDBObject(attRef, true);
                                            }

                                        }

                                    }

                                    bref.DowngradeOpen();
                                }
                            }

                            btrec.DowngradeOpen();

                            acBlkTbl.DowngradeOpen();

                            // editor.Regen();

                            acTrans.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta($"Algo de errado aconteceu ao tentar inserir o bloco {endereco}\n\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}");
                return;
            }
            FLayer.Desligar(new List<string> { "Defpoints" }, false);
        }
        public static void MarcaPerfil(Point2d p0, string marca, double comprimento, DLM.cam.Perfil perfil, int quantidade, string material, string tratamento, double peso = 0, double superficie = 0, double escala = 10, string posicao = "", string mercadoria = "")
        {
            try
            {
                Hashtable ht = new Hashtable();
                ht.Add(TAB_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(TAB_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(TAB_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(TAB_DBF1.NOM_PRO.ToString(), perfil.Descricao);
                ht.Add(TAB_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.LUN_PRO.ToString(), comprimento.ToString("N0").Replace(",", ""));
                ht.Add(TAB_DBF1.MAT_PRO.ToString(), material);
                ht.Add(TAB_DBF1.TRA_PEZ.ToString(), tratamento);
                if (peso == 0)
                {
                    ht.Add(TAB_DBF1.PUN_LIS.ToString(), Math.Round(perfil.Peso * comprimento / 1000, CADVars.DECIMAIS_TON).ToString("N4").Replace(",", ""));
                }
                else
                {
                    ht.Add(TAB_DBF1.PUN_LIS.ToString(), peso);
                }

                if (superficie == 0)
                {
                    ht.Add(TAB_DBF1.SUN_LIS.ToString(), Math.Round(perfil.Peso * comprimento / 1000 / 1000 / 100, CADVars.DECIMAIS_TON).ToString("N4").Replace(",", ""));
                }
                else
                {
                    ht.Add(TAB_DBF1.SUN_LIS.ToString(), superficie.ToString("N4").Replace(",", ""));
                }
                ht.Add(TAB_DBF1.ING_PEZ.ToString(), perfil.Altura + "*" + perfil.Largura + "*" + comprimento);
                ht.Add(T_DBF2.DIM_PRO.ToString(), perfil.GetDIM_PRO());

                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                if (posicao == "")
                {
                    Inserir(acDoc, CADVars.Marca_Perfil, p0, escala, 0, ht);
                }
                else
                {
                    Inserir(acDoc, CADVars.Posicao_Perfil, p0, escala, 0, ht);

                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);

            }
        }
        public static void MarcaChapa(Point2d p0, ConfiguracaoChapa_Dobrada pf, Tipo_Bloco tipo, double escala, string posicao = "")
        {
            try
            {
                var bloco = CADVars.Marca_Chapa;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:
                ht.Add(TAB_DBF1.MAR_PEZ.ToString(), pf.Marca);
                ht.Add(TAB_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(TAB_DBF1.NOM_PRO.ToString(), pf.Descricao);
                ht.Add(TAB_DBF1.QTA_PEZ.ToString(), pf.Quantidade.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.LUN_PRO.ToString(), pf.Comprimento.ToString("N0").Replace(",", ""));
                ht.Add(TAB_DBF1.LAR_PRO.ToString(), pf.Largura.ToString("N0").Replace(",", ""));
                ht.Add(TAB_DBF1.SPE_PRO.ToString(), pf.Espessura.ToString("N2").Replace(",", ""));
                ht.Add(TAB_DBF1.MAT_PRO.ToString(), pf.Material);
                ht.Add(TAB_DBF1.TRA_PEZ.ToString(), pf.Ficha);
                ht.Add(TAB_DBF1.PUN_LIS.ToString(), pf.Peso_Unitario.ToString("N4").Replace(",", ""));
                ht.Add(TAB_DBF1.SUN_LIS.ToString(), pf.Superficie.ToString("N4").Replace(",", ""));
                ht.Add(TAB_DBF1.DES_PEZ.ToString(), pf.Mercadoria);
                ht.Add(TAB_DBF1.COS_PEZ.ToString(), pf.Dobras);
                ht.Add(TAB_DBF1.ING_PEZ.ToString(), pf.Volume);


                ht.Add(TAB_DBF1.COD_PEZ.ToString(), pf.SAP);
                ht.Add(TAB_DBF1.NOT_PEZ.ToString(), pf.Cor_1);
                ht.Add(TAB_DBF1.TIP_PEZ.ToString(), pf.Cor_2);
                ht.Add(T_DBF2.DIM_PRO.ToString(), 0);


                if (tipo == Tipo_Bloco.Arremate)
                {
                    if (posicao != "")
                    {
                        bloco = CADVars.Posicao_Chapa;
                    }
                    else
                    {
                        bloco = CADVars.Marca_Arremate;
                    }
                }
                else if (tipo == Tipo_Bloco.Chapa)
                {
                    if (posicao != "")
                    {
                        bloco = CADVars.Posicao_Chapa;
                    }
                    else
                    {
                        bloco = CADVars.Marca_Chapa;
                    }
                }
                else if (tipo == Tipo_Bloco.Elemento_M2)
                {
                    if (posicao != "")
                    {
                        bloco = CADVars.Posicao_Elemento_M2;
                    }
                    else
                    {
                        bloco = CADVars.Marca_Elemento_M2;
                    }
                }
                else
                {
                    Conexoes.Utilz.Alerta("Não implementado.");
                    return;
                }


                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public static void MarcaElemM2(Point2d p0, DLM.cam.Perfil pf, string marca, double quantidade, double comp, double larg, double area, double perimetro, string ficha, string material, double escala, string posicao = "", string mercadoria = "")
        {
            try
            {
                var bloco = CADVars.Marca_Chapa;
                Hashtable ht = new Hashtable();

                double superficie = area * 2 / 1000 / 100;
                //Pairs of tag-value:
                ht.Add(TAB_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(TAB_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(TAB_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(TAB_DBF1.NOM_PRO.ToString(), pf.Descricao);
                ht.Add(TAB_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.LUN_PRO.ToString(), comp.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.LAR_PRO.ToString(), larg.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.TRA_PEZ.ToString(), ficha);
                ht.Add(TAB_DBF1.PUN_LIS.ToString(), Math.Round(pf.Peso * area / 1000 / 1000 / 100, CADVars.DECIMAIS));
                ht.Add(TAB_DBF1.SUN_LIS.ToString(), Math.Round((area * 2 + perimetro * 2) / 1000 / 1000, CADVars.DECIMAIS));
                ht.Add(TAB_DBF1.ING_PEZ.ToString(), $"{comp}*{larg}");
                ht.Add(TAB_DBF1.COD_PEZ.ToString(), pf.SAP);
                ht.Add(TAB_DBF1.MAT_PRO.ToString(), material);


                if (posicao != "")
                {
                    bloco = CADVars.BL_P_ELEM2;
                }
                else
                {
                    bloco = CADVars.BL_M_ELEM2;
                }


                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public static void MarcaElemUnitario(Point2d p0, Conexoes.RMA pf, double quantidade, string marca, double escala, string posicao = "", string mercadoria = "")
        {
            try
            {
                var bloco = CADVars.Marca_Chapa;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:
                ht.Add(TAB_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(TAB_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(TAB_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(TAB_DBF1.NOM_PRO.ToString(), pf.DESC);
                ht.Add(TAB_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(TAB_DBF1.MAT_PRO.ToString(), pf.NORMA);
                ht.Add(TAB_DBF1.TRA_PEZ.ToString(), pf.TRATAMENTO);
                ht.Add(TAB_DBF1.PUN_LIS.ToString(), pf.PESOUNIT);


                ht.Add(TAB_DBF1.COD_PEZ.ToString(), pf.SAP);
                ht.Add(T_DBF2.DIM_PRO.ToString(), 0);


                if (posicao != "")
                {
                    bloco = CADVars.BL_P_ELUNIT;
                }
                else
                {
                    bloco = CADVars.BL_M_ELUNIT;
                }


                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public static void CamToMarcaSimples(DLM.cam.ReadCAM cam, Point2d origem, double escala)
        {

            if (cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Dobrado | cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Laminado | cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Soldado && !cam.Nome.Contains("_"))
            {
                var perfil = Conexoes.DBases.GetdbPerfil().GetPerfilTecnoMetal(cam.Descricao);
                if (perfil != null)
                {
                    if (perfil.Descricao == "")
                    {
                        Conexoes.Utilz.Alerta("Perfil não cadastrado: " + cam.Descricao + "\nTipo: " + cam.Perfil.Tipo + "\nCadastre o perfil no tecnometal e tente novamente.");
                    }
                    else
                    {
                        MarcaPerfil(origem, cam.Posicao, cam.Comprimento, perfil, cam.Quantidade, cam.Material, cam.Tratamento, cam.Peso, cam.Superficie, escala);
                    }
                }


            }
            else if (cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Chapa)
            {
                MarcaChapa(origem, new ConfiguracaoChapa_Dobrada(cam), Tipo_Bloco.Chapa, escala);
            }
            else
            {
                Conexoes.Utilz.Alerta("Tipo de CAM inválido ou não suportado:\n" + cam.Nome + "\n" + cam.Perfil.Tipo);
            }
        }


        public static BlockTableRecord GetPai(BlockReference bloco, Database acCurDb = null)
        {
            try
            {
                if (acCurDb == null)
                {
                    acCurDb = CAD.acCurDb;
                }
                using (DocumentLock acLckDoc = acDoc.LockDocument())
                {
                    using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        BlockReference blk = acTrans.GetObject(bloco.ObjectId, OpenMode.ForRead) as BlockReference;
                        BlockTableRecord acBlkTblRec = blk.IsDynamicBlock ?
                             acTrans.GetObject(blk.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord
                             :
                             acTrans.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;


                        return acBlkTblRec;
                    }
                }
            }
            catch (Exception)
            {

                return null;
            }

        }
        public static string GetNome(BlockReference bloco)
        {
            var nt = GetPai(bloco);
            if(nt!=null)
            {
                return nt.Name;
            }
            return bloco.Name;
        }

        public static List<Entity> GetEntities(BlockReference bloco)
        {

            List<Entity> retorno = new List<Entity>();

            try
            {
                DBObjectCollection acDBObjColl = new DBObjectCollection();
                bloco.Explode(acDBObjColl);

                foreach (Entity acEnt in acDBObjColl)
                {
                    try
                    {
                        retorno.Add(acEnt);
                    }
                    catch (Exception)
                    {

                    }

                }
            }
            catch (Exception)
            {

              
            }

            return retorno.FindAll(x=>x!=null);

        }

        public static List<Point2d> GetInterSeccao(BlockReference obj, Entity obj2)
        {
            List<Point2d> ptss = new List<Point2d>();



         
            DBObjectCollection acDBObjColl = new DBObjectCollection();
            obj.Explode(acDBObjColl);
           
            foreach (Entity acEnt in acDBObjColl)
            {
                try
                {
                    Point3dCollection pts = new Point3dCollection();
                    acEnt.IntersectWith(obj, Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendArgument, pts, new IntPtr(), new IntPtr());
                    foreach (Point3d p in pts)
                    {
                        ptss.Add(new Point2d(p.X, p.Y));
                    }
                }
                catch (Exception)
                {

                }

            }


            return ptss;
        }

        public static List<BlocoTag> GetBlocosProximos(List<BlocoTag> blocos, Point3d pt1, Point3d pt2, double tolerancia = 1)
        {
            return GetBlocosProximos(blocos, new Point2d(pt1.X, pt1.Y), new Point2d(pt2.X, pt2.Y), tolerancia);
        }

        


        public static List<BlocoTag> GetBlocosProximos(List<BlocoTag> blocos, Point2d pt1, Point2d pt2, double tolerancia = 1)
        {
            List<BlocoTag> blks = new List<BlocoTag>();


            Line p = new Line();
            p.StartPoint = new Point3d(pt1.X, pt1.Y,0);
            p.EndPoint = new Point3d(pt2.X, pt2.Y,0);

            //foreach(var b in blocos)
            //{
            //    var pts = GetInterSeccao(b, p);

            //    foreach (var pt in pts)
            //    {
            //        var dist1 = Math.Abs(pt.GetDistanceTo(pt1));
            //        var dist2 = Math.Abs(pt.GetDistanceTo(pt1));

            //        if (dist1 <= tolerancia | dist2 <= tolerancia)
            //        {
            //            blks.Add(b);
            //            break;
            //        }
            //    }
            //}
            //if(blks.Count>0)
            //{
            //return blks;
            //}
            




            using (var acTrans = acDoc.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (var blk in blocos)
                {

                    var d1 = Math.Round(Math.Abs(blk.Bloco.Position.DistanceTo(Ut.GetP3d(pt1))));
                    var d2 = Math.Round(Math.Abs(blk.Bloco.Position.DistanceTo(Ut.GetP3d(pt2))));





                    if (d1 <= tolerancia | d2 <= tolerancia)
                    {
                        blks.Add(blk);
                        continue;
                    }


                    var pts = blk.GetContorno(acTrans);

                    List<double> dists = new List<double>();
                    dists.AddRange(pts.Select(x => Math.Round(pt1.GetDistanceTo(x))).Distinct().ToList());
                    dists.AddRange(pts.Select(x => Math.Round(pt2.GetDistanceTo(x))).Distinct().ToList());

                    if(dists.Count>0)
                    {
                        var min = dists.Min();


                        if (min <= tolerancia)
                        {
                            blks.Add(blk);

                        }
                    }
                    

                }
            }
            return blks;
        }
        public static List<BlockReference> GetBlocosPrancha(string nome = "")
        {
            List<BlockReference> blocos = new List<BlockReference>();

            using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
            {

                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId objId in acBlkTblRec)
                {
                    Entity ent = (Entity)acTrans.GetObject(objId, OpenMode.ForRead);
                    if (ent is BlockReference)
                    {
                        var s = ent as BlockReference;

                        if (nome != "")
                        {
                            if (s.Name.Replace(" ", "").ToUpper() == nome.ToUpper().Replace(" ", ""))
                            {
                                blocos.Add(s);
                            }
                        }
                        else
                        {
                            blocos.Add(s);
                        }

                    }
                }



            }
            return blocos;
        }
        public static void Mover(BlockReference bloco, Point2d posicao)
        {
            Clonar(bloco, posicao);
            Ut.Apagar(new List<Entity> { bloco });
        }
        public static void Clonar(BlockReference bloco, Point2d novaposicao)
        {
            var atributos = Atributos.GetBlocoTag(bloco);

            Hashtable ht = new Hashtable();
            foreach (var cel in atributos.Atributos)
            {
                ht.Add(cel.Coluna, cel.Valor);
            }
            Blocos.Inserir(CAD.acDoc, bloco.Name, novaposicao, bloco.ScaleFactors.X, bloco.Rotation, ht);
        }
        public static void SetEscala(List<BlockReference> blocos, double escala)
        {
            if (escala > 0)
            {
                using (acDoc.LockDocument())
                {
                    using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        foreach (var bl in blocos)
                        {
                            bl.TransformBy(Matrix3d.Scaling(escala / bl.ScaleFactors.X, bl.Position));
                        }
                        acTrans.Commit();
                    }
                    acDoc.Editor.Regen();
                }
            }

        }

    }
}