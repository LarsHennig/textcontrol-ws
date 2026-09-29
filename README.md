# TxWinformsVbDemo1

Demoprojekt für die Entsorgungsbetriebe Lübeck (EBL): ein Windows-Forms-Editor auf Basis von **TX Text Control**, der als Ersatz für Word beim Erstellen von Dokumenten aus Textbausteinen dient.

## Funktionen

- **Dokumente öffnen und speichern** (u. a. DOCX) mit TX Text Control und Ribbon-Oberfläche
- **Textbausteine** (`app_data/Bausteine`, `app_data/Hinweis`) per Klick einfügen; die Nummerierung (a./b./c.) läuft über mehrere Bausteine hinweg fortlaufend (`Helpers/Bausteinhelper.vb`)
- **Nummerische Varianten** der Hinweis-Bausteine (`app_data/HinweisNummerisch`)
- **Formularfelder**: Checkboxen im Dokument werden in einer Sidebar (`Controls/UcFormularfelder`) aufgelistet und lassen sich dort setzen (`Helpers/FormularHelper.vb`)
- **Serienbrief** (Mail Merge) mit Personendaten (`Helpers/MailMergeHelper.vb`, `Person.vb`)
- **Bilder einfügen**, auch an bestimmten Positionen (`Helpers/ImageHelper.vb`)
- **Platzhalter** ersetzen und importieren
- **AutoSave** in Wiederherstellungsdateien statt ins Original, mit Wiederherstellung nach Absturz (`Helpers/Autospeicherung`)
- Rechtschreibprüfung (TXSpell), Undo/Redo, Drucken

## Voraussetzungen

- Windows
- Visual Studio 2022 (oder neuer) mit .NET-Framework-4.8-Unterstützung
- **TX Text Control .NET for Windows Forms 34.0** (inkl. Server, Barcode, Spell, DocumentServer) und eine gültige Lizenz (`My Project/licenses.licx`)

## Bauen und Starten

1. `TxWinformsVbDemo1.sln` in Visual Studio öffnen
2. Die TX-Text-Control-Referenzen bei Bedarf auf die lokal installierte Version zeigen lassen
3. Mit F5 starten (Ziel: `bin/Debug` bzw. `bin/Release`)

## Projektstruktur

| Pfad | Inhalt |
| --- | --- |
| `Form1.vb` | Hauptfenster: Ribbon, Datei-Menü, Bausteine, Sidebar |
| `Form2.vb` | Zusatzdialog |
| `Controls/` | Sidebar-Control für Formularfelder |
| `Helpers/` | Bausteine, Formularfelder, Bilder, Serienbrief, Ribbon-Buttons, AutoSave/Wiederherstellung |
| `app_data/` | Vorlagen und Textbausteine als DOCX |
| `Images/` | Icons und Beispielbilder |

## Technik

- VB.NET, Windows Forms, .NET Framework 4.8
- TX Text Control 34.0 (`TXTextControl`, `TXTextControl.Server`, `TXSpell`, `TXBarcode`, `TXDocumentServer`)
