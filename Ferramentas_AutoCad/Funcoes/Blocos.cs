using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoeditorInput;
using Autodesk.AutoCAD.Geometry;
using Conexoes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ferramentas_DLM.CAD;


namespace Ferramentas_DLM
{
   public static class Blocos
    {
        public static void MarcaComposta(Point3d p0,string marca, double quantidade, string ficha,string mercadoria, double escala = 10)
        {
            try
            {
                
                Hashtable ht = new Hashtable();


                ht.Add(Constantes.ATT_MAR, marca);
                ht.Add(Constantes.ATT_QTD, quantidade);
                ht.Add(Constantes.ATT_FIC, ficha);
                ht.Add(Constantes.ATT_MER, mercadoria);

                Inserir(acDoc, Constantes.Marca_Composta, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Utilidades.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }


        public static void Inserir(Document acDoc, string nome, Point3d origem, double escala, double rotacao, Hashtable atributos)
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
                var s = Conexoes.Utilz.GetArquivos(Constantes.Raiz_Blocos_TecnoMetal_Marcacao, nome + ".dwg");

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(Constantes.Raiz_Blocos_TecnoMetal_Simbologias, nome + ".dwg"); ;
                }


                s = Conexoes.Utilz.GetArquivos(Constantes.Raiz_Blocos_Indicacao, nome + ".dwg");

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(Constantes.Raiz_Blocos_Pcs, nome + ".dwg"); ;
                }

                if (s.Count == 0)
                {
                    s = Conexoes.Utilz.GetArquivos(Constantes.Raiz_Blocos_A2, nome + ".dwg", SearchOption.AllDirectories);
                }
                if (s.Count == 0)
                {
                    Utilidades.Alerta($"Bloco não encontrado:{nome}", "Operação abortada");
                    return;
                }
                else
                {
                    endereco = s[0];

                }
            }

