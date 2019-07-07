using System;
using System.Text;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;

namespace CiscoDial
{
    class Program
    {
        static void Main(string[] args)
        {
            //CUCM IP address. Change it to your Cisco Call Manager IP address.

            string ipCCM = "172.168.2.11";
            //Calls a phone. For more info, read the Cisco CUCM AXL manuals.
            string soapreq;
            soapreq = "<soapenv:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:WebdialerSoap\">\n";
            soapreq += "<soapenv:Header/>\n";
            soapreq += "<soapenv:Body>\n";
            soapreq += "<urn:makeCallSoap soapenv:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\n";
            soapreq += "<cred xsi:type=\"urn:Credential\">\n";
            soapreq += "<userID xsi:type=\"xsd:string\">a_user_id\n";
            soapreq += "<password xsi:type=\"xsd:string\">1234\n";
            soapreq += "</cred>\n";
            soapreq += "<dest xsi:type=\"xsd:string\">1108\n";
            soapreq += "<prof xsi:type=\"urn:UserProfile\">\n";
            soapreq += "<user xsi:type=\"xsd:string\">a_user_id\n";
            soapreq += "<deviceName xsi:type=\"xsd:string\">SEP002D80AADEA7\n";
            soapreq += "<lineNumber xsi:type=\"xsd:string\">2103\n";
            soapreq += "<supportEM xsi:type=\"xsd:boolean\">false\n";
            soapreq += "<locale xsi:type=\"xsd:string\">\n";
            soapreq += "</prof>\n";
            soapreq += "</urn:makeCallSoap>\n";
            soapreq += "</soapenv:Body>\n";
            soapreq += "</soapenv:Envelope>\n";


            //Set up SSL 
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

            //Issue the request over SSL
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)WebRequest.Create("https://" + ipCCM + ":8443/webdialer/services/WebdialerSoapService");
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";
            req.Headers.Add("SOAPAction: http://localhost:8373/");
            req.Credentials = CredentialCache.DefaultCredentials;

            StreamWriter sw = new StreamWriter(req.GetRequestStream());
            System.Text.StringBuilder soapRequest = new System.Text.StringBuilder();
            soapRequest.Append(soapreq);
            sw.Write(soapRequest.ToString());
            sw.Close();

            try
            {
                //Get response and display
                using (System.Net.WebResponse webresp = (System.Net.WebResponse)req.GetResponse())

                {

                    // Load data into a dataset
                    DataSet dsBasicInfo = new DataSet();
                    dsBasicInfo.ReadXml(webresp.GetResponseStream());
                    if (dsBasicInfo.Tables["multiRef"] != null)
                    {
                        if (dsBasicInfo.Tables["multiRef"].Rows.Count > 0)
                        {
                            //Retrieve outcome of the soap operation.
                            DataRow itemRow = dsBasicInfo.Tables["multiRef"].Rows[0];

                            //Success
                            //User Authentication Error
                            string outcome = itemRow[1].ToString();
                            Console.WriteLine(outcome);

                            if (outcome == "Success")
                            {
                                //Means user has authenticated properly against the credentials.
                                //Get also use getUserProfile instead of makeCallSoap
                            }
                            else
                            {
                                //Something has not worked.
                            }
                        }
                        else
                        {
                        Console.WriteLine("multiref count = 0, nothing processed.");
                        }
                    }
                    else
                    {
                    Console.WriteLine("Error - Table multiref does not exist!");
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }

            Console.ReadKey();

        }
    }
}
