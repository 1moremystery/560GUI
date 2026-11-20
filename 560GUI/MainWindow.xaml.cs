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

        private void ViewAllCustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.ItemsSource = controller.GetAllCustomers();
        }

        private void ViewTop10CustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.ItemsSource = controller.GetTop10Customers();
        }

        private void ViewAllProductsClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.ItemsSource = controller.GetAllProducts();
        }

        private void ViewProductSalesClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.ItemsSource = controller.GetAllProductSales();
        }

        private void ViewBestSellingBySupplierClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void ViewMonthlySalesProductTypeClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddCustomerClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddProductClick(object sender, RoutedEventArgs e)
        {

        }

        private async void debugClick(object sender, RoutedEventArgs e)
        {
            object[] data = await controller.TestQuery();
            uxDataGrid.ItemsSource = data;
            //{
            //    for(int col = 0; col < data.GetLength(1); col++)
            //    {
            //        object? item = data[row, col];
            //        if (item == null) continue;

            //    }
                
            //}
        }

        
    }
}