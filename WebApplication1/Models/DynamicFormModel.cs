using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class TableSchema
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
    }

    public class DynamicFormModel
    {
        public string TableName { get; set; }
        public List<TableSchema> Columns { get; set; }
        public Dictionary<string, string> FormData { get; set; }
        public List<Dictionary<string, object>> GridData { get; set; }
    }
}