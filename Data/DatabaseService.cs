using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DEBA.StockApp.Data
{
    public class DatabaseService : IDatabaseService
    {
        public string DatabasePath { get; }

        private readonly string _schemaSqlPath;

        public DatabaseService(string? databasePath = null)
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(baseDir, "DEBA");
            Directory.CreateDirectory(appDir);

            DatabasePath = databasePath ?? Path.Combine(appDir, "deba_stock.db");
            _schemaSqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "schema.sql");
        }

        public async Task InitializeAsync()
        {
            var dir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var connectionString = new SqliteConnectionStringBuilder { DataSource = DatabasePath, Cache = SqliteCacheMode.Shared }.ToString();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // Recommended pragmas for local single-user app
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";
                await pragma.ExecuteNonQueryAsync();
            }

            string sql = await LoadSchemaSqlAsync();
            if (string.IsNullOrWhiteSpace(sql)) return;

            // Execute the schema in a single command if possible
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> LoadSchemaSqlAsync()
        {
            // Prefer file next to executable (Data/schema.sql), fallback to embedded resource
            if (File.Exists(_schemaSqlPath))
            {
                return await File.ReadAllTextAsync(_schemaSqlPath, Encoding.UTF8);
            }

            var asm = Assembly.GetExecutingAssembly();
            var resourceName = "DEBA.StockApp.Data.schema.sql";
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null) return string.Empty;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
