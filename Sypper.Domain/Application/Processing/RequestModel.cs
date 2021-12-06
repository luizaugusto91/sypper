using Sypper.Domain.Application.Enumerators;
using Sypper.Domain.Generico.Processing;
using System.Net;

namespace Sypper.Domain.Application.Processing
{
    public class RequestModel<T>
    {
        public HttpStatusCode code { get; set; }
        public ReturnEnum tipo { get; set; }
        public string mensagem { get; set; }
        public ErrorModel erro { get; set; }
        public T retorno { get; set; }

        public RequestModel()
        {
            tipo = ReturnEnum.NoProccess;
        }

        public bool Validate()
        {
            if (tipo == ReturnEnum.Success && (code == HttpStatusCode.OK || code == HttpStatusCode.Created || code == HttpStatusCode.Accepted)) return true;
            else return false;
        }

        public void Success(HttpStatusCode Status, string Mensagem, T Retorno)
        {
            this.code = Status;
            this.tipo = ReturnEnum.Success;
            this.mensagem = Mensagem;
            this.retorno = Retorno;
        }

        public void Fail(HttpStatusCode Status, string Mensagem, ErrorModel Erro = null)
        {
            this.code = Status;
            this.tipo = ReturnEnum.Fail;
            this.mensagem = Mensagem;
            this.erro = Erro;
        }

        public void Fail(HttpStatusCode Status, string Mensagem, T Retorno, ErrorModel Erro = null)
        {
            this.code = Status;
            this.tipo = ReturnEnum.Fail;
            this.mensagem = Mensagem;
            this.erro = Erro;
            this.retorno = Retorno;
        }

        public void Error(HttpStatusCode Status, string Mensagem, ErrorModel Erro)
        {
            this.code = Status;
            this.tipo = ReturnEnum.Error;
            this.mensagem = Mensagem;
            this.erro = Erro;
        }
    }
}
