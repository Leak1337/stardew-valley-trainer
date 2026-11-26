# Stardew Valley Trainer

[English instructions](README.en.md)

Ein Ingame-Trainer für Stardew Valley 1.6.15 unter Windows. Öffne im Einzelspieler mit **F5** das Menü. Der Trainer folgt der Spielsprache und unterstützt Deutsch und Englisch.

| Menü | Funktionen |
| --- | --- |
| Spieler | Gold, Gesundheit, Ausdauer und Rucksackplätze |
| Welt | Spieluhr anhalten, Sonne oder Regen für morgen einstellen und zu Orten im Grundspiel reisen |
| Items | Item-Katalog mit Suche nach Name oder ID, Menge und Qualität |
| Farm | Felder gießen, Kulturen erntereif machen, Gießkannen auffüllen und Tiere am aktuellen Ort füttern oder streicheln |
| Fortschritt | Erfahrung für die fünf Fähigkeiten des Grundspiels hinzufügen |
| Freundschaft | Freundschaftspunkte für bereits bekannte Dorfbewohner hinzufügen |
| Geschenke | Aufenthaltsort, Geburtstag, Herzen, Geschenkzähler und Geschenkvorlieben je Bewohner. Empfohlene Items einzeln ins Inventar legen |
| Rezepte | Gelernte Rezepte zählen und Koch- oder Handwerksrezepte freischalten |

Unter **Geschenke** wechselst du oben zwischen Bewohnern und unten zwischen Gegenständen. **Liebt** zeigt die besten Geschenke. Die Bewertungen stammen aus dem Spiel. Ein Klick auf **+1 ins Inventar** gibt dir den Gegenstand, damit du ihn selbst verschenken kannst.

Die Zeitsperre endet am nächsten Morgen. Besondere Spieltage können das eingestellte Wetter überschreiben. Dauerhafte Änderungen werden beim regulären Speichern übernommen.

## Bauen und installieren

1. Installiere [SMAPI](https://smapi.io/) für deine Stardew-Valley-Version. Der Trainer wurde gegen Stardew Valley 1.6.15 und SMAPI 4.1.10 gebaut.
2. Baue den Trainer mit einem installierten .NET SDK. Setze `-GamePath` auf deinen Spielordner.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -GamePath 'C:\Pfad\zu\Stardew Valley'
   ```

3. Kopiere den Ordner `dist\StardewValleyTrainer` in `Mods` im Spielverzeichnis.
4. Starte `StardewModdingAPI.exe` und lade einen Einzelspieler-Spielstand.

Die geplante nächste Ausbaustufe ist die Unterstützung für Stardew Valley Expanded.
