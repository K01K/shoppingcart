using Microsoft.OpenApi.Models;
using ShoppingBasket.Repository;
using ShoppingBasket.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<IBasketRepository, SqlBasketRepository>();
builder.Services.AddSingleton<IProductService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var productServiceUrl = config.GetValue<string>("ProductServiceUrl");
    return new HttpProductService(productServiceUrl);
});
builder.Services.AddSingleton<BasketService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shopping Basket API",
        Version = "v1",
        Description = "API do zarz¹dzania koszykiem zakupowym"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Basket API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();