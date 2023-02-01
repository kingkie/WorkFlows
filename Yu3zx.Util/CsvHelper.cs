using System.Data;
using System.IO;
using System.Text;

namespace Yu3zx.Util
{
    /// <summary>
    /// CSV文件转换类
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 导出报表为Csv
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="strFilePath">物理路径</param>
        /// <param name="tableheader">表头</param>
        /// <param name="columname">字段标题,逗号分隔</param>
        public static bool dt2csv(DataTable dt, string strFilePath, string tableheader, string columname)
        {
            try
            {
                string strBufferLine = "";
                StreamWriter strmWriterObj = new StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
                strmWriterObj.WriteLine(tableheader);
                strmWriterObj.WriteLine(columname);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    strBufferLine = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                            strBufferLine += ",";
                        strBufferLine += dt.Rows[i][j].ToString();
                    }
                    strmWriterObj.WriteLine(strBufferLine);
                }
                strmWriterObj.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将Csv读入DataTable
        /// </summary>
        /// <param name="filePath">csv文件路径</param>
        /// <param name="n">表示第n行是字段title,第n+1行是记录开始</param>
        public static DataTable csv2dt(string filePath, int n, DataTable dt)
        {
            StreamReader reader = new StreamReader(filePath, System.Text.Encoding.UTF8, false);
            int i = 0, m = 0;
            reader.Peek();
            while (reader.Peek() > 0)
            {
                m = m + 1;
                string str = reader.ReadLine();
                if (m >= n + 1)
                {
                    string[] split = str.Split(',');

                    System.Data.DataRow dr = dt.NewRow();
                    for (i = 0; i < split.Length; i++)
                    {
                        dr[i] = split[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        /// 将文本文件内容导入到DataTable
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DataTable ConvertTextToTable(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            DataTable mydt = new DataTable("");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mysr = new StreamReader(stream))
                {
                    string strline = mysr.ReadLine();
                    string[] aryline = strline.Split('\t');
                    for (int i = 0; i < aryline.Length; i++)
                    {
                        aryline[i] = aryline[i].Replace("\"", "");
                        mydt.Columns.Add(new DataColumn(aryline[i] + i));
                    }
                    int intColCount = aryline.Length;
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = strline.Split('\t');
                        DataRow mydr = mydt.NewRow();
                        for (int i = 0; i < intColCount; i++)
                        {
                            mydr[i] = aryline[i].Replace("\"", "");
                        }
                        mydt.Rows.Add(mydr);
                    }
                    return mydt;
                }
            }
        }
    }
}
