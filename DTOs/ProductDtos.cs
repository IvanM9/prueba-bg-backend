namespace bg_backend.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Code,
    string Category,
    decimal Price,
    int Stock,
    bool IsActive
);

public record ProductFilter(
    string? Name,
    string? Code,
    string? Category
);
