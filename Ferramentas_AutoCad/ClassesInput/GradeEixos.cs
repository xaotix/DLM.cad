using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Ferramentas_DLM
{
    public class GradeEixos
    {
        public override string ToString()
        {
            return $"Grade Eixos {GetComprimento()}x{GetLargura()}";
        }
        public double GetComprimento()
        {
            if (this.GetEixosHorizontais().Count == 0) { return 0; }
            return this.GetEixosHorizontais().Sum(x => x.Vao);
        }
        public double GetLargura()
        {
            if (this.GetEixosVerticais().Count == 0) { return 0; }
            return this.GetEixosVerticais().Sum(x => x.Vao);
        }
        public Tipo_Vista Vista { get; private set; } = Tipo_Vista.Planta;
        private List<Eixo> _eixos { get; set; } = new List<Eixo>();
        public List<Eixo> GetEixosVerticais()
        {
            return _eixos.FindAll(x=>x.Sentido== Sentido.Vertical);
        }

        private List<VaoObra> _vaos_verticais { get; set; } = null;
        public List<VaoObra> GetVaosVerticais(bool update = false)
        {
            if(_vaos_verticais == null | update)
            {
                _vaos_verticais = new List<VaoObra>();
                var verticais = GetEixosVerticais();
                if (verticais.Count > 1)
                {
                    for (int i = 1; i < verticais.Count; i++)
                    {
                        Tipo_Vao tipo = Tipo_Vao.Intermediario;
                        if(i ==1)
                        {
                            tipo = Tipo_Vao.Borda_Esquerdo;
                        }
                        else if(i == verticais.Count-1)
                        {
                            tipo = Tipo_Vao.Borda_Direito;
                        }
                        VaoObra pp = new VaoObra(verticais[i - 1], verticais[i], this.CADPurlin, tipo);
                        _vaos_verticais.Add(pp);
                    }
                }
                int c = 0;
                var alturas = this._vaos_verticais.SelectMany(x => x.GetPurlins().Select(z => z.Y)).Distinct().ToList();

               
                foreach (var v in _vaos_verticais)
                {
                    List<ObjetoPurlin> pts = new List<ObjetoPurlin>();
                    foreach (var alt in alturas)
                    {
                        var igual = v.GetPurlins().Find(x => x.Y == alt);
                        if (igual == null)
                        {
                            igual = new ObjetoPurlin(new Point3d(v.CentroX, alt, 0), v);
                        }
                        pts.Add(igual);
                    }

                    v.SetPurlinsDummy(pts);
                }

                for (int i = 0; i < _vaos_verticais.Count; i++)
                {
                    for (int a = 0; a < _vaos_verticais[i].PurlinsDummy.Count; a++)
                    {
                        var y = _vaos_verticais[i].PurlinsDummy[a].Y;

                        if(i>0)
                        {
                            _vaos_verticais[i].PurlinsDummy[a].PurlinEsquerda = _vaos_verticais[i - 1].PurlinsDummy.Find(x => x.Y == y);
                        }

                        if(i<_vaos_verticais.Count-1)
                        {
                            _vaos_verticais[i].PurlinsDummy[a].PurlinDireita = _vaos_verticais[i + 1].PurlinsDummy.Find(x => x.Y == y);
                        }
                    }
                }

                for (int i = 0; i < _vaos_verticais.Count; i++)
                {
                    _vaos_verticais[i].GetTirantes();
                    _vaos_verticais[i].GetCorrentes();
                }


            }

          

            return _vaos_verticais;
        }
        public List<Eixo> GetEixosHorizontais()
        {
            return _eixos.FindAll(x => x.Sentido == Sentido.Horizontal);
        }
        public void Add(Sentido Sentido, string Nome, double Vao)
        {
            if(Nome.Replace(" ","") == "") { return; }
            if(_eixos.Find(x=>x.Nome.ToUpper().Replace(" ","") == Nome.ToUpper().Replace(" ","")) != null) { return; }
            if(Vao>0)
            {
                _eixos.Add(new Eixo(Sentido, Nome, Vao));
            }
        }
        public void Add(Sentido Sentido, double Vao, BlockReference bloco, Line line)
        {
            var neixo = new Eixo(Sentido, bloco, line, Vao);
            _eixos.Add(neixo);
        }

        public CADPurlin CADPurlin { get; private set; }
        public GradeEixos(CADPurlin cADPurlin)
        {
            this.CADPurlin = cADPurlin;
        }
    }
}
