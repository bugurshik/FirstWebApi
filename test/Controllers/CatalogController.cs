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
        }

        //GET api/catalog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogItem>>> Get()
        {
            // Каталог пуст?
            if (!db.Catalog.Any())
                // Парсинг прервался?
                if (!parsingCatalog("https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/"))
                    return NotFound();

            return await db.Catalog.ToListAsync();
        }

        //GET api/catalog/PartId
        [HttpGet("{CatalogId}")]
        public async Task<ActionResult> Get(int CatalogId)
        {
            // Информации о части нет?
            if (!db.Parts.Where(x => x.CatalogId == CatalogId).Any())
            {
                string href = db.Catalog.Find(CatalogId).Href;

                // Ссылка на эту часть отсутствует?
                if (href == null)
                    return NotFound();

                // Парсинг прервался?
                if (!parsingParts(href, CatalogId))
                    return NotFound();
            }
            Part part = await db.Parts.Include(x => x.Details).ThenInclude(d => d.Products).FirstOrDefaultAsync(y => y.CatalogId == CatalogId);
            return new ObjectResult(part);

            //Byte[] b = part.Image;       
            //return File(b, "image/jpeg");
        }

        bool parsingCatalog(string href)
        {
            //=======================
            // Set variables
            //=======================
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

                    if (node.SelectNodes("ul/li") == null)
                    {
                        var detailsHref = "https://www.avtoall.ru" + node.Element("a").Attributes["href"].Value;
                        db.Catalog.Add(new CatalogItem { ParentId = parentId, Hierarchy = hierarchy, Name = name, Href = detailsHref });
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Catalog.Add(new CatalogItem { ParentId = parentId, Hierarchy = hierarchy, Name = name });
                        db.SaveChanges();
                        Recurs(node, hierarchy + 1, id);
                    }
                }
            }
            db.SaveChanges();
            return true;
        }

        bool parsingParts(string href, int catalogId) // Возвращает элементы части
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

            // Запись в DB
            Part NewPart = new Part { Name = name, CatalogId = catalogId, Image = data };
            db.Parts.Add(NewPart);

            // Парсинг деталей 
            parsingDetails(HtmlDoc, NewPart);
            db.SaveChanges();
            return true;
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
                System.Diagnostics.Debug.WriteLine("Parsing detail... model: " + NewDetail.Model);

                // Запись в DB
                NewDetail = new Detail { Model = model, Count = Convert.ToInt32(count), Part = part, Name = name };
                db.Details.Add(NewDetail);
                db.SaveChanges();
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
                System.Diagnostics.Debug.WriteLine("Parsing product... id: " + detail.Id );
                NewProduct.Details.Add(detail); 
                // добавить связь "многие-многие"
                db.Products.Add(NewProduct);
                db.SaveChanges();
            }
        }
    }
}
