using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using CsvHelper;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Services
{
    public class NexusPayablesService
    {
        private readonly string _connectionString;
        private readonly ILogger<NexusPayablesService> _logger;

        public NexusPayablesService(IConfiguration configuration, ILogger<NexusPayablesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public void ImportNexusCSV(string filePath, DateTime systemDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ClearExistingData(connection, transaction);
                        ImportCSVData(filePath, systemDate, connection, transaction);
                        CorrectDateAndDeleteNULines(systemDate, connection, transaction);
                        ProcessTempCurrentProperties(connection, transaction);
                        MarkExemptLines(connection, transaction);
                        MarkAsNPVendor(connection, transaction);

                        transaction.Commit();
                        _logger.LogInformation($"CSV import completed successfully at {DateTime.Now}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Error during CSV import");
                        throw;
                    }
                }
            }
        }

        private void ClearExistingData(SqlConnection connection, SqlTransaction transaction)
        {
            using var command = new SqlCommand("DELETE FROM 1_New_tblNexus_IBS_CSV_Processing", connection, transaction);
            command.ExecuteNonQuery();
            _logger.LogInformation("Existing data cleared");
        }

        private void ImportCSVData(string filePath, DateTime systemDate, SqlConnection connection, SqlTransaction transaction)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var sql = @"INSERT INTO 1_New_tblNexus_IBS_CSV_Processing 
                            (VENDOR_ID, DATE_IN, DATE_PD, AMOUNT, G_L_, INVOICE_, ENTITY, PROPERTY, TENANT, 
                            PAY_DATE, CHECK_NUMB, DESCRIPTIO, BATCH, PHASE, BUILDING, UNIT, DISCOUNT_A, DISCOUNT_D,
                            FLAG1, FLAG2, FLAG3, FLAG4, FLAG5, FLAG6, FLAG7, FLAG8, FLAG9, FLAG10, K1, 
                            Invoice_Number, DATEIN, FILENAME, Property_Nexus, AmountC, DateUploaded, SysDate)
                            VALUES (@VENDOR_ID, @DATE_IN, @DATE_PD, @AMOUNT, @G_L_, @INVOICE_, @ENTITY, @PROPERTY, @TENANT,
                            @PAY_DATE, @CHECK_NUMB, @DESCRIPTIO, @BATCH, @PHASE, @BUILDING, @UNIT, @DISCOUNT_A, @DISCOUNT_D,
                            @FLAG1, @FLAG2, @FLAG3, @FLAG4, @FLAG5, @FLAG6, @FLAG7, @FLAG8, @FLAG9, @FLAG10, @K1,
                            @Invoice_Number, @DATEIN, @FILENAME, @Property_Nexus, @AmountC, @DateUploaded, @SysDate)";

                using var command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@VENDOR_ID", csv.GetField("VENDOR_ID"));
                command.Parameters.AddWithValue("@DATE_IN", DateTime.Parse(csv.GetField("DATE_IN")));
                command.Parameters.AddWithValue("@DATE_PD", DateTime.Parse(csv.GetField("DATE_PD")));
                command.Parameters.AddWithValue("@AMOUNT", decimal.Parse(csv.GetField("AMOUNT")));
                command.Parameters.AddWithValue("@G_L_", csv.GetField("G_L_"));
                command.Parameters.AddWithValue("@INVOICE_", csv.GetField("INVOICE_"));
                command.Parameters.AddWithValue("@ENTITY", csv.GetField("ENTITY"));
                command.Parameters.AddWithValue("@PROPERTY", csv.GetField("PROPERTY"));
                command.Parameters.AddWithValue("@TENANT", csv.GetField("TENANT"));
                command.Parameters.AddWithValue("@PAY_DATE", DateTime.Parse(csv.GetField("PAY_DATE")));
                command.Parameters.AddWithValue("@CHECK_NUMB", csv.GetField("CHECK_NUMB"));
                command.Parameters.AddWithValue("@DESCRIPTIO", csv.GetField("DESCRIPTIO"));
                command.Parameters.AddWithValue("@BATCH", csv.GetField("BATCH"));
                command.Parameters.AddWithValue("@PHASE", csv.GetField("PHASE"));
                command.Parameters.AddWithValue("@BUILDING", csv.GetField("BUILDING"));
                command.Parameters.AddWithValue("@UNIT", csv.GetField("UNIT"));
                command.Parameters.AddWithValue("@DISCOUNT_A", decimal.Parse(csv.GetField("DISCOUNT_A")));
                command.Parameters.AddWithValue("@DISCOUNT_D", DateTime.Parse(csv.GetField("DISCOUNT_D")));
                command.Parameters.AddWithValue("@FLAG1", csv.GetField("FLAG1"));
                command.Parameters.AddWithValue("@FLAG2", csv.GetField("FLAG2"));
                command.Parameters.AddWithValue("@FLAG3", csv.GetField("FLAG3"));
                command.Parameters.AddWithValue("@FLAG4", csv.GetField("FLAG4"));
                command.Parameters.AddWithValue("@FLAG5", csv.GetField("FLAG5"));
                command.Parameters.AddWithValue("@FLAG6", csv.GetField("FLAG6"));
                command.Parameters.AddWithValue("@FLAG7", csv.GetField("FLAG7"));
                command.Parameters.AddWithValue("@FLAG8", csv.GetField("FLAG8"));
                command.Parameters.AddWithValue("@FLAG9", csv.GetField("FLAG9"));
                command.Parameters.AddWithValue("@FLAG10", csv.GetField("FLAG10"));
                command.Parameters.AddWithValue("@K1", csv.GetField("K1"));
                command.Parameters.AddWithValue("@Invoice_Number", csv.GetField("INVOICE_"));
                command.Parameters.AddWithValue("@DATEIN", DateTime.Parse(csv.GetField("DATE_IN")));
                command.Parameters.AddWithValue("@FILENAME", Path.GetFileName(filePath));
                command.Parameters.AddWithValue("@Property_Nexus", csv.GetField("PROPERTY"));
                command.Parameters.AddWithValue("@AmountC", decimal.Parse(csv.GetField("AMOUNT")));
                command.Parameters.AddWithValue("@DateUploaded", DateTime.Now);
                command.Parameters.AddWithValue("@SysDate", systemDate);

                command.ExecuteNonQuery();
            }
            _logger.LogInformation($"CSV data imported from {filePath}");
        }

        private void CorrectDateAndDeleteNULines(DateTime systemDate, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"UPDATE 1_New_tblNexus_IBS_CSV_Processing 
                        SET DATE_IN = @SystemDate
                        WHERE CAST(DATE_IN AS DATE) < @SystemDate;
                        DELETE FROM 1_New_tblNexus_IBS_CSV_Processing WHERE K1 = 'NU';";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@SystemDate", systemDate);
            command.ExecuteNonQuery();
            _logger.LogInformation("Dates corrected and NU lines deleted");
        }

        private void ProcessTempCurrentProperties(SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"DELETE FROM TempCurrentProperties;
                        INSERT INTO TempCurrentProperties (PROPERTY)
                        SELECT DISTINCT PROPERTY FROM 1_New_tblNexus_IBS_CSV_Processing;";

            using var command = new SqlCommand(sql, connection, transaction);
            command.ExecuteNonQuery();
            _logger.LogInformation("Temporary current properties processed");
        }

        private void MarkExemptLines(SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"UPDATE n
                        SET n.ExemptNP = 1
                        FROM 1_New_tblNexus_IBS_CSV_Processing n
                        INNER JOIN I4_ExemptProp e ON e.Field1 = n.PROPERTY";

            using var command = new SqlCommand(sql, connection, transaction);
            command.ExecuteNonQuery();
            _logger.LogInformation("Exempt lines marked");
        }

        private void MarkAsNPVendor(SqlConnection connection, SqlTransaction transaction)
        {
            var sql = @"UPDATE n
                        SET n.Is_NP_Vendor = 1
                        FROM 1_New_tblNexus_IBS_CSV_Processing n
                        INNER JOIN [I3_NexusConnect Suppliers4-21-2] s ON s.Vendor_Num = n.VENDOR_ID
                        WHERE (s.ConnectOne = 1 AND n.Property_Nexus = 'FM')
                        OR s.NesusVendor = 1";

            using var command = new SqlCommand(sql, connection, transaction);
            command.ExecuteNonQuery();
            _logger.LogInformation("NP vendors marked");
        }
    }
}