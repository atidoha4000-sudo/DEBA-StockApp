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
        private bool _isEditMode;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                UpdateEditMode();
                RaiseCanExecuteChanged();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        private RelayCommand _addCommandRef;
        private RelayCommand _updateCommandRef;
        private RelayCommand _deleteCommandRef;

        public InventoryViewModel()
        {
            _productService = new ProductService();

            ResetForm();

            _addCommandRef = new RelayCommand(async (_) => await AddProductAsync(), (_) => !IsEditMode);
            _updateCommandRef = new RelayCommand(async (_) => await UpdateProductAsync(), (_) => IsEditMode && SelectedProduct?.ProductId > 0);
            _deleteCommandRef = new RelayCommand(async (_) => await DeleteProductAsync(), (_) => IsEditMode && SelectedProduct?.ProductId > 0);

            AddCommand = _addCommandRef;
            UpdateCommand = _updateCommandRef;
            DeleteCommand = _deleteCommandRef;
            RefreshCommand = new RelayCommand(async (_) => await RefreshAsync());

            // Charger les produits au démarrage
            RefreshCommand.Execute(null);
        }

        private void ResetForm()
        {
            SelectedProduct = new Product { IsActive = true };
            IsEditMode = false;
        }

        private void UpdateEditMode()
        {
            IsEditMode = SelectedProduct?.ProductId > 0;
        }

        private void RaiseCanExecuteChanged()
        {
            _addCommandRef?.RaiseCanExecuteChanged();
            _updateCommandRef?.RaiseCanExecuteChanged();
            _deleteCommandRef?.RaiseCanExecuteChanged();
        }

        private bool ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                System.Windows.MessageBox.Show("Le nom du produit est obligatoire.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            if (product.UnitPrice <= 0)
            {
                System.Windows.MessageBox.Show("Le prix unitaire doit être supérieur à 0.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            if (product.CostPrice < 0)
            {
                System.Windows.MessageBox.Show("Le prix de coût ne peut pas être négatif.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            if (product.ReorderLevel < 0)
            {
                System.Windows.MessageBox.Show("Le niveau de réapprovisionnement ne peut pas être négatif.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            return true;
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
                System.Windows.MessageBox.Show($"Erreur lors du chargement des produits: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task AddProductAsync()
        {
            if (SelectedProduct == null)
            {
                System.Windows.MessageBox.Show("Veuillez remplir les informations du produit.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!ValidateProduct(SelectedProduct))
                return;

            try
            {
                await _productService.AddAsync(SelectedProduct);
                await RefreshAsync();
                ResetForm();
                System.Windows.MessageBox.Show("Produit ajouté avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'ajout du produit: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task UpdateProductAsync()
        {
            if (SelectedProduct == null || SelectedProduct.ProductId <= 0)
            {
                System.Windows.MessageBox.Show("Aucun produit sélectionné pour la modification.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!ValidateProduct(SelectedProduct))
                return;

            try
            {
                await _productService.UpdateAsync(SelectedProduct);
                await RefreshAsync();
                ResetForm();
                System.Windows.MessageBox.Show("Produit mis à jour avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la mise à jour du produit: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task DeleteProductAsync()
        {
            if (SelectedProduct == null || SelectedProduct.ProductId <= 0)
            {
                System.Windows.MessageBox.Show("Aucun produit sélectionné pour la suppression.", "Validation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer '{SelectedProduct.Name}' ?\n\nCette action est irréversible.",
                "Confirmation de suppression",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                await _productService.DeleteAsync(SelectedProduct.ProductId);
                await RefreshAsync();
                ResetForm();
                System.Windows.MessageBox.Show("Produit supprimé avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la suppression du produit: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
