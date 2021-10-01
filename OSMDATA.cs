using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace KMZRebuilder
{
    public class OSMDictionary
    {
        public string language = "";
        public Dictionary<string, DictCatalog> catalog = new Dictionary<string, DictCatalog>();
        public Dictionary<string, DictCatalog> moretags = new Dictionary<string, DictCatalog>();
        public Dictionary<string, Dictionary<string, string>> clas = new Dictionary<string, Dictionary<string, string>>();

        public static OSMDictionary ReadFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string jsontext = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            OSMDictionary result = new OSMDictionary();

            Newtonsoft.Json.Linq.JToken osmd = (Newtonsoft.Json.Linq.JContainer)Newtonsoft.Json.JsonConvert.DeserializeObject(jsontext);
            foreach (Newtonsoft.Json.Linq.JProperty suntoken in osmd)
            {
                if (suntoken.Name == "language")
                    result.language = suntoken.Value.ToString();
                if (suntoken.Name == "catalog")
                {
                    foreach(Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        DictCatalog c = new DictCatalog();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                        {
                            if (catv.Name == "name") c.name = catv.Value.ToString();
                            if (catv.Name == "description") c.description = catv.Value.ToString();
                            if (catv.Name == "link") c.link = catv.Value.ToString();
                            if (catv.Name == "keywords") c.keywords = catv.Value.ToString();
                        };
                        result.catalog.Add(cat.Name, c);
                    };
                };
                if (suntoken.Name == "moretags")
                {
                    foreach (Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        DictCatalog c = new DictCatalog();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                        {
                            if (catv.Name == "name") c.name = catv.Value.ToString();
                            if (catv.Name == "description") c.description = catv.Value.ToString();
                            if (catv.Name == "link") c.link = catv.Value.ToString();
                            if (catv.Name == "keywords") c.keywords = catv.Value.ToString();
                        };
                        result.moretags.Add(cat.Name, c);
                    };
                };
                if (suntoken.Name == "class")
                {
                    foreach (Newtonsoft.Json.Linq.JProperty cat in suntoken.Value)
                    {
                        Dictionary<string, string> kv = new Dictionary<string, string>();
                        foreach (Newtonsoft.Json.Linq.JProperty catv in cat.Value)
                            kv.Add(catv.Name, catv.Value.ToString());
                        result.clas.Add(cat.Name, kv);
                    };
                };     
            };

            return result;
        }

        public class DictCatalog
        {
            public string name;
            public string description;
            public string link;
            public string keywords;
        }

        public string Translate(string text)
        {
            string result = text;
            foreach(KeyValuePair<string,DictCatalog> kvp in catalog)
                if (kvp.Key == text) return kvp.Value.name;
            foreach (KeyValuePair<string, DictCatalog> kvp in moretags)
                if (kvp.Key == text) return kvp.Value.name;
            return result;
        }

        public string Translate(string key, string value)
        {
            string result = value;
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in clas)
                if (kvp.Key == key)
                    foreach (KeyValuePair<string, string> vvp in kvp.Value)
                        if (vvp.Key == value)
                            return vvp.Value;
            return result;
                    
        }
    }    

    public class OSMCatalog
    {
        public List<OSMCatalogRecord> records = new List<OSMCatalogRecord>();

        public int Count
        {
            get
            {
                return records.Count;
            }
        }

        public string GetTopParentCategory(string category)
        {
            string result = category;
            if (Count == 0) return result;
            bool ex = true;
            while (ex)
            {
                ex = false;
                for (int i = 0; i < Count; i++)
                    if (this[i].name == result)
                    {
                        ex = true;
                        if (this[i].parent == null) return result;
                        if (this[i].parent.Length == 0) return result;
                        result = this[i].parent[0];
                        break;
                    };
            };
            return result;
        }

        public OSMCatalogRecord this[int index]
        {
            get
            {
                return records[index];
            }
        }

        public OSMCatalogRecord ByName(string name)
        {
            if (Count == 0) return null;
            foreach (OSMCatalogRecord rec in records)
                if (rec.name == name)
                    return rec;
            return null;
        }

        public OSMCatalogRecord ByID(int id)
        {
            if (Count == 0) return null;
            foreach (OSMCatalogRecord rec in records)
                if (rec.id == id)
                    return rec;
            return null;
        }

        public class OSMCatalogRecord
        {
            public string name;
            public string[] parent;
            [JsonIgnore]
            public Dictionary<string, string> tags = new Dictionary<string, string>();
            [JsonIgnore]
            public Dictionary<string, string> moretags = new Dictionary<string, string>();
            public bool poi;
            public string[] type;
            public int id;
        }

        public static OSMCatalog ReadFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string jsontext = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            Newtonsoft.Json.Linq.JArray src = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(jsontext);
            List<OSMCatalogRecord> res = new List<OSMCatalogRecord>();
            
            for (int i = 0; i < src.Count; i++)
            {
                Newtonsoft.Json.Linq.JToken t = (Newtonsoft.Json.Linq.JToken)src[i];
                OSMCatalogRecord sc = new OSMCatalogRecord();
                sc.id = (int)t["id"];
                sc.name = t["name"].ToString();
                sc.poi = (bool)t["poi"];

                Newtonsoft.Json.Linq.JArray sub_arr = (Newtonsoft.Json.Linq.JArray)t["parent"];
                if (sub_arr.Count > 0)
                {
                    sc.parent = new string[sub_arr.Count];
                    for (int a = 0; a < sub_arr.Count; a++)
                        sc.parent[a] = sub_arr[a].ToString();
                };

                sub_arr = (Newtonsoft.Json.Linq.JArray)t["type"];
                if (sub_arr.Count > 0)
                {
                    sc.type = new string[sub_arr.Count];
                    for (int a = 0; a < sub_arr.Count; a++)
                        sc.type[a] = sub_arr[a].ToString();
                };

                Newtonsoft.Json.Linq.JToken tags = (Newtonsoft.Json.Linq.JContainer)t["tags"];
                foreach (Newtonsoft.Json.Linq.JProperty suntoken in tags)
                    sc.tags.Add(suntoken.Name, suntoken.Value.ToString());

                Newtonsoft.Json.Linq.JToken moretags = (Newtonsoft.Json.Linq.JContainer)t["moretags"];
                foreach (Newtonsoft.Json.Linq.JProperty suntoken in moretags)
                    sc.moretags.Add(suntoken.Name, suntoken.Value.ToString());

                
                res.Add(sc);
            };
            OSMCatalog catalogue = new OSMCatalog();
            catalogue.records = res;
            return catalogue;
        }
    }
}
