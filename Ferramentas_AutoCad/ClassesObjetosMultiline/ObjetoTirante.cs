namespace Ferramentas_DLM
{
    public class ObjetoTirante :ObjetoMultiLineBase
    {
        public override string ToString()
        {
            return this.Nome;
        }
        public double Comprimento
        {
            get
            {
                return this.Multiline.Comprimento +  2*Offset;
            }
        }
        public double Offset { get; set; } = 0;

        public ObjetoTirante(ObjetoMultiline multiline, int numero, VaoObra vao)
        {
            this.CADPurlin = vao.CADPurlin;
            this.id_peca = vao.CADPurlin.id_tirante;
            this.Multiline = multiline;
            this.CentroBloco = multiline.Centro.GetPoint();
            this.VaoObra = vao;

            this.Offset = vao.CADPurlin.TirantesOffSet;

            this.Suporte = vao.CADPurlin.TirantesSuporte;
            this.SetPeca(vao.CADPurlin.GetTirantePadrao());
        }
    }



}
