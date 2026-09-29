Imports TXTextControl

''' <summary>
''' Sichert das Dokument selbsttätig in eine eigene Wiederherstellungsdatei,
''' damit ein Absturz oder Stromausfall nicht die Arbeit der letzten Stunde
''' kostet.
'''
''' Die Datei des Anwenders wird dabei nicht angefasst. Das ist der
''' Unterschied zu Speichern und der Grund, warum diese Klasse verlustfrei
''' arbeiten kann: Sie schreibt im internen Format von TX, das alles mitnimmt -
''' auch die Textmarken, an denen die benannten Formularfelder hängen. Ein
''' DOCX-Rundlauf kostet genau die.
'''
''' Gespeichert wird in zwei Schritten. Der Schnappschuss geht in den
''' Arbeitsspeicher und muss im UI-Thread laufen, weil das TextControl ihm
''' gehört; er ist der einzige Teil, den der Anwender überhaupt spüren könnte.
''' Das Schreiben auf die Platte - der langsame Teil - läuft im Hintergrund.
'''
''' Anders als früher braucht es keinen Zielpfad: Auch ein neues, nie
''' gespeichertes Dokument wird gesichert. Genau dort schmerzt ein Verlust am
''' meisten.
''' </summary>
Public Class AutoSpeichern
    Implements IDisposable

