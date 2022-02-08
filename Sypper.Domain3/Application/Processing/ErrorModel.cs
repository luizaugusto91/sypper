namespace Sypper.Domain.Generico.Processing
{
    public class ErrorModel
    {
        public int codigo { get; set; } = -1;
        public string mensagem { get; set; } = string.Empty;
#pragma warning disable CS8632 // A anotação para tipos de referência anuláveis deve ser usada apenas em código em um contexto de anotações '#nullable'.
        public string? detalhes { get; set; } = string.Empty;
#pragma warning restore CS8632 // A anotação para tipos de referência anuláveis deve ser usada apenas em código em um contexto de anotações '#nullable'.

#pragma warning disable CS8632 // A anotação para tipos de referência anuláveis deve ser usada apenas em código em um contexto de anotações '#nullable'.
        public string? stack { get; set; } = string.Empty;
#pragma warning restore CS8632 // A anotação para tipos de referência anuláveis deve ser usada apenas em código em um contexto de anotações '#nullable'.

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
