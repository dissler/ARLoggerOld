USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetCategories]    Script Date: 12/22/2017 10:30:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_GetCategories]
AS
SELECT CategoryId
	,[Description]
	,[Group]
FROM Categories
ORDER BY [Description]
GO

