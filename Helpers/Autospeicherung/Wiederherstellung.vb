Imports System.IO
Imports System.Text

''' <summary>
''' Die Dateiseite der selbsttätigen Sicherung: Ablageort, Namensschema,
''' Begleitdatei, Schreiben, Suchlauf beim Start und Aufräumen.
'''
''' Hier läuft nichts nebenläufig ausser <see cref="Schreibe"/>, das bewusst
''' vom Hintergrund-Thread aufgerufen wird und dafür nur seine Parameter
''' anfasst. Die Trennung von AutoSpeichern ist Absicht: Dort steckt die
''' gesamte Nebenläufigkeit und keine Dateilogik, hier umgekehrt. Vermischt
''' müsste man beim Lesen ständig mitdenken, auf welchem Thread man gerade ist.
'''
''' Eine Sitzung besitzt ihren Dateinamen, nicht das Dokument. Ein vom
''' Originalpfad abgeleiteter Name hätte beim noch namenlosen Dokument keine
''' Eingabe, müsste bei "Speichern unter" mitten in der Sitzung umbenannt
''' werden, und zwei gleichzeitig laufende Instanzen am selben Dokument würden
''' sich gegenseitig überschreiben. Eine Sitzungskennung gibt es dagegen immer.
''' </summary>
Public Module Wiederherstellung

