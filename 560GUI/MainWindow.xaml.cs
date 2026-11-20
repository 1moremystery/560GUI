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

        // Shows the clicked menu item's header (e.g. "Top 10 Customers") as the current report title
        void SetReportTitle(object sender)
        {
            if (sender is MenuItem item)
            {
                uxReportTitle.Text = item.Header?.ToString();
            }
        }

        // Selected dates from the DatePickers; falls back to the old hardcoded range if a picker is cleared
        DateTime StartDate => uxStartDate.SelectedDate ?? new DateTime(2000, 1, 1);
        DateTime StopDate => uxStopDate.SelectedDate ?? new DateTime(2100, 12, 31);

        private void ViewAllCustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllCustomers();
            SetReadOnly();
            SetReportTitle(sender);
        }

        private void ViewTop10CustomersClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            //uxDataGrid.ItemsSource = controller.GetTop10Customers();
            uxDataGrid.ItemsSource = controller.GetTop10Customers(StartDate, StopDate);
            SetReadOnly();
            SetReportTitle(sender);
        }

        private void ViewAllProductsClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            uxDataGrid.ItemsSource = controller.GetAllProducts();
            SetReadOnly();
            SetReportTitle(sender);
        }

        private void ViewProductSalesClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            //uxDataGrid.ItemsSource = controller.GetAllProductSales();
            uxDataGrid.ItemsSource = controller.GetAllProductSales(StartDate, StopDate);
            SetReadOnly();
            SetReportTitle(sender);
        }

        private void ViewBestSellingBySupplierClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            //uxDataGrid.ItemsSource = controller.GetAllSupplierProdSold();
            uxDataGrid.ItemsSource = controller.GetAllSupplierProdSold(StartDate, StopDate);
            SetReadOnly();
            SetReportTitle(sender);
        }

        private void ViewMonthlySalesProductTypeClick(object sender, RoutedEventArgs e)
        {
            uxDataGrid.CancelEdit();
            //uxDataGrid.ItemsSource = controller.GetMonthlyProductTypeSales();
            uxDataGrid.ItemsSource = controller.GetMonthlyProductTypeSales(StopDate);
            SetReadOnly();
            SetReportTitle(sender);
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