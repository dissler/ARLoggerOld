USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetAccountDetails]    Script Date: 12/22/2017 10:30:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Get all account details
CREATE PROCEDURE [dbo].[sp_GetAccountDetails]
AS
SELECT AccountName
	,AccountNum
	,Street
	,ISNULL(City, '') AS City
	,ISNULL([State], '') AS [State]
	,ISNULL([Zone], '') AS [Zone]
	,AccountId
FROM Accounts
ORDER BY AccountNum
GO

