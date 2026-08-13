CREATE OR ALTER PROCEDURE [dbo].[Sp_GetMfgPosPendingList]
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ROW_NUMBER() OVER (ORDER BY s.PoSubId) AS SlNo,

        ISNULL(s.[LineNo], 0) AS [LineNo],
        ISNULL(c.CustName, '') AS Customer,

        ISNULL(p.PONo, '') AS PONo,
        ISNULL(p.PODate, '1900-01-01') AS PODate,

        ISNULL(cs.CurrName, '') AS Currency,

        CASE 
            WHEN ISNULL(p.MfgORLab, 0) = 1 THEN 'Manufacturing'
            ELSE 'Labour'
        END AS Type,

        ISNULL(p.PayTerms, '') AS PaymentTerms,
        ISNULL(p.DeliveryTerms, '') AS DeliveryTerms,
        ISNULL(p.MainRemark, '') AS Remarks,
        ISNULL(p.RejAndRet, 0) AS Rejection,

        CASE 
            WHEN ISNULL(p.PoTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(p.PoCancl, 0) = 1 THEN 'Cancelled'
            WHEN ISNULL(p.ShortClose, 0) = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END AS POStatus,

        ISNULL(i.ItemCode, '') AS ItemCode,
        ISNULL(i.ItemName, '') AS ItemName,
        ISNULL(i.Specification, '') AS ItemSpecification,
        ISNULL(i.MeasureUnit, '') AS MeasureUnit,
        ISNULL(i.HSNCode, '') AS HSNCode,

        ISNULL(s.Qty, 0) AS Qty,
        ISNULL(s.BalQty, 0) AS BalQty,
        ISNULL(s.UnitPrice, 0) AS UnitPrice,
        ISNULL(s.LineGross, 0) AS GrossAmount,

        ISNULL(s.LineDiscountAmount, 0) AS Discount,
        ISNULL(s.LineDiscountPercent, 0) AS DiscountPerce,

        ISNULL(s.LineBasicAmount, 0) AS BasicAmt,

        ISNULL(s.Remark, 'N/A') AS ItemRemarks,

        ISNULL(CONVERT(VARCHAR(10), s.DueDate, 103), '') AS DueDate,
        ISNULL(CONVERT(VARCHAR(10), s.PoDate, 103), '') AS PoGetDate,

        ISNULL(q.QuoteNo, 'NA') AS RefQuoteNo,
        ISNULL(CONVERT(VARCHAR(10), q.QuoteDate, 103), '') AS RefQuoteDt

    FROM MfgPo p
    INNER JOIN MfgPoSub s ON p.PoId = s.PoId
    INNER JOIN Item i ON s.ItemId = i.ItemId
    INNER JOIN Customer c ON p.CustId = c.CustId
    INNER JOIN Currency cs ON p.CurrId = cs.CurrId
    LEFT JOIN MfgQuoteSub qs ON s.RefQuoteSubId = qs.QuoteSubId
    LEFT JOIN MfgQuote q ON qs.QuoteId = q.QuoteId

    WHERE
    (
        @Status IS NULL
        OR @Status = ''
        OR @Status = 'All'   -- ✅ FIX ADDED
        OR
        CASE 
            WHEN ISNULL(p.PoTally, 0) = 1 THEN 'Completed'
            WHEN ISNULL(p.PoCancl, 0) = 1 THEN 'Cancelled'
            WHEN ISNULL(p.ShortClose, 0) = 1 THEN 'Short Closed'
            ELSE 'Pending'
        END = @Status
    )

END
