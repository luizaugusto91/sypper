using Sypper.Domain.Application.Enumerators;
using System;

namespace Sypper.Domain.Infra.Data
{
    public class FieldsFilterModel
    {
        public string name { get; set; }
        public TypeCode type { get; set; }
        public TypeFilterEnum method { get; set; }
        public string value { get; set; }
    }
}
