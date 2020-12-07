CREATE PROCEDURE [dbo].[p_ClearTelemetryData]
	@programId int,
	@startDate datetime,
	@endDate datetime
AS


BEGIN TRANSACTION;

   DECLARE @DeletedIds TABLE ( ID INT );
   delete from EventTelemetrySummaries
   output deleted.id into @DeletedIds
   from table1 as t1
    inner join table2 as t2
      on t2.id = t1.id
    inner join table3 as t3
      on t3.id = t2.id;

   delete from t2
   from table2 as t2
    inner join @deletedIds as d
      on d.id = t2.id;

COMMIT TRANSACTION;

