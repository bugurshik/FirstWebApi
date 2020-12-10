using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using test.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Drawing;
using System.IO;
using System.Net;

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
                var catalog = parsingCatalog("https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/");               
                foreach (var Item in catalog)
                {
                    db.Catalog.Add(Item);
                    db.SaveChanges();
                }
            }
        }

        //GET api/catalog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogItem>>> Get()
        {
            return await db.Catalog.ToListAsync();
        }

        //GET api/catalog/PartId
        [HttpGet("{CatalogId}")]
        public ActionResult Get(int CatalogId)
        {
            var part = db.Parts.FirstOrDefault(x => x.CatalogId == CatalogId);
            if (part == null)
            {
                var href = db.Catalog.Find(CatalogId).Href;
                parsingParts(href, CatalogId);
                db.SaveChanges();
            }

            
            var test = db.Database.ExecuteSqlRaw("select * from Products join (select Model from Details as d right join(select Id from Parts Where Parts.CatalogId = 7) as p on p.Id = d.PartId) AS m on m.Model = Products.DetailModel");
            var huh = from p in db.Products
                      join d in db.Details.Where(d => d.Part == part) on p.DetailModel equals d.Model
                      select new
                      {
                          model = p.DetailModel
                      };

            //Byte[] b = part.Image;       
            //return File(b, "image/jpeg");

            var detail = db.Details.Where(d => d.Part == part).ToList();

            var products = db.Products.Include(x => x.Details.Where(d => d.Part == part)).ToList();

            return new ObjectResult(huh);
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
            db.SaveChanges();
        }

        void parsingParts(string href, int catalogId) // Возвращает элементы части
        {
            // Получить документ
            HtmlWeb Web = new HtmlWeb();
            var HtmlDoc = Web.Load(href).DocumentNode;

            // Поиск по документу
            var imageUrl = HtmlDoc.QuerySelector("#picture_img").GetAttributeValue("src","");

            byte[] data;
            using (WebClient webClient = new WebClient())
            {
                data = webClient.DownloadData("https://yandex.ru/images/search?pos=1&from=tabbar&img_url=https%3A%2F%2Fsun9-19.userapi.com%2Fc851120%2Fv851120612%2Fd4da%2FylKmY4VPl2A.jpg&text=картинки+здесь+rfhnbyrf+yt+yfqltyf&rpt=simage");
            }

            string name = HtmlDoc.QuerySelector("h1").GetDirectInnerText().Trim();

            // debug
            System.Diagnostics.Debug.WriteLine(name);

            //TODO: добавить алгоритм записи картинки
            // Запись в DB
            Part NewPart = new Part { Name = name, CatalogId = catalogId, Image = data };
            db.Parts.Add(NewPart);

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
                    db.SaveChanges();
                    // Парсинг товаров этой детали
                    parsingGoods(detailNode, NewDetail);
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
                NewDetail = new Detail { Model = model, Count = Convert.ToInt32(count), Part = part, Name = name };
                db.Details.Add(NewDetail);
                }
        }

        void parsingGoods(HtmlNode GoodsContainer, Detail detail)
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

                // Запись в DB
                NewProduct = new Product { Price = price, Name = name, DetailModel = detail.Model};
                // добавить в таблицу

                //debug
                System.Diagnostics.Debug.WriteLine(detail.Id + "=================");
                NewProduct.Details.Add(detail); 
                // добавить связь "многие-многие"
                db.Products.Add(NewProduct);
                db.SaveChanges();
            }
        }


    }
}
