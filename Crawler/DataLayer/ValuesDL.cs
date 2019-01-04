using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient;
using System.Data;
using System.Text;
using System.IO;
using HtmlAgilityPack;

namespace Crawler.DataLayer
{
    public class ValuesDL
    {
        DataTable table = new DataTable();
        static int rowNumber = 0;
        static List<string> companyList = new List<string>();

        public ValuesDL()
        {
            table.Columns.Add("DIN");
            table.Columns.Add("Director Name");
            table.Columns.Add("Designation");
            table.Columns.Add("Appointment Date");
            table.Columns.Add("Company");
            table.Columns.Add("URL");
            table.Columns.Add("Search Depth");
        }

        public void Crawl(string url, int depth)
        {
            //Fetches the Directors and Add them to a datatable
            AddDirectors(url, depth);

            //Save data from Datatable to CSV, so that we can upload data to Ne04j in bulk
            GenerateCSV(table);

            //Reset the variables
            table.Clear();
            companyList.Clear();
            rowNumber = 0;

            //Upload crawled data to Neo4j
            ValuesDL.Connect();
        }

        public void AddDirectors(string url, int depth)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(url);
            var totalRows = document.DocumentNode.SelectNodes("//*[contains(@data-target,'package')]");

            if (!(totalRows is null) && totalRows.Count > 0)
            {
                var DirectorShip = document.DocumentNode.SelectNodes("//*[text()][(normalize-space(.)= 'Director Details')]").First()
                .NextSibling.ChildNodes["tbody"].SelectNodes(".//tr[not(ancestor::tr)]");

                string companyName = document.DocumentNode.SelectNodes("//h1")[0].InnerText;
                if (!companyList.Contains(companyName))
                {
                    companyList.Add(companyName);
                    for (int i = 0; i < totalRows.Count; i++)
                    {
                        table.Rows.Add(DirectorShip[i * 2].SelectNodes(".//td").Select(td => td.InnerText).ToArray());

                        table.Rows[rowNumber]["Company"] = companyName;
                        table.Rows[rowNumber]["URL"] = url;
                        table.Rows[rowNumber]["Search Depth"] = depth;

                        rowNumber++;
                        if (depth > 1)
                        {
                            var companyAssociated = DirectorShip[2 * i + 1]
                                .SelectNodes(".//*[text()][(normalize-space(.)= 'Other Companies Associated with')]")[0]
                                .NextSibling.SelectNodes(".//a");

                            if (!(companyAssociated is null))
                            {
                                for (int j = 0; j < companyAssociated.Count; j++)
                                {
                                    string companyUrl = companyAssociated[j].Attributes["href"].Value;
                                    AddDirectors(companyUrl, depth - 1);
                                }
                            }
                        }

                    }
                }
            }
            return;
        }

        private void GenerateCSV(DataTable dt)
        {
            string path = @"C:\Users\Mehara\Desktop\test.csv";
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(path, sb.ToString());
        }

        public static bool Connect()
        {
            string url = "http://localhost:7474/db/data";
            string username = "neo4j";
            string password = "testing";

            var client = new GraphClient(new Uri(url), username, password);
            client.Connect();

            var query = client.Cypher.LoadCsv(new Uri("file:\\C:\\Users\\Mehara\\Desktop\\test.csv"), "row", true, periodicCommit: 500)
                .With("row.`Director Name` AS Name,row.`DIN` AS DIN, row.`Designation` AS Designation,row.`Company` AS Company,row.`Appointment Date` AS AppointmentDate,row.`URL` AS URL")
                .Merge("(d:Director{din:DIN,name:Name})")
                .Merge("(c:Company{companyName:Company,url:URL})")
                .With("Name,DIN,Company,URL,Designation,AppointmentDate")
                .Match("(x:Director{din:DIN,name:Name})")
                .Match("(y:Company{companyName:Company,url:URL})")
                .With("x,y,Name,DIN,Company,URL,Designation,AppointmentDate")
                .Merge("(x)-[r:Assocaited_With{designation:Designation,appointmentDate:AppointmentDate}]->(y)");

            query.ExecuteWithoutResults();

            return true;
        }
    }
}