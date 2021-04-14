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
        private Conexoes.SubEtapaTecnoMetal _subetapa { get; set; }
        private Conexoes.ObraTecnoMetal _obra { get; set; }
        private Conexoes.PedidoTecnoMetal _pedido { get; set; }

        public Conexoes.SubEtapaTecnoMetal GetSubEtapa()
        {
            if(_subetapa==null && Directory.Exists(this.Pasta))
            {
                _subetapa = new Conexoes.SubEtapaTecnoMetal(this.Pasta,null);
            }
            return _subetapa;
        }
        public Conexoes.ObraTecnoMetal GetObra()
        {
            if (_obra == null && Directory.Exists(this.Pasta))
            {
                _obra = GetSubEtapa().GetObra();
            }
            return _obra;
        }
        public Conexoes.PedidoTecnoMetal GetPedido()
        {
            if (_pedido == null && Directory.Exists(this.Pasta))
            {
                _pedido = GetSubEtapa().GetPedido();
            }
            return _pedido;
        }
        public TecnoMetal()
        {

        }

        public DB.Tabela GerarDBF(string destino = null)
        {
            if(!this.Pasta.ToUpper().EndsWith(@".TEC\"))
            {
                Alerta($"Não é possível rodar esse comando fora de pastas de etapas (.TEC)" +
                    $"\nPasta atual: {this.Pasta}");
                return new DB.Tabela();
            }

            var etapa = this.GetSubEtapa();
            if (destino == null | destino =="")
            {
                destino = etapa.PastaDBF + etapa.Nome + ".DBF";
            }

            if (!Conexoes.Utilz.Apagar(destino))
            {
                Alerta($"Não é possível substituir o arquivo atual: {destino}");
                return new DB.Tabela();
            }




            DB.Tabela ret = new DB.Tabela();
            var pcs = LerPecas();
            if(pcs.Linhas.Count==0)
            {
                return new DB.Tabela();
            }

     



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
                    ret.Banco = "";
                    return ret;
                }
            }
            ret.Banco = destino;
            return ret;
        }
        public DB.Linha GetLinha(BlockReference bloco, Database db, string arquivo, string nome,DateTime ultima_edicao)
        {
            try
            {
                var att = Utilidades.GetAtributos(bloco, db);
                att.Add("ARQUIVO", arquivo);
                att.Add("NUM_COM", this.GetPedido().Codigo);
                att.Add("DES_COM", this.GetObra().Nome);
                att.Add("LOT_COM", this.GetSubEtapa().NomeEtapa);
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

        public DB.Tabela LerPecas(List<string> pranchas = null, bool filtrar = true)
        {

            DB.Tabela marcas = new DB.Tabela();
            DB.Tabela posicoes = new DB.Tabela();
            try
            {
                List<FileInfo> arquivos = new List<FileInfo>();
               if(pranchas==null)
                {
                    DirectoryInfo d = new DirectoryInfo(this.Pasta);
                    arquivos = d.GetFiles("*.dwg").ToList();
                }
               else
                {
                    arquivos = pranchas.Select(x => new FileInfo(x)).ToList();
                }

                arquivos = arquivos.FindAll(x => x.FullName.ToUpper().EndsWith(".DWG")).ToList();
                
                if(filtrar)
                {
                    var selecao = arquivos.FindAll(x => x.Name.ToUpper().Contains("-FA-"));
                    var resto = arquivos.FindAll(x => !x.Name.ToUpper().Contains("-FA-"));
                    arquivos = Conexoes.Utilz.SelecionarObjetos( resto, selecao, "Selecione as pranchas.");
                }

                if (arquivos.Count==0)
                {
                    Alerta("Operação abortada - Nada Selecionado.");
                    return new DB.Tabela();
                }

                Conexoes.Wait w = new Conexoes.Wait(arquivos.Count(), "Carregando...");
                w.Show();
                foreach (FileInfo file in arquivos)
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
