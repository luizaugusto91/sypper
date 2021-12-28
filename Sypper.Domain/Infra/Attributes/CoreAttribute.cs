namespace Sypper.Domain.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class Core : Attribute
    {
        public bool PrimaryKey { get; set; }
        public bool Restrict { get; set; }
        public bool Index { get; set; }
        public bool Exclude { get; set; }
        public bool Ignore { get; set; }
    }
}
