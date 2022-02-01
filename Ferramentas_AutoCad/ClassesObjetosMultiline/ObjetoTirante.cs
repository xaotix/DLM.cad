namespace DLM.cad
{
    public class ObjetoTirante :ObjetoMultiLineBase
    {
        public override string ToString()
        {
            return this.Nome;
        }

        public double Offset { get; set; } = 0;

        public ObjetoTirante(CADMline multiline,  VaoObra vao)
        {
            this.Grade = vao.Grade;
            this.id_peca = Core.CADPurlin.id_tirante;
            this.Multiline = multiline;
            this.VaoObra = vao;

            this.Offset = Core.CADPurlin.TirantesOffSet;

            this.Suporte = Core.CADPurlin.TirantesSuporte;
            this.SetPeca(Core.CADPurlin.GetTirantePadrao());

            this.Origem_Direita = this.Multiline.Inicio;
            this.Origem_Esquerda = this.Multiline.Fim;
        }
    }



}
