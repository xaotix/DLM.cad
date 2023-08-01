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
using DLM.desenho;
using Conexoes;
using DLM.vars.cad;

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

        public static void Inserir(Document acDoc, string nome, P3d origem, double escala, double rotacao, db.Linha atributos)
        {
            string Endereco = "";
            if (File.Exists(nome))
            {
                Endereco = nome;
            }
            else
            {
                if (nome.Contains(@"\"))
                {
                    nome = nome.getNome();
                }
                var s = Cfg.Init.CAD_Raiz_Blocos_TecnoMetal_Marcacao.GetArquivos(nome + ".dwg");

                if (s.Count == 0)
                {
                    s = Cfg.Init.CAD_Raiz_Blocos_TecnoMetal_Simbologias.GetArquivos(nome + ".dwg"); ;
                }

                if (s.Count == 0)
                {
                    s = Cfg.Init.CAD_Raiz_Blocos_Indicacao.GetArquivos(nome + ".dwg");

                }

                if (s.Count == 0)
                {
                    s = Cfg.Init.CAD_Raiz_Blocos_Pcs.GetArquivos(nome + ".dwg"); ;
                }

                if (s.Count == 0)
                {
                    s = Cfg.Init.CAD_Raiz_Blocos_A2.GetArquivos(nome + ".dwg", true);
                }


                if (s.Count == 0)
                {
                    s = Cfg.Init.CAD_Raiz_Blocos.GetArquivos(nome + ".dwg", true);
                }
                if (s.Count == 0)
                {
                    Conexoes.Utilz.Alerta($"Bloco não encontrado:{nome}", "Operação abortada");
                    return;
                }
                else
                {
                    Endereco = s[0].Endereco;

                }
            }

            if (!File.Exists(Endereco))
            {
                Conexoes.Utilz.Alerta($"Bloco não encontrado\n! {Endereco}");
                return;
            }


            try
            {
                string nomeBloco = Endereco.getNome();




                ObjectId blkid = ObjectId.Null;
                using (var acTrans = acDoc.acTransST())
                {
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (acBlkTbl.Has(nomeBloco))
                    {
                        using (var docLock = acDoc.LockDocument())
                        {
                            //ed.WriteMessage("\nBloco já existe, adicionando atual...\n");

                            blkid = acBlkTbl[nomeBloco];
                            BlockReference bref = new BlockReference(new Point3d(origem.X, origem.Y, 0), blkid);
                            BlockTableRecord btr2 = (BlockTableRecord)acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite);
                            using (BlockTableRecord bdef = (BlockTableRecord)acTrans.GetObject(bref.BlockTableRecord, OpenMode.ForWrite))
                            {
                                bref.ScaleFactors = new Scale3d(escala, escala, escala);
                                bref.Rotation = rotacao.GrausParaRadianos();
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
                                        AttributeDefinition attRef = (AttributeDefinition)obj;
                                        AttributeReference atref = new AttributeReference();
                                        if (attRef != null)
                                        {
                                            atref = new AttributeReference();
                                            atref.SetAttributeFromBlock(attRef, bref.BlockTransform);
                                            //atref.Position = atdef.Position + bref.Position.GetAsVector();
                                            atref.Position = attRef.Position.TransformBy(bref.BlockTransform);
                                            if (atributos.Contem(attRef.Tag))
                                            {
                                                atref.TextString = atributos[attRef.Tag.ToUpper()].ToString();
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

                using (var docLock = acDoc.LockDocument())
                {
                    using (Database AcTmpDB = new Database(false, true))
                    {
                        using (var acTrans = acDoc.acTransST())
                        {
                            BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                            acBlkTbl.UpgradeOpen();

                            //se nao tem, ai ele vai tentar abrir e inserir
                            AcTmpDB.ReadDwgFile(Endereco, System.IO.FileShare.Read, true, "");
                            blkid = acCurDb.Insert(Endereco, AcTmpDB, true);

                            BlockTableRecord btrec = (BlockTableRecord)blkid.GetObject(OpenMode.ForRead);
                            btrec.UpgradeOpen();
                            btrec.Name = nomeBloco;
                            btrec.DowngradeOpen();



                            using (BlockTableRecord btr = (BlockTableRecord)acCurDb.CurrentSpaceId.GetObject(OpenMode.ForWrite))
                            {
                                using (BlockReference bref = new BlockReference(new Point3d(origem.X, origem.Y, 0), blkid))
                                {
                                    Matrix3d mat = Matrix3d.Identity;
                                    bref.TransformBy(mat);
                                    bref.ScaleFactors = new Scale3d(escala, escala, escala);
                                    bref.Rotation = rotacao.GrausParaRadianos();
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

                                                if (atributos.Contem(attRef.Tag))
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
                Conexoes.Utilz.Alerta(ex, $"Algo de errado aconteceu ao tentar inserir o bloco {Endereco}");
                return;
            }
            FLayer.Desligar(new List<string> { "Defpoints" }, false);
        }

        public static void Renomear(string nome_antigo, string novo_nome, bool auto_cont = true)
        {
            using (var acTrans = acCurDb.acTransST())
            {
                string nome = novo_nome;
                var block = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                if (block.Has(nome_antigo))
                {
                    if (auto_cont)
                    {
                        int c = 1;
                        while (block.Has(nome))
                        {
                            nome = novo_nome + "_" + c;
                            c++;
                        }
                    }


                    if (!block.Has(nome))
                    {
                        var btr = (BlockTableRecord)acTrans.GetObject(block[nome_antigo], OpenMode.ForWrite);
                        btr.Name = nome;
                    }
                    else
                    {
                        Conexoes.Utilz.Alerta($"Já Existe um bloco com o nome {nome}.");
                    }

                }

                acTrans.Commit();
            }
        }


        public static void IndicacaoPeca(string Bloco, string CODIGO, double COMP, int ID, P3d origem, string DESC = "", double escala = 1, double rotacao = 0, string QTD = "1", string DESTINO = "RME", string N = "", string FAMILIA = "PECA", string TIPO = "PECA")
        {
            var ht = new db.Linha();
            ht.Add(Cfg.Init.CAD_ATT_N, N);
            ht.Add(Cfg.Init.CAD_ATT_Familia, FAMILIA);
            ht.Add(Cfg.Init.CAD_ATT_Tipo, TIPO);
            ht.Add(Cfg.Init.CAD_ATT_Comprimento, COMP);
            ht.Add("CODIGO", CODIGO);
            ht.Add(Cfg.Init.CAD_ATT_id, ID);
            ht.Add(Cfg.Init.CAD_ATT_Descricao, DESC);
            ht.Add(Cfg.Init.CAD_ATT_Destino, DESTINO);
            ht.Add(Cfg.Init.CAD_ATT_Quantidade, QTD);

            Inserir(CAD.acDoc, Bloco, origem, escala, rotacao, ht);
        }
        public static void Criar(string nome, List<Entity> Objetos, P3d origem)
        {
            string nome_fim = nome;
            using (var acTrans = acCurDb.acTransST())
            {
                // Get the block table from the drawing
                BlockTable bt = (BlockTable)acTrans.GetObject(CAD.acCurDb.BlockTableId, OpenMode.ForRead);

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
                acTrans.AddNewlyCreatedDBObject(btr, true);

                foreach (Entity ent in Objetos)
                {
                    btr.AppendEntity(ent);
                    acTrans.AddNewlyCreatedDBObject(ent, true);
                }
                // Insere o bloco
                BlockTableRecord ms = (BlockTableRecord)acTrans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                BlockReference br = new BlockReference(origem.GetPoint3dCad(), btrId);

                ms.AppendEntity(br);
                acTrans.AddNewlyCreatedDBObject(br, true);
                acTrans.Commit();
            }

        }
        public static void MarcaComposta(P3d p0, string marca, double quantidade, string ficha, string mercadoria, double escala = 10)
        {
            try
            {

                var ht = new db.Linha();


                ht.Add(T_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), quantidade);
                ht.Add(T_DBF1.TRA_PEZ.ToString(), ficha);
                ht.Add(T_DBF1.DES_PEZ.ToString(), mercadoria);

                Inserir(acDoc, Cfg.Init.CAD_Marca_Composta, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

        }
        public static void MarcaPerfil(P3d p0, string marca, double comprimento, DLM.cam.Perfil perfil, int quantidade, string material, string tratamento, double peso = 0, double superficie = 0, double escala = 10, string posicao = "", string mercadoria = "")
        {
            try
            {
                var ht = new db.Linha();
                ht.Add(T_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(T_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(T_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(T_DBF1.NOM_PRO.ToString(), perfil.Descricao);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(T_DBF1.LUN_PRO.ToString(), comprimento.ToString("N0").Replace(",", ""));
                ht.Add(T_DBF1.MAT_PRO.ToString(), material);
                ht.Add(T_DBF1.TRA_PEZ.ToString(), tratamento);
                if (peso == 0)
                {
                    ht.Add(T_DBF1.PUN_LIS.ToString(), (perfil.Peso * comprimento / 1000).Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));
                }
                else
                {
                    ht.Add(T_DBF1.PUN_LIS.ToString(), peso.Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));
                }

                if (superficie == 0)
                {
                    ht.Add(T_DBF1.SUN_LIS.ToString(), (perfil.Peso * comprimento / 1000 / 1000 / 100).String(Cfg.Init.DECIMAIS_Superficie));
                }
                else
                {
                    ht.Add(T_DBF1.SUN_LIS.ToString(), superficie.ToString("N4").Replace(",", ""));
                }
                ht.Add(T_DBF1.ING_PEZ.ToString(), perfil.Altura + "*" + perfil.Largura + "*" + comprimento);
                ht.Add(T_DBF2.DIM_PRO.ToString(), perfil.GetDIM_PRO());

                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                if (posicao == "")
                {
                    Inserir(acDoc, Cfg.Init.CAD_Marca_Perfil, p0, escala, 0, ht);
                }
                else
                {
                    Inserir(acDoc, Cfg.Init.CAD_Posicao_Perfil, p0, escala, 0, ht);

                }
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);

            }
        }
        public static void MarcaChapa(P3d p0, ConfiguracaoChapa_Dobrada pf, Tipo_Bloco tipo, double escala, string posicao = "")
        {
            try
            {
                var bloco = Cfg.Init.CAD_Marca_Chapa;
                var ht = new db.Linha();
                //Pairs of tag-value:
                ht.Add(T_DBF1.MAR_PEZ.ToString(), pf.Marca);
                ht.Add(T_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(T_DBF1.NOM_PRO.ToString(), pf.Descricao);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), pf.Quantidade.ToString().Replace(",", ""));
                ht.Add(T_DBF1.LUN_PRO.ToString(), pf.Comprimento.ToString("N0").Replace(",", ""));
                ht.Add(T_DBF1.LAR_PRO.ToString(), pf.Largura.ToString("N0").Replace(",", ""));
                ht.Add(T_DBF1.SPE_PRO.ToString(), pf.Espessura.ToString("N2").Replace(",", ""));
                ht.Add(T_DBF1.MAT_PRO.ToString(), pf.Material);
                ht.Add(T_DBF1.TRA_PEZ.ToString(), pf.Ficha);
                ht.Add(T_DBF1.PUN_LIS.ToString(), pf.Peso_Unitario.Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));
                ht.Add(T_DBF1.SUN_LIS.ToString(), pf.Superficie.Round(Cfg.Init.DECIMAIS_Superficie));
                ht.Add(T_DBF1.DES_PEZ.ToString(), pf.Mercadoria);
                ht.Add(T_DBF1.COS_PEZ.ToString(), pf.Dobras);
                ht.Add(T_DBF1.ING_PEZ.ToString(), pf.Volume);


                ht.Add(T_DBF1.COD_PEZ.ToString(), pf.SAP);
                ht.Add(T_DBF1.NOT_PEZ.ToString(), pf.Cor_1);
                ht.Add(T_DBF1.TIP_PEZ.ToString(), pf.Cor_2);
                ht.Add(T_DBF2.DIM_PRO.ToString(), 0);


                if (tipo == Tipo_Bloco.Arremate)
                {
                    if (posicao != "")
                    {
                        bloco = Cfg.Init.CAD_Posicao_Chapa;
                    }
                    else
                    {
                        bloco = Cfg.Init.CAD_Marca_Arremate;
                    }
                }
                else if (tipo == Tipo_Bloco.Chapa)
                {
                    if (posicao != "")
                    {
                        bloco = Cfg.Init.CAD_Posicao_Chapa;
                    }
                    else
                    {
                        bloco = Cfg.Init.CAD_Marca_Chapa;
                    }
                }
                else if (tipo == Tipo_Bloco.Elemento_M2)
                {
                    if (posicao != "")
                    {
                        bloco = Cfg.Init.CAD_Posicao_Elemento_M2;
                    }
                    else
                    {
                        bloco = Cfg.Init.CAD_Marca_Elemento_M2;
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
                Conexoes.Utilz.Alerta(ex);

            }

        }

        public static void MarcaChapa(P3d p0, List<P3d> pts, double Espessura, int Quantidade, string Marca, string Material, string Ficha, double escala)
        {
            try
            {
                var Largura = pts.Largura();
                var Area = pts.Area();
                var Perimetro = pts.Perimetro();
                var Comprimento = pts.Comprimento();
                var Descricao = $"Ch #{Espessura.ToString("N2")}x{Largura.ToString("N0")}x{Comprimento.ToString("N0")}";
                var Peso_Unitario = pts.Peso(Espessura);
                var Superficie = (Area * 2) + (Perimetro * Espessura);
                var Geometria = $"{Comprimento.ToString("N0").Replace(",", "")}*{Espessura.ToString("N2").Replace(",", "")}*{Largura.ToString("N0").Replace(",", "")}";

                var bloco = Cfg.Init.CAD_Marca_Chapa;
                var ht = new db.Linha();
                //Pairs of tag-value:
                ht.Add(T_DBF1.MAR_PEZ.ToString(), Marca);
                ht.Add(T_DBF1.POS_PEZ.ToString(), Marca);
                ht.Add(T_DBF1.NOM_PRO.ToString(), Descricao);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), Quantidade.ToString().Replace(",", ""));
                ht.Add(T_DBF1.LUN_PRO.ToString(), Comprimento.ToString("N0").Replace(",", ""));
                ht.Add(T_DBF1.LAR_PRO.ToString(), Largura.ToString("N0").Replace(",", ""));
                ht.Add(T_DBF1.SPE_PRO.ToString(), Espessura.ToString("N2").Replace(",", ""));
                ht.Add(T_DBF1.MAT_PRO.ToString(), Material);
                ht.Add(T_DBF1.TRA_PEZ.ToString(), Ficha);
                ht.Add(T_DBF1.PUN_LIS.ToString(), Peso_Unitario.Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));
                ht.Add(T_DBF1.SUN_LIS.ToString(), Superficie.Round(Cfg.Init.DECIMAIS_Superficie));
                ht.Add(T_DBF1.DES_PEZ.ToString(), "CHAPA");
                ht.Add(T_DBF1.ING_PEZ.ToString(), Geometria);


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

        }
        public static void MarcaElemM2(P3d p0, DLM.cam.Perfil perfil, string marca, double quantidade, double comp, double larg, double area, double perimetro, string ficha, string material, double escala, string posicao = "", string mercadoria = "")
        {
            try
            {
                var bloco = Cfg.Init.CAD_Marca_Chapa;
                var ht = new db.Linha();

                double superficie = area * 2 / 1000 / 100;
                //Pairs of tag-value:
                ht.Add(T_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(T_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(T_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(T_DBF1.NOM_PRO.ToString(), perfil.Descricao);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(T_DBF1.LUN_PRO.ToString(), comp.String(0));
                ht.Add(T_DBF1.LAR_PRO.ToString(), larg.String(0));
                ht.Add(T_DBF1.TRA_PEZ.ToString(), ficha);
                ht.Add(T_DBF1.PUN_LIS.ToString(), (perfil.Peso * area / 1000 / 1000 / 100).Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));
                ht.Add(T_DBF1.SUN_LIS.ToString(), Math.Round((area * 2 + perimetro * 2) / 1000 / 1000, Cfg.Init.DECIMAIS_Superficie));
                ht.Add(T_DBF1.ING_PEZ.ToString(), $"{comp}*{larg}");
                ht.Add(T_DBF1.COD_PEZ.ToString(), perfil.SAP);
                ht.Add(T_DBF1.MAT_PRO.ToString(), material);


                if (posicao != "")
                {
                    bloco = Cfg.Init.CAD_BL_P_ELEM2;
                }
                else
                {
                    bloco = Cfg.Init.CAD_BL_M_ELEM2;
                }


                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

        }
        public static void MarcaElemUnitario(P3d p0, Conexoes.RMA pf, double quantidade, string marca, double escala, string posicao = "", string mercadoria = "")
        {
            try
            {
                var bloco = Cfg.Init.CAD_Marca_Chapa;
                var ht = new db.Linha();
                //Pairs of tag-value:
                ht.Add(T_DBF1.MAR_PEZ.ToString(), marca);
                ht.Add(T_DBF1.DES_PEZ.ToString(), mercadoria);
                ht.Add(T_DBF1.POS_PEZ.ToString(), posicao);
                ht.Add(T_DBF1.NOM_PRO.ToString(), pf.DESC);
                ht.Add(T_DBF1.QTA_PEZ.ToString(), quantidade.ToString().Replace(",", ""));
                ht.Add(T_DBF1.MAT_PRO.ToString(), pf.NORMA);
                ht.Add(T_DBF1.TRA_PEZ.ToString(), pf.TRATAMENTO);
                ht.Add(T_DBF1.PUN_LIS.ToString(), pf.PESOUNIT.Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS));


                ht.Add(T_DBF1.COD_PEZ.ToString(), pf.SAP);
                ht.Add(T_DBF2.DIM_PRO.ToString(), 0);


                if (posicao != "")
                {
                    bloco = Cfg.Init.CAD_BL_P_ELUNIT;
                }
                else
                {
                    bloco = Cfg.Init.CAD_BL_M_ELUNIT;
                }


                if (posicao != "")
                {
                    p0 = Ut.AddLeader(0, p0, escala, "", 12, true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

        }
        public static void CamToMarcaSimples(DLM.cam.ReadCAM cam, P3d origem, double escala)
        {

            if (cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Dobrado | cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Laminado | cam.Perfil.Familia == DLM.vars.CAM_FAMILIA.Soldado && !cam.Nome.Contains("_"))
            {
                var perfil = DBases.GetdbPerfil().GetPerfilTecnoMetal(cam.Descricao);
                if (perfil != null)
                {
                    if (perfil.Descricao == "")
                    {
                        Conexoes.Utilz.Alerta("Perfil não cadastrado: " + cam.Descricao + "\nTipo: " + cam.Perfil.Tipo + "\nCadastre o perfil no tecnometal e tente novamente.");
                    }
                    else
                    {
                        MarcaPerfil(origem, cam.Nome, cam.Comprimento, perfil, cam.Quantidade, cam.Material, cam.Tratamento, cam.Peso, cam.Superficie, escala);
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



        public static string GetNome(BlockReference bloco)
        {
            var parent = bloco.GetTableRecord();
            if (parent != null)
            {
                return parent.Name;
            }
            return bloco.Name;
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

        public static List<BlockAttributes> GetBlocosProximos(List<BlockAttributes> blocos, Point3d pt1, Point3d pt2, double tolerancia = 1)
        {
            return GetBlocosProximos(blocos, new P3d(pt1.X, pt1.Y), new P3d(pt2.X, pt2.Y), tolerancia);
        }




        public static List<BlockAttributes> GetBlocosProximos(List<BlockAttributes> blocos, P3d pt1, P3d pt2, double tolerancia = 1)
        {
            List<BlockAttributes> blks = new List<BlockAttributes>();


            Line p = new Line();
            p.StartPoint = new Point3d(pt1.X, pt1.Y, 0);
            p.EndPoint = new Point3d(pt2.X, pt2.Y, 0);

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





            using (var acTrans = acCurDb.acTrans())
            {
                foreach (var blk in blocos)
                {

                    var d1 = Math.Round(Math.Abs(blk.Block.Position.DistanceTo(pt1.GetPoint3dCad())));
                    var d2 = Math.Round(Math.Abs(blk.Block.Position.DistanceTo(pt2.GetPoint3dCad())));





                    if (d1 <= tolerancia | d2 <= tolerancia)
                    {
                        blks.Add(blk);
                        continue;
                    }


                    var pts = blk.GetPontos(acTrans);

                    List<double> dists = new List<double>();
                    dists.AddRange(pts.Select(x => Math.Round(pt1.Distancia(x.P3d()))).Distinct().ToList());
                    dists.AddRange(pts.Select(x => Math.Round(pt2.Distancia(x.P3d()))).Distinct().ToList());

                    if (dists.Count > 0)
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

            using (var acTrans = acCurDb.acTrans())
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
        public static void Mover(BlockReference bloco, P3d posicao)
        {
            Clonar(bloco, posicao);
            acDoc.Apagar(new List<Entity> { bloco });
        }
        public static void Clonar(BlockReference bloco, P3d novaposicao)
        {
            var atributos = bloco.GetAttributes();

            var ht = new db.Linha();
            foreach (var celula in atributos.Celulas)
            {
                ht.Add(celula.Coluna, celula.Valor);
            }
            Blocos.Inserir(acDoc, bloco.Name, novaposicao, bloco.ScaleFactors.X, bloco.Rotation, ht);
        }
        public static void SetEscala(List<BlockReference> blocos, double escala)
        {
            if (escala > 0)
            {
                using (var docLock = acDoc.LockDocument())
                {
                    using (var acTrans = acDoc.acTransST())
                    {
                        foreach (var bl in blocos)
                        {
                            bl.TransformBy(Matrix3d.Scaling(escala / bl.ScaleFactors.X, bl.Position));
                        }
                        acTrans.Commit();
                    }
                    editor.Regen();
                }
            }

        }

    }
}