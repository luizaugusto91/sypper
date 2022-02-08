namespace Sypper.Domain.Application.Processing
{
    public class ParametersModel<T>
    {
        public string name { get; set; }
        public T value { get; set; }
    }
}
