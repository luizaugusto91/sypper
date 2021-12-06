using Sypper.Domain.Application.Enumerators;

namespace Sypper.Domain.Application.Notification
{
    public class HtmlModalModel
    {
        public HtmlClassEnum classe { get; set; }
        public MessageEnum type { get; set; }
        public string conteudo { get; set; }
        public bool visivel { get; set; }

        public HtmlModalModel()
        {
            visivel = false;
            type = MessageEnum.alert;
        }

        public HtmlModalModel(MessageEnum Tipo, string Conteudo)
        {
            this.visivel = true;
            this.type = Tipo;
            this.conteudo = Conteudo;
        }

        private string GetHtmlClass()
        {
            string result = string.Empty;
            try
            {
                switch (classe)
                {
                    case HtmlClassEnum.info:
                        result = "";
                        break;
                    case HtmlClassEnum.alert:
                        result = "";
                        break;
                    case HtmlClassEnum.success:
                        result = "";
                        break;
                    case HtmlClassEnum.error:
                        result = "";
                        break;
                    default:
                        result = "";
                        break;
                }
            }
            catch (Exception)
            {

            }
            return result;
        }

        public string GetHtmlMessage()
        {
            string result = string.Empty;
            try
            {
                switch (type)
                {
                    case MessageEnum.alert:
                        result = "";
                        break;
                    default:
                        result = "";
                        break;
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}
