CREATE PROCEDURE [dbo].[p_GetDailySummaryCounts]
	@programId int,
	@startDate datetime,
	@endDate datetime
	AS

SELECT CAST(Timestamp AS DATE) [Date], COUNT (Id) FROM EventTelemetryDetails d 
	WHERE TelemetrySummaryId IN (
		SELECT Id FROM EventTelemetrySummaries 
		WHERE EventId IN (
			SELECT Id FROM Events v WHERE v.ProgramId = @programId
		)
	)
	AND d.Timestamp BETWEEN @startDate AND @endDate
GROUP BY CAST(Timestamp AS DATE)

SELECT CAST(Timestamp AS DATE) [Date], COUNT (Id) FROM ViewTelemetryDetails d 
	WHERE TelemetrySummaryId IN (
		SELECT Id FROM ViewTelemetrySummaries 
		WHERE ViewId IN (
			SELECT Id FROM Views v WHERE v.ProgramId = @programId
		)
	)
	AND d.Timestamp BETWEEN @startDate AND @endDate
GROUP BY CAST(Timestamp AS DATE)

SELECT CAST(Timestamp AS DATE) [Date], COUNT (Id) FROM ExceptionInfoes d 
	WHERE d.ProgramId = @programId
	AND d.Timestamp BETWEEN @startDate AND @endDate
GROUP BY CAST(Timestamp AS DATE)
