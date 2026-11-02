# Инструкция по сборке и запуску (Lab 2)

### 1. Требования
* Установленный **.NET 8 SDK** (проверить версию: `dotnet --version`).

### 2. Получение проекта
Вы можете получить проект любым удобным способом:

* **Вариант А (Скачать архив):**
  1. Перейдите на страницу репозитория: [https://github.com/Zahartshenko-ship-it/Lab2OS](https://github.com/Zahartshenko-ship-it/Lab2OS)
  2. Нажмите зелёную кнопку **Code** $\rightarrow$ **Download ZIP**.
  3. Распакуйте архив в удобную папку на компьютере.

* **Вариант Б (Через терминал):**
  Склонируйте репозиторий с помощью Git и перейдите в папку проекта:
  ```bash
  git clone [https://github.com/Zahartshenko-ship-it/Lab2OS.git](https://github.com/Zahartshenko-ship-it/Lab2OS.git)
  cd Lab2OS
### 3. Запуск программы
Вы можете запустить программу в релизном режиме:
```bash
dotnet run -c Release
```
Также вы можете запустить программу с параметрами
* dotnet run -c Release -- [N] [потоки] [static/dynamic]
```bash
dotnet run -c Release -- 2000 10 dynamic
```
