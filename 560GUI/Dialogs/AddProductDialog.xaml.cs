using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _560GUI.Dialogs
{
    /// <summary>
    /// Interaction logic for AddProductDialog.xaml
    /// </summary>
    public partial class AddProductDialog : Window
    {
        public string SupplierName { get => (string)uxSupplierBox.SelectedItem; }
        public string BrandName { get => (string)uxBrandBox.SelectedItem; }
        public string ProductTypeName { get => (string)uxProductTypeBox.SelectedItem; }
        public string ProductName { get => uxNameBox.Text; }
        public int Quantity { get; private set; }
        public decimal SellPrice { get; private set; }
        public decimal BulkPrice { get; private set; }
        
        MainController controller;
        public AddProductDialog(MainController ctrl)
        {
            InitializeComponent();
            this.controller = ctrl;

            SetUpDropDowns();
        }

        void SetUpDropDowns()
        {
            List<string> supplier;
            List<string> brands;
            List<string> productTypes;
            controller.GetSuppliersBrandProductType(out supplier, out brands, out productTypes);
            
            uxSupplierBox.ItemsSource = supplier;
            uxBrandBox.ItemsSource = brands;
            uxProductTypeBox.ItemsSource = productTypes;

        }

        bool Verify()
        {
            if( uxSupplierBox.SelectedIndex < 0 || uxBrandBox.SelectedIndex < 0 || uxProductTypeBox.SelectedIndex < 0 || string.IsNullOrEmpty(uxNameBox.Text)) return false;
            if (!int.TryParse(uxQuantity.Text, out int q) || !decimal.TryParse(uxBulkPrice.Text,out decimal bp) || !decimal.TryParse(uxSellPrice.Text, out decimal sp)) return false;
            Quantity = Math.Abs(q);
            BulkPrice = Math.Round(Math.Abs(bp),2);
            SellPrice = Math.Round(Math.Abs(sp),2);
            return true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkayClick(object sender, RoutedEventArgs e)
        {
            if (Verify()) DialogResult = true;
            else MessageBox.Show("Some values are missing or malformed");
        }
    }
}