#Region "Ablageort und Namensschema"

    ''' <summary>
    ''' Ordner der Wiederherstellungsdateien.
    '''
    ''' Nicht neben der Anwendung: Unter "Programme" hat ein Standardbenutzer
    ''' kein Schreibrecht, und auf einem Terminalserver würden sich mehrere
    ''' Anwender gegenseitig ihre Dokumente anbieten. Die Dateien enthalten
    ''' Vertragsinhalte, sie gehören in das Profil ihres Anwenders.
    '''
    ''' LocalApplicationData und nicht ApplicationData: Die Dateien werden
    ''' mehrere Megabyte gross und sind auf einem anderen Rechner wertlos - im
    ''' servergespeicherten Profil würden sie nur die Anmeldung bremsen.
    ''' </summary>
    Public Function Ordner() As String

        Dim basis = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

        ' Nur der Produktname. AssemblyCompany ist in diesem Projekt leer, und
        ' Path.Combine würde ein leeres Segment stillschweigend verschlucken.
        Return Path.Combine(basis, System.Windows.Forms.Application.ProductName, "AutoWiederherstellung")

    End Function

    ''' <summary>
    ''' Kennung für eine neue Sitzung. Der Zeitanteil macht den Ordner für
    ''' einen Menschen lesbar, der Zufallsanteil hält zwei im selben Moment
    ''' gestartete Instanzen auseinander.
    ''' </summary>
    Public Function NeueKennung() As String

        Return DateTime.Now.ToString("yyyy-MM-dd_HHmmss") & "_" &
               Guid.NewGuid().ToString("N").Substring(0, 8)

    End Function

    ''' <summary>Schnappschuss im InternalUnicodeFormat.</summary>
    Public Function Datendatei(kennung As String) As String
        Return Path.Combine(Ordner(), kennung & ".tx")
    End Function

    ''' <summary>Begleitdatei mit Originalpfad und Sitzungsangaben.</summary>
    Public Function Begleitdatei(kennung As String) As String
        Return Path.Combine(Ordner(), kennung & ".info")
    End Function

#End Region

#Region "Schreiben"

    ''' <summary>
    ''' Legt die Begleitdatei an oder schreibt sie neu. Beim Sitzungsstart
    ''' aufrufen und nach jedem Wechsel des Originalpfades.
    '''
    ''' Sie ist zugleich der Marker für die Absturzerkennung: Ist sie beim
    ''' nächsten Start noch da, wurde die Sitzung nicht sauber beendet. Deshalb
    ''' entsteht sie vor dem ersten Schnappschuss - ein Absturz dazwischen
    ''' hinterlässt eine Begleitdatei ohne Daten, die der Suchlauf wegräumt,
    ''' statt eine leere Wiederherstellung anzubieten.
    ''' </summary>
    ''' <param name="kennung">Sitzungskennung aus <see cref="NeueKennung"/>.</param>
    ''' <param name="originalPfad">Datei des Anwenders. Leer beim noch namenlosen Dokument.</param>
    Public Sub SchreibeBegleitdatei(kennung As String, originalPfad As String)

        Directory.CreateDirectory(Ordner())

        Dim eigener = Process.GetCurrentProcess()

        Dim zeilen As New List(Of String) From {
            SchluesselZeile(SchluesselOriginalpfad, If(originalPfad, "")),
            SchluesselZeile(SchluesselProzessId, eigener.Id.ToString()),
            SchluesselZeile(SchluesselProzessStart, eigener.StartTime.ToUniversalTime().ToString("o"))
        }

        File.WriteAllLines(Begleitdatei(kennung), zeilen, New UTF8Encoding(False))

    End Sub

    ''' <summary>
    ''' Schreibt den Schnappschuss. Wird vom Hintergrund-Thread aufgerufen und
    ''' fasst deshalb nur seine Parameter an.
    '''
    ''' Erst daneben schreiben, dann umbenennen: Ein Absturz mitten im Schreiben
    ''' hinterlässt sonst eine halbe Datei, die bei der Wiederherstellung nichts
    ''' mehr hergibt. Anders als beim früheren Speichern in das Originaldokument
    ''' steht dabei nichts Wertvolles auf dem Spiel - scheitert das Umbenennen,
    ''' ist nur der jüngste Schnappschuss verloren, und den erzeugt der nächste
    ''' Takt ohnehin neu.
    ''' </summary>
    Public Sub Schreibe(datendatei As String, daten As Byte())

        Directory.CreateDirectory(Path.GetDirectoryName(datendatei))

        Dim zwischenPfad = datendatei & EndungZwischendatei

        File.WriteAllBytes(zwischenPfad, daten)

        If File.Exists(datendatei) Then File.Delete(datendatei)
        File.Move(zwischenPfad, datendatei)

    End Sub

    ''' <summary>
    ''' Entfernt beide Dateien einer Sitzung.
    '''
    ''' Die Reihenfolge ist wichtig: erst die Begleitdatei, dann die Daten.
    ''' Beim Beenden kann noch ein Hintergrundschreibvorgang unterwegs sein und
    ''' die Datendatei danach wieder anlegen. Sie bleibt dann ohne Begleitdatei
    ''' zurück - und die räumt der Suchlauf beim nächsten Start als Waise weg.
    ''' Auf den Hintergrund-Thread zu warten wäre die schlechtere Lösung: Seine
    ''' Fortsetzung braucht den UI-Thread, ein Warten verklemmte die Anwendung.
    ''' </summary>
    Public Sub Verwerfen(kennung As String)

        LoescheStill(Begleitdatei(kennung))
        LoescheStill(Datendatei(kennung))
        LoescheStill(Datendatei(kennung) & EndungZwischendatei)

    End Sub

#End Region

