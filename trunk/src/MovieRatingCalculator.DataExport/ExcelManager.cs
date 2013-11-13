using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MovieRatingCalculator.DataExport
{
    public class ExcelManager
    {
        public static Byte[] GetBytes<T>(List<T> list)
        {
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet ws = CreateWorksheet(package,
                                                    String.Format("{0}List", typeof (T).Name));

                int colIndex = 1;
                int rowIndex = 1;

                var properties = TypeDescriptor.GetProperties(typeof (T));
                foreach (PropertyDescriptor prop in properties)
                {
                    SetHeaderCell(ws.Cells[rowIndex, colIndex], prop.Name);
                    colIndex++;
                }

                foreach (var item in list)
                {
                    colIndex = 1;
                    rowIndex++;
                    foreach (PropertyDescriptor prop in properties)
                    {
                        SetRowCell(ws.Cells[rowIndex, colIndex], prop.GetValue(item) ?? DBNull.Value);
                        colIndex++;
                    }
                }

                return package.GetAsByteArray();
            }
        }

        public static Byte[] GetBytes(DataTable table)
        {
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet ws = CreateWorksheet(package, table.TableName);

                int colIndex = 1;
                int rowIndex = 1;

                foreach (DataColumn column in table.Columns)
                {
                    SetHeaderCell(ws.Cells[rowIndex, colIndex], column.ColumnName);
                    colIndex++;
                }

                foreach (DataRow item in table.Rows)
                {
                    colIndex = 1;
                    rowIndex++;
                    foreach (DataColumn column in table.Columns)
                    {
                        SetRowCell(ws.Cells[rowIndex, colIndex], item[column.ColumnName]);
                        colIndex++;
                    }
                }
                
                return package.GetAsByteArray();
            }
        }

        #region Private methods

        private static ExcelWorksheet CreateWorksheet(ExcelPackage package, string sheetName)
        {
            package.Workbook.Worksheets.Add(sheetName);
            ExcelWorksheet ws = package.Workbook.Worksheets[1];
            ws.Name = sheetName;
            return ws;
        }

        private static void SetHeaderCell(ExcelRange cell, string value)
        {
            var fill = cell.Style.Fill;
            fill.PatternType = ExcelFillStyle.Solid;
            fill.BackgroundColor.SetColor(Color.LightSlateGray);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Value = value;
        }

        private static void SetRowCell(ExcelRange cell, object value)
        {
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            cell.Value = value;
        }

        #endregion
    }
}