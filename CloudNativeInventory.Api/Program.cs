using CloudNativeInventory.Api.Data;
using CloudNativeInventory.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Azure.Identity; // TODO (Del 4): Krävs för Key Vault

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // .NET 9 OpenAPI

// TODO (Del 4 i "Tips och förslag"): Konfigurera Azure Key Vault
// Använd Managed Identity för att hämta hemligheter i produktion.
if (!builder.Environment.IsDevelopment())
{
    var kvUrl = builder.Configuration["KeyVaultUrl"];
    if (!string.IsNullOrEmpty(kvUrl))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(kvUrl), new DefaultAzureCredential());
    }
}

// Vi använder InMemory-databas lokalt
builder.Services.AddDbContext<InventoryDbContext>(options =>
  options.UseInMemoryDatabase("InventoryDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seeda data (se till att vi inte dubblar om appen startas om i samma process)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

    if (!db.Products.Any())
    {
        db.Products.Add(new Product { Id = 1, Name = "Laptop", Price = 9999, StockQuantity = 10 });
        db.SaveChanges();
    }
}

app.Run();