using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class LendRecord : INotifyPropertyChanged
    {
        private string BorrowTimeRecStr;
        public int RecordID { get; set; }
        public int BookIDRec { get; set; }
        public string BookNameRec { get; set; }
        public int CardIDRec { get; set; }
        public string BorrowerRec { get; set; }
        public string BorrowTimeRec
        {
            get { return BorrowTimeRecStr; }
            set { BorrowTimeRecStr = String.IsNullOrEmpty(value) ? "null" : value; }
        }
        public int AgentIDRec { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReturnBook : Page
    {
        public ReturnBook()
        {
            this.InitializeComponent();
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
            NavView.SelectedItem = NavView.MenuItems[5];
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        public ObservableCollection<LendRecord> GetRecords(string connectionString, string GetRecordsQuery)
        {
            var records = new ObservableCollection<LendRecord>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetRecordsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var record = new LendRecord();
                                    record.RecordID = reader.GetInt32(0);
                                    record.BookIDRec = reader.GetInt32(1);
                                    record.BookNameRec = reader[2].ToString();
                                    record.CardIDRec = reader.GetInt32(3);
                                    record.BorrowerRec = reader.GetString(4);
                                    record.BorrowTimeRec = reader[5].ToString();
                                    record.AgentIDRec = reader.GetInt32(6);
                                    records.Add(record);
                                }
                            }
                        }
                    }
                }
                return records;
            }
            catch (Exception eSql)
            {
                App.DisplaySqlError(eSql);
                return null;
            }
        }

        private async void DisplayWrongQuery()
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "查询失败",
                Content = "未找到对应借书证信息。",
                CloseButtonText = "Ok"
            };
            

            ContentDialogResult result = await WrongLend.ShowAsync();
        }

        private async void DisplayWrongReturn()
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "还书失败",
                Content = "所选图书证没有借阅对应书籍。",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await WrongLend.ShowAsync();
        }

        private async void DisplaySuccessReturn()
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "还书成功！",
                Content = "",
                CloseButtonText = "Ok"
            };
            
            ContentDialogResult result = await WrongLend.ShowAsync();
        }

        private void SearchRecordsButton(object sender, RoutedEventArgs e)
        {
            if (ReturnCardID.Text != "")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                    {
                        conn.Open();
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "select cardID from cards where cardID = '"
                                                + ReturnCardID.Text + "'";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        DisplayWrongQuery();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception eSql)
                {
                    App.DisplaySqlError(eSql);
                    return;
                }
                RecordList.ItemsSource = 
                    GetRecords((App.Current as App).ConnectionString,
                        "select recordID, records.bookID, bookName, records.cardID, name, borrowDate, agentID"
                        + " from records, books, cards"
                        + " where records.bookID = books.bookID and records.cardID = cards.cardID"
                        + " and due is null and records.cardID = " + ReturnCardID.Text);
            }
            else return;
        }

        private void ReturnBookButton(object sender, RoutedEventArgs e)
        {
            ObservableCollection<LendRecord> returnRec = GetRecords((App.Current as App).ConnectionString,
                        "select recordID, records.bookID, bookName, records.cardID, name, borrowDate, agentID"
                        + " from records, books, cards"
                        + " where records.bookID = books.bookID and records.cardID = cards.cardID"
                        + " and due is null and records.cardID = '" + ReturnCardID.Text
                        + "' and records.bookID = '" + ReturnBookID.Text + "'");
            if (returnRec == null || returnRec.Count() == 0)
            {
                DisplayWrongReturn();
                return;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "update records set due = GETDATE()"
                                            + " where recordID = "
                                            + returnRec[0].RecordID.ToString();
                            cmd.ExecuteNonQuery();
                            DisplaySuccessReturn();
                            RecordList.ItemsSource =
                                GetRecords((App.Current as App).ConnectionString,
                                    "select recordID, records.bookID, bookName, records.cardID, name, borrowDate, agentID"
                                    + " from records, books, cards"
                                    + " where records.bookID = books.bookID and records.cardID = cards.cardID"
                                    + " and due is null and records.cardID = " + ReturnCardID.Text);
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

