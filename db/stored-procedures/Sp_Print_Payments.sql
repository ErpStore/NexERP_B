CREATE OR ALTER     PROCEDURE [dbo].[Sp_Print_Payments]
    @PaymentId INT
AS
BEGIN
    SELECT 
        p.PaymentNo,
        p.PaymentDate,
        b.BankName,
        b.AccountNo,
        p.ChequeNo,
        p.ChequeDate,
        p.PayToName,
        e.ExpenseName,
        p.Amount,
		dbo.NumToWords(p.Amount) AS AmountInWords,
        ps.BillNo,
        ps.AdjustAmount
    FROM Payments p
    INNER JOIN PaymentsSub ps 
        ON p.PaymentId = ps.PaymentId
    LEFT JOIN Bank b 
        ON p.BankId = b.BankId
    LEFT JOIN Expense e 
        ON e.ExpenseCode = p.ExpenseCode
    WHERE p.PaymentId = @PaymentId
END
