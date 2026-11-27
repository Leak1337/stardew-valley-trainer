# Stardew Valley Trainer

[English instructions](README.md)

Ein Ingame-Trainer für Stardew Valley 1.6.15 unter Windows. Öffne im Einzelspieler mit **F5** das Menü. Der Trainer folgt der Spielsprache und unterstützt Deutsch und Englisch.

| Menü | Funktionen |
| --- | --- |
| Spieler | Gold, Gesundheit, Ausdauer, Rucksackplätze, Lauftempo der Spielfigur und Grundspiel-Debuffs entfernen |
| Welt | Zu Orten teleportieren, zum letzten Absprungpunkt zurückkehren, Zeit anhalten oder vorspulen und Wetter für morgen einstellen |
| Items | Item-Katalog mit Suche nach Name oder ID, Menge und Qualität |
| Farm | Felder gießen, Kulturen erntereif machen, Gießkannen auffüllen, Tiere versorgen, Zäune reparieren und Maschinen fertigstellen |
| Fortschritt | Erfahrung für die fünf Fähigkeiten des Grundspiels hinzufügen |
| Freundschaft | Freundschaftspunkte für bereits bekannte Dorfbewohner hinzufügen |
| Geschenke | Aufenthaltsort, Geburtstag, Herzen, Geschenkzähler und Geschenkvorlieben je Bewohner. Empfohlene Items einzeln ins Inventar legen |
| Rezepte | Gelernte Rezepte zählen und Koch- oder Handwerksrezepte freischalten |
| Gemeinschaft | Bündel-Items in passender Qualität holen, Bündel oder Zentrum abschließen, Museumsspenden finden und Aufträge abschließen |
| Mods | Geladene Zusatzmods ansehen, eigene Schnellreisepunkte speichern und den Expanded-Item-Filter öffnen |
| Ausrüstung | Werkzeugstufe ändern, eine Minenebene wählen oder eine Leiter nach unten platzieren |
| Spielereien | Zufallsemotes, Lichtshow, Emote-Party und vorübergehende Glücks- und Magneteffekte |

Unter **Geschenke** wechselst du oben zwischen Bewohnern und unten zwischen Gegenständen. **Liebt** zeigt die besten Geschenke. Die Bewertungen stammen aus dem Spiel. Ein Klick auf **+1 ins Inventar** gibt dir den Gegenstand, damit du ihn selbst verschenken kannst.

Die Beträge für Gold, Items, EP und Freundschaftspunkte kannst du selbst eingeben.

Unter **Gemeinschaft → Gemeinschaftszentrum** kannst du die Bündel deines Spielstands durchsehen und fehlende Items in der benötigten Menge und Qualität holen. Du kannst die Items selbst abgeben, ein Bündel direkt abschließen oder nach Bestätigung alle übrigen Bündel erledigen. Bündelbelohnungen bleiben abholbar, und Räume werden nach ihren Bündeln freigeschaltet. Die Anzeige unterstützt gemischte Bündel. Die Aktionen stehen auf der Joja-Route nicht zur Verfügung.

**Gemeinschaft → Museum** zeigt fehlende Fundstücke für Gunther. Unter **Aufträge** kannst du sichtbare Tagebuchaufträge nach Bestätigung abschließen. Unter **Ausrüstung** lässt sich das ausgewählte Werkzeug in der Schnellleiste anpassen oder eine Minenebene als Ziel wählen. Für die Schädelhöhle brauchst du den Schädelschlüssel.

Unter **Welt → Schnellreise** bringt dich ein Klick auf **Teleportieren: …** direkt zum gewählten Ort. Über **Zum letzten Absprungpunkt zurück** kommst du zurück, solange die Stelle frei ist. Unter **Welt → Zeit & Wetter** kannst du die Uhr in Zehnerschritten vorlaufen lassen und die Vorhersage fürs Tal ändern. Das Vorspulen stoppt vor dem erzwungenen Tagesende.

Unter **Spieler** kannst du einen Lauftempo-Bonus von 1 bis 20 anwenden. Er wirkt nur auf deine Spielfigur und bleibt bis zum Zurücksetzen oder Verlassen des Spielstands aktiv. Unter **Spielereien** kannst du Stärke und Dauer für Glück und Magnetismus einstellen.

## Grundspiel und Mods

Der Trainer benötigt nur SMAPI. Er liest Items, Bewohner, Geschenkvorlieben und Rezepte aus den geladenen Spieldaten. Dadurch erscheinen auch Inhalte kompatibler Zusatzmods.

Unter **Geschenke** kannst du nach Anzeigename oder internem Namen suchen. Wenn [Stardew Valley Expanded](https://github.com/FlashShifter/StardewValleyExpanded) geladen ist, zeigt **Mods** die erkannte Version an und bietet Direktlinks zu Expanded-Items mit entsprechendem ID-Präfix und zu unterstützten Expanded-Bewohnern. Unter **Alle** findest du weiterhin sämtliche geladenen Items und Bewohner. Andere Zusatzmods stehen unter **Erkannte Mods**.

Du kannst bis zu drei bereits erreichte Stellen auf dauerhaft verfügbaren Grundspiel- oder Mod-Karten markieren. Die Punkte werden beim regulären Speichern pro Spielstand übernommen. Eine fehlende Karte oder eine blockierte Stelle kann nicht als Reiseziel verwendet werden.

Die Zeitsperre endet am nächsten Morgen. Besondere Spieltage können das eingestellte Wetter überschreiben. Dauerhafte Änderungen werden beim regulären Speichern übernommen.

## Bauen und installieren

1. Installiere [SMAPI](https://smapi.io/) für deine Stardew-Valley-Version. Der Trainer wurde gegen Stardew Valley 1.6.15 und SMAPI 4.1.10 gebaut.
2. Baue den Trainer mit einem installierten .NET SDK. Setze `-GamePath` auf deinen Spielordner.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1 -GamePath 'C:\Pfad\zu\Stardew Valley'
   ```

3. Kopiere den Ordner `dist\StardewValleyTrainer` in `Mods` im Spielverzeichnis.
4. Starte `StardewModdingAPI.exe` und lade einen Einzelspieler-Spielstand.
