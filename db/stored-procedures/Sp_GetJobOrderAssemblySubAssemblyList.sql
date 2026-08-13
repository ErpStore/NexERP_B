

CREATE OR ALTER   PROCEDURE [dbo].[Sp_GetJobOrderAssemblySubAssemblyList]
(
    @Status NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY j.JobId, js.JobSubId) AS SlNo,

        ISNULL(j.JobNo,'') + ISNULL(j.Suffix,'') AS JobNo,
        ISNULL(CONVERT(VARCHAR(20),j.JobDate,103),'') AS JobDate,

        ISNULL(p.PONo,'') AS RefPoNo,
        ISNULL(CONVERT(VARCHAR(20),p.PODate,103),'') AS RefPoDate,

        ISNULL(c.CustName,'') AS Customer,

		(SELECT   CAST([LineNo] AS INT) FROM MfgPoSub  WHERE PoSubId in(j.refposubid)) AS [LineNo],
        -- Main Assembly
        ISNULL(ai.ItemCode,'') AS Assembly,

        -- Parent Job Assembly
        ISNULL(pi.ItemCode,'') AS SubAssembly,

        ISNULL(j.POQty,0) AS POQty,
        ISNULL(j.JobOrderQty,0) AS JobOrderQty,
        ISNULL(j.JobBalQty,0) AS JobOrderBalQty,

        ISNULL(i.ItemCode,'') AS ItemCode,
        ISNULL(i.ItemName,'') AS ItemName,
        ISNULL(i.Specification,'') AS ItemSpecification,
        ISNULL(i.MeasureUnit,'') AS MeasureUnit,
        ISNULL(i.HSNCode,'') AS HSNCode,

        ISNULL(js.UtilQty,0) AS UtilQty,
        ISNULL(js.ReqQty,0) AS RequiredQty,
        ISNULL(js.BalQty,0) AS BalQyy,
		CASE
		    WHEN ISNULL(j.MfgORLab,0)=1 then 'Manufacturing' else 'Labour' end AS JobType,

        CASE
            WHEN ISNULL(j.Cancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(j.JobTally,0)=1 THEN 'Completed'
            ELSE 'Pending'
        END AS Status,
		 ISNULL(j.CreatedBy,'') AS CreatedBy,
        ISNULL(CONVERT(VARCHAR(20), j.CreatedDate, 103), '') AS CreatedDate,
        ISNULL(j.ModifiedBy,'') AS ModifiedBy,
        ISNULL(CONVERT(VARCHAR(20), j.ModifiedDate, 103), '') AS ModifiedDate

    FROM JobOrder j

    INNER JOIN JobOrderSub js
        ON js.JobId = j.JobId

    LEFT JOIN Customer c
        ON c.CustId = j.CustId

    LEFT JOIN MfgPo p
        ON p.PoId = j.RefPoId

    -- Current Assembly
    LEFT JOIN Item ai
        ON ai.ItemId = j.AssyId

    -- Parent Job
    LEFT JOIN JobOrder pj
        ON pj.JobId = j.ParentJobId

    LEFT JOIN Item pi
        ON pi.ItemId = pj.AssyId

    -- Material Item
    LEFT JOIN Item i
        ON i.ItemId = js.ItemId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        CASE
            WHEN ISNULL(j.Cancel,0)=1 THEN 'Cancelled'
            WHEN ISNULL(j.JobTally,0)=1 THEN 'Completed'
            ELSE 'Pending'
        END = @Status
    )

    ORDER BY j.JobId, js.JobSubId

END