#Region "Zustand"

    Private ReadOnly tc As TXTextControl.TextControl

    ' Ausdrücklich der Timer der Oberfläche: Sein Tick läuft im UI-Thread, und
    ' nur deshalb darf dort ohne Weiteres auf das TextControl zugegriffen
    ' werden. System.Threading.Timer täte das nicht.
    Private WithEvents uhr As New System.Windows.Forms.Timer()

    ''' <summary>Kennung dieser Sitzung. Bestimmt die Dateinamen.</summary>
    Private ReadOnly kennung As String

    Private ReadOnly datendatei As String

    Private originalPfad As String = ""

    ' Die Begleitdatei wird nicht im Konstruktor geschrieben, sondern beim
    ' ersten Sichern - zusammen mit den Daten und im Hintergrund. So fällt im
    ' Startweg der Anwendung kein Datei-I/O an.
    Private begleitdateiSteht As Boolean = False

    ' Sämtliche Felder hier werden ausschliesslich vom UI-Thread berührt. Der
    ' Schnappschuss läuft dort, und die Fortsetzung nach dem Await kehrt
    ' dorthin zurück. Deshalb kein SyncLock und kein Interlocked: Die
    ' Absicherung liegt in der Reihenfolge der Zuweisungen, nicht in Sperren.
    Private etwasGeaendert As Boolean = False
    Private schreibtGerade As Boolean = False

    ' Beim Beenden kann noch ein Schreibvorgang unterwegs sein. Seine
    ' Fortsetzung läuft, nachdem das Fenster schon zu ist - sie darf dann keine
    ' Ereignisse mehr auslösen, die in abgeräumte Steuerelemente greifen.
    Private beendet As Boolean = False

    Private letzteAenderung As DateTime = DateTime.UtcNow
    Private letzteSicherung As DateTime = DateTime.UtcNow
    Private naechsterVersuch As DateTime = DateTime.MinValue

#End Region

#Region "Öffentliche API"

    ''' <summary>Es gibt einen frischen Schnappschuss auf der Platte.</summary>
    Public Event Gesichert As EventHandler(Of AutoSpeichernEventArgs)

    ''' <summary>
    ''' Das Sichern ist misslungen. Bewusst als Ereignis: Eine MessageBox
    ''' mitten im Tippen wäre das Letzte, was der Anwender gebrauchen kann.
    ''' </summary>
    Public Event Fehlgeschlagen As EventHandler(Of AutoSpeichernFehlerEventArgs)

    ''' <summary>Wartezeit ohne Änderung, bevor gesichert wird.</summary>
    Public Property Ruhephase As TimeSpan = TimeSpan.FromSeconds(3)

    ''' <summary>
    ''' Spätestens nach dieser Zeit wird gesichert, auch wenn der Anwender
    ''' ohne Pause weitertippt.
    ''' </summary>
    Public Property Hoechstabstand As TimeSpan = TimeSpan.FromSeconds(60)

    ''' <summary>
    ''' Pause nach einem Fehlschlag.
    ''' </summary>
    Public Property Fehlerpause As TimeSpan = TimeSpan.FromSeconds(10)

    ''' <summary>Die Datei, in die gesichert wird.</summary>
    Public ReadOnly Property Wiederherstellungsdatei As String
        Get
            Return datendatei
        End Get
    End Property

    Public Sub New(textControl As TXTextControl.TextControl)

        If textControl Is Nothing Then Throw New ArgumentNullException(NameOf(textControl))

        tc = textControl

        kennung = Wiederherstellung.NeueKennung()
        datendatei = Wiederherstellung.Datendatei(kennung)

        AddHandler tc.Changed, AddressOf TextControl_Changed

        ' Ein Takt von einer Sekunde. Er prüft nur und steigt im Regelfall nach
        ' zwei Abfragen wieder aus; die Entscheidung, ob gesichert wird, fällt
        ' über die Zeitmarken. Das ersetzt das frühere Anhalten und Neustarten
        ' der Uhr bei jedem einzelnen Tastendruck.
        uhr.Interval = 1000
        uhr.Start()

    End Sub

    ''' <summary>
    ''' Zu welcher Datei des Anwenders der gesicherte Stand gehört.
    '''
    ''' Der Pfad landet in der Begleitdatei und dient allein dazu, dem Anwender
    ''' bei der Wiederherstellung zu zeigen, woran er gearbeitet hat. Gesichert
    ''' wird unabhängig davon - ein leerer Pfad schaltet nichts ab.
    '''
    ''' Bewusst getrennt von <see cref="DokumentGewechselt"/>: Nach einer
    ''' Wiederherstellung gehört der Stand im Fenster zu diesem Pfad, steht
    ''' aber in keiner Datei. Er darf dann gerade nicht als gesichert gelten.
    ''' </summary>
    Public Property Originaldokument As String
        Get
            Return originalPfad
        End Get
        Set(wert As String)

            originalPfad = If(wert, "")

            ' Die Begleitdatei trägt den alten Pfad und muss neu geschrieben
            ' werden. Das erledigt die nächste Sicherung im Hintergrund mit.
            begleitdateiSteht = False

        End Set
    End Property

    ''' <summary>
    ''' Nach jedem Öffnen und nach jedem Speichern aufrufen: Der Stand im
    ''' Fenster entspricht jetzt der Datei des Anwenders.
    '''
    ''' Damit gibt es nichts mehr wiederherzustellen, und die bisherige
    ''' Wiederherstellungsdatei wird weggeräumt. Sie stehen zu lassen wäre
    ''' schädlich: Sie trüge einen älteren Stand als die gerade geschriebene
    ''' Datei, und nach einem Absturz bekäme der Anwender angeboten, hinter
    ''' seinen eigenen Speicherstand zurückzufallen.
    '''
    ''' Eine Schutzlücke entsteht dadurch nicht - es ist ja gerade nichts
    ''' ungesichert. Die erste Änderung danach startet den nächsten Takt.
    '''
    ''' Das Laden und Speichern löst selbst ein Changed aus; ohne das
    ''' Zurücksetzen liefe unmittelbar danach eine Sicherung an.
    ''' </summary>
    Public Sub DokumentGewechselt(dokumentPfad As String)

        Originaldokument = dokumentPfad

        etwasGeaendert = False
        letzteSicherung = DateTime.UtcNow

        Wiederherstellung.Verwerfen(kennung)

    End Sub

    ''' <summary>
    ''' Beim regulären Beenden aufrufen. Räumt die Wiederherstellungsdateien
    ''' weg - ihr Vorhandensein ist das Kennzeichen dafür, dass eine Sitzung
    ''' nicht sauber zu Ende ging.
    ''' </summary>
    Public Sub SauberBeenden()

        Anhalten()

        Wiederherstellung.Verwerfen(kennung)

        uhr.Dispose()

    End Sub

    ''' <summary>
    ''' Hält die Sicherung an, ohne aufzuräumen. Wer nur freigibt, ohne sauber
    ''' zu beenden, bekommt beim nächsten Start eine Wiederherstellung
    ''' angeboten - lieber eine überflüssige als eine fehlende.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose

        Anhalten()

        uhr.Dispose()

    End Sub

#End Region

#Region "Intern"

    Private Sub Anhalten()

        beendet = True

        uhr.Stop()
        RemoveHandler tc.Changed, AddressOf TextControl_Changed

    End Sub

    ''' <summary>
    ''' Bewusst ohne jede Bedingung und ohne Vergleich mit einem Text-Hash: Wer
    ''' nur Formate ändert, erzeugt denselben Text und würde nie gesichert. Ein
    ''' Modified-Kennzeichen bietet TX nicht an, nur dieses Ereignis - also ist
    ''' das Ereignis selbst die Wahrheit.
    '''
    ''' Auch wenn gerade geschrieben wird, wird hier nicht abgebrochen. Genau
    ''' daran krankte die frühere Fassung: Sie verwarf Änderungen, die während
    ''' einer laufenden Sicherung eintrafen.
    ''' </summary>
    Private Sub TextControl_Changed(sender As Object, e As EventArgs)

        etwasGeaendert = True
        letzteAenderung = DateTime.UtcNow

    End Sub

    Private Async Sub uhr_Tick(sender As Object, e As EventArgs) Handles uhr.Tick

        If Not SollJetztSichern() Then Exit Sub

        schreibtGerade = True

        Try
            ' Erster Schritt: der Schnappschuss. Muss im UI-Thread laufen, geht
            ' nur in den Arbeitsspeicher - kein Datei-I/O, kein ZIP-Aufbau wie
            ' bei DOCX.
            Dim daten As Byte() = Nothing
            tc.Save(daten, BinaryStreamType.InternalUnicodeFormat)

            ' Hierhin gehört das Zurücksetzen und nirgendwo sonst. Der
            ' Schnappschuss ist der neue Bezugspunkt; alles, was ab jetzt
            ' hereinkommt, steht nicht darin und muss die nächste Sicherung
            ' auslösen. Stünde die Zeile hinter dem Schreiben, ginge genau das
            ' verloren. Nebenbei räumt sie eine Änderungsmeldung weg, die das
            ' Speichern selbst ausgelöst haben könnte.
            etwasGeaendert = False

            ' Zweiter Schritt: auf die Platte, im Hintergrund. Der UI-Thread
            ' ist währenddessen frei, der Anwender tippt ungestört weiter.
            Dim begleitungNoetig = Not begleitdateiSteht
            Dim pfadFuerBegleitung = originalPfad

            Await Task.Run(
                Sub()
                    ' Die Begleitdatei steht vor den Daten: Sie ist der Marker
                    ' für die Absturzerkennung. Umgekehrt bliebe nach einem
                    ' Absturz eine Datendatei ohne Begleitung liegen, die der
                    ' Suchlauf als Waise wegräumt.
                    If begleitungNoetig Then Wiederherstellung.SchreibeBegleitdatei(kennung, pfadFuerBegleitung)

                    Wiederherstellung.Schreibe(datendatei, daten)
                End Sub)

            begleitdateiSteht = True
            letzteSicherung = DateTime.UtcNow

            ' Inzwischen wurde beendet. Die Datei ist zwar geschrieben, aber
            ' SauberBeenden hat sie schon weggeräumt - was hier entstanden ist,
            ' fängt der Suchlauf beim nächsten Start als Waise ab. Melden wäre
            ' jetzt nur noch schädlich: Das Fenster gibt es nicht mehr.
            If beendet Then Exit Try

            RaiseEvent Gesichert(Me, New AutoSpeichernEventArgs(datendatei, letzteSicherung))

        Catch ex As Exception
            ' Ein Async Sub hat keinen Aufrufer, der eine Ausnahme auffangen
            ' könnte - was hier durchrutscht, reisst die Anwendung mit.
            ' Deshalb fängt der Block alles ab.
            etwasGeaendert = True
            naechsterVersuch = DateTime.UtcNow + Fehlerpause

            If Not beendet Then RaiseEvent Fehlgeschlagen(Me, New AutoSpeichernFehlerEventArgs(ex))

        Finally
            schreibtGerade = False
        End Try

    End Sub

    ''' <summary>
    ''' Gesichert wird, sobald der Anwender eine Ruhephase lang nichts mehr
    ''' getan hat - oder wenn seit der letzten Sicherung zu viel Zeit vergangen
    ''' ist, damit durchgehendes Tippen nicht dazu führt, dass nie gesichert
    ''' wird.
    ''' </summary>
    Private Function SollJetztSichern() As Boolean

        If beendet Then Return False
        If Not etwasGeaendert Then Return False

        ' Es läuft noch ein Schreibvorgang. Nicht verwerfen, nicht warten - der
        ' nächste Takt kommt in einer Sekunde und holt es nach. Der Takt ist
        ' der Nachlauf, deshalb braucht es dafür keinen eigenen Code.
        If schreibtGerade Then Return False

        Dim jetzt = DateTime.UtcNow

        If jetzt < naechsterVersuch Then Return False

        Return jetzt - letzteAenderung >= Ruhephase OrElse
               jetzt - letzteSicherung >= Hoechstabstand

    End Function

#End Region

End Class

''' <summary>Meldet eine erfolgreiche selbsttätige Sicherung.</summary>
Public Class AutoSpeichernEventArgs
    Inherits EventArgs

    Public ReadOnly Property Pfad As String
    Public ReadOnly Property Zeitpunkt As DateTime

    Public Sub New(pfad As String, zeitpunkt As DateTime)
        Me.Pfad = pfad
        Me.Zeitpunkt = zeitpunkt
    End Sub

End Class

''' <summary>Meldet eine misslungene Sicherung.</summary>
Public Class AutoSpeichernFehlerEventArgs
    Inherits EventArgs

    Public ReadOnly Property Fehler As Exception

    Public Sub New(fehler As Exception)
        Me.Fehler = fehler
    End Sub

End Class
