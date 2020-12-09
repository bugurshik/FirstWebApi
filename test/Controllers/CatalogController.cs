using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using test.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        CatalogContext db;
        public CatalogController(CatalogContext context)
        {
            db = context;
            if (!db.Catalog.Any())
            {
                var x = parsingCatalog("https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/");
               
                foreach (var y in x)
                {
                    db.Catalog.Add(new CatalogItem { ParentId = y.ParentId, Hierarchy = y.Hierarchy, Name = y.Name, Href = y.Href });
                    db.SaveChanges();
                }
                
                //ParsingAll("https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/");
            }

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            var Catal = new CatalogItem { ParentId = 0, Hierarchy = 1 };
            db.Catalog.Add(Catal);

            var part1 = new Part { CatalogId = 1 };
            var part2 = new Part { CatalogId = 2 };
            db.Parts.AddRange(part1, part2);

            var Det1 = new Detail { Name = "Table", Count = 2, PartId = 1 };
            var Det2 = new Detail { Name = "Floor", Count = 11, PartId = 2 };
            db.Details.AddRange(Det1, Det2);

           // var Prod1 = new Product { Name = "best", Price = 11 };
           // var Prod2 = new Product { Name = "worst", Price = 200 };
          //  var Prod3 = new Product { Name = "oors", Price = 50 };
          //  db.Products.AddRange(Prod1, Prod2, Prod3);

            //многие к многим
           // Prod1.Details.Add(Det1);
           // Prod1.Details.Add(Det2);
           // Prod2.Details.Add(Det2);
           // Prod3.Details.Add(Det1);

            db.SaveChanges();
        }

        //GET api/catalog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogItem>>> Get()
        {
            return await db.Catalog.ToListAsync();
        }

        //GET api/catalog/PartId
        [HttpGet("{CatalogId}")]
        public Answer Get(int CatalogId)
        {
            var part = db.Parts.FirstOrDefault(x => x.CatalogId == CatalogId);

            //System.Diagnostics.Debug.WriteLine((db.Parts.Find(1) == null) + "======================");
            var detail1 = db.Details.Where(x => x.Id == 0);
           // db.Details.Where(x => x.Part.Id == 2);
            var products = db.Products.Where(x => x.Id == 2);

           // var details = db.Details.Include(x => x.Part).Where(u => u.PartId == 3).ToList();
            foreach (Product user in products)
               System.Diagnostics.Debug.WriteLine($"{user.Name} - {user.Price}");

           // var details = db.Details.Include(x => x.Part).Where(u => u.PartId == 3).ToList();
            //foreach (Detail user in details)
                //System.Diagnostics.Debug.WriteLine($"{user.Name} - {user.Part?.Name}");

            // var product = db.Products.Include(db.Details.Where(x => x.PartId == PartId));
            // if (detail == null)
            // return NotFound();
            // var tt = new ObjectResult(product);
            return new Answer { Part = part, Products = products, Details = detail1};
               //var new ObjectResult(detail);
        }

        IEnumerable<CatalogItem> parsingCatalog(string href)
        {
            //=======================
            // Set variables
            //=======================
            List<CatalogItem> Elem = new List<CatalogItem> { };
            var web = new HtmlWeb();
            var htmlDoc = web.Load(href);
            var partsTree = htmlDoc.GetElementbyId("autoparts_tree");
            var Categoryes = partsTree.SelectNodes("ul/li");
            int id = 0;

            //=======================
            // Actions
            //=======================
            Recurs(partsTree, 0, 0);

            void Recurs(HtmlNode CatalogElem, int hierarchy, int parentId)
            {
                var childs = CatalogElem.SelectNodes("ul/li");
                
                foreach (var node in childs)
                {
                    id++;
                    string name = node.Element("a").GetDirectInnerText().Trim(); //название элемента каталога
                    System.Diagnostics.Debug.WriteLine(name + "----------------");
                    if (node.SelectNodes("ul/li") == null)
                    {
                        var detailsHref = "https://www.avtoall.ru" + node.Element("a").Attributes["href"].Value;
                        Elem.Add(new CatalogItem { ParentId = parentId, Hierarchy = hierarchy, Name = name, Href = detailsHref });
                    }
                    else
                    {
                        Elem.Add(new CatalogItem { ParentId = parentId, Hierarchy = hierarchy, Name = name});
                        Recurs(node, hierarchy + 1, id);
                    }
                }
            }
            return Elem;
        }


        void ParsingAll(string href)
        {
            int counter = 0;
            var catalog = db.Catalog.Select(c => new { c.Href, c.Id }).Where(x => x.Href!=null).ToList();
            foreach (var element in catalog)
            {
                counter++;
                parsingParts(element.Href, element.Id);
                if (counter > 10)
                {
                    break;
                }
            }
        }
        void parsingParts(string href, int catalogId) // Возвращает элементы части
        {
            // Получить документ
            HtmlWeb Web = new HtmlWeb();
            var HtmlDoc = Web.Load(href).DocumentNode;


            // Поиск по документу
            var Images = HtmlDoc.Descendants("img");
            string name = HtmlDoc.QuerySelector("h1").GetDirectInnerText().Trim();

            // debug
            System.Diagnostics.Debug.WriteLine(name);

            //TODO: добавить алгоритм записи картинки
            // Запись в DB
            Part NewPart = new Part { Name = name, CatalogId = catalogId};
            db.Parts.Add(NewPart);
            db.SaveChanges();

            // Парсинг деталей 
            parsingDetails(HtmlDoc, NewPart);
            
        }
        void parsingDetails(HtmlNode HtmlDoc, Part part)
        {
            Detail NewDetail = new Detail { };
            var DetailsNodes = HtmlDoc.SelectNodes("//tr[contains(@class, 'part')]");

            foreach (var detailNode in DetailsNodes)
            {
                // У детали есть товары?
                if (detailNode.HasClass("goods"))
                {
                    // Парсинг товаров этой детали
                    parsingGoods(detailNode, NewDetail.Model, NewDetail);
                    continue;
                }
                // поиск по части документа
                string position = detailNode.QuerySelector(".position").InnerText;
                string model = detailNode.QuerySelector(".number").InnerText.Trim();
                string name = detailNode.QuerySelector(".name").InnerText.Trim();
                /*на некоторых страницах нет значения "Count"*/
                string count = detailNode.QuerySelector(".count span") == null ? "0" : detailNode.QuerySelector(".count span").InnerText.Trim();

                // debug
                //System.Diagnostics.Debug.WriteLine("{0} | {1} | {2} | частей : {3}", position.PadRight(4).PadLeft(30), model.PadRight(20), name.PadRight(70), count);

                // Запись в DB
                NewDetail = new Detail { Model = model, Count = Convert.ToInt32(count), PartId = part.Id, Name = name };
                db.Details.Add(NewDetail);
                db.SaveChanges();
            }
        }
        void parsingGoods(HtmlNode GoodsContainer, string detailNumber, Detail detail)
        {
            string name;
            int price;

            Product NewProduct;
            foreach (var goods in GoodsContainer.SelectNodes("//div[contains(@class, 'item-elem')]"))
            {
                name = goods.QuerySelector("strong").InnerText;
                price = Convert.ToInt32(goods.QuerySelector(".price-internet").InnerText.Split()[0]);

                //TODO: добавить алгоритм записи картинки
                // image = 

                //debug
                System.Diagnostics.Debug.WriteLine(price + "  " + name);

                // Запись в DB
                NewProduct = new Product { Price = price, Name = name, DetailId = detailNumber };
                // добавить в таблицу
                NewProduct.Details.Add(detail);
                // добавить связь многие-многие
                db.Products.Add(NewProduct);
                db.SaveChanges();
            }
        }
    }
}
