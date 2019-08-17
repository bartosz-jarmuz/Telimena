CREATE PROCEDURE [dbo].[p_GetProgramUsagesSummary]
	@programIds nvarchar (max)
AS
SELECT 
v.ProgramId
,COUNT(DISTINCT v.Name) 'Types'
,MAX(details.Timestamp) 'Last'
,COUNT(details.Id) 'Total'
,(SELECT COUNT (details.Id) 
	FROM EventTelemetryDetails details
	JOIN EventTelemetrySummaries ON details.TelemetrySummaryId = EventTelemetrySummaries.Id
	JOIN Events innerE ON innerE.Id = EventTelemetrySummaries.EventId	
	AND innerE.ProgramId = v.ProgramId
	WHERE CONVERT(date,details.Timestamp)=CONVERT(DATE,getdate())
) 'Todays'
FROM EventTelemetryDetails details
JOIN EventTelemetrySummaries summaries ON details.TelemetrySummaryId = summaries.Id
JOIN Events v ON v.Id = summaries.EventId
WHERE v.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))
GROUP BY v.ProgramId

SELECT 
v.ProgramId
,COUNT(DISTINCT v.Name) 'Types'
,MAX(details.Timestamp) 'Last'
,COUNT(details.Id) 'Total'
,(SELECT COUNT (details.Id) 
	FROM ViewTelemetryDetails details
	JOIN ViewTelemetrySummaries ON details.TelemetrySummaryId = ViewTelemetrySummaries.Id
	JOIN Views innerV ON innerV.Id = ViewTelemetrySummaries.ViewId	
	AND innerV.ProgramId = v.ProgramId
	WHERE CONVERT(date, details.Timestamp)=CONVERT(DATE,getdate())
) 'Todays'
FROM ViewTelemetryDetails details
JOIN ViewTelemetrySummaries summaries ON details.TelemetrySummaryId = summaries.Id
JOIN Views v ON v.Id = summaries.ViewId
WHERE v.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))
GROUP BY v.ProgramId