using System;
using System.Collections.Generic;
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
    public sealed partial class AddBook : Page
    {
        public AddBook()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

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
            NavView.SelectedItem = NavView.MenuItems[2];
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }
        bool once = false;
        private async void DisplayError(string msg, bool isSuccess = false)
        {
            ContentDialog WrongLend = new ContentDialog
            {
                Title = "错误",
                Content = msg,
                CloseButtonText = "Ok"
            };
            if (isSuccess)
            {
                WrongLend.Title = "添加成功！";
            }
            if (!once)
            {
                once = true;
                ContentDialogResult result = await WrongLend.ShowAsync();
                once = false;
            }
        }

        private void AddOneBook(object sender, RoutedEventArgs e)
        {
            CheckAndAdd(AddID.Text + "," + AddType.Text + ","
                      + AddName.Text + "," + AddPublisher.Text + ","
                      + AddYear.Text + "," + AddAuthor.Text + ","
                      + AddPrice.Text + "," + AddNum.Text, false);
        }

        private async void AddByFile(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".txt");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string filepath = file.Name;
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                using (StreamReader sr = new StreamReader(filepath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if(CheckAndAdd(line))
                            DisplayError("", true); 
                    }
                }  
            }
        }

        private bool CheckAndAdd(string line, bool mode = true)
        {
            int ID, year, num;
            decimal price;

            char[] seperator = { ',', ' ', '(', ')' };

            string[] arr;
            if (mode)
                arr = line.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            else
                arr = line.Split(',');

            if (arr.GetLength(0) != 8)
            {
                DisplayError("输入格式有误！信息个数不符。");
                return false;
            }
            
            try {ID = Convert.ToInt32(arr[0]);}
            catch {DisplayError("输入格式有误！书号应为整数。"); return false; }

            try
            {
                num = Convert.ToInt32(arr[7]);
                if (num <= 0)
                {
                    DisplayError("输入有误！数目应为正整数。");
                    return false;
                }
            }
            catch { DisplayError("输入格式有误！数目应为整数。"); return false; }

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "select bookID from books where bookID = '"
                                            + arr[0] + "'";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    goto next;
                                }
                            }
                            cmd.CommandText = "update books set totalNum " +
                                "= totalNum + " + arr[7] + ", storageNum = storageNum + " + arr[7]
                                + "where bookID = " + arr[0];
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                App.DisplaySqlError(eSql);
                return false;
            }
            if (mode) DisplayError("", true);
            return true;

            next:
            try { year = Convert.ToInt32(arr[4]); }
            catch { DisplayError("输入格式有误！出版年份应为整数。"); return false; }
            
            try
            {
                price = Convert.ToDecimal(arr[6]);
                if (price < 0)
                {
                    DisplayError("输入有误！价格应为正数。");
                    return false;
                }
            }
            catch { DisplayError("输入格式有误！价格应为有理数。"); return false; }

            if (arr[2] == "")
            {
                DisplayError("输入有误！请输入书名。");
                return false;
            }

            string query = "insert into books values (" + arr[0] + ", ";
            for (int i = 1; i <= 7; i++)
            {
                if (arr[i] == "" || arr[i] == "null")
                    query += "null, ";
                else
                    query += "'" + arr[i] + "', ";
            }
            query += "" + arr[7] + ")";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                App.DisplaySqlError(eSql);
                return false;
            }
            if (mode) DisplayError("", true);
            return true;
        }
    }
}
