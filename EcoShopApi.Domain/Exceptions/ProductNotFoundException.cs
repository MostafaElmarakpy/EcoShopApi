using EcoShopApi.Domain.Exceptions;

namespace EcoShopApi.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public int ProductId { get; }

    public ProductNotFoundException(int productId)
        : base($"Product with ID {productId} not found.", "PRODUCT_NOT_FOUND")
    {
        ProductId = productId;
    }
}
