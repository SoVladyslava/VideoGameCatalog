// ПОРТ 
const API_URL = "https://localhost:7086/api";


let currentCategoryId = null;
let currentBrandId = null;
let currentGenreId = null;
// Змінна для перевірки, чи адмін зараз на сайті
let isAdmin = false;

// Змінна, щоб знати, що ми зараз редагуємо в довідниках (brand, category або genre)
let currentRefType = '';

// Глобальний список усіх можливих атрибутів (Платформа, Розробник...)
let allAttributesList = [];

// ▼▼▼ ДОДАЙТЕ ЦІ ЗМІННІ ▼▼▼
let allBrandsList = [];
let allCategoriesList = [];
let allGenresList = [];

document.addEventListener("DOMContentLoaded", () => {
    const savedRole = localStorage.getItem('userRole');
    if (savedRole === 'Admin') {
        isAdmin = true;
        updateAdminUI();
    }
    loadAllData();
});

function loadAllData() {
    loadCategories();
    loadBrands();
    loadGenres();
	loadAttributes(); // Завантажуємо список атрибутів
    loadGames();
}

// 1. Завантаження списку назв атрибутів
async function loadAttributes() {
    try {
        const response = await fetch(`${API_URL}/attributes`);
        allAttributesList = await response.json();
    } catch (e) { console.error("Помилка завантаження атрибутів", e); }
}
// 2. Функція додавання візуального рядка в модалку
function addAttributeRow(selectedId = null, value = '') {
    const container = document.getElementById('attributes-container');
    
    const row = document.createElement('div');
    row.className = 'attribute-row'; // Клас для пошуку при збереженні
    row.style.display = 'flex';
    row.style.gap = '5px';
    row.style.marginBottom = '5px';

    // Випадаючий список (Select)
    let options = '<option value="">-- Виберіть --</option>';
    allAttributesList.forEach(attr => {
        const name = attr.attributeName || attr.name; 
        const isSelected = (selectedId && attr.id == selectedId) ? 'selected' : '';
        options += `<option value="${attr.id}" ${isSelected}>${name}</option>`;
    });


    const selectHtml = `<select class="attr-select" style="flex:1; padding:8px; background:#111; color:white; border:1px solid #555;">${options}</select>`;
    
    // Поле введення (Input)
    const inputHtml = `<input type="text" class="attr-value" value="${value}" placeholder="Значення" style="flex:1; padding:8px; background:#111; color:white; border:1px solid #555;">`;
    
    // Кнопка видалення (X)
    const btnHtml = `<button onclick="this.parentElement.remove()" style="background:#cf6679; border:none; color:white; cursor:pointer; padding:0 10px;">X</button>`;

    row.innerHTML = selectHtml + inputHtml + btnHtml;
    container.appendChild(row);
}

// 3. Відкриття модалки для створення
function openAddGameModal() {
    document.getElementById('game-id').value = '';
    document.getElementById('game-modal-title').innerText = 'Додати нову гру';
    
    document.getElementById('new-name').value = '';
    document.getElementById('new-price').value = '';
    document.getElementById('new-rating').value = '';
    document.getElementById('new-brand').value = '';
    document.getElementById('new-category').value = '';
    document.getElementById('new-genres').value = '';
    
    // Очищаємо атрибути
    document.getElementById('attributes-container').innerHTML = '';
    
    document.getElementById('add-game-modal').style.display = 'block';
}

