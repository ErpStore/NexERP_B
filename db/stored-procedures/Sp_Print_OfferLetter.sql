CREATE OR ALTER   PROCEDURE [dbo].[Sp_Print_OfferLetter]
(
    @OfferId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.CandidateName,
        C.Address,
        O.OfferDate,
        O.Introduction,
        O.ClosingNotes,
        OS.Title,
        OS.Description
    FROM OfferLetter O
    LEFT JOIN OfferLetterSub OS
        ON O.OfferId = OS.OfferId
    LEFT JOIN Candidate C
        ON C.CandidateID = O.CandidateID
    WHERE O.OfferId = @OfferId
    ORDER BY OS.SlNo;
END
