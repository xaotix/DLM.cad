using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ferramentas_DLM
{
    public class TecnoMetal : ClasseBase
    {
        public TecnoMetal()
        {

        }
        public string GetPastaDBF()
        {
            var pasta = this.Pasta;
            var dbf = Conexoes.Utilz.CriarPasta(pasta, "DBF");
            return dbf;
        }
        public DB.Tabela GerarDBF(string destino = null)
        {
            if(destino == null)
            {
                destino = GetPastaDBF() + Conexoes.Utilz.getNome(this.Pasta).ToUpper().Split('.')[0] + ".DBF";
            }

            DB.Tabela ret = new DB.Tabela();
            var pcs = LerPecas();
            List<string> colunas = new List<string>();
            colunas.Add("FLG_REC");
            colunas.Add("NUM_COM");
            colunas.Add("DES_COM");
            colunas.Add("LOT_COM");
            colunas.Add("DLO_COM");
            colunas.Add("CLI_COM");
            colunas.Add("IND_COM");
            colunas.Add("DT1_COM");
            colunas.Add("DT2_COM");
            colunas.Add("NUM_DIS");
            colunas.Add("DES_DIS");
            colunas.Add("NOM_DIS");
            colunas.Add("REV_DIS");
            colunas.Add("DAT_DIS");
            colunas.Add("TRA_PEZ");
            colunas.Add("SBA_PEZ");
            colunas.Add("TIP_PEZ");
            colunas.Add("MAR_PEZ");
            colunas.Add("MBU_PEZ");
            colunas.Add("DES_PEZ");
            colunas.Add("POS_PEZ");
            colunas.Add("NOT_PEZ");
            colunas.Add("ING_PEZ");
            colunas.Add("QTA_PEZ");
            colunas.Add("QT1_PEZ");
            colunas.Add("MCL_PEZ");
            colunas.Add("COD_PEZ");
            colunas.Add("COS_PEZ");
            colunas.Add("NOM_PRO");
            colunas.Add("LUN_PRO");
            colunas.Add("LAR_PRO");
            colunas.Add("SPE_PRO");
            colunas.Add("MAT_PRO");
            colunas.Add("TIP_BUL");
            colunas.Add("DIA_BUL");
            colunas.Add("LUN_BUL");
            colunas.Add("PRB_BUL");
            colunas.Add("PUN_LIS");
            colunas.Add("SUN_LIS");
            colunas.Add("PRE_LIS");
            colunas.Add("FLG_DWG");

            foreach(var s in pcs.Linhas)
            {
                DB.Linha l = new DB.Linha();
                foreach(var c  in colunas)
                {
                    var igual = s.Get(c);
                    l.Add(c, igual.valor);
                }
                ret.Linhas.Add(l);
            }

            if(destino!="" && destino!=null && ret.Linhas.Count>0)
            {
               if(!Conexoes.Utilz.GerarDBF(ret, destino))
                {
                    ret.Banco = "erro ao tentar gerar a DBF";
                }
            }

            return ret;
        }
        public DB.Linha GetLinha(BlockReference bloco, Database db, string arquivo, string nome,DateTime ultima_edicao)
        {
            try
            {
                var att = Utilidades.GetAtributos(bloco, db);
                att.Add("ARQUIVO", arquivo);
                att.Add("NUM_COM", pedido);
                att.Add("DES_COM", nome_da_obra);
                att.Add("LOT_COM", etapa);
                att.Add("NUM_DIS", nome);
                att.Add("FLG_DWG", nome);
                att.Add("FLG_REC", att.Get("POS_PEZ").ToString() == "" ? "03" : "04");
                att.Add("DAT_DIS", ultima_edicao.ToShortDateString());

                return att;
            }
            catch (Exception ex)
            {
                DB.Linha att = new DB.Linha();
                att.Add("BLOCO", nome);
                att.Add("ERRO", ex.Message);
                att.Add("ARQUIVO", arquivo);

                return att;
            }
        }
        public string pedido = "";
        public string nome_da_obra = "";
        public string etapa = "";
        public DB.Tabela LerPecas()
        {


            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            try
            {
                DirectoryInfo d = new DirectoryInfo(this.Pasta);
                FileInfo[] Files = d.GetFiles("*-FA-*.dwg");
                Conexoes.Wait w = new Conexoes.Wait(Files.Count(), "Carregando...");
                w.Show();
                foreach (FileInfo file in Files)
                {
                    w.somaProgresso(file.Name);
                    DateTime ultima_edicao = System.IO.File.GetLastWriteTime(file.FullName);

                    var nome_arq = Conexoes.Utilz.getNome(file.FullName);
                    string arquivo = file.FullName;
                    try
                    {
                        using (Database db = new Database(false, true))
                        {
                            db.ReadDwgFile(arquivo, FileOpenMode.OpenForReadAndAllShare, false, null);
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {
                               
                                var btl = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                                List<BlockReference> blocos = new List<BlockReference>();
                                foreach (ObjectId objId in btr)
                                {
                                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                                    if (ent is BlockReference)
                                    {
                                        var s = ent as BlockReference;
                                        blocos.Add(s);
                                    }
                                }

                                var nomes = blocos.Select(x => x.Name).Distinct().ToList();
                                List<BlockReference> ms = Utilidades.GetBlocos(blocos, Constantes.BlocosTecnoMetalMarcas);
                                List<BlockReference> pos = Utilidades.GetBlocos(blocos, Constantes.BlocosTecnoMetalPosicoes);


                                foreach(var m in ms)
                                {
                                    marcas.Linhas.Add(GetLinha(m, db, arquivo, nome_arq, ultima_edicao));
                                }

                                foreach (var m in pos)
                                {
                                    posicoes.Linhas.Add(GetLinha(m, db, arquivo, nome_arq, ultima_edicao));
                                }

                            }
                            db.CloseInput(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        w.Close();
                        Alerta(ex.Message + "\n" + ex.StackTrace);
                        return new DB.Tabela();
                    }

                }
                w.Close();
            }
            catch (System.Exception ex)
            {
            }
            return new DB.Tabela(new List<DB.Tabela> { marcas, posicoes });
        }
    }
}
