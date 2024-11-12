using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IDynamicFormService
    {
        DynamicFormModel GetFormModel(string tableName);
        List<Dictionary<string, object>> GetTableData(string tableName);
        void SaveFormData(string tableName, Dictionary<string, string> formData);
        byte[] GenerateReport(string tableName);
        List<PropertyListingViewModel> GetPropertyListing();
        bool ValidateTableName(string tableName);
    }

    public class DynamicFormService : IDynamicFormService
    {
        private readonly string _connectionString;
        private readonly ILogger<DynamicFormService> _logger;
        private const string VIEW_NAME = "vwInHousePropertyListing";

        public DynamicFormService(IConfiguration configuration, ILogger<DynamicFormService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateTableName(string tableName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT OBJECT_ID(@TableName, 'U')", connection))
                    {
                        command.Parameters.AddWithValue("@TableName", tableName);
                        return command.ExecuteScalar() != DBNull.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating table name: {TableName}", tableName);
                return false;
            }
        }

        public DynamicFormModel GetFormModel(string tableName)
        {
            if (!ValidateTableName(tableName))
            {
                throw new ArgumentException($"Invalid table name: {tableName}");
            }

            var model = new DynamicFormModel
            {
                TableName = tableName,
                Columns = new List<TableSchema>(),
                FormData = new Dictionary<string, string>(),
                GridData = GetTableData(tableName)
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var schema = connection.GetSchema("Columns", new[] { null, null, tableName });

                foreach (DataRow column in schema.Rows)
                {
                    model.Columns.Add(new TableSchema
                    {
                        ColumnName = column["COLUMN_NAME"].ToString(),
                        DataType = column["DATA_TYPE"].ToString(),
                        IsNullable = Convert.ToBoolean(column["IS_NULLABLE"]),
                        IsIdentity = IsIdentityColumn(connection, tableName, column["COLUMN_NAME"].ToString())
                    });
                }
            }

            return model;
        }

        public List<Dictionary<string, object>> GetTableData(string tableName)
        {
            if (!ValidateTableName(tableName))
            {
                throw new ArgumentException($"Invalid table name: {tableName}");
            }

            var result = new List<Dictionary<string, object>>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand($"SELECT * FROM [{tableName}]", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader[i];
                            }
                            result.Add(row);
                        }
                    }
                }
            }
            return result;
        }

        public void SaveFormData(string tableName, Dictionary<string, string> formData)
        {
            if (!ValidateTableName(tableName))
            {
                throw new ArgumentException($"Invalid table name: {tableName}");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var columns = string.Join(", ", formData.Keys.Select(k => $"[{k}]"));
                        var parameters = string.Join(", ", formData.Keys.Select(k => $"@{k}"));

                        var sql = $"INSERT INTO [{tableName}] ({columns}) VALUES ({parameters})";

                        using (var command = new SqlCommand(sql, connection, transaction))
                        {
                            foreach (var item in formData)
                            {
                                command.Parameters.AddWithValue($"@{item.Key}",
                                    string.IsNullOrEmpty(item.Value) ? DBNull.Value : (object)item.Value);
                            }
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<PropertyListingViewModel> GetPropertyListing()
        {
            var properties = new List<PropertyListingViewModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand($"SELECT * FROM {VIEW_NAME}", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var property = new PropertyListingViewModel
                                {
                                    PropertyID = reader["PropertyID"].ToString(),
                                    MRI_PropertyID = reader["MRI_PropertyID"]?.ToString(),
                                    PropertyName = reader["PropertyName"]?.ToString(),
                                    Street = reader["Street"]?.ToString(),
                                    City = reader["City"]?.ToString(),
                                    State = reader["State"]?.ToString(),
                                    Zip = reader["Zip"]?.ToString(),
                                    Email = reader["Email"]?.ToString(),
                                    PropTel = reader["PropTel"]?.ToString(),
                                    Fax = reader["Fax"]?.ToString(),
                                    NoOfUnits = reader["NoOfUnits"] != DBNull.Value ?
                                        Convert.ToInt32(reader["NoOfUnits"]) : null,
                                    TaxID = reader["TaxID"]?.ToString(),
                                    DatePurchased = reader["DatePurchased"] != DBNull.Value ?
                                        Convert.ToDateTime(reader["DatePurchased"]) : null,
                                    County = reader["County"]?.ToString(),
                                    Borough = reader["Borough"]?.ToString(),
                                    Township = reader["Township"]?.ToString(),
                                    SchoolDistrict = reader["SchoolDistrict"]?.ToString(),
                                    PropMgr = reader["PropMgr"]?.ToString(),
                                    CoPropMgr = reader["CoPropMgr"]?.ToString(),
                                    AdmAsst = reader["AdmAsst"]?.ToString(),
                                    EntityID = reader["EntityID"]?.ToString(),
                                    EntityName = reader["EntityName"]?.ToString(),
                                    DBA = reader["DBA"]?.ToString(),
                                    StaffAcct = reader["StaffAcct"]?.ToString(),
                                    AR = reader["AR"]?.ToString(),
                                    AP = reader["AP"]?.ToString(),
                                    AP_CAPEX = reader["AP_CAPEX"]?.ToString(),
                                    OnsiteMgr = reader["OnsiteMgr"]?.ToString(),
                                    OMBeeper = reader["OMBeeper"]?.ToString(),
                                    Supt = reader["Supt"]?.ToString(),
                                    SuptTel = reader["SuptTel"]?.ToString(),
                                    SuptBeeper = reader["SuptBeeper"]?.ToString(),
                                    CSZ = $"{reader["City"]?.ToString()}, {reader["State"]?.ToString()} {reader["Zip"]?.ToString()}",
                                    ARMemo = reader["ARMemo"]?.ToString(),
                                    ConstructionMgr = reader["ConstructionMgr"]?.ToString()
                                };
                                properties.Add(property);
                            }
                        }
                    }
                }

                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving properties from view");
                throw;
            }
        }

        public byte[] GenerateReport(string tableName)
        {
            // TODO: Implement report generation
            throw new NotImplementedException("Report generation not yet implemented");
        }

        private bool IsIdentityColumn(SqlConnection connection, string tableName, string columnName)
        {
            using (var command = new SqlCommand(
                "SELECT COLUMNPROPERTY(OBJECT_ID(@TableName), @ColumnName, 'IsIdentity')",
                connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@ColumnName", columnName);
                return Convert.ToBoolean(command.ExecuteScalar());
            }
        }
    }
}