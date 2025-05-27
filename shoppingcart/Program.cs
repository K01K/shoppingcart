using ShoppingBasket.Repository;
using ShoppingBasket.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repository registration
builder.Services.AddScoped<IBasketRepository, SqlBasketRepository>();
builder.Services.AddScoped<IProductLockRepository, SqlProductLockRepository>();

// Service registration
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<IProductService, LocalProductService>();

// HttpClient for external services
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();