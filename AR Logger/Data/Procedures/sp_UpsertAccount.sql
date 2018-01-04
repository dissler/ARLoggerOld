USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[sp_UpsertAccount]    Script Date: 12/22/2017 10:32:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- insert or update account details
CREATE PROCEDURE [dbo].[sp_UpsertAccount] @AccountName NVARCHAR(64) = ''
	,@AccountNum NVARCHAR(64)
	,@Street NVARCHAR(64) = ''
	,@City NVARCHAR(64) = ''
	,@State NVARCHAR(4) = ''
	,@Zone NVARCHAR(4) = ''
	,@AccountId BIGINT = NULL
AS
IF EXISTS (
		SELECT AccountId
		FROM Accounts
		WHERE AccountName = @AccountName
			AND AccountNum = @AccountNum
		)
	UPDATE Accounts
	SET Street = @Street
		,City = @City
		,[State] = @State
		,[Zone] = @Zone
	WHERE AccountName = @AccountName
		AND AccountNum = @AccountNum
ELSE
	INSERT INTO Accounts (
		AccountName
		,AccountNum
		,Street
		,City
		,[State]
		,[Zone]
		)
	VALUES (
		@AccountName
		,@AccountNum
		,@Street
		,@City
		,@State
		,@Zone
		)
GO