#Region "Suchlauf beim Start"

    ''' <summary>
    ''' Alle Sitzungen, die nicht sauber beendet wurden und noch Daten haben -
    ''' jüngste zuerst. Räumt dabei auf, was nichts mehr hergibt.
    '''
    ''' Wirft nicht: Der Aufruf steht im Startweg der Anwendung, ein Fehler im
    ''' Wiederherstellungsordner darf sie nicht am Hochkommen hindern.
    ''' </summary>
    Public Function OffeneWiederherstellungen() As List(Of Wiederherstellungspunkt)

        Dim punkte As New List(Of Wiederherstellungspunkt)

        Try
            If Not Directory.Exists(Ordner()) Then Return punkte

            Dim dateien = Directory.GetFiles(Ordner())

            ' Abgebrochene Schreibvorgänge.
            For Each pfad In MitEndung(dateien, EndungZwischendatei)
                LoescheStill(pfad)
            Next

            ' Datendateien ohne Begleitdatei. Entweder Waisen aus dem Wettlauf
            ' beim Beenden oder Reste einer sauber beendeten Sitzung.
            For Each pfad In MitEndung(dateien, EndungDaten)
                If Not File.Exists(Path.ChangeExtension(pfad, EndungBegleit)) Then LoescheStill(pfad)
            Next

            For Each begleitPfad In MitEndung(dateien, EndungBegleit)

                Dim punkt = LiesPunkt(begleitPfad)
                If punkt IsNot Nothing Then punkte.Add(punkt)

            Next

        Catch ex As Exception
            ' Bewusst still. Ohne Wiederherstellung zu starten ist besser als
            ' gar nicht zu starten.
        End Try

        Return punkte.OrderByDescending(Function(punkt) punkt.Gesichert).ToList()

    End Function

    ''' <summary>
    ''' Wertet eine Begleitdatei aus. Nothing bedeutet: nicht anzubieten -
    ''' entweder weil die Sitzung noch läuft oder weil aufgeräumt wurde.
    ''' </summary>
    Private Function LiesPunkt(begleitPfad As String) As Wiederherstellungspunkt

        Try
            Dim werte = LiesBegleitdatei(begleitPfad)

            ' Eine zweite, gerade laufende Instanz. Ihre Dateien gehören ihr.
            If SitzungLaeuftNoch(werte) Then Return Nothing

            Dim datenPfad = Path.ChangeExtension(begleitPfad, EndungDaten)

            ' Absturz vor dem ersten Schnappschuss: Es gibt nichts anzubieten.
            If Not File.Exists(datenPfad) Then
                LoescheStill(begleitPfad)
                Return Nothing
            End If

            Dim gesichert = File.GetLastWriteTimeUtc(datenPfad)

            If DateTime.UtcNow - gesichert > Vorhaltezeit Then
                LoescheStill(begleitPfad)
                LoescheStill(datenPfad)
                Return Nothing
            End If

            Dim originalpfad As String = Nothing
            werte.TryGetValue(SchluesselOriginalpfad, originalpfad)

            Return New Wiederherstellungspunkt(
                Path.GetFileNameWithoutExtension(begleitPfad),
                datenPfad,
                If(originalpfad, ""),
                gesichert)

        Catch ex As Exception
            ' Unlesbare Begleitdatei. Sie bringt niemandem etwas.
            LoescheStill(begleitPfad)
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Läuft der Prozess, der diese Begleitdatei angelegt hat, noch?
    '''
    ''' Die Prozessnummer allein genügt nicht: Windows vergibt sie wieder. Erst
    ''' zusammen mit der Startzeit ist sie eindeutig - sonst täuscht ein
    ''' beliebiger fremder Prozess mit derselben Nummer eine laufende Sitzung
    ''' vor und unterdrückt die Wiederherstellung dauerhaft.
    ''' </summary>
    Private Function SitzungLaeuftNoch(werte As Dictionary(Of String, String)) As Boolean

        Dim idText As String = Nothing
        Dim startText As String = Nothing

        If Not werte.TryGetValue(SchluesselProzessId, idText) Then Return False
        If Not werte.TryGetValue(SchluesselProzessStart, startText) Then Return False

        Dim prozessId As Integer
        Dim prozessStart As DateTime

        If Not Integer.TryParse(idText, prozessId) Then Return False
        If Not DateTime.TryParse(startText, Nothing, Globalization.DateTimeStyles.RoundtripKind, prozessStart) Then Return False

        Try
            Return Process.GetProcessById(prozessId).StartTime.ToUniversalTime() = prozessStart.ToUniversalTime()

        Catch ex As Exception
            ' Prozess weg oder nicht abfragbar. Als beendet werten - der Ordner
            ' liegt im Profil des Anwenders, fremde Prozesse gehören hier nicht hin.
            Return False
        End Try

    End Function

#End Region

#Region "Begleitdatei lesen und schreiben"

    Private Const SchluesselOriginalpfad As String = "Originalpfad"
    Private Const SchluesselProzessId As String = "ProzessId"
    Private Const SchluesselProzessStart As String = "ProzessStart"

    Private Const EndungDaten As String = ".tx"
    Private Const EndungBegleit As String = ".info"
    Private Const EndungZwischendatei As String = ".neu"

    ''' <summary>
    ''' Nach dieser Zeit wird eine liegengebliebene Sicherung nicht mehr
    ''' angeboten, sondern weggeräumt. Wer einen Monat nicht danach gefragt hat,
    ''' braucht sie nicht mehr.
    ''' </summary>
    Private ReadOnly Vorhaltezeit As TimeSpan = TimeSpan.FromDays(30)

    ''' <summary>
    ''' Schlicht Schluessel=Wert je Zeile. Kein JSON: Das zöge einen
    ''' Serialisierer und eine Vertragsklasse nach sich, für drei Angaben.
    ''' </summary>
    Private Function SchluesselZeile(schluessel As String, wert As String) As String
        Return schluessel & "=" & wert
    End Function

    Private Function LiesBegleitdatei(pfad As String) As Dictionary(Of String, String)

        Dim werte As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        For Each zeile In File.ReadAllLines(pfad, Encoding.UTF8)

            ' Nur am ersten Gleichheitszeichen trennen - ein Dateipfad darf
            ' selbstverständlich weitere enthalten.
            Dim trenner = zeile.IndexOf("="c)
            If trenner <= 0 Then Continue For

            werte(zeile.Substring(0, trenner)) = zeile.Substring(trenner + 1)

        Next

        Return werte

    End Function

