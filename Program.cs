using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace XML_parser_test
{
    class Program
    {
        static bool hasInternetAccess = true;

        enum AlertType
        {
            NoInternet = 1
        }


        static void Main()
        {
            Console.WriteLine("Haetaan dataa...\n");

            List<string> alerts = GetAlerts();
            string temperature = GetTemperature();

            for (int i = 0; i < alerts.Count; i++)
            {
                Console.WriteLine(alerts[i]);
            }
            if(alerts.Count == 1)
            {
                Console.WriteLine("Ei varoituksia.");
            }

            Console.WriteLine($"\n\n{temperature}");


            Console.ReadKey();
        }


        static List<string> GetAlerts()
        {
            List<string> result = new List<string>();
            XmlDocument alerts = new XmlDocument();

            try
            {
                alerts.Load("https://alerts.fmi.fi/cap/feed/rss_fi-FI.rss");
            }
            catch (System.Net.WebException)
            {
                hasInternetAccess = false;
                Alert(AlertType.NoInternet);
                return null;
            }

            XmlNodeList results = alerts.GetElementsByTagName("item");
            XmlNodeList description = alerts.GetElementsByTagName("description");

            if (hasInternetAccess)
            {
                result.Add(description[0].InnerText + ":");
            }

            for (int i = 0; i < results.Count; i++)
            {
                XmlNodeList contents = results[i].ChildNodes;

                for (int j = 0; j < contents.Count; j++)
                {
                    if (contents[j].Name.Contains("title") && (contents[j].InnerText.Contains("koko maa") || contents[j].InnerText.Contains("Vaasa") || contents[j].InnerText.Contains("Pohjanmaa")))
                    {
                        result.Add($"\n{contents[j].InnerText}");
                        string info = contents[j + 2].InnerText.Substring(contents[j + 2].InnerText.IndexOf(' ') + 1);

                        result.Add($"Lisätietoa: {info}\n");
                    }
                }
            }

            return result;
        }


        static string GetTemperature()
        {
            string result;

            XmlDocument alerts = new XmlDocument();

            string currTime = DateTime.Now.ToString("HH':'mm':'ss");
            string currDate = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                alerts.Load($"http://opendata.fmi.fi/wfs?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi::observations::weather::multipointcoverage&place=vaasa&starttime={DateTime.Now.ToString("yyyy-MM-dd")}T00:00:00Z&endtime={currDate}T{currTime}Z&Parameters=temperature");
            }
            catch (System.Net.WebException)
            {
                hasInternetAccess = false;
                return null;
            }

            XmlNodeList results = alerts.GetElementsByTagName("gml:doubleOrNilReasonTupleList");

            string[] temps = results[0].InnerText.Split('\n');
            result = Regex.Replace(temps[temps.Length - 2], @"\s+", "");
            result = $"Lämpötila {currTime}: {result}\n";

            return result;
        }


        static void Alert(AlertType type)
        {
            switch (type)
            {
                case AlertType.NoInternet:
                    Console.WriteLine("Palvelimeen ei saada yhteyttä.\nTarkista verkkoasetuksesi.");
                    //Ilmoitetaan käyttäjälle virheestä
                    break;
                default:
                    break;
            }
        }
    }
}
