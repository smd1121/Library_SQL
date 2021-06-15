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
    totalNum	int,
    storageNum	int,
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