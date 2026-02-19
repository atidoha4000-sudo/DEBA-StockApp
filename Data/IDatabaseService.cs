using System.Threading.Tasks;

namespace DEBA.StockApp.Data
{
    public interface IDatabaseService
    {
        string DatabasePath { get; }
        Task InitializeAsync();
    }
}
