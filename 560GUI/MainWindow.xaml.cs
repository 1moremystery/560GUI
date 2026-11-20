using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _560GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainController controller;
        public MainWindow()
        {
            InitializeComponent();
            controller = new MainController();
        }

        void SetReadOnly()
        {
            foreach (var item in uxDataGrid.Columns)
            {
                item.IsReadOnly = true;
            }
        }

        private void ViewAllCustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllCustomers();
            SetReadOnly();
        }

        private void ViewTop10CustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetTop10Customers();
            SetReadOnly();
        }

        private void ViewAllProductsClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllProducts();
            SetReadOnly();
        }

        private void ViewProductSalesClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllProductSales();
            SetReadOnly();
        }

        private void ViewBestSellingBySupplierClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllSupplierProdSold();
            SetReadOnly();
        }

        private void ViewMonthlySalesProductTypeClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetMonthlyProductTypeSales();
            SetReadOnly();
        }

        private void AddCustomerClick(object sender, RoutedEventArgs e)
        {
            controller.AddCustomer();
        }

        private void AddProductClick(object sender, RoutedEventArgs e)
        {
            controller.AddProduct();
        }
    }
}