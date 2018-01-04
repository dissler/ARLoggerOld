USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetTickets]    Script Date: 12/22/2017 10:31:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_GetTickets] @AccountId BIGINT
AS
SELECT A.AccountId
	,T.TicketNum
	,T.Created
	,T.Notes
	,T.Active
	,T.TicketId
FROM Tickets T
INNER JOIN Accounts A
	ON T.AccountId = A.AccountId
WHERE A.AccountId = @AccountId
ORDER BY T.Created DESC
GO

