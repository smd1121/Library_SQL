using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Library
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public class BookRecord : INotifyPropertyChanged
    {
        private string TypeString, PublisherString, AuthorString;
        public int BookID { get; set; }
        public string Type
        {
            get { return TypeString; }
            set { TypeString = String.IsNullOrEmpty(value) ? "null" : value; }
        }
        public string BookName { get; set; }
        public string Publisher
        {
            get { return PublisherString; }
            set { PublisherString = String.IsNullOrEmpty(value) ? "null" : value; }
        }
        public int? publishYear { get; set; }
        public string publishYearString { get { return (publishYear == null) ? "null" : publishYear.ToString(); } }

        public string Author
        {
            get { return AuthorString; }
            set { AuthorString = String.IsNullOrEmpty(value) ? "null" : value; }
        }
        public decimal? Price { get; set; }
        public string PriceString { get { return (Price == null) ? "null" : Price.ToString(); } }
        public int TotalNum { get; set; }
        public int StorageNum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class BookSearch : Page
    {
        public BookSearch()
        {
            this.InitializeComponent();
            InventoryList.ItemsSource = GetBooks((App.Current as App).ConnectionString);
        }

        public ObservableCollection<BookRecord> GetBooks(string connectionString)
        {
            const string GetProductsQuery = "select *" +
               " from books";

            var bookRecords = new ObservableCollection<BookRecord>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetProductsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var book = new BookRecord();
                                    book.BookID = reader.GetInt32(0);
                                    book.Type = reader[1].ToString();
                                    book.BookName = reader.GetString(2);
                                    book.Publisher = reader[3].ToString();
                                    book.publishYear = reader.GetInt32(4);
                                    book.Author = reader[5].ToString();
                                    book.Price = reader.GetDecimal(6);
                                    book.TotalNum = reader.GetInt32(7);
                                    book.StorageNum = reader.GetInt32(8);
                                    bookRecords.Add(book);
                                }
                            }
                        }
                    }
                }
                return bookRecords;
            }
            catch (Exception eSql)
            {
                throw eSql;
            }
            return null;
        }

        public void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            //Title.Text = args.InvokedItem.ToString();
            switch (args.InvokedItem)
            {
                case "管理员登录":
                    if ((App.Current as App).isLoggedIn)
                        this.Frame.Navigate(typeof(AdminStatus));
                    else
                        this.Frame.Navigate(typeof(MainPage));
                    break;
                case "图书入库":
                    if ((App.Current as App).isLoggedIn)
                        this.Frame.Navigate(typeof(AddBook));
                    else
                        this.Frame.Navigate(typeof(GuideLogin));
                    break;
                case "图书查询":
                    this.Frame.Navigate(typeof(BookSearch));
                    break;
                case "借书":
                    if ((App.Current as App).isLoggedIn)
                        this.Frame.Navigate(typeof(LendBook));
                    else
                        this.Frame.Navigate(typeof(GuideLogin));
                    break;
                case "还书":
                    if ((App.Current as App).isLoggedIn)
                        this.Frame.Navigate(typeof(ReturnBook));
                    else
                        this.Frame.Navigate(typeof(GuideLogin));
                    break;
                case "借书证管理":
                    if ((App.Current as App).isLoggedIn)
                        this.Frame.Navigate(typeof(CardManage));
                    else
                        this.Frame.Navigate(typeof(GuideLogin));
                    break;
            }
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
