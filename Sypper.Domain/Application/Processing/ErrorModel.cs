namespace Sypper.Domain.Generico.Processing
{
    public class ErrorModel
    {
        public int codigo { get; set; } = -1;
        public string mensagem { get; set; } = string.Empty;
        public string detalhes { get; set; } = string.Empty;
        public string stack { get; set; } = string.Empty;

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
