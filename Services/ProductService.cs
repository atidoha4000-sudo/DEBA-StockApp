using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.Sqlite;
using DEBA.StockApp.Models;

namespace DEBA.StockApp.Services
{
    public class ProductService
    {
        private readonly string _connectionString;

        public ProductService(string? connectionString = null)
        {
            connectionString ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DEBA",
                "deba_stock.db");
            
            _connectionString = $"Data Source={connectionString}";
        }

        public async Task AddAsync(Product product)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Products (SKU, Name, Description, UnitPrice, CostPrice, ReorderLevel, IsActive, CreatedAt, UpdatedAt)
                    VALUES (@sku, @name, @description, @unitPrice, @costPrice, @reorderLevel, @isActive, @createdAt, @updatedAt)";

                command.Parameters.AddWithValue("@sku", product.SKU ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@unitPrice", product.UnitPrice);
                command.Parameters.AddWithValue("@costPrice", product.CostPrice);
                command.Parameters.AddWithValue("@reorderLevel", product.ReorderLevel);
                command.Parameters.AddWithValue("@isActive", product.IsActive ? 1 : 0);
                command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(Product product)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Products 
                    SET SKU = @sku, Name = @name, Description = @description, 
                        UnitPrice = @unitPrice, CostPrice = @costPrice, 
                        ReorderLevel = @reorderLevel, IsActive = @isActive, UpdatedAt = @updatedAt
                    WHERE ProductId = @productId";

                command.Parameters.AddWithValue("@productId", product.ProductId);
                command.Parameters.AddWithValue("@sku", product.SKU ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@unitPrice", product.UnitPrice);
                command.Parameters.AddWithValue("@costPrice", product.CostPrice);
                command.Parameters.AddWithValue("@reorderLevel", product.ReorderLevel);
                command.Parameters.AddWithValue("@isActive", product.IsActive ? 1 : 0);
                command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(long productId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Products WHERE ProductId = @productId";
                command.Parameters.AddWithValue("@productId", productId);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ProductId, SKU, Name, Description, UnitPrice, CostPrice, ReorderLevel, IsActive, CreatedAt, UpdatedAt FROM Products ORDER BY Name";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new Product
                        {
                            ProductId = reader.GetInt64(0),
                            SKU = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            UnitPrice = reader.GetDecimal(4),
                            CostPrice = reader.GetDecimal(5),
                            ReorderLevel = reader.GetInt32(6),
                            IsActive = reader.GetInt32(7) == 1,
                            CreatedAt = reader.GetDateTime(8),
                            UpdatedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                        });
                    }
                }
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(long productId)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ProductId, SKU, Name, Description, UnitPrice, CostPrice, ReorderLevel, IsActive, CreatedAt, UpdatedAt FROM Products WHERE ProductId = @productId";
                command.Parameters.AddWithValue("@productId", productId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Product
                        {
                            ProductId = reader.GetInt64(0),
                            SKU = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            UnitPrice = reader.GetDecimal(4),
                            CostPrice = reader.GetDecimal(5),
                            ReorderLevel = reader.GetInt32(6),
                            IsActive = reader.GetInt32(7) == 1,
                            CreatedAt = reader.GetDateTime(8),
                            UpdatedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                        };
                    }
                }
            }

            return null;
        }
    }
}
