using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Library
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LendBook : Page
    {
        public LendBook()
        {
            this.InitializeComponent();
            InventoryList.ItemsSource = GetBooks((App.Current as App).ConnectionString, "select * from books");
            bookTypes = GetBookTypes((App.Current as App).ConnectionString);
        }

        ObservableCollection<string> bookTypes;

        public ObservableCollection<string> GetBookTypes(string connectionString)
        {
            var bookTypeRec = new ObservableCollection<string>();
            string GetBookTypeQuery = "select distinct type from books";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetBookTypeQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(0))
                                        bookTypeRec.Add(reader[0].ToString());
                                }
                            }
                        }
                    }
                }
                bookTypeRec.Add("");
                return bookTypeRec;
            }
            catch (Exception eSql)
            {
                throw eSql;
            }
        }

        public ObservableCollection<BookRecord> GetBooks(string connectionString, string GetBooksQuery)
        {
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
                            cmd.CommandText = GetBooksQuery;
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
                App.DisplaySqlError(eSql);
                return null;
            }
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
            NavView.SelectedItem = NavView.MenuItems[4];
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBooks(object sender, RoutedEventArgs e)
        {
            string query = "select * from books";
            bool hasCondition = false;
            if (searchOnlyHave.IsChecked == true)
            {
                query += hasCondition ? " and" : " where";
                query += " storageNum > 0";
                hasCondition = true;
            }
            if (SearchBookID.Text != "")
            {
                query += hasCondition ? " and" : " where";
                query += " bookID = '" + SearchBookID.Text + "'";
                hasCondition = true;
            }
            if (SearchType.SelectedValue != null && SearchType.SelectedValue.ToString().Length != 0)
            {
                query += hasCondition ? " and" : " where";
                query += " type = '" + SearchType.SelectedValue.ToString() + "'";
                hasCondition = true;
            }
            if (SearchBookName.Text != "")
            {
                query += hasCondition ? " and" : " where";
                query += " bookName like '" + SearchBookName.Text + "'";
                hasCondition = true;
            }

            ObservableCollection<BookRecord> bookRec = GetBooks((App.Current as App).ConnectionString, query);
            bookTypes = GetBookTypes((App.Current as App).ConnectionString);
            if (SearchSorting.SelectedValue != null)
                switch (SearchSorting.SelectedValue.ToString())
                {
                    case "图书编号":
                        InventoryList.ItemsSource =
                            new ObservableCollection<BookRecord>
                                (bookRec.OrderBy(item => item.BookID));
                        break;
                    case "单价":
                        InventoryList.ItemsSource =
                            new ObservableCollection<BookRecord>
                                (bookRec.OrderBy(item => item.Price));
                        break;
                    case "出版年份":
                        InventoryList.ItemsSource =
                            new ObservableCollection<BookRecord>
                                (bookRec.OrderBy(item => item.publishYear));
                        break;
                    case "存量":
                        InventoryList.ItemsSource =
                            new ObservableCollection<BookRecord>
                                (bookRec.OrderBy(item => item.StorageNum));
                        break;
                }
            else
            {
                InventoryList.ItemsSource = bookRec;
            }
        }

        private async void DisplayWrongLend(int signal)
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "借阅失败",
                Content = "",
                CloseButtonText = "Ok"
            };

            switch (signal)
            {
                case 1:
                    WrongLend.Content = "请输入需要借阅的图书编号。";
                    break;
                case 2:
                    WrongLend.Content = "请输入借阅证编号。";
                    break;
                case 3:
                    WrongLend.Content = "请输入需要借阅的图书编号和借阅证编号。";
                    break;
                case 4:
                    WrongLend.Content = "未找到对应书籍。";
                    break;
                case 5:
                    WrongLend.Content = "所需要的图书没有库存。";
                    break;
                case 6:
                    WrongLend.Content = "未找到对应借书证信息。";
                    break;
            }

            ContentDialogResult result = await WrongLend.ShowAsync();
        }

        private async void DisplaySuccessLend()
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "借阅成功！",
                Content = "",
                CloseButtonText = "Ok"
            };
            

            ContentDialogResult result = await WrongLend.ShowAsync();
        }

        private void LendBooks(object sender, RoutedEventArgs e)
        {
            int errorSignal = 0;
            if (LendBookID.Text == "")
                errorSignal += 1;
            if (LendCardID.Text == "")
                errorSignal += 2;
            if (errorSignal != 0)
            {
                DisplayWrongLend(errorSignal);
                return;
            }

            int currentNum;

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select storageNum from books where bookID = '"
                                            + LendBookID.Text + "'";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    errorSignal = 4;
                                    DisplayWrongLend(errorSignal);
                                    return;
                                }
                                else
                                {
                                    currentNum = reader.GetInt32(0);
                                    if (currentNum <= 0)
                                    {
                                        errorSignal = 5;
                                        DisplayWrongLend(errorSignal);
                                        return;
                                    }
                                }
                            }
                        }
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select cardID from cards where cardID = '"
                                            + LendCardID.Text + "'";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    errorSignal = 6;
                                    DisplayWrongLend(errorSignal);
                                    return;
                                }
                            }
                        }
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "update books set storageNum = storageNum - 1 where bookID = '"
                                            + LendBookID.Text + "'";
                            cmd.ExecuteNonQuery();
                            
                            int currentRecNum = 0;
                            cmd.CommandText = "select recordID from records";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (reader.GetInt32(0) > currentRecNum)
                                        currentRecNum = reader.GetInt32(0);
                                }
                            }
                            currentRecNum++;
                            cmd.CommandText = "insert into records values (" + currentRecNum.ToString()
                                            + ", " + LendCardID.Text + ", " + LendBookID.Text + ", "
                                            + "GETDATE(), null, "
                                            + (App.Current as App).adminID.ToString() + ")";
                            cmd.ExecuteNonQuery();
                            SearchBooks(sender, e);
                            DisplaySuccessLend();
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                App.DisplaySqlError(eSql);
                return;
            }
        }
    }
}
