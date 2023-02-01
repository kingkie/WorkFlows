using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace Yu3zx.Util
{
    public enum ExcelColumnType
    {
        Decimal,
        Integer,
        String,
        Date
    }

    public class ExcelHelper
    {
        /// 将ListView保存为Excel文件
        /// <param name="listView"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool SaveListViewToExcelFile(ListView listView, string filename)
        {
            return SaveListViewToExcelFile(listView, new List<ExcelColumnType>(), filename);
        }

        /// 将ListView保存为Excel文件(指定列类型)
        /// <param name="listView"></param>
        /// <param name="colTypes"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool SaveListViewToExcelFile(ListView listView, List<ExcelColumnType> colTypes, string filename)
        {
            var excelDoc = new StreamWriter(filename);
            const string startExcelXml = @"<xml version> <Workbook " +
                  "xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" " +
                  " xmlns:o=\"urn:schemas-microsoft-com:office:office\"  " +
                  "xmlns:x=\"urn:schemas-    microsoft-com:office:" +
                  "excel\"  xmlns:ss=\"urn:schemas-microsoft-com:" +
                  "office:spreadsheet\">  <Styles>  " +
                  "<Style ss:ID=\"Default\" ss:Name=\"Normal\">  " +
                  "<Alignment ss:Vertical=\"Bottom\"/>  <Borders/>" +
                  "  <Font/>  <Interior/>  <NumberFormat/>" +
                  "  <Protection/>  </Style>  " +
                  "<Style ss:ID=\"BoldColumn\">  <Font " +
                  "x:Family=\"宋体\" ss:Bold=\"0\"/>  </Style>  " +
                  "<Style     ss:ID=\"StringLiteral\">  <NumberFormat" +
                  " ss:Format=\"@\"/>  </Style>  <Style " +
                  "ss:ID=\"Decimal\">  <NumberFormat " +
                  "ss:Format=\"0.00\"/>  </Style>  " +
                  "<Style ss:ID=\"Integer\">  <NumberFormat " +
                  "ss:Format=\"0\"/>  </Style>  <Style " +
                  "ss:ID=\"DateLiteral\">  <NumberFormat " +
                  "ss:Format=\"yyyy-MM-dd HH:mm:ss\"/>  </Style>  " +
                  "</Styles>  ";
            const string endExcelXml = "</Workbook>";

            int rowCount = 0;
            int sheetCount = 1;

            excelDoc.Write(startExcelXml);
            excelDoc.Write("<Worksheet ss:Name=\"Sheet" + sheetCount + "\">");
            excelDoc.Write("<Table ss:DefaultColumnWidth=\"90\">");
            excelDoc.Write("<Row>");
            for (int x = 0; x < listView.Columns.Count; x++)
            {
                excelDoc.Write("<Cell ss:StyleID=\"BoldColumn\"><Data ss:Type=\"String\">");
                excelDoc.Write(listView.Columns[x].Text);
                excelDoc.Write("</Data></Cell>");
            }
            excelDoc.Write("</Row>");
            for (int i = 0; i < listView.Items.Count; i++)
            {
                rowCount++;
                //if the number of rows is > 64000 create a new page to continue output
                if (rowCount == 64000)
                {
                    rowCount = 0;
                    sheetCount++;
                    excelDoc.Write("</Table>");
                    excelDoc.Write(" </Worksheet>");
                    excelDoc.Write("<Worksheet ss:Name=\"Sheet" + sheetCount + "\">");
                    excelDoc.Write("<Table>");
                }
                excelDoc.Write("<Row>"); //ID=" + rowCount + "
                ListViewItem item = listView.Items[i];
                for (int y = 0; y < listView.Columns.Count; y++)
                {
                    if (y >= item.SubItems.Count) continue;

                    ExcelColumnType colType = ExcelColumnType.String;
                    if (y < colTypes.Count)
                    {
                        colType = colTypes[y];
                    }
                    switch (colType)
                    {
                        case ExcelColumnType.Decimal:
                            excelDoc.Write("<Cell ss:StyleID=\"Decimal\">" +
                            "<Data ss:Type=\"Number\">");
                            if (item.SubItems[y].Text != "--")
                            {
                                excelDoc.Write(item.SubItems[y].Text);
                            }
                            excelDoc.Write("</Data></Cell>");
                            break;
                        case ExcelColumnType.Integer:
                            excelDoc.Write("<Cell ss:StyleID=\"Integer\">" +
                           "<Data ss:Type=\"Number\">");
                            if (item.SubItems[y].Text != "--")
                            {
                                excelDoc.Write(item.SubItems[y].Text);
                            }
                            excelDoc.Write("</Data></Cell>");
                            break;
                        case ExcelColumnType.String:
                            string XMLstring = item.SubItems[y].Text;
                            XMLstring = XMLstring.Trim();
                            XMLstring = XMLstring.Replace("&", "&");
                            XMLstring = XMLstring.Replace(">", ">");
                            XMLstring = XMLstring.Replace("<", "<");
                            excelDoc.Write("<Cell ss:StyleID=\"StringLiteral\">" +
                                           "<Data ss:Type=\"String\">");
                            excelDoc.Write(XMLstring);
                            excelDoc.Write("</Data></Cell>");
                            break;
                        case ExcelColumnType.Date:
                            DateTime XMLDate = DateTime.Parse(item.SubItems[y].Text);
                            string XMLDatetoString = ""; //Excel Converted Date
                            XMLDatetoString = XMLDate.Year.ToString() +
                                 "-" +
                                 (XMLDate.Month < 10 ? "0" +
                                 XMLDate.Month.ToString() : XMLDate.Month.ToString()) +
                                 "-" +
                                 (XMLDate.Day < 10 ? "0" +
                                 XMLDate.Day.ToString() : XMLDate.Day.ToString()) +
                                 "T" +
                                 (XMLDate.Hour < 10 ? "0" +
                                 XMLDate.Hour.ToString() : XMLDate.Hour.ToString()) +
                                 ":" +
                                 (XMLDate.Minute < 10 ? "0" +
                                 XMLDate.Minute.ToString() : XMLDate.Minute.ToString()) +
                                 ":" +
                                 (XMLDate.Second < 10 ? "0" +
                                 XMLDate.Second.ToString() : XMLDate.Second.ToString()) +
                                 ".000";
                            excelDoc.Write("<Cell ss:StyleID=\"DateLiteral\">" +
                                         "<Data ss:Type=\"DateTime\">");
                            excelDoc.Write(XMLDatetoString);
                            excelDoc.Write("</Data></Cell>");
                            break;
                        default:
                            XMLstring = item.SubItems[y].Text;
                            XMLstring = XMLstring.Trim();
                            XMLstring = XMLstring.Replace("&", "&");
                            XMLstring = XMLstring.Replace(">", ">");
                            XMLstring = XMLstring.Replace("<", "<");
                            excelDoc.Write("<Cell ss:StyleID=\"StringLiteral\">" +
                                           "<Data ss:Type=\"String\">");
                            excelDoc.Write(XMLstring);
                            excelDoc.Write("</Data></Cell>");
                            break;
                    }
                }
                excelDoc.Write("</Row>");
            }
            excelDoc.Write("</Table>");
            excelDoc.Write(" </Worksheet>");
            excelDoc.Write(endExcelXml);
            excelDoc.Close();
            return true;
        }

    }
}
