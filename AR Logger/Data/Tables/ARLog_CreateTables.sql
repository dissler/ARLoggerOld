USE [ARLog]
GO

/****** Object:  Table [dbo].[Accounts]    Script Date: 1/2/2018 8:05:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Accounts](
	[AccountId] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountName] [nvarchar](64) NOT NULL,
	[AccountNum] [nvarchar](64) NULL,
	[Street] [nvarchar](64) NULL,
	[City] [nvarchar](64) NULL,
	[State] [nvarchar](64) NULL,
	[Zone] [nvarchar](64) NULL,
 CONSTRAINT [PK_AccountID] PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Categories](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](50) NOT NULL,
	[Group] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CategoryID] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Records](
	[RecordId] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[CategoryId] [bigint] NOT NULL,
	[BusDate] [date] NOT NULL,
	[Amount] [money] NOT NULL,
	[Tech] [nvarchar](4) NULL,
	[Notes] [nvarchar](max) NULL,
	[Done] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Records] ADD  CONSTRAINT [DF_Records_CategoryId]  DEFAULT ((0)) FOR [CategoryId]

ALTER TABLE [dbo].[Records] ADD  CONSTRAINT [DF_Records_Amount]  DEFAULT ((0)) FOR [Amount]

ALTER TABLE [dbo].[Records] ADD  CONSTRAINT [DF_Records_Done]  DEFAULT ((0)) FOR [Done]

CREATE TABLE [dbo].[Tickets](
	[TicketId] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[TicketNum] [nvarchar](50) NOT NULL,
	[Created] [date] NULL,
	[Notes] [nvarchar](max) NULL,
	[Active] [bit] NULL,
 CONSTRAINT [PK_TicketID] PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [FK_Tickets_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([AccountId])

ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [FK_Tickets_Accounts]

