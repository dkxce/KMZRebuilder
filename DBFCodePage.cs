using System;
using System.Collections.Generic;
using System.Text;

namespace KMZRebuilder.DBF
{

    public class CodePageSet
    {
        public byte headerCode = 0;
        public int codePage = 0;
        public string codeName = "UNKNOWN";

        public Encoding Encoding
        {
            get
            {
                return System.Text.Encoding.GetEncoding(codePage);
            }
        }

        public CodePageSet() { }

        public static CodePageSet Default
        {
            get
            {
                CodePageSet result = new CodePageSet();
                result.headerCode = 201;
                result.codePage = 1251;
                result.codeName = @"Russian Windows \ Windows-1251 [0xC9]";
                return result;
            }
        }

        public override string ToString()
        {
            return codeName;
        }
    }

    public class CodePageList : List<CodePageSet>
    {
        public CodePageList()
        {
            this.Add(204, 01257, "Baltic Windows");
            this.Add(079, 00950, "Chinese Big5 (Taiwan)");
            this.Add(077, 00936, "Chinese GBK (PRC)");
            this.Add(122, 00936, "PRC GBK");
            this.Add(031, 00852, "Czech OEM");
            this.Add(008, 00865, "Danish OEM");
            this.Add(009, 00437, "Dutch OEM");
            this.Add(010, 00850, "Dutch OEM*");
            this.Add(025, 00437, "English OEM (Great Britain)");
            this.Add(026, 00850, "English OEM (Great Britain)*");
            this.Add(027, 00437, "English OEM (US)");
            this.Add(055, 00850, "English OEM (US)*");
            this.Add(200, 01250, "Eastern European Windows");
            this.Add(100, 00852, "Eastern European MS-DOS");
            this.Add(151, 10029, "Eastern European Macintosh");
            this.Add(011, 00437, "Finnish OEM");
            this.Add(013, 00437, "French OEM");
            this.Add(014, 00850, "French OEM*");
            this.Add(029, 00850, "French OEM*2");
            this.Add(028, 00863, "French OEM (Canada)");
            this.Add(108, 00863, "French-Canadian MS-DOS");
            this.Add(015, 00437, "German OEM");
            this.Add(016, 00850, "German OEM*");
            this.Add(203, 01253, "Greek Windows");
            this.Add(106, 00737, "Greek MS-DOS (437G)");
            this.Add(134, 00737, "Greek OEM");
            this.Add(152, 00006, "Greek Macintosh");
            this.Add(121, 00949, "Hangul (Wansung)");
            this.Add(034, 00852, "Hungarian OEM");
            this.Add(103, 00861, "Icelandic MS-DOS");
            this.Add(017, 00437, "Italian OEM");
            this.Add(018, 00850, "Italian OEM*");
            this.Add(019, 00932, "Japanese Shift-JIS");
            this.Add(123, 00932, "Japanese Shift-JIS 2");
            this.Add(104, 00895, "Kamenicky (Czech) MS-DOS");
            this.Add(078, 00949, "Korean (ANSI/OEM)");
            this.Add(105, 00620, "Mazovia (Polish) MS-DOS");
            this.Add(102, 00865, "Nordic MS-DOS");
            this.Add(023, 00865, "Norwegian OEM");
            this.Add(035, 00852, "Polish OEM");
            this.Add(036, 00860, "Portuguese OEM");
            this.Add(037, 00850, "Portuguese OEM*");
            this.Add(064, 00852, "Romanian OEM");
            this.Add(201, 01251, "Russian Windows");
            this.Add(101, 00866, "Russian MS-DOS");
            this.Add(038, 00866, "Russian OEM");
            this.Add(150, 10007, "Russian Macintosh");
            this.Add(135, 00852, "Slovenian OEM");
            this.Add(089, 01252, "Spanish ANSI");
            this.Add(020, 00850, "Spanish OEM*");
            this.Add(021, 00437, "Swedish OEM");
            this.Add(022, 00850, "Swedish OEM*");
            this.Add(024, 00437, "Spanish OEM");
            this.Add(087, 01250, "Standard ANSI");
            this.Add(003, 01252, "Standard Windows ANSI Latin I");
            this.Add(002, 00850, "Standard International MS-DOS");
            this.Add(004, 10000, "Standard Macintosh");
            this.Add(120, 00950, "Taiwan Big 5");
            this.Add(080, 00874, "Thai (ANSI/OEM)");
            this.Add(124, 00874, "Thai Windows/MS–DOS");
            this.Add(202, 01254, "Turkish Windows");
            this.Add(107, 00857, "Turkish MS-DOS");
            this.Add(136, 00857, "Turkish OEM");
            this.Add(001, 00437, "US MS-DOS");
            this.Add(088, 01252, "Western European ANSI");
        }

        private void Add(byte headerCode, int codePage, string codeName)
        {
            CodePageSet cpc = new CodePageSet();
            cpc.headerCode = headerCode;
            cpc.codePage = codePage;
            try
            {
                cpc.codeName = codeName + " ";
                Encoding enc = System.Text.Encoding.GetEncoding(cpc.codePage);
                if ((enc.EncodingName.ToUpper().IndexOf("DOS") >= 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") < 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") < 0))
                    cpc.codeName += @"\ DOS-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else if ((enc.EncodingName.ToUpper().IndexOf("DOS") < 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") >= 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") < 0))
                    cpc.codeName += @"\ Windows-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else if ((enc.EncodingName.ToUpper().IndexOf("DOS") < 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") < 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") >= 0))
                    cpc.codeName += @"\ OEM-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else
                    cpc.codeName += @" \ " + enc.EncodingName;
            }
            catch
            {
                cpc.codeName = codeName + @" \ --**--UNKNOWN--**-- ";
            };
            cpc.codeName += String.Format(@" -- 0x{0:X2}", cpc.headerCode);
            this.Add(cpc);
        }

        public CodePageSet this[byte headerCode]
        {
            get
            {
                if (this.Count == 0) return new CodePageSet();
                foreach (CodePageSet cpc in this)
                    if (cpc.headerCode == headerCode)
                        return cpc;
                return new CodePageSet();
            }
        }
    }
}
