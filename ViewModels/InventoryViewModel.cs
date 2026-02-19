using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DEBA.StockApp.Infrastructure;
using DEBA.StockApp.Models;
using DEBA.StockApp.Services;

namespace DEBA.StockApp.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private ObservableCollection<Product> _products = new();
        private Product? _selectedProduct;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public InventoryViewModel()
        {
            _productService = new ProductService();

            SelectedProduct = new Product(); // 🔥 IMPORTANT

            RefreshCommand = new RelayCommand(async (_) => await RefreshAsync());
            AddCommand = new RelayCommand(async (_) => await AddProductAsync());
            UpdateCommand = new RelayCommand(async (_) => await UpdateProductAsync(), (_) => SelectedProduct != null);
            DeleteCommand = new RelayCommand(async (_) => await DeleteProductAsync(), (_) => SelectedProduct != null);

            // Charger les produits au démarrage
            RefreshCommand.Execute(null);
        }

        private async System.Threading.Tasks.Task RefreshAsync()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des produits: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task AddProductAsync()
        {
            if (SelectedProduct == null)
            {
                System.Windows.MessageBox.Show("Veuillez remplir les informations du produit");
                return;
            }

            try
            {
                await _productService.AddAsync(SelectedProduct);
                await RefreshAsync();

                SelectedProduct = new Product(); // ✅ reset propre

                System.Windows.MessageBox.Show("Produit ajouté avec succès");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'ajout du produit: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task UpdateProductAsync()
        {
            if (SelectedProduct == null)
                return;

            try
            {
                await _productService.UpdateAsync(SelectedProduct);
                await RefreshAsync();
                SelectedProduct = new Product(); // ✅ reset propre
                System.Windows.MessageBox.Show("Produit mis à jour avec succès");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la mise à jour du produit: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer '{SelectedProduct.Name}' ?",
                "Confirmation",
                System.Windows.MessageBoxButton.YesNo);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                await _productService.DeleteAsync(SelectedProduct.ProductId);
                await RefreshAsync();
                SelectedProduct = new Product(); // ✅ reset propre
                System.Windows.MessageBox.Show("Produit supprimé avec succès");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la suppression du produit: {ex.Message}");
            }
        }
    }
}
