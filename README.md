# Stardew Valley Trainer

Ein Ingame-Trainer für Stardew Valley 1.6.15 unter Windows. Öffne im Einzelspieler mit **F5** das Menü.

| Menü | Funktionen |
| --- | --- |
| Spieler | Gold, Gesundheit, Ausdauer und Rucksackplätze |
| Welt | Spieluhr anhalten und zu Orten im Grundspiel reisen |
| Items | Item-Katalog mit Suche nach Name oder ID, Menge und Qualität |
| Farm | Bepflanzten Boden am aktuellen Ort gießen und lebende Kulturen erntereif machen |
| Fortschritt | Erfahrung für die fünf Fähigkeiten des Grundspiels hinzufügen |

**F6** gibt 10.000 Gold, **F7** heilt, **F8** füllt die Ausdauer und **F9** schaltet die Zeitsperre um. Die Zeitsperre endet am nächsten Morgen. Änderungen an Gold, Inventar, Farm und Fähigkeiten werden mit dem Spielstand gespeichert, wenn das Spiel regulär speichert.

## Bauen und installieren

1. Installiere [SMAPI](https://smapi.io/) für deine Stardew-Valley-Version. Der Trainer wurde gegen Stardew Valley 1.6.15 und SMAPI 4.1.10 gebaut.
2. Baue den Trainer mit einem installierten .NET SDK. Setze `-GamePath` auf deinen Spielordner.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -GamePath 'C:\Pfad\zu\Stardew Valley'
   ```

3. Kopiere den Ordner `dist\StardewValleyTrainer` in `Mods` im Spielverzeichnis.
4. Starte `StardewModdingAPI.exe` und lade einen Einzelspieler-Spielstand.

Die geplante nächste Ausbaustufe ist die Unterstützung für Stardew Valley Expanded.