            if (!File.Exists(endereco))
            {
                Utilidades.Alerta($"Bloco não encontrado\n! {endereco}");
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
                            BlockReference bref = new BlockReference(origem, blkid);
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



                            BlockTableRecord btr = (BlockTableRecord)acCurDb.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                            using (btr)
                            {
                                using (BlockReference bref = new BlockReference(origem, blkid))
                                {
                                    Matrix3d mat = Matrix3d.Identity;
                                    bref.TransformBy(mat);
                                    bref.ScaleFactors = new Scale3d(escala, escala, escala);
                                    bref.Rotation = Conexoes.Utilz.GrausParaRadianos(rotacao);
                                    bref.Position = origem;
                                    btr.AppendEntity(bref);
                                    acTrans.AddNewlyCreatedDBObject(bref, true);

                                    using (BlockTableRecord btAttRec = (BlockTableRecord)bref.BlockTableRecord.GetObject(OpenMode.ForRead))
                                    {
                                        Autodesk.AutoCAD.DatabaseServices.AttributeCollection atcoll = bref.AttributeCollection;

                                        foreach (ObjectId subid in btAttRec)
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
                Utilidades.Alerta($"Algo de errado aconteceu ao tentar inserir o bloco {endereco}\n\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}");
                return;
            }
            FLayer.Desligar(new List<string> { "Defpoints" }, false);
        }
        public static void MarcaPerfil(Point3d p0, string marca, double comprimento, Conexoes.TecnoMetal_Perfil perfil, int quantidade, string material, string tratamento, double peso = 0, double superficie = 0, double escala = 10, string posicao = "", string mercadoria = "")
        {
            try
            {
                Hashtable ht = new Hashtable();
                ht.Add(Constantes.ATT_MAR, marca);
                ht.Add(Constantes.ATT_MER, mercadoria);
                ht.Add(Constantes.ATT_POS, posicao);
                ht.Add(Constantes.ATT_PER, perfil.Nome);
                ht.Add(Constantes.ATT_QTD, quantidade.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_CMP, comprimento.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_MAT, material);
                ht.Add(Constantes.ATT_FIC, tratamento);
                if (peso == 0)
                {
                    ht.Add(Constantes.ATT_PES, Math.Round(perfil.PESO * comprimento / 1000,4).ToString("N4").Replace(",", ""));
                }
                else
                {
                    ht.Add(Constantes.ATT_PES, peso);
                }

                if (superficie == 0)
                {
                    ht.Add(Constantes.ATT_SUP, Math.Round(perfil.SUPERFICIE * comprimento / 1000/1000/100,4).ToString("N4").Replace(",", ""));
                }
                else
                {
                    ht.Add(Constantes.ATT_SUP, superficie.ToString("N4").Replace(",", ""));
                }
                ht.Add(Constantes.ATT_VOL, perfil.H + "*" + perfil.ABA_1 + "*" + comprimento);
                ht.Add(Constantes.ATT_GEO, perfil.DIM_PRO);

                if (posicao != "")
                {
                    p0 = Utilidades.AddLeader(0, p0, escala, "", 12, true);
                }


                if (posicao=="")
                {
                Inserir(acDoc, Constantes.Marca_Perfil, p0, escala, 0, ht);
                }
                else
                {
                    Inserir(acDoc, Constantes.Posicao_Perfil, p0, escala, 0, ht);

                }
            }
            catch (System.Exception ex)
            {
                Utilidades.Alerta(ex.Message + "\n" + ex.StackTrace);

            }
        }
        public static void MarcaChapa(Point3d p0, Chapa_Dobrada pf, Tipo_Bloco tipo, double escala, string posicao ="")
        {
            try
            {
                var bloco =  Constantes.Marca_Chapa;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:
                ht.Add(Constantes.ATT_MAR, pf.Marca);
                ht.Add(Constantes.ATT_POS, posicao);
                ht.Add(Constantes.ATT_PER, pf.Descricao);
                ht.Add(Constantes.ATT_QTD, pf.Quantidade.ToString("N4").Replace(",", ""));
                ht.Add(Constantes.ATT_CMP, pf.Comprimento.ToString("N0").Replace(",", ""));
                ht.Add(Constantes.ATT_LRG, pf.Largura.ToString("N0").Replace(",", ""));
                ht.Add(Constantes.ATT_ESP, pf.Espessura.ToString("N2").Replace(",", ""));
                ht.Add(Constantes.ATT_MAT, pf.Material);
                ht.Add(Constantes.ATT_FIC, pf.Ficha);
                ht.Add(Constantes.ATT_PES, pf.Peso_Unitario.ToString("N4").Replace(",", ""));
                ht.Add(Constantes.ATT_SUP, pf.Superficie.ToString("N4").Replace(",", ""));
                ht.Add(Constantes.ATT_MER, pf.Mercadoria);
                ht.Add(Constantes.ATT_CCC, pf.Dobras);
                ht.Add(Constantes.ATT_VOL, pf.Volume);


                ht.Add(Constantes.ATT_SAP, pf.SAP);
                ht.Add(Constantes.ATT_NOT, pf.Cor_1);
                ht.Add(Constantes.ATT_TPC, pf.Cor_2);
                ht.Add(Constantes.ATT_GEO, 0);


                if (tipo == Tipo_Bloco.Arremate)
                {
                    if(posicao!="")
                    {
                        bloco = Constantes.Posicao_Chapa;
                    }
                    else
                    {
                        bloco = Constantes.Marca_Arremate;
                    }
                }
                else if (tipo == Tipo_Bloco.Chapa)
                {
                    if (posicao != "")
                    {
                        bloco = Constantes.Posicao_Chapa;
                    }
                    else
                    {
                        bloco = Constantes.Marca_Chapa;
                    }
                }
                else if (tipo ==  Tipo_Bloco.Elemento_M2)
                {
                    if (posicao != "")
                    {
                        bloco = Constantes.Posicao_Elemento_M2;
                    }
                    else
                    {
                        bloco = Constantes.Marca_Elemento_M2;
                    }
                }
                else
                {
                    Utilidades.Alerta("Não implementado.");
                    return;
                }


                if(posicao!="")
                {
                    p0 = Utilidades.AddLeader(0, p0, escala,"",12,true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Utilidades.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }

        public static void MarcaElemM2(Point3d p0,Conexoes.TecnoMetal_Perfil pf,  string marca, double quantidade,double comp, double larg, double area, double perimetro, string ficha, string material, double escala, string posicao = "",string mercadoria = "")
        {
            try
            {
                var bloco = Constantes.Marca_Chapa;
                Hashtable ht = new Hashtable();

                double superficie = area * 2 / 1000 / 100;
                //Pairs of tag-value:
                ht.Add(Constantes.ATT_MAR, marca);
                ht.Add(Constantes.ATT_MER, mercadoria);
                ht.Add(Constantes.ATT_POS, posicao);
                ht.Add(Constantes.ATT_PER, pf.Nome);
                ht.Add(Constantes.ATT_QTD, quantidade.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_CMP, comp.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_LRG, larg.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_FIC, ficha);
                ht.Add(Constantes.ATT_PES, Math.Round(pf.PESO * area /1000/1000/100,3));
                ht.Add(Constantes.ATT_SUP, Math.Round((area*2 + perimetro*2)/1000/1000,3));
                ht.Add(Constantes.ATT_VOL, $"{comp}*{larg}");
                ht.Add(Constantes.ATT_SAP, pf.SAP);
                ht.Add(Constantes.ATT_MAT, material);


                if (posicao != "")
                {
                    bloco = Constantes.BL_P_ELEM2;
                }
                else
                {
                    bloco = Constantes.BL_M_ELEM2;
                }


                if (posicao != "")
                {
                    p0 = Utilidades.AddLeader(0, p0, escala, "", 12,true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Utilidades.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }


        public static void MarcaElemUnitario(Point3d p0, RMA pf, double quantidade, string marca, double escala, string posicao = "", string mercadoria = "")
        {
            try
            {
                var bloco = Constantes.Marca_Chapa;
                Hashtable ht = new Hashtable();
                //Pairs of tag-value:
                ht.Add(Constantes.ATT_MAR, marca);
                ht.Add(Constantes.ATT_MER, mercadoria);
                ht.Add(Constantes.ATT_POS, posicao);
                ht.Add(Constantes.ATT_PER, pf.DESC);
                ht.Add(Constantes.ATT_QTD, quantidade.ToString().Replace(",", ""));
                ht.Add(Constantes.ATT_MAT, pf.NORMA);
                ht.Add(Constantes.ATT_FIC, pf.TRATAMENTO);
                ht.Add(Constantes.ATT_PES, pf.PESO);


                ht.Add(Constantes.ATT_SAP, pf.SAP);
                ht.Add(Constantes.ATT_GEO, 0);


                if (posicao != "")
                {
                    bloco = Constantes.BL_P_ELUNIT;
                }
                else
                {
                    bloco = Constantes.BL_M_ELUNIT;
                }


                if (posicao != "")
                {
                    p0 = Utilidades.AddLeader(0, p0, escala, "", 12,true);
                }


                Inserir(acDoc, bloco, p0, escala, 0, ht);
            }
            catch (System.Exception ex)
            {
                Utilidades.Alerta(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public static void CamToMarcaSimples(DLMCam.ReadCam cam, Point3d origem, double escala)
        {

            if (cam.Familia == DLMCam.Familia.Dobrado | cam.Familia == DLMCam.Familia.Laminado | cam.Familia == DLMCam.Familia.Soldado && !cam.Nome.Contains("_"))
            {
                TecnoMetal_Perfil perfil = Utilidades.GetdbTecnoMetal().Get(cam.Descricao);
                if (perfil != null)
                {
                    if (perfil.Nome == "")
                    {
                        Utilidades.Alerta("Perfil não cadastrado: " + cam.Descricao + "\nTipo: " + cam.TipoPerfil + "\nCadastre o perfil no tecnometal e tente novamente.");
                    }
                    else
                    {
                        MarcaPerfil(origem, cam.Posicao, cam.Comprimento, perfil, cam.Quantidade, cam.Material, cam.Tratamento, cam.Peso, cam.Superficie,escala);
                    }
                }


            }
            else if (cam.Familia == DLMCam.Familia.Chapa)
            {
                MarcaChapa(origem, new Chapa_Dobrada(cam), Tipo_Bloco.Chapa, escala);
            }
            else
            {
                Utilidades.Alerta("Tipo de CAM inválido ou não suportado:\n" + cam.Nome + "\n" + cam.TipoPerfil);
            }
        }
    }
}