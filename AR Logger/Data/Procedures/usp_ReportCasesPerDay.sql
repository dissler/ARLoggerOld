USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[usp_ReportCasesPerDay]    Script Date: 12/22/2017 10:33:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_ReportCasesPerDay] (
	@startDate DATETIME
	,@endDate DATETIME
	)
AS
BEGIN
	SET NOCOUNT ON
	SET DATEFIRST 3

	SELECT DATEPART(wk, R.BusDate) - 1 AS [Week]
		,DATEPART(yy, R.BusDate) AS [Year]
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Wednesday'
					THEN 1
				ELSE 0
				END) AS Wednesday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Thursday'
					THEN 1
				ELSE 0
				END) AS Thursday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Friday'
					THEN 1
				ELSE 0
				END) AS Friday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Saturday'
					THEN 1
				ELSE 0
				END) AS Saturday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Sunday'
					THEN 1
				ELSE 0
				END) AS Sunday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Monday'
					THEN 1
				ELSE 0
				END) AS Monday
		,SUM(CASE 
				WHEN DATENAME(dw, R.BusDate) = 'Tuesday'
					THEN 1
				ELSE 0
				END) AS Tuesday
		,COUNT(R.RecordId) AS Total
	FROM Records R
	LEFT JOIN Categories C
		ON R.CategoryId = C.CategoryId
	WHERE (
			C.[Group] <> 'ignore'
			OR R.CategoryID = 0
			)
		AND R.BusDate BETWEEN @startDate
			AND @endDate
	GROUP BY DATEPART(yy, R.BusDate)
		,DATEPART(wk, R.BusDate)
	ORDER BY DATEPART(yy, R.BusDate)
		,DATEPART(wk, R.BusDate)
END
GO

