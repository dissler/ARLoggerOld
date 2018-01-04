USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[usp_ReportCategoriesByDate]    Script Date: 12/22/2017 10:33:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Gets a report of cases in each category for the entirety of the
-- fiscal weeks covered by the start and end dates.
CREATE PROCEDURE [dbo].[usp_ReportCategoriesByDate] (
	@startDate DATETIME
	,@endDate DATETIME
	)
AS
BEGIN
	SET NOCOUNT ON
	SET DATEFIRST 3

	DECLARE @crlf NVARCHAR(4) = CHAR(13) + CHAR(10)
		,@tab NVARCHAR(2) = CHAR(9)
		--,@formatDate NVARCHAR(10)
	DECLARE @cmd NVARCHAR(MAX) = 
		'SELECT (SELECT [Description] FROM Categories WHERE CategoryId = R.CategoryId) AS Category,' 
		+ @crlf + @tab + 
		'(SELECT [Group] FROM Categories WHERE CategoryId = R.CategoryId) AS [Group]'
		,@thisDate DATETIME = @endDate

	WHILE @thisDate >= @startDate
	BEGIN
		IF DATENAME(dw, @thisDate) = 'Tuesday'
		BEGIN
			SET @cmd += ',' + @crlf + @tab + '(SELECT COUNT(R1.RecordId)' + @crlf + @tab + @tab + 
				' FROM Records R1' + @crlf + @tab + @tab + 
				'LEFT JOIN Categories C1 on C1.CategoryId = R1.CategoryId' + @crlf + @tab + @tab + 
				'WHERE R1.CategoryId = R.CategoryId AND R1.BusDate BETWEEN ''' + CONVERT(
					NVARCHAR(10), DATEADD(d, - 6, @thisDate), 1) + ''' AND ''' + CONVERT(NVARCHAR(
						10), @thisDate, 1) + ''') AS [Week ' + CONVERT(NVARCHAR(4), DATEPART(wk, 
						@thisDate) - 1) + ']' -- subtract 1 for partial first week
		END

		SET @thisDate = DATEADD(d, - 1, @thisDate)
	END

	SET @cmd += @crlf + 'FROM Records R' + @crlf + 
		'LEFT JOIN Categories C ON R.CategoryId = C.CategoryId' + @crlf + 
		'WHERE R.BusDate BETWEEN ''' + CONVERT(NVARCHAR(10), @startDate, 1) + ''' AND ''' + CONVERT(NVARCHAR(10), @endDate, 1) + ''' AND C.[Group] <> ''ignore''' + @crlf + 
		'GROUP BY R.CategoryId ORDER BY COUNT(R.CategoryId) DESC'

	--PRINT @cmd
	EXEC(@cmd)
END
GO

