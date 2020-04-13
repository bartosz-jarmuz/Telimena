CREATE PROCEDURE [dbo].[p_GetViewTelemetryUnits]
	@programId int,
	@startDate datetime,
	@endDate datetime
AS

DECLARE @Summaries table( Id uniqueidentifier)

INSERT INTO @Summaries select Id from ViewTelemetrySummaries s where s.ViewId IN (select Id from Views e where e.ProgramId = @programId) 

select d.Timestamp, 
d.UserIdentifier,
d.EntryKey,
d.Sequence,
u.[Key],
u.ValueString,
u.ValueDouble
from ViewTelemetryDetails d 
JOIN ViewTelemetryUnits u ON d.Id = u.TelemetryDetail_Id
where d.TelemetrySummaryId IN (select Id from @Summaries) 
AND d.Timestamp BETWEEN @startDate AND @endDate