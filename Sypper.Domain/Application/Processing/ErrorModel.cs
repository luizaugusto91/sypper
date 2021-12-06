namespace Sypper.Domain.Generico.Processing
{
    public class ErrorModel
    {
        public int codigo { get; set; }
        public string mensagem { get; set; }
        public string detalhes { get; set; }
        public string stack { get; set; }

        public ErrorModel() { }

        public ErrorModel(string Mensagem, string Detalhes, string Stack)
        {
            mensagem = Mensagem;
            detalhes = Detalhes;
            stack = Stack;
        }

        public ErrorModel(int Codigo, string Mensagem, string Detalhes, string Stack)
        {
            codigo = Codigo;
            mensagem = Mensagem;
            detalhes = Detalhes;
            stack = Stack;
        }
    }
}
