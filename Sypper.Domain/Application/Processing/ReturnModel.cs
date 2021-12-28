using Sypper.Domain.Application.Enumerators;

namespace Sypper.Domain.Generico.Processing
{
    public class ReturnModel<T>
    {
        public ReturnEnum tipo { get; set; } = ReturnEnum.NoProccess;
        public string mensagem { get; set; } = string.Empty;
        public ErrorModel erro { get; set; } = new ErrorModel();
        public T retorno { get; set; } = default(T);

        public ReturnModel()
        {
            tipo = ReturnEnum.NoProccess;
        }

        public bool Validate()
        {
            return tipo == ReturnEnum.Success ? true : false;
        }

        public void Success(string Mensagem, T Retorno)
        {
            this.tipo = ReturnEnum.Success;
            this.mensagem = Mensagem;
            this.retorno = Retorno;
        }

        public void Fail(string Mensagem, ErrorModel Erro = null)
        {
            this.tipo = ReturnEnum.Fail;
            this.mensagem = Mensagem;
            this.erro = Erro;
            if (Erro != null)
            {
                Console.WriteLine($"FALHA: {Erro.mensagem}. DETALHES: {Erro.detalhes}. STACK: {Erro.stack}");
            }
        }

        public void Fail(string Mensagem, T Retorno, ErrorModel Erro = null)
        {
            this.tipo = ReturnEnum.Fail;
            this.mensagem = Mensagem;
            this.erro = Erro;
            this.retorno = Retorno;
            if (Erro != null)
            { 
                Console.WriteLine($"FALHA: {Erro.mensagem}. DETALHES: {Erro.detalhes}. STACK: {Erro.stack}"); 
            }
        }

        public void Error(string Mensagem, ErrorModel Erro)
        {
            this.tipo = ReturnEnum.Error;
            this.mensagem = Mensagem;
            this.erro = Erro;
            Console.WriteLine($"ERRO: {Erro.mensagem}. DETALHES: {Erro.detalhes}. STACK: {Erro.stack}");
        }        
    }
}
