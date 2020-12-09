using System.Collections.Generic;
using test.Models;

namespace test
{
    public class Answer
    {
        public Part Part { get; set; }
        public IEnumerable<Detail> Details { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
