CREATE PROCEDURE [dbo].[p_GetDailyUsersCounts]
	@programId int,
	@startDate datetime,
	@endDate datetime
	AS

SELECT CAST(Timestamp AS DATE) [Date], COUNT (DISTINCT UserIdentifier) FROM EventTelemetryDetails d 
	WHERE TelemetrySummaryId IN (
		SELECT Id FROM EventTelemetrySummaries 
		WHERE EventId IN (
			SELECT Id FROM Events v WHERE v.ProgramId = @programId
		)
	)
	AND d.Timestamp BETWEEN @startDate AND @endDate
GROUP BY CAST(Timestamp AS DATE)
