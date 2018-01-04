USE [ARLog]
GO

/****** Object:  StoredProcedure [dbo].[usp_SetCategories]    Script Date: 12/22/2017 10:33:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_SetCategories] @dropThreshold MONEY = 1.00
	,@pennyThreshold MONEY = 0.09
AS
BEGIN
	-- False positives
	UPDATE Records
	SET CategoryId = 1
	WHERE CategoryId = 0
		AND Done = 1
		AND (
			Notes LIKE '%false positive%'
			OR Notes LIKE '%cleared after z out%'
			)

	-- Drop adjustments
	UPDATE Records
	SET CategoryId = 4
	WHERE CategoryId = 0
		AND Done = 1
		AND ABS(Amount) <= @dropThreshold

	-- Manual order adjustment
	UPDATE Records
	SET CategoryId = 9
	WHERE CategoryId = 0
		AND Done = 1
		AND Notes LIKE '%manual void%'

	-- Void issue
	UPDATE Records
	SET CategoryId = 12
	WHERE CategoryId = 0
		AND Done = 1
		AND Notes LIKE '%void issue%'

	-- Order missing transver in livelink
	UPDATE Records
	SET CategoryId = 17
	WHERE CategoryId = 0
		AND Done = 1
		AND Notes LIKE '%duped in livelink%'

	-- Pennies from promos (due to setting emp discount amount 
	-- to 50% of price w/o rounding to 2 dec places, fixed 8/22/17)
	UPDATE Records
	SET CategoryId = 36
	WHERE CategoryId NOT IN (20, 36)
		AND Done = 1
		AND ABS(Amount) <= @pennyThreshold
		AND AccountId IN (
			SELECT AccountId
			FROM Accounts
			WHERE AccountNum IN (
					310, 387, 586, 683, 822, 1426, 1574, 1588, 1799, 2044, 2133, 2261, 2262, 2314, 2350, 
					2351, 2352, 2380, 2516, 3066, 3081, 3343, 3396, 3407, 3730
					)
			)

	-- Pennies from web orders (AP stores), often PIF-related
	UPDATE Records
	SET CategoryId = 37
	WHERE CategoryId NOT IN (20, 37)
		AND Done = 1
		AND ABS(Amount) <= @pennyThreshold
		AND AccountId IN (
			SELECT AccountId
			FROM Accounts
			WHERE AccountNum IN (
					170, 572, 598, 614, 778, 799, 813, 852, 853, 893, 917, 930, 952, 961, 962, 1002, 1068, 
					1134, 1168, 1173, 1215, 1216, 1261, 1355, 1362, 1493, 1735, 2304, 2601, 2822, 2936, 
					3025, 3503, 3819
					)
			)

	-- Driver balances
	UPDATE Records
	SET CategoryId = 39
	WHERE CategoryId = 0
		AND Done = 1
		AND Notes LIKE '%driver%'
		AND Notes LIKE '%balance%'
END
GO

