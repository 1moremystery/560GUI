using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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

        /// <summary>
        /// Runs a query and shows the result in the grid; shows a message box on database errors.
        /// </summary>
        private async Task ShowAsync(Task<DataTable> query)
        {
            try
            {
                uxDataGrid.ItemsSource = (await query).DefaultView;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        // Dates from the pickers (fall back to the last year if one is cleared)
        private DateTime StartDate => uxStartDate.SelectedDate ?? DateTime.Today.AddYears(-1);
        private DateTime StopDate => uxStopDate.SelectedDate ?? DateTime.Today;

        // ---- View menu ----
        // Each one shows the given table in its entirety
        private async void ViewCustomersClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetCustomers());
        }


        private async void ViewProductsClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.TestQuery());
        }

        private async void ViewSuppliersClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetSuppliers());
        }

        // ---- Reports menu ----
        // These are the queries, they each have their own menu button to click to immediately show
        // MainController.cs defines the methods that hold the queries
        private async void ProductSalesClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetProductSales(StartDate, StopDate));
        }

        private async void TopCustomersClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetTopCustomers(StartDate, StopDate));
        }

        private async void BestSellingBySupplierClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetBestSellingProductsBySupplier(StartDate, StopDate));
        }

        private async void MonthlyTypeSalesClick(object sender, RoutedEventArgs e)
        {
            await ShowAsync(controller.GetMonthlyProductTypeSales(StopDate));
        }

        // Add a Customer
        private void AddCustomerClick(object sender, RoutedEventArgs e)
        {

        }

        // Add a Product
        private void AddProductClick(object sender, RoutedEventArgs e)
        {

        }

        // Runs a test query to show the database is connected.
        // Lists the products and a popup of how many items found
        private async void debugClick(object sender, RoutedEventArgs e)
        {
            //object[,] data = await controller.TestQuery();
            ////for(int row = 0; row < data.GetLength(0); row++)
            ////{
            ////    for(int col = 0; col < data.GetLength(1); col++)
            ////    {
            ////        object? item = data[row, col];
            ////        if (item == null) continue;

            ////    }
            //
            ////}
            //    try
            //    {
            //        var t = await controller.RunAsync("SELECT COUNT(*) AS Products FROM dbo.Products", false);
            //        MessageBox.Show($"Connected. {t.Rows[0][0]} products found.");
            //    }
            //    catch (SqlException ex)
            //    {
            //        MessageBox.Show("Database error: " + ex.Message);
            //    }

            try
            {
                var table = await controller.TestQuery();
                uxDataGrid.ItemsSource = table.DefaultView;
                MessageBox.Show($"Connected. {table.Rows.Count} products found.");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }
    }
}