// 4. Відкриття модалки для редагування
async function openEditGameModal(event, id) {
    event.stopPropagation();
    const response = await fetch(`${API_URL}/videogames/${id}`);
    const game = await response.json();

    document.getElementById('game-id').value = game.id;
    document.getElementById('game-modal-title').innerText = 'Редагувати гру';
    document.getElementById('new-name').value = game.gameName;
    document.getElementById('new-price').value = game.price;
    document.getElementById('new-rating').value = game.rating;
	
	// 1. Спочатку малюємо порожні списки
    populateGameModalInputs();
	
	// 2. Вибираємо Бренд (знаходимо ID за назвою, бо в деталях приходить назва)
    const foundBrand = allBrandsList.find(b => b.brandName === game.brandName);
    if (foundBrand) document.getElementById('new-brand').value = foundBrand.id;

    // 3. Вибираємо Категорію
    const foundCat = allCategoriesList.find(c => c.categoryName === game.categoryName);
    if (foundCat) document.getElementById('new-category').value = foundCat.id;
	
	// 4. Ставимо галочки на Жанрах
    // (game.genreNames - це масив ["Action", "RPG"])
    if (game.genreNames) {
        game.genreNames.forEach(gName => {
            // Знаходимо ID жанру за його назвою
            const foundGenre = allGenresList.find(gl => gl.name === gName);
            if (foundGenre) {
                const checkbox = document.getElementById(`genre-cb-${foundGenre.id}`);
                if (checkbox) checkbox.checked = true;
            }
        });
    }
	
	// 5. Атрибути (як і було раніше)
    // Очищаємо і заповнюємо атрибути
    const container = document.getElementById('attributes-container');
    container.innerHTML = '';

    if (game.attributes) {
        game.attributes.forEach(gameAttr => {
            // Знаходимо ID атрибута за його назвою
            const foundAttr = allAttributesList.find(a => (a.attributeName || a.name) === gameAttr.attributeName);
            if (foundAttr) {
                addAttributeRow(foundAttr.id, gameAttr.value);
            }
        });
    }

   // alert("Увага: Введіть ID Бренду, Категорії та Жанрів вручну (вони не підтягуються автоматично в цьому прикладі).");
    document.getElementById('add-game-modal').style.display = 'block';
}

