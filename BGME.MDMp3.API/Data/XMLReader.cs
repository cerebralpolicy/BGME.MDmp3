using P3R.CalendarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BGME.MDmp3.Data
{
    internal class XMLReader
    {
        public static string DateFormat = "dd/MM/yyyy";
        public static P3Date ParseRelease(string releaseField)
        {
            if (string.IsNullOrEmpty(releaseField))
            {
                return new P3Date(2009, 03, 31);
            }
            else
            {
                return P3Date.Parse(releaseField);
            }
        }
        public XDocument ReadXMLCatchErrors(string xmlPath)
        {
            try
            {
                return XDocument.Load(xmlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong:");
                Console.WriteLine(ex);
            }
            return new XDocument();
        }
    }
}