#End Region

#Region "Kleinkram"

    ''' <summary>
    ''' Dateien mit genau dieser Endung. Bewusst nicht über ein Suchmuster:
    ''' Die Windows-Dateisuche bezieht kurze 8.3-Namen mit ein und liefert bei
    ''' "*.tx" auch Treffer, die gar nicht so heissen.
    ''' </summary>
    Private Function MitEndung(dateien As String(), endung As String) As String()

        Return dateien.
               Where(Function(pfad) String.Equals(Path.GetExtension(pfad), endung, StringComparison.OrdinalIgnoreCase)).
               ToArray()

    End Function

    ''' <summary>
    ''' Löschen, das nicht wirft. Beim Aufräumen ist jeder Fehlschlag
    ''' hinnehmbar: Die Datei bleibt liegen und wird beim nächsten Start
    ''' erneut angefasst.
    ''' </summary>
    Private Sub LoescheStill(pfad As String)

        Try
            If File.Exists(pfad) Then File.Delete(pfad)
        Catch ex As Exception
        End Try

    End Sub

#End Region

End Module

''' <summary>
''' Eine Sitzung, die nicht sauber beendet wurde und noch Daten hat.
''' </summary>
Public Class Wiederherstellungspunkt

    ''' <summary>Sitzungskennung - für <see cref="Wiederherstellung.Verwerfen"/>.</summary>
    Public ReadOnly Property Kennung As String

    Public ReadOnly Property Datendatei As String

    ''' <summary>Datei des Anwenders. Leer, wenn das Dokument nie einen Namen hatte.</summary>
    Public ReadOnly Property Originalpfad As String

    ''' <summary>Zeitpunkt der letzten Sicherung, aus dem Dateidatum.</summary>
    Public ReadOnly Property Gesichert As DateTime

    ''' <summary>Was dem Anwender im Wiederherstellungsdialog angezeigt wird.</summary>
    Public ReadOnly Property Anzeigename As String
        Get
            If String.IsNullOrWhiteSpace(Originalpfad) Then Return "Unbenanntes Dokument"
            Return IO.Path.GetFileName(Originalpfad)
        End Get
    End Property

    Public Sub New(kennung As String, datendatei As String, originalpfad As String, gesichert As DateTime)

        Me.Kennung = kennung
        Me.Datendatei = datendatei
        Me.Originalpfad = originalpfad
        Me.Gesichert = gesichert

    End Sub

End Class
