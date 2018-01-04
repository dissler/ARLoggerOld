USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetTechList]    Script Date: 12/22/2017 10:31:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Get list of techs and the date of their latest record
CREATE PROCEDURE [dbo].[sp_GetTechList]
AS
SELECT Tech
	,MAX(BusDate) AS LastDate
FROM Records
WHERE Tech IS NOT NULL
	AND Tech <> ''
GROUP BY Tech
ORDER BY MAX(BusDate) DESC
	,Tech
GO

