USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[usp_ReportCurrentWeek]    Script Date: 12/22/2017 10:33:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Gets case totals from past four weeks and case categories for current week
CREATE PROCEDURE [dbo].[usp_ReportCurrentWeek]
AS
BEGIN
	SET DATEFIRST 3

	DECLARE @startRange DATE = (
			SELECT MIN(BusDate)
			FROM Records
			)
		,@endRange DATE = (
			SELECT MAX(BusDate)
			FROM Records
			)
	DECLARE @endDate DATE = DATEADD(dd, 7 - DATEPART(dw, @endRange), @endRange)
	DECLARE @startCategory DATE = DATEADD(dd, - 6, @endDate)
	DECLARE @startCount DATE = DATEADD(wk, - 4, @startCategory)

	EXEC usp_SetCategories

	EXEC usp_ReportCasesPerDay @startCount
		,@endDate

	EXEC usp_ReportCategoriesByDate @startCategory
		,@endDate
END
GO