// 5. ЗБЕРЕЖЕННЯ ГРИ (РАЗОМ З АТРИБУТАМИ)
/*async function saveGame() {
    const id = document.getElementById('game-id').value;
    
    // Збираємо атрибути з рядків
    const attributes = [];
    const rows = document.querySelectorAll('.attribute-row');
    rows.forEach(row => {
        const select = row.querySelector('.attr-select');
        const input = row.querySelector('.attr-value');
        if (select.value && input.value) {
            attributes.push({
                attributeId: parseInt(select.value),
                value: input.value
            });
        }
    });

	// ▼▼▼ ЗБИРАЄМО ЖАНРИ З ГАЛОЧОК ▼▼▼
    const genreIds = [];
    const checkboxes = document.querySelectorAll('.genre-checkbox:checked'); // Тільки вибрані
    checkboxes.forEach(cb => {
        genreIds.push(parseInt(cb.value));
    });

    // ▼▼▼ ЗЧИТУЄМО БРЕНД І КАТЕГОРІЮ З SELECT ▼▼▼
    const brandSelect = document.getElementById('new-brand');
    const categorySelect = document.getElementById('new-category');
	
	
	// Валідація
    if (!brandSelect.value || !categorySelect.value || !genresSelect.value) {
        alert("Будь ласка, оберіть Бренд, Категорію та Жанр зі списку.");
        return;
    }
	
   // const genreIds = document.getElementById('new-genres').value
    //    .split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n));

    const gameData = {
        gameName: document.getElementById('new-name').value,
        price: parseFloat(document.getElementById('new-price').value),
        rating: parseFloat(document.getElementById('new-rating').value),
        //brandId: parseInt(document.getElementById('new-brand').value),
        //categoryId: parseInt(document.getElementById('new-category').value),
        //genreIds: genreIds,
		
		brandId: parseInt(brandSelect.value),     // <--- Значення з Select
        categoryId: parseInt(categorySelect.value), // <--- Значення з Select
        
        genreIds: genreIds, // <--- Масив з чекбоксів
		
        attributes: attributes // Відправляємо масив атрибутів
    };

    const isEdit = id !== '';
    const url = isEdit ? `${API_URL}/videogames/${id}` : `${API_URL}/videogames`;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(gameData)
        });

        if (response.ok) {
            document.getElementById('add-game-modal').style.display = 'none';
            loadGames();
            alert(isEdit ? "Зміни збережено!" : "Гру додано!");
        } else {
            alert("Помилка збереження");
        }
    } catch (e) { console.error(e); }
}*/
async function saveGame() {
    // 1. Отримуємо елементи форми
    const nameInput = document.getElementById('new-name');
    const priceInput = document.getElementById('new-price');
    const brandSelect = document.getElementById('new-brand');
    const categorySelect = document.getElementById('new-category');
    const genreContainer = document.getElementById('genres-checkbox-list'); // Контейнер жанрів
    
	
	const ratingInput = document.getElementById('new-rating'); // Отримуємо саме поле, щоб фарбувати рамку
    const ratingValue = parseFloat(ratingInput.value);
	
    // Знаходимо всі відмічені галочки жанрів
    const checkedGenres = document.querySelectorAll('.genre-checkbox:checked');

    // 2. --- ВАЛІДАЦІЯ (ПЕРЕВІРКА) ---
    
    let isValid = true;
    let errors = [];

    // Скидаємо старі червоні рамки (робимо їх знову сірими)
    nameInput.style.borderColor = '#555';
    priceInput.style.borderColor = '#555';
    brandSelect.style.borderColor = '#444'; // У select свій стиль рамки
    categorySelect.style.borderColor = '#444';
    genreContainer.style.borderColor = '#555';

    // Перевірка Назви
    if (!nameInput.value.trim()) {
        nameInput.style.borderColor = '#cf6679'; // Червоний колір помилки
        isValid = false;
        errors.push("Назва гри");
    }

    // Перевірка Ціни
    if (!priceInput.value) {
        priceInput.style.borderColor = '#cf6679';
        isValid = false;
        errors.push("Ціна");
    }

    // Перевірка Бренду
    if (!brandSelect.value) {
        brandSelect.style.borderColor = '#cf6679';
        isValid = false;
        errors.push("Бренд");
    }

    // Перевірка Категорії
    if (!categorySelect.value) {
        categorySelect.style.borderColor = '#cf6679';
        isValid = false;
        errors.push("Категорія");
    }

    // Перевірка Жанрів (має бути хоча б один)
    if (checkedGenres.length === 0) {
        genreContainer.style.borderColor = '#cf6679';
        isValid = false;
        errors.push("Хоча б один Жанр");
    }
	
	// ПЕРЕВІРКА РЕЙТИНГУ 
    // Перевіряємо, чи число в межах від 0 до 5 (якщо воно введене)
    if (ratingInput.value && (ratingValue < 0 || ratingValue > 5)) {
        ratingInput.style.borderColor = '#cf6679';
        isValid = false;
        errors.push("Рейтинг має бути від 0 до 5");
    } else {
        ratingInput.style.borderColor = '#555'; // Скидаємо колір, якщо все ок
    }

    // ЯКЩО Є ПОМИЛКИ - ЗУПИНЯЄМОСЯ
    if (!isValid) {
        alert("Дані не збережено! Будь ласка, заповніть:\n- " + errors.join("\n- "));
        return; // <--- ЦЕЙ RETURN НЕ ДАЄ КОДУ ЙТИ ДАЛІ
    }

    // 3. --- ЗБИРАЄМО ДАНІ (Цей код виконується тільки якщо все добре) ---

    const id = document.getElementById('game-id').value;
    
    // Атрибути
    const attributes = [];
    document.querySelectorAll('.attribute-row').forEach(row => {
        const select = row.querySelector('.attr-select');
        const input = row.querySelector('.attr-value');
        if (select.value && input.value) {
            attributes.push({ attributeId: parseInt(select.value), value: input.value });
        }
    });

    // Жанри (збираємо ID з відмічених чекбоксів)
    const genreIds = [];
    checkedGenres.forEach(cb => {
        genreIds.push(parseInt(cb.value));
    });

    const gameData = {
        gameName: nameInput.value,
        price: parseFloat(priceInput.value),
        rating: parseFloat(document.getElementById('new-rating').value) || null, // Рейтинг може бути пустим
        brandId: parseInt(brandSelect.value),
        categoryId: parseInt(categorySelect.value),
        genreIds: genreIds,
        attributes: attributes
    };

    // 4. --- ВІДПРАВКА НА СЕРВЕР ---

    const isEdit = id !== '';
    const url = isEdit ? `${API_URL}/videogames/${id}` : `${API_URL}/videogames`;
    const method = isEdit ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(gameData)
        });

        if (response.ok) {
            document.getElementById('add-game-modal').style.display = 'none';
            loadGames();
            alert(isEdit ? "Зміни збережено!" : "Гру додано!");
        } else {
            const errorText = await response.text();
            alert("Помилка сервера: " + errorText);
        }
    } catch (e) { 
        console.error(e);
        alert("Не вдалося з'єднатися з сервером.");
    }
}

