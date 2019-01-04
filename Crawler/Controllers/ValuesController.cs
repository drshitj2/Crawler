using System.Web.Http;
using Crawler.DataLayer;

namespace Sample.Controllers
{
    public class ValuesController : ApiController
    {

        //Entry point for crawling

        [Route("Values/GetRelation")]
        [HttpGet]
        public bool GetRelation(string url, int depth)
        {
            ValuesDL v = new ValuesDL();
            v.Crawl(url, depth);
            return true;
        }
    }
}
