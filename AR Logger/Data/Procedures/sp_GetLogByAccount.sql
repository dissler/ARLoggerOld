USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetLogByAccount]    Script Date: 12/27/2017 8:03:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Get imbalance log for store
CREATE PROCEDURE [dbo].[sp_GetLogByAccount] @AccountId BIGINT
AS
SELECT BusDate AS [Date]
	,Amount
	,Done
	,Tech
	,Notes
	,CategoryId
	,AccountId
	,RecordId
FROM Records
WHERE AccountId = @AccountId
ORDER BY BusDate DESC
GO