// --- АВТОРИЗАЦІЯ ---

function openLoginModal() {
    document.getElementById('login-modal').style.display = 'block';
}

async function login() {
    const email = document.getElementById('email-input').value;
    const password = document.getElementById('pass-input').value;

    try {
        const response = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: email, password: password })
        });

        if (response.ok) {
            const data = await response.json();
            if (data.role === 'Admin') {
                isAdmin = true;
                localStorage.setItem('userRole', 'Admin'); // Зберігаємо сесію
                updateAdminUI();
                document.getElementById('login-modal').style.display = 'none';
                loadGames(); // Перезавантажити ігри, щоб з'явилися кнопки видалення
                alert("Вітаємо, Адміністратор!");
            } else {
                alert("У вас немає прав адміністратора.");
            }
        } else {
            alert("Невірний логін або пароль");
        }
    } catch (error) {
        console.error(error);
        alert("Помилка сервера");
    }
}

function logout() {
    isAdmin = false;
    localStorage.removeItem('userRole');
    updateAdminUI();
    loadGames(); // Перезавантажити, щоб прибрати кнопки
}

function updateAdminUI() {
    const loginBtn = document.getElementById('login-btn');
    const adminInfo = document.getElementById('admin-info');
    const adminToolbar = document.getElementById('admin-toolbar');

    if (isAdmin) {
        loginBtn.style.display = 'none';
        adminInfo.style.display = 'block';
        adminToolbar.style.display = 'block';
    } else {
        loginBtn.style.display = 'block';
        adminInfo.style.display = 'none';
        adminToolbar.style.display = 'none';
    }
}

// --- УПРАВЛІННЯ ДАНИМИ (Create / Update / Delete) ---

// Відкриття модалки для РЕДАГУВАННЯ
// --- ФУНКЦІЇ ДЛЯ ГРИ (Додавання та Збереження) ---

function openAddGameModal() {
    // 1. Очищаємо всі поля, щоб форма була пуста
    document.getElementById('game-id').value = ''; 
    document.getElementById('game-modal-title').innerText = 'Додати нову гру';
    
    document.getElementById('new-name').value = '';
    document.getElementById('new-price').value = '';
    document.getElementById('new-rating').value = '';
    // document.getElementById('new-brand').value = '';
    // document.getElementById('new-category').value = '';
    // document.getElementById('new-genres').value = '';
	
	// Очищаємо атрибути
    document.getElementById('attributes-container').innerHTML = '';
	
	// ▼▼▼ ЗАПОВНЮЄМО СПИСКИ ТА ЧЕКБОКСИ ▼▼▼
    populateGameModalInputs();

    // 2. Показуємо модальне вікно
    document.getElementById('add-game-modal').style.display = 'block';
}

