namespace GestionCommerciale.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"L'entité '{entityName}' ({key}) est introuvable.")
    {
    }
}
