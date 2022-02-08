namespace Sypper.Domain.Application.Enumerators
{
    public enum TypeFilterEnum
    {
        Igual = 0,          // ==
        Contem = 1,         // LIKE %V%
        NaoContem = 2,      // NOT LIKE V
        Inicia = 3,         // LIKE V%
        Termina = 4,        // LIKE %V
        Maior = 5,          // >
        MaiorIgual = 6,     // >=
        Menor = 7,          // <
        MenorIgual = 8,     // <=
        Forade = 9,         // NOT IN ()
        Em = 10,            // IN ()
        Entre = 11          // BETWEEN        
    }
}
