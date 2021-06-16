# Report

```SQL
use LibraryManager;

create table admin
(
	adminID		int not null,
	password	varchar(64) not null,
	name		varchar(32) not null,
	contact		varchar(64) not null,
	primary key (adminID)
);

create table books
(
	bookID		int not null,
    type		varchar(32),
    bookName	varchar(64) not null,
    publisher	varchar(64),
    publishYear	int,
    author		varchar(64),
    price		numeric(6, 2),
    totalNum	int not null,
    storageNum	int not null,
    primary key (bookID),
    check (totalNum >= storageNum),
    check (storageNum >= 0),
    check (price >= 0.0)
);

create table cards
(
	cardID		int not null,
    name		varchar(64) not null,
    department	varchar(64),
    type		varchar(32) not null,
    primary key (cardID)
);

create table records
(
	recordID	int not null,
    cardID		int not null,
    bookID		int not null,
    borrowDate	datetime,
    due			datetime,
    agentID		int not null,
    primary key (recordID),
    foreign key (cardID) references cards,
    foreign key (bookID) references books,
    foreign key (agentID) references admin
);
```

在 https://docs.microsoft.com/zh-cn/windows/uwp/design/controls-and-patterns/navigationview 学习侧边栏（导航视图） Navigation View 的使用

在 https://docs.microsoft.com/zh-cn/windows/uwp/design/style/segoe-ui-symbol-font 找到图标



```SQL
use LibraryManager;

insert into admin
	values (1, 'admin1', 'XYX', '15029757365'), (123, 'lc', 'LC', '0');
```



页面结构：

* `MainPage.xaml` - 登录页面
* `AdminStatus.xaml` - 管理员状态页面
* `GuideLogin.xaml` - 引导登录页面
* `AddBook.xaml` - 图书入库页面
* `BookSearch.xaml` - 图书查询页面
* `LendBook.xaml` -  借书页面
* `ReturnBook.xaml` - 还书页面
* `CardManage.xaml` - 借书证管理页面



```
<NavigationView x:Name="NavView"
                         Loaded="NavView_Loaded"
                         ItemInvoked="NavView_ItemInvoked"
                         BackRequested="NavView_BackRequested" 
                         IsSettingsVisible="False"
                         IsBackButtonVisible="0"
                         Margin="0,0,0,0" FontFamily="Segoe UI"
                         >
            <NavigationView.MenuItems>
                <NavigationViewItem x:Name="AdminLogin" Tag="account" Icon="Contact" Content="管理员登录" HorizontalAlignment="Stretch" ScrollViewer.HorizontalScrollBarVisibility="Disabled" FontWeight="Normal" AccessKeyInvoked="AdminLogin_AccessKeyInvoked"/>
                <NavigationViewItemSeparator HorizontalAlignment="Stretch" Margin="5,5,0,0" VerticalAlignment="Stretch"/>
                <NavigationViewItem x:Name="BookNew" Tag="apps" Content="图书入库" FontFamily="Segoe UI">
                    <NavigationViewItem.Icon>
                        <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xECCD;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem x:Name="BookSearch" Tag="games" Content="图书查询" FontFamily="Segoe UI">
                    <NavigationViewItem.Icon>
                        <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xF78B;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem x:Name="Borrow" Content="借书">
                    <NavigationViewItem.Icon>
                        <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xEBE7;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem x:Name="Return" Content="还书">
                    <NavigationViewItem.Icon>
                        <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xEC52;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
                <NavigationViewItem x:Name="CardManage" Content="借书证管理" FontFamily="Segoe UI">
                    <NavigationViewItem.Icon>
                        <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xE716;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
            </NavigationView.MenuItems>

            <ScrollViewer>
                <Grid x:Name="ContentFrame" Padding="0,0,12,24">
                    <TextBlock x:Name="Title" HorizontalAlignment="Left" Text="管理员登录" TextWrapping="Wrap" VerticalAlignment="Top" Margin="100,70,0,0" Height="35" Width="229" FontSize="24" FontWeight="Bold" SelectionChanged="TextBlock_SelectionChanged" FontFamily="Segoe UI"/>

                    <StackPanel>
                        <TextBox x:Name="AdminID" Width="300" Header="管理员 ID" PlaceholderText="请键入您的 ID" HorizontalAlignment="Left" Margin="100,160,0,0"/>
                        <PasswordBox x:Name="passwordBox" Width="300" MaxLength="16" Header="密码" PlaceholderText="请输入密码" Margin="100,40,0,0" HorizontalAlignment="Left" VerticalAlignment="Stretch"/>
                    </StackPanel>
                    <StackPanel>
                        <Button Content="登录" HorizontalAlignment="Left" VerticalAlignment="Top" Click="Button_Click" Margin="100,360,0,0"/>
                        <TextBlock x:Name="LoginTip" Margin="100,40,0,0" 
                                   HorizontalAlignment="Left" Text=""
                                   FontFamily="Segoe UI" FontSize="16" 
                                   VerticalAlignment="Stretch" Foreground="Red"/>
                    </StackPanel>
                    
                </Grid>
            </ScrollViewer>
        </NavigationView>
```

