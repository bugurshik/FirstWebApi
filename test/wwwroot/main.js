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
    }
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

// Создание иерархии каталога
function createCatalog1(catalog) {

    const root = document.getElementById('catalog-root')
    let parent
    let thisDiv = root;

    catalog.forEach()

    for (let i = 0; i < Object.keys(catalog).length; i++) {

        console.log(i + " " + (+catalog[i].id - 1))
        catalog[i].name += " " + i + " id: " + catalog[i - 1].id + "H: " + catalog[i].hierarchy

        // Если элемент начальный тогда parent = root
        if (catalog[i].hierarchy == 0) {
            parent = root
        }
        else if (catalog[i].hierarchy > catalog[i - 1].hierarchy) {
            parent = thisDiv
        }
        else if (catalog[i].hierarchy < catalog[i - 1].hierarchy) {

            parent = parent.parentElement;
        } else if (catalog[i].hierarchy > catalog[i + 1].hierarchy) {
            catalog[i].name += "  " + thisDiv.innerText
        }


        thisDiv = CreateElement(catalog[i])
        thisDiv.setAttribute("Part-id", catalog[i].id - 1);
        parent.append(thisDiv)

        if (catalog[i].hierarchy == 2) {
            thisDiv.setAttribute("Part-id", catalog[i].id - 1);
            thisDiv.className += " part";

            thisDiv.querySelector("a").addEventListener('click', e => {
                e.preventDefault();
                LoadDetails(catalog[i].id)
            })
        }
    }
}