async function deleteGame(event, id) {
    event.stopPropagation();
    if (!confirm("Видалити гру?")) return;
    await fetch(`${API_URL}/videogames/${id}`, { method: 'DELETE' });
    loadGames();
}
// --- УПРАВЛІННЯ ДОВІДНИКАМИ (Бренди, Категорії, Жанри) ---

async function openReferenceModal(type) {
    currentRefType = type; 
    document.getElementById('reference-modal').style.display = 'block';
    
    // Динамічні заголовки, щоб було зрозуміло, що ми робимо
    let title = '';
    let btnText = '';
    let placeholder = '';

    if (type === 'brand') {
        title = 'Керування Брендами';
        btnText = 'Додати Бренд';
        placeholder = 'Назва бренду (напр. Sony)';
    } else if (type === 'category') {
        title = 'Керування Категоріями';
        btnText = 'Додати Категорію';
        placeholder = 'Назва категорії (напр. PS5)';
    } else if (type === 'genre') {
        title = 'Керування Жанрами';
        btnText = 'Додати Жанр';
        placeholder = 'Назва жанру (напр. RPG)';
    }

    // Оновлюємо тексти в модалці
    document.getElementById('ref-modal-title').innerText = title;
    document.getElementById('ref-add-btn').innerText = btnText;
    document.getElementById('new-ref-name').placeholder = placeholder;
    document.getElementById('ref-input-label').innerText = `Створити новий запис у "${title}":`;
    
    loadReferenceItems();
}
async function loadReferenceItems() {
    let endpoint = currentRefType + 's';
    if (currentRefType === 'category') endpoint = 'categories';

    const response = await fetch(`${API_URL}/${endpoint}`);
    const items = await response.json();
    
    const list = document.getElementById('ref-list');
    list.innerHTML = '';

    if (items.length === 0) {
        list.innerHTML = '<p style="text-align:center; color:#777;">Список порожній</p>';
        return;
    }

    items.forEach(item => {
        const name = item.name || item.brandName || item.categoryName || item.genreName;
        
        const div = document.createElement('div');
        div.className = 'ref-item'; // Використовуємо новий CSS клас

        div.innerHTML = `
            <span style="font-size: 1.1em;">${name}</span>
            <div style="display:flex; gap: 10px;">
                <button class="filter-btn" style="padding: 5px 10px;" title="Редагувати" onclick="editReferenceItem(${item.id}, '${name}')">✏️</button>
                <button class="reset-btn" style="padding: 5px 10px;" title="Видалити" onclick="deleteReferenceItem(${item.id})">🗑️</button>
            </div>
        `;
        list.appendChild(div);
    });
}

async function addReferenceItem() {
    const name = document.getElementById('new-ref-name').value;
    if (!name) return;

    let endpoint = currentRefType + 's';
    if (currentRefType === 'category') endpoint = 'categories';

    await fetch(`${API_URL}/${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: name })
    });

    document.getElementById('new-ref-name').value = ''; // Очистити
    loadReferenceItems(); // Оновити список у модалці
    loadAllData(); // Оновити все на сайті
}

async function editReferenceItem(id, oldName) {
    const newName = prompt("Нова назва:", oldName);
    if (!newName || newName === oldName) return;

    let endpoint = currentRefType + 's';
    if (currentRefType === 'category') endpoint = 'categories';

    await fetch(`${API_URL}/${endpoint}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: newName })
    });
    
    loadReferenceItems();
    loadAllData();
}

