using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Collections.ObjectModel;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Library
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void AdminLogin_AccessKeyInvoked(UIElement sender, AccessKeyInvokedEventArgs args)
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
            if ((App.Current as App).isLoggedIn)
                this.Frame.Navigate(typeof(AdminStatus));
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
            string GetAdmin = "select * from admin where adminID = '"
                              + AdminID.Text + "' and password = '"
                              + passwordBox.Password + "';";
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetAdmin;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    (App.Current as App).adminID = reader.GetInt32(0);
                                    (App.Current as App).adminName = reader[2].ToString();
                                    (App.Current as App).isLoggedIn = true;
                                    this.Frame.Navigate(typeof(AdminStatus));
                                    LoginTip.Text = "欢迎管理员 " + (App.Current as App).adminName;
                                    return;
                                }
                                else
                                {
                                    LoginTip.Text = "管理员 ID 或密码错误。";
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
            }
        }
    }
}
