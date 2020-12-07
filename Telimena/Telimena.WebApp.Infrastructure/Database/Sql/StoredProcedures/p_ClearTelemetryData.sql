CREATE PROCEDURE [dbo].[p_ClearTelemetryData]
	@programId int,
	@startDate datetime,
	@endDate datetime
AS


BEGIN TRANSACTION;



COMMIT TRANSACTION;

