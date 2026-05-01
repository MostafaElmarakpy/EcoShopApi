namespace EcoShopApi.Domain.Exceptions;

/// <summary>
/// Base class for all domain-level exceptions.
/// Provides structured error codes for better error handling.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; protected set; }

    protected DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message)
    {
        Code = code;
    }
}
