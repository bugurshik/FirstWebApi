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
        createPart(answer)
    }
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
    // Создать элемент каталога
    function CreateElement(item) {
        const div = document.createElement("div");
        div.setAttribute("class", 'element');
        const title = document.createElement("a");
        title.append(item.name)
        div.append(title)
        return div
    }
}

function createPart(part) {

    // название части
    const title = document.querySelector("h1")
    title.innerText = part.name

    // картинка части
    document.querySelector("img").src = "data:image/gif;base64," + part.image

    // создать таблицу деталей 
    createDetailsTable(part.details)
}

function createDetailsTable(details) {

    const Table = document.getElementById("details-root")
    Table.innerHTML = "";

    details.forEach(detail => {

        // строка-контейнер
        const row = document.createElement("tr")
        row.setAttribute("id", detail.model);
        Table.append(row)

        // модель
        const modelTd = document.createElement("td");
        modelTd.append(detail.model);
        row.append(modelTd);

        // название 
        const name = document.createElement("td");
        name.append(detail.name);
        row.append(name);

        // количество
        const count = document.createElement("td");
        count.append(' количество: ' + detail.count);
        row.append(count);

        // Есть товары у детали?
        if (detail.products.length)
            createGoodsContainer(row, detail.products)        
    })
}

function createGoodsContainer(parentRow, goods) {

    // создать строку товаров в таблице
    const subrow = document.createElement('td')
    subrow.setAttribute('colspan', 3)

    // контейнер для товаров
    const goodsDiv = document.createElement('div')
    goodsDiv.setAttribute('class', 'goods')
    subrow.append(goodsDiv)

    // назначить индекс строки
    const rootIndex = parentRow.rowIndex
    const tableRow = parentRow.parentElement.insertRow(rootIndex + 1)
    tableRow.append(subrow)

    // создать товары
    goods.forEach(product => {
        createProductItem(product, goodsDiv);
    })
    
}

function createProductItem(product, parentContainer) {

    // контейнер
    const productItem = document.createElement('div')
    productItem.setAttribute('class', 'product')
    parentContainer.append(productItem)

    // картинка
    const image = document.createElement("img")
    image.src = "data:image/gif;base64," + product.image
    image.setAttribute("class", "product-img")
    productItem.append(image)

    // название
    const cellName = document.createElement('div');
    cellName.append(product.name);
    productItem.append(cellName);

    // цена
    const price = document.createElement("div");
    price.append(' Цена: ' + product.price);
    productItem.append(price);

    
}

// загрузка каталога
LoadCatalog();