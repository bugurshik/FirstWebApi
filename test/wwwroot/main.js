"use strict"

// Получить данные о каталоге
async function LoadCatalog() {
    // отправляет запрос и получаем ответ
    const response = await fetch("/api/catalog", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные

        const catalogList = await response.json();
        createCatalog(catalogList);
   }
}

let curentCatalog
// Получить данные о деталях
async function LoadDetails(id) {

    if (curentCatalog == id)
        return
    curentCatalog = id

    // отправляет запрос и получаем ответ
    const response = await fetch("/api/catalog/ " + id, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const answer = await response.json();
        createTitle(answer.part)
        createDetailsRow(answer.part.details)
        createSubTable(answer.products.value);
    }
}


function createSubTable(products) {

    products.forEach(product => {

        //console.log(product)

        const Root = document.getElementById(product.model)

        let goodsDiv = Root.parentElement.querySelector('.goods')

        console.log(Root.innerHTML)
       
        // Нет контейнера с товароми?
        if (goodsDiv == null) {

            // создать контейнер товаров

            const rootIndex = Root.rowIndex
            const tableRow = Root.parentElement.insertRow(rootIndex + 1)

            const subrow = document.createElement('td')
            subrow.setAttribute('colspan', 3)
            tableRow.append(subrow)

            goodsDiv = document.createElement('div')
            goodsDiv.setAttribute('class', 'goods')
            subrow.append(goodsDiv)

        }
        

        const productItem = document.createElement('div')
        productItem.setAttribute('class', 'product')
        goodsDiv.append(productItem)

        const cellName = document.createElement('div');
        cellName.append(product.name);
        productItem.append(cellName);

        const price = document.createElement("div");
        price.append(' Цена: ' + product.price);
        productItem.append(price);
    })

}

function createCatalog(catalog) {

    const root = document.getElementById('catalog-root')
    root.id = -1
    let parent
    let thisDiv = root;
    let previosElem;

    catalog.forEach(
        function (item) {

            // Если элемент начальный тогда parent = root
            if (item.hierarchy == 0) {
                parent = root
            }
            else if (item.hierarchy < previosElem.hierarchy) {

                parent = parent.parentElement;
            } 
            else if (item.hierarchy > previosElem.hierarchy) {
                parent = thisDiv
             }


            thisDiv = CreateElement(item)
            thisDiv.setAttribute("Part-id", item.id);
            parent.append(thisDiv)

            if (item.hierarchy == 2) {
                thisDiv.setAttribute("Part-id", item.id);
                thisDiv.className += " part";

                thisDiv.querySelector("a").addEventListener('click', e => {
                    e.preventDefault();
                    LoadDetails(item.id)
                })
            }
            previosElem = item
        })
}

// Создать элемент каталога
function CreateElement(item) {
    const div = document.createElement("div");
    div.setAttribute("class", 'element');
    const title = document.createElement("a");
    title.append(item.name)
    div.append(title)
    return div
}

// загрузка каталога
LoadCatalog();

function createDetailsRow(details) {

    
    const Table = document.getElementById("details-root")
    Table.innerHTML = "";

    details.forEach(detail => {

        const tr = document.createElement("tr")
        tr.setAttribute("id", detail.model);
        Table.append(tr)

        const modelTd = document.createElement("td");
        modelTd.append(detail.model);
        tr.append(modelTd);

        const name = document.createElement("td");
        name.append(detail.name);
        tr.append(name);

        const count = document.createElement("td");
        count.append(' количество: ' + detail.count);
        tr.append(count);

    })
}

function createTitle(part) {
    const title = document.querySelector("h1")
    title.innerText = part.name
}