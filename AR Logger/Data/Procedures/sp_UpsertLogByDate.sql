USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_UpsertLogByDate]    Script Date: 12/22/2017 10:32:29 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_UpsertLogByDate] @RecordId BIGINT
	,@AccountName NVARCHAR(64) = ''
	,@AccountNum NVARCHAR(64)
	,@BusDate DATETIME
	,@Amount MONEY
	,@Done BIT = 0
	,@Tech NVARCHAR(4) = ''
	,@Notes NVARCHAR(max) = ''
	,@CategoryId INT = 0
	,@NewRecordId BIGINT OUTPUT
AS
BEGIN
	-- Make sure Account record exists
	IF NOT EXISTS (
			SELECT AccountId
			FROM Accounts
			WHERE AccountNum = @AccountNum
			)
		EXEC sp_UpsertAccount @AccountName
			,@AccountNum

	DECLARE @AccountId INT = (
			SELECT AccountId
			FROM Accounts
			WHERE AccountNum = @AccountNum
			)

	IF @RecordId > 0
		UPDATE Records
		SET Amount = @Amount
			,Done = @Done
			,Tech = @Tech
			,Notes = @Notes
			,CategoryId = @CategoryId
		WHERE RecordId = @RecordId
	ELSE
	BEGIN
		-- If RecordId is 0, the client thinks this is a new row, but it might exist in the database
		SET @RecordId = (
				ISNULL((
						SELECT TOP 1 RecordId
						FROM Records
						WHERE AccountId = @AccountId
							AND BusDate = @BusDate
							AND Amount = @Amount
						), 0)
				)

		IF @RecordId > 0
			UPDATE Records
			SET Amount = @Amount
				,Done = @Done
				,Tech = @Tech
				,Notes = @Notes
				,CategoryId = @CategoryId
			WHERE RecordId = @RecordId
		ELSE
		BEGIN		
			INSERT INTO Records (
				AccountId
				,BusDate
				,Amount
				,Done
				,Tech
				,Notes
				,CategoryId
				)
			VALUES (
				@AccountId
				,@BusDate
				,@Amount
				,@Done
				,@Tech
				,@Notes
				,@CategoryId
				)

			SET @RecordID = (
					SELECT TOP 1 RecordId
					FROM Records
					WHERE AccountId = @AccountID
						AND BusDate = @BusDate
						AND Amount = @Amount
					)
		END
	END

	SELECT @NewRecordID = @RecordID
END
GO

