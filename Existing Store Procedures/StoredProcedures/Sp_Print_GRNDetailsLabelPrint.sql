CREATE OR ALTER PROCEDURE Sp_Print_GRNDetailsLabelPrint
    @GRNId INT
AS
BEGIN
    SELECT 
        i.ItemCode,
        i.ItemName,
        i.PartWeight,
        i.TotalArea,
        i.Remarks AS Process,
        labsub.ItemSpecification AS OutputPartName,
        c.CustName,
        lab.RefDcNo,
        convert(varchar,cast(lab.RefDcDate as date),103) as RefDcDate,
		co.FilePath,
		co.Image,
        lab.GRNNo AS GRNNoWithSuffix,
        labsub.Qty

    FROM LabourGRNSub labsub

    INNER JOIN Item i 
        ON i.ItemId = labsub.ItemId

    INNER JOIN LabourGRN lab 
        ON labsub.GRNId = lab.GRNId

    INNER JOIN Customer c 
        ON lab.CustId = c.CustId

   LEFT JOIN Correspondences co
		ON  labsub.ItemId= co.ReferenceId AND co.DocumentType='Drawing'

    WHERE 
        labsub.GRNId = @GRNId
        AND labsub.TransType = 'In';
END