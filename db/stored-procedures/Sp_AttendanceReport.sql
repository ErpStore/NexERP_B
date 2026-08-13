CREATE OR ALTER   PROCEDURE [dbo].[Sp_AttendanceReport]
    @StaffId INT,
    @MonthId INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Ldetail VARCHAR(MAX),
        @InTime VARCHAR(MAX),
        @OutTime VARCHAR(MAX)

    SELECT 
        @Ldetail = Ldetail,
        @InTime = InTime,
        @OutTime = OutTime
    FROM Attendance
    WHERE StaffId = @StaffId
      AND MonthId = @MonthId
      AND Year = @Year

    ;WITH L AS (
        SELECT value, ROW_NUMBER() OVER (ORDER BY (SELECT 1)) rn
        FROM STRING_SPLIT(@Ldetail, ',')
    ),
    I AS (
        SELECT value, ROW_NUMBER() OVER (ORDER BY (SELECT 1)) rn
        FROM STRING_SPLIT(@InTime, ',')
    ),
    O AS (
        SELECT value, ROW_NUMBER() OVER (ORDER BY (SELECT 1)) rn
        FROM STRING_SPLIT(@OutTime, ',')
    )

    SELECT 
        @StaffId AS StaffId,
        L.rn AS DayNo,
        L.value AS Status,
        I.value AS InTime,
        O.value AS OutTime
    FROM L
    LEFT JOIN I ON L.rn = I.rn
    LEFT JOIN O ON L.rn = O.rn
END
