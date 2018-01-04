USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetLogByDate]    Script Date: 12/22/2017 10:31:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Get imbalance log for date
CREATE PROCEDURE [dbo].[sp_GetLogByDate] @firstDate DATETIME
	,@lastDate DATETIME = NULL
AS
SELECT R.BusDate AS [Date]
	,A.AccountName
	,A.AccountNum
	,A.Street
	,A.City
	,A.[State]
	,A.[Zone]
	,R.Amount
	,R.Done
	,ISNULL(R.Tech, '') AS Tech
	,ISNULL(R.Notes, '') AS Notes
	,ISNULL(C.CategoryId, 0) AS CategoryId
	,A.AccountId
	,R.RecordId
FROM Accounts A
LEFT JOIN Records R
	ON A.AccountId = R.AccountId
LEFT JOIN Categories C
	ON C.CategoryId = R.CategoryId
WHERE R.BusDate BETWEEN @firstDate
		AND ISNULL(@lastdate, @firstDate)
ORDER BY R.BusDate
	,A.AccountName
	,A.AccountNum
GO