/*async function deleteReferenceItem(id) {
    if (!confirm("Видалити? (Якщо є ігри з цим елементом, може виникнути помилка)")) return;

    let endpoint = currentRefType + 's';
    if (currentRefType === 'category') endpoint = 'categories';

    const response = await fetch(`${API_URL}/${endpoint}/${id}`, { method: 'DELETE' });
    
    if (response.ok) {
        loadReferenceItems();
        loadAllData();
    } else {
        alert("Не вдалося видалити. Можливо, цей елемент використовується в іграх.");
    }
}*/
async function deleteReferenceItem(id) {
    let confirmMessage = '';
    
    // Визначаємо, що ми видаляємо, щоб сформувати повідомлення
    if (currentRefType === 'brand' || currentRefType === 'category') {
        // Якщо Brand або Category, видаляються й ігри!
        confirmMessage = "⚠️ УВАГА! Видалення цієї(го) " + 
                         (currentRefType === 'brand' ? 'Бренду' : 'Категорії') + 
                         " призведе до БЕЗПОВОРОТНОГО ВИДАЛЕННЯ УСІХ ПОВ'ЯЗАНИХ ІГОР, ЇХ ЖАНРІВ ТА ХАРАКТЕРИСТИК. Ви впевнені?";
    } else if (currentRefType === 'genre') {
        // Якщо Genre, видаляються лише зв'язки
        confirmMessage = "Це видалить Жанр та усі зв'язки з іграми. Самі ігри залишаться. Ви впевнені?";
    } else {
        confirmMessage = "Ви впевнені, що хочете видалити цей елемент?";
    }

    if (!confirm(confirmMessage)) {
        return; // Якщо користувач відмовився, просто виходимо
    }

    // Якщо користувач підтвердив, продовжуємо видалення
    let endpoint = currentRefType + 's';
    if (currentRefType === 'category') endpoint = 'categories';

    const response = await fetch(`${API_URL}/${endpoint}/${id}`, { method: 'DELETE' });
    
    if (response.ok) {
        alert("Елемент успішно видалено.");
        loadReferenceItems(); // Оновити список у модалці
        loadAllData();        // Оновити список ігор на сайті
    } else {
        alert("Не вдалося видалити. Перевірте, чи не залишились непотрібні посилання в базі.");
    }
}

// --- СТАНДАРТНЕ ЗАВАНТАЖЕННЯ (Без змін) ---
// (Тут ваші старі функції loadGames, loadCategories...)
// ВАЖЛИВО: Оновіть createGame в index.html на onclick="saveGame()"
// ВАЖЛИВО: Оновіть loadGames, щоб додати кнопку "Редагувати"


// --- СТАНДАРТНІ ФУНКЦІЇ (Змінено лише loadGames) ---

// ... loadCategories, loadBrands, loadGenres, setFilter такі самі ... 
async function loadCategories() {
    const response = await fetch(`${API_URL}/categories`);
    const data = await response.json();
	allCategoriesList = data; // <--- ЗБЕРІГАЄМО У ЗМІННУ
    renderFilters(data, 'categories-list', 'category');
}
async function loadBrands() {
    const response = await fetch(`${API_URL}/brands`);
    const data = await response.json();
	allBrandsList = data; // <--- ЗБЕРІГАЄМО У ЗМІННУ
    renderFilters(data, 'brands-list', 'brand');
}
async function loadGenres() {
    const response = await fetch(`${API_URL}/genres`);
    const data = await response.json();
    const formattedData = data.map(g => ({ id: g.id, name: g.genreName })); 
	allGenresList = formattedData; // <--- ЗБЕРІГАЄМО У ЗМІННУ (вже відформатовані)
    renderFilters(formattedData, 'genres-list', 'genre');
}
function renderFilters(items, containerId, type) {
    const container = document.getElementById(containerId);
    container.innerHTML = ''; 
    const allBtn = document.createElement('button');
    allBtn.className = 'filter-btn active'; 
    allBtn.innerText = 'Всі';
    allBtn.onclick = () => setFilter(type, null, allBtn);
    container.appendChild(allBtn);

    items.forEach(item => {
        const btn = document.createElement('button');
        btn.className = 'filter-btn';
        const text = item.name || item.categoryName || item.brandName || item.genreName;
        btn.innerText = text;
        btn.onclick = () => setFilter(type, item.id, btn);
        container.appendChild(btn);
    });
}
function setFilter(type, id, clickedBtn) {
    if (type === 'category') currentCategoryId = id;
    if (type === 'brand') currentBrandId = id;
    if (type === 'genre') currentGenreId = id;
    const container = clickedBtn.parentElement;
    const buttons = container.getElementsByClassName('filter-btn');
    for (let btn of buttons) btn.classList.remove('active');
    clickedBtn.classList.add('active');
    loadGames();
}
function resetFilters() {
    currentCategoryId = null; currentBrandId = null; currentGenreId = null;
    location.reload();
}

