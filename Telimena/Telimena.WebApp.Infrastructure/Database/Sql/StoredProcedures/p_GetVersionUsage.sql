CREATE PROCEDURE [dbo].[p_GetVersionUsage]
	@programId int,
	@startDate datetime,
	@endDate datetime
	AS
SELECT 
FileVersion 'Version', 
Count (FileVersion) 'Count' 
FROM EventTelemetryDetails ed 
WHERE ed.TelemetrySummaryId IN 
	(
		SELECT Id FROM EventTelemetrySummaries WHERE EventId IN (
			SELECT Id FROM Events e WHERE e.ProgramId = @programId AND e.Name = 'TelimenaSessionStarted')
	)
AND ed.Timestamp BETWEEN @startDate AND @endDate
GROUP BY FileVersion
