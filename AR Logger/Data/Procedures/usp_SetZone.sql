USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[usp_SetZone]    Script Date: 12/22/2017 10:34:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--Assigns store an approximate TZ based on state
CREATE PROCEDURE [dbo].[usp_SetZone]
AS
BEGIN
	UPDATE Accounts
	SET [Zone] = CASE 
			WHEN [State] IN ('WA', 'CA', 'NV')
				THEN 'PST'
			WHEN [State] IN ('MT', 'UT', 'AZ', 'WY', 'CO', 'NM')
				THEN 'MST'
			WHEN [State] IN ('OK', 'LA', 'AR', 'MO', 'IA', 'MN', 'WI', 'IL', 'MS', 'AL'
					)
				THEN 'CST'
			WHEN [State] IN (
					'ME', 'NH', 'VT', 'MA', 'RI', 'CT', 'NY', 'NJ', 'PA', 'DE', 'MD', 'OH', 'WV', 'VA', 
					'NC', 'SC', 'GA', 'DC'
					)
				THEN 'EST'
					-- states mostly in one TZ
			WHEN [State] IN ('OR')
				THEN 'PST'
			WHEN [State] IN ('ID')
				THEN 'MST'
			WHEN [State] IN ('ND', 'SD', 'NE', 'KS', 'TX', 'TN')
				THEN 'CST'
			WHEN [State] IN ('MI', 'IN', 'KY', 'FL')
				THEN 'EST'
			ELSE ''
			END
	WHERE [Zone] IS NULL
		OR [Zone] = ''
END
GO

