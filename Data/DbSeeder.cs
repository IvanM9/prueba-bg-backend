using bg_backend.Common;
using bg_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace bg_backend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var admin = new User
        {
            Email = "admin@tienda.com",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            Role = "Admin"
        };
        var customer = new User
        {
            Email = "cliente@tienda.com",
            PasswordHash = PasswordHasher.Hash("Cliente123!"),
            Role = "Customer"
        };
        context.Users.AddRange(admin, customer);

        var products = new List<Product>
        {
            new() { Name = "Laptop HP Pavilion 15", Code = "ELEC-001", Category = "Electrónica", Price = 899.99m, Stock = 10 },
            new() { Name = "Mouse Logitech MX Master 3", Code = "ELEC-002", Category = "Electrónica", Price = 99.99m, Stock = 25 },
            new() { Name = "Teclado Mecánico Keychron K2", Code = "ELEC-003", Category = "Electrónica", Price = 79.99m, Stock = 0 }, // AGOTADO
            new() { Name = "Monitor LG UltraWide 29\"", Code = "ELEC-004", Category = "Electrónica", Price = 299.99m, Stock = 2 }, // STOCK BAJO
            new() { Name = "Camiseta Algodón Básica", Code = "ROPA-001", Category = "Ropa", Price = 19.99m, Stock = 50 },
            new() { Name = "Jeans Levi's 501", Code = "ROPA-002", Category = "Ropa", Price = 59.99m, Stock = 30 },
            new() { Name = "Zapatillas Nike Air Max", Code = "ROPA-003", Category = "Ropa", Price = 129.99m, Stock = 15 },
            new() { Name = "El Quijote - Cervantes", Code = "LIBR-001", Category = "Libros", Price = 14.99m, Stock = 40 },
            new() { Name = "Clean Code - Robert C. Martin", Code = "LIBR-002", Category = "Libros", Price = 34.99m, Stock = 20 },
            new() { Name = "Café Colombiano 500g", Code = "ALIM-001", Category = "Alimentación", Price = 12.99m, Stock = 100 },
            new() { Name = "Chocolate 70% Cacao 100g", Code = "ALIM-002", Category = "Alimentación", Price = 4.99m, Stock = 3 }, // STOCK BAJO
            new() { Name = "Auriculares Sony WH-1000XM5", Code = "ELEC-005", Category = "Electrónica", Price = 349.99m, Stock = 8 },
        };
        context.Products.AddRange(products);

        await context.SaveChangesAsync();
    }
}