async function loadGames() {
    const grid = document.getElementById('games-grid');
    grid.innerHTML = '<p>Завантаження...</p>';

    let url = `${API_URL}/videogames?`;
    if (currentCategoryId) url += `categoryId=${currentCategoryId}&`;
    if (currentBrandId) url += `brandId=${currentBrandId}&`;
    if (currentGenreId) url += `genreId=${currentGenreId}&`;

    try {
        const response = await fetch(url);
        const games = await response.json();
        grid.innerHTML = ''; 
        if (games.length === 0) { grid.innerHTML = '<p>Ігор не знайдено.</p>'; return; }

        games.forEach(game => {
            const card = document.createElement('div');
            card.className = 'game-card';
            card.onclick = () => openGameDetails(game.id); 

            let adminBtns = '';
            if (isAdmin) {
                adminBtns = `
                    <div style="margin-top:10px; display:flex; gap:5px;">
                        <button class="filter-btn" style="padding:5px;" onclick="openEditGameModal(event, ${game.id})">Ред.</button>
                        <button class="reset-btn" onclick="deleteGame(event, ${game.id})">Вид.</button>
                    </div>
                `;
            }

            card.innerHTML = `
                <h3>${game.gameName}</h3>
                <p style="color: #888;">${game.categoryName} | ${game.brandName}</p>
                <p class="price-tag">від ${game.price} грн</p>
                ${adminBtns}
            `;
            grid.appendChild(card);
        });
    } catch (error) { console.error(error); }
}

