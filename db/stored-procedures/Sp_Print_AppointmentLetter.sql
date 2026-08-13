CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_AppointmentLetter]
(
    @AppointmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.CandidateName,
        C.Address,
        AL.AppointmentDate,
        AL.Introduction,
        AL.ClosingNotes,
        ALS.Title,
        ALS.Description
    FROM AppointmentLetter AL
    LEFT JOIN AppointmetLetterSub ALS
        ON AL.AppointmentId = ALS.AppointmentId
    LEFT JOIN Candidate C
        ON C.CandidateID = AL.CandidateID
    WHERE AL.AppointmentId = @AppointmentId
    ORDER BY ALS.SlNo;
END
