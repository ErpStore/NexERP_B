CREATE OR ALTER     PROCEDURE [dbo].[Sp_GetPurchasePosPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ROW_NUMBER() OVER (ORDER BY  s.PoSubId) AS SlNo,

        -- Header Details
        ISNULL(c.VendorName, '') AS VendorSupplier,
        ISNULL(p.PONo, '') AS PONo,
        ISNULL(p.PODate, '1900-01-01') AS PODate,
        ISNULL(cs.CurrName, '') AS Currency,

        CASE
            WHEN ISNULL(p.PurchORSubCon, 0) = 1 THEN 'Purchase'
            ELSE 'SubContract'
        END AS Type,

        ISNULL(p.PayTerms, '') AS PaymentTerms,
        ISNULL(p.DeliveryTerms, '') AS DeliveryTerms,
        ISNULL(p.MainRemark, '') AS Remarks,
        ISNULL(p.RejAndRet, 0) AS Rejection,

        CASE
            WHEN ISNULL(p.PoTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(p.PoCancl, 0) = 1 THEN 'Cancelled'
            ELSE 'Pending'
        END AS POStatus,

        -- Item Details
        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(s.Qty, 0) AS Qty,
        ISNULL(s.BalQty, 0) AS BalQty,
		ISNULL((select sum(BalQty) from StockAdd where StoreId not in(6,7) and ItemId=i.ItemId), 0) AS StockQty,
        ISNULL(s.UnitPrice, 0) AS UnitPrice,
        ISNULL(s.LineGross, 0) AS GrossAmount,
        ISNULL(s.LineDiscountAmount, 0) AS Discount,
        ISNULL(s.LineDiscountPercent, 0) AS DiscountPerce,
        ISNULL(s.LineBasicAmount, 0) AS BasicAmt,

        -- Safe Date Strings
        ISNULL(CONVERT(VARCHAR(10), s.DueDate, 103), '') AS DueDate,

        ISNULL(s.Remark, '') AS ItemRemarks,

        -- Quote Reference
        ISNULL(q.QuoteNo, '') AS RefQuoteNo,
        ISNULL(CONVERT(VARCHAR(10), q.QuoteDate, 103), '') AS RefQuoteDt,

        -- PR Reference
        ISNULL(m.MReqNo, '') AS RefPRNo,
        ISNULL(CONVERT(VARCHAR(10), m.MreqDate, 103), '') AS RefPRDt

    FROM PurchPo p

        INNER JOIN PurchPoSub s
            ON p.PoId = s.PoId

        INNER JOIN Item i
            ON s.ItemId = i.ItemId

        INNER JOIN Vendor c
            ON p.VendorCode = c.VendorCode

        INNER JOIN Currency cs
            ON p.CurrId = cs.CurrId

        LEFT JOIN PurchaseQuoteSub qs
            ON s.RefQuoteSubId = qs.QuoteSubId

        LEFT JOIN PurchaseQuote q
            ON qs.QuoteId = q.QuoteId

        LEFT JOIN MaterialReqSub ms
            ON ms.MreqSubId = s.RefMReqSubId

        LEFT JOIN MaterialReq m
            ON m.MreqId = ms.MreqId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'
        OR
        (
            @Status = 'Completed'
            AND ISNULL(p.PoTally, 0) = 1
        )
        OR
        (
            @Status = 'Cancelled'
            AND ISNULL(p.PoCancl, 0) = 1
        )
        OR
        (
            @Status = 'Pending'
            AND ISNULL(p.PoTally, 0) = 0
            AND ISNULL(p.PoCancl, 0) = 0
        )
    )

    ORDER BY p.PONo, s.PoSubId;

END


