using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Cashflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Income",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Expense",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "Bank",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpiId",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpiName",
                table: "Bank",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Advaceadjustment",
                columns: table => new
                {
                    AdjumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvaceadjustmentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdjumentDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseCode = table.Column<int>(type: "int", nullable: true),
                    IncomeCode = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayToRefCode = table.Column<int>(type: "int", nullable: true),
                    PayToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsIncome = table.Column<bool>(type: "bit", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advaceadjustment", x => x.AdjumentId);
                    table.ForeignKey(
                        name: "FK_Advaceadjustment_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advaceadjustment_Expense_ExpenseCode",
                        column: x => x.ExpenseCode,
                        principalTable: "Expense",
                        principalColumn: "ExpenseCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advaceadjustment_Income_IncomeCode",
                        column: x => x.IncomeCode,
                        principalTable: "Income",
                        principalColumn: "IncomeCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseCode = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    PayToRefCode = table.Column<int>(type: "int", nullable: true),
                    PayToName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Expense_ExpenseCode",
                        column: x => x.ExpenseCode,
                        principalTable: "Expense",
                        principalColumn: "ExpenseCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    ReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncomeCode = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    PayFromRefCode = table.Column<int>(type: "int", nullable: true),
                    PayFromName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_Receipts_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Income_IncomeCode",
                        column: x => x.IncomeCode,
                        principalTable: "Income",
                        principalColumn: "IncomeCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvaceadjustmentSub",
                columns: table => new
                {
                    AdjustSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjumentId = table.Column<int>(type: "int", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefId = table.Column<int>(type: "int", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavedAdjustAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvaceadjustmentSub", x => x.AdjustSubId);
                    table.ForeignKey(
                        name: "FK_AdvaceadjustmentSub_Advaceadjustment_AdjumentId",
                        column: x => x.AdjumentId,
                        principalTable: "Advaceadjustment",
                        principalColumn: "AdjumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsSub",
                columns: table => new
                {
                    PaymentSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefId = table.Column<int>(type: "int", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsSub", x => x.PaymentSubId);
                    table.ForeignKey(
                        name: "FK_PaymentsSub_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundTrans",
                columns: table => new
                {
                    FundTransId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransNo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentRefId = table.Column<int>(type: "int", nullable: true),
                    ReceiptRefId = table.Column<int>(type: "int", nullable: true),
                    AdvanceRefId = table.Column<int>(type: "int", nullable: true),
                    ExpenseCode = table.Column<int>(type: "int", nullable: true),
                    IncomeCode = table.Column<int>(type: "int", nullable: true),
                    TransType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TrasactionMode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    LedgerId = table.Column<int>(type: "int", nullable: true),
                    LedgerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LedgerType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpenseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChequeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidOrReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RunningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTrans", x => x.FundTransId);
                    table.ForeignKey(
                        name: "FK_FundTrans_Advaceadjustment_AdvanceRefId",
                        column: x => x.AdvanceRefId,
                        principalTable: "Advaceadjustment",
                        principalColumn: "AdjumentId");
                    table.ForeignKey(
                        name: "FK_FundTrans_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundTrans_Expense_ExpenseCode",
                        column: x => x.ExpenseCode,
                        principalTable: "Expense",
                        principalColumn: "ExpenseCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTrans_Income_IncomeCode",
                        column: x => x.IncomeCode,
                        principalTable: "Income",
                        principalColumn: "IncomeCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTrans_Payments_PaymentRefId",
                        column: x => x.PaymentRefId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_FundTrans_Receipts_ReceiptRefId",
                        column: x => x.ReceiptRefId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId");
                });

            migrationBuilder.CreateTable(
                name: "ReceiptsSub",
                columns: table => new
                {
                    ReceiptSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptId = table.Column<int>(type: "int", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefId = table.Column<int>(type: "int", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptsSub", x => x.ReceiptSubId);
                    table.ForeignKey(
                        name: "FK_ReceiptsSub_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advaceadjustment_BankId",
                table: "Advaceadjustment",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Advaceadjustment_ExpenseCode",
                table: "Advaceadjustment",
                column: "ExpenseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Advaceadjustment_IncomeCode",
                table: "Advaceadjustment",
                column: "IncomeCode");

            migrationBuilder.CreateIndex(
                name: "IX_AdvaceadjustmentSub_AdjumentId",
                table: "AdvaceadjustmentSub",
                column: "AdjumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_AdvanceRefId",
                table: "FundTrans",
                column: "AdvanceRefId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_BankId",
                table: "FundTrans",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_ExpenseCode",
                table: "FundTrans",
                column: "ExpenseCode");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_IncomeCode",
                table: "FundTrans",
                column: "IncomeCode");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_PaymentRefId",
                table: "FundTrans",
                column: "PaymentRefId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTrans_ReceiptRefId",
                table: "FundTrans",
                column: "ReceiptRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BankId",
                table: "Payments",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExpenseCode",
                table: "Payments",
                column: "ExpenseCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsSub_PaymentId",
                table: "PaymentsSub",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_BankId",
                table: "Receipts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_IncomeCode",
                table: "Receipts",
                column: "IncomeCode");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptsSub_ReceiptId",
                table: "ReceiptsSub",
                column: "ReceiptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvaceadjustmentSub");

            migrationBuilder.DropTable(
                name: "FundTrans");

            migrationBuilder.DropTable(
                name: "PaymentsSub");

            migrationBuilder.DropTable(
                name: "ReceiptsSub");

            migrationBuilder.DropTable(
                name: "Advaceadjustment");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Income");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "UpiId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "UpiName",
                table: "Bank");
        }
    }
}
