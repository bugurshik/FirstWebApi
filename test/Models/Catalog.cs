using System.Collections.Generic;

namespace test.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int Hierarchy { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
    }
    public class Part
    {
        public int Id { get; set; } // Первичный ключ
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public int CatalogId { get; set; }
        public virtual List<Detail> Details { get; set; } = new List<Detail>(); //Один ко многим -> Detail
    }

    public class Detail
    {
        public int Id { get; set; } // Первичный ключ
        public string Model { get; set; }
        public int? Count { get; set; }
        public string Name { get; set; }
        public int PartId { get; set; }
        public  Part Part { get; set; } //Один ко многим <- Parts
        public virtual List<Product> Products { get; set; } = new List<Product>(); // Многие ко многим Product
    }

    public class Product
    {
        public int Id { get; set; } // Первичный ключ
        public string DetailModel { get; set; }
        public string Name { get; set; }
        public int? Price { get; set; }
        public byte[] Image { get; set; }
        public virtual List<Detail> Details { get; set; } = new List<Detail>(); // Многие ко многим Detail
    }
    public class Answer
    {
        public Part Part { get; set; }
        public IEnumerable<Detail> Details { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}

