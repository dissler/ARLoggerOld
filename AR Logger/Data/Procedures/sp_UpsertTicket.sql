USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_UpsertTicket]    Script Date: 12/22/2017 10:32:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_UpsertTicket] @AccountId BIGINT
	,@TicketNum NVARCHAR(50)
	,@Created DATETIME = getdate
	,@Notes NVARCHAR(max) = NULL
	,@Active BIT = NULL
	,@TicketId BIGINT = 0
AS
BEGIN
	IF @TicketID > 0
		UPDATE Tickets
		SET TicketNum = @TicketNum
			,Created = @Created
			,Notes = @Notes
			,Active = @Active
		WHERE TicketId = @TicketId
	ELSE
		INSERT INTO Tickets (
			AccountId
			,TicketNum
			,Created
			,Notes
			,Active
			)
		VALUES (
			@AccountId
			,@TicketNum
			,@Created
			,@Notes
			,@Active
			)
END
GO

