USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetDateRange]    Script Date: 12/22/2017 10:30:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Get imbalance log for date range
CREATE PROCEDURE [dbo].[sp_GetDateRange]
AS
SELECT MIN(BusDate) AS [First]
	,MAX(BusDate) AS [Last]
FROM Records
GO

