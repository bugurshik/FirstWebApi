"use strict"

const contentBox = document.getElementById("content")
let curentCatalogId
let selectedCatalog

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

// Получить данные о деталях
async function LoadDetails(id) {

    // Если контент отображается - спрятать
    if (contentBox.hasAttribute("class", "show")) {
        contentBox.classList.toggle("show")
        contentBox.style.opacity = 0
    }

    // отправляет запрос и получаем ответ
    const response = await fetch("/api/catalog/ " + id, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });

    // если запрос прошел нормально
    if (response.ok === true) {
        // анимация появления
        contentBox.classList.toggle("show")

        // получаем данные
        const answer = await response.json();

        // 
        createTitle(answer)
        createDetailsRow(answer.details)
    }
}
function createTitle(part) {
    const title = document.querySelector("h1")
    title.innerText = part.name
}


function createCatalog(catalog) {

    const root = document.getElementById('catalog-root')
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
            parent.append(thisDiv)

            if (item.hierarchy == 2) {
                thisDiv.setAttribute("id", item.id);
                thisDiv.className += " part";

                thisDiv.querySelector("a").addEventListener('click', function (e) {

                    e.preventDefault();

                    // Уже есть выбранный каталог?
                    if (typeof selectedCatalog != 'undefined') {
                        // Выбран активный каталог?
                        if (selectedCatalog == this.parentElement)
                            return
                        selectedCatalog.classList.toggle("selected")
                    }

                    selectedCatalog = selectedCatalog = this.parentElement
                    selectedCatalog.classList.add("selected")

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

        const row = document.createElement("tr")
        row.setAttribute("id", detail.model);
        Table.append(row)

        const modelTd = document.createElement("td");
        modelTd.append(detail.model);
        row.append(modelTd);

        const name = document.createElement("td");
        name.append(detail.name);
        row.append(name);

        const count = document.createElement("td");
        count.append(' количество: ' + detail.count);
        row.append(count);

        if (detail.products.length)
            createSubTable(detail.products, row);
    })
}

function createSubTable(products, parentRow) {
    products.forEach(product => {

        let goodsDiv = parentRow.parentElement.querySelector('.goods')

        // Нет контейнера с товароми?
        if (goodsDiv == null) {

            // создать контейнер товаров
            const rootIndex = parentRow.rowIndex
            const tableRow = parentRow.parentElement.insertRow(rootIndex + 1)

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

