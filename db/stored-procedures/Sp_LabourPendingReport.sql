CREATE OR ALTER PROCEDURE [dbo].[Sp_LabourPendingReport]
(
    @IsPoWise BIT,
    @IsDetails BIT,
    @DocType VARCHAR(20),
	@FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CustId INT = NULL
)
AS

BEGIN
IF @IsPoWise=1
	BEGIN
	   IF @IsDetails = 1

			BEGIN
			  IF @DocType = 'Labour GRN'
			   BEGIN
				 SELECT
				ROW_NUMBER() OVER (ORDER BY g.GRNId DESC, gs.GRNSubId DESC) AS SlNo,
				c.CustId AS CustId,
				c.CustName AS CustName,

				g.GrnNo+g.Suffix AS DocNo,
				convert(Varchar(10),g.GrnDate,103) AS DocDate,

				g.RefDcNo AS RefDcNo,
				convert(Varchar(10),g.RefDcDate,103) AS RefDcDate,

				g.BatchNo AS BatchNo,
				g.NoOfItems AS NoOfItems,
				ms.[LineNo],
				i.itemid,
				i.ItemCode,
				i.ItemName,
				gs.ItemSpecification,
				i.MeasureUnit,
				gs.Qty,
				gs.BalQty,
				gs.TransType As TransType,
				m.PONo As PONo,
				convert(Varchar(10),m.PODate,103) AS PODate,
				'' AS ChildItemCode,
				'' as childItemName,
			    CAST(0 AS DECIMAL(18,2)) AS UtilQty,
				CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
				CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
				CAST(0 AS DECIMAL(18,2)) AS PendingQty,
				g.CreatedBy AS CreatedBy,
				convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
				FROM LabourGRN g INNER JOIN LabourGRNSub gs ON g.GRNId = gs.GRNId
				LEFT JOIN Customer c ON c.CustId = g.CustId
				LEFT JOIN Item i on i.itemid=gs.itemid
				LEFT JOIN MfgPoSub ms on ms.Posubid=gs.RefPoSubid
				LEFT JOIN MfgPo m on m.poid=ms.poid
				WHERE gs.BalQty > 0 AND gs.TransType='In'
					AND (@FromDate IS NULL OR CAST(g.GrnDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.GrnDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
			   END

			  Else IF @DocType = 'Labour SCN'
			   BEGIN
				SELECT
				ROW_NUMBER() OVER (ORDER BY g.SCNId DESC, gs.SCNSubid DESC) AS SlNo,

				c.CustId AS CustId,
				c.CustName AS CustName,

				g.SCNNo+g.Suffix AS DocNo,
				convert(Varchar(10),g.SCNDate,103) AS DocDate,

				'' AS RefDcNo,
				'' AS RefDcDate,
	
				'' AS BatchNo,
				g.NoOfItems AS NoOfItems,
				i.itemid,
				0 As [LineNo],
				i.ItemCode,
				i.ItemName,
				i.Specification As ItemSpecification,
				i.MeasureUnit,
				gs.AcceptQty As Qty,
				gs.BalQty,
				'' As TransType,
				'' As PONo,
				'' AS PODate,
				'' AS ChildItemCode,
				'' as childItemName,
				CAST(0 AS DECIMAL(18,2)) AS UtilQty,
				CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
				CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
				CAST(0 AS DECIMAL(18,2)) AS PendingQty,
				g.CreatedBy AS CreatedBy,
				convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
				FROM LabourSCN g INNER JOIN LabourSCNSub gs ON g.SCNId = gs.SCNId
				LEFT JOIN Customer c ON c.CustId = g.CustId
				LEFT JOIN Item i on i.itemid=gs.itemid
				WHERE gs.BalQty > 0
					AND (@FromDate IS NULL OR CAST(g.SCNDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.SCNDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
			   END
			   Else IF @DocType = 'Labour Dc-Out'
			   BEGIN
				 SELECT
				ROW_NUMBER() OVER (ORDER BY g.Dcid DESC, gs.DcSubid DESC) AS SlNo,

				c.CustId AS CustId,
				c.CustName AS CustName,

				g.DcNo+g.Suffix AS DocNo,
				convert(Varchar(10),g.DcDate,103) AS DocDate,
				'' AS RefDcNo,
				'' AS RefDcDate,
				'' AS BatchNo,
				g.NoOfItems AS NoOfItems,
				i.itemid,
				ms.[LineNo],
				i.ItemCode,
				i.ItemName,
				i.Specification As ItemSpecification,
				i.MeasureUnit,
				gs.Qty,
				gs.BalQty,
				gs.TransType As TransType,
				m.PONo As PONo,
				convert(Varchar(10),m.PODate,103) AS PODate,
	            '' AS ChildItemCode,
				'' as childItemName,
				CAST(0 AS DECIMAL(18,2)) AS UtilQty,
				CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
				CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
				CAST(0 AS DECIMAL(18,2)) AS PendingQty,
				g.CreatedBy AS CreatedBy,
				convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
				FROM LabourDcOutgoing g INNER JOIN LabourDcOutgoingSub gs ON g.DcId = gs.DcId
				LEFT JOIN Customer c ON c.CustId = g.CustId
				LEFT JOIN Item i on i.itemid=gs.itemid
				LEFT JOIN MfgPoSub ms on ms.Posubid=gs.RefPoSubid
				LEFT JOIN MfgPo m on m.poid=ms.poid
				WHERE gs.BalQty > 0 AND g.NonReturnDc=0  And g.ReceivedReturnRM=0
					AND (@FromDate IS NULL OR CAST(g.DcDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.DcDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
			   END
			   Else IF @DocType = 'Sales Po'
			   BEGIN
				   SELECT
					0 AS SlNo,
					c.CustId,
					c.custname as CustName,
					po.PONo As DocNo,
					po.PODate As DocDate,
					'' AS RefDcNo,
					'' AS RefDcDate,
					'' AS BatchNo,
					po.NoofItems,
					p.[LineNo],
					A.ItemId,
					i.ItemCode AS ItemCode,
					i.ItemName as ItemName,
					im.ItemCode AS ChildItemCode,
					im.ItemName as childItemName,
					i.Specification,
				    CAST(0 AS DECIMAL(18,3)) AS Qty,
					CAST(0 AS DECIMAL(18,3)) AS BalQty,
					CAST(A.UtilQty AS DECIMAL(18,3)) AS UtilQty,

					CAST((P.Qty * A.UtilQty) AS DECIMAL(18,3)) AS RequiredQty,

					CAST(ISNULL(SUM(GS.Qty),0) AS DECIMAL(18,3)) AS ReceivedQty,

					CAST((P.Qty * A.UtilQty) - ISNULL(SUM(GS.Qty),0) AS DECIMAL(18,3)) AS PendingQty

					FROM MfgPOSub P
					inner join mfgpo po on p.PoId =po.PoId
					inner join customer c on c.custid=po.custid

					INNER JOIN AssmblyDef A
						ON A.AssmblyID = P.ItemId

					LEFT JOIN Item im
						ON im.ItemId = A.ItemId

					LEFT JOIN LabourGRNSub lg
						ON lg.RefPoSubId = P.PoSubId
						AND lg.TransType = 'Out'

					LEFT JOIN LabourGRNSub GS
						ON GS.GroupId = lg.GroupId
						AND GS.TransType = 'In'
						AND GS.ItemId = A.ItemId and gs.GRNId=lg.GRNId

					LEFT JOIN Item i
						ON i.ItemId = P.ItemId
					WHERE gs.BalQty > 0
								AND (@FromDate IS NULL OR CAST(po.PODate AS DATE) >= @FromDate)
								AND (@ToDate IS NULL OR CAST(po.PODate AS DATE) <= @ToDate)
								AND (@CustId = 0 OR po.CustId = @CustId)
					GROUP BY	
						c.CustId,
						c.CustName,
						po.pono,
						po.NoofItems,
						po.PODate,
						P.PoSubId,
						p.[LineNo],
						i.ItemCode,
						im.ItemCode,
						im.ItemName,
						i.ItemName,
						i.Specification,
						P.Qty,
						A.ItemId,
						A.UtilQty;
				END
			END
		ELSE
			BEGIN
				IF @DocType = 'Labour GRN'
				BEGIN
					SELECT
						ROW_NUMBER() OVER (ORDER BY g.GRNId DESC) AS SlNo,
						c.CustId,
						c.CustName,
						g.GrnNo+g.Suffix AS DocNo,
						CONVERT(VARCHAR(10), g.GrnDate, 103) AS DocDate,
						g.RefDcNo,
						CONVERT(VARCHAR(10), g.RefDcDate, 103) AS RefDcDate,
						g.BatchNo,
						g.NoOfItems,
						g.CreatedBy,
						CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
					FROM LabourGRN g
					LEFT JOIN Customer c
						ON c.CustId = g.CustId
					WHERE EXISTS
					(
						SELECT 1
						FROM LabourGRNSub gs
						WHERE gs.GRNId = g.GRNId
						  AND gs.BalQty > 0 AND gs.TransType='In'
					)
					AND (@FromDate IS NULL OR CAST(g.GrnDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.GrnDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
				END
				Else If @DocType = 'Labour SCN'
				BEGIN 
					SELECT
				ROW_NUMBER() OVER (ORDER BY g.SCNId DESC) AS SlNo,
				c.CustId,
				c.CustName,
				g.SCNNo+g.Suffix AS DocNo,
				CONVERT(VARCHAR(10), g.SCNDate, 103) AS DocDate,
				'' As RefDcNo,
				'' RefDcDate,
				'' As BatchNo,
				g.NoOfItems,
				g.CreatedBy,
				CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
			FROM LabourSCN g
			LEFT JOIN Customer c
				ON c.CustId = g.CustId
			WHERE EXISTS
			(
				SELECT 1
				FROM LabourSCNSub gs
				WHERE gs.Scnid = g.Scnid
					AND gs.BalQty > 0
			)
			AND (@FromDate IS NULL OR CAST(g.SCNDate AS DATE) >= @FromDate)
			AND (@ToDate IS NULL OR CAST(g.SCNDate AS DATE) <= @ToDate)
			AND (@CustId = 0 OR g.CustId = @CustId);
				END
				Else If @DocType = 'Labour Dc-Out'
				BEGIN 
				  SELECT
					ROW_NUMBER() OVER (ORDER BY g.Dcid DESC) AS SlNo,
					c.CustId,
					c.CustName,
					g.DcNo+g.Suffix AS DocNo,
					CONVERT(VARCHAR(10), g.DcDate, 103) AS DocDate,
					'' As RefDcNo,
					'' RefDcDate,
					'' As BatchNo,
					g.NoOfItems,
					g.CreatedBy,
					CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
				FROM LabourDcOutgoing g
				LEFT JOIN Customer c
					ON c.CustId = g.CustId
				WHERE EXISTS
				(
					SELECT 1
					FROM LabourDcOutgoingSub gs
					WHERE gs.DcId = g.Dcid
						AND gs.BalQty > 0 AND g.NonReturnDc=0  And g.ReceivedReturnRM=0
				)
				AND (@FromDate IS NULL OR CAST(g.DcDate AS DATE) >= @FromDate)
				AND (@ToDate IS NULL OR CAST(g.DcDate AS DATE) <= @ToDate)
				AND (@CustId = 0 OR g.CustId = @CustId);
				END
			END
	END
ELSE
	BEGIN
	   IF @IsDetails = 1

		BEGIN
			IF @DocType = 'Labour GRN'
			BEGIN
				SELECT
				ROW_NUMBER() OVER (ORDER BY g.GRNId DESC, gs.GRNSubId DESC) AS SlNo,

				c.CustId AS CustId,
				c.CustName AS CustName,

				g.GrnNo+g.Suffix AS DocNo,
				convert(Varchar(10),g.GrnDate,103) AS DocDate,

				g.RefDcNo AS RefDcNo,
				convert(Varchar(10),g.RefDcDate,103) AS RefDcDate,

				g.BatchNo AS BatchNo,
				g.NoOfItems AS NoofItems,
				ms.[LineNo],
				i.itemid,
				i.ItemCode,
				i.ItemName,
				gs.ItemSpecification,
				i.MeasureUnit,
				gs.Qty,
				gs.BalQty,
				gs.TransType As TransType,
				m.PONo As PONo,
				convert(Varchar(10),m.PODate,103) AS PODate,
				'' AS ChildItemCode,
				'' as childItemName,
				CAST(0 AS DECIMAL(18,2)) AS UtilQty,
				CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
				CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
				CAST(0 AS DECIMAL(18,2)) AS PendingQty,
				g.CreatedBy AS CreatedBy,
				convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
				FROM LabourGRN g INNER JOIN LabourGRNSub gs ON g.GRNId = gs.GRNId
				LEFT JOIN Customer c ON c.CustId = g.CustId
				LEFT JOIN Item i on i.itemid=gs.itemid
				LEFT JOIN MfgPoSub ms on ms.Posubid=gs.RefPoSubid
				LEFT JOIN MfgPo m on m.poid=ms.poid
				WHERE gs.BalQty > 0 AND g.DcCancel=0 AND g.ShortClose=0 
					AND (@FromDate IS NULL OR CAST(g.GrnDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.GrnDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
			END

			Else IF @DocType = 'Labour SCN'
			BEGIN
			   SELECT
			ROW_NUMBER() OVER (ORDER BY g.SCNId DESC, gs.SCNSubid DESC) AS SlNo,

			c.CustId AS CustId,
			c.CustName AS CustName,

			g.SCNNo+g.Suffix AS DocNo,
			convert(Varchar(10),g.SCNDate,103) AS DocDate,

			'' AS RefDcNo,
			'' AS RefDcDate,
	
			'' AS BatchNo,
			g.NoOfItems AS NoOfItems,
			i.itemid,
			0 As [LineNo],
			i.ItemCode,
			i.ItemName,
			i.Specification As ItemSpecification,
			i.MeasureUnit,
			gs.AcceptQty As Qty,
			gs.BalQty,
			'' As TransType,
			'' As PONo,
			'' AS PODate,
			'' AS ChildItemCode,
			'' as childItemName,
			CAST(0 AS DECIMAL(18,2)) AS UtilQty,
			CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
			CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
			CAST(0 AS DECIMAL(18,2)) AS PendingQty,
			g.CreatedBy AS CreatedBy,
			convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
			FROM LabourSCN g INNER JOIN LabourSCNSub gs ON g.SCNId = gs.SCNId
			LEFT JOIN Customer c ON c.CustId = g.CustId
			LEFT JOIN Item i on i.itemid=gs.itemid
			WHERE gs.BalQty > 0
				AND (@FromDate IS NULL OR CAST(g.SCNDate AS DATE) >= @FromDate)
				AND (@ToDate IS NULL OR CAST(g.SCNDate AS DATE) <= @ToDate)
				AND (@CustId = 0 OR g.CustId = @CustId);
			END
			Else IF @DocType = 'Labour Dc-Out'
			BEGIN
				SELECT
				ROW_NUMBER() OVER (ORDER BY g.Dcid DESC, gs.DcSubid DESC) AS SlNo,

				c.CustId AS CustId,
				c.CustName AS CustName,

				g.DcNo+g.Suffix AS DocNo,
				convert(Varchar(10),g.DcDate,103) AS DocDate,

				'' AS RefDcNo,
				'' AS RefDcDate,
	
				'' AS BatchNo,
				g.NoOfItems AS NoOfItems,
				i.itemid,
				ms.[LineNo],
				i.ItemCode,
				i.ItemName,
				i.Specification As ItemSpecification,
				i.MeasureUnit,
				gs.Qty,
				gs.BalQty,
				gs.TransType As TransType,
				m.PONo As PONo,
				convert(Varchar(10),m.PODate,103) AS PODate,
			    '' AS ChildItemCode,
				'' as childItemName,
			    CAST(0 AS DECIMAL(18,2)) AS UtilQty,
				CAST(0 AS DECIMAL(18,2)) AS RequiredQty,
				CAST(0 AS DECIMAL(18,2)) AS ReceivedQty,
				CAST(0 AS DECIMAL(18,2)) AS PendingQty,
				g.CreatedBy AS CreatedBy,
				convert(Varchar(10),g.CreatedDate,103) AS CreatedDate
				FROM LabourDcOutgoing g INNER JOIN LabourDcOutgoingSub gs ON g.DcId = gs.DcId
				LEFT JOIN Customer c ON c.CustId = g.CustId
				LEFT JOIN Item i on i.itemid=gs.itemid
				LEFT JOIN MfgPoSub ms on ms.Posubid=gs.RefPoSubid
				LEFT JOIN MfgPo m on m.poid=ms.poid
				WHERE gs.BalQty > 0
					AND (@FromDate IS NULL OR CAST(g.DcDate AS DATE) >= @FromDate)
					AND (@ToDate IS NULL OR CAST(g.DcDate AS DATE) <= @ToDate)
					AND (@CustId = 0 OR g.CustId = @CustId);
			END

			Else IF @DocType = 'Sales Po'
			BEGIN
			   SELECT
				0 AS SlNo,
				c.CustId,
				c.custname as CustName,
				po.PONo As DocNo,
				po.PODate As DocDate,
			    '' AS RefDcNo,
				'' AS RefDcDate,
				'' AS BatchNo,
				po.NoofItems,
				p.[LineNo],
				A.ItemId,
				i.ItemCode AS ItemCode,
				i.ItemName as ItemName,
				im.ItemCode AS ChildItemCode,
				im.ItemName as childItemName,
				i.Specification,
				CAST(0 AS DECIMAL(18,3)) AS Qty,
				CAST(0 AS DECIMAL(18,3)) AS BalQty,
				CAST(A.UtilQty AS DECIMAL(18,3)) AS UtilQty,
				CAST((P.Qty * A.UtilQty) AS DECIMAL(18,3)) AS RequiredQty,
				CAST(ISNULL(SUM(GS.Qty),0) AS DECIMAL(18,3)) AS ReceivedQty,
				CAST((P.Qty * A.UtilQty) - ISNULL(SUM(GS.Qty),0) AS DECIMAL(18,3)) AS PendingQty
				FROM MfgPOSub P
				inner join mfgpo po on p.PoId =po.PoId
				inner join customer c on c.custid=po.custid

				INNER JOIN AssmblyDef A
					ON A.AssmblyID = P.ItemId

				LEFT JOIN Item im
					ON im.ItemId = A.ItemId

				LEFT JOIN LabourGRNSub lg
					ON lg.RefPoSubId = P.PoSubId
					AND lg.TransType = 'Out'

				LEFT JOIN LabourGRNSub GS
					ON GS.GroupId = lg.GroupId
					AND GS.TransType = 'In'
					AND GS.ItemId = A.ItemId and gs.GRNId=lg.GRNId

				LEFT JOIN Item i
					ON i.ItemId = P.ItemId
				WHERE gs.BalQty > 0
							AND (@FromDate IS NULL OR CAST(po.PODate AS DATE) >= @FromDate)
							AND (@ToDate IS NULL OR CAST(po.PODate AS DATE) <= @ToDate)
							AND (@CustId = 0 OR po.CustId = @CustId)
				GROUP BY	
				    c.CustId,
					c.CustName,
					po.pono,
					po.NoofItems,
					po.PODate,
					P.PoSubId,
					p.[LineNo],
					i.ItemCode,
					im.ItemCode,
					im.ItemName,
					i.ItemName,
					i.Specification,
					P.Qty,
					A.ItemId,
					A.UtilQty;
			END
		END
	ELSE
		BEGIN
			IF @DocType = 'Labour GRN'
			BEGIN
				SELECT
					ROW_NUMBER() OVER (ORDER BY g.GRNId DESC) AS SlNo,
					c.CustId,
					c.CustName,
					g.GrnNo+g.Suffix AS DocNo,
					CONVERT(VARCHAR(10), g.GrnDate, 103) AS DocDate,
					g.RefDcNo,
					CONVERT(VARCHAR(10), g.RefDcDate, 103) AS RefDcDate,
					g.BatchNo,
					g.NoOfItems,
					g.CreatedBy,
					CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
				FROM LabourGRN g
				LEFT JOIN Customer c
					ON c.CustId = g.CustId
				WHERE EXISTS
				(
					SELECT 1
					FROM LabourGRNSub gs
					WHERE gs.GRNId = g.GRNId
						AND gs.BalQty > 0
				)
				AND (@FromDate IS NULL OR CAST(g.GrnDate AS DATE) >= @FromDate)
				AND (@ToDate IS NULL OR CAST(g.GrnDate AS DATE) <= @ToDate)
				AND (@CustId = 0 OR g.CustId = @CustId);
			END
			Else If @DocType = 'Labour SCN'
			BEGIN 
				SELECT
			ROW_NUMBER() OVER (ORDER BY g.SCNId DESC) AS SlNo,
			c.CustId,
			c.CustName,
			g.SCNNo+g.Suffix AS DocNo,
			CONVERT(VARCHAR(10), g.SCNDate, 103) AS DocDate,
			'' As RefDcNo,
			'' RefDcDate,
			'' As BatchNo,
			g.NoOfItems,
			g.CreatedBy,
			CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
		FROM LabourSCN g
		LEFT JOIN Customer c
			ON c.CustId = g.CustId
		WHERE EXISTS
		(
			SELECT 1
			FROM LabourSCNSub gs
			WHERE gs.Scnid = g.Scnid
				AND gs.BalQty > 0
		)
		AND (@FromDate IS NULL OR CAST(g.SCNDate AS DATE) >= @FromDate)
		AND (@ToDate IS NULL OR CAST(g.SCNDate AS DATE) <= @ToDate)
		AND (@CustId = 0 OR g.CustId = @CustId);
			END
			Else If @DocType = 'Labour Dc-Out'
			BEGIN 
				SELECT
			ROW_NUMBER() OVER (ORDER BY g.Dcid DESC) AS SlNo,
			c.CustId,
			c.CustName,
			g.DcNo+g.Suffix AS DocNo,
			CONVERT(VARCHAR(10), g.DcDate, 103) AS DocDate,
			'' As RefDcNo,
			'' RefDcDate,
			'' As BatchNo,
			g.NoOfItems,
			g.CreatedBy,
			CONVERT(VARCHAR(10), g.CreatedDate, 103) AS CreatedDate
		FROM LabourDcOutgoing g
		LEFT JOIN Customer c
			ON c.CustId = g.CustId
		WHERE EXISTS
		(
			SELECT 1
			FROM LabourDcOutgoingSub gs
			WHERE gs.DcId = g.Dcid
				AND gs.BalQty > 0
		)
		AND (@FromDate IS NULL OR CAST(g.DcDate AS DATE) >= @FromDate)
		AND (@ToDate IS NULL OR CAST(g.DcDate AS DATE) <= @ToDate)
		AND (@CustId = 0 OR g.CustId = @CustId);
			END
		END
	END
END
