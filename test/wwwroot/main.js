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
        createTitle(answer.part)
        createDetailsRow(answer.details)
        createSubTable(answer.products);
    }
}


function createSubTable(products) {

    
    const thisDiv = document.getElementById("details-root")
    thisDiv.innerHTML = "";

    details.forEach(product => {

        const container = document.getElementById("details-root")

        const tr = document.createElement("tr")
        tr.setAttribute("item-id", product.id);

        const name = document.createElement("td");
        name.append(product.name);
        tr.append(name);

        const count = document.createElement("td");
        count.append(' количество: ' + detail.count);
        tr.append(count);

        container.append(tr)
        thisDiv.append(container)
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

            console.log(item.id + "  " + item.name)
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

    const thisDiv = document.getElementById("details-root")
    thisDiv.innerHTML = "";

    details.forEach(detail => {
        const tr = document.createElement("tr")
        tr.setAttribute("data-rowid", details.id);

        const modelTd = document.createElement("td");
        modelTd.append(detail.model);
        tr.append(modelTd);

        const name = document.createElement("td");
        name.append(detail.name);
        tr.append(name);

        const count = document.createElement("td");
        count.append(' количество: ' + detail.count);
        tr.append(count);

        thisDiv.append(tr)
    })
}

function createTitle(part) {
    const title = document.querySelector("h1")
    title.innerText = part.name
}
