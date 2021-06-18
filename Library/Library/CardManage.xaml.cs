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
    public sealed partial class CardManage : Page
    {
        public CardManage()
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
            NavView.SelectedItem = NavView.MenuItems[6];
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

        private async void DisplayQueryResult(int signal, int cardNo)
        {
            ContentDialog Dialog = new ContentDialog
            {
                Title = "",
                Content = "",
                CloseButtonText = "Ok"
            };

            switch (signal)
            {
                case 0:
                    Dialog.Title = "查询失败";
                    Dialog.Content = "请输入借书证编号。";
                    break;
                case 1:
                    Dialog.Title = "注册成功！";
                    Dialog.Content = "借书证编号为：" + cardNo.ToString() + "。";
                    break;
                case 2:
                    Dialog.Title = "查询失败";
                    Dialog.Content = "未找到对应借书证信息。";
                    break;
                case 3:
                    Dialog.Title = "注册失败";
                    Dialog.Content = "请完整输入信息。";
                    break;
                case 4:
                    Dialog.Title = "注销失败";
                    Dialog.Content = "当前借书证仍有未归还图书记录！";
                    break;
                case 5:
                    Dialog.Title = "注销成功！";
                    break;
            }

            ContentDialogResult result = await Dialog.ShowAsync();
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
                                        DisplayQueryResult(2, -1);
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

        private void RegisterButton(object sender, RoutedEventArgs e)
        {
            if (RegName.Text == "" || RegDept.Text == "" || RegType.SelectedValue == null)
                DisplayQueryResult(3, -1);
            else
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
                                int currentCardID = 0;
                                cmd.CommandText = "select cardID from cards";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.GetInt32(0) > currentCardID)
                                            currentCardID = reader.GetInt32(0);
                                    }
                                }
                                currentCardID++;
                                cmd.CommandText = "insert into cards values (" + currentCardID.ToString()
                                                + ", '" + RegName.Text + "', '" + RegDept.Text + "', '"
                                                + RegType.SelectedValue.ToString() + "')";
                                cmd.ExecuteNonQuery();
                                DisplayQueryResult(1, currentCardID);
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

        private void DeleteButton(object sender, RoutedEventArgs e)
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
                                        DisplayQueryResult(2, -1);
                                        return;
                                    }
                                }
                                cmd.CommandText = "select recordID from records where cardID = '"
                                                + ReturnCardID.Text + "'";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        DisplayQueryResult(4, -1);
                                        return;
                                    }
                                }
                                cmd.CommandText = "delete from cards where cardID = '"
                                                + ReturnCardID.Text + "'";
                                cmd.ExecuteNonQuery();
                                DisplayQueryResult(5, -1);
                                return;
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
            else return;
        }
    }
}
