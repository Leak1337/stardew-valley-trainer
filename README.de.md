# Stardew Valley Trainer

**[Direktdownload – StardewValleyTrainer.zip](https://github.com/Leak1337/stardew-valley-trainer/releases/latest/download/StardewValleyTrainer.zip)**

Das ZIP in `Mods` entpacken und Stardew Valley über SMAPI starten.

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

Unter **Geschenke** kannst du nach Anzeigename oder internem Namen suchen. Geladene Erweiterungen haben in **Items** und **Geschenke** einen eigenen Filter neben **Alle**. Unter **Rezepte** kannst du Rezepte der ausgewählten Erweiterung lernen. Andere Mods siehst du mit ihrer Version unter **Mods → Erkannte Mods**. Den Menü-Hotkey kannst du über `MenuKey` in der `config.json` des Trainers ändern. Standard bleibt **F5**.

### Optionale Mod-Kompatibilität

| Mod | Verhalten des Trainers |
| --- | --- |
| [Stardew Valley Expanded](https://github.com/FlashShifter/StardewValleyExpanded) | Item-, Bewohner- und Rezeptfilter. Schnellreise zum Blue Moon Vineyard, zur Shearwater Bridge und zum Adventurer Summit über die geladenen Karten. |
| [Ridgeside Village](https://github.com/Rafseazz/Ridgeside-Village-Mod) | Filter für Items und Rezepte mit Ridgeside-ID sowie Bewohner mit Wohnort in Ridgeside. Der Test mit der vollständigen Erweiterung steht noch aus. |
| [Automate](https://github.com/Pathoschild/StardewMods/tree/develop/Automate) | **Laufende Maschinen fertigstellen** überspringt Maschinen, die Automate als Teil einer Gruppe meldet. Ist die Schnittstelle nicht verfügbar, wird die Aktion ausgesetzt. |
| [UI Info Suite 2](https://github.com/Annosz/UIInfoSuite2), [NPC Map Locations](https://github.com/Bouhm/stardew-valley-mods/tree/main/NPCMapLocations), [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541), [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518) | Werden unter **Erkannte Mods** angezeigt. Der Trainer nutzt ein eigenes Menü und öffnet sich nur im freien Spiel. |
| [DaisyNiko's Earthy Recolour](https://www.nexusmods.com/stardewvalley/mods/5255), [Seasonal Outfits – Slightly Cuter Aesthetic](https://www.nexusmods.com/stardewvalley/mods/5450), [Visible Fish](https://www.nexusmods.com/stardewvalley/mods/8897) | Werden unter **Erkannte Mods** angezeigt. Der Trainer liest die geladenen Spieldaten und lässt die Darstellung bei den jeweiligen Mods. |

Die Erkennung nutzt das SMAPI-Manifest der geladenen Mod. Die Zusatzmods haben jeweils eigene SMAPI- und Installationsanforderungen.

Version 1.9.0 wurde im Grundspiel sowie in einem separaten Profil mit Expanded 1.15.11, Automate 2.4.5, UI Info Suite 2 2.3.7, Chests Anywhere 1.29.1 und Lookup Anything 1.50.0 gemeinsam getestet. Die Ridgeside-Filter wurden mit repräsentativen Content-Patcher-Daten geprüft. Das vollständige Ridgeside Village, NPC Map Locations und die drei Grafikmods waren nicht Teil dieses Spieltests.

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

## Lizenz und Namensnennung

Der in diesem Repository von Leak1337 geschriebene Code steht unter der [Apache License 2.0](LICENSE). Wenn du den Code oder eine bearbeitete Fassung weitergibst, lege den Lizenztext bei und übernimm den zutreffenden Herkunftshinweis aus [NOTICE](NOTICE) gemäß Abschnitt 4. Für separat lizenziertes Material gelten dessen eigene Bedingungen.
