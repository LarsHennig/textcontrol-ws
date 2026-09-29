Imports System.IO
Imports TXTextControl

''' <summary>
''' Fügt Textbausteine (.docx) an der aktuellen Cursorposition ein und führt
''' gleichartige Listen über Bausteingrenzen hinweg zusammen.
'''
''' Hintergrund: TX Text Control kennt keine Listen-Identität. ListFormat hat
''' kein Id-Feld. Eine Liste ist eine Folge von Absätzen mit identischen
''' Listenattributen. Word-numId geht beim Import verloren und wäre über
''' Dateigrenzen hinweg ohnehin nicht vergleichbar. Die Zugehörigkeit wird
''' deshalb hier zur Laufzeit hergestellt.
''' </summary>
Public Module Bausteinhelper

#Region "Öffentliche API"

    ''' <summary>
    ''' Fügt alle .docx-Dateien eines Verzeichnisses an der Cursorposition ein.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <param name="verzeichnis"></param>
    Public Sub AlleBausteineEinfuegen(tc As TextControl, verzeichnis As String)

        If Not Directory.Exists(verzeichnis) Then Exit Sub

        Dim dateien = Directory.GetFiles(verzeichnis, "*.docx").
                                OrderBy(Function(f) Path.GetFileName(f)).
                                ToArray()

        BausteineEinfuegen(tc, dateien)

    End Sub

    ''' <summary>
    ''' Fügt die übergebenen Dateien in genau dieser Reihenfolge ein.
    ''' Für das Auswahl-UI: Reihenfolge der Häkchen bzw. der Anzeige übergeben.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <param name="dateien">Einzufügende Dateien</param>
    Public Sub BausteineEinfuegen(tc As TextControl, dateien As IEnumerable(Of String))

        If dateien Is Nothing OrElse Not dateien.Any() Then Exit Sub

        ' Cursorposition merken, evtl. markierten Text nicht ersetzen
        Dim einfuegeStart As Integer = tc.Selection.Start
        tc.Selection.Length = 0

        For Each pfad In dateien

            If Not File.Exists(pfad) Then Continue For

            Try
                tc.Selection.Load(pfad, StreamType.WordprocessingML)
            Catch ex As Exception
                ' defekter Baustein darf den Rest nicht blockieren
                System.Diagnostics.Debug.WriteLine(
                    String.Format("Baustein übersprungen: {0} – {1}", pfad, ex.Message))
            End Try

        Next

        Dim einfuegeEnde As Integer = tc.Selection.Start + tc.Selection.Length

        ListenZusammenfuehren(tc, einfuegeStart, einfuegeEnde)

        ' Cursor hinter den eingefügten Block setzen
        tc.Selection.Start = einfuegeEnde
        tc.Selection.Length = 0

    End Sub

#End Region

#Region "Listenlogik"

    ''' <summary>
    ''' Kennung einer Liste: nur die Merkmale, die inhaltlich eine Listenart
    ''' ausmachen. Einzüge bewusst NICHT enthalten – die werden angeglichen,
    ''' nicht verglichen. Genau daran scheitern sonst Bausteine, die sich um
    ''' wenige Twips unterscheiden (714/357 gegen 720/360).
    ''' </summary>
    Private Function Listenkennung(lf As ListFormat) As String

        If lf Is Nothing Then Return Nothing

        Select Case lf.Type

            Case ListType.Numbered
                Return String.Format("num|{0}|{1}", lf.NumberFormat, lf.Level)

            Case ListType.Bulleted
                ' Font mit aufnehmen: gleicher Codepoint in Symbol und Wingdings
                ' ergibt optisch verschiedene Zeichen
                Return String.Format("bul|{0}|{1}|{2}",
                                     AscW(lf.BulletCharacter), lf.FontName, lf.Level)

            Case Else
                Return Nothing

        End Select

    End Function

    ''' <summary>
    ''' Gleicht alle Listenabsätze im Bereich [von, bis] an den jeweils ersten
    ''' Absatz gleicher Kennung an und setzt RestartNumbering entsprechend.
    ''' </summary>
    Private Sub ListenZusammenfuehren(tc As TextControl, von As Integer, bis As Integer)

        ' je Listenkennung die maßgebliche Ausprägung
        Dim anker As New Dictionary(Of String, ListFormat)

        ' Absätze vorab einsammeln – während der Bearbeitung nicht über die
        ' Live-Collection iterieren
        Dim absaetze As New List(Of Paragraph)
        For Each p As Paragraph In tc.Paragraphs
            absaetze.Add(p)
        Next

        For Each p As Paragraph In absaetze

            If p.Start > bis Then Exit For

            Dim lf As ListFormat = p.ListFormat
            Dim kennung As String = Listenkennung(lf)

            ' Fließtext: überspringen, aber die Kette NICHT unterbrechen.
            ' Wichtig für Bausteine mit Fortsetzungsabsätzen (z. B. ARCHAEOLOGIE
            ' mit numId=0 oder DICHT_JAHR mit zweitem Absatz).
            If kennung Is Nothing Then Continue For

            ' Absätze vor der Einfügestelle nur als Referenz aufnehmen,
            ' damit der erste Baustein an eine bestehende Liste andockt
            If p.Start + p.Length < von Then
                anker(kennung) = lf
                Continue For
            End If

            Dim referenz As ListFormat = Nothing

            If anker.TryGetValue(kennung, referenz) Then

                ' Geometrie der Ankerliste übernehmen, sonst sieht TX
                ' weiterhin zwei getrennte Listen
                lf.LeftIndent = referenz.LeftIndent
                lf.HangingIndent = referenz.HangingIndent
                lf.FormatCharacter = referenz.FormatCharacter
                lf.TextBeforeNumber = referenz.TextBeforeNumber
                lf.TextAfterNumber = referenz.TextAfterNumber
                lf.FontName = referenz.FontName
                lf.RestartNumbering = False

                p.ListFormat = lf

            Else

                ' erste Liste dieser Art im Dokument: neu beginnen
                lf.RestartNumbering = True
                p.ListFormat = lf

                ' zurücklesen statt lf wiederverwenden
                anker(kennung) = p.ListFormat

            End If

        Next

    End Sub

#End Region

End Module