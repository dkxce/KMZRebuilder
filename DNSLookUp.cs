//                      //
// Author Milok Zbrozek //
//   milokz@gmail.com   //
//                      //

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DNS
{
    public class DNSLookUp
    {
        public class ResponseRecord
        {
            public QueryTypes respType;
            public string respName;
            public string respValue;

            public ResponseRecord() { }
            public ResponseRecord(QueryTypes rType, string rName, string rValue) { respType = rType; respName = rName; respValue = rValue; }

            public static bool Exists(List<ResponseRecord> rr, ResponseRecord r)
            {
                if (rr == null) return false;
                if (rr.Count == 0) return false;
                foreach (ResponseRecord _r in rr)
                    if ((_r.respName == r.respName) && (_r.respType == r.respType) && (_r.respValue == r.respValue))
                        return true;
                return false;
            }
        }

        //
        // http://msdn.microsoft.com/en-us/library/ms682016
        //
        // pstrName -  the name of the owner of the record set that is queried.
        // wType - DNS Record Type that is queried. wType determines the format of data pointed to by ppQueryResultsSet. For example, if the value of wType is DNS_TYPE_A, the format of data pointed to by ppQueryResultsSet is DNS_A_DATA.
        // Options - A value that contains a bitmap of DNS Query Options. Options can be combined and all options override DNS_QUERY_STANDARD.
        // aipServers [in, out, optional] - null (0)
        // ppQueryResultsSet [out, optional] - Optional. A pointer to a pointer that points to the list of RRs that comprise the response. For more information, see the Remarks section.
        // pReserved - null (0)
        //
        
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);

        //
        // http://msdn.microsoft.com/en-us/library/cc982162
        public enum QueryTypes : short
        {
            DNS_TYPE_A = 0x0001,
            DNS_TYPE_NS = 0x0002,
            DNS_TYPE_MD = 0x0003,
            DNS_TYPE_MF = 0x0004,
            DNS_TYPE_CNAME = 0x0005,
            DNS_TYPE_SOA = 0x0006,
            DNS_TYPE_MB = 0x0007,
            DNS_TYPE_MG = 0x0008,
            DNS_TYPE_MR = 0x0009,
            DNS_TYPE_NULL = 0x000a,
            DNS_TYPE_WKS = 0x000b,
            DNS_TYPE_PTR = 0x000c,
            DNS_TYPE_HINFO = 0x000d,
            DNS_TYPE_MINFO = 0x000e,
            DNS_TYPE_MX = 0x000f,
            DNS_TYPE_TEXT = 0x0010,
            DNS_TYPE_RP = 0x0011,
            DNS_TYPE_AFSDB = 0x0012,
            DNS_TYPE_X25 = 0x0013,
            DNS_TYPE_ISDN = 0x0014,
            DNS_TYPE_RT = 0x0015,
            DNS_TYPE_NSAP = 0x0016,
            DNS_TYPE_NSAPPTR = 0x0017,
            DNS_TYPE_SIG = 0x0018,
            DNS_TYPE_KEY = 0x0019,
            DNS_TYPE_PX = 0x001a,
            DNS_TYPE_GPOS = 0x001b,
            DNS_TYPE_AAAA = 0x001c,
            DNS_TYPE_LOC = 0x001d,
            DNS_TYPE_NXT = 0x001e,
            DNS_TYPE_EID = 0x001f,
            DNS_TYPE_NIMLOC = 0x0020,
            DNS_TYPE_SRV = 0x0021,
            DNS_TYPE_ATMA = 0x0022,
            DNS_TYPE_NAPTR = 0x0023,
            DNS_TYPE_KX = 0x0024,
            DNS_TYPE_CERT = 0x0025,
            DNS_TYPE_A6 = 0x0026,
            DNS_TYPE_DNAME = 0x0027,
            DNS_TYPE_SINK = 0x0028,
            DNS_TYPE_OPT = 0x0029,
            DNS_TYPE_DS = 0x002B,
            DNS_TYPE_RRSIG = 0x002E,
            DNS_TYPE_NSEC = 0x002F,
            DNS_TYPE_DNSKEY = 0x0030,
            DNS_TYPE_DHCID = 0x0031,
            DNS_TYPE_UINFO = 0x0064,
            DNS_TYPE_UID = 0x0065,
            DNS_TYPE_GID = 0x0066,
            DNS_TYPE_UNSPEC = 0x0067,
            DNS_TYPE_ADDRS = 0x00f8,
            DNS_TYPE_TKEY = 0x00f9,
            DNS_TYPE_TSIG = 0x00fa,
            DNS_TYPE_IXFR = 0x00fb,
            DNS_TYPE_AXFR = 0x00fc,
            DNS_TYPE_MAILB = 0x00fd,
            DNS_TYPE_MAILA = 0x00fe,
            DNS_TYPE_ALL = 0x00ff,
            DNS_TYPE_ANY = 0x00ff
        }

        // http://msdn.microsoft.com/en-us/library/cc982162
        public enum QueryOptions
        {
            DNS_QUERY_STANDARD = 0x00000000, //Standard query.
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 0x00000001, //Returns truncated results. Does not retry under TCP.
            DNS_QUERY_USE_TCP_ONLY = 0x00000002, //Uses TCP only for the query.
            DNS_QUERY_NO_RECURSION = 0x00000004, //Directs the DNS server to perform an iterative query (specifically directs the DNS server not to perform recursive resolution to resolve the query).
            DNS_QUERY_BYPASS_CACHE = 0x00000008, //Bypasses the resolver cache on the lookup.
            DNS_QUERY_NO_WIRE_QUERY = 0x00000010, //Directs DNS to perform a query on the local cache only.  Windows 2000 Server and Windows 2000 Professional:  This value is not supported. For similar functionality, use DNS_QUERY_CACHE_ONLY.
            DNS_QUERY_NO_LOCAL_NAME = 0x00000020, //Directs DNS to ignore the local name.    Windows 2000 Server and Windows 2000 Professional:  This value is not supported.
            DNS_QUERY_NO_HOSTS_FILE = 0x00000040, //Prevents the DNS query from consulting the HOSTS file.    Windows 2000 Server and Windows 2000 Professional:  This value is not supported.
            DNS_QUERY_NO_NETBT = 0x00000080, //Prevents the DNS query from using NetBT for resolution.    Windows 2000 Server and Windows 2000 Professional:  This value is not supported.
            DNS_QUERY_WIRE_ONLY = 0x00000100, //Directs DNS to perform a query using the network only, bypassing local information.    Windows 2000 Server and Windows 2000 Professional:  This value is not supported.
            DNS_QUERY_RETURN_MESSAGE = 0x00000200, //Directs DNS to return the entire DNS response message.    Windows 2000 Server and Windows 2000 Professional:  This value is not supported.
            DNS_QUERY_MULTICAST_ONLY = 0x00000400, //Prevents the query from using DNS and uses only Local Link Multicast Name Resolution (LLMNR). Windows Vista and Windows Server 2008 or later.:  This value is supported.
            DNS_QUERY_NO_MULTICAST = 0x00000800, //
            DNS_QUERY_TREAT_AS_FQDN = 0x00001000, //Prevents the DNS response from attaching suffixes to the submitted name in a name resolution process.
            DNS_QUERY_ADDRCONFIG = 0x00002000, //Windows 7 only: Do not send A type queries if IPv4 addresses are not available on an interface and do not send AAAA type queries if IPv6 addresses are not available.
            DNS_QUERY_DUAL_ADDR = 0x00004000, //Windows 7 only: Query both AAAA and A type records and return results for each. Results for A type records are mapped into AAAA type.
            DNS_QUERY_MULTICAST_WAIT = 0x00020000, //Waits for a full timeout to collect all the responses from the Local Link. If not set, the default behavior is to return with the first response.    Windows Vista and Windows Server 2008 or later.:  This value is supported.
            DNS_QUERY_MULTICAST_VERIFY = 0x00040000, //Directs a test using the local machine hostname to verify name uniqueness on the same Local Link. Collects all responses even if normal LLMNR Sender behavior is not enabled.    Windows Vista and Windows Server 2008 or later.:  This value is supported.
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x00100000, //If set, and if the response contains multiple records, records are stored with the TTL corresponding to the minimum value TTL from among all records. When this option is set, "Do not change the TTL of individual records" in the returned record set is not modified.
            DNS_QUERY_APPEND_MULTILABEL = 0x00800000 //
        }

        /// <summary>
        ///     ¬ызываетс€ дл€ каждой записи DNS
        /// </summary>
        /// <param name="DNSQueryResult">”казатель на структуру записи</param>
        public delegate void OnDNSResponse(IntPtr DNSQueryResult, string domain);

        /// <summary>
        ///    
        /// </summary>
        /// <param name="DNSQueryResultCode"></param>
        public delegate void OnDNSResultCode(int DNSQueryResultCode);

        /// <summary>
        ///     ¬ызываетс€ дл€ каждой записи DNS
        /// </summary>
        /// <param name="pName">им€ записи</param>
        /// <param name="qt">тип записи</param>
        /// <param name="value">значение записи</param>
        public delegate void OnDNSResponseText(string domain, QueryTypes qt, string value);

        /// <summary>
        ///     ѕолучаем все записи
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="queryType">домен</param>
        /// <returns></returns>
        public static List<ResponseRecord> GetRecords(string domain, out System.ComponentModel.Win32Exception errCode)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            errCode = null;
            List<ResponseRecord> res = new List<ResponseRecord>();

            OnGetRecordsInText = (delegate(string pName, QueryTypes qt, string value) 
            {
                ResponseRecord _r = new ResponseRecord(qt, pName, value);
                if(!ResponseRecord.Exists(res, _r))
                    res.Add(_r); 
            });

            QueryTypes[] qtps = new QueryTypes[] { QueryTypes.DNS_TYPE_A, QueryTypes.DNS_TYPE_NS, QueryTypes.DNS_TYPE_CNAME, QueryTypes.DNS_TYPE_DNAME, QueryTypes.DNS_TYPE_MX, QueryTypes.DNS_TYPE_SOA, QueryTypes.DNS_TYPE_SRV, QueryTypes.DNS_TYPE_TEXT };
            foreach (QueryTypes qType in qtps)
            {
                IntPtr ResultPtr = IntPtr.Zero;
                IntPtr RecordPtr = IntPtr.Zero;

                int result = DnsQuery(ref domain, qType, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
                if (result == 9501) continue;
                if (result != 0) { errCode = new System.ComponentModel.Win32Exception(result); continue; };

                RR.DNS_HEADER recMx;
                for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
                {
                    OnData(RecordPtr, domain);
                    recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));   
                };
                DnsRecordListFree(ResultPtr, 0);
            };            
            return res;
        }

        /// <summary>
        ///     ѕолучаем все записи согласно типу запроса
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="queryType">домен</param>
        /// <param name="errCode">тип запроса</param>
        /// <returns></returns>
        public static List<ResponseRecord> GetRecords(string domain, QueryTypes queryType, out System.ComponentModel.Win32Exception errCode)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            if (queryType == QueryTypes.DNS_TYPE_ALL) return GetRecords(domain, out errCode);
            if (queryType == QueryTypes.DNS_TYPE_ANY) return GetRecords(domain, out errCode);            

            errCode = null;
            List<ResponseRecord> res = new List<ResponseRecord>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, queryType, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result != 0) { errCode = new System.ComponentModel.Win32Exception(result); return res; };

            OnGetRecordsInText = (delegate(string pName, QueryTypes qt, string value){
                res.Add(new ResponseRecord(qt, pName, value)); });
            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                OnData(RecordPtr, domain);
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
            }
            DnsRecordListFree(ResultPtr, 0);
            return res;
        }

        /// <summary>
        ///     ѕолучаем все записи согласно типу запроса
        /// </summary>
        /// <param name="domain">домен</param>
        /// <param name="queryType">тип запроса</param>
        /// <param name="onResponse">идентификатор на процедуру дл€ каждой записи</param>
        /// <returns>число записей</returns>
        public static int GetRecords(string domain, QueryTypes queryType, OnDNSResponse onResponse, OnDNSResultCode onResultCode)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            int count = 0;
            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, queryType, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (onResultCode != null) onResultCode(result);
            if (result != 0)
            {
                if (result == 9501) return 0;
                if (onResultCode != null)
                    onResultCode(result);
                else
                    throw new System.ComponentModel.Win32Exception(result);
            };
            
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms681391(v=vs.85).aspx
            // 1460 Timeout Expired
            // 9001 (0x2329) DNS server unable to interpret format.
            // 9002 (0x232A) DNS server failure.
            // 9003 (0x232B) DNS name does not exist.
            // 9004 (0x232C) DNS request not supported by name server.

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                if (onResponse != null) onResponse(RecordPtr, domain);
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                count++;
            }
            DnsRecordListFree(ResultPtr, 0);
            return count;
        }

        /// <summary>
        ///     ѕолучаем все записи согласно типу запроса
        /// </summary>
        /// <param name="domain">домен</param>
        /// <param name="queryType">тип запроса</param>
        /// <param name="onResponse">идентификатор на процедуру дл€ каждой записи</param>
        /// <returns>число записей</returns>
        public static int GetRecords(string domain, QueryTypes queryType, OnDNSResponseText onResponse, OnDNSResultCode onResultCode)
        {
            OnGetRecordsInText = onResponse;
            int count = GetRecords(domain, queryType, OnData, onResultCode);
            OnGetRecordsInText = null;
            return count;
        }

        private static OnDNSResponseText OnGetRecordsInText;
        private static void OnData(IntPtr DNSQueryResult, string domain)
        {
            DNSLookUp.RR.DNS_HEADER recMx = (DNSLookUp.RR.DNS_HEADER)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_HEADER));
            switch (recMx.wType)
            {
                case DNSLookUp.QueryTypes.DNS_TYPE_A: // GetHostAddress
                    DNSLookUp.RR.DNS_A_DATA a = (DNSLookUp.RR.DNS_A_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_A_DATA));
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, a.IP[0].ToString() + "." + a.IP[1].ToString() + "." + a.IP[2].ToString() + "." + a.IP[3].ToString());
                    break;

                case DNSLookUp.QueryTypes.DNS_TYPE_SOA:
                    DNSLookUp.RR.DNS_SOA_DATA soa = (DNSLookUp.RR.DNS_SOA_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_SOA_DATA));
                    if (OnGetRecordsInText != null)
                    {
                        OnGetRecordsInText(domain, recMx.wType, "Admin " + soa.pNameAdministrator);
                        OnGetRecordsInText(domain, recMx.wType, "Server " + soa.pNamePrimaryServer);
                    };
                    break;

                case DNSLookUp.QueryTypes.DNS_TYPE_PTR:
                case DNSLookUp.QueryTypes.DNS_TYPE_NS:    // GetDNSServer
                case DNSLookUp.QueryTypes.DNS_TYPE_CNAME: // GetDNSCName
                case DNSLookUp.QueryTypes.DNS_TYPE_DNAME:
                case DNSLookUp.QueryTypes.DNS_TYPE_MB:
                case DNSLookUp.QueryTypes.DNS_TYPE_MD:
                case DNSLookUp.QueryTypes.DNS_TYPE_MF:
                case DNSLookUp.QueryTypes.DNS_TYPE_MG:
                case DNSLookUp.QueryTypes.DNS_TYPE_MR:
                    DNSLookUp.RR.DNS_PTR_DATA ptr = (DNSLookUp.RR.DNS_PTR_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_PTR_DATA));
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, ptr.pNameHost);
                    break;

                case DNSLookUp.QueryTypes.DNS_TYPE_MINFO:
                case DNSLookUp.QueryTypes.DNS_TYPE_RP:
                    DNSLookUp.RR.DNS_MINFO_DATA minfo = (DNSLookUp.RR.DNS_MINFO_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_MINFO_DATA));
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, minfo.pNameMailbox + " ERRORS " + minfo.pNameErrorsMailbox);
                    break;

                case DNSLookUp.QueryTypes.DNS_TYPE_MX:
                    DNSLookUp.RR.DNS_MX_DATA mx = (DNSLookUp.RR.DNS_MX_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_MX_DATA));                    
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, mx.pNameExchange);
                    break;

                case DNSLookUp.QueryTypes.DNS_TYPE_HINFO:
                case DNSLookUp.QueryTypes.DNS_TYPE_ISDN:
                case DNSLookUp.QueryTypes.DNS_TYPE_X25:
                case DNSLookUp.QueryTypes.DNS_TYPE_TEXT: // GetDNSText
                    DNSLookUp.RR.DNS_TXT_DATA txt = (DNSLookUp.RR.DNS_TXT_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_TXT_DATA));
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, txt.text);
                    break;
                case  DNSLookUp.QueryTypes.DNS_TYPE_SRV:
                    DNSLookUp.RR.DNS_SRV_DATA srv = (DNSLookUp.RR.DNS_SRV_DATA)Marshal.PtrToStructure(DNSQueryResult, typeof(DNSLookUp.RR.DNS_SRV_DATA));
                    if (OnGetRecordsInText != null)
                        OnGetRecordsInText(domain, recMx.wType, srv.pNameTarget + ":" + srv.wPort.ToString());
                    break;
            };
        }

        /// <summary>
        ///     (IN A) ѕолучаем IP адрес домена
        /// </summary>
        /// <param name="domain">домен</param>
        /// <returns>список адресов</returns>
        public static System.Net.IPAddress[] Get_A(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<System.Net.IPAddress> list = new List<System.Net.IPAddress>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_A, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_A)
                    list.Add(new System.Net.IPAddress(((DNSLookUp.RR.DNS_A_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_A_DATA))).IP));
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     (IN SOA)
        /// </summary>
        /// <param name="domain">домен</param>
        /// <returns>сервера</returns>
        public static string[] Get_SOA(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_SOA, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_SOA)
                {
                    DNSLookUp.RR.DNS_SOA_DATA soa = ((DNSLookUp.RR.DNS_SOA_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_SOA_DATA)));
                    list.Add("Admin " + soa.pNameAdministrator);
                    list.Add("Server " + soa.pNamePrimaryServer);
                };
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     (IN NS) ѕолучаем список DNS серверов дл€ домена
        /// </summary>
        /// <param name="domain">домен</param>
        /// <returns>сервера</returns>
        public static string[] Get_NS(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_NS, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_NS)
                    list.Add(((DNSLookUp.RR.DNS_PTR_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_PTR_DATA))).pNameHost);
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     (IN CNAME) ѕолучаем список CName серверов дл€ домена
        /// </summary>
        /// <param name="domain">домен</param>
        /// <returns>сервера</returns>
        public static string[] Get_CNAME(string domain)
        {
            //if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_CNAME, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_CNAME)
                    list.Add(((DNSLookUp.RR.DNS_PTR_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_PTR_DATA))).pNameHost);
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     ѕолучаем список текстовых записей домена (IN TXT)
        /// </summary>
        /// <param name="domain">домен</param>
        /// <returns>записи</returns>
        public static string[] Get_TXT(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_TEXT, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_TEXT)
                    list.Add(((DNSLookUp.RR.DNS_TXT_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_TXT_DATA))).text);
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     (IN SRV)
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string[] Get_SRV(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_SRV, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_SRV)
                {
                    DNSLookUp.RR.DNS_SRV_DATA srv = ((DNSLookUp.RR.DNS_SRV_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_SRV_DATA)));
                    list.Add(srv.pNameTarget+":"+srv.wPort.ToString());
                };
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        /// <summary>
        ///     (IN MX)
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string[] Get_MX(string domain)
        {
            // if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new NotSupportedException();

            List<string> list = new List<string>();

            IntPtr ResultPtr = IntPtr.Zero;
            IntPtr RecordPtr = IntPtr.Zero;

            int result = DnsQuery(ref domain, QueryTypes.DNS_TYPE_MX, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ResultPtr, 0);
            if (result == 9501) return list.ToArray();
            if (result != 0) throw new System.ComponentModel.Win32Exception(result);

            RR.DNS_HEADER recMx;
            for (RecordPtr = ResultPtr; !RecordPtr.Equals(IntPtr.Zero); RecordPtr = recMx.pNext)
            {
                recMx = (RR.DNS_HEADER)Marshal.PtrToStructure(RecordPtr, typeof(RR.DNS_HEADER));
                if (recMx.wType == DNSLookUp.QueryTypes.DNS_TYPE_MX)
                {
                    DNSLookUp.RR.DNS_MX_DATA mx = ((DNSLookUp.RR.DNS_MX_DATA)Marshal.PtrToStructure(RecordPtr, typeof(DNSLookUp.RR.DNS_MX_DATA)));
                    list.Add(mx.pNameExchange);
                };
            };
            DnsRecordListFree(ResultPtr, 0);
            return list.ToArray();
        }

        // http://msdn.microsoft.com/en-us/library/ms682082
        public struct RR
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_HEADER
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_A_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] IP;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_SOA_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //           
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNamePrimaryServer;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameAdministrator;
                public long dwSerialNo;
                public long dwRefresh;
                public long dwRetry;
                public long dwExpire;
                public long dwDefaultTtl;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_PTR_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //           
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameHost;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_MINFO_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //           
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameMailbox;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameErrorsMailbox;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_MX_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameExchange;
                public short wPreference;
                public short Pad;

            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_TXT_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //
                public uint dwStringCount;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string text;

            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DNS_SRV_DATA
            {
                public IntPtr pNext;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pName;
                public QueryTypes wType;
                public short wDataLength;
                public int flags;
                public int dwTtl;
                public int dwReserved;
                //
                [MarshalAs(UnmanagedType.LPWStr)]
                public string pNameTarget;
                public ushort wPriority;
                public ushort wWeight;
                public ushort wPort;
                public ushort Pad;
            }
        }
    }
}
