# Chess 2

## Opis projektu

Ta wersja projektu jest trzecim podejściem do problemu. Pierwsza wersja aplikacji działała na bazie MonoGame, ale z uwagi na brak możliwości łatwego tworzenia elementów interfejsu jej rozwój był utrudniony. Druga wersja została napisana w języku C# z użyciem Windows Presentation Foundation (WPF), jednak zastosowanie modelu MVVM okazało się trudne do pogodzenia z funkcjonalnością aplikacji (przeciąganie figur, proceduralne generowanie planszy itp.). Ostatecznie wersja trzecia nadal jest napisana w C# i WPF-ie, ale nie jest podzielona na części, jak miało to miejsce w przypadku MVVM.

Projekt został podzielony na cztery moduły:
- **ChessEngine** – silnik i logika gry,
- **ChessUI** – aplikacja zawierająca interfejs użytkownika,
- **Tests** – testy generowania legalnych ruchów,
- **MultiplayerTest** – pomocniczy projekt do testowania połączeń sieciowych.

## Uruchomienie
Aplikacja wymaga werjsi .NET 8 oraz projektem startowym powinien być projekt **ChessUI** lub **Tests**

## Chess Engine

Silnik gry do reprezentacji planszy wykorzystuje **mailbox** oraz listę figur (co przyspiesza wyszukiwanie). Ruchy są generowane pseudo-legalnie.

Aby zwiększyć wydajność, silnik używa **tabel transpozycyjnych** do śledzenia wcześniej przeanalizowanych pozycji oraz potrafi zapisywać planszę jako unikalny hash.

## Chess UI

Ze względu na rezygnację z modelu MVVM klasa `ChessboardView` znacznie się rozrosła (łącznie 890 linii kodu). Z tego powodu konieczne było podzielenie jej na osobne pliki w celu zwiększenia przejrzystości kodu.

## Wnioski i plany na przyszłość

Podczas pracy nad projektem nauczyliśmy się wielu ciekawych rzeczy o szachach i ich działaniu. Planujemy w przyszłości ponownie zająć się programowaniem szachów, tym razem mając na uwadze wydajność. Chcemy wykorzystać język **C++**, **bitboardy** oraz wielowątkowość aby rzucić kieydś wyzwanie Stockfishowi – albo przynajmniej jego starszej wersji.


![image](https://github.com/user-attachments/assets/8fb1ad2e-12eb-4800-b21e-aca4ccef1caa)


