CREATE OR ALTER        PROCEDURE [dbo].[Sp_GetItemModificationReport]
(
    @FromDate DATE,
    @ToDate DATE,
    @ReportType VARCHAR(20)
)
AS
BEGIN

    SET NOCOUNT ON;

    SELECT

        ROW_NUMBER() OVER
        (
            ORDER BY i.ItemCode
        ) AS SlNo,

        i.ItemId,

        i.ItemCode,

        i.ItemName,

        c.CategoryName,

        i.MeasureUnit,

        i.HSNCode,

        i.SACCode,

        i.Rate,

        i.ROL,

        i.MOL,

        i.Specification,

        i.CGST,

        i.SGST,

        i.IGST,

        i.Remarks,

        i.CreatedBy,

        i.CreatedDate,

        i.ModifiedBy,

        i.ModifiedDate,

        i.DeletedBy,

        i.ActionDate,

i.DrawingNo,i.leadTime,i.RackNo,i.PartWeight,i.TotalArea,i.Make,i.RmId,i.Shape,i.Density,i.Length,
i.Width,i.Height,i.Thickness,i.WallThickness,i.OuterDia,i.InnerDia,i.Weight,i.BatchNo

		


		

    FROM ItemHistory i

    LEFT JOIN Category c
        ON c.CategoryCode = i.CategoryCode

    WHERE
    (

        -----------------------------------
        -- ALL
        -----------------------------------

        @ReportType = 'All'

        OR

        -----------------------------------
        -- CREATED
        -----------------------------------

        (
            @ReportType = 'Created'

            AND i.ActionType = 'INSERT'

            AND CONVERT(DATE, i.ActionDate)
                BETWEEN @FromDate AND @ToDate
        )

        OR

        -----------------------------------
        -- MODIFIED
        -----------------------------------

        (
            @ReportType = 'Modified'

            AND i.ActionType IN ('OLD', 'NEW')

            AND CONVERT(DATE, i.ActionDate)
                BETWEEN @FromDate AND @ToDate
        )

        OR

        -----------------------------------
        -- DELETED
        -----------------------------------

        (
            @ReportType = 'Deleted'

            AND i.ActionType = 'DELETE'

            AND CONVERT(DATE, i.ActionDate)
                BETWEEN @FromDate AND @ToDate
        )
    )

    ORDER BY i.ActionDate DESC

END

