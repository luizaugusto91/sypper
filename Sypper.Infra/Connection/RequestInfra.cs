using Sypper.Domain.Application.Enumerators;
using Sypper.Domain.Application.Processing;
using Sypper.Domain.Converter;
using Sypper.Domain.Generico.Processing;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sypper.Infra.Connection
{
    public static class RequestInfra
    {
        public static ReturnModel<IRestResponse> Request(string URL, Method Metodo, bool Autenticacao, string Token = "", string Body = "")
        {
            ReturnModel<IRestResponse> result = new ReturnModel<IRestResponse>();
            try
            {
                var client = new RestClient(URL);
                var request = new RestRequest();

                request.Method = Metodo;
                request.AddHeader("Accept", "application/json");
                request.Parameters.Clear();
                if (Autenticacao)
                {
                    request.AddParameter("Authorization", $"Bearer {Token}", ParameterType.HttpHeader);
                }
                if (Metodo != Method.GET && Metodo != Method.DELETE)
                {
                    request.AddParameter("application/json", Body, ParameterType.RequestBody);
                }
                request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);
                var response = client.Execute(request);

                result.Success("Consulta realizada com sucesso", response);
            }
            catch (HttpRequestException h)
            {
                result.Fail($"Ocorreu uma falha ao realizar a requisição. Erro: {h.InnerException} - {h.Message}", null);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Data.ToString(), stack = e.StackTrace };
                result.Error($"Ocorreu um erro ao realizar a requisição. Erro: {e.Message}", erro);
            }
            return result;
        }

        public static RequestModel<T> HTTPRequest<T>(string URL, Method Metodo, bool Autenticacao, string Token = "", string Body = "")
        {
            RequestModel<T> result = new RequestModel<T>();
            try
            {
                var requisicao = Request(URL, Metodo, Autenticacao, Token, Body);

                switch (requisicao.tipo)
                {
                    case ReturnEnum.Error:
                        {
                            result.Error(0, requisicao.mensagem, requisicao.erro);
                        }
                        break;
                    default:
                        {
                            switch (requisicao.retorno.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    {
                                        result.Fail(HttpStatusCode.NotFound, "End-point não encontrado.");
                                    }
                                    break;
                                case HttpStatusCode.BadRequest:
                                    {
                                        result.Fail(HttpStatusCode.BadRequest, "Falha na requisição.");
                                    }
                                    break;
                                case HttpStatusCode.BadGateway:
                                    {
                                        result.Fail(HttpStatusCode.BadGateway, "Falha no end-point.");
                                    }
                                    break;
                                case HttpStatusCode.Unauthorized:
                                    {
                                        if (Autenticacao)
                                        {
                                            result.Fail(HttpStatusCode.Unauthorized, "Falha na autenticação.");
                                        }
                                        else
                                        {
                                            result.Fail(HttpStatusCode.Unauthorized, "O end-point informado necessita de autenticação.");
                                        }
                                    }
                                    break;
                                case HttpStatusCode.GatewayTimeout:
                                    {
                                        result.Fail(HttpStatusCode.GatewayTimeout, "A requisição excedeu o tempo limite do processamento.");
                                    }
                                    break;
                                case HttpStatusCode.InternalServerError:
                                    {
                                        result.Fail(HttpStatusCode.InternalServerError, "O end-point solicitado apresentou um erro interno.");
                                    }
                                    break;
                                case HttpStatusCode.Forbidden:
                                    {
                                        result.Fail(HttpStatusCode.Forbidden, "Sem permissão para acessar o end-point informado.");
                                    }
                                    break;
                                // Sucesso
                                case HttpStatusCode.OK:
                                    {
                                        JsonSerializerOptions options = new JsonSerializerOptions();
                                        options.Converters.Add(new JsonStringEnumConverter());
                                        options.Converters.Add(new TimeSpanConverter());
                                        result.Success(HttpStatusCode.OK, "Requisição executada com sucesso.", JsonSerializer.Deserialize<T>(requisicao.retorno.Content, options));
                                    }
                                    break;
                                default:
                                    {
                                        result.Fail(requisicao.retorno.StatusCode, "Não foi possivel realizar a requisição ou processamento.");
                                    }
                                    break;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Data.ToString(), stack = e.StackTrace };
                result.Error(0, $"Ocorreu um erro ao realizar a requisição. Erro: {e.Message}", erro);
            }
            return result;
        }
    }
}
