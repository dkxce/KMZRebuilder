using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace KMZRebuilder
{
    public class RussianHighwaysAjaxImporter
    {
        public class YMapsPlacemark
        {
            public int id;
            public float x;
            public float y;
            public string style;
            public string description;

            public YMapsPlacemark(int id, float x, float y, string style)
            {
                this.id = id;
                this.x = x;
                this.y = y;
                this.style = style;
            }
        }


        // http://russianhighways.ru//ajax/getpoints.php?sid=37
        // http://russianhighways.ru//ajax/getpoints.php?sid=24
        public static string ParsePageAndSave()
        {
            return ParsePageAndSave("http://russianhighways.ru//ajax/getpoints.php?sid=24");
        }

        // http://russianhighways.ru//ajax/getpoints.php?sid=37
        // http://russianhighways.ru//ajax/getpoints.php?sid=24
        public static string ParsePageAndSave(string url)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            //string filename = @"C:\Downloads\CD-REC\getpoints.php.htm";
            //FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            //StreamReader sr = new StreamReader(fs);
            //string inf = sr.ReadToEnd().Replace("\r\n", " ");
            //sr.Close();
            //fs.Close();

            string inf = "";
            
            try
            {
            inf = GrabURL(url).Replace("\r\n", " ");
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            };

            Hashtable iconStyles = new Hashtable();
            List<YMapsPlacemark> pms = new List<YMapsPlacemark>();

            // http://www.cyberforum.ru/csharp-beginners/thread244709.html

            Regex reg = new Regex(@"(p\d+)\.iconStyle\.href\s=\s""([^""]*)"";", RegexOptions.None);
            MatchCollection mc = reg.Matches(inf);
            foreach (Match mat in mc)
            {
                string gt1 = mat.Groups[1].Value.Trim();
                string gt2 = mat.Groups[2].Value.Trim();
                if (!gt2.ToLower().StartsWith("http://"))
                    gt2 = "http://russianhighways.ru/" + gt2;
                iconStyles.Add(gt1, gt2);
            };

            reg = new Regex(@"\sPlaceMarks\[(\d+)\]\s=\snew\sYMaps\.Placemark\(new\sYMaps\.GeoPoint\(([\d\.]+),\s([\d\.]+)\),\s{[^}]+style:\s([^}]+)}", RegexOptions.None);
            mc = reg.Matches(inf);
            foreach (Match mat in mc)
            {
                string gt1 = mat.Groups[1].Value.Trim();
                string gt2 = mat.Groups[2].Value.Trim();
                string gt3 = mat.Groups[3].Value.Trim();
                string gt4 = mat.Groups[4].Value.Trim();
                string mt = mat.ToString();
                pms.Add(new YMapsPlacemark(int.Parse(gt1), float.Parse(gt2, ni), float.Parse(gt3, ni), gt4));
            };
            reg = new Regex(@"PlaceMarks\[(\d+)\].description\s=\s\""([^}]+)\"";\s+}", RegexOptions.None);
            mc = reg.Matches(inf);
            foreach (Match mat in mc)
            {
                string gt1 = mat.Groups[1].Value.Trim();
                string gt2 = mat.Groups[2].Value.Trim();
                Regex repl = new Regex("<[^>]*>");
                gt2 = repl.Replace(gt2, " ").Trim();
                while (gt2.IndexOf("  ") >= 0) gt2 = gt2.Replace("  ", " ");
                string mt = mat.ToString();
                foreach (YMapsPlacemark pm in pms)
                    if (pm.id.ToString() == gt1)
                        pm.description = gt2.Trim();
            };

            string txt = String.Format("Parsed url: \r\n{2}\r\n\r\nParsed {1} placemarks with {0} styles\r\nDo you want to save result to file?", iconStyles.Count, pms.Count, url);
            DialogResult dr = MessageBox.Show(txt, "Parsing URL", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dr == DialogResult.No) return "";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select output KML file";
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml";
            string fn = "";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                fn = sfd.FileName;
                Save2KML(iconStyles, pms.ToArray(), sfd.FileName, url);
            };
            sfd.Dispose();
            return fn;
        }

        private static string GrabURL(string url)
        {
            string res = "";
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            System.Net.WebResponse wres = wr.GetResponse();
            System.IO.Stream rs = wres.GetResponseStream();
            byte[] ba = new byte[4096];
            int read = -1;
            while ((read = rs.Read(ba, 0, ba.Length)) > 0)
                res += System.Text.Encoding.UTF8.GetString(ba, 0, read);
            wres.Close();
            return res;
            
        }

        private static void Save2KML(Hashtable styles, YMapsPlacemark[] pms, string filename, string name)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>" + name + "</name>");
            foreach (YMapsPlacemark pm in pms)
            {
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name>" + System.Security.SecurityElement.Escape(pm.description) + "</name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + pm.description + "]]></description>");
                sw.WriteLine("\t\t\t\t<styleUrl>" + (pm.style == "" ? "#nocion" : "#" + pm.style) + "</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + String.Format(ni,"{0},{1},0",pm.x,pm.y) + "</coordinates></Point>");                
                sw.WriteLine("\t\t\t</Placemark>");
            };
            sw.WriteLine("\t\t</Folder>");
            sw.WriteLine("\t<Style id=\"noicon\"><IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle></Style>");
            foreach (string key in styles.Keys)
                sw.WriteLine("\t<Style id=\""+key+"\"><IconStyle><Icon><href>"+styles[key].ToString()+"</href></Icon></IconStyle></Style>");
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
        }
    }

    public class RussianHighwaysESBImporter
    {
        //данные получены путем декомпил€ции apk јвтодор дл€ Android
        //см папку APK_Decompile
        // ru.avtodortr.mobileclient -> webservices -> poi -> AVTPoiWebServices.class
        // \ru\avtodortr\mobileclient\webservices\poi\AVTPoiWebServices.class
        //
        // https://mp.russianhighways.ru:10443//esb/1.0/cxf/poi/types?lang=EN
        // https://mp.russianhighways.ru:10443/esb/1.0/cxf/poi/objects?north=90&south=0&west=0&east=180
        // http://square.github.io/retrofit/
        //

        public class POIType
        {
            public int id;
            public string name;
        }

        public class POI
        {
            public int id;
            public object extId;
            public double latitude;
            public double longitude;
            public string name;
            public POIType type;
            public string image;
            public string language;
            public string longDescription;
        }

        public static List<POIType> GetPOITypes(string url)
        {
            string json = GrabURL(url);
            return JsonConvert.DeserializeObject<List<POIType>>(json);            
        }

        public static List<POI> GetPOIs(string url)
        {
            string json = GrabURL(url);
            return JsonConvert.DeserializeObject<List<POI>>(json);
        }

        public static string ParsePageAndSave()
        {
            return ParsePageAndSave("https://mp.russianhighways.ru:10443//esb/1.0/cxf/poi/types?lang=EN","https://mp.russianhighways.ru:10443/esb/1.0/cxf/poi/objects?north=90&south=0&west=0&east=180");
        }

        // http://russianhighways.ru//ajax/getpoints.php?sid=37
        // http://russianhighways.ru//ajax/getpoints.php?sid=24
        public static string ParsePageAndSave(string types, string pois)
        {
            //string filename = @"C:\Downloads\CD-REC\response.txt";
            //FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            //StreamReader sr = new StreamReader(fs);
            //string inf = sr.ReadToEnd().Replace("\r\n", " ");
            //sr.Close();
            //fs.Close();

            List<POIType> tlist = new List<POIType>();
            List<POI> plist = new List<POI>();
            try
            {
                tlist = GetPOITypes(types);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            };
            try
            {
               plist = GetPOIs(pois);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            };

            string txt = String.Format("Parsed {0} types from {1}\r\nParsed {2} pois from {3}\r\nDo you want to save result to file?", tlist.Count, types, plist.Count, pois);
            DialogResult dr = MessageBox.Show(txt, "Parsing URL", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dr == DialogResult.No) return "";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select output KML file";
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml";
            string fn = "";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                fn = sfd.FileName;
                Save2KML(tlist, plist, sfd.FileName, "Grab URLs for Avtodor.apk");
            };
            sfd.Dispose();
            return fn;
        }

        private static string GrabURL(string url)
        {
            string res = "";
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
            wr.UserAgent = "Avtodor Mobile Client/1.2.2 (Android 5.1)";
            System.Net.WebResponse wres = wr.GetResponse();
            System.IO.Stream rs = wres.GetResponseStream();
            byte[] ba = new byte[4096];
            int read = -1;
            while ((read = rs.Read(ba, 0, ba.Length)) > 0)
                res += System.Text.Encoding.UTF8.GetString(ba, 0, read);
            wres.Close();
            return res;

        }

        private static void Save2KML(List<POIType> types, List<POI> pois, string filename, string name)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>" + name + "</name>");
            foreach (POIType tp in types)
            {
                sw.WriteLine("\t\t<Folder>");
                sw.WriteLine("\t\t<id>" + tp.id.ToString() + "</id>");
                sw.WriteLine("\t\t<name>" + System.Security.SecurityElement.Escape(tp.name) + "</name>");
                foreach (POI p in pois)
                    if (p.type.id == tp.id)
                    {
                        sw.WriteLine("\t\t\t<Placemark>");
                        sw.WriteLine("\t\t\t<id>" + p.id.ToString() + "</id>");
                        sw.WriteLine("\t\t\t\t<name>" + System.Security.SecurityElement.Escape(p.name) + "</name>");
                        sw.WriteLine("\t\t\t\t<description><![CDATA[" + (p.longDescription == null ? "" : p.longDescription) + "]]></description>");
                        sw.WriteLine("\t\t\t\t<styleUrl>#nocion</styleUrl>");
                        sw.WriteLine("\t\t\t\t<Point><coordinates>" + String.Format(ni, "{0},{1},0", p.longitude, p.latitude) + "</coordinates></Point>");                        
                        sw.WriteLine("\t\t\t</Placemark>");
                    };
                sw.WriteLine("\t\t</Folder>");
            };
            sw.WriteLine("\t<Style id=\"noicon\"><IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle></Style>");
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
        }
    }
}
