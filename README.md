# Читай, Пиши и не спиши

Учебный WPF-проект на C# и SQL Server.

## Что внутри

- авторизация и регистрация;
- каталог книг с поиском, сортировкой и фильтрацией;
- страница книги с чтением текста, отзывами и жалобами;
- списки книг пользователя;
- профиль пользователя и работа с заморозкой;
- страница автора с добавлением и редактированием книг;
- администрирование жалоб, заявок и пользователей.

## База данных

Проект рассчитан на SQL Server / SQL Server Express.

По умолчанию в [App.config](/C:/Users/pelog/source/repos/YP1/YP1/App.config) используется экземпляр:

`.\SQLEXPRESS`

Если у тебя другой экземпляр SQL Server, поменяй строки подключения `ServerConnection` и `LibraryConnection`.

При первом запуске приложение:

1. выполняет скрипт [CreateDatabase.sql](/C:/Users/pelog/source/repos/YP1/YP1/Scripts/CreateDatabase.sql);
2. создаёт базу `ReadWriteDontCheatDb`;
3. заполняет её тестовыми данными.

## Тестовые аккаунты

- `admin` / `admin123`
- `author1` / `author123`
- `reader1` / `reader123`
- `frozen1` / `frozen123`

## Сборка

Открыть `YP1.sln` в Visual Studio и запустить WPF-проект `YP1`.
