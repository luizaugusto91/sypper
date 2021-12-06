using Sypper.Domain.Application.Enumerators;

namespace Sypper.Domain.Application.Notification
{
    public class HtmlMessageModel
    {
        public HtmlClassEnum classe { get; set; }
        public MessageEnum type { get; set; }
        public string titulo { get; set; }
        public string conteudo { get; set; }
        public bool visivel { get; set; }

        public HtmlMessageModel()
        {
            visivel = false;
            type = MessageEnum.alert;
            titulo = string.Empty;
        }
        
        public HtmlMessageModel(string Titulo)
        {
            visivel = false;
            type = MessageEnum.alert;
            titulo = Titulo;
        }

        public HtmlMessageModel(MessageEnum Tipo, string Titulo, string Conteudo)
        {
            this.visivel = true;
            this.type = Tipo;
            this.conteudo = Conteudo;
            this.titulo = Titulo;
        }

        public HtmlMessageModel(HtmlClassEnum Classe, string Titulo, string Conteudo)
        {
            this.visivel = true;
            this.classe = Classe;
            this.conteudo = Conteudo;
            this.titulo = Titulo;
        }

        private string GetHtmlClass() 
        {
            string result = string.Empty;
            try
            {
                switch (classe)
                {
                    case HtmlClassEnum.info:
                        result = "alert alert-info";
                        break;
                    case HtmlClassEnum.alert:
                        result = "alert alert-warning";
                        break;
                    case HtmlClassEnum.success:
                        result = "alert alert-success";
                        break;
                    case HtmlClassEnum.error:
                        result = "alert alert-danger";
                        break;
                    default:
                        result = "alert alert-primary";
                        break;
                }
            }
            catch (Exception)
            {
                result = "alert alert-primary";
            }
            return result;
        }

        public string GetHtmlMessage() 
        {
            string result = string.Empty;
            try
            {
                if (titulo == "")
                {
                    switch (classe)
                    {
                        case HtmlClassEnum.info:
                            result = $"<div class=\"{GetHtmlClass()}\" role=\"alert\"><strong>Informação! </strong>{conteudo}</div>";
                            break;
                        case HtmlClassEnum.alert:
                            result = $"<div class=\"{GetHtmlClass()}\" role=\"alert\"><strong>Alerta! </strong>{conteudo}</div>";
                            break;
                        case HtmlClassEnum.error:
                            result = $"<div class=\"{GetHtmlClass()}\" role=\"alert\"><strong>Erro! </strong>{conteudo}</div>";
                            break;
                        case HtmlClassEnum.success:
                            result = $"<div class=\"{GetHtmlClass()}\" role=\"alert\"><strong>Sucesso! </strong>{conteudo}</div>";
                            break;
                        default:
                            result = $"<div class=\"alert alert-primary\" role=\"alert\"><strong>Info! </strong>{conteudo}</div>";
                            break;
                    }
                }
                else 
                {
                    result = $"<div class=\"{GetHtmlClass()}\" role=\"alert\"><strong>{titulo} </strong>{conteudo}</div>";
                }
            }
            catch (Exception)
            {
                result = $"<div class=\"alert alert-primary\" role=\"alert\"><strong>Info! </strong>{conteudo}</div>";
            }
            return result;
        }
    }
}