// ... openGameDetails, closeModal такі самі ...
async function openGameDetails(id) {
    const modal = document.getElementById('game-modal');
    const modalBody = document.getElementById('modal-body');
    
    // Показуємо "Завантаження", поки чекаємо відповідь
    modalBody.innerHTML = '<div style="text-align:center; padding:20px; color:#888;">Завантаження деталей...</div>';
    modal.style.display = "block";

    try {
        // 1. Робимо запит до API
        const response = await fetch(`${API_URL}/videogames/${id}`);
        
        if (!response.ok) {
            throw new Error("Гру не знайдено");
        }

        const game = await response.json();

        // 2. Форматуємо список жанрів
        const genresHtml = (game.genreNames && game.genreNames.length > 0) 
            ? game.genreNames.join(', ') 
            : '<span style="color:#666;">Не вказано</span>';

        // 3. Форматуємо список характеристик (ValueAttributes)
        let attributesHtml = '';
        
        // Перевіряємо, чи є масив attributes і чи він не порожній
        if (game.attributes && game.attributes.length > 0) {
            attributesHtml = `
                <div style="margin-top: 20px; background-color: #252525; border-radius: 8px; padding: 15px; border: 1px solid #444;">
                    <h4 style="margin-top: 0; color: #bb86fc; border-bottom: 1px solid #555; padding-bottom: 10px; margin-bottom: 10px;">
                        Технічні характеристики
                    </h4>
                    <ul style="list-style: none; padding: 0; margin: 0;">
            `;

            game.attributes.forEach(attr => {
                // Враховуємо можливі варіанти написання полів (camelCase vs PascalCase)
                const name = attr.attributeName || attr.AttributeName || "Характеристика";
                const value = attr.value || attr.Value || "—";

                attributesHtml += `
                    <li style="display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #333;">
                        <span style="color: #aaa;">${name}:</span>
                        <span style="color: #fff; font-weight: 500;">${value}</span>
                    </li>
                `;
            });

            attributesHtml += `
                    </ul>
                </div>
            `;
        } else {
            attributesHtml = '<p style="color:#666; font-style:italic; margin-top:20px;">Додаткові характеристики відсутні.</p>';
        }

        // 4. Збираємо все разом у HTML
        modalBody.innerHTML = `
            <h2 style="font-size: 2em; margin-bottom: 5px; color: white;">${game.gameName}</h2>
            
            <div style="display: flex; gap: 15px; color: #888; font-size: 0.9em; margin-bottom: 20px;">
                <span>${game.brandName || 'Бренд не вказано'}</span>
                <span>|</span>
                <span>${game.categoryName || 'Категорія не вказана'}</span>
            </div>

            <div style="display: flex; justify-content: space-between; align-items: center; background: #1a1a1a; padding: 15px; border-radius: 8px; border: 1px solid #333;">
                <div>
                    <span style="display:block; color:#aaa; font-size:0.8em;">Ціна</span>
                    <span class="price-tag" style="font-size: 1.5em;">${game.price} грн</span>
                </div>
                <div style="text-align: right;">
                    <span style="display:block; color:#aaa; font-size:0.8em;">Рейтинг</span>
                    <span style="font-size: 1.2em; color: #ffb74d;"> ${game.rating || 'N/A'} / 5</span>
                </div>
            </div>

            <div style="margin-top: 20px;">
                <strong style="color: #bb86fc;">Жанри:</strong> 
                <span style="color: #e0e0e0;">${genresHtml}</span>
            </div>

            ${attributesHtml}
        `;

    } catch (error) {
        console.error(error);
        modalBody.innerHTML = `<div style="text-align:center; padding:20px; color:#cf6679;">Помилка: ${error.message}</div>`;
    }
}
function closeModal() { document.getElementById('game-modal').style.display = "none"; }
window.onclick = function(event) {
    const gModal = document.getElementById('game-modal');
    const rModal = document.getElementById('reference-modal');
    const aModal = document.getElementById('add-game-modal');
    const lModal = document.getElementById('login-modal');
    if (event.target == gModal) gModal.style.display = "none";
    if (event.target == rModal) rModal.style.display = "none";
    if (event.target == aModal) aModal.style.display = "none";
    if (event.target == lModal) lModal.style.display = "none";
}
function populateGameModalInputs() {
    // 1. Заповнюємо Бренди (Select)
    const brandSelect = document.getElementById('new-brand');
    brandSelect.innerHTML = '<option value="">-- Оберіть Бренд --</option>';
    allBrandsList.forEach(b => {
        brandSelect.innerHTML += `<option value="${b.id}">${b.brandName}</option>`;
    });

    // 2. Заповнюємо Категорії (Select)
    const catSelect = document.getElementById('new-category');
    catSelect.innerHTML = '<option value="">-- Оберіть Категорію --</option>';
    allCategoriesList.forEach(c => {
        catSelect.innerHTML += `<option value="${c.id}">${c.categoryName}</option>`;
    });

    // 3. Заповнюємо Жанри (Checkboxes)
    const genreContainer = document.getElementById('genres-checkbox-list');
    genreContainer.innerHTML = '';
    allGenresList.forEach(g => {
        const div = document.createElement('div');
        div.className = 'checkbox-item';
        // value="${g.id}" - це ID жанру, який ми будемо відправляти
        div.innerHTML = `
            <input type="checkbox" id="genre-cb-${g.id}" value="${g.id}" class="genre-checkbox">
            <label for="genre-cb-${g.id}" style="margin:0; cursor:pointer;">${g.name}</label>
        `;
        genreContainer.appendChild(div);
    });
}