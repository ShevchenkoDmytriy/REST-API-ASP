
using System.Text.RegularExpressions;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Product> products = new List<Product>
{
    new() { Id = Guid.NewGuid().ToString(), Name = "Product 1", Description = "Description 1", Price = 100 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Product 2", Description = "Description 2", Price = 200 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Product 3", Description = "Description 3", Price = 300 }
};

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;

    string expressinForGuid = @"^/api/products/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/products" && request.Method == "GET")
    {
        await GetAllProducts(response);
    }
    else if (Regex.IsMatch(path, expressinForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetProduct(id, response);
    }
    else if (path == "/api/products" && request.Method == "POST")
    {
        await CreateProduct(response, request);
    }
    else if (path == "/api/products" && request.Method == "PUT")
    {
        await UpdateProduct(response, request);
    }
    else if (Regex.IsMatch(path, expressinForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteProduct(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }

});

app.Run();

async Task GetAllProducts(HttpResponse response)
{
    await response.WriteAsJsonAsync(products);
}

async Task GetProduct(string? id, HttpResponse response)
{
    Product? product = products.FirstOrDefault(x => x.Id == id);
    if (product != null)
    {
        await response.WriteAsJsonAsync(product);
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        await response.WriteAsJsonAsync(new { message = "Product not found" });
    }
}

async Task DeleteProduct(string? id, HttpResponse response)
{
    Product? product = products.FirstOrDefault(x => x.Id == id);
    if (product != null)
    {
        products.Remove(product);
        await response.WriteAsJsonAsync(product);
    }
    else
    {
        response.StatusCode = StatusCodes.Status404NotFound;
        await response.WriteAsJsonAsync(new { message = "Product not found" });
    }
}

async Task CreateProduct(HttpResponse response, HttpRequest request)
{
    try
    {
        var product = await request.ReadFromJsonAsync<Product>();
        if (product != null)
        {
            product.Id = Guid.NewGuid().ToString();
            products.Add(product);
            await response.WriteAsJsonAsync(product);
        }
        else
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            await response.WriteAsJsonAsync(new { message = "Invalid value" });
        }
    }
    catch (Exception)
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsJsonAsync(new { message = "Invalid value" });
    }
}

async Task UpdateProduct(HttpResponse response, HttpRequest request)
{
    try
    {
        Product? productData = await request.ReadFromJsonAsync<Product>();
        if (productData != null)
        {
            var product = products.FirstOrDefault(p => p.Id == productData.Id);
            if (product != null)
            {
                product.Name = productData.Name;
                product.Description = productData.Description;
                product.Price = productData.Price;
                await response.WriteAsJsonAsync(product);
            }
            else
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new { message = "Product not found" });
            }
        }
    }
    catch (Exception)
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
        await response.WriteAsJsonAsync(new { message = "Invalid value" });
    }
}

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

