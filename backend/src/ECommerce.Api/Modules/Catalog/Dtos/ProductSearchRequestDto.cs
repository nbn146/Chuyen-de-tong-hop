namespace ECommerce.Api.Modules.Catalog;

public record ProductSearchRequestDto(
    string? Keyword,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Brand,
    int PageNumber = 1,
    int PageSize = 10,
    string? Sort = null
);
