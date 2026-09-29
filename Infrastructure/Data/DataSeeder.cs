using Core.Entities;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Reflection;

namespace Infrastructure.Data;

public class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        try
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if(!context.Brands.Any()){
                using (var readerBrands = new StreamReader(location + "/Data/Csv/brands.csv")){
                    using (var csvBrands = new CsvReader(readerBrands, CultureInfo.InvariantCulture)){
                        var brands = csvBrands.GetRecords<Brand>();
                        
                        await context.Database.OpenConnectionAsync();

                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Brands] ON");

                            context.Brands.AddRange(brands);
                            await context.SaveChangesAsync();
                        }
                        finally
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Brands] OFF");

                            await context.Database.CloseConnectionAsync();
                        }
                    }
                }
            }

            if(!context.Categories.Any()){
                using (var readerCategories = new StreamReader(location + "/Data/Csv/categories.csv")){
                    using (var csvCategories = new CsvReader(readerCategories,CultureInfo.InvariantCulture)){
                        var categories = csvCategories.GetRecords<Category>();

                        await context.Database.OpenConnectionAsync();
                        
                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Categories] ON");

                            context.Categories.AddRange(categories);
                            await context.SaveChangesAsync();
                        }
                        finally
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Categories] OFF");

                            await context.Database.CloseConnectionAsync();
                        }
                    }
                }
            }


            if(!context.Products.Any()){
                using (var readerProducts = new StreamReader(location + "/Data/Csv/products.csv")){
                    using (var csvProducts = new CsvReader(readerProducts,CultureInfo.InvariantCulture)){
                        var productsCsv = csvProducts.GetRecords<Product>();

                        await context.Database.OpenConnectionAsync();

                        List<Product> products = new List<Product>();
                        foreach(var item in productsCsv){
                            products.Add(new Product{
                                Id = item.Id,
                                Name = item.Name,
                                Price = item.Price,
                                CreatedAt = item.CreatedAt,
                                CategoryId = item.CategoryId,
                                BrandId = item.BrandId,                                                               
                            });
                        }

                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Products] ON");

                            context.Products.AddRange(products);
                            await context.SaveChangesAsync();
                        }
                        finally
                        {
                            await context.Database.ExecuteSqlRawAsync(
                                "SET IDENTITY_INSERT [Products] OFF");

                            await context.Database.CloseConnectionAsync();
                        }
                    }
                }
            }


        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
    }
}
