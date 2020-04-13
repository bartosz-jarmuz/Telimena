CREATE PROCEDURE [dbo].[p_GetEventTelemetryUnits]
	@programId int,
	@startDate datetime,
	@endDate datetime
AS

DECLARE @Summaries table( Id uniqueidentifier)

INSERT INTO @Summaries select Id from EventTelemetrySummaries s where s.EventId IN (select Id from Events e where e.ProgramId = @programId) 

select d.Timestamp, 
d.UserIdentifier,
d.EntryKey,
d.Sequence,
u.[Key],
u.ValueString,
u.ValueDouble
from EventTelemetryDetails d 
JOIN EventTelemetryUnits u ON d.Id = u.TelemetryDetail_Id
where d.TelemetrySummaryId IN (select Id from @Summaries) 
AND d.Timestamp BETWEEN @startDate AND @endDate