
CREATE OR ALTER    PROCEDURE [dbo].[Sp_GetMfgPOPendingList]
(
    @Type VARCHAR(20),
    @Id INT = 0,

    @FromDate DATE = NULL,
    @ToDate DATE = NULL,

    @Mode VARCHAR(20) = 'List'
)
AS
BEGIN
    SET NOCOUNT ON;

    ---------------------------------------------------
    -- LIST (Customer / Vendor)
    ---------------------------------------------------
    IF (@Mode = 'List')
    BEGIN
        IF (@Type = 'Sales')
        BEGIN
            SELECT DISTINCT
                C.CustId AS Id,
                C.CustName AS Name
            FROM Customer C
            INNER JOIN MfgPo MP 
                ON MP.CustId = C.CustId
            WHERE MP.PoTally = 0
			 AND (@FromDate IS NULL OR MP.PODate >= @FromDate)
             AND (@ToDate IS NULL  OR MP.PODate <= @ToDate) 

			AND ISNULL(MP.PoCancl,0) = 0
            ORDER BY C.CustName;
        END
        ELSE IF (@Type = 'Purchase')
        BEGIN
            SELECT DISTINCT
                V.VendorCode AS Id,
                V.VendorName AS Name
            FROM Vendor V
            INNER JOIN PurchPo PP 
                ON PP.VendorCode = V.VendorCode
            WHERE PP.PoTally = 0 and  (@FromDate IS NULL OR pp.PODate >= @FromDate)
             AND (@ToDate IS NULL  OR pp.PODate <= @ToDate) 

			AND ISNULL(pp.PoCancl,0) = 0 
            ORDER BY V.VendorName;
        END

        RETURN;
    END

    ---------------------------------------------------
    -- DETAILS (PO + Items)
    ---------------------------------------------------
    IF (@Mode = 'Details')
    BEGIN

        ---------------------------------------------------
        -- SALES
        ---------------------------------------------------
        IF (@Type = 'Sales')
        BEGIN
            SELECT ROW_NUMBER() over (Order by MPS.poid desc) as SlNo,
                C.CustName AS Name,
                MP.PONo+mp.suffix as PONO,
                MP.PODate,
                I.ItemCode,
                I.ItemName,
				ISNULL(I.Specification,'') as Specification,
				I.MeasureUnit as UOM,
			    I.HSNCode,
                MPS.Qty,
                MPS.BalQty,
				mps.UnitPrice,
				mps.LineBasicAmount as BalanceAmount,

                ISNULL(
                (
                    SELECT SUM(SA.BalQty)
                    FROM StockAdd SA
                    WHERE SA.ItemId = MPS.ItemId
                      AND SA.StoreId NOT IN (6,7)
                ),0) AS StockQty,
                MPS.DueDate,
				ISNULL(I.ROL,0) AS ROL,
               ISNULL(I.MOL,0) AS MOL,
			   case when mp.MfgORLab=1 then 'Sales' else 'Labour' end as [Type]

            FROM MfgPo MP

            INNER JOIN MfgPoSub MPS
                ON MP.PoId = MPS.PoId

            INNER JOIN Customer C
                ON MP.CustId = C.CustId

            LEFT JOIN Item I
                ON I.ItemId = MPS.ItemId

            WHERE MP.PoTally = 0
			AND ISNULL(MP.PoCancl,0) = 0
              AND (@Id = 0 OR MP.CustId = @Id) and mps.BalQty>0 and (mps.ItemCancel is null or mps.ItemCancel=0)
			       AND (@FromDate IS NULL
                   OR MP.PODate >= @FromDate)

              AND (@ToDate IS NULL
                   OR MP.PODate <= @ToDate)

            ORDER BY MP.PODate DESC, MP.PONo;
        END

        ---------------------------------------------------
        -- PURCHASE
        ---------------------------------------------------
        ELSE IF (@Type = 'Purchase')
        BEGIN
            SELECT  ROW_NUMBER() over (Order by PPS.PoId desc) as SlNo,
                V.VendorName AS Name,
                PP.PONo +
CASE 
    WHEN ISNULL(PP.RevisionNo,0) = 0 THEN ''
    ELSE '/R' + CAST(PP.RevisionNo AS VARCHAR(10))
END +
CASE 
    WHEN ISNULL(PP.Suffix,'') = '' THEN ''
    ELSE '/' + PP.Suffix
END AS PONO,
                PP.PODate,
                I.ItemCode,
                I.ItemName,
				ISNULL(I.Specification,'') as Specification,
				I.MeasureUnit as UOM,
			    I.HSNCode,
                PPS.Qty,
                PPS.BalQty,
				pps.UnitPrice,
				pps.LineBasicAmount as BalanceAmount,

                ISNULL(
                (
                    SELECT SUM(SA.BalQty)
                    FROM StockAdd SA
                    WHERE SA.ItemId = PPS.ItemId
                      AND SA.StoreId NOT IN (6,7)
                ),0) AS StockQty,

                PPS.DueDate,
                case when PP.PurchORSubCon=1 then 'Purchase' else 'SubContract' end as [Type],
				ISNULL(I.ROL,0) AS ROL,
                ISNULL(I.MOL,0) AS MOL
				


            FROM PurchPo PP

            INNER JOIN PurchPoSub PPS
                ON PP.PoId = PPS.PoId

            INNER JOIN Vendor V
                ON PP.VendorCode = V.VendorCode

            LEFT JOIN Item I
                ON I.ItemId = PPS.ItemId

            WHERE PP.PoTally = 0
			AND ISNULL(pp.PoCancl,0) = 0
              AND (@Id = 0 OR PP.VendorCode = @Id) and PPS.BalQty>0 and (PPS.ItemCancel is null or PPS.ItemCancel=0)
			   AND (@FromDate IS NULL
                   OR pp.PODate >= @FromDate)

              AND (@ToDate IS NULL
                   OR pp.PODate <= @ToDate)
            ORDER BY PP.PODate DESC, PP.PONo;
        END
    END
END



