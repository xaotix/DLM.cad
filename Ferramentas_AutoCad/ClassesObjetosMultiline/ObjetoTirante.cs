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

        public ObjetoTirante(CADMline multiline,  VaoObra vao)
        {
            this.Grade = vao.Grade;
            this.CADPurlin = vao.CADPurlin;
            this.id_peca = vao.CADPurlin.id_tirante;
            this.Multiline = multiline;
            this.VaoObra = vao;

            this.Offset = vao.CADPurlin.TirantesOffSet;

            this.Suporte = vao.CADPurlin.TirantesSuporte;
            this.SetPeca(vao.CADPurlin.GetTirantePadrao());

            this.Origem_Direita = this.Multiline.Inicio;
            this.Origem_Esquerda = this.Multiline.Fim;
        }
    }



}
