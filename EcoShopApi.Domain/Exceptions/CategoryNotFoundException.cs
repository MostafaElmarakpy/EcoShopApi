namespace EcoShopApi.Domain.Exceptions;

public class CategoryNotFoundException : DomainException
{
    public int CategoryId { get; }

    public CategoryNotFoundException(int categoryId)
        : base($"Category with ID {categoryId} not found.", "CATEGORY_NOT_FOUND")
    {
        CategoryId = categoryId;
    }
}
