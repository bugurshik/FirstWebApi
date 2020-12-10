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

// Получить данные о деталях
async function LoadDetails(id) {
    // отправляет запрос и получаем ответ
    const response = await fetch("/api/catalog/ " + id, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const answer = await response.json();
        console.log(answer)
        createTitle(answer.part)
        createDetailsRow(answer.details)
        createSubTable(answer.products);
    }
}


function createSubTable(products) {

    
  //  const thisDiv = document.getElementById("details-root")
    const Table = document.createElement('table')
  
    products.forEach(product => {

        console.log(product)
        const tr = document.createElement("tr")
        tr.setAttribute("item-id", product.name);

        const name = document.createElement("td");
        name.append(product.name);
        tr.append(name);

        const count = document.createElement("td");
        count.append(' количество: ' + product.price);
        tr.append(count);

        Table.append(tr)
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
            item.name += " " + " id: " + item.id 

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
        tr.setAttribute("Model", detail.model);
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