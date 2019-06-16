using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BoilerLevel.Utils
{
    public class ExcelManager
    {
        private readonly ExcelWorksheet Worksheet;
        private readonly ExcelPackage ExcelPackage;
        private int ColumnNumber = 1;

        public ExcelManager()
        {
            ExcelPackage = new ExcelPackage();
            Worksheet = ExcelPackage.Workbook.Worksheets.Add("Sheet 1");
        }

        public ExcelManager AddTable<Tx, Ty>(string name, IEnumerable<Tx> Xdata, IEnumerable<Ty> Ydata)
        {
            int FirstRow = 2;
            int LastRow = Xdata.Count() + FirstRow + 1;

            int FirstColumn = ColumnNumber;
            int LastColumn = ColumnNumber + 1;

            var cell = Worksheet.Cells[1, FirstColumn, 1, LastColumn];
            cell.Merge = true;
            cell.Value = name;
            cell.Style.Font.Bold = true;

            var Xcell = Worksheet.Cells[FirstRow + 1, FirstColumn];
            Xcell.Value = "X";
            Xcell.Style.Font.UnderLine = true;

            var Ycell = Worksheet.Cells[FirstRow + 1, FirstColumn + 1];
            Ycell.Value = "Y";
            Ycell.Style.Font.UnderLine = true;

            for (int i = 0; i < Xdata.Count(); i++)
            {
                Worksheet.Cells[i + FirstRow + 2, FirstColumn].Value = Xdata.ElementAt(i);
            }

            for (int i = 0; i < Ydata.Count(); i++)
            {
                Worksheet.Cells[i + FirstRow + 2, FirstColumn + 1].Value = Ydata.ElementAt(i);
            }

            ExcelAddress range = Worksheet.Cells[FirstRow, FirstColumn, LastRow, LastColumn];
            Worksheet.Tables.Add(range, name);

            ColumnNumber += 3;
            return this;
        }

        public string Save(string filename)
        {
            var envpath = Xamarin.Essentials.FileSystem.AppDataDirectory;
            var path = Path.Combine(envpath, filename + ".xlsx");
            FileStream fileStream = File.Create(path);

            ExcelPackage.SaveAs(fileStream);
            fileStream.Close();
            return path;
        }

        public void SaveAs(FileStream fileStream)
        {
            ExcelPackage.SaveAs(fileStream);
            fileStream.Close();
        }
    }
}