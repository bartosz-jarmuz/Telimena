CREATE PROCEDURE [dbo].[p_GetProgramSummaryCounts]
	@programIds nvarchar (max)
AS

SELECT 

COUNT(DISTINCT users.UserIdentifier) 'UsersCount'
,(SELECT COUNT (a.UserIdentifier) FROM
	(SELECT DISTINCT details.UserIdentifier
		FROM EventTelemetryDetails details
		JOIN EventTelemetrySummaries  summaries ON details.TelemetrySummaryId = summaries.Id
		JOIN Events innerE ON innerE.Id = summaries.EventId	
		AND innerE.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))
		GROUP BY details.UserIdentifier
		HAVING DATEADD(day, 7,  MIN(details.Timestamp)) >= CONVERT(DATE, GETDATE())  
	) a
) 'NewUsersCount'
,COUNT (DISTINCT events.Id) 'EventTypes'
,COUNT (eventDetails.Id) 'EventsTotal'
,(SELECT COUNT (DISTINCT views.Id)
	FROM Views views
	WHERE views.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ',')))
'ViewTypes'
,(SELECT COUNT (DISTINCT details.Id)
	FROM Views views
	JOIN ViewTelemetrySummaries summaries ON views.Id = summaries.ViewId
	JOIN ViewTelemetryDetails details ON details.TelemetrySummaryId = summaries.Id
	WHERE views.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ',')))
'ViewsTotal'
,(SELECT COUNT (ex.Id) FROM
	(SELECT DISTINCT exceptions.Id
		FROM ExceptionInfoes exceptions
		WHERE exceptions.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))
		GROUP BY exceptions.Id
		HAVING DATEADD(day, 7,  MIN(exceptions.Timestamp)) >= CONVERT(DATE, GETDATE())  
	) ex
) 'RecentExceptionsCount'
,(SELECT TOP 1(exceptions.TypeName) from ExceptionInfoes exceptions
			WHERE exceptions.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))
			GROUP BY exceptions.TypeName
			HAVING DATEADD(day, 7,  MIN(exceptions.Timestamp)) >= CONVERT(DATE, GETDATE())  
			ORDER BY COUNT (exceptions.TypeName) DESC) 'MostPopularRecentException'
FROM EventTelemetryDetails eventDetails
JOIN EventTelemetrySummaries summaries ON eventDetails.TelemetrySummaryId = summaries.Id
JOIN ClientAppUsers users ON users.Id = summaries.ClientAppUserId
JOIN Events events ON events.Id = summaries.EventId
WHERE events.ProgramId IN (SELECT Item FROM f_SplitInts(@programIds, ','